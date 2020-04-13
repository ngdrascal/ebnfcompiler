using EbnfCompiler.Compiler;

namespace EbnfCompiler.AST
{
   public abstract class Node : INode
   {
      protected readonly IDebugTracer Tracer;
      protected readonly TerminalSet FirstSetInternal = new TerminalSet();

      public ISourceLocation Location { get; set; }

      public NodeType NodeType { get; }

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

      protected Node(NodeType nodeType, IToken token, IDebugTracer tracer)
      {
         Tracer = tracer;
         NodeType = nodeType;
         Location = token.Location;
         Image = token.Image;
      }

      protected abstract void CalcFirstSet();
   }

   public class StatementNode : Node, IStatementNode
   {
      public StatementNode(IToken token, IDebugTracer tracer)
         : base(NodeType.Statement, token, tracer)
      {
      }

      public IExpressionNode Expression { get; set; }

      public override string ToString()
      {
         return $"{Expression} .";
      }

      protected override void CalcFirstSet()
      {
      }
   }

   public class ExpressionNode : Node, IExpressionNode
   {
      public ExpressionNode(IToken token, IDebugTracer tracer)
         : base(NodeType.Expression, token, tracer)
      {
         Image = string.Empty;
         TermCount = 0;
         FirstTerm = null;
      }

      public int TermCount { get; set; }

      public ITermNode FirstTerm { get; private set; }

      public void AppendTerm(ITermNode newTerm)
      {
         TermCount++;

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
         var allIncludeEpsilon = true;

         var term = FirstTerm;
         while (term != null)
         {
            FirstSetInternal.Union(term.FirstSet, false);
            if (!term.FirstSet.IncludesEpsilon)
               allIncludeEpsilon = false;

            term = term.NextTerm;
         }

         if (allIncludeEpsilon)
            FirstSetInternal.Add(FirstSetInternal.Epsilon);
      }
   }

   public class TermNode : Node, ITermNode
   {
      public TermNode(IToken token, IDebugTracer tracer)
         : base(NodeType.Term, token, tracer)
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

            factor = factor.NextFactor;
         }

         if (allIncludeEpsilon)
            FirstSetInternal.Add(FirstSetInternal.Epsilon);
      }
   }

   public class FactorNode : Node, IFactorNode
   {
      public FactorNode(IToken token, IDebugTracer tracer)
         : base(NodeType.Factor, token, tracer)
      {
      }

      public INode FactorExpr { get; set; }

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

   public class ProdRefNode : Node, IProdRefNode
   {
      public ProdRefNode(IToken token, IDebugTracer tracer)
         : base(NodeType.ProdRef, token, tracer)
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

   public class TerminalNode : Node, ITerminalNode
   {
      public TerminalNode(IToken token, IDebugTracer tracer)
         : base(NodeType.Terminal, token, tracer)
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

   public class LParenNode : Node, ILParenNode
   {
      public LParenNode(IToken token, IDebugTracer tracer)
         : base(NodeType.LParen, token, tracer)
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

   public class LOptionNode : Node, ILOptionNode
   {
      public LOptionNode(IToken token, IDebugTracer tracer)
         : base(NodeType.BeginOption, token, tracer)
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

   public class LKleeneNode : Node, ILKleeneStarNode
   {
      public LKleeneNode(IToken token, IDebugTracer tracer)
         : base(NodeType.BeginKleeneStar, token, tracer)
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

   public class ActionNode : Node, IActionNode
   {
      public ActionNode(IToken token, IDebugTracer tracer)
         : base(NodeType.Action, token, tracer)
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
         // if (Next != null)
         // {
         //    _firstSet.Add(Next.FirstSet);
         //    _firstSet.IncludesEpsilon = Next.FirstSet.IncludesEpsilon;
         // }
         // else
         //    _firstSet.IncludesEpsilon = false;
      }
   }
}