using System;
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
      [TestCase(typeof(SyntaxNode))]
      [TestCase(typeof(StatementNode))]
      [TestCase(typeof(ExpressionNode))]
      [TestCase(typeof(TermNode))]
      [TestCase(typeof(FactorNode))]
      [TestCase(typeof(ProdRefNode))]
      [TestCase(typeof(TerminalNode))]
      [TestCase(typeof(ActionNode))]
      [TestCase(typeof(ParenNode))]
      [TestCase(typeof(OptionNode))]
      [TestCase(typeof(KleeneNode))]
      public void AstTraverser_GivenANodeType_CallsDelegates(Type nodeType)
      {
         // Arrange:
         IAstNode actualNode = null;
         var postProcessWasCalled = false;
         var tracer = new Mock<IDebugTracer>().Object;
         var expectedNode = (IAstNode)Activator.CreateInstance(nodeType, new Token(), tracer);

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

      [Test]
      public void Test()
      {
         using var sb = new SyntaxBuilder();
         sb.Syntax(
            sb.Statement("<S>",
               sb.Expression(
                  sb.Term(
                     sb.Factor(sb.Terminal("a")),
                     sb.Factor(sb.Terminal("b")),
                     sb.Factor(sb.ProdRef("<T>"))),
                  sb.Term(
                     sb.Factor(sb.Terminal("c"))))));

         var output = sb.Build();
      }
   }
}
