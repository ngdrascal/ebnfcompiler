using System;

namespace EbnfCompiler.AST.Impl
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

      public static IParenNode AsLParen(this IAstNode astNode)
      {
         if (!(astNode is IParenNode result))
            throw new NodeCastException(ErrorMessage(astNode.AstNodeType, typeof(IParenNode)));
         return result;
      }

      public static IOptionNode AsLOption(this IAstNode astNode)
      {
         if (!(astNode is IOptionNode result))
            throw new NodeCastException(ErrorMessage(astNode.AstNodeType, typeof(IOptionNode)));

         return result;
      }

      public static IKleeneStarNode AsLKleeneStarNode(this IAstNode astNode)
      {
         if (!(astNode is IKleeneStarNode result))
            throw new NodeCastException(ErrorMessage(astNode.AstNodeType, typeof(IKleeneStarNode)));

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
