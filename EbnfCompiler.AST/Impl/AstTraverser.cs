using System;

namespace EbnfCompiler.AST.Impl
{
   public class AstTraverser : IAstTraverser
   {
      private readonly IDebugTracer _tracer;

      public AstTraverser(IDebugTracer tracer)
      {
         _tracer = tracer;
      }

      public event Action<IAstNode> PreProcess;

      public event Action PostProcess;

      public void Traverse(IAstNode astNode)
      {
         PreProcess?.Invoke(astNode);

         switch (astNode.AstNodeType)
         {
            case AstNodeType.Statement:
               _tracer.BeginTrace("Statement");

               Traverse(astNode.AsStatement().Expression);

               _tracer.EndTrace("Statement");
               break;

            case AstNodeType.Expression:
               _tracer.BeginTrace("Expression");

               var term = astNode.AsExpression().FirstTerm;
               while (term != null)
               {
                  Traverse(term);

                  term = term.NextTerm;
               }

               _tracer.EndTrace("Expression");
               break;

            case AstNodeType.Term:
               _tracer.BeginTrace("Term");

               var factor = astNode.AsTerm().FirstFactor;
               while (factor != null)
               {
                  Traverse(factor);
                  factor = factor.NextFactor;
               }

               _tracer.EndTrace("Term");
               break;

            case AstNodeType.Factor:
               _tracer.BeginTrace(astNode.AstNodeType.ToString());

               Traverse(astNode.AsFactor().FactorExpr);

               _tracer.EndTrace(astNode.AstNodeType.ToString());
               break;

            case AstNodeType.ProdRef:
               _tracer.TraceLine($"ProdRef - {astNode.AsProdRef().ProdName}");
               break;

            case AstNodeType.Terminal:
               _tracer.TraceLine($"Terminal - {astNode.AsTerminal().TermName}");
               break;

            case AstNodeType.Action:
               _tracer.TraceLine("Action - {}");

               break;

            case AstNodeType.Paren:
               _tracer.BeginTrace("LParens");

               Traverse(astNode.AsLParen().Expression);

               _tracer.EndTrace("LParens");
               break;

            case AstNodeType.Option:
               _tracer.BeginTrace("BeginOption");

               Traverse(astNode.AsLOption().Expression);

               _tracer.EndTrace("BeginOption");
               break;

            case AstNodeType.KleeneStar:
               _tracer.BeginTrace("BeginKleene");

               Traverse(astNode.AsLKleeneStarNode().Expression);

               _tracer.EndTrace("BeginKleene");
               break;
         }

         PostProcess?.Invoke();
      }
   }
}
