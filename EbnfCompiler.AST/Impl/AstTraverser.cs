using System;
using EbnfCompiler.Compiler;

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

         _tracer.BeginTrace(astNode.AstNodeType.ToString());

         switch (astNode.AstNodeType)
         {
            case AstNodeType.Syntax:
               if (astNode.AsSyntax().PreActionNode != null)
                  Traverse(astNode.AsSyntax().PreActionNode);

               foreach (var stmt in astNode.AsSyntax().Statements)
                  Traverse(stmt);

               if (astNode.AsSyntax().PostActionNode != null)
                  Traverse(astNode.AsSyntax().PostActionNode);

               break;

            case AstNodeType.Statement:
               if (astNode.AsStatement().PreActionNode != null)
                  Traverse(astNode.AsStatement().PreActionNode);

               Traverse(astNode.AsStatement().Expression);

               if (astNode.AsStatement().PostActionNode != null)
                  Traverse(astNode.AsStatement().PostActionNode);

               break;

            case AstNodeType.Expression:
               if (astNode.AsExpression().PreActionNode != null)
                  Traverse(astNode.AsExpression().PreActionNode);

               foreach(var term in astNode.AsExpression().Terms)
                  Traverse(term);

               if (astNode.AsExpression().PostActionNode != null)
                  Traverse(astNode.AsExpression().PreActionNode);

               break;

            case AstNodeType.Term:
               foreach(var factor in astNode.AsTerm().Factors)
                  Traverse(factor);

               break;

            case AstNodeType.Factor:
               if (astNode.AsFactor().PreActionNode != null)
                  Traverse(astNode.AsFactor().PreActionNode);

               Traverse(astNode.AsFactor().FactorExpr);

               if (astNode.AsFactor().PostActionNode != null)
                  Traverse(astNode.AsFactor().PostActionNode);

               break;

            case AstNodeType.ProdRef:
               _tracer.TraceLine($"{astNode.AsProdRef().ProdName}");
               
               break;

            case AstNodeType.Terminal:
               _tracer.TraceLine($"{astNode.AsTerminal().TermName}");
              
               break;

            case AstNodeType.Action:
               _tracer.TraceLine($"{astNode.AsAction().ActionName}");

               break;

            case AstNodeType.Paren:
               Traverse(astNode.AsParen().Expression);

               break;

            case AstNodeType.Option:
               Traverse(astNode.AsOption().Expression);

               break;

            case AstNodeType.KleeneStar:
               Traverse(astNode.AsKleene().Expression);

               break;
         }

         _tracer.EndTrace(astNode.AstNodeType.ToString());

         PostProcessNode?.Invoke();
      }
   }
}
