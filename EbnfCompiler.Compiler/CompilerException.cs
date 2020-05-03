using System;

namespace EbnfCompiler.Compiler
{
   public class CompilerException : Exception
   {
      protected CompilerException(string message, ISourceLocation location)
         : base(message)
      {
         Location = location;
      }

      public ISourceLocation Location { get; }
   }
}
