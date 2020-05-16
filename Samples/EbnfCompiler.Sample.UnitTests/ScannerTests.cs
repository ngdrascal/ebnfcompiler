using System.Diagnostics.CodeAnalysis;
using System.IO;
using NUnit.Framework;

namespace EbnfCompiler.Sample.UnitTests
{
   [TestFixture, ExcludeFromCodeCoverage]
   public class ScannerTests
   {
      [TestCase("n", TokenKind.Identifier, "n")]
      [TestCase("nu", TokenKind.Identifier, "nu")]
      [TestCase("num", TokenKind.Identifier, "num")]
      [TestCase("numb", TokenKind.Identifier, "numb")]
      [TestCase("numbe", TokenKind.Identifier, "numbe")]
      [TestCase("number", TokenKind.Number, "number")]

      [TestCase("nz", TokenKind.Identifier, "nz")]
      [TestCase("nuz", TokenKind.Identifier, "nuz")]
      [TestCase("numz", TokenKind.Identifier, "numz")]
      [TestCase("numbz", TokenKind.Identifier, "numbz")]
      [TestCase("numbez", TokenKind.Identifier, "numbez")]
      [TestCase("numberz", TokenKind.Identifier, "numberz")]

      [TestCase("P", TokenKind.Identifier, "P")]
      [TestCase("Pr", TokenKind.Identifier, "Pr")]
      [TestCase("Pri", TokenKind.Identifier, "Pri")]
      [TestCase("Prin", TokenKind.Identifier, "Prin")]
      [TestCase("Print", TokenKind.Print, "Print")]

      [TestCase("Pz", TokenKind.Identifier, "Pz")]
      [TestCase("Prz", TokenKind.Identifier, "Prz")]
      [TestCase("Priz", TokenKind.Identifier, "Priz")]
      [TestCase("Prinz", TokenKind.Identifier, "Prinz")]
      [TestCase("Printz", TokenKind.Identifier, "Printz")]

      [TestCase("PrintL", TokenKind.Identifier, "PrintL")]
      [TestCase("PrintLi", TokenKind.Identifier, "PrintLi")]
      [TestCase("PrintLin", TokenKind.Identifier, "PrintLin")]
      [TestCase("PrintLine", TokenKind.PrintLine, "PrintLine")]

      [TestCase("PrintLz", TokenKind.Identifier, "PrintLz")]
      [TestCase("PrintLiz", TokenKind.Identifier, "PrintLiz")]
      [TestCase("PrintLinz", TokenKind.Identifier, "PrintLinz")]
      [TestCase("PrintLinez", TokenKind.Identifier, "PrintLinez")]

      [TestCase("s", TokenKind.Identifier, "s")]
      [TestCase("st", TokenKind.Identifier, "st")]
      [TestCase("str", TokenKind.Identifier, "str")]
      [TestCase("stri", TokenKind.Identifier, "stri")]
      [TestCase("strin", TokenKind.Identifier, "strin")]
      [TestCase("string", TokenKind.String, "string")]

      [TestCase("sz", TokenKind.Identifier, "sz")]
      [TestCase("stz", TokenKind.Identifier, "stz")]
      [TestCase("strz", TokenKind.Identifier, "strz")]
      [TestCase("striz", TokenKind.Identifier, "striz")]
      [TestCase("strinz", TokenKind.Identifier, "strinz")]
      [TestCase("stringz", TokenKind.Identifier, "stringz")]

      [TestCase("v", TokenKind.Identifier, "v")]
      [TestCase("va", TokenKind.Identifier, "va")]
      [TestCase("var", TokenKind.Var, "var")]

      [TestCase("vz", TokenKind.Identifier, "vz")]
      [TestCase("vaz", TokenKind.Identifier, "vaz")]
      [TestCase("varz", TokenKind.Identifier, "varz")]

      [TestCase("(", TokenKind.LeftParen, "(")]
      [TestCase(")", TokenKind.RightParen, ")")]
      [TestCase(":", TokenKind.Colon, ":")]
      [TestCase(";", TokenKind.SemiColon, ";")]
      [TestCase("+", TokenKind.Plus, "+")]
      [TestCase("-", TokenKind.Minus, "-")]
      [TestCase("*", TokenKind.Asterisk, "*")]
      [TestCase("/", TokenKind.ForwardSlash, "/")]
      [TestCase("=", TokenKind.Assign, "=")]

      [TestCase("\"Some string literal\"", TokenKind.StringLiteral, "Some string literal")]
      [TestCase("\"missing quote", TokenKind.Error, "missing quote")]


      [TestCase("12", TokenKind.NumberLiteral, "12")]
      [TestCase("12.34", TokenKind.NumberLiteral, "12.34")]
      [TestCase("+12", TokenKind.NumberLiteral, "+12")]
      [TestCase("-12", TokenKind.NumberLiteral, "-12")]
      [TestCase("+12.34", TokenKind.NumberLiteral, "+12.34")]
      [TestCase("-12.34", TokenKind.NumberLiteral, "-12.34")]

      [TestCase("id", TokenKind.Identifier, "id")]
      [TestCase("+id", TokenKind.Plus, "+")]
      [TestCase("-id", TokenKind.Minus, "-")]

      //[TestCase("", TokenKind.Eof, "<eof>")]
      public void Scanner_GivenValidInput_ReturnsCorrectToken(string input, TokenKind expectedTokenKind, string expectedTokenImage)
      {
         // Arrange:
         var stream = new MemoryStream();
         var writer = new StreamWriter(stream);
         writer.Write(input);
         writer.Flush();
         stream.Seek(0, SeekOrigin.Begin);
         var scanner = new Scanner(stream);

         // Act:
         // scanner.Advance();

         // Assert:
         Assert.AreEqual(expectedTokenKind, scanner.CurrentToken.TokenKind);
         Assert.AreEqual(expectedTokenImage, scanner.CurrentToken.Image);
         Assert.AreEqual(1, scanner.CurrentToken.Location.StartLine);
         Assert.AreEqual(1, scanner.CurrentToken.Location.StartColumn);
         Assert.AreEqual(1, scanner.CurrentToken.Location.StopLine);
      }

      [TestCase("!", "")]
      public void Scanner_GivenInvalidInput_ReturnsErrorToken(string input, string expectedTokenImage)
      {
         // Arrange:
         var stream = new MemoryStream();
         var writer = new StreamWriter(stream);
         writer.Write(input);
         writer.Flush();
         stream.Seek(0, SeekOrigin.Begin);

         // Act:
         var scanner = new Scanner(stream); // scanner advances on creation

         // Assert:
         Assert.That(scanner.CurrentToken.TokenKind, Is.EqualTo(TokenKind.Error));
         Assert.That(scanner.CurrentToken.Image, Is.EqualTo(expectedTokenImage));
         Assert.That(scanner.CurrentToken.Location.StartLine, Is.EqualTo(1));
         Assert.That(scanner.CurrentToken.Location.StartColumn, Is.EqualTo(1));
         Assert.That(scanner.CurrentToken.Location.StopLine, Is.EqualTo(1));
      }

      [TestCase("\rvar")]
      [TestCase("\nvar")]
      [TestCase("\r\nvar")]
      public void Scanner_GivenControlChars_ReturnsCorrectToken(string input)
      {
         // Arrange:
         var stream = new MemoryStream();
         var writer = new StreamWriter(stream);
         writer.Write(input);
         writer.Flush();
         stream.Seek(0, SeekOrigin.Begin);

         // Act:
         var scanner = new Scanner(stream); // scanner advances on creation

         // Assert:
         Assert.That(scanner.CurrentToken.TokenKind, Is.EqualTo(TokenKind.Var));
      }

      [Test]
      public void Scanner_GivenACommentOnlyLine_IgnoresLines()
      {
         // Arrange:
         const string input = "// this is a comment";

         var stream = new MemoryStream();
         var writer = new StreamWriter(stream);
         writer.Write(input);
         writer.Flush();
         stream.Seek(0, SeekOrigin.Begin);

         // Act:
         var scanner = new Scanner(stream); // scanner advances on creation

         // Assert:
         Assert.That(scanner.CurrentToken.TokenKind, Is.EqualTo(TokenKind.Eof));
         Assert.That(scanner.CurrentToken.Image, Is.EqualTo("<eof>"));
         Assert.That(scanner.CurrentToken.Location.StartLine, Is.EqualTo(1));
         Assert.That(scanner.CurrentToken.Location.StartColumn, Is.EqualTo(input.Length));
         Assert.That(scanner.CurrentToken.Location.StopLine, Is.EqualTo(1));
         Assert.That(scanner.CurrentToken.Location.StopColumn, Is.EqualTo(input.Length));
      }
   }
}
