using System.Collections.Generic;

namespace EbnfCompiler.Sample
{
   public enum UnaryOperators { Plus, Minus };

   public enum BinaryOperators { Add, Subtract, Multiply, Divide };

   public enum AstNodeTypes
   {
      VarStatement, PrintStatement, PrintLineStatement,
      UnaryOperator, BinaryOperator, NumberLiteral, StringLiteral, Variable, Type
   };

   public interface IAstNode
   {
      AstNodeTypes AstNodeTypes { get; }

      ISourceLocation Location { get; }

      string ToString();
   }

   public interface IVarStatementNode : IAstNode
   {
      IVariableNode Variable { get; set; }
      ITypeNode Type { get; set; }
      IAstNode Expression { get; set; }
   }

   public interface IPrintStatementNode : IAstNode
   {
      IReadOnlyCollection<IAstNode> Expressions { get; }
      void AppendExpression(IAstNode expression);
   }

   public interface IUnaryOperatorNode : IAstNode
   {
      public UnaryOperators Operator { get; }

      public IAstNode Operand { get; set; }
   }

   public interface IBinaryOperatorNode : IAstNode
   {
      public BinaryOperators Operator { get; }

      public IAstNode LeftOperand { get; set; }

      public IAstNode RightOperand { get; set; }
   }

   public interface INumberLiteralNode : IAstNode
   {
      float Value { get; }
   }

   public interface IStringLiteralNode : IAstNode
   {
      string Value { get; }
   }

   public interface IVariableNode : IAstNode
   {
      string Name { get; }
   }

   public interface ITypeNode : IAstNode
   {
      string Name { get; }
   }
}
