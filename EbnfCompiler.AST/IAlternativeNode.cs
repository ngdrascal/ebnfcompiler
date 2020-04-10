namespace EbnfCompiler.AST
{
   public interface IAlternativeNode : INode
   {
      IAlternativeNode NextAlt { get; set; }
   }
}
