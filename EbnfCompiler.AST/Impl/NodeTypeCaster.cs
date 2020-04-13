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
      public static IStatementAstNode AsStatement(this IAstNode astNode)
      {
         if (!(astNode is IStatementAstNode result))
            throw new NodeCastException(ErrorMessage(astNode.AstNodeType, typeof(IStatementAstNode)));

         return result;
      }


      public static IExpressionAstNode AsExpression(this IAstNode astNode)
      {
         if (!(astNode is IExpressionAstNode result))
            throw new NodeCastException(ErrorMessage(astNode.AstNodeType, typeof(IExpressionAstNode)));

         return result;
      }

      public static ITermAstNode AsTerm(this IAstNode astNode)
      {
         if (!(astNode is ITermAstNode result))
            throw new NodeCastException(ErrorMessage(astNode.AstNodeType, typeof(ITermAstNode)));

         return result;
      }

      public static IFactorAstNode AsFactor(this IAstNode astNode)
      {
         if (!(astNode is IFactorAstNode result))
            throw new NodeCastException(ErrorMessage(astNode.AstNodeType, typeof(IFactorAstNode)));

         return result;
      }

      public static IProdRefAstNode AsProdRef(this IAstNode astNode)
      {
         if (!(astNode is IProdRefAstNode result))
            throw new NodeCastException(ErrorMessage(astNode.AstNodeType, typeof(IProdRefAstNode)));

         return result;
      }

      public static ITerminalAstNode AsTerminal(this IAstNode astNode)
      {
         if (!(astNode is ITerminalAstNode result))
            throw new NodeCastException(ErrorMessage(astNode.AstNodeType, typeof(ITerminalAstNode)));

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
