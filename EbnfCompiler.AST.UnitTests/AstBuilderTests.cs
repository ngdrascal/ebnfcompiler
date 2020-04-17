using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using EbnfCompiler.AST.Impl;
using EbnfCompiler.Compiler;
using Moq;
using NUnit.Framework;

namespace EbnfCompiler.AST.UnitTests
{
   [TestFixture, ExcludeFromCodeCoverage]
   public class AstBuilderTests
   {
      private Mock<IDebugTracer> _tracerMock;
      private Mock<IProdInfoFactory> _prodInfoFactoryMock;
      private Mock<IAstNodeFactory> _nodeFactoryMock;

      [SetUp]
      public void SetUp()
      {
         _tracerMock = new Mock<IDebugTracer>();
         _prodInfoFactoryMock = new Mock<IProdInfoFactory>();
         _nodeFactoryMock = new Mock<IAstNodeFactory>();
      }

      [Test]
      public void AddTokenName_WhenFirstOccurrencesOfToken_AddToTokenDefinition()
      {
         // Arrange:
         var builder = new AstBuilder(null, null, null, _tracerMock.Object);
         const string actualImage = "a";
         var token = new Token { TokenKind = TokenKind.String, Image = actualImage };

         // Act:
         builder.AddTokenName(token);

         // Assert:
         Assert.That(builder.TokenDefinitions.Count, Is.EqualTo(1));
         Assert.That(builder.TokenDefinitions.First().Image, Is.EqualTo(actualImage));
      }

      [Test]
      public void AddTokenName_WhenNotFirstOccurrencesOfToken_ThrowsSemanticErrorException()
      {
         // Arrange:
         var builder = new AstBuilder(null, null, null, _tracerMock.Object);
         const string actualImage = "a";
         var token = new Token { TokenKind = TokenKind.String, Image = actualImage };
         builder.AddTokenName(token);

         // Act:
         void AddTokenName() => builder.AddTokenName(token);

         // Assert:
         Assert.Throws<SemanticErrorException>(AddTokenName);
      }

      [Test]
      public void SetTokenDefinition_GivenAToken_AddToTokenDefinition()
      {
         // Arrange:
         var builder = new AstBuilder(null, null, null, _tracerMock.Object);
         const string actualDefinition = "tkA";
         var token = new Token { TokenKind = TokenKind.String, Image = actualDefinition };

         // Act:
         builder.AddTokenName(token);
         builder.SetTokenDef(token);

         // Assert:
         Assert.That(builder.TokenDefinitions.Count, Is.EqualTo(1));
         Assert.That(builder.TokenDefinitions.First().Definition, Is.EqualTo(actualDefinition));
      }

      [Test]
      public void BeginStatement_WhenGivenToken_PushesStatementNode()
      {
         // Arrange:
         var token = new Token { TokenKind = TokenKind.String, Image = "<S>" };
         _nodeFactoryMock.Setup(m => m.Create(It.IsAny<AstNodeType>(),
                                                                        It.IsAny<IToken>()))
                         .Returns(new StatementNode(token, _tracerMock.Object));

         _prodInfoFactoryMock.Setup(m => m.Create(It.IsAny<string>()));

         var stack = new Stack<IAstNode>();

         var builder = new AstBuilder(_nodeFactoryMock.Object, _prodInfoFactoryMock.Object, stack, _tracerMock.Object);

         // Act:
         builder.BeginStatement(token);

         // Assert:
         Assert.That(stack.Count, Is.EqualTo(1));
         Assert.That(stack.Peek(), Is.InstanceOf(typeof(IStatementNode)));
         Assert.That((stack.Peek() as IStatementNode)?.ProdName, Is.EqualTo(token.Image));
      }

      [Test]
      public void EndStatement_WhenGivenToken_SetsProductionsRhs()
      {
         // Arrange:
         var token = new Token { TokenKind = TokenKind.String, Image = "<S>" };
         _nodeFactoryMock.Setup(m => m.Create(It.IsAny<AstNodeType>(),
               It.IsAny<IToken>()))
            .Returns(new StatementNode(token, _tracerMock.Object));

         _prodInfoFactoryMock.Setup(m => m.Create(It.IsAny<string>()))
                             .Returns(new ProductionInfo(token.Image, _tracerMock.Object));

         var stack = new Stack<IAstNode>();
         var statement = new StatementNode(token, _tracerMock.Object)
         {
            Expression = new ExpressionNode(new Token { Image = "\"a\"" }, _tracerMock.Object)
         };
         stack.Push(statement);

         var builder = new AstBuilder(_nodeFactoryMock.Object, _prodInfoFactoryMock.Object, stack, _tracerMock.Object);

         // Act:
         builder.EndStatement();

         // Assert:
         Assert.That(stack.Count, Is.EqualTo(0));
         Assert.That(builder.Productions.First().Value.Name, Is.EqualTo(token.Image));
         Assert.That(builder.Productions.First().Value.RightHandSide, Is.InstanceOf(typeof(IExpressionNode)));
      }

      [Test]
      public void BeginExpression_WhenGivenAToken_PushesExpressionNode()
      {
         // Arrange:
         var token = new Token { TokenKind = TokenKind.String, Image = "<T>" };
         _nodeFactoryMock.Setup(m => m.Create(It.IsAny<AstNodeType>(),
                                                                        It.IsAny<IToken>()))
            .Returns(new ExpressionNode(token, _tracerMock.Object));

         _prodInfoFactoryMock.Setup(m => m.Create(It.IsAny<string>()));

         var stack = new Stack<IAstNode>();

         var builder = new AstBuilder(_nodeFactoryMock.Object, _prodInfoFactoryMock.Object, stack, _tracerMock.Object);

         // Act:
         builder.BeginExpression(token);

         // Assert:
         Assert.That(stack.Count, Is.EqualTo(1));
         Assert.That(stack.Peek(), Is.InstanceOf(typeof(IExpressionNode)));
         Assert.That((stack.Peek() as IExpressionNode)?.Image, Is.EqualTo(string.Empty));
         Assert.That((stack.Peek() as IExpressionNode)?.FirstTerm, Is.Null);
      }

      [Test]
      public void EndExpression_WhenInsideStatement_SetsExpression()
      {
         // Arrange:
         var stack = new Stack<IAstNode>();

         var stmtToken = new Token { TokenKind = TokenKind.String, Image = "<S>" };
         var stmt = new StatementNode(stmtToken, _tracerMock.Object);
         stack.Push(stmt);

         var exprToken = new Token { TokenKind = TokenKind.String, Image = "<T>" };
         var expr = new ExpressionNode(exprToken, _tracerMock.Object);
         stack.Push(expr);

         var builder = new AstBuilder(null, null, stack, _tracerMock.Object);

         // Act:
         builder.EndExpression();

         // Assert:
         Assert.That(stack.Count, Is.EqualTo(1));
         Assert.That(stack.Peek(), Is.InstanceOf(typeof(IStatementNode)));
         Assert.That((stack.Peek() as IStatementNode)?.Expression, Is.Not.Null);
      }

      [Test]
      public void EndExpression_WhenInsideParen_SetsExpression()
      {
         // Arrange:
         var stack = new Stack<IAstNode>();

         var parenToken = new Token { TokenKind = TokenKind.LeftParen, Image = "(" };
         var paren = new ParenNode(parenToken, _tracerMock.Object);
         stack.Push(paren);

         var exprToken = new Token { TokenKind = TokenKind.String, Image = "<T>" };
         var expr = new ExpressionNode(exprToken, _tracerMock.Object);
         stack.Push(expr);

         var builder = new AstBuilder(null, null, stack, _tracerMock.Object);

         // Act:
         builder.EndExpression();

         // Assert:
         Assert.That(stack.Count, Is.EqualTo(1));
         Assert.That(stack.Peek(), Is.InstanceOf(typeof(IParenNode)));
         Assert.That((stack.Peek() as IParenNode)?.Expression, Is.Not.Null);
      }

      [Test]
      public void EndExpression_WhenInsideOption_SetsExpression()
      {
         // Arrange:
         var stack = new Stack<IAstNode>();

         var optionToken = new Token { TokenKind = TokenKind.LeftBracket, Image = "[" };
         var option = new OptionNode(optionToken, _tracerMock.Object);
         stack.Push(option);

         var exprToken = new Token { TokenKind = TokenKind.String, Image = "<T>" };
         var expr = new ExpressionNode(exprToken, _tracerMock.Object);
         stack.Push(expr);

         var builder = new AstBuilder(null, null, stack, _tracerMock.Object);

         // Act:
         builder.EndExpression();

         // Assert:
         Assert.That(stack.Count, Is.EqualTo(1));
         Assert.That(stack.Peek(), Is.InstanceOf(typeof(IOptionNode)));
         Assert.That((stack.Peek() as IOptionNode)?.Expression, Is.Not.Null);
      }

      [Test]
      public void EndExpression_WhenInsideKleene_SetsExpression()
      {
         // Arrange:
         var stack = new Stack<IAstNode>();

         var kleeneToken = new Token { TokenKind = TokenKind.LeftBrace, Image = "{" };
         var kleene = new KleeneNode(kleeneToken, _tracerMock.Object);
         stack.Push(kleene);

         var exprToken = new Token { TokenKind = TokenKind.String, Image = "<T>" };
         var expr = new ExpressionNode(exprToken, _tracerMock.Object);
         stack.Push(expr);

         var builder = new AstBuilder(null, null, stack, _tracerMock.Object);

         // Act:
         builder.EndExpression();

         // Assert:
         Assert.That(stack.Count, Is.EqualTo(1));
         Assert.That(stack.Peek(), Is.InstanceOf(typeof(IKleeneStarNode)));
         Assert.That((stack.Peek() as IKleeneStarNode)?.Expression, Is.Not.Null);
      }
      [Test, Ignore("For use as template")]
      public void XYZ_WhenGivenToken_AddToTokenDefinition()
      {
         // Arrange:
         var nodeFactory = new AstNodeFactory(_tracerMock.Object);
         var prodFactory = new ProdInfoFactory(_tracerMock.Object);
         var stack = new Stack<IAstNode>();
         var builder = new AstBuilder(nodeFactory, prodFactory, stack, _tracerMock.Object);
         const string actualImage = "a";
         var token = new Token { Image = actualImage };

         // Act:
         builder.AddTokenName(token);

         // Assert:
         Assert.That(builder.TokenDefinitions.Count, Is.EqualTo(1));
         Assert.That(builder.TokenDefinitions.First().Image, Is.EqualTo(actualImage));
      }
   }
}
