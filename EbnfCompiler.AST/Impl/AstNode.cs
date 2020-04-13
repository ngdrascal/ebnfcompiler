using EbnfCompiler.Compiler;

namespace EbnfCompiler.AST
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

   public class StatementAstNode : AstNode, IStatementAstNode
   {
      public StatementAstNode(IToken token, IDebugTracer tracer)
         : base(AstNodeType.Statement, token, tracer)
      {
      }

      public IExpressionAstNode ExpressionAst { get; set; }

      public override string ToString()
      {
         return $"{ExpressionAst} .";
      }

      protected override void CalcFirstSet()
      {
      }
   }

   public class ExpressionAstNode : AstNode, IExpressionAstNode
   {
      public ExpressionAstNode(IToken token, IDebugTracer tracer)
         : base(AstNodeType.Expression, token, tracer)
      {
         Image = string.Empty;
         TermCount = 0;
         FirstTermAst = null;
      }

      public int TermCount { get; set; }

      public ITermAstNode FirstTermAst { get; private set; }

      public void AppendTerm(ITermAstNode newTermAst)
      {
         TermCount++;

         if (FirstTermAst == null)
            FirstTermAst = newTermAst;
         else
         {
            var t = FirstTermAst;
            while (t.NextTermAst != null)
               t = t.NextTermAst;
            t.NextTermAst = newTermAst;
         }
      }

      public override string ToString()
      {
         var result = string.Empty;

         var term = FirstTermAst;
         while (term != null)
         {
            result += term.ToString();
            term = term.NextTermAst;
            if (term != null)
               result += " | ";
         }

         return result;
      }

      protected override void CalcFirstSet()
      {
         var allIncludeEpsilon = true;

         var term = FirstTermAst;
         while (term != null)
         {
            FirstSetInternal.Union(term.FirstSet, false);
            if (!term.FirstSet.IncludesEpsilon)
               allIncludeEpsilon = false;

            term = term.NextTermAst;
         }

         if (allIncludeEpsilon)
            FirstSetInternal.Add(FirstSetInternal.Epsilon);
      }
   }

   public class TermAstNode : AstNode, ITermAstNode
   {
      public TermAstNode(IToken token, IDebugTracer tracer)
         : base(AstNodeType.Term, token, tracer)
      {
      }

      public ITermAstNode NextTermAst { get; set; }

      public IFactorAstNode FirstFactorAst { get; private set; }

      public void AppendFactor(IFactorAstNode newFactorAst)
      {
         if (FirstFactorAst == null)
            FirstFactorAst = newFactorAst;
         else
         {
            var t = FirstFactorAst;
            while (t.NextFactorAst != null)
               t = t.NextFactorAst;
            t.NextFactorAst = newFactorAst;
         }
      }

      public override string ToString()
      {
         var result = string.Empty;

         var factor = FirstFactorAst;
         while (factor != null)
         {
            result += " " + factor.ToString();
            factor = factor.NextFactorAst;
         }

         return result;
      }

      protected override void CalcFirstSet()
      {
         var allIncludeEpsilon = true;

         var factor = FirstFactorAst;
         while (factor != null)
         {
            FirstSetInternal.Union(factor.FirstSet, false);
            if (!factor.FirstSet.IncludesEpsilon)
               allIncludeEpsilon = false;

            factor = factor.NextFactorAst;
         }

         if (allIncludeEpsilon)
            FirstSetInternal.Add(FirstSetInternal.Epsilon);
      }
   }

   public class FactorAstNode : AstNode, IFactorAstNode
   {
      public FactorAstNode(IToken token, IDebugTracer tracer)
         : base(AstNodeType.Factor, token, tracer)
      {
      }

      public IAstNode FactorExpr { get; set; }

      public IFactorAstNode NextFactorAst { get; set; }

      public override string ToString()
      {
         return FactorExpr.ToString();
      }

      protected override void CalcFirstSet()
      {
         FirstSetInternal.Union(FactorExpr.FirstSet);
      }
   }

   public class ProdRefAstNode : AstNode, IProdRefAstNode
   {
      public ProdRefAstNode(IToken token, IDebugTracer tracer)
         : base(AstNodeType.ProdRef, token, tracer)
      {
         ProdName = token.Image;
      }

      public string ProdName { get; }

      public IExpressionAstNode ExpressionAst { get; set; }

      public override string ToString()
      {
         return $"<{ProdName}>";
      }

      protected override void CalcFirstSet()
      {
         FirstSetInternal.Union(ExpressionAst.FirstSet);
      }
   }

   public class TerminalAstNode : AstNode, ITerminalAstNode
   {
      public TerminalAstNode(IToken token, IDebugTracer tracer)
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

   public class LParenAstNode : AstNode, ILParenNode
   {
      public LParenAstNode(IToken token, IDebugTracer tracer)
         : base(AstNodeType.LParen, token, tracer)
      {
      }

      public IExpressionAstNode ExpressionAst { get; set; }

      public override string ToString()
      {
         return $"( {ExpressionAst} )";
      }

      protected override void CalcFirstSet()
      {
         FirstSetInternal.Union(ExpressionAst.FirstSet);
      }
   }

   public class LOptionAstNode : AstNode, ILOptionNode
   {
      public LOptionAstNode(IToken token, IDebugTracer tracer)
         : base(AstNodeType.BeginOption, token, tracer)
      {
      }

      public IExpressionAstNode ExpressionAst { get; set; }

      public override string ToString()
      {
         return $"[ {ExpressionAst} ]";
      }

      protected override void CalcFirstSet()
      {
         FirstSetInternal.Union(ExpressionAst.FirstSet);
         FirstSetInternal.Add(FirstSetInternal.Epsilon);
      }
   }

   public class LKleeneAstNode : AstNode, ILKleeneStarNode
   {
      public LKleeneAstNode(IToken token, IDebugTracer tracer)
         : base(AstNodeType.BeginKleeneStar, token, tracer)
      {
      }

      public IExpressionAstNode ExpressionAst { get; set; }

      public override string ToString()
      {
         return "{ " + ExpressionAst.ToString() + " }";
      }

      protected override void CalcFirstSet()
      {
         FirstSetInternal.Union(ExpressionAst.FirstSet);
      }
   }

   public class ActionAstNode : AstNode, IActionNode
   {
      public ActionAstNode(IToken token, IDebugTracer tracer)
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