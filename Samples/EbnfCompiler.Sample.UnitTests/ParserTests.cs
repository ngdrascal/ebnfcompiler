using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Text;
using EbnfCompiler.Sample.Impl;
using NUnit.Framework;

namespace EbnfCompiler.Sample.UnitTests
{
    [TestFixture, ExcludeFromCodeCoverage]
    public class ParserTests
    {
        [TestCase("let i : number = 1;",               "let i : number = 1;")]
        [TestCase("let i : number = 1 + 2;",           "let i : number = (1 + 2);")]
        [TestCase("let i : number = 1 + 2 * 3;",       "let i : number = (1 + (2 * 3));")]
        [TestCase("let i : number = (1 + 2) * 3 / 4;", "let i : number = (((1 + 2) * 3) / 4);")]
        [TestCase("let i : number = -1;",              "let i : number = -1;")]
        [TestCase("let i : number = -1 - -2;",         "let i : number = (-1 - -2);")]
        [TestCase("let i : number = -(1 + 2);",        "let i : number = -(1 + 2);")]
        [TestCase("let i : number = -(-1 - -2);",      "let i : number = -(-1 - -2);")]
        [TestCase("let i : number = j + 1;",           "let i : number = (j + 1);")]

        [TestCase("let s : string = \"Hello\";",       "let s : string = \"Hello\";")]
        [TestCase("let s : string = \"Hello,\" + \"world!\";",
                  "let s : string = (\"Hello,\" + \"world!\");")]

        [TestCase("Print(1);",                         "Print(1);")]
        [TestCase("Print(\"Hello, world!\");",         "Print(\"Hello, world!\");")]

        [TestCase("Print(\"i = \", i);",               "Print(\"i = \", i);")]

        public void Parser_WhenValidSyntax_GeneratesCorrectAst(string input, string expectedImage)
        {
            // Arrange:
            var encoding = new UTF8Encoding();
            using var inStream = new MemoryStream();
            inStream.Write(encoding.GetBytes(input));
            inStream.Seek(0, SeekOrigin.Begin);

            var scanner = new Scanner(inStream);

            IAstBuilder astBuilder = new AstBuilder();

            var parser = new Parser(scanner, astBuilder);

            // Act:
            var rootNode = parser.ParseGoal();

            // Assert:
            Assert.That(rootNode.Statements.First().ToString(), Is.EqualTo(expectedImage));
        }

        [TestCase("print(1)", "(1,1):Expecting: Let, Print. Found: Identifier.")]
        [TestCase("Print(1)", "(1,8):Expecting: Semicolon. Found: Eof.")]
        [TestCase("Print(1;", "(1,8):Expecting: RightParen. Found: Semicolon.")]
        public void Parser_WhenInvalidSyntax_ThrowsSyntaxException(string input, string expectedMessage)
        {
            // Arrange:
            var encoding = new UTF8Encoding();
            using var inStream = new MemoryStream();
            inStream.Write(encoding.GetBytes(input));
            inStream.Seek(0, SeekOrigin.Begin);

            var scanner = new Scanner(inStream);

            IAstBuilder astBuilder = new AstBuilder();

            var parser = new Parser(scanner, astBuilder);

            // Act:
            void Lambda() => parser.ParseGoal();

            // Assert:
            var ex = Assert.Throws<SyntaxErrorException>(Lambda);
            Assert.That(ex.Message, Is.EqualTo(expectedMessage));
        }
    }
}
