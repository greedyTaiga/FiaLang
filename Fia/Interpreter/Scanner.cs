using static FiaLang.TokenType;

namespace FiaLang
{
    internal class Scanner
    {
        private static readonly Dictionary<string, TokenType> KeywordDict =
            new Dictionary<string, TokenType>
            {
                ["if"] = IF,
                ["else"] = ELSE,
                ["for"] = FOR,
                ["while"] = WHILE,
                ["and"] = AND,
                ["or"] = OR,
                ["var"] = VAR,
                ["func"] = FUNC,
                ["class"] = CLASS,
                ["return"] = RETURN,
                ["super"] = SUPER,
                ["this"] = THIS,
                ["print"] = PRINT,
                ["true"] = TRUE,
                ["false"] = FALSE,
                ["nolla"] = NOLLA
            };

        private readonly string source;
        private readonly List<Token> tokens;

        private int line = 1;
        private int current = 0;
        private int tokenStart = 0;
        public Scanner(string source)
        {
            this.source = source;
            tokens = new List<Token>();
        }

        public List<Token> ScanTokens()
        {
            while (!ReachedEnd())
            {
                tokenStart = current;
                ScanToken();
            }

            //Adds and end of file token at the end.
            tokens.Add(new Token(EOF, "", null, line));

            return tokens;
        }

        private void ScanToken()
        {
            char c = Advance();
            switch (c)
            {
                //Single character tokens.
                case '(': AddToken(LEFT_PAREN); break;
                case ')': AddToken(RIGHT_PAREN); break;
                case '{': AddToken(LEFT_BRACE); break;
                case '}': AddToken(RIGHT_BRACE); break;
                case '+': AddToken(PLUS); break;
                case '-': AddToken(MINUS); break;
                case '*': AddToken(STAR); break;
                case '.': AddToken(DOT); break;
                case ',': AddToken(COMMA); break;
                case ';': AddToken(SEMICOLON); break;

                //One or two character tokens.
                case '=': AddToken(Match('=') ? EQUAL_EQUAL : EQUAL); break;
                case '!': AddToken(Match('=') ? BANG_EQUAL : BANG); break;
                case '<': AddToken(Match('=') ? LESSER_EQUAL : LESSER); break;
                case '>': AddToken(Match('=') ? GREATER_EQUAL : GREATER); break;

                //Slash and double slash(comment).
                case '/':
                    //If it's a comment, it gets ignored.
                    if (Match('/')) SkipLine();
                    else AddToken(SLASH);
                    break;

                //String literal.
                case '\"':
                    CaptureString();
                    break;

                //Skip space characters.
                case '\t':
                case '\r':
                case ' ':
                    break;
                //Reached a new line.
                case '\n':
                    line++;
                    break;

                default:
                    if (char.IsNumber(c))
                    {
                        CaptureNumber();
                    }
                    else if (IsAlpha(c))
                    {
                        CaptureKeywordOrIdentifier();
                    }
                    else Fia.Error("Unexpected character.", line);
                    break;
            }

        }

        void AddToken(TokenType type, object? literal = null)
        {
            string lex = source.Substring(tokenStart, current - tokenStart);
            tokens.Add(new Token(type, lex, literal, line));
        }

        private char Advance()
        {
            if (ReachedEnd()) return '\0';
            return source[current++];
        }

        private char Peek()
        {
            if (ReachedEnd()) return '\0';
            return source[current];
        }

        private char PeekNext()
        {
            if (current + 1 >= source.Length) return '\0';
            return source[current + 1];
        }

        private bool ReachedEnd() => current >= source.Length;

        private bool Match(char x)
        {
            if (source[current] == x)
            {
                Advance();
                return true;
            }
            return false;
        }

        private void CaptureString()
        {
            while (Peek() != '\"')
            {
                char x = Advance();
                if (x == '\n' || x == '\0')
                {
                    Fia.Error("Unterminated string", line);
                    return;
                }
            }

            string literal = source.Substring(tokenStart + 1, current - tokenStart - 1);
            Advance();
            AddToken(STRING, literal);
        }

        private void CaptureNumber()
        {
            while (char.IsNumber(Peek()))
            {
                Advance();
            }
            if (Peek() == '.' && char.IsNumber(PeekNext())) Advance();

            while (char.IsNumber(Peek()))
            {
                Advance();
            }
            string lex = source.Substring(tokenStart, current - tokenStart);

            var literal = Double.Parse(lex);

            AddToken(NUMBER, literal);
        }

        private void CaptureKeywordOrIdentifier()
        {
            while (IsAlphaNumeric(Peek())) Advance();

            string word = source.Substring(tokenStart, current - tokenStart);

            TokenType type;

            if (!KeywordDict.TryGetValue(word, out type))
            {
                type = IDENTIFIER;
            }

            AddToken(type);
        }

        private void SkipLine()
        {
            while (current < source.Length && source[current] != '\n')
            {
                current++;
            }
            if (current < source.Length) current++;
            line++;
            
        }

        //Helper functions.
        private static bool IsAlpha(char c)
        {
            return char.IsLetter(c) || c == '_';
        }

        private static bool IsAlphaNumeric(char c)
        {
            return IsAlpha(c) || char.IsNumber(c);
        }

    }
}
