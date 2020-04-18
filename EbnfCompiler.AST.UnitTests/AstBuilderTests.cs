﻿using System.Collections.Generic;
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
      private readonly List<ProductionInfo> _allProdInfo = new List<ProductionInfo>();
      private Mock<IAstNodeFactory> _nodeFactoryMock;
      private readonly List<IAstNode> _allNodes = new List<IAstNode>();

      [SetUp]
      public void SetUp()
      {
         _tracerMock = new Mock<IDebugTracer>();
         _prodInfoFactoryMock = BuildProdInfoFactory();
         _nodeFactoryMock = BuildNodeFactory();
      }

      private Mock<IProdInfoFactory> BuildProdInfoFactory()
      {
         var mock = new Mock<IProdInfoFactory>();
         mock.Setup(factory => factory.Create(It.IsAny<string>()))
            .Returns((string name) =>
            {
               var prodInfo = new ProductionInfo(name, _tracerMock.Object);
               _allProdInfo.Add(prodInfo);
               return prodInfo;
            });

         mock.Setup(factory => factory.AllProductions)
            .Returns(_allProdInfo);

         return mock;
      }

      private Mock<IAstNodeFactory> BuildNodeFactory()
      {
         var mock = new Mock<IAstNodeFactory>();

         // Statement
         mock.Setup(factory =>
               factory.Create(It.Is<AstNodeType>(nodeType => nodeType == AstNodeType.Statement),
                  It.IsAny<IToken>()))
            .Returns(() =>
            {
               var node = new StatementNode(new Token { TokenKind = TokenKind.String, Image = "<S>" },
                  _tracerMock.Object);
               _allNodes.Add(node);
               return node;
            });

         // Expression
         mock.Setup(factory =>
               factory.Create(It.Is<AstNodeType>(nodeType => nodeType == AstNodeType.Expression),
                  It.IsAny<IToken>()))
            .Returns(() =>
            {
               var node = new ExpressionNode(new Token { TokenKind = TokenKind.String, Image = "<E>" },
                  _tracerMock.Object);
               _allNodes.Add(node);
               return node;
            });

         // Term
         mock.Setup(factory =>
               factory.Create(It.Is<AstNodeType>(nodeType => nodeType == AstNodeType.Term),
                  It.IsAny<IToken>()))
            .Returns(() =>
            {
               var node = new TermNode(new Token { TokenKind = TokenKind.String, Image = "<T>" }, _tracerMock.Object);
               _allNodes.Add(node);
               return node;
            });

         // Factor
         mock.Setup(factory =>
               factory.Create(It.Is<AstNodeType>(nodeType => nodeType == AstNodeType.Factor),
                  It.IsAny<IToken>()))
            .Returns(() =>
            {
               var node = new FactorNode(new Token { TokenKind = TokenKind.String, Image = "Factor" }, _tracerMock.Object);
               _allNodes.Add(node);
               return node;

            });

         mock.Setup(factory => factory.AllNodes).Returns(_allNodes);

         return mock;
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
      public void BeginSyntax_WhenGivenAToken_DoesNotPush()
      {
         // Arrange:
         var stack = new Stack<IAstNode>();

         var builder = new AstBuilder(null, null, stack, _tracerMock.Object);

         // Act:
         builder.BeginSyntax();

         // Assert:
         Assert.That(stack.Count, Is.EqualTo(0));
      }

      [Test]
      public void EndSyntax_WhenGivenAToken_FixupOnProdRef()
      {
         // Arrange:
         // _nodeFactoryMock.Setup(m => m.AllNodes)
         //    .Returns(new IAstNode[] { new ProdRefNode(new Token() { Image = "<S>" }, _tracerMock.Object) });

         var prodInfo = _prodInfoFactoryMock.Object.Create("<S>");
         var expr = _nodeFactoryMock.Object
            .Create(AstNodeType.Expression, new Token() { TokenKind = TokenKind.String, Image = "a" });
         prodInfo.RightHandSide = (IExpressionNode)expr;

         var stack = new Stack<IAstNode>();

         var builder = new AstBuilder(_nodeFactoryMock.Object, _prodInfoFactoryMock.Object,
                                      stack, _tracerMock.Object);

         // Act:
         builder.EndSyntax();

         // Assert:
         Assert.That(stack.Count, Is.EqualTo(0));

         foreach (var node in _nodeFactoryMock.Object.AllNodes.Where(p => p.AstNodeType == AstNodeType.ProdRef))
         {
            Assert.That(((IProdRefNode)node).Expression, Is.Not.Null);
         }
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
         Assert.That(builder.Productions.First().Name, Is.EqualTo(token.Image));
         Assert.That(builder.Productions.First().RightHandSide, Is.InstanceOf(typeof(IExpressionNode)));
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
         Assert.That((stack.Peek() as IExpressionNode)?.Image, Is.EqualTo(token.Image));
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

      [Test]
      public void BeginTerm_WhenGivenAToken_PushesTermNode()
      {
         // Arrange:
         var token = new Token { TokenKind = TokenKind.String, Image = "<T>" };
         _nodeFactoryMock.Setup(m => m.Create(It.IsAny<AstNodeType>(),
                                                                        It.IsAny<IToken>()))
                         .Returns(new TermNode(token, _tracerMock.Object));

         var stack = new Stack<IAstNode>();

         var builder = new AstBuilder(_nodeFactoryMock.Object, null, stack, _tracerMock.Object);

         // Act:
         builder.BeginTerm(token);

         // Assert:
         Assert.That(stack.Count, Is.EqualTo(1));
         Assert.That(stack.Peek(), Is.InstanceOf(typeof(ITermNode)));
         Assert.That((stack.Peek() as ITermNode)?.Image, Is.EqualTo(token.Image));
         Assert.That((stack.Peek() as ITermNode)?.FirstFactor, Is.Null);
      }

      [Test]
      public void EndTerm_WhenInsideExpression_AppendsTerm()
      {
         // Arrange:
         var stack = new Stack<IAstNode>();

         var exprToken = new Token { TokenKind = TokenKind.String, Image = "<E>" };
         var expr = new ExpressionNode(exprToken, _tracerMock.Object);
         stack.Push(expr);

         var termToken = new Token { TokenKind = TokenKind.String, Image = "<T>" };
         var term = new TermNode(termToken, _tracerMock.Object);
         stack.Push(term);

         var builder = new AstBuilder(null, null, stack, _tracerMock.Object);

         // Act:
         builder.EndTerm();

         // Assert:
         Assert.That(stack.Count, Is.EqualTo(1));
         Assert.That(stack.Peek(), Is.InstanceOf(typeof(IExpressionNode)));
         Assert.That((stack.Peek() as IExpressionNode)?.FirstTerm, Is.Not.Null);
      }

      [Test]
      public void BeginFactor_WhenGivenAToken_PushesFactorNode()
      {
         // Arrange:
         var token = new Token { TokenKind = TokenKind.String, Image = "<F>" };
         _nodeFactoryMock.Setup(m => m.Create(It.IsAny<AstNodeType>(),
                                                                        It.IsAny<IToken>()))
                         .Returns(new FactorNode(token, _tracerMock.Object));

         var stack = new Stack<IAstNode>();

         var builder = new AstBuilder(_nodeFactoryMock.Object, null, stack, _tracerMock.Object);

         // Act:
         builder.BeginFactor(token);

         // Assert:
         Assert.That(stack.Count, Is.EqualTo(1));
         Assert.That(stack.Peek(), Is.InstanceOf(typeof(IFactorNode)));
         Assert.That((stack.Peek() as IFactorNode)?.Image, Is.EqualTo(token.Image));
         Assert.That((stack.Peek() as IFactorNode)?.NextFactor, Is.Null);
      }

      [Test]
      public void EndFactor_WhenInsideTerm_AppendsFactor()
      {
         // Arrange:
         var stack = new Stack<IAstNode>();

         var termToken = new Token { TokenKind = TokenKind.String, Image = "<T>" };
         var term = new TermNode(termToken, _tracerMock.Object);
         stack.Push(term);

         var factorToken = new Token { TokenKind = TokenKind.String, Image = "<E>" };
         var factor = new FactorNode(factorToken, _tracerMock.Object);
         stack.Push(factor);


         var builder = new AstBuilder(null, null, stack, _tracerMock.Object);

         // Act:
         builder.EndFactor();

         // Assert:
         Assert.That(stack.Count, Is.EqualTo(1));
         Assert.That(stack.Peek(), Is.InstanceOf(typeof(ITermNode)));
         Assert.That((stack.Peek() as ITermNode)?.FirstFactor, Is.Not.Null);
      }

      [Test]
      public void BeginParens_WhenGivenAToken_PushesParenNode()
      {
         // Arrange:
         var token = new Token { TokenKind = TokenKind.LeftParen, Image = "(" };
         _nodeFactoryMock.Setup(m => m.Create(It.IsAny<AstNodeType>(),
                                                                         It.IsAny<IToken>()))
                         .Returns(new ParenNode(token, _tracerMock.Object));

         var stack = new Stack<IAstNode>();

         var builder = new AstBuilder(_nodeFactoryMock.Object, null, stack, _tracerMock.Object);

         // Act:
         builder.BeginParens(token);

         // Assert:
         Assert.That(stack.Count, Is.EqualTo(1));
         Assert.That(stack.Peek(), Is.InstanceOf(typeof(IParenNode)));
         Assert.That((stack.Peek() as IParenNode)?.Expression, Is.Null);
      }

      [Test]
      public void EndParen_WhenInsideTerm_AppendsFactor()
      {
         // Arrange:
         var stack = new Stack<IAstNode>();

         var factorToken = new Token { TokenKind = TokenKind.String, Image = "<E>" };
         var factor = new FactorNode(factorToken, _tracerMock.Object);
         stack.Push(factor);

         var lParenToken = new Token { TokenKind = TokenKind.LeftParen, Image = "(" };
         var paren = new ParenNode(lParenToken, _tracerMock.Object);
         stack.Push(paren);

         var builder = new AstBuilder(null, null, stack, _tracerMock.Object);

         // Act:
         builder.EndParens();

         // Assert:
         Assert.That(stack.Count, Is.EqualTo(1));
         Assert.That(stack.Peek(), Is.InstanceOf(typeof(IFactorNode)));
         Assert.That((stack.Peek() as IFactorNode)?.FactorExpr, Is.EqualTo(paren));
      }

      [Test]
      public void BeginOption_WhenGivenAToken_PushesOptionNode()
      {
         // Arrange:
         var token = new Token { TokenKind = TokenKind.LeftBracket, Image = "[" };
         _nodeFactoryMock.Setup(m => m.Create(It.IsAny<AstNodeType>(),
                                                                        It.IsAny<IToken>()))
                         .Returns(new OptionNode(token, _tracerMock.Object));

         var stack = new Stack<IAstNode>();

         var builder = new AstBuilder(_nodeFactoryMock.Object, null, stack, _tracerMock.Object);

         // Act:
         builder.BeginOption(token);

         // Assert:
         Assert.That(stack.Count, Is.EqualTo(1));
         Assert.That(stack.Peek(), Is.InstanceOf(typeof(IOptionNode)));
         Assert.That((stack.Peek() as IOptionNode)?.Expression, Is.Null);
      }

      [Test]
      public void EndOption_WhenInsideTerm_AppendsFactor()
      {
         // Arrange:
         var stack = new Stack<IAstNode>();

         var factorToken = new Token { TokenKind = TokenKind.String, Image = "<F>" };
         var factor = new FactorNode(factorToken, _tracerMock.Object);
         stack.Push(factor);

         var lBracketToken = new Token { TokenKind = TokenKind.LeftBracket, Image = "[" };
         var option = new OptionNode(lBracketToken, _tracerMock.Object);
         stack.Push(option);

         var builder = new AstBuilder(null, null, stack, _tracerMock.Object);

         // Act:
         builder.EndOption();

         // Assert:
         Assert.That(stack.Count, Is.EqualTo(1));
         Assert.That(stack.Peek(), Is.InstanceOf(typeof(IFactorNode)));
         Assert.That((stack.Peek() as IFactorNode)?.FactorExpr, Is.EqualTo(option));
      }

      [Test]
      public void BeginKleene_WhenGivenAToken_PushesOptionNode()
      {
         // Arrange:
         var token = new Token { TokenKind = TokenKind.LeftBrace, Image = "{" };
         _nodeFactoryMock.Setup(m => m.Create(It.IsAny<AstNodeType>(),
                                                                         It.IsAny<IToken>()))
                         .Returns(new KleeneNode(token, _tracerMock.Object));

         var stack = new Stack<IAstNode>();

         var builder = new AstBuilder(_nodeFactoryMock.Object, null, stack, _tracerMock.Object);

         // Act:
         builder.BeginKleene(token);

         // Assert:
         Assert.That(stack.Count, Is.EqualTo(1));
         Assert.That(stack.Peek(), Is.InstanceOf(typeof(IKleeneStarNode)));
         Assert.That((stack.Peek() as IKleeneStarNode)?.Expression, Is.Null);
      }

      [Test]
      public void EndKleene_WhenInsideTerm_AppendsFactor()
      {
         // Arrange:
         var stack = new Stack<IAstNode>();

         var factorToken = new Token { TokenKind = TokenKind.String, Image = "<F>" };
         var factor = new FactorNode(factorToken, _tracerMock.Object);
         stack.Push(factor);

         var lBracketToken = new Token { TokenKind = TokenKind.LeftBrace, Image = "{" };
         var kleene = new KleeneNode(lBracketToken, _tracerMock.Object);
         stack.Push(kleene);

         var builder = new AstBuilder(null, null, stack, _tracerMock.Object);

         // Act:
         builder.EndKleene();

         // Assert:
         Assert.That(stack.Count, Is.EqualTo(1));
         Assert.That(stack.Peek(), Is.InstanceOf(typeof(IFactorNode)));
         Assert.That((stack.Peek() as IFactorNode)?.FactorExpr, Is.EqualTo(kleene));
      }
   }
}
