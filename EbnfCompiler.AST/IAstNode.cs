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

   public interface IStatementAstNode : IAstNode
   {
      IExpressionAstNode ExpressionAst { get; set; }
   }

   public interface IExpressionAstNode : IAstNode
   {
      int TermCount { get; }

      ITermAstNode FirstTermAst { get; }

      void AppendTerm(ITermAstNode newTermAst);
   }

   public interface ITermAstNode : IAstNode
   {
      ITermAstNode NextTermAst { get; set; }

      IFactorAstNode FirstFactorAst { get; }

      void AppendFactor(IFactorAstNode newFactorAst);
   }

   public interface IFactorAstNode : IAstNode
   {
      IAstNode FactorExpr { get; set; }
      
      IFactorAstNode NextFactorAst { get; set; }
   }

   public interface IProdRefAstNode : IAstNode
   {
      string ProdName { get; }
   }

   public interface ITerminalAstNode : IAstNode
   {
      string TermName { get; }
   }

   public interface ILParenNode
   {
      IExpressionAstNode ExpressionAst { get; set; }
   }

   public interface ILOptionNode
   {
      IExpressionAstNode ExpressionAst { get; set; }
   }

   public interface ILKleeneStarNode
   {
      IExpressionAstNode ExpressionAst { get; set; }
   }

   public interface IActionNode
   {
      string ActionName { get; }
   }
}
