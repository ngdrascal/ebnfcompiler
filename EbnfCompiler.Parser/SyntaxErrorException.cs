using System;
using EbnfCompiler.Compiler;

namespace EbnfCompiler.Parser
{
   public class SyntaxErrorException : Exception
   {
      private readonly IToken _token;

      public SyntaxErrorException(string message, IToken token)
         : base(message)
      {
         _token = token;
      }

      public IToken Token => _token;
   }
}
