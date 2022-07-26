using System; 
using static Fia.TokenType;

namespace Fia
{
    internal class Parser
    {
        private class ParseError : Exception {}

        private readonly List<Token> tokens;
        private int current = 0;

        public Parser(List<Token> tokens)
        {
            this.tokens = tokens;
        }

        public List<Stmt?> Parse()
        {
            var statements = new List<Stmt?>();

            while (!ReachedEnd())
            {
                try
                {
                    statements.Add(Declaration());
                }
                catch(ParseError)
                {
                    Synchronize();
                }
            }

            return statements;
        }

        //Statements
        private Stmt Declaration()
        {
            if (Match(VAR))
            {
                return VarDeclaration();
            }
            if (Match(FUNC))
            {
                return FuncDeclaration();
            }


            return Statement();
        }

        private Stmt FuncDeclaration()
        {
            var name = Consume(IDENTIFIER, "Expect function name after 'func'.");

            Consume(LEFT_PAREN, "Expect '(' after function name.");
            var parameters = Parameters();
            Consume(RIGHT_PAREN, "Expect ')' after parameters.");

            Consume(LEFT_BRACE, "Expect '{' before function body.");
            var body = ((Stmt.Block)Block()).statements;
            
            return new Stmt.Function(name, parameters, body);
        }

        private List<Token> Parameters()
        {
            var parameters = new List<Token>();
            Token? token;
            if (!Match(out token, IDENTIFIER)) return parameters;
            parameters.Add(token);

            while(Match(COMMA))
            {
                token = Consume(IDENTIFIER, "Expect parameter name after ','.");
                parameters.Add(token);
            }
            return parameters;
        }

        private Stmt VarDeclaration()
        {
            Token name = Consume(IDENTIFIER, "Expect variable name after 'var'.");
            Expr? init = null;
            if (Match(EQUAL))
            {
                init = Expression();
            }
            Consume(SEMICOLON, "Expect ';' at the of variable declaration");

            return new Stmt.Var(name, init);
        }
        private Stmt Statement()
        {
            //Potential improvement: make this a switch statement using Peek()
            if (Match(PRINT))return PrintStatement();
            if (Match(IF))return Conditional();
            if (Match(WHILE))return Loop();
            if (Match(FOR)) return ForLoop();
            if (Match(LEFT_BRACE)) return Block();
            if (Match(RETURN)) return Returning();

            return ExpressionStatement();
        }

        private Stmt Returning()
        {
            Token keyword = Previous();
            Expr? val = null;
            if (!Check(SEMICOLON))
            {
                val = Expression();
            }

            Consume(SEMICOLON, "Expect ';' after return value.");

            return new Stmt.Returning(keyword, val);
        }

        private Stmt Block()
        {
            var statements = new List<Stmt>();
            while(!ReachedEnd() && !Check(RIGHT_BRACE))
            {
                statements.Add(Declaration());
            }

            Consume(RIGHT_BRACE, "Expect '}' at the end of a block.");

            return new Stmt.Block(statements);
        }

        private Stmt Conditional()
        {
            Consume(LEFT_PAREN, "Expect '(' after 'if'.");
            Expr condition = Expression();
            Consume(RIGHT_PAREN, "Expect ')' after if condition.");

            Stmt thenBranch = Statement();
            Stmt? elseBranch = null;

            if (Match(ELSE))
            {
                elseBranch = Statement();
            }
            return new Stmt.Conditional(condition, thenBranch, elseBranch);
        }

        private Stmt Loop()
        {
            Consume(LEFT_PAREN, "Expect '(' after 'if'.");
            Expr condition = Expression();
            Consume(RIGHT_PAREN, "Expect ')' after if condition.");

            Stmt stmt = Statement();

            return new Stmt.Loop(condition, stmt);
        }

        //Parses for statement to a while.
        private Stmt ForLoop()
        {
            Consume(LEFT_PAREN, "Expect '(' after 'for'.");

            Stmt? init = null;
            if (Match(VAR))
            {
                init = VarDeclaration();
            }
            else if (!Match(SEMICOLON))
            {
                init = ExpressionStatement();
            }

            Expr? cond = null;
            if (!Check(SEMICOLON))
            {
                cond = Expression();
            }
            Consume(SEMICOLON, "Expected ';' after loop condition.");

            Expr? incr = null;
            if (!Check(RIGHT_PAREN))
            {
                incr = Expression();
            }
            Consume(RIGHT_PAREN, "Expected ')' at the end of for.");

            Stmt body = Statement();
            if (incr != null)
            {
                body = new Stmt.Block(
                    new List<Stmt> 
                    { 
                        body,
                        new Stmt.Expression(incr)
                    }) ;
            }
            if (cond == null) cond = new Expr.Literal(true);
            
            Stmt loop = new Stmt.Loop(cond, body);

            if (init != null)
            {
                loop = new Stmt.Block( new List<Stmt> {init , loop} );
            }

            return loop;
        }

        private Stmt PrintStatement()
        {
            Expr val = Expression();
            Consume(SEMICOLON, "Expect ';' at the end of statement.");
            return new Stmt.Print(val);
        }

        private Stmt ExpressionStatement()
        {
            Expr expr = Expression();
            Consume(SEMICOLON, "Expect ';' at the end of statement.");
            return new Stmt.Expression(expr);
        }

        //Expressions

        private Expr Expression()
        {
            return Assigment();
        }

        private Expr Assigment()
        {
            Expr expr = LogicOr();

            if (Match(out Token? token, EQUAL))
            {
                Expr val = LogicOr();
                
                if (expr is Expr.Variable)
                {
                    Token name = ((Expr.Variable)expr).name;
                    return new Expr.Assigment(name, val);
                }
                Error(token, "Invalid assigment.");
            }

            return expr;
        }

        private Expr LogicOr()
        {
            Expr expr = LogicAnd();

            while(Match(out Token? oper, OR))
            {
                Expr right = LogicAnd();

                expr = new Expr.Logical(expr, oper, right);
            }

            return expr;
        }

        private Expr LogicAnd()
        {
            Expr expr = Equality();

            while (Match(out Token? oper, AND))
            {
                Expr right = Equality();

                expr = new Expr.Logical(expr, oper, right);
            }

            return expr;
        }
        
        private Expr Equality()
        {
            var expr = Comparison();

            while (Match(out Token? oper, EQUAL_EQUAL, BANG_EQUAL))
            {
                expr = new Expr.Binary(expr, oper, Comparison());
            }

            return expr;
        }

        private Expr Comparison()
        {
            var expr = Term();

            while ( Match(out Token? oper, LESSER, 
                LESSER_EQUAL, GREATER ,GREATER_EQUAL))
            {
                expr = new Expr.Binary(expr, oper, Term());
            }

            return expr;
        }

        private Expr Term()
        {
            var expr = Factor();

            while (Match(out Token? oper, PLUS, MINUS))
            {
                expr = new Expr.Binary(expr, oper, Factor());
            }

            return expr;
        }

        private Expr Factor()
        {
            var expr = Unary();

            while (Match(out Token? oper, STAR, SLASH))
            {
                expr = new Expr.Binary(expr, oper, Unary());
            }

            return expr;
        }

        private Expr Unary()
        {
            if (Match(out Token? oper, BANG, MINUS))
            {
                return new Expr.Unary(oper, Unary());
            }
            return Call();
        }

        private Expr Call()
        {
            Expr expr = Primary();
            while(Match(LEFT_PAREN))
            {
                var args = Arguments();
                Token token =
                    Consume(RIGHT_PAREN, "Expect ')' at the end of function call");
                expr = new Expr.Call(expr, token, args);
            }
            return expr;
        }

        private List<Expr> Arguments()
        {
            var args = new List<Expr>();

            while (!Check(RIGHT_PAREN))
            {
                if (args.Count > 0) Consume(COMMA, "Expect ',' between arguments.");
                if (args.Count > 255) Error(Peek(), "Can't have more than 255 arguments.");
                Expr arg = Expression();
                args.Add(arg);
            }

            return args;
        }

        private Expr Primary()
        {

            if (Match( TRUE)) return new Expr.Literal(true);
            if (Match(FALSE)) return new Expr.Literal(false);
            if (Match(NOLLA)) return new Expr.Literal(null);

            Token? token;

            if (Match(out token, STRING, NUMBER))
            {
                return new Expr.Literal(token?.literal);
            }

            if (Match(LEFT_PAREN))
            {
                var expr = Expression();
                Consume(RIGHT_PAREN, "No closing parenthesis.");
                return new Expr.Grouping(expr);
            }

            if (Match(out token, IDENTIFIER))
            {
                return new Expr.Variable(token);
            }

            throw Error(Peek(), "Expected Expression.");
        }


        //Helper methods for going through the tokens list.
        private Token Peek()
        {
            return tokens[current];
        }

        private Token Previous()
        {
            if (current > 0) return tokens[current - 1];
            return Peek();
        }

        private bool Check(TokenType type)
        {
            return Peek().type == type;
        }
        private bool ReachedEnd()
        {
            return Peek().type == EOF;
        }
        private Token Advance()
        {
            if (ReachedEnd()) return tokens[current];
            return tokens[current++];
        }
        private bool Match(params TokenType[] types)
        {
            foreach (var type in types)
            {
                if (Peek().type == type)
                {
                    Advance();
                    return true;
                }
            }
            return false;
        }
        private bool Match(out Token? token, params TokenType[] types)
        {
            foreach(var type in types)
            {
                if (Peek().type == type)
                {
                    token = Advance();
                    return true;
                }
            }
            token = null;
            return false;
        }
        private Token Consume(TokenType type, string errMessage)
        {
            if (Match(out Token? token, type))
            {
                return token;
            }
            else throw Error(Peek(), errMessage);
        }
        
        //Error Handling.
        private static ParseError Error(Token token, string message)
        {
            Fia.Error(message, token.line);
            return new ParseError();
        }

        private void Synchronize()
        {
            while(!ReachedEnd())
            {
                if (Match(SEMICOLON)) return;

                switch (Peek().type)
                {
                    case CLASS:
                    case FUNC:
                    case IF:
                    case FOR:
                    case WHILE:
                    case VAR:
                    case PRINT:
                    case RETURN:
                        return;
                }
                Advance();
            }

        }

    }
}