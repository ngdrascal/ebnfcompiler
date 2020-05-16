using System;

namespace EbnfCompiler.Sample
{
   public static class NodeTypeCaster
   {
      public static IVarStatementNode AsVarStatement(this IAstNode astNode)
      {
         if (!(astNode is IVarStatementNode result))
            throw new NodeCastException(ErrorMessage(astNode.AstNodeTypes, typeof(IVarStatementNode)));

         return result;
      }

      public static IUnaryOperatorNode AsUnaryOp(this IAstNode astNode)
      {
         if (!(astNode is IUnaryOperatorNode result))
            throw new NodeCastException(ErrorMessage(astNode.AstNodeTypes, typeof(IUnaryOperatorNode)));

         return result;
      }

      public static IBinaryOperatorNode AsBinaryOp(this IAstNode astNode)
      {
         if (!(astNode is IBinaryOperatorNode result))
            throw new NodeCastException(ErrorMessage(astNode.AstNodeTypes, typeof(IBinaryOperatorNode)));

         return result;
      }

      private static string ErrorMessage(AstNodeTypes fromType, Type toType)
      {
         return $"Internal error casting {fromType} to {toType.Name}.";
      }
   }
}
