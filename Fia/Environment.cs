using System.Collections.Generic;
using Fia.Error;

namespace Fia
{
    internal class Environment
    {
        public readonly Environment? parentEnv;
        private readonly Dictionary<string, Object?> values;
        public Environment(Environment? env = null)
        {
            parentEnv = env;
            values = new Dictionary<string, Object?>();
        }
        public void Define(Token variable, Object? value)
        {
            string name = variable.lexeme;
            if (!values.ContainsKey(name))
            {
                values.Add(name, value);
                return;
            }

            throw new RuntimeError(variable,
                $"Variable '{name}' already exists in this context.");
        }

        public void Assign(Token variable, Object? value)
        {
            string name = variable.lexeme;
            if (values.ContainsKey(name))
            {
                values[name] = value;
                return;
            }

            if (parentEnv != null)
            {
                parentEnv.Assign(variable, value);
                return;
            }

            throw new RuntimeError(variable, 
                $"Undefined variable '{name}'.");
        }

        public void AssignAt(int distance, Token variable, Object? value)
        {
            FindAncestor(distance)?.Assign(variable, value);
        }

        public Object? Get(Token variable)
        {
            string name = variable.lexeme;

            if (values.ContainsKey(name))
            {
                return values[name];
            }

            if (parentEnv != null) return parentEnv.Get(variable);

            throw new RuntimeError(variable, $"Undefined variable '{name}'.");
        }

        public Object? GetAt(int distance, Token variable)
        {
            return FindAncestor(distance)?.Get(variable);
        }

        private Environment? FindAncestor(int distance)
        {
            var env = this;
            for (int i = 0; i < distance; i++)
            {
                env = env?.parentEnv;
            }
            return env;
        }
    }
}
