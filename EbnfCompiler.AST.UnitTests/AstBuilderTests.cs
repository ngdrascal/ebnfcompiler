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

      [SetUp]
      public void SetUp()
      {
         _tracerMock = new Mock<IDebugTracer>();
      }

      [Test]
      public void AddTokenName_WhenFirstOccurrencesOfToken_AddToTokenDefinition()
      {
         // Arrange:
         var builder = new AstBuilder(null, null, _tracerMock.Object);
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
         var builder = new AstBuilder(null, null, _tracerMock.Object);
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
         var builder = new AstBuilder(null, null, _tracerMock.Object);
         const string actualDefinition = "tkA";
         var token = new Token { Image = actualDefinition };

         // Act:
         builder.AddTokenName(token);
         builder.SetTokenDef(token);

         // Assert:
         Assert.That(builder.TokenDefinitions.Count, Is.EqualTo(1));
         Assert.That(builder.TokenDefinitions.First().Definition, Is.EqualTo(actualDefinition));
      }

      [Test, Ignore("For use as template")]
      public void XYZ_WhenGivenToken_AddToTokenDefinition()
      {
         // Arrange:
         var nodeFactory = new AstNodeFactory(_tracerMock.Object);
         var prodFactory = new ProdInfoFactory(_tracerMock.Object);
         var builder = new AstBuilder(nodeFactory, prodFactory, _tracerMock.Object);
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
