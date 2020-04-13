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
      public static IStatementNode AsStatement(this INode node)
      {
         if (!(node is IStatementNode result))
            throw new NodeCastException(ErrorMessage(node.NodeType, typeof(IStatementNode)));

         return result;
      }


      public static IExpressionNode AsExpression(this INode node)
      {
         if (!(node is IExpressionNode result))
            throw new NodeCastException(ErrorMessage(node.NodeType, typeof(IExpressionNode)));

         return result;
      }

      public static ITermNode AsTerm(this INode node)
      {
         if (!(node is ITermNode result))
            throw new NodeCastException(ErrorMessage(node.NodeType, typeof(ITermNode)));

         return result;
      }

      public static IFactorNode AsFactor(this INode node)
      {
         if (!(node is IFactorNode result))
            throw new NodeCastException(ErrorMessage(node.NodeType, typeof(IFactorNode)));

         return result;
      }

      public static IProdRefNode AsProdRef(this INode node)
      {
         if (!(node is IProdRefNode result))
            throw new NodeCastException(ErrorMessage(node.NodeType, typeof(IProdRefNode)));

         return result;
      }

      public static ITerminalNode AsTerminal(this INode node)
      {
         if (!(node is ITerminalNode result))
            throw new NodeCastException(ErrorMessage(node.NodeType, typeof(ITerminalNode)));

         return result;
      }

      public static ILParenNode AsLParen(this INode node)
      {
         if (!(node is ILParenNode result))
            throw new NodeCastException(ErrorMessage(node.NodeType, typeof(ILParenNode)));
         return result;
      }

      public static ILOptionNode AsLOption(this INode node)
      {
         if (!(node is ILOptionNode result))
            throw new NodeCastException(ErrorMessage(node.NodeType, typeof(ILOptionNode)));

         return result;
      }

      public static ILKleeneStarNode AsLKleeneStarNode(this INode node)
      {
         if (!(node is ILKleeneStarNode result))
            throw new NodeCastException(ErrorMessage(node.NodeType, typeof(ILKleeneStarNode)));

         return result;
      }

      private static string ErrorMessage(NodeType fromType, Type toType)
      {
         if (toType == null) 
            throw new ArgumentNullException(nameof(toType));

         return $"Internal error casting {fromType} to {nameof(toType)}.";
      }
   }
}
