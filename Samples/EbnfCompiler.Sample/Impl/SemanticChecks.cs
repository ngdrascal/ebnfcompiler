﻿using System.Collections.Generic;

namespace EbnfCompiler.Sample.Impl
{
   public class SemanticChecks : ISemanticChecks
   {
      private readonly List<string> _symbolTable = new List<string>();

      public void Check(IRootNode rootNode)
      {
         foreach (var stmtNode in rootNode.Statements)
         {
            if (stmtNode.AstNodeType == AstNodeTypes.VarStatement)
               Check(stmtNode.AsVarStatement());
            else if(stmtNode.AstNodeType == AstNodeTypes.PrintStatement)
               Check(stmtNode.AsPrintStatement());
         }
      }

      private void Check(IVarStatementNode varStmtNode)
      {
         // verify the identifier doesn't already exist
         var varName = varStmtNode.Variable.Name;

         if (_symbolTable.Exists(p => p == varName))
            throw new SemanticErrorException($"Variable \"{varName}\" already declared.", 
               varStmtNode.Variable);

         _symbolTable.Add(varName);

         // verify the all the sub-expressions 
         CalculateTypeForNodes(varStmtNode.Expression);
         CheckNodeType(varStmtNode.Expression);

         // verify the declared type and the expression type match
         if (varStmtNode.Variable.TypeName != varStmtNode.Expression.AsNodeWithType().TypeName)
            throw new SemanticErrorException("Type mismatch.", varStmtNode.Variable);
      }

      private void Check(IPrintStatementNode printStmtNode)
      {
         foreach (var expr in printStmtNode.Expressions)
         {
            CalculateTypeForNodes(expr);
            CheckNodeType(expr);
         }
      }

      private void CalculateTypeForNodes(IAstNode exprNode)
      {
         if (exprNode.AstNodeType == AstNodeTypes.BinaryOperator)
         {
            var binOpNode = exprNode.AsBinaryOp();

            CalculateTypeForNodes(binOpNode.LeftOperand);
            binOpNode.TypeName = binOpNode.LeftOperand.AsNodeWithType().TypeName;
            CalculateTypeForNodes(binOpNode.RightOperand);
         }
         else if (exprNode.AstNodeType == AstNodeTypes.UnaryOperator)
         {
            CalculateTypeForNodes(exprNode.AsUnaryOp().Operand);
         }
      }

      private void CheckNodeType(IAstNode exprNode)
      {
         if (exprNode.AstNodeType == AstNodeTypes.BinaryOperator)
         {
            var binOpNode = exprNode.AsBinaryOp();

            CheckNodeType(binOpNode.LeftOperand);
            CheckNodeType(binOpNode.RightOperand);

            if (binOpNode.LeftOperand.AsNodeWithType().TypeName !=
                binOpNode.RightOperand.AsNodeWithType().TypeName)
            {
               throw new SemanticErrorException("Type mismatch.", binOpNode);
            }
         }
         else if (exprNode.AstNodeType == AstNodeTypes.UnaryOperator)
         {
            var unaryOpNode = exprNode.AsUnaryOp();

            CheckNodeType(unaryOpNode.Operand);
            if (unaryOpNode.TypeName != unaryOpNode.Operand.AsNodeWithType().TypeName)
               throw new SemanticErrorException("Type mismatch.", unaryOpNode);
         }
      }
   }
}