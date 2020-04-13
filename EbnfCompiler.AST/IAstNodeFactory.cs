using System;
using System.Collections.Generic;
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
               newAstNode = new StatementAstNode(token, _tracer);
               break;
            case AstNodeType.Expression:
               newAstNode = new ExpressionAstNode(token, _tracer);
               break;
            case AstNodeType.Term:
               newAstNode = new TermAstNode(token, _tracer);
               break;
            case AstNodeType.Factor:
               newAstNode = new FactorAstNode(token, _tracer);
               break;
            case AstNodeType.ProdRef:
               newAstNode = new ProdRefAstNode(token, _tracer);
               break;
            case AstNodeType.Terminal:
               newAstNode = new TerminalAstNode(token, _tracer);
               break;
            case AstNodeType.LParen:
               newAstNode = new LParenAstNode(token, _tracer);
               break;
            case AstNodeType.BeginOption:
               newAstNode = new LOptionAstNode(token, _tracer);
               break;
            case AstNodeType.BeginKleeneStar:
               newAstNode = new LKleeneAstNode(token, _tracer);
               break;
            case AstNodeType.Action:
               newAstNode = new ActionAstNode(token, _tracer);
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
