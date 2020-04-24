using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Text;
using EbnfCompiler.AST;
using EbnfCompiler.Compiler;
using EbnfCompiler.Scanner;
using Moq;
using NUnit.Framework;

namespace EbnfCompiler.Parser.UnitTests
{
   [TestFixture, ExcludeFromCodeCoverage]
   public class ParserTests
   {
      private const string GoodTestCase1 = @"
         %TOKENS%
            ""a"" = ""tkA""
         %EBNF%
            <S> ::= ""a"" .
      ";

      private const string GoodTestCase2 = @"
         %TOKENS%
            ""a"" = ""tkA""
            ""b"" = ""tkB""
         %EBNF%
            <S> ::= ""a"" ""b"" .
      ";

      private const string GoodTestCase3 = @"
         %TOKENS%
            ""a"" = ""tkA""
            ""b"" = ""tkB""
         %EBNF%
            <S> ::= (<T> [""a""]) | { ""c"" }  .
            <T> ::= ""b"" .
      ";

      private const string GoodTestCase4 = @"
         %TOKENS%
            ""a"" = ""tkA""
         %EBNF%
            <S> ::= #Action# ""a"" .
      ";

      private const string GoodTestCase5 = @"
         %TOKENS%
            ""a"" = ""tkA""
         %EBNF%
            <Syntax>     ::= <Statement> { <Statement> } .

            <Statement>  ::= ""PRODNAME"" ""::="" <Expression> ""."" .

            <Expression> ::= <Term> { ""|"" <Term> } .

            <Term>       ::= <Factor> { <Factor> } .

            <Factor>     ::= ""PRODNAME"" |
                             ""STRING"" |
                             ""("" <Expression> "")"" |
                             ""["" <Expression> ""]"" |
                             ""{"" <Expression> ""}"" .
      ";

      private Mock<IAstBuilder> _astBuilderMock;

      [SetUp]
      public void Setup()
      {
         _astBuilderMock = new Mock<IAstBuilder>();
      }

      private Stream PrepareStream(string input)
      {
         var encoding = new UTF8Encoding();
         var stream = new MemoryStream();
         stream.Write(encoding.GetBytes(input));
         stream.Seek(0, SeekOrigin.Begin);

         return stream;
      }

      [TestCase(GoodTestCase1)]
      [TestCase(GoodTestCase2)]
      [TestCase(GoodTestCase3)]
      [TestCase(GoodTestCase4)]
      [TestCase(GoodTestCase5)]
      public void Parser_WhenValidInput_DoesNotThrowException(string input)
      {
         // Arrange:
         using var stream = PrepareStream(input);
         var scanner = new Scanner.Scanner(stream);
         var parser = new Parser(scanner, _astBuilderMock.Object);

         // Act:
         void ParseGoal() => parser.ParseGoal();

         // Assert:
         Assert.DoesNotThrow(ParseGoal);
      }

      private const string BadTestCase1 = @"
         %TOKENS%
            ""a"" = ""tkA""
         %EBNF%
            <S> := ""a"" .
      ";

      private const string BadTestCase2 = @"
         %TOKENS%
            ""a"" = ""tkA""
         %EBNF%
            <S> ::= ""a""
      ";

      private const string BadTestCase3 = @"
         %TOKENS%
            ""a"" = ""tkA""
         %EBNF%
            <S> ::= ""a"" }
      ";

      [TestCase(BadTestCase1, ":")]
      [TestCase(BadTestCase2, "<eof>")]
      [TestCase(BadTestCase3, "}")]
      public void Parser_WhenInvalidInput_ThrowsException(string input, string image)
      {
         // Arrange:
         using var stream = PrepareStream(input);
         var scanner = new Scanner.Scanner(stream);
         var parser = new Parser(scanner, _astBuilderMock.Object);

         // Act:
         void ParseGoal() => parser.ParseGoal();

         // Assert:
         var ex = Assert.Throws<SyntaxErrorException>(ParseGoal);
         Assert.That(ex.Token.Image, Is.EqualTo(image));
      }

      private static readonly IToken[] TestData1 =
      {
         new Token() {TokenKind = TokenKind.TokensTag,  Image = "%TOKENS%"},
         new Token() {TokenKind = TokenKind.String,     Image = "a"},
         new Token() {TokenKind = TokenKind.Equal,      Image = "="},
         new Token() {TokenKind = TokenKind.String,     Image = "tkA"},
         new Token() {TokenKind = TokenKind.EbnfTag,    Image = "%EBNF%"},
         new Token() {TokenKind = TokenKind.Identifier, Image = "S"},
         new Token() {TokenKind = TokenKind.Assign,     Image = "::="},
         new Token() {TokenKind = TokenKind.String,     Image = "a"},
         new Token() {TokenKind = TokenKind.Period,     Image = "."},
         new Token() {TokenKind = TokenKind.Eof,        Image = "<eof>"}
      };

      private static IToken[][] allTests = {TestData1};

      [TestCaseSource(nameof(allTests))]
      public void Parser_(IToken[] testData)
      {
         // Arrange:
         var scanner = new Mock<IScanner>();
         var currentIndex = 0;

         scanner.Setup(s => s.Advance())
            .Callback(() =>
            {
               if (testData[currentIndex].TokenKind == TokenKind.Eof)
                  return;

               currentIndex++;
            });
         scanner.Setup(s => s.CurrentToken).Returns(()=>
            testData[currentIndex]
            );

         var parser = new Parser(scanner.Object, _astBuilderMock.Object);

         // Act:
         void ParseGoal() => parser.ParseGoal();

         // Assert:
         Assert.DoesNotThrow(ParseGoal);
      }
   }
}
