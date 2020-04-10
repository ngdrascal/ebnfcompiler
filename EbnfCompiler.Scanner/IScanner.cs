using EbnfCompiler.Compiler;

namespace EbnfCompiler.Scanner
{
   public interface IScanner
   {
      IToken CurrentToken { get; }
      void Advance();
   }
}
