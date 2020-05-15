using System.Collections.Generic;

namespace EbnfCompiler.Sample
{
   public interface IAstBuilder
   {
      void VarStmtStart(IToken token);
      void VarStmtIdent(IToken token);
      void VarStmtType(IToken token);
      void VarStmtEnd(IToken token);

      void ExprStart(IToken token);
      void ExprEnd(IToken token);

      void TermStart(IToken token);
      void TermEnd(IToken token);
      
      void FactStart(IToken token);
      void FactEnd(IToken token);

      void BinaryOp(IToken token);

      void NumLiteral(IToken token);
   }

   public class AstBuilder : IAstBuilder
   {
      private readonly Stack<IAstNode> _stack = new Stack<IAstNode>();

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

      public void ExprStart(IToken token)
      {
      }

      public void ExprEnd(IToken token)
      {
      }

      public void TermStart(IToken token)
      {
      }

      public void TermEnd(IToken token)
      {
      }

      public void FactStart(IToken token)
      {
      }

      public void FactEnd(IToken token)
      {
      }

      public void BinaryOp(IToken token)
      {
      }

      public void NumLiteral(IToken token)
      {
         var numLitNode = new NumberLiteralNode(token);
         _stack.Push(numLitNode);
      }
   }
}
