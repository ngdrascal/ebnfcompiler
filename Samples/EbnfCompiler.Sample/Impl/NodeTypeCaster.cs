using System;

namespace EbnfCompiler.Sample.Impl
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

        public static IPrintExpressionNode AsPrintExpression(this IAstNode astNode)
        {
            if (!(astNode is IPrintExpressionNode result))
                throw new NodeCastException(ErrorMessage(astNode.AstNodeType, typeof(IPrintExpressionNode)));

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

        public static INumberLiteralNode AsNumberLit(this IAstNode astNode)
        {
            if (!(astNode is INumberLiteralNode result))
                throw new NodeCastException(ErrorMessage(astNode.AstNodeType, typeof(INumberLiteralNode)));

            return result;
        }

        public static IStringLiteralNode AsStringLit(this IAstNode astNode)
        {
            if (!(astNode is IStringLiteralNode result))
                throw new NodeCastException(ErrorMessage(astNode.AstNodeType, typeof(IStringLiteralNode)));

            return result;
        }

        public static IVariableNode AsVarReferene(this IAstNode astNode)
        {
            if (!(astNode is IVariableNode result))
                throw new NodeCastException(ErrorMessage(astNode.AstNodeType, typeof(IVariableNode)));

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
