namespace EbnfCompiler.AST
{
   public interface ITermNode : INode
   {
      ITermNode NextTerm { get; set; }
   }
}
