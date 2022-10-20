using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FiaLang
{
    internal class FiaFunction : IFiaCallable
    {
        private readonly Environment closure;
        private readonly Stmt.Function declaration;
        private readonly bool isInit;
        public FiaFunction(Stmt.Function declaration, Environment closure, 
            bool isInit = false)
        {
            this.declaration = declaration;
            this.closure = closure;
            this.isInit = isInit;
        }
        public int Arity()
        {
            return declaration.parameters.Count;
        }

        public object? Call(Interpreter interpreter, List<object?> args)
        {
            var env = new Environment(closure);

            for (int i = 0; i < args.Count; i++)
            {
                env.Define(declaration.parameters[i], args[i]);
            }

            interpreter.ExecuteBlock(declaration.body, env);
            return null;
        }

        public FiaFunction Bind(FiaInstance instance)
        {
            var env = new Environment(closure);
            env.InternalDefine("this", instance);
            return new FiaFunction(declaration, env);
        }
    }
}
