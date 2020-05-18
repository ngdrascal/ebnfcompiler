using System.Linq;

namespace EbnfCompiler.Sample.Impl
{
   public class SyntaxErrorException : CompilerException
   {
      public SyntaxErrorException(TokenKind expecting, IToken token)
         : base($"Expecting: {expecting}. Found: {token.TokenKind}.",
            token.Location)
      {
      }

      public SyntaxErrorException(TokenKind[] expecting, IToken token)
         : base($"Expecting: {string.Join(", ", expecting.Select(t=>t.ToString()))}. Found: {token.TokenKind}.",
                token.Location)
      {
      }
   }
}
