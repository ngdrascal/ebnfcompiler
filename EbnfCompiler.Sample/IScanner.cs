namespace EbnfCompiler.Sample
{
   public interface IScanner
   {
      IToken CurrentToken { get; }
      void Advance();
   }
}
