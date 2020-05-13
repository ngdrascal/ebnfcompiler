namespace EbnfCompiler.Sample
{
   public class SourceLocation : ISourceLocation
   {
      public int StartLine { get; set; }
      public int StartColumn { get; set; }
      public int StopLine { get; set; }
      public int StopColumn { get; set; }
   }
}
