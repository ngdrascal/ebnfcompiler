﻿using System.Collections.Generic;
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
      string NodeId { get; }

      ISourceLocation Location { get; }

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
      IReadOnlyCollection<IStatementNode> Statements { get; }

      void AppendStatement(IStatementNode newNode);
   }

   public interface IStatementNode : IAstNode, IHaveActions
   {
      string ProdName { get; }

      IExpressionNode Expression { get; set; }
   }

   public interface IExpressionNode : IAstNode, IHaveActions
   {
      IReadOnlyCollection<ITermNode> Terms { get; }

      void AppendTerm(ITermNode newTerm);

      int TermCount { get; }
   }

   public interface ITermNode : IAstNode, IHaveActions
   {
      IReadOnlyCollection<IFactorNode> Factors { get; }

      void AppendFactor(IFactorNode newFactor);
   }

   public interface IFactorNode : IAstNode, IHaveActions
   {
      IAstNode FactorExpr { get; set; }
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

   public interface IParenNode : IAstNode
   {
      IExpressionNode Expression { get; set; }
   }

   public interface IOptionNode : IAstNode
   {
      IExpressionNode Expression { get; set; }
   }

   public interface IKleeneStarNode : IAstNode
   {
      IExpressionNode Expression { get; set; }
   }

   public interface IActionNode : IAstNode
   {
      string ActionName { get; }
   }
}
