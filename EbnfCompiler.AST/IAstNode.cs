using EbnfCompiler.Compiler;

namespace EbnfCompiler.AST
{
   public enum AstNodeType
   {
      Syntax, Statement, Expression, Term, Factor, ProdRef, Terminal, Action,
      Paren, Option, KleeneStar
   };

   public interface IAstNode
   {
      ISourceLocation Location { get; set; }

      AstNodeType AstNodeType { get; }

      string Image { get; }

      ITerminalSet FirstSet { get; }

      string ToString();
   }

   public interface IHaveActions
   {
      IActionNode PreActionNode { get; set; }
      IActionNode PostActionNode { get; set; }
   }

   public interface ISyntaxNode : IAstNode, IHaveActions
   {
      IStatementNode FirstStatement { get; set; }
   }

   public interface IStatementNode : IAstNode, IHaveActions
   {
      string ProdName { get; }
      IExpressionNode Expression { get; set; }
   }

   public interface IExpressionNode : IAstNode, IHaveActions
   {
      ITermNode FirstTerm { get; }

      void AppendTerm(ITermNode newTerm);

      int TermCount { get; }
   }

   public interface ITermNode : IAstNode, IHaveActions
   {
      ITermNode NextTerm { get; set; }

      IFactorNode FirstFactor { get; }

      void AppendFactor(IFactorNode newFactor);
   }

   public interface IFactorNode : IAstNode, IHaveActions
   {
      IAstNode FactorExpr { get; set; }

      IFactorNode NextFactor { get; set; }
   }

   public interface IProdRefNode : IAstNode
   {
      string ProdName { get; }
      IExpressionNode Expression { get; set; }
   }

   public interface ITerminalNode : IAstNode
   {
      string TermName { get; }
   }

   public interface IParenNode
   {
      IExpressionNode Expression { get; set; }
   }

   public interface IOptionNode
   {
      IExpressionNode Expression { get; set; }
   }

   public interface IKleeneStarNode
   {
      IExpressionNode Expression { get; set; }
   }

   public interface IActionNode: IAstNode
   {
      string ActionName { get; }
   }
}
