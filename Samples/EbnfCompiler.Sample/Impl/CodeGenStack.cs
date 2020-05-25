using Mono.Cecil.Cil;

namespace EbnfCompiler.Sample.Impl
{
   internal class StackElementBase
   {
      protected StackElementBase(AstNodeTypes nodeType)
      {
         NodeType = nodeType;
      }

      public AstNodeTypes NodeType { get; }
   }

   internal class VarStmtElement : StackElementBase
   {
      public VarStmtElement() : base(AstNodeTypes.VarStatement)
      {
      }

      public VariableDefinition VariableDef { get; set; }
   }

   internal class PrintStmtElement : StackElementBase
   {
      public PrintStmtElement() : base(AstNodeTypes.PrintStatement)
      {
      }

      public VariableDefinition VariableDef { get; set; }
   }

   internal class PrintExprElement : StackElementBase
   {
      public PrintExprElement() : base(AstNodeTypes.PrintExpression)
      {
      }
   }

   internal class UnaryOperatorElement : StackElementBase
   {
      public UnaryOperatorElement() : base(AstNodeTypes.UnaryOperator)
      {
      }

      public UnaryOperators Operator { get; set; }
   }

   internal class BinaryOperatorElement : StackElementBase
   {
      public BinaryOperatorElement() : base(AstNodeTypes.BinaryOperator)
      {
      }

      public BinaryOperators Operator { get; set; }

      public string TypeName { get; set; }
   }

   internal class NumberLiteralElement : StackElementBase
   {
      public NumberLiteralElement() : base(AstNodeTypes.NumberLiteral)
      {
      }

      public float Value { get; set; }
   }

   internal class StringLiteralElement : StackElementBase
   {
      public StringLiteralElement() : base(AstNodeTypes.StringLiteral)
      {
      }

      public string Value { get; set; }
   }

   internal class VarRefenceElement : StackElementBase
   {
      public VarRefenceElement() : base(AstNodeTypes.VarReference)
      {
      }

      public VariableDefinition VariableDef { get; set; }

      public string TypeName { get; set; }
   }
}
