using System.Collections.Generic;

namespace EbnfCompiler.Sample
{
   public interface IAstBuilder
   {
      IRootNode RootNode { get; }

      void StmtEnd(IToken token);

      void VarStmtStart(IToken token);
      void VarStmtIdent(IToken token);
      void VarStmtType(IToken token);
      void VarStmtEnd(IToken token);

      void UnaryOp(IToken token);
      void UnaryOpEnd(IToken token);

      void BinaryOp(IToken token);
      void BinaryOpEnd(IToken token);

      void NumLiteral(IToken token);
      void StrLiteral(IToken token);

      void FactIdent(IToken token);
   }

   public class AstBuilder : IAstBuilder
   {
      private readonly Stack<IAstNode> _stack = new Stack<IAstNode>();

      public IRootNode RootNode { get; } = new RootNode();

      public void StmtEnd(IToken token)
      {
         var stmt = _stack.Pop();
         RootNode.AppendStatement(stmt);
      }

      public void VarStmtStart(IToken token)
      {
         var varStmtNode = new VarStatementNode(token);
         _stack.Push(varStmtNode);
      }

      public void VarStmtIdent(IToken token)
      {
         var varNode = new VariableNode(token);
         _stack.Peek().AsVarStatement().Variable = varNode;
      }

      public void VarStmtType(IToken token)
      {
         var typeNode = new TypeNode(token);
         _stack.Peek().AsVarStatement().Type = typeNode;
      }

      public void VarStmtEnd(IToken token)
      {
         var expr = _stack.Pop();
         _stack.Peek().AsVarStatement().Expression = expr;
      }

      public void UnaryOp(IToken token)
      {
         var opNode = new UnaryOperatorNode(token);
         _stack.Push(opNode);
      }

      public void UnaryOpEnd(IToken token)
      {
         var operand = _stack.Pop();
         if (_stack.Peek().AstNodeTypes == AstNodeTypes.UnaryOperator)
            _stack.Peek().AsUnaryOp().Operand = operand;
         else
            _stack.Push(operand);
      }

      public void BinaryOp(IToken token)
      {
         var left = _stack.Pop();
         var opNode = new BinaryOperatorNode(token) { LeftOperand = left };
         _stack.Push(opNode);
      }

      public void BinaryOpEnd(IToken token)
      {
         var right = _stack.Pop();
         _stack.Peek().AsBinaryOp().RightOperand = right;
      }

      public void NumLiteral(IToken token)
      {
         var numLitNode = new NumberLiteralNode(token);
         _stack.Push(numLitNode);
      }

      public void StrLiteral(IToken token)
      {
        var strLitNode = new StringLiteralNode(token);
        _stack.Push(strLitNode);
      }

      public void FactIdent(IToken token)
      {
         var varNode = new VariableNode(token);
         _stack.Push(varNode);
      }
   }
}
