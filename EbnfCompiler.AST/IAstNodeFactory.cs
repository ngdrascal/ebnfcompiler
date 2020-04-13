using System;
using System.Collections.Generic;
using EbnfCompiler.Compiler;

namespace EbnfCompiler.AST
{
   public interface IAstNodeFactory
   {
      INode Create(NodeType nodeType, IToken token);

      IReadOnlyCollection<INode> AllNodes { get; }
   }

   public class AstNodeFactory : IAstNodeFactory
   {
      private readonly IDebugTracer _tracer;
      private readonly List<INode> _allNodes = new List<INode>();

      public AstNodeFactory(IDebugTracer tracer)
      {
         _tracer = tracer;
      }

      public INode Create(NodeType nodeType, IToken token)
      {
         INode newNode;

         switch (nodeType)
         {
            case NodeType.Statement:
               newNode = new StatementNode(token, _tracer);
               break;
            case NodeType.Expression:
               newNode = new ExpressionNode(token, _tracer);
               break;
            case NodeType.Term:
               newNode = new TermNode(token, _tracer);
               break;
            case NodeType.Factor:
               newNode = new FactorNode(token, _tracer);
               break;
            case NodeType.ProdRef:
               newNode = new ProdRefNode(token, _tracer);
               break;
            case NodeType.Terminal:
               newNode = new TerminalNode(token, _tracer);
               break;
            case NodeType.LParen:
               newNode = new LParenNode(token, _tracer);
               break;
            case NodeType.BeginOption:
               newNode = new LOptionNode(token, _tracer);
               break;
            case NodeType.BeginKleeneStar:
               newNode = new LKleeneNode(token, _tracer);
               break;
            case NodeType.Action:
               newNode = new ActionNode(token, _tracer);
               break;
            default:
               throw new InvalidOperationException($"Type matching {nodeType} not found.");
         }

         _allNodes.Add(newNode);
         return newNode;
      }

      public IReadOnlyCollection<INode> AllNodes => _allNodes.AsReadOnly();
   }
}
