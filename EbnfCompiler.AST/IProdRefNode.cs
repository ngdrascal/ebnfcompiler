namespace EbnfCompiler.AST
{
   public interface IProdRefNode : INode
   {
      string ProdName { get; }
   }
}