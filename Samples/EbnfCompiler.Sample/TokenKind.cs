namespace EbnfCompiler.Sample
{
    public enum TokenKind
    {
        Let, Identifier, Number, String, Print, NumberLiteral, StringLiteral,
        Comma, Semicolon, Colon, Assign, LeftParen, RightParen, Plus, Minus, Asterisk, ForwardSlash,
        Eof, Error
    };
}
