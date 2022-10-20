namespace FiaLang
{
    enum TokenType
    {
        //Single-character tokens.
        LEFT_PAREN, RIGHT_PAREN, LEFT_BRACE, RIGHT_BRACE,
        DOT, COMMA, SEMICOLON, PLUS, MINUS, SLASH, STAR,

        //One or two charcater tokens.
        BANG, EQUAL, BANG_EQUAL, EQUAL_EQUAL,
        LESSER, GREATER, LESSER_EQUAL, GREATER_EQUAL,
        SLASH_SLASH,

        //Literals.
        IDENTIFIER, NUMBER, STRING,

        //Keywords.
        IF, ELSE, FOR, WHILE, AND, OR,
        VAR, FUNC, CLASS,
        RETURN, SUPER, THIS,
        PRINT,
        TRUE, FALSE, NOLLA,

        EOF
    }
}