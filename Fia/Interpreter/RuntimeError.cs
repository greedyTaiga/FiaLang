namespace FiaLang.Error
{
    internal class RuntimeError : Exception
    {
        public readonly Token token;
        public readonly string message;
        public RuntimeError(Token token, string message)
        {
            this.token = token;
            this.message = message;
        }
    }
}
