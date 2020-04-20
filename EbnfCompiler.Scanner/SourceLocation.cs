using System.Diagnostics.CodeAnalysis;
using EbnfCompiler.Compiler;

namespace EbnfCompiler.Scanner
{
   [ExcludeFromCodeCoverage]
   internal class SourceLocation : ISourceLocation
   {
      public int StartLine { get; set; }
      public int StartColumn { get; set; }
      public int StopLine { get; set; }
      public int StopColumn { get; set; }
   }
}
