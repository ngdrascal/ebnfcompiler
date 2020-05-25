using System;

namespace EbnfCompiler.Sample.Impl
{
    public class AstTraverser : IAstTraverser
    {
        public event Action<IAstNode> ProcessNode;

        public event Action<AstNodeTypes> PostProcessNode;

        public void Traverse(IAstNode astNode)
        {
            ProcessNode?.Invoke(astNode);

            switch (astNode.AstNodeType)
            {
                case AstNodeTypes.VarStatement:
                    Traverse(astNode.AsVarStatement().Expression);
                    break;

                case AstNodeTypes.PrintStatement:
                    foreach (var printExpr in astNode.AsPrintStatement().PrintExpressions)
                        Traverse(printExpr);
                    break;

                case AstNodeTypes.PrintExpression:
                    Traverse(astNode.AsPrintExpression().Expression);
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

                case AstNodeTypes.VarReference:
                    break;
            }

            PostProcessNode?.Invoke(astNode.AstNodeType);
        }
    }
}
