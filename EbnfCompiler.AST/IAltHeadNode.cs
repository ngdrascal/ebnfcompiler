namespace EbnfCompiler.AST
{
   public interface IAltHeadNode : INode
   {
      int AltCount { get; set; }
      IAlternativeNode FirstAlt { get; }
   }
}
