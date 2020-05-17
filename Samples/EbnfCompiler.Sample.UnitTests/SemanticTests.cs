using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Text;
using NUnit.Framework;

namespace EbnfCompiler.Sample.UnitTests
{
   [TestFixture, ExcludeFromCodeCoverage]
   public class SemanticTests
   {
      [TestCase("var i : string = 1;", "(1,1):Type mismatch.")]
      [TestCase("var s : number = \"Hello\";", "(1,1):Type mismatch.")]
      [TestCase("var i : string = 1 + 2;", "(1,5):Type mismatch.")]
      [TestCase("var i : string = 1 + \"hello\";", "(1,20):Type mismatch.")]
      public void Parser_WhenSemanticError_ThrowsSemanticException1(
         string input, string expectedMessage)
      {
         // Arrange:
         var rootNode = BuildAst(input);
         ISemanticChecks semanticCheckCheck = new SemanticChecks();

         // Act:
         void Action() => semanticCheckCheck.Check(rootNode.Statements.First().AsVarStatement());

         // Assert:
         var ex = Assert.Throws<SemanticErrorException>(Action);
         Assert.That(ex.Message, Is.EqualTo(expectedMessage));
      }

      [TestCase("var i : number = 1; var i : number = 2;",
                "(1,25):Variable \"i\" already declared.")]
      [TestCase("var i : number = 1; var s : string = i;",
                "(1,25):Type mismatch.")]
      // NOTE: this test differs from the other test because it requires two
      //       statements to execute
      public void Parser_WhenSemanticError_ThrowsSemanticException2(
         string input, string expectedMessage)
      {
         // Arrange:
         var rootNode = BuildAst(input);
         ISemanticChecks semanticCheckCheck = new SemanticChecks();

         // Act:
         semanticCheckCheck.Check(rootNode.Statements.First().AsVarStatement());
         void Action() => semanticCheckCheck.Check(rootNode.Statements.Last().AsVarStatement());

         // Assert:
         var ex = Assert.Throws<SemanticErrorException>(Action);
         Assert.That(ex.Message, Is.EqualTo(expectedMessage));
      }

      private IRootNode BuildAst(string input)
      {
         var encoding = new UTF8Encoding();
         using var inStream = new MemoryStream();
         inStream.Write(encoding.GetBytes(input));
         inStream.Seek(0, SeekOrigin.Begin);

         IScanner scanner = new Scanner(inStream);
         IAstBuilder astBuilder = new AstBuilder();
         return new Parser(scanner, astBuilder).ParseGoal();
      }
   }
}
