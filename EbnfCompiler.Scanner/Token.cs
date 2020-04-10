using EbnfCompiler.Compiler;

namespace EbnfCompiler.Scanner
{
   internal class Token : IToken
   {
      public ISourceLocation Location { get; private set; }
      public string Image { get; set; }
      public TokenKind TokenKind { get; set; }

      public Token()
      {
         Location = new SourceLocation();
         Image = string.Empty;
         TokenKind = TokenKind.Error;
      }
   }
}
