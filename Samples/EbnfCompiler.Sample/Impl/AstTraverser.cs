using System;

namespace EbnfCompiler.Sample.Impl
{
    public class AstTraverser : IAstTraverser
    {
        public event Action<IAstNode> ProcessNode;

        public event Action PostProcessNode;

        public void Traverse(IAstNode astNode)
        {
            ProcessNode?.Invoke(astNode);

            switch (astNode.AstNodeType)
            {
                case AstNodeTypes.VarStatement:
                    Traverse(astNode.AsVarStatement().Variable);
                    Traverse(astNode.AsVarStatement().Expression);
                    break;

                case AstNodeTypes.PrintStatement:
                    foreach (var expr in astNode.AsPrintStatement().Expressions)
                        Traverse(expr);
                    break;

                case AstNodeTypes.UnaryOperator:
                    Traverse(astNode.AsUnaryOp().Operand);
                    break;

                case AstNodeTypes.BinaryOperator:
                    Traverse(astNode.AsBinaryOp().LeftOperand);
                    Traverse(astNode.AsBinaryOp().RightOperand);
                    break;

                case AstNodeTypes.NumberLiteral:
                    break;

                case AstNodeTypes.StringLiteral:
                    break;

                case AstNodeTypes.Variable:
                    break;
            }

            PostProcessNode?.Invoke();
        }
    }
}
