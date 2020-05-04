using System;

namespace EbnfCompiler.AST.Impl
{
   public static class NodeTypeCaster
   {
      public static ISyntaxNode AsSyntax(this IAstNode astNode)
      {
         if (!(astNode is ISyntaxNode result))
            throw new NodeCastException(ErrorMessage(astNode.AstNodeType, typeof(ISyntaxNode)));

         return result;
      }

      public static IStatementNode AsStatement(this IAstNode astNode)
      {
         if (!(astNode is IStatementNode result))
            throw new NodeCastException(ErrorMessage(astNode.AstNodeType, typeof(IStatementNode)));

         return result;
      }
      
      public static IExpressionNode AsExpression(this IAstNode astNode)
      {
         if (!(astNode is IExpressionNode result))
            throw new NodeCastException(ErrorMessage(astNode.AstNodeType, typeof(IExpressionNode)));

         return result;
      }

      public static ITermNode AsTerm(this IAstNode astNode)
      {
         if (!(astNode is ITermNode result))
            throw new NodeCastException(ErrorMessage(astNode.AstNodeType, typeof(ITermNode)));

         return result;
      }

      public static IFactorNode AsFactor(this IAstNode astNode)
      {
         if (!(astNode is IFactorNode result))
            throw new NodeCastException(ErrorMessage(astNode.AstNodeType, typeof(IFactorNode)));

         return result;
      }

      public static IProdRefNode AsProdRef(this IAstNode astNode)
      {
         if (!(astNode is IProdRefNode result))
            throw new NodeCastException(ErrorMessage(astNode.AstNodeType, typeof(IProdRefNode)));

         return result;
      }

      public static ITerminalNode AsTerminal(this IAstNode astNode)
      {
         if (!(astNode is ITerminalNode result))
            throw new NodeCastException(ErrorMessage(astNode.AstNodeType, typeof(ITerminalNode)));

         return result;
      }

      public static IParenNode AsParen(this IAstNode astNode)
      {
         if (!(astNode is IParenNode result))
            throw new NodeCastException(ErrorMessage(astNode.AstNodeType, typeof(IParenNode)));
         return result;
      }

      public static IOptionNode AsOption(this IAstNode astNode)
      {
         if (!(astNode is IOptionNode result))
            throw new NodeCastException(ErrorMessage(astNode.AstNodeType, typeof(IOptionNode)));

         return result;
      }

      public static IKleeneStarNode AsKleene(this IAstNode astNode)
      {
         if (!(astNode is IKleeneStarNode result))
            throw new NodeCastException(ErrorMessage(astNode.AstNodeType, typeof(IKleeneStarNode)));

         return result;
      }

      public static IActionNode AsAction(this IAstNode astNode)
      {
         if (!(astNode is IActionNode result))
            throw new NodeCastException(ErrorMessage(astNode.AstNodeType, typeof(IActionNode)));

         return result;
      }

      private static string ErrorMessage(AstNodeType fromType, Type toType)
      {
         return $"Internal error casting {fromType} to {toType.Name}.";
      }
   }
}
