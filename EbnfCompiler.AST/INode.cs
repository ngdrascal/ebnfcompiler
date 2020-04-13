using EbnfCompiler.Compiler;

namespace EbnfCompiler.AST
{
   public enum NodeType
   {
      Statement, Expression, Term, Factor, ProdRef, Terminal, Action,
      LParen, BeginOption, BeginKleeneStar
   };

   public interface INode
   {
      ISourceLocation Location { get; set; }

      NodeType NodeType { get; }

      string Image { get; }

      ITerminalSet FirstSet { get; }

      string ToString();
   }

   public interface IStatementNode : INode
   {
      IExpressionNode Expression { get; set; }
   }

   public interface IExpressionNode : INode
   {
      int TermCount { get; }

      ITermNode FirstTerm { get; }

      void AppendTerm(ITermNode newTerm);
   }

   public interface ITermNode : INode
   {
      ITermNode NextTerm { get; set; }

      IFactorNode FirstFactor { get; }

      void AppendFactor(IFactorNode newFactor);
   }

   public interface IFactorNode : INode
   {
      INode FactorExpr { get; set; }
      
      IFactorNode NextFactor { get; set; }
   }

   public interface IProdRefNode : INode
   {
      string ProdName { get; }
   }

   public interface ITerminalNode : INode
   {
      string TermName { get; }
   }

   public interface ILParenNode
   {
      IExpressionNode Expression { get; set; }
   }

   public interface ILOptionNode
   {
      IExpressionNode Expression { get; set; }
   }

   public interface ILKleeneStarNode
   {
      IExpressionNode Expression { get; set; }
   }

   public interface IActionNode
   {
      string ActionName { get; }
   }
}
