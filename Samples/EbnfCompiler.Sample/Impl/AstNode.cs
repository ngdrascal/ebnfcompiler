using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace EbnfCompiler.Sample.Impl
{
    public class AstNodeBase : IAstNode
    {
        protected AstNodeBase(AstNodeTypes nodeType, IToken token)
        {
            AstNodeType = nodeType;
            Location = token.Location;
        }

        public AstNodeTypes AstNodeType { get; }

        public ISourceLocation Location { get; }
    }

    public class VarStatementNode : AstNodeBase, IVarStatementNode
    {
        public VarStatementNode(IToken token) : base(AstNodeTypes.VarStatement, token)
        {
        }

        public IVariableNode Variable { get; set; }

        public IAstNode Expression { get; set; }

        public override string ToString()
        {
            return $"let {Variable?.ToString()} : {Variable?.TypeName} = {Expression?.ToString()};";
        }
    }

    public class PrintStatementNode : AstNodeBase, IPrintStatementNode
    {
        private readonly List<IPrintExpressionNode> _expressions = new List<IPrintExpressionNode>();

        public PrintStatementNode(IToken token) : base(AstNodeTypes.PrintStatement, token)
        {
        }

        public IReadOnlyCollection<IPrintExpressionNode> PrintExpressions => _expressions.AsReadOnly();

        public void AppendExpression(IPrintExpressionNode expression)
        {
            _expressions.Add(expression);
        }

        public override string ToString()
        {
            var expressions = string.Join(", ", _expressions.Select(node => node.ToString()));
            return $"Print({expressions});";
        }
    }

    public class PrintExpressionNode : AstNodeBase, IPrintExpressionNode
    {
        public PrintExpressionNode(IToken token) : base(AstNodeTypes.PrintExpression, token)
        {
        }

        public IAstNode Expression { get; set; }

        public override string ToString()
        {
            return Expression.ToString();
        }
    }

    public class UnaryOperatorNode : AstNodeBase, IUnaryOperatorNode
    {
        public UnaryOperatorNode(IToken token) : base(AstNodeTypes.UnaryOperator, token)
        {
            var op = token.Image switch
            {
                "+" => UnaryOperators.Plus,
                "-" => UnaryOperators.Minus,
                _ => throw new ArgumentOutOfRangeException()
            };

            Operator = op;
            TypeName = "unknown";
        }

        public UnaryOperators Operator { get; }

        public IAstNode Operand { get; set; }

        public string TypeName { get; set; }

        public override string ToString()
        {
            var op = Operator switch
            {
                UnaryOperators.Plus => "+",
                UnaryOperators.Minus => "-",
                _ => null
            };

            return $"{op}{Operand.ToString()}";
        }
    }

    public class BinaryOperatorNode : AstNodeBase, IBinaryOperatorNode
    {
        public BinaryOperatorNode(IToken token) : base(AstNodeTypes.BinaryOperator, token)
        {
            var op = BinaryOperators.Add;
            switch (token.Image)
            {
                case "+":
                    op = BinaryOperators.Add;
                    break;
                case "-":
                    op = BinaryOperators.Subtract;
                    break;
                case "*":
                    op = BinaryOperators.Multiply;
                    break;
                case "/":
                    op = BinaryOperators.Divide;
                    break;
            }

            Operator = op;
            TypeName = "unknown";
        }

        public BinaryOperators Operator { get; }

        public IAstNode LeftOperand { get; set; }

        public IAstNode RightOperand { get; set; }

        public string TypeName { get; set; }

        public override string ToString()
        {
            var left = LeftOperand.ToString();
            var op = Operator switch
            {
                BinaryOperators.Add => "+",
                BinaryOperators.Subtract => "-",
                BinaryOperators.Multiply => "*",
                BinaryOperators.Divide => "/",
                _ => throw new ArgumentOutOfRangeException()
            };
            var right = RightOperand.ToString();

            return $"({left} {op} {right})";
        }
    }

    public class NumberLiteralNode : AstNodeBase, INumberLiteralNode
    {
        public NumberLiteralNode(IToken token) : base(AstNodeTypes.NumberLiteral, token)
        {
            float.TryParse(token.Image, out var value);
            Value = value;
        }

        public float Value { get; }

        public string TypeName { get => "number"; set { } }

        public override string ToString()
        {
            return Value.ToString(CultureInfo.InvariantCulture);
        }
    }

    public class StringLiteralNode : AstNodeBase, IStringLiteralNode
    {
        public StringLiteralNode(IToken token) : base(AstNodeTypes.StringLiteral, token)
        {
            Value = token.Image;
        }

        public string Value { get; }

        public string TypeName { get => "string"; set { } }

        public override string ToString()
        {
            return $"\"{Value}\"";
        }
    }

    public class VariableNode : AstNodeBase, IVariableNode
    {
        public VariableNode(IToken token) : base(AstNodeTypes.VarReference, token)
        {
            Name = token.Image;
            TypeName = "unknown";
        }

        public string Name { get; }

        public string TypeName { get; set; }

        public override string ToString()
        {
            return Name;
        }
    }
}
