using static FiaLang.TokenType;
using FiaLang.Error;
using FiaLang.Other;

namespace FiaLang
{
    internal class Interpreter : Expr.IVisitor<Object?>, Stmt.IVisitor<None>
    {
        public readonly Environment globalEnv;

        private Environment env;
        private readonly Dictionary<Expr, int> locals;

        public Interpreter()
        {
            globalEnv = new Environment();
            env = globalEnv;
            locals = new Dictionary<Expr, int>();
        }
        public void Interpret(List<Stmt?> statements)
        {
            try
            {
                foreach (var stmt in statements)
                {
                    Execute(stmt);
                }
            }
            catch(RuntimeError error)
            {
                Fia.Error(error.message, error.token.line);
            }
        }

        //Statement visitor methods.
        //They all have None as a return type, which is an empty class.
        //None is used because C# doesn't allow void type in generics.

        public None VisitClassObj(Stmt.ClassObj stmt)
        {
            env.Define(stmt.name, null);
            
            var methods = new Dictionary<String, FiaFunction>();
            foreach (var method in stmt.methods)
            {
                var function = new FiaFunction(method, env, 
                    method.name.lexeme == "init");
                methods[method.name.lexeme] = function;
            }

            FiaClass classObj = new FiaClass(stmt.name.lexeme, methods);
            env.Assign(stmt.name, classObj);

            return new None();
        }

        public None VisitFunction(Stmt.Function declaration)
        {
            var function = new FiaFunction(declaration, env);

            globalEnv.Define(declaration.name, function);

            return new None();
        }

        public None VisitReturning(Stmt.Returning returning)
        {
            object? val = null;
            if (returning.value != null) val = Evaluate(returning.value);

            throw new Return(val);
        }

        public None VisitBlock(Stmt.Block block)
        {
            ExecuteBlock(block.statements, new Environment(env));
            return new None();
        }

        public void ExecuteBlock(List<Stmt> statements, Environment env)
        {
            Environment prev = this.env;
            try
            {
                this.env = env;

                foreach (var stmt in statements)
                {
                    Execute(stmt);
                }
            }
            finally
            {
                this.env = prev;
            }
        }

        public None VisitConditional(Stmt.Conditional conditional)
        {
            bool trueness = IsTrue(Evaluate(conditional.condition));
            
            if (trueness)
            {
                Execute(conditional.thenBranch);
            }
            else if (conditional.elseBranch is not null)
            {
                Execute(conditional.elseBranch);
            }

            return new None();
        }

        public None VisitLoop(Stmt.Loop loop) {

            while(IsTrue(Evaluate(loop.condition)))
            {
                Execute(loop.stmt);
            }

            return new None();
        }

        public None VisitExpression(Stmt.Expression expression)
        {
            Evaluate(expression.expr);
            return new None();
        }

        public None VisitPrint(Stmt.Print print)
        {
            var val = Evaluate(print.val);
            Fia.writer.WriteLine(Stringify(val));

            return new None();
        }

        public None VisitVar(Stmt.Var var)
        {
            
            env.Define(var.name, Evaluate(var.init));

            return new None();
        }


        //Expression visitor methods.
        
        public object? VisitCall(Expr.Call call)
        {
            
            var callee = Evaluate(call.callee);
            
            if (callee is not IFiaCallable)
            {
                throw new RuntimeError(call.paren, $"Can only call functions and classes.");
            }

            var args = new List<Object?>();
            foreach(var expr in call.arguments)
            {
                args.Add(Evaluate(expr));
            }

            var function = (IFiaCallable)callee;

            if (args.Count() != function.Arity())
            {
                throw new RuntimeError(call.paren, 
                    $"Expected {function.Arity()} arguments, but got {args.Count()}.");
            }

            try
            {
                return function.Call(this, args);
            }
            catch (Return r) {
                return r.val;
            }

            return null;
        }

        public object? VisitGet(Expr.Get get)
        {
            var obj = Evaluate(get.obj);

            if (obj is FiaInstance)
            {
                return ((FiaInstance)obj).Get(get.name);
            }

            throw new RuntimeError(get.name, "Only class instances have properties.");
        }

        public object? VisitSet(Expr.Set set)
        {
            var obj = Evaluate(set.obj);

            if(obj is not FiaInstance) {
                throw new RuntimeError(set.name, "Only instances have fields.");
            }

            object? value = Evaluate(set.value);

            ((FiaInstance)obj).Set(set.name, value);

            return value;

        }

        public object? VisitThisRef(Expr.ThisRef expr)
        {
            return LookUpVariable(expr.keyword, expr);
        }

        public object? VisitAssigment(Expr.Assigment assigment)
        {
            var value = Evaluate(assigment.value);

            if (locals.TryGetValue(assigment, out int distance))
            {
                env.AssignAt(distance, assigment.name, value);
            }
            else
            {
                globalEnv.Assign(assigment.name, value);
            }

            return value;
        }
        public object? VisitBinary(Expr.Binary binary)
        {
            var left = Evaluate(binary.left);
            var right = Evaluate(binary.right);
            var oper = binary.oper;

            switch(oper.type)
            {
                case STAR:
                    checkNumberOperands(left, oper, right);
                    return (double)left * (double)right;
                case MINUS:
                    checkNumberOperands(left, oper, right);
                    return (double)left - (double)right;
                case SLASH:
                    checkNumberOperands(left, oper, right);
                    return (double)left / (double)right;
                case PLUS:
                    if (left is string && right is string)
                    {
                        return (string)left + (string)right;
                    }
                    if (left is double && right is double)
                    {
                        return (double)left + (double)right;
                    }
                    throw new RuntimeError(oper, "Operands must be all numbers or all strings");
                case LESSER:
                    checkNumberOperands(left, oper, right);
                    return (double)left < (double)right;
                case LESSER_EQUAL:
                    checkNumberOperands(left, oper, right);
                    return (double)left <= (double)right;
                case GREATER:
                    checkNumberOperands(left, oper, right);
                    return (double)left > (double)right;
                case GREATER_EQUAL:
                    checkNumberOperands(left, oper, right);
                    return (double)left >= (double)right;
                case EQUAL_EQUAL:
                    return IsEqual(left, right);
                case BANG_EQUAL:
                    return !IsEqual(left, right);
                default:
                    return null;
            }
        }

        public object? VisitLogical(Expr.Logical? logical)
        {
            object? leftVal = Evaluate(logical?.left);
            TokenType oper = logical.oper.type;

            if (oper == OR)
            {
                if (IsTrue(leftVal)) return leftVal;
            }
            else
            {
                if (!IsTrue(leftVal)) return leftVal;
            }

            return Evaluate(logical?.right);
        }

        public object? VisitGrouping(Expr.Grouping? grouping)
        {
            return grouping?.expr.Accept(this);
        }

        public object? VisitLiteral(Expr.Literal? literal)
        {
            return literal?.value;
        }

        public object? VisitUnary(Expr.Unary? unary)
        {
            var right = Evaluate(unary?.right);
            var oper = unary?.oper;

            switch(oper?.type)
            {
                case MINUS:
                    checkNumberOperand(oper, right);
                    return -(double)right;
                case BANG:
                    return !IsTrue(right);
            }

            return null;
        }

        public object? VisitVariable(Expr.Variable variable)
        {
            return LookUpVariable(variable.name, variable);
        }

        //Methods for binding and resolving.
        public void Resolve(Expr expr, int depth)
        {
            locals[expr] = depth;
        }

        private Object? LookUpVariable(Token name, Expr expr)
        {
            if (locals.TryGetValue(expr, out var distance))
            {
                return env.GetAt(distance, name);
            }
            return globalEnv.Get(name);
        }

        //Helper methods.
        private void Execute(Stmt stmt)
        {
            stmt.Accept(this);
        }

        private object? Evaluate(Expr? expr)
        {
            return expr?.Accept(this);
        }

        private void checkNumberOperand(Token oper, object? a)
        {
            if (a is double) return;

            throw new RuntimeError(oper, "Operands must be numbers");
        }

        private void checkNumberOperands(object? a, Token oper, object? b)
        {
            if (a is double && b is double) return;

            throw new RuntimeError(oper, "Operands must be numbers");
        }

        private bool IsTrue(object? val)
        {
            if (val == null) return false;
            if (val is bool) return (bool)val;
            return true;
        }
        private bool IsEqual(object? a, object? b)
        {
            if (a is null && b is null) return true;
            if (a is null) return false;

            return a.Equals(b);
        }

        private string Stringify(Object? val)
        {
            if (val == null) return "nolla";
            return val.ToString();
        }
        
    }
}
