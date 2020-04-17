using EbnfCompiler.Compiler;

namespace EbnfCompiler.AST.Impl
{
   public abstract class AstNode : IAstNode
   {
      protected readonly IDebugTracer Tracer;
      protected readonly TerminalSet FirstSetInternal = new TerminalSet();

      public ISourceLocation Location { get; set; }

      public AstNodeType AstNodeType { get; }

      public string Image { get; protected set; }

      public ITerminalSet FirstSet
      {
         get
         {
            if (!FirstSetInternal.IsEmpty())
               return FirstSetInternal;

            Tracer.BeginTrace(message: $"First: {GetType().Name}: {this}");

            CalcFirstSet();

            Tracer.EndTrace($"First: {GetType().Name} = {FirstSetInternal} ");

            return FirstSetInternal;
         }
      }

      protected AstNode(AstNodeType astNodeType, IToken token, IDebugTracer tracer)
      {
         Tracer = tracer;
         AstNodeType = astNodeType;
         Location = token.Location;
         Image = token.Image;
      }

      protected abstract void CalcFirstSet();
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
         return $"{Expression} .";
      }

      protected override void CalcFirstSet()
      {
         FirstSetInternal.Union(Expression.FirstSet);
      }
   }

   public class ExpressionNode : AstNode, IExpressionNode
   {
      public ExpressionNode(IToken token, IDebugTracer tracer)
         : base(AstNodeType.Expression, token, tracer)
      {
         Image = string.Empty;
         FirstTerm = null;
      }

      public ITermNode FirstTerm { get; private set; }

      public void AppendTerm(ITermNode newTerm)
      {
         if (FirstTerm == null)
            FirstTerm = newTerm;
         else
         {
            var t = FirstTerm;
            while (t.NextTerm != null)
               t = t.NextTerm;
            t.NextTerm = newTerm;
         }
      }

      public override string ToString()
      {
         var result = string.Empty;

         var term = FirstTerm;
         while (term != null)
         {
            result += term.ToString();
            term = term.NextTerm;
            if (term != null)
               result += " | ";
         }

         return result;
      }

      protected override void CalcFirstSet()
      {
         var term = FirstTerm;
         while (term != null)
         {
            // ReSharper disable once RedundantArgumentDefaultValue
            FirstSetInternal.Union(term.FirstSet, true);

            term = term.NextTerm;
         }
      }
   }

   public class TermNode : AstNode, ITermNode
   {
      public TermNode(IToken token, IDebugTracer tracer)
         : base(AstNodeType.Term, token, tracer)
      {
      }

      public ITermNode NextTerm { get; set; }

      public IFactorNode FirstFactor { get; private set; }

      public void AppendFactor(IFactorNode newFactor)
      {
         if (FirstFactor == null)
            FirstFactor = newFactor;
         else
         {
            var t = FirstFactor;
            while (t.NextFactor != null)
               t = t.NextFactor;
            t.NextFactor = newFactor;
         }
      }

      public override string ToString()
      {
         var result = string.Empty;

         var factor = FirstFactor;
         while (factor != null)
         {
            result += " " + factor.ToString();
            factor = factor.NextFactor;
         }

         return result;
      }

      protected override void CalcFirstSet()
      {
         var allIncludeEpsilon = true;

         var factor = FirstFactor;
         while (factor != null)
         {
            FirstSetInternal.Union(factor.FirstSet, false);
            if (!factor.FirstSet.IncludesEpsilon)
               allIncludeEpsilon = false;

            // keep going as long as FirstSet includes Epsilon
            factor = factor.FactorExpr.FirstSet.IncludesEpsilon ? factor.NextFactor : null;
         }

         if (allIncludeEpsilon)
            FirstSetInternal.Add(FirstSetInternal.Epsilon);
      }
   }

   public class FactorNode : AstNode, IFactorNode
   {
      public FactorNode(IToken token, IDebugTracer tracer)
         : base(AstNodeType.Factor, token, tracer)
      {
      }

      public IAstNode FactorExpr { get; set; }

      public IFactorNode NextFactor { get; set; }

      public override string ToString()
      {
         return FactorExpr.ToString();
      }

      protected override void CalcFirstSet()
      {
         FirstSetInternal.Union(FactorExpr.FirstSet);
      }
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
         return $"<{ProdName}>";
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
         return $"( {Expression} )";
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
         return $"[ {Expression} ]";
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
         return $"#{ActionName}#";
      }

      protected override void CalcFirstSet()
      {
      }
   }
}