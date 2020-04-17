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
         var token = new Token { Image = actualImage };

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
         var token = new Token { Image = actualImage };
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
         var token = new Token { Image = actualDefinition };

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
         var token = new Token { Image = "<S>" };
         _nodeFactoryMock.Setup(m => m.Create(It.IsAny<AstNodeType>(), 
                                                                        It.IsAny<IToken>()))
                         .Returns(new StatementNode(token, _tracerMock.Object));

         _prodInfoFactoryMock.Setup(m=>m.Create(It.IsAny<string>()));

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
         var token = new Token { Image = "<S>" };
         _nodeFactoryMock.Setup(m => m.Create(It.IsAny<AstNodeType>(),
               It.IsAny<IToken>()))
            .Returns(new StatementNode(token, _tracerMock.Object));

         _prodInfoFactoryMock.Setup(m => m.Create(It.IsAny<string>()))
                             .Returns(new ProductionInfo(token.Image, _tracerMock.Object));

         var stack = new Stack<IAstNode>();
         var statement = new StatementNode(token, _tracerMock.Object)
         {
            Expression = new ExpressionNode(new Token {Image = "\"a\""}, _tracerMock.Object)
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

      [Test, Ignore("For use as template")]
      public void XYZ_WhenGivenToken_AddToTokenDefinition()
      {
         // Arrange:
         var nodeFactory = new AstNodeFactory(_tracerMock.Object);
         var prodFactory = new ProdInfoFactory(_tracerMock.Object);
         var stack = new Stack<IAstNode>();
         var builder = new AstBuilder(nodeFactory, prodFactory,  stack, _tracerMock.Object);
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
