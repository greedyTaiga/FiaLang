using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fia
{
    internal class FiaFunction : IFiaCallable
    {
        private readonly Environment closure;
        private readonly Stmt.Function declaration;
        public FiaFunction(Stmt.Function declaration, Environment closure)
        {
            this.declaration = declaration;
            this.closure = closure;
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
    }
}
