using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using EbnfCompiler.AST.Impl;
using EbnfCompiler.Compiler;
using Moq;
using NUnit.Framework;

namespace EbnfCompiler.AST.UnitTests
{
   [TestFixture, ExcludeFromCodeCoverage]
   public class AstTraverserTests
   {
      [Test]
      public void AstTraverser_WhenSyntaxNode_CallsDelegates()
      {
         // Arrange:
         IAstNode actualNode = null;
         var postProcessWasCalled = false;
         var tracer = new Mock<IDebugTracer>().Object;
         var expectedNode = new SyntaxNode(new Token(TokenKind.Identifier, "<T>"), tracer);
         var traverser = new AstTraverser(tracer);
         traverser.ProcessNode += (node) => { actualNode = node; };
         traverser.PostProcessNode += () => { postProcessWasCalled = true; };

         // Act:
         traverser.Traverse(expectedNode);

         // Assert:
         Assert.That(actualNode, Is.SameAs(expectedNode));
         Assert.That(postProcessWasCalled, Is.True);
      }

      [Test]
      public void AstTraverser_WhenSyntaxNode_TraversesStatements()
      {
         // Arrange:
         var tally = new Dictionary<AstNodeType, int>();

         var tracer = new Mock<IDebugTracer>().Object;

         var exprNode = new ExpressionNode(new Token(TokenKind.String, "a"), tracer);
         var stmtNode = new StatementNode(new Token(TokenKind.Identifier, "<S>"), tracer);
         stmtNode.Expression = exprNode;
         var syntaxNode = new SyntaxNode(new Token(TokenKind.Identifier, "<T>"), tracer);
         syntaxNode.AppendStatement(stmtNode);

         var traverser = new AstTraverser(tracer);
         traverser.ProcessNode += (node) =>
         {
            if (!tally.ContainsKey(node.AstNodeType))
               tally.Add(node.AstNodeType, 0);

            tally[node.AstNodeType]++;
         };

         // Act:
         traverser.Traverse(syntaxNode);

         // Assert:
         Assert.That(tally[AstNodeType.Syntax], Is.EqualTo(1));
         Assert.That(tally[AstNodeType.Statement], Is.EqualTo(1));
         Assert.That(tally[AstNodeType.Expression], Is.EqualTo(1));
      }
   }
}
