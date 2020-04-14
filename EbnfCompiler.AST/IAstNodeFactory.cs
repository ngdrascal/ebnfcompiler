using System;
using System.Collections.Generic;
using EbnfCompiler.AST.Impl;
using EbnfCompiler.Compiler;

namespace EbnfCompiler.AST
{
   public interface IAstNodeFactory
   {
      IAstNode Create(AstNodeType astNodeType, IToken token);

      IReadOnlyCollection<IAstNode> AllNodes { get; }
   }

   public class AstNodeFactory : IAstNodeFactory
   {
      private readonly IDebugTracer _tracer;
      private readonly List<IAstNode> _allNodes = new List<IAstNode>();

      public AstNodeFactory(IDebugTracer tracer)
      {
         _tracer = tracer;
      }

      public IAstNode Create(AstNodeType astNodeType, IToken token)
      {
         IAstNode newAstNode;

         switch (astNodeType)
         {
            case AstNodeType.Statement:
               newAstNode = new StatementNode(token, _tracer);
               break;
            case AstNodeType.Expression:
               newAstNode = new ExpressionNode(token, _tracer);
               break;
            case AstNodeType.Term:
               newAstNode = new TermNode(token, _tracer);
               break;
            case AstNodeType.Factor:
               newAstNode = new FactorNode(token, _tracer);
               break;
            case AstNodeType.ProdRef:
               newAstNode = new ProdRefNode(token, _tracer);
               break;
            case AstNodeType.Terminal:
               newAstNode = new TerminalNode(token, _tracer);
               break;
            case AstNodeType.LParen:
               newAstNode = new LParenNode(token, _tracer);
               break;
            case AstNodeType.BeginOption:
               newAstNode = new LOptionNode(token, _tracer);
               break;
            case AstNodeType.BeginKleeneStar:
               newAstNode = new LKleeneNode(token, _tracer);
               break;
            case AstNodeType.Action:
               newAstNode = new ActionNode(token, _tracer);
               break;
            default:
               throw new InvalidOperationException($"Type matching {astNodeType} not found.");
         }

         _allNodes.Add(newAstNode);
         return newAstNode;
      }

      public IReadOnlyCollection<IAstNode> AllNodes => _allNodes.AsReadOnly();
   }
}
