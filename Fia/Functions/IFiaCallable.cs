namespace FiaLang
{
    internal interface IFiaCallable
    {
        int Arity();
        public Object? Call(Interpreter interpreter, List<Object?> args);
    }
}
