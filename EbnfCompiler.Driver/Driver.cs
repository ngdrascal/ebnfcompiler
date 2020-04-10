using System.Diagnostics;
using System.IO;
using System.Text;
using EbnfCompiler.AST;
using NUnit.Framework;

namespace EbnfCompiler.Driver
{
   [TestFixture]
   public class Driver
   {
      //private const string TestFilePath = @"..\..\..\..\Test Data\";

      private const string TestCase = @"
         %TOKENS%
            ""a"" = ""tkA""
         %EBNF%
            <S> ::= (""a"") | <T> .
            <T> ::= ""b"" .
      ";

      [Test]
      public void Test01()
      {
         //var stream = new FileStream(TestFilePath + "Ebnf.bnf", FileMode.Open, FileAccess.Read);
         var encoding = new UTF8Encoding();
         using var stream = new MemoryStream();
         stream.Write(encoding.GetBytes(TestCase));
         stream.Seek(0, SeekOrigin.Begin);


         var scanner = new Scanner.Scanner(stream);
         var astBuilder = new AstBuilder();
         var parser = new Parser.Parser(scanner, astBuilder);
         parser.ParseGoal();

         foreach (var prod in astBuilder.Productions.Values)
         {
            Debug.WriteLine($"{prod.Name}: {prod.Expression.FirstSet}");
         }
      }
   }
}
