namespace EbnfCompiler.AST
{
   public interface IExpressionNode : INode
   {
      int TermCount { get; }
      ITermNode FirstTerm { get; }

      void AppendTerm(ITermNode newTerm);
   }
}
