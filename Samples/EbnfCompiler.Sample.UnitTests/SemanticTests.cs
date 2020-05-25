using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Text;
using EbnfCompiler.Sample.Impl;
using NUnit.Framework;

namespace EbnfCompiler.Sample.UnitTests
{
    [TestFixture, ExcludeFromCodeCoverage]
    public class SemanticTests
    {
        [TestCase("var i : string = 1;", "(1,5):Type mismatch.")]
        [TestCase("var s : number = \"Hello\";", "(1,5):Type mismatch.")]
        [TestCase("var i : string = 1 + 2;", "(1,5):Type mismatch.")]
        [TestCase("var i : string = 1 + \"hello\";", "(1,20):Type mismatch.")]
        [TestCase("var i : number = 1; var i : number = 2;", "(1,25):Variable 'i' already declared.")]
        [TestCase("var i : number = 1; var s : string = i;", "(1,25):Type mismatch.")]
        [TestCase("Print(n);", "(1,7):Variable 'n' is not declared.")]
        public void Semantics_WhenSemanticError_ThrowsSemanticException1(
           string input, string expectedMessage)
        {
            // Arrange:
            var rootNode = BuildAst(input);
            ISemanticChecks semanticCheckCheck = new SemanticChecks();

            // Act:
            void Lambda() => semanticCheckCheck.Check(rootNode);

            // Assert:
            var ex = Assert.Throws<SemanticErrorException>(Lambda);
            Assert.That(ex.Message, Is.EqualTo(expectedMessage));
        }

        [TestCase("var i : number = 1; var j : number = 2; Print(i + j);")]
        [TestCase("var s : string = \"Hello, \"; var t : string = \"world!\"; Print(s + t);")]
        [TestCase("var s : string = \"Hello, \"; var t : string = \"world!\"; Print(s, t);")]
        public void Semantics_WhenValidSemantics_DoesNotThrowException(string input)
        {
            // Arrange:
            var rootNode = BuildAst(input);
            ISemanticChecks semanticCheckCheck = new SemanticChecks();

            // Act:
            semanticCheckCheck.Check(rootNode);

            // Assert:
            Assert.Pass();
        }

        private IRootNode BuildAst(string input)
        {
            var encoding = new UTF8Encoding();
            using var inStream = new MemoryStream();
            inStream.Write(encoding.GetBytes(input));
            inStream.Seek(0, SeekOrigin.Begin);

            IScanner scanner = new Scanner(inStream);
            IAstBuilder astBuilder = new AstBuilder();
            return new Impl.Parser(scanner, astBuilder).ParseGoal();
        }
    }
}
