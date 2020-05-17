using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Text;
using NUnit.Framework;

namespace EbnfCompiler.Sample.UnitTests
{
   [TestFixture, ExcludeFromCodeCoverage]
   public class ParserTests
   {
      [TestCase("var i : number = 1;",           "var i : number = 1;")]
      [TestCase("var i : number = 1 + 2;",       "var i : number = (1 + 2);")]
      [TestCase("var i : number = 1 + 2 * 3;",   "var i : number = (1 + (2 * 3));")]
      [TestCase("var i : number = (1 + 2) * 3;", "var i : number = ((1 + 2) * 3);")]
      [TestCase("var i : number = -1;",          "var i : number = -1;")]
      [TestCase("var i : number = -1 - -2;",     "var i : number = (-1 - -2);")]
      [TestCase("var i : number = j + 1;",       "var i : number = (j + 1);")]

      [TestCase("var s : string = \"Hello\";",   "var s : string = \"Hello\";")]
      [TestCase("var s : string = \"Hello,\" + \"world!\";", 
                "var s : string = (\"Hello,\" + \"world!\");")]

      [TestCase("Print(1);", "Print(1);")]
      [TestCase("Print(\"Hello, world!\");",     "Print(\"Hello, world!\");")]

      [TestCase("Print(\"i = \", i);",           "Print(\"i = \", i);")]

      public void Test(string input, string expectedImage)
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
   }
}
