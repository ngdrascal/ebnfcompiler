using System.Collections.Generic;
using EbnfCompiler.AST;
using Microsoft.Extensions.Logging;

namespace EbnfCompiler.CodeGenerator
{
   public class TestTreeGenerator : ICodeGenerator
   {
      private class StackElement
      {
         public string NodeName { get; set; }
         public IAstNode AstNode { get; set; }
      }

      private readonly IRootNode _rootNode;
      private readonly IAstTraverser _traverser;
      private readonly ILogger _log;
      private readonly Stack<StackElement> _stack = new Stack<StackElement>();
      private readonly Dictionary<AstNodeType, int> _nodeIndexer = new Dictionary<AstNodeType, int>();

      public TestTreeGenerator(IRootNode rootNode, IAstTraverser traverser, ILogger log)
      {
         _rootNode = rootNode;
         _traverser = traverser;
         _log = log;

         PopulateIndexer();
      }

      private void PopulateIndexer()
      {
         _nodeIndexer.Add(AstNodeType.Syntax, 0);
         _nodeIndexer.Add(AstNodeType.Statement, 0);
         _nodeIndexer.Add(AstNodeType.Expression, 0);
         _nodeIndexer.Add(AstNodeType.Term, 0);
         _nodeIndexer.Add(AstNodeType.Factor, 0);
         _nodeIndexer.Add(AstNodeType.ProdRef, 0);
         _nodeIndexer.Add(AstNodeType.Terminal, 0);
         _nodeIndexer.Add(AstNodeType.Paren, 0);
         _nodeIndexer.Add(AstNodeType.Option, 0);
         _nodeIndexer.Add(AstNodeType.KleeneStar, 0);
         _nodeIndexer.Add(AstNodeType.Action, 0);
      }

      public void Run()
      {
         _traverser.ProcessNode += ProcessNode;
         _traverser.PostProcessNode += PostProcessNode;

         _traverser.Traverse(_rootNode.Syntax);
      }

      private void ProcessNode(IAstNode node)
      {
         _stack.Push(new StackElement { NodeName = GenNodeName(node.AstNodeType), AstNode = node });

         switch (node.AstNodeType)
         {
            case AstNodeType.Syntax:

               break;

            case AstNodeType.Statement:

               break;

            case AstNodeType.Expression:

               break;

            case AstNodeType.Term:

               break;

            case AstNodeType.Factor:

               break;

            case AstNodeType.ProdRef:
               var prodRef = _stack.Peek();
               _log.LogDebug($"var {prodRef.NodeName} = new ProRefNode(new Token(TokenKind.String, {prodRef.AstNode.Image}), tracer)");

               var prodRefParent = _stack.ToArray()[1];
               switch (prodRefParent.AstNode.AstNodeType)
               {
                  case AstNodeType.Factor:
                     _log.LogDebug($"{prodRefParent.NodeName}.FactorExpr = {prodRef.NodeName};");
                     break;
               }
               break;

            case AstNodeType.Terminal:
               var terminal = _stack.Peek();
               _log.LogDebug($"var {terminal.NodeName} = new TerminalNode(new Token(TokenKind.String, {terminal.AstNode.Image}), tracer)");

               var terminalParent = _stack.ToArray()[1];
               switch (terminalParent.AstNode.AstNodeType)
               {
                  case AstNodeType.Factor:
                     _log.LogDebug($"{terminalParent.NodeName}.FactorExpr = {terminal.NodeName};");
                     break;
               }
               break;

            case AstNodeType.Action:

               break;

            case AstNodeType.Paren:

               break;

            case AstNodeType.Option:

               break;

            case AstNodeType.KleeneStar:

               break;
         }
      }

      private void PostProcessNode()
      {
         var element = _stack.Pop();
         switch (element.AstNode.AstNodeType)
         {
            case AstNodeType.Syntax:
               break;

            case AstNodeType.Statement:
               break;

            case AstNodeType.Expression:
               break;

            case AstNodeType.Term:
               break;

            case AstNodeType.Factor:
               break;

            case AstNodeType.ProdRef:
               break;

            case AstNodeType.Terminal:
               break;

            case AstNodeType.Action:
               break;

            case AstNodeType.Paren:
               break;

            case AstNodeType.Option:
               break;

            case AstNodeType.KleeneStar:
               break;
         }
      }

      private string GenNodeName(AstNodeType nodeType)
      {
         _nodeIndexer[nodeType]++;
         switch (nodeType)
         {
            case AstNodeType.Syntax: return $"syntax{_nodeIndexer[nodeType]}";
            case AstNodeType.Statement: return $"stmt{_nodeIndexer[nodeType]}";
            case AstNodeType.Expression: return $"expr{_nodeIndexer[nodeType]}";
            case AstNodeType.Term: return $"term{_nodeIndexer[nodeType]}";
            case AstNodeType.Factor: return $"fact{_nodeIndexer[nodeType]}";
            case AstNodeType.ProdRef: return $"prod{_nodeIndexer[nodeType]}";
            case AstNodeType.Terminal: return $"terminal{_nodeIndexer[nodeType]}";
            case AstNodeType.Action: return $"action{_nodeIndexer[nodeType]}";
            case AstNodeType.Paren: return $"paren{_nodeIndexer[nodeType]}";
            case AstNodeType.Option: return $"option{_nodeIndexer[nodeType]}";
            case AstNodeType.KleeneStar: return $"kleene{_nodeIndexer[nodeType]}";

            default: return string.Empty;
         }
      }
   }
}
