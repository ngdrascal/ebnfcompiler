namespace EbnfCompiler.Sample
{
   public interface ISourceLocation
   {
      int StartLine { get; set; }
      int StartColumn { get; set; }
      int StopLine { get; set; }
      int StopColumn { get; set; }
   }
}
