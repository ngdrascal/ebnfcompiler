using System.Diagnostics.CodeAnalysis;
using EbnfCompiler.AST.Impl;
using EbnfCompiler.Compiler;
using Moq;
using NUnit.Framework;

namespace EbnfCompiler.AST.UnitTests
{
   [TestFixture, ExcludeFromCodeCoverage]
   public class NodeTests
   {
      private IDebugTracer _tracer;

      [SetUp]
      public void Setup()
      {
         _tracer = new Mock<IDebugTracer>().Object;
      }

      [Test]
      public void SyntaxNode_WhenToStringCalled_ReturnsCorrectString()
      {
         // Arranged:
         var stmtNodeMock = new Mock<IStatementNode>();
         stmtNodeMock.Setup(node=> node.ToString()).Returns(()=> "<S> ::= \"a\" .");
         var syntaxNode = new SyntaxNode(new Token(), _tracer);
         syntaxNode.AppendStatement(stmtNodeMock.Object);

         // Act:
         var actual = syntaxNode.ToString();

         // Assert:
         Assert.That(actual, Is.EqualTo("<S> ::= \"a\" .\r\n"));
      }

      [Test]
      public void StatementNode_WhenToStringCalled_ReturnsCorrectString()
      {
         // Arranged:
         var exprNodeMock = new Mock<IExpressionNode>();
         exprNodeMock.Setup(node => node.ToString()).Returns(() => "\"a\"");
         var stmtNode = new StatementNode(new Token(TokenKind.Identifier, "<S>"), _tracer)
         {
            Expression = exprNodeMock.Object
         };

         // Act:
         var actual = stmtNode.ToString();

         // Assert:
         Assert.That(actual, Is.EqualTo("<S> ::= \"a\" ."));
      }

      [Test]
      public void ExpressionNode_WhenToStringCalled_ReturnsCorrectString()
      {
         // Arranged:
         var termNodeMock1 = new Mock<ITermNode>();
         termNodeMock1.Setup(node => node.ToString()).Returns(() => "<U>");
         var termNodeMock2 = new Mock<ITermNode>();
         termNodeMock2.Setup(node => node.ToString()).Returns(() => "<T>");
         var exprNode = new ExpressionNode(new Token(), _tracer);
         exprNode.AppendTerm(termNodeMock1.Object);
         exprNode.AppendTerm(termNodeMock2.Object);

         // Act:
         var actual = exprNode.ToString();

         // Assert:
         Assert.That(actual, Is.EqualTo("<U> | <T>"));
      }

      [Test]
      public void TermNode_WhenToStringCalled_ReturnsCorrectString()
      {
         // Arranged:
         var factNodeMock1 = new Mock<IFactorNode>();
         factNodeMock1.Setup(node => node.ToString()).Returns(() => "<T>");
         var factNodeMock2 = new Mock<IFactorNode>();
         factNodeMock2.Setup(node => node.ToString()).Returns(() => "<U>");
         var termNode = new TermNode(new Token(TokenKind.String, "\"a\""), _tracer);
         termNode.AppendFactor(factNodeMock1.Object);
         termNode.AppendFactor(factNodeMock2.Object);

         // Act:
         var actual = termNode.ToString();

         // Assert:
         Assert.That(actual, Is.EqualTo("<T> <U>"));
      }
      
      [Test]
      public void FactorNode_WithIdentifier_ReturnsCorrectString()
      {
         // Arranged:
         var prodRefNode = new Mock<IProdRefNode>();
         prodRefNode.Setup(terminalNode => terminalNode.AstNodeType).Returns(AstNodeType.ProdRef);
         prodRefNode.Setup(terminalNode => terminalNode.ToString()).Returns(() => "<T>");

         var factNode = new FactorNode(new Token(TokenKind.Identifier, "T"), _tracer) { FactorExpr = prodRefNode.Object };

         // Act:
         var actual = factNode.ToString();

         // Assert:
         Assert.That(actual, Is.EqualTo("<T>"));
      }

      [Test]
      public void FactorNode_WithTerminal_ReturnsCorrectString()
      {
         // Arranged:
         var terminalNodeMock = new Mock<ITerminalNode>();
         terminalNodeMock.Setup(node => node.AstNodeType).Returns(AstNodeType.Expression);
         terminalNodeMock.Setup(node => node.ToString()).Returns(() => "\"a\"");
         
         var factNode = new FactorNode(new Token(TokenKind.String, "a"), _tracer) { FactorExpr = terminalNodeMock.Object };

         // Act:
         var actual = factNode.ToString();

         // Assert:
         Assert.That(actual, Is.EqualTo("\"a\""));
      }

      [Test]
      public void FactorNode_WithParens_ReturnsCorrectString()
      {
         // Arranged:
         var parenNodeMock = new Mock<IParenNode>();
         parenNodeMock.Setup(node => node.AstNodeType).Returns(AstNodeType.Expression);
         parenNodeMock.Setup(node => node.ToString()).Returns(() => "( <U> )");

         var factNode = new FactorNode(new Token(), _tracer);
         factNode.FactorExpr = parenNodeMock.Object;

         // Act:
         var actual = factNode.ToString();

         // Assert:
         Assert.That(actual, Is.EqualTo("( <U> )"));
      }

      [Test]
      public void ProdRefNode_WithIdentifier_ReturnsCorrectString()
      {
         // Arranged:
         var node = new ProdRefNode(new Token(TokenKind.Identifier, "<T>"), _tracer);

         // Act:
         var actual = node.ToString();

         // Assert:
         Assert.That(actual, Is.EqualTo("<T>"));
      }

      [Test]
      public void TerminalNode_WithString_ReturnsCorrectString()
      {
         // Arranged:
         var node = new TerminalNode(new Token(TokenKind.String, "a"), _tracer);

         // Act:
         var actual = node.ToString();

         // Assert:
         Assert.That(actual, Is.EqualTo("\"a\""));
      }

      [Test]
      public void ParenNode_WithExpression_ReturnsCorrectString()
      {
         // Arranged:
         var exprNodeMock = new Mock<IExpressionNode>();
         exprNodeMock.Setup(exprNode => exprNode.AstNodeType).Returns(AstNodeType.Expression);
         exprNodeMock.Setup(exprNode => exprNode.ToString()).Returns(() => "<T>");

         var node = new ParenNode(new Token(TokenKind.LeftParen, "("), _tracer) {Expression = exprNodeMock.Object};

         // Act:
         var actual = node.ToString();

         // Assert:
         Assert.That(actual, Is.EqualTo("( <T> )"));
      }

      [Test]
      public void OptionNode_WithExpression_ReturnsCorrectString()
      {
         // Arranged:
         var exprNodeMock = new Mock<IExpressionNode>();
         exprNodeMock.Setup(exprNode => exprNode.AstNodeType).Returns(AstNodeType.Expression);
         exprNodeMock.Setup(exprNode => exprNode.ToString()).Returns(() => "<T>");

         var node = new OptionNode(new Token(TokenKind.LeftBracket, "["), _tracer) {Expression = exprNodeMock.Object};

         // Act:
         var actual = node.ToString();

         // Assert:
         Assert.That(actual, Is.EqualTo("[ <T> ]"));
      }


      [Test]
      public void KleeneNode_WithExpression_ReturnsCorrectString()
      {
         // Arranged:
         var exprNodeMock = new Mock<IExpressionNode>();
         exprNodeMock.Setup(exprNode => exprNode.AstNodeType).Returns(AstNodeType.Expression);
         exprNodeMock.Setup(exprNode => exprNode.ToString()).Returns(() => "<T>");

         var node = new KleeneNode(new Token(TokenKind.LeftBrace, "("), _tracer) {Expression = exprNodeMock.Object};

         // Act:
         var actual = node.ToString();

         // Assert:
         Assert.That(actual, Is.EqualTo("{ <T> }"));
      }

      [Test]
      public void ActionNode__ReturnsCorrectString()
      {
         // Arranged:
         var node = new ActionNode(new Token(TokenKind.Action, "#Action#"), _tracer);

         // Act:
         var actual = node.ToString();

         // Assert:
         Assert.That(actual, Is.EqualTo("#Action#"));
      }
   }
}
