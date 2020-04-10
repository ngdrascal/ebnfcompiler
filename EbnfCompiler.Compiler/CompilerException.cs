using System;

namespace EbnfCompiler.Compiler
{
   public class CompilerException : Exception
   {
      private readonly ISourceLocation _location;

      protected CompilerException(string message, ISourceLocation location)
         : base(message)
      {
         _location = location;
      }

      public ISourceLocation Location => _location;
   }
}
