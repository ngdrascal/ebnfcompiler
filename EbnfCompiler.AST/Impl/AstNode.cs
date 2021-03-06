﻿using System.Collections.Generic;
using System.Linq;
using System.Text;
using EbnfCompiler.Compiler;

namespace EbnfCompiler.AST.Impl
{
   public abstract class AstNode : IAstNode
   {
      private readonly IDebugTracer _tracer;
      protected readonly TerminalSet FirstSetInternal = new TerminalSet();
      private static readonly Dictionary<AstNodeType, int> NodeIdSuffix = new Dictionary<AstNodeType, int>()
      {
         {AstNodeType.Syntax, 0},
         {AstNodeType.Statement, 0},
         {AstNodeType.Expression, 0},
         {AstNodeType.Term, 0},
         {AstNodeType.Factor, 0},
         {AstNodeType.ProdRef, 0},
         {AstNodeType.Terminal, 0},
         {AstNodeType.Paren, 0},
         {AstNodeType.Option, 0},
         {AstNodeType.KleeneStar, 0},
         {AstNodeType.Action, 0}
      };

      public string NodeId { get; }

      public ISourceLocation Location { get; }

      public AstNodeType AstNodeType { get; }

      public string Image { get; protected set; }

      public ITerminalSet FirstSet
      {
         get
         {
            if (!FirstSetInternal.IsEmpty())
               return FirstSetInternal;

            _tracer.BeginTrace(message: $"First: {GetType().Name}: {this}");

            CalcFirstSet();

            _tracer.EndTrace($"First: {GetType().Name} = {FirstSetInternal} ");

            return FirstSetInternal;
         }
      }

      protected AstNode(AstNodeType astNodeType, IToken token, IDebugTracer tracer)
      {
         AstNodeType = astNodeType;
         _tracer = tracer;
         Location = token.Location;
         NodeId = $"{AstNodeType}{++NodeIdSuffix[AstNodeType]}";
         Image = token.Image;
      }

      protected abstract void CalcFirstSet();
   }

   public class SyntaxNode : AstNode, ISyntaxNode
   {
      private readonly List<IStatementNode> _statements = new List<IStatementNode>();

      public SyntaxNode(IToken token, IDebugTracer tracer)
         : base(AstNodeType.Syntax, token, tracer)
      {
      }

      protected override void CalcFirstSet()
      {
         // there isn't a first set for the Syntax node
      }

      public IReadOnlyCollection<IStatementNode> Statements => _statements.AsReadOnly();

      public void AppendStatement(IStatementNode newStatement)
      {
         _statements.Add(newStatement);
      }

      public override string ToString()
      {
         var sb = new StringBuilder();
         foreach (var s in _statements)
            sb.AppendLine(s.ToString());

         return sb.ToString();
      }

      public IActionNode PreActionNode { get; set; }
      public IActionNode PostActionNode { get; set; }
   }

   public class StatementNode : AstNode, IStatementNode
   {
      public StatementNode(IToken token, IDebugTracer tracer)
         : base(AstNodeType.Statement, token, tracer)
      {
      }

      public string ProdName => Image;

      public IExpressionNode Expression { get; set; }

      public override string ToString()
      {
         return ProdName + " ::= " + Expression.ToString() + " .";
      }

      protected override void CalcFirstSet()
      {
         FirstSetInternal.Union(Expression.FirstSet);
      }

      public IActionNode PreActionNode { get; set; }
      public IActionNode PostActionNode { get; set; }
   }

   public class ExpressionNode : AstNode, IExpressionNode
   {
      private readonly List<ITermNode> _terms = new List<ITermNode>();

      public ExpressionNode(IToken token, IDebugTracer tracer)
         : base(AstNodeType.Expression, token, tracer)
      {
      }

      public IReadOnlyCollection<ITermNode> Terms => _terms.AsReadOnly();

      public void AppendTerm(ITermNode newTerm)
      {
         _terms.Add(newTerm);
      }

      public int TermCount => _terms.Count;

      public override string ToString()
      {
         var result = string.Empty;

         foreach (var term in _terms.SkipLast(1))
         {
            result += term.ToString();
            result += " | ";
         }

         result += _terms.Last().ToString();

         return result;
      }

      protected override void CalcFirstSet()
      {
         foreach (var term in _terms)
            // ReSharper disable once RedundantArgumentDefaultValue
            FirstSetInternal.Union(term.FirstSet, true);
      }

      public IActionNode PreActionNode { get; set; }
      public IActionNode PostActionNode { get; set; }
   }

   public class TermNode : AstNode, ITermNode
   {
      private readonly List<IFactorNode> _factors = new List<IFactorNode>();

      public TermNode(IToken token, IDebugTracer tracer)
         : base(AstNodeType.Term, token, tracer)
      {
      }

      public IReadOnlyCollection<IFactorNode> Factors => _factors.AsReadOnly();

      public void AppendFactor(IFactorNode newFactor)
      {
         _factors.Add(newFactor);
      }

      public override string ToString()
      {
         var result = _factors.Count > 0 ? _factors.First().ToString() : string.Empty;

         foreach (var fact in _factors.TakeLast(_factors.Count - 1))
            result += " " + fact.ToString();

         return result;
      }

      protected override void CalcFirstSet()
      {
         var allIncludeEpsilon = true;

         foreach (var factor in _factors)
         {
            FirstSetInternal.Union(factor.FirstSet, false);
            if (!factor.FirstSet.IncludesEpsilon)
               allIncludeEpsilon = false;

            // keep going as long as FirstSet includes Epsilon
            if (!factor.FactorExpr.FirstSet.IncludesEpsilon)
               break;
         }

         if (allIncludeEpsilon)
            FirstSetInternal.Add(FirstSetInternal.Epsilon);
      }

      public IActionNode PreActionNode { get; set; }
      public IActionNode PostActionNode { get; set; }
   }

   public class FactorNode : AstNode, IFactorNode
   {
      public FactorNode(IToken token, IDebugTracer tracer)
         : base(AstNodeType.Factor, token, tracer)
      {
      }

      public IAstNode FactorExpr { get; set; }

      public override string ToString()
      {
         return FactorExpr.ToString();
      }

      protected override void CalcFirstSet()
      {
         FirstSetInternal.Union(FactorExpr.FirstSet);
      }

      public IActionNode PreActionNode { get; set; }
      public IActionNode PostActionNode { get; set; }
   }

   public class ProdRefNode : AstNode, IProdRefNode
   {
      public ProdRefNode(IToken token, IDebugTracer tracer)
         : base(AstNodeType.ProdRef, token, tracer)
      {
         ProdName = token.Image;
      }

      public string ProdName { get; }

      public IExpressionNode Expression { get; set; }

      public override string ToString()
      {
         return $"{ProdName}";
      }

      protected override void CalcFirstSet()
      {
         FirstSetInternal.Union(Expression.FirstSet);
      }
   }

   public class TerminalNode : AstNode, ITerminalNode
   {
      public TerminalNode(IToken token, IDebugTracer tracer)
         : base(AstNodeType.Terminal, token, tracer)
      {
         Image = '"' + Image + '"';

         TermName = token.Image;
      }

      public string TermName { get; }

      public override string ToString()
      {
         return Image;
      }

      protected override void CalcFirstSet()
      {
         FirstSetInternal.Add(TermName);
      }
   }

   public class ParenNode : AstNode, IParenNode
   {
      public ParenNode(IToken token, IDebugTracer tracer)
         : base(AstNodeType.Paren, token, tracer)
      {
      }

      public IExpressionNode Expression { get; set; }

      public override string ToString()
      {
         return "( " + Expression.ToString() + " )";
      }

      protected override void CalcFirstSet()
      {
         FirstSetInternal.Union(Expression.FirstSet);
      }
   }

   public class OptionNode : AstNode, IOptionNode
   {
      public OptionNode(IToken token, IDebugTracer tracer)
         : base(AstNodeType.Option, token, tracer)
      {
      }

      public IExpressionNode Expression { get; set; }

      public override string ToString()
      {
         return "[ " + Expression.ToString() + " ]";
      }

      protected override void CalcFirstSet()
      {
         FirstSetInternal.Union(Expression.FirstSet);
         FirstSetInternal.Add(FirstSetInternal.Epsilon);
      }
   }

   public class KleeneNode : AstNode, IKleeneStarNode
   {
      public KleeneNode(IToken token, IDebugTracer tracer)
         : base(AstNodeType.KleeneStar, token, tracer)
      {
      }

      public IExpressionNode Expression { get; set; }

      public override string ToString()
      {
         return "{ " + Expression.ToString() + " }";
      }

      protected override void CalcFirstSet()
      {
         FirstSetInternal.Union(Expression.FirstSet);
      }
   }

   public class ActionNode : AstNode, IActionNode
   {
      public ActionNode(IToken token, IDebugTracer tracer)
         : base(AstNodeType.Action, token, tracer)
      {
         Image = '#' + Image + '#';
         ActionName = token.Image;
      }

      public string ActionName { get; }

      public override string ToString()
      {
         return $"{ActionName}";
      }

      protected override void CalcFirstSet()
      {
         FirstSetInternal.Add("Action");
         FirstSetInternal.Add(FirstSetInternal.Epsilon);
      }
   }
}