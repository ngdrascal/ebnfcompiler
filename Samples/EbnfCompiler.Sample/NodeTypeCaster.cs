using System;

namespace EbnfCompiler.Sample
{
   public static class NodeTypeCaster
   {
      public static IVarStatementNode AsVarStatement(this IAstNode astNode)
      {
         if (!(astNode is IVarStatementNode result))
            throw new NodeCastException(ErrorMessage(astNode.AstNodeType, typeof(IVarStatementNode)));

         return result;
      }

      public static IPrintStatementNode AsPrintStatement(this IAstNode astNode)
      {
         if (!(astNode is IPrintStatementNode result))
            throw new NodeCastException(ErrorMessage(astNode.AstNodeType, typeof(IPrintStatementNode)));

         return result;
      }

      public static IUnaryOperatorNode AsUnaryOp(this IAstNode astNode)
      {
         if (!(astNode is IUnaryOperatorNode result))
            throw new NodeCastException(ErrorMessage(astNode.AstNodeType, typeof(IUnaryOperatorNode)));

         return result;
      }

      public static IBinaryOperatorNode AsBinaryOp(this IAstNode astNode)
      {
         if (!(astNode is IBinaryOperatorNode result))
            throw new NodeCastException(ErrorMessage(astNode.AstNodeType, typeof(IBinaryOperatorNode)));

         return result;
      }

      public static IHaveNodeType AsNodeWithType(this IAstNode astNode)
      {
         if (!(astNode is IHaveNodeType result))
            throw new NodeCastException(ErrorMessage(astNode.AstNodeType, typeof(IHaveNodeType)));

         return result;
      }

      private static string ErrorMessage(AstNodeTypes fromType, Type toType)
      {
         return $"Internal error casting {fromType} to {toType.Name}.";
      }
   }
}
