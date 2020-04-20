using System.Diagnostics.CodeAnalysis;
using System.IO;
using EbnfCompiler.Compiler;
using NUnit.Framework;

namespace EbnfCompiler.Scanner.UnitTests
{
   [TestFixture, ExcludeFromCodeCoverage]
   public class ScannerTests
   {
      [TestCase("%TOKENS%", TokenKind.TokensTag, "TOKENS")]
      [TestCase("%EBNF%", TokenKind.EbnfTag, "EBNF")]
      [TestCase("<SomeIdentifier>", TokenKind.Identifier, "SomeIdentifier")]
      [TestCase("\"SomeString\"", TokenKind.String, "SomeString")]
      [TestCase("#SomeAction#", TokenKind.Action, "SomeAction")]

      [TestCase("(", TokenKind.LeftParen, "(")]
      [TestCase(")", TokenKind.RightParen, ")")]
      [TestCase("[", TokenKind.LeftBracket, "[")]
      [TestCase("]", TokenKind.RightBracket, "]")]
      [TestCase("{", TokenKind.LeftBrace, "{")]
      [TestCase("}", TokenKind.RightBrace, "}")]

      [TestCase(".", TokenKind.Period, ".")]
      [TestCase("|", TokenKind.Or, "|")]
      [TestCase("::=", TokenKind.Assign, "::=")]
      [TestCase("=", TokenKind.Equal, "=")]
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
         scanner.Advance();

         // Assert:
         Assert.AreEqual(expectedTokenImage, scanner.CurrentToken.Image);
         Assert.AreEqual(expectedTokenKind, scanner.CurrentToken.TokenKind);
         Assert.AreEqual(1, scanner.CurrentToken.Location.StartLine);
         Assert.AreEqual(1, scanner.CurrentToken.Location.StartColumn);
         Assert.AreEqual(1, scanner.CurrentToken.Location.StopLine);
         Assert.AreEqual(input.Length, scanner.CurrentToken.Location.StopColumn);
      }

      [TestCase("%T\tOKENS>", "T")]
      [TestCase("%EB\tNF>", "EB")]
      [TestCase("%ERROR%>", "ERROR")]
      [TestCase("<Some\tidentifier>", "Some")]
      [TestCase("\"Some\tstring\"", "Some")]
      [TestCase("#Some\taction#", "Some")]
      [TestCase(":=", ":")]
      [TestCase("::", "::")]
      [TestCase("/", "")]
      [TestCase("!", "")]
      public void Scanner_GivenInvalidInput_ReturnsErrorToken(string input, string expectedTokenImage)
      {
         // Arrange:
         var stream = new MemoryStream();
         var writer = new StreamWriter(stream);
         writer.Write(input);
         writer.Flush();
         stream.Seek(0, SeekOrigin.Begin);
         var scanner = new Scanner(stream);

         // Act:
         scanner.Advance();

         // Assert:
         Assert.That(scanner.CurrentToken.Image, Is.EqualTo(expectedTokenImage));
         Assert.That(scanner.CurrentToken.TokenKind, Is.EqualTo(TokenKind.Error));
         Assert.That(scanner.CurrentToken.Location.StartLine, Is.EqualTo(1));
         Assert.That(scanner.CurrentToken.Location.StartColumn, Is.EqualTo(1));
         Assert.That(scanner.CurrentToken.Location.StopLine, Is.EqualTo(1));
         //Assert.That(scanner.CurrentToken.Location.StopColumn, Is.EqualTo(expectedTokenImage.Length));
      }

      [TestCase("\r<S>")]
      [TestCase("\n<S>")]
      [TestCase("\r\n<S>")]
      public void Scanner_GivenControlChars_ReturnsCorrectToken(string input)
      {
         // Arrange:
         var stream = new MemoryStream();
         var writer = new StreamWriter(stream);
         writer.Write(input);
         writer.Flush();
         stream.Seek(0, SeekOrigin.Begin);
         var scanner = new Scanner(stream);

         // Act:
         scanner.Advance();

         // Assert:
         Assert.That(scanner.CurrentToken.TokenKind, Is.EqualTo(TokenKind.Identifier));
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
         var scanner = new Scanner(stream);

         // Act:
         scanner.Advance();

         // Assert:
         Assert.That(scanner.CurrentToken.Image, Is.EqualTo("<eof>"));
         Assert.That(scanner.CurrentToken.TokenKind, Is.EqualTo(TokenKind.Eof));
         Assert.That(scanner.CurrentToken.Location.StartLine, Is.EqualTo(1));
         Assert.That(scanner.CurrentToken.Location.StartColumn, Is.EqualTo(input.Length));
         Assert.That(scanner.CurrentToken.Location.StopLine, Is.EqualTo(1));
         Assert.That(scanner.CurrentToken.Location.StopColumn, Is.EqualTo(input.Length));
      }
   }
}
