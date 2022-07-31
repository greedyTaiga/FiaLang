using System;
namespace Fia
{
    public class Fia
    {
        private static readonly Interpreter interpreter = new Interpreter();
        private static bool errorEncountered = false;

        
        public static void Main(string[] args)
        {
            
            if (args.Length == 0)
            {
                RunPrompt();
            } 
            else if (args.Length == 1)
            {
                var path = args[0];
                string? text;

                if(File.Exists(path))
                {
                    text = File.ReadAllText(path);
                }
                else
                {
                    text = null;
                    Console.WriteLine("Invalid path");
                    return;
                }

                if (text != null)
                {
                    Run(text);
                } else
                {
                    Console.WriteLine("Invalid File");
                }
            } 
        }

        private static void RunPrompt()
        {
            while (true)
            {
                Console.Write(">");
                string? line = Console.ReadLine();

                if (line == null) break;

                Run(line);
            }
        }
        private static void Run(string text)
        {
            errorEncountered = false;

            var scanner = new Scanner(text);
            var tokens = scanner.ScanTokens();

            var parser = new Parser(tokens);

            var statements = parser.Parse();

            var resolver = new Resolver(interpreter);
            resolver.Run(statements);

            if (errorEncountered) return;

            interpreter.Interpret(statements);
        }

        internal static void Error(string message, int line)
        {
            errorEncountered = true;
            Console.WriteLine($"On line {line}, {message}");
        }
    }
}
