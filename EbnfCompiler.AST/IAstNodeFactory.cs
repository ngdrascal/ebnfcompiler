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
         IAstNode newAstNode = astNodeType switch
         {
            AstNodeType.Syntax => new SyntaxNode(token, _tracer),
            AstNodeType.Statement => new StatementNode(token, _tracer),
            AstNodeType.Expression => new ExpressionNode(token, _tracer),
            AstNodeType.Term => new TermNode(token, _tracer),
            AstNodeType.Factor => new FactorNode(token, _tracer),
            AstNodeType.ProdRef => new ProdRefNode(token, _tracer),
            AstNodeType.Terminal => new TerminalNode(token, _tracer),
            AstNodeType.Paren => new ParenNode(token, _tracer),
            AstNodeType.Option => new OptionNode(token, _tracer),
            AstNodeType.KleeneStar => new KleeneNode(token, _tracer),
            AstNodeType.Action => new ActionNode(token, _tracer),
            _ => throw new InvalidOperationException($"Type matching {astNodeType} not found.")
         };

         _allNodes.Add(newAstNode);
         return newAstNode;
      }

      public IReadOnlyCollection<IAstNode> AllNodes => _allNodes.AsReadOnly();
   }
}
