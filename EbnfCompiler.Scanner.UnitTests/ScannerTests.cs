using System.IO;
using EbnfCompiler.Compiler;
using NUnit.Framework;

namespace EbnfCompiler.Scanner.UnitTests
{
   [TestFixture]
   public class ScannerTests
   {
      [TestCase("<SomeIdentifer>", TokenKind.Identifier, "SomeIdentifer")]
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

      [TestCase("%TOKENS%", TokenKind.TokensTag, "TOKENS")]
      [TestCase("%EBNF%", TokenKind.EbnfTag, "EBNF")]
      //[TestCase("\u0026", TokenKind.Eof, "\u0026")]
      public void ScanValidIdentifier(string tokenImage, TokenKind expectedTokenKind, string expectedTokenImage)
      {
         // Arrange:
         var stream = new MemoryStream();
         var writter = new StreamWriter(stream);
         writter.Write(tokenImage);
         writter.Flush();
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
         Assert.AreEqual(tokenImage.Length, scanner.CurrentToken.Location.StopColumn);
      }
   }
}
