namespace EbnfCompiler.Sample
{
   public interface IToken
   {
      ISourceLocation Location { get; }
      string Image { get; set; }
      TokenKind TokenKind { get; set; }
   }
}
