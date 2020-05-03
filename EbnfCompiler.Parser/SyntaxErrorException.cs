using EbnfCompiler.Compiler;

namespace EbnfCompiler.Parser
{
   public class SyntaxErrorException : CompilerException
   {
      public SyntaxErrorException(TokenKind expecting, IToken token)
         : base($"Expecting: {expecting}. Found: {token.TokenKind} At: {token.Location.StartLine} {token.Location.StartColumn}",
            token.Location)
      {
      }
   }
}
