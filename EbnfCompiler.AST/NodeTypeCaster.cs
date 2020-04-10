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
      public static IExpressionNode AsExpression(this INode node)
      {
         if (!(node is IExpressionNode result))
            throw new NodeCastException($"Internal error casting {node.NodeType} to IExpression node. ");

         return result;
      }
   }
}
