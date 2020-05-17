using System.Collections.Generic;

namespace EbnfCompiler.Sample
{
   public enum UnaryOperators { Plus, Minus };

   public enum BinaryOperators { Add, Subtract, Multiply, Divide };

   public enum AstNodeTypes
   {
      VarStatement, PrintStatement,
      UnaryOperator, BinaryOperator, NumberLiteral, StringLiteral, Variable, //Type
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
      IReadOnlyCollection<IAstNode> Expressions { get; }
      void AppendExpression(IAstNode expression);
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
