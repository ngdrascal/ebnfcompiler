using System.Diagnostics;

namespace EbnfCompiler.AST
{
   public class AstTraverser
   {
      public void Traverse(INode node)
      {
         switch (node.NodeType)
         {
            case NodeType.Expression:
               BeginTrace("Expression");

               var term = node.AsExpression().FirstTerm;
               while (term != null)
               {
                  Traverse(term);

                  term = term.NextTerm;
               }

               EndTrace("Expression");
               break;

            case NodeType.Term:
               BeginTrace("Term");

               var factor = node.AsTerm().FirstFactor;
               while (factor != null)
               {
                  Traverse(factor);
                  factor = factor.NextFactor;
               }

               EndTrace("Term");
               break;

            case NodeType.Factor:
               BeginTrace(node.NodeType.ToString());

               if (node.AsFactor().FactorExpr is IProdRefNode)
                  TraceLine($"Production: {node.Image}");
               else if (node.AsFactor().FactorExpr is ITerminalNode)
                  TraceLine($"Terminal: {node.Image}");
               else if (node.AsFactor().FactorExpr is ILParenNode)
                  Traverse(node.AsFactor().FactorExpr);
               else if (node.AsFactor().FactorExpr is ILOptionNode)
                  Traverse(node.AsFactor().FactorExpr);
               else if (node.AsFactor().FactorExpr is ILKleeneStarNode)
                  Traverse(node.AsFactor().FactorExpr);

               EndTrace(node.NodeType.ToString());
               break;

            case NodeType.ProdRef:
               TraceLine($"ProdRef - {node.AsProdRef().ProdName}");
               break;

            case NodeType.Terminal:
               TraceLine($"Terminal - {node.AsTerminal().TermName}");
               break;

            case NodeType.Action:
               TraceLine("ActName - {}");

               break;

            case NodeType.LParen:
               BeginTrace("LParens");

               Traverse(node.AsLParen().Expression);

               EndTrace("LParens");
               break;

            case NodeType.BeginOption:
               BeginTrace("BeginOption");

               Traverse(node.AsLOption().Expression);

               EndTrace("BeginOption");
               break;

            case NodeType.BeginKleeneStar:
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
