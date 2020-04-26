using System;
using EbnfCompiler.Compiler;

namespace EbnfCompiler.Parser
{
   public class SyntaxErrorException : Exception
   {
      private readonly IToken _token;

      public SyntaxErrorException(TokenKind expecting, IToken token)
         : base($"Expecting: {expecting}. Found: {token.TokenKind} At: {token.Location.StartLine} {token.Location.StartColumn}")
      {
         _token = token;
      }

      public IToken Token => _token;
   }
}
