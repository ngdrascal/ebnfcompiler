using System.Diagnostics;

namespace EbnfCompiler.AST.Impl
{
   public class AstTraverser
   {
      public void Traverse(IAstNode astNode)
      {
         switch (astNode.AstNodeType)
         {
            case AstNodeType.Expression:
               BeginTrace("Expression");

               var term = astNode.AsExpression().FirstTerm;
               while (term != null)
               {
                  Traverse(term);

                  term = term.NextTerm;
               }

               EndTrace("Expression");
               break;

            case AstNodeType.Term:
               BeginTrace("Term");

               var factor = astNode.AsTerm().FirstFactor;
               while (factor != null)
               {
                  Traverse(factor);
                  factor = factor.NextFactor;
               }

               EndTrace("Term");
               break;

            case AstNodeType.Factor:
               BeginTrace(astNode.AstNodeType.ToString());

               if (astNode.AsFactor().FactorExpr is IProdRefNode)
                  TraceLine($"Production: {astNode.Image}");
               else if (astNode.AsFactor().FactorExpr is ITerminalNode)
                  TraceLine($"Terminal: {astNode.Image}");
               else if (astNode.AsFactor().FactorExpr is ILParenNode)
                  Traverse(astNode.AsFactor().FactorExpr);
               else if (astNode.AsFactor().FactorExpr is ILOptionNode)
                  Traverse(astNode.AsFactor().FactorExpr);
               else if (astNode.AsFactor().FactorExpr is ILKleeneStarNode)
                  Traverse(astNode.AsFactor().FactorExpr);

               EndTrace(astNode.AstNodeType.ToString());
               break;

            case AstNodeType.ProdRef:
               TraceLine($"ProdRef - {astNode.AsProdRef().ProdName}");
               break;

            case AstNodeType.Terminal:
               TraceLine($"Terminal - {astNode.AsTerminal().TermName}");
               break;

            case AstNodeType.Action:
               TraceLine("ActName - {}");

               break;

            case AstNodeType.Paren:
               BeginTrace("LParens");

               Traverse(astNode.AsLParen().Expression);

               EndTrace("LParens");
               break;

            case AstNodeType.Option:
               BeginTrace("BeginOption");

               Traverse(astNode.AsLOption().Expression);

               EndTrace("BeginOption");
               break;

            case AstNodeType.KleeneStar:
               BeginTrace("BeginKleene");

               EndTrace("BeginKleene");
               break;
         }
      }

      private int _traceIndent;

      private void BeginTrace(string message)
      {
         var ident = new string(' ', _traceIndent);
         _traceIndent += 2;

         Trace.WriteLine($"{ident}->{message}");
      }

      private void EndTrace(string message)
      {
         _traceIndent -= 2;
         var ident = new string(' ', _traceIndent);

         Trace.WriteLine($"{ident}<-{message}");
      }

      private void TraceLine(string message)
      {
         var ident = new string(' ', _traceIndent);

         Trace.WriteLine($"{ident}{message}");
      }
   }
}
