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

               CheckForValue(astNode, "Expression", astNode.AsStatement().Expression);
               Traverse(astNode.AsStatement().Expression);

               if (astNode.AsStatement().PostActionNode != null)
                  Traverse(astNode.AsStatement().PostActionNode);

               break;

            case AstNodeType.Expression:
               if (astNode.AsExpression().PreActionNode != null)
                  Traverse(astNode.AsExpression().PreActionNode);

               foreach (var term in astNode.AsExpression().Terms)
                  Traverse(term);

               if (astNode.AsExpression().PostActionNode != null)
                  Traverse(astNode.AsExpression().PreActionNode);

               break;

            case AstNodeType.Term:
               if (astNode.AsTerm().PreActionNode != null)
                  Traverse(astNode.AsTerm().PreActionNode);

               foreach (var factor in astNode.AsTerm().Factors)
                  Traverse(factor);

               if (astNode.AsTerm().PostActionNode != null)
                  Traverse(astNode.AsTerm().PreActionNode);

               break;

            case AstNodeType.Factor:
               if (astNode.AsFactor().PreActionNode != null)
                  Traverse(astNode.AsFactor().PreActionNode);

               CheckForValue(astNode, "FactorExpr", astNode.AsFactor().FactorExpr);
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
               CheckForValue(astNode, "Expression", astNode.AsParen().Expression);

               Traverse(astNode.AsParen().Expression);

               break;

            case AstNodeType.Option:
               CheckForValue(astNode, "Expression", astNode.AsOption().Expression);
               Traverse(astNode.AsOption().Expression);

               break;

            case AstNodeType.KleeneStar:
               CheckForValue(astNode, "Expression", astNode.AsKleene().Expression);
               Traverse(astNode.AsKleene().Expression);

               break;
         }

         _tracer.EndTrace(astNode.AstNodeType.ToString());

         PostProcessNode?.Invoke();
      }

      private void CheckForValue(IAstNode parentNode, string propertyName, IAstNode propertyValue)
      {
         if (propertyValue == null)
            throw new AstErrorException(message: $"Invalid AST node: {parentNode.AstNodeType.ToString()}. Missing value for property {propertyName}.");
      }
   }
}
