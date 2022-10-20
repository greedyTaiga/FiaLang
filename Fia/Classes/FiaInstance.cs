using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fia.Error;

namespace Fia
{
    internal class FiaInstance
    {
        private FiaClass classObj;
        private readonly Dictionary<string, object?> fields;

        public FiaInstance(FiaClass classObj)
        {
            this.classObj = classObj;
            this.fields = new Dictionary<string, object?>();
        }

        public object? Get(Token name)
        {
            if (fields.ContainsKey(name.lexeme))
            {
                return fields[name.lexeme];
            }

            var method = classObj.FindMethod(name.lexeme);
            if (method != null) return method.Bind(this);

            throw new RuntimeError(name, $"Undefined property {name.lexeme}.");
        }

        public void Set(Token name, object? value)
        {
            fields[name.lexeme] = value;
        }

        public override string ToString()
        {
            
            return classObj.name + " instance";
        }
    }
}
