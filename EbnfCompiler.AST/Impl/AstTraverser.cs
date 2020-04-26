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

      public event Action<IAstNode> ProcessNode;

      public event Action PostProcessNode;

      public void Traverse(IAstNode astNode)
      {
         ProcessNode?.Invoke(astNode);

         switch (astNode.AstNodeType)
         {
            case AstNodeType.Statement:
               _tracer.BeginTrace("Statement");

               if (astNode.AsStatement().PreActionNode != null)
                  Traverse(astNode.AsStatement().PreActionNode);

               Traverse(astNode.AsStatement().Expression);

               if (astNode.AsStatement().PostActionNode != null)
                  Traverse(astNode.AsStatement().PostActionNode);

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
               _tracer.TraceLine($"Action - {astNode.AsActionNode().ActionName}");

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

         PostProcessNode?.Invoke();
      }
   }
}
