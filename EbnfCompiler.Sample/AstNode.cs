namespace EbnfCompiler.Sample
{
   public class AstNodeBase : IAstNode
   {
      public AstNodeBase(AstNodeTypes nodeType, IToken token)
      {
         AstNodeTypes = nodeType;
         Location = token.Location;
      }

      public AstNodeTypes AstNodeTypes { get; }

      public ISourceLocation Location { get; set; }
   }

   public class VarStatementNode : AstNodeBase, IVarStatementNode
   {
      public VarStatementNode(IToken token) : base(AstNodeTypes.VarStatement, token)
      {
      }

      public IVariableNode Variable { get; set; }

      public ITypeNode Type { get; set; }

      public IAstNode Expression { get; set; }
   }

   public class UnaryOperatorNode : AstNodeBase, IUnaryOperatorNode
   {
      public UnaryOperatorNode(IToken token, UnaryOperators op) : base(AstNodeTypes.UnaryOperator, token)
      {
         Operator = op;
      }

      public UnaryOperators Operator { get; }

      public IAstNode Operand { get; set; }
   }

   public class BinaryOperatorNode : AstNodeBase, IBinaryOperatorNode
   {
      public BinaryOperatorNode(IToken token, BinaryOperators op) : base(AstNodeTypes.BinaryOperator, token)
      {
         Operator = op;
      }

      public BinaryOperators Operator { get; }

      public IAstNode LeftOperand { get; set; }

      public IAstNode RightOperand { get; set; }
   }

   public class NumberLiteralNode : AstNodeBase, INumberLiteralNode
   {
      public NumberLiteralNode(IToken token): base(AstNodeTypes.NumberLiteral, token)
      {
         float.TryParse(token.Image, out var value);
         Value = value;
      }

      public float Value { get; }
   }
   
   public class StringLiteralNode : AstNodeBase, IStringLiteralNode
   {
      public StringLiteralNode(IToken token, string value) : base(AstNodeTypes.StringLiteral, token)
      {
         Value = value;
      }

      public string Value { get; }
   }

   public class VariableNode : AstNodeBase, IVariableNode
   {
      public VariableNode(IToken token) : base(AstNodeTypes.Variable, token)
      {
         Name = token.Image;
      }

      public string Name { get; }
   }

   public class TypeNode : AstNodeBase, ITypeNode
   {
      public TypeNode(IToken token) : base(AstNodeTypes.Type, token)
      {
         Name = token.Image;
      }

      public string Name { get; }
   }
}
