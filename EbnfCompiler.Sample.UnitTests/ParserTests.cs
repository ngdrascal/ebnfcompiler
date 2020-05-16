using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Text;
using NUnit.Framework;

namespace EbnfCompiler.Sample.UnitTests
{
   [TestFixture, ExcludeFromCodeCoverage]
   public class ParserTests
   {
      private const string TestCase1 = @"
         var i : number = -1 - 2;
      ";

      [Test]
      public void Test()
      {
         var encoding = new UTF8Encoding();
         using var inStream = new MemoryStream();
         inStream.Write(encoding.GetBytes(TestCase1));
         inStream.Seek(0, SeekOrigin.Begin);

         var scanner = new Scanner(inStream);

         IAstBuilder astBuilder = new AstBuilder();

         var parser = new Parser(scanner, astBuilder);

         var rootNode = parser.ParseGoal();
      }
   }
}
