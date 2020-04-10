namespace EbnfCompiler.Compiler
{
   public enum TokenKind
   {
      Identifier, String, Action,
      LeftParen, RightParen, LeftBracket, RightBracket, LeftBrace, RightBrace,
      Period, Or, Assign, Equal,
      TokensTag, EbnfTag,
      Eof, Error
   };
}
