﻿namespace EbnfCompiler.Sample
{
   public enum TokenKind
   {
      Var, Identifier, Number, String, Print, PrintLine, NumberLiteral, StringLiteral,
      SemiColon, Colon, Assign, LeftParen, RightParen, Plus, Minus, Asterisk, ForwardSlash,
      Eof, Error
   };
}
