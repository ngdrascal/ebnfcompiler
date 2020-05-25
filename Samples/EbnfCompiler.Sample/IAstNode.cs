using System.Collections.Generic;

namespace EbnfCompiler.Sample
{
    public enum UnaryOperators { Plus, Minus };

    public enum BinaryOperators { Add, Subtract, Multiply, Divide };

    public enum AstNodeTypes
    {
        VarStatement, PrintStatement, PrintExpression,
        UnaryOperator, BinaryOperator, NumberLiteral, StringLiteral, VarReference
    };

    public interface IHaveNodeType
    {
        string TypeName { get; set; }
    }

    public interface IAstNode
    {
        AstNodeTypes AstNodeType { get; }

        ISourceLocation Location { get; }

        string ToString();
    }

    public interface IVarStatementNode : IAstNode
    {
        IVariableNode Variable { get; set; }

        IAstNode Expression { get; set; }
    }

    public interface IPrintStatementNode : IAstNode
    {
        IReadOnlyCollection<IPrintExpressionNode> PrintExpressions { get; }

        void AppendExpression(IPrintExpressionNode expression);
    }

    public interface IPrintExpressionNode : IAstNode
    {
        IAstNode Expression { get; set; }
    }

    public interface IUnaryOperatorNode : IAstNode, IHaveNodeType
    {
        public UnaryOperators Operator { get; }

        public IAstNode Operand { get; set; }
    }

    public interface IBinaryOperatorNode : IAstNode, IHaveNodeType
    {
        public BinaryOperators Operator { get; }

        public IAstNode LeftOperand { get; set; }

        public IAstNode RightOperand { get; set; }
    }

    public interface INumberLiteralNode : IAstNode, IHaveNodeType
    {
        float Value { get; }
    }

    public interface IStringLiteralNode : IAstNode, IHaveNodeType
    {
        string Value { get; }
    }

    public interface IVariableNode : IAstNode, IHaveNodeType
    {
        string Name { get; }
    }
}
