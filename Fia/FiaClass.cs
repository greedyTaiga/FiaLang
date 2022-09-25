namespace Fia
{
    internal class FiaClass : IFiaCallable
    {
        public readonly string name;
        private readonly Dictionary<string, FiaFunction> methods;

        public FiaClass(string name, Dictionary<string, FiaFunction> methods)
        {
            this.name = name;
            this.methods = methods;
        }

        public int Arity()
        {
            var init = FindMethod("init");
            if (init == null) return 0;
            return init.Arity();
        }

        public Object? Call(Interpreter interpreter, List<object?> args)
        {
            var instance = new FiaInstance(this);
            var init = FindMethod("init");
            if (init != null)
            {
                init.Bind(instance).Call(interpreter, args);
            }
            return instance;
        }

        public FiaFunction? FindMethod(string name)
        {
            if (methods.ContainsKey(name)) return methods[name];
            return null;
        }


        public override string ToString()
        {
            return name;
        }

    }
}
