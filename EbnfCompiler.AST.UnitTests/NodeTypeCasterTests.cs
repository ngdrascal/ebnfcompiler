using System;
using System.Diagnostics.CodeAnalysis;
using EbnfCompiler.AST.Impl;
using EbnfCompiler.Compiler;
using Moq;
using NUnit.Framework;

namespace EbnfCompiler.AST.UnitTests
{
   public delegate IAstNode CastFunc(IAstNode node);

   [TestFixture, ExcludeFromCodeCoverage]
   public class NodeTypeCasterTests
   {
      [Test]
      public void NodeTypeCaster_WhenValidCast_ReturnsCorrectType()
      {
         AsSomething_WhenIsSomething_ReturnsSomething<SyntaxNode, ISyntaxNode>(s => s.AsSyntax());
         AsSomething_WhenIsSomething_ReturnsSomething<StatementNode, IStatementNode>(s => s.AsStatement());
         AsSomething_WhenIsSomething_ReturnsSomething<ExpressionNode, IExpressionNode>(s => s.AsExpression());
         AsSomething_WhenIsSomething_ReturnsSomething<TermNode, ITermNode>(s => s.AsTerm());
         AsSomething_WhenIsSomething_ReturnsSomething<FactorNode, IFactorNode>(s => s.AsFactor());
         AsSomething_WhenIsSomething_ReturnsSomething<ProdRefNode, IProdRefNode>(s => s.AsProdRef());
         AsSomething_WhenIsSomething_ReturnsSomething<TerminalNode, ITerminalNode>(s => s.AsTerminal());
         AsSomething_WhenIsSomething_ReturnsSomething<ParenNode, IParenNode>(s => s.AsParen());
         // AsSomething_WhenIsSomething_ReturnsSomething<OptionNode, IOptionNode>(s => s.AsOption());
         // AsSomething_WhenIsSomething_ReturnsSomething<KleeneNode, IKleeneStarNode>(s => s.AsKleene());
         AsSomething_WhenIsSomething_ReturnsSomething<ActionNode, IActionNode>(s => s.AsAction());
      }

      private void AsSomething_WhenIsSomething_ReturnsSomething<TConcrete , TInterface>(CastFunc castFunc)
      {
         // Arrange:
         var token = new Token();
         var tracer = new Mock<IDebugTracer>().Object;
         var something = (IAstNode)Activator.CreateInstance(typeof(TConcrete), token, tracer);

         // Act:
         var actual = castFunc(something);

         // Assert:
         Assert.That(actual, Is.InstanceOf(typeof(TInterface)));
      }

      [Test]
      public void NodeTypeCaster_WhenInvalidCast_ThrowsException()
      {
         AsSomething_WhenIsNotSomething_ThrowsException<FactorNode>(s => s.AsSyntax());
         AsSomething_WhenIsNotSomething_ThrowsException<SyntaxNode>(s => s.AsStatement());
         AsSomething_WhenIsNotSomething_ThrowsException<SyntaxNode>(s => s.AsExpression());
         AsSomething_WhenIsNotSomething_ThrowsException<SyntaxNode>(s => s.AsTerm());
         AsSomething_WhenIsNotSomething_ThrowsException<SyntaxNode>(s => s.AsFactor());
         AsSomething_WhenIsNotSomething_ThrowsException<SyntaxNode>(s => s.AsProdRef());
         AsSomething_WhenIsNotSomething_ThrowsException<SyntaxNode>(s => s.AsTerminal());
         AsSomething_WhenIsNotSomething_ThrowsException<SyntaxNode>(s => s.AsParen());
         AsSomething_WhenIsNotSomething_ThrowsException<SyntaxNode>(s => s.AsOption());
         AsSomething_WhenIsNotSomething_ThrowsException<SyntaxNode>(s => s.AsKleene());
         AsSomething_WhenIsNotSomething_ThrowsException<SyntaxNode>(s => s.AsAction());
      }

      private void AsSomething_WhenIsNotSomething_ThrowsException<TConcrete>(CastFunc castFunc)
      {
         // Arrange:
         var token = new Token();
         var tracer = new Mock<IDebugTracer>().Object;
         var something = (IAstNode)Activator.CreateInstance(typeof(TConcrete), token, tracer);

         // Act:
         void Lambda() => castFunc(something);

         // Assert:
         Assert.Throws<NodeCastException>(Lambda);
      }

   }
}
