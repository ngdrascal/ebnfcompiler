using System;

namespace EbnfCompiler.AST
{
   public class NodeCastException : Exception
   {
      public NodeCastException()
      {
      }

      public NodeCastException(string message)
      : base(message)
      {
      }

      public NodeCastException(string message, Exception innerException)
         : base(message, innerException)
      {
      }
   }

   public static class NodeTypeCaster
   {
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

      public static ILParenNode AsLParen(this IAstNode astNode)
      {
         if (!(astNode is ILParenNode result))
            throw new NodeCastException(ErrorMessage(astNode.AstNodeType, typeof(ILParenNode)));
         return result;
      }

      public static ILOptionNode AsLOption(this IAstNode astNode)
      {
         if (!(astNode is ILOptionNode result))
            throw new NodeCastException(ErrorMessage(astNode.AstNodeType, typeof(ILOptionNode)));

         return result;
      }

      public static ILKleeneStarNode AsLKleeneStarNode(this IAstNode astNode)
      {
         if (!(astNode is ILKleeneStarNode result))
            throw new NodeCastException(ErrorMessage(astNode.AstNodeType, typeof(ILKleeneStarNode)));

         return result;
      }

      private static string ErrorMessage(AstNodeType fromType, Type toType)
      {
         if (toType == null) 
            throw new ArgumentNullException(nameof(toType));

         return $"Internal error casting {fromType} to {nameof(toType)}.";
      }
   }
}
