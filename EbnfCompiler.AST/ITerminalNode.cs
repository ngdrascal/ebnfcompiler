namespace EbnfCompiler.AST
{
   public interface ITerminalNode : INode
   {
      string TermName { get; }
   }
}