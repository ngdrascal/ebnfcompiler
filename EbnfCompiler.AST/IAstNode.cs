using EbnfCompiler.Compiler;

namespace EbnfCompiler.AST
{
   public enum AstNodeType
   {
      Statement, Expression, Term, Factor, ProdRef, Terminal, Action,
      LParen, BeginOption, BeginKleeneStar
   };

   public interface IAstNode
   {
      ISourceLocation Location { get; set; }

      AstNodeType AstNodeType { get; }

      string Image { get; }

      ITerminalSet FirstSet { get; }

      string ToString();
   }

   public interface IStatementNode : IAstNode
   {
      IExpressionNode Expression { get; set; }
   }

   public interface IExpressionNode : IAstNode
   {
      ITermNode FirstTerm { get; }

      void AppendTerm(ITermNode newTerm);
   }

   public interface ITermNode : IAstNode
   {
      ITermNode NextTerm { get; set; }

      IFactorNode FirstFactor { get; }

      void AppendFactor(IFactorNode newFactor);
   }

   public interface IFactorNode : IAstNode
   {
      IAstNode FactorExpr { get; set; }
      
      IFactorNode NextFactor { get; set; }
   }

   public interface IProdRefNode : IAstNode
   {
      string ProdName { get; }
   }

   public interface ITerminalNode : IAstNode
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
