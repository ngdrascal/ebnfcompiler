﻿namespace EbnfCompiler.Sample
{
    public enum TokenKind
    {
        Var, Identifier, Number, String, Print, NumberLiteral, StringLiteral,
        Comma, Semicolon, Colon, Assign, LeftParen, RightParen, Plus, Minus, Asterisk, ForwardSlash,
        Eof, Error
    };
}
