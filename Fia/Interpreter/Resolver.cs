using System;
using FiaLang.Other;

namespace FiaLang
{
    internal class Resolver : Expr.IVisitor<None>, Stmt.IVisitor<None>
    {
        private readonly Interpreter interpreter;
        private readonly List<Dictionary<string, bool>> scopes;
        private enum ClassType
        {
            NONE,
            CLASS
        }
        private enum FunctionType
        {
            NONE,
            FUNCTION,
            INIT,
            METHOD
        }
        private FunctionType currentFunction = FunctionType.NONE;
        private ClassType currentClass = ClassType.NONE;

        public Resolver(Interpreter interpreter)
        {
            this.interpreter = interpreter;
            this.scopes = new List<Dictionary<string, bool>>();
        }

        public void Run(List<Stmt> stmts)
        {
            Resolve(stmts);
        }

        //Statements.

        public None VisitExpression(Stmt.Expression stmt)
        {
            Resolve(stmt.expr);
            return new None();
        }

        public None VisitConditional(Stmt.Conditional stmt)
        {
            Resolve(stmt.condition);
            Resolve(stmt.thenBranch);
            if (stmt.elseBranch != null ) Resolve(stmt.elseBranch);
            return new None();
        }

        public None VisitLoop(Stmt.Loop stmt)
        {
            Resolve(stmt.condition);
            Resolve(stmt.stmt);
            return new None();
        }

        public None VisitReturning(Stmt.Returning stmt)
        {
            if (currentFunction != FunctionType.FUNCTION)
            {
                Fia.Error("Can't return outside of a function.", stmt.keyword.line);
            }

            if (stmt.value != null)
            {
                if (currentFunction == FunctionType.INIT)
                {
                    Fia.Error("Can't return from an initializer.", stmt.keyword.line);
                }

                Resolve(stmt.value);
            }
            return new None();
        }

        public None VisitPrint(Stmt.Print stmt)
        {
            Resolve(stmt.val);
            return new None();
        }
        public None VisitBlock(Stmt.Block stmt)
        {
            BeginScope();
            Resolve(stmt.statements);
            EndScope();
            return new None();
        }

        public None VisitClassObj(Stmt.ClassObj stmt) {
            var enclosingClass = currentClass;
            currentClass = ClassType.CLASS;

            Declare(stmt.name);
            Define(stmt.name);

            BeginScope();
            scopes.Last()["this"] = true;

            foreach (var method in stmt.methods)
            {
                var declaration = FunctionType.METHOD;

                if (method.name.lexeme == "init")
                {
                    declaration = FunctionType.INIT;
                }

                ResolveFunction(method, declaration);
            }

            EndScope();

            currentClass = enclosingClass;
            return new None();
        }

        public None VisitFunction(Stmt.Function stmt)
        {
            Declare(stmt.name);
            Define(stmt.name);

            ResolveFunction(stmt, FunctionType.FUNCTION);

            return new None();
        }

        public None VisitVar(Stmt.Var stmt)
        {
            Declare(stmt.name);
            if (stmt.init != null)
            {
                Resolve(stmt.init);
            }
            Define(stmt.name);

            return new None();
        }
        
        //Expressions.

        public None VisitBinary(Expr.Binary expr)
        {
            Resolve(expr.left);
            Resolve(expr.right);

            return new None();
        }

        public None VisitLogical(Expr.Logical expr)
        {
            Resolve(expr.left);
            Resolve(expr.right);

            return new None();
        }

        public None VisitSet(Expr.Set expr)
        {
            Resolve(expr.value);
            Resolve(expr.obj);

            return new None();
        }

        public None VisitThisRef(Expr.ThisRef expr)
        {
            if (currentClass == ClassType.NONE)
            {
                Fia.Error("Can't use 'this' outside of a class.", expr.keyword.line);
                return new None();
            }
            ResolveLocal(expr, expr.keyword);
            return new None();
        }

        public None VisitUnary(Expr.Unary expr)
        {
            Resolve(expr.right);

            return new None();
        }

        public None VisitGrouping(Expr.Grouping expr)
        {
            Resolve(expr.expr);

            return new None();
        }

        public None VisitLiteral(Expr.Literal expr)
        {
            return new None();
        }

        public None VisitCall(Expr.Call expr)
        {
            Resolve(expr.callee);
            foreach(var argument in expr.arguments)
            {
                Resolve(argument);
            }

            return new None();
        }

        public None VisitGet(Expr.Get expr)
        {
            Resolve(expr.obj);
            return new None();
        }

        public None VisitAssigment(Expr.Assigment expr)
        {
            Resolve(expr.value);
            ResolveLocal(expr, expr.name);

            return new None();
        }

        public None VisitVariable(Expr.Variable expr)
        {
            if (scopes.Count > 0 &&
                scopes.Last().ContainsKey(expr.name.lexeme) && 
                scopes.Last()[expr.name.lexeme] == false)
            {
                Fia.Error("Can't read local variable in it's own initializer.", expr.name.line);
            }

            ResolveLocal(expr, expr.name);

            return new None();
        }


        //Private methods.
        private void Declare(Token name)
        {
            if (scopes.Count == 0) return;

            if (scopes.Last().ContainsKey(name.lexeme))
            {
                Fia.Error($"Aready a variable with the name '{name.lexeme}' in this scope.", name.line);
            }

            scopes.Last()[name.lexeme] = false;
        }

        private void Define(Token name)
        {
            if (scopes.Count == 0) return;
            scopes.Last()[name.lexeme] = true;
        }

        private void BeginScope()
        {
            scopes.Add(new Dictionary<string, bool>());
        }

        private void EndScope()
        {
            scopes.Remove(scopes.Last());
        }

        private void Resolve(List<Stmt> stmts)
        {
            foreach (Stmt stmt in stmts)
            {
                Resolve(stmt);
            }
        }

        public void Resolve(Stmt stmt)
        {
            stmt.Accept(this);
        }

        private void Resolve(Expr expr)
        {
            expr.Accept(this);
        }

        private void ResolveLocal(Expr expr, Token name)
        {
            for (int i = scopes.Count - 1; i >= 0; i--)
            {
                if (scopes[i].ContainsKey(name.lexeme))
                {
                    interpreter.Resolve(expr, scopes.Count - i - 1);
                    return;
                }
            }
        }

        private void ResolveFunction(Stmt.Function function, FunctionType type)
        {
            BeginScope();
            FunctionType enclosingFunction = currentFunction;
            currentFunction = type;

            foreach(var parameter in function.parameters)
            {
                Declare(parameter);
                Define(parameter);
            }

            Resolve(function.body);

            EndScope();
            currentFunction = enclosingFunction;
        }

    }
}
