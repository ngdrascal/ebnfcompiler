using System.Diagnostics.CodeAnalysis;
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
      private Mock<IAstBuilder> _astBuilderMock;

      [SetUp]
      public void Setup()
      {
         _astBuilderMock = new Mock<IAstBuilder>();
      }

      //    %TOKENS%
      //       ""a"" = ""tkA""
      //    %EBNF%
      //       <S> := ""a"" .
      private static readonly IToken[] FailingTest1 =
      {
         new Token(TokenKind.TokensTag,  "%TOKENS%"),
         new Token(TokenKind.String,     "a"),
         new Token(TokenKind.Equal,      "="),
         new Token(TokenKind.String,     "tkA"),
         new Token(TokenKind.EbnfTag,    "%EBNF%"),
         new Token(TokenKind.Identifier, "S"),
         new Token(TokenKind.Error,      ":="),
         new Token(TokenKind.String,     "a"),
         new Token(TokenKind.Period,     "."),
         new Token(TokenKind.Eof,        "<eof>")
      };

      //    %TOKENS%
      //       ""a"" = ""tkA""
      //    %EBNF%
      //       <S> ::= ""a""
      private static readonly IToken[] FailingTest2 =
      {
         new Token(TokenKind.TokensTag,  "%TOKENS%"),
         new Token(TokenKind.String,     "a"),
         new Token(TokenKind.Equal,      "="),
         new Token(TokenKind.String,     "tkA"),
         new Token(TokenKind.EbnfTag,    "%EBNF%"),
         new Token(TokenKind.Identifier, "S"),
         new Token(TokenKind.Assign,     "::="),
         new Token(TokenKind.String,     "a"),
         new Token(TokenKind.Eof,        "<eof>")
      };

      //    %TOKENS%
      //       ""a"" = ""tkA""
      //    %EBNF%
      //       <S> ::= ""a"" }
      private static readonly IToken[] FailingTest3 =
      {
         new Token(TokenKind.TokensTag,  "%TOKENS%"),
         new Token(TokenKind.String,     "a"),
         new Token(TokenKind.Equal,      "="),
         new Token(TokenKind.String,     "tkA"),
         new Token(TokenKind.EbnfTag,    "%EBNF%"),
         new Token(TokenKind.Identifier, "S"),
         new Token(TokenKind.Assign,     "::="),
         new Token(TokenKind.String,     "a"),
         new Token(TokenKind.RightBrace, "}"),
         new Token(TokenKind.Eof,        "<eof>")
      };

      private static IToken[][] _allFailingTests =
      {
         FailingTest1, FailingTest2, FailingTest3
      };

      [TestCaseSource(nameof(_allFailingTests))]
      public void Parser_WhenInvalidInput_ThrowsException(IToken[] testData)
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
         scanner.Setup(s => s.CurrentToken).Returns(() =>
            testData[currentIndex]
         );

         var parser = new Parser(scanner.Object, _astBuilderMock.Object);

         // Act:
         void ParseGoal() => parser.ParseGoal();

         // Assert:
         Assert.Throws<SyntaxErrorException>(ParseGoal);
      }

      // %TOKENS%
      //    "a" = "tkA"
      // %EBNF%
      //    <S> ::= "a" .
      private static readonly IToken[] PassingTest1 =
      {
         new Token(TokenKind.TokensTag,  "%TOKENS%"),
         new Token(TokenKind.String,     "a"),
         new Token(TokenKind.Equal,      "="),
         new Token(TokenKind.String,     "tkA"),
         new Token(TokenKind.EbnfTag,    "%EBNF%"),
         new Token(TokenKind.Identifier, "S"),
         new Token(TokenKind.Assign,     "::="),
         new Token(TokenKind.String,     "a"),
         new Token(TokenKind.Period,     "."),
         new Token(TokenKind.Eof,        "<eof>")
      };

      // %TOKENS%
      //    "a" = "tkA"
      //    "b" = "tkB"
      // %EBNF%
      //    <S> ::= "a" "b" .
      private static readonly IToken[] PassingTest2 =
      {
         new Token(TokenKind.TokensTag,  "%TOKENS%"),
         new Token(TokenKind.String,     "a"),
         new Token(TokenKind.Equal,      "="),
         new Token(TokenKind.String,     "tkA"),
         new Token(TokenKind.String,     "b"),
         new Token(TokenKind.Equal,      "="),
         new Token(TokenKind.String,     "tkB"),
         new Token(TokenKind.EbnfTag,    "%EBNF%"),
         new Token(TokenKind.Identifier, "S"),
         new Token(TokenKind.Assign,     "::="),
         new Token(TokenKind.String,     "a"),
         new Token(TokenKind.String,     "a"),
         new Token(TokenKind.Period,     "."),
         new Token(TokenKind.Eof,        "<eof>")
      };

      // %TOKENS%
      //    ""a"" = ""tkA""
      //    ""b"" = ""tkB""
      // %EBNF%
      //    <S> ::= (<T> [""a""]) | { ""c"" }  .
      //    <T> ::= ""b"" .
      private static readonly IToken[] PassingTest3 =
      {
         new Token(TokenKind.TokensTag,    "%TOKENS%"),
         new Token(TokenKind.String,       "a"),
         new Token(TokenKind.Equal,        "="),
         new Token(TokenKind.String,       "tkA"),
         new Token(TokenKind.String,       "b"),
         new Token(TokenKind.Equal,        "="),
         new Token(TokenKind.String,       "tkB"),
         new Token(TokenKind.EbnfTag,      "%EBNF%"),

         new Token(TokenKind.Identifier,   "<S>"),
         new Token(TokenKind.Assign,       "::="),
         new Token(TokenKind.LeftParen,    "("),
         new Token(TokenKind.Identifier,   "<T>"),
         new Token(TokenKind.LeftBracket,  "["),
         new Token(TokenKind.String,       "a"),
         new Token(TokenKind.RightBracket, "]"),
         new Token(TokenKind.RightParen,   ")"),
         new Token(TokenKind.Or,           "|"),
         new Token(TokenKind.LeftBrace,    "{"),
         new Token(TokenKind.String,       "c"),
         new Token(TokenKind.RightBrace,   "}"),
         new Token(TokenKind.Period,       "."),

         new Token(TokenKind.Identifier,   "<T>"),
         new Token(TokenKind.Assign,       "::="),
         new Token(TokenKind.String,       "b"),
         new Token(TokenKind.Period,       "."),

         new Token(TokenKind.Eof,        "<eof>")
      };

      // %TOKENS%
      //    "a" = "tkA"
      // %EBNF%
      //    <S> ::= #Action# "a" .
      private static readonly IToken[] PassingTest4 =
      {
         new Token(TokenKind.TokensTag,  "%TOKENS%"),
         new Token(TokenKind.String,     "a"),
         new Token(TokenKind.Equal,      "="),
         new Token(TokenKind.String,     "tkA"),
         new Token(TokenKind.EbnfTag,    "%EBNF%"),
         new Token(TokenKind.Identifier, "S"),
         new Token(TokenKind.Assign,     "::="),
         new Token(TokenKind.Action,     "#Action#"),
         new Token(TokenKind.String,     "a"),
         new Token(TokenKind.Period,     "."),
         new Token(TokenKind.Eof,        "<eof>")
      };

      // %TOKENS%
      //    "a" = "tkA"
      // %EBNF%
      //    <Syntax>     ::= <Statement> { <Statement> } .
      //
      //    <Statement>  ::= "PRODNAME" "::=" <Expression> "." .
      //
      //    <Expression> ::= <Term> { "|" <Term> } .
      //
      //    <Term>       ::= <Factor> { <Factor> } .
      //
      //    <Factor>     ::= "PRODNAME" |
      //                     "STRING" |
      //                     "(" <Expression> ")" |
      //                     "[" <Expression> "]" |
      //                     "{" <Expression> "}" .
      private static readonly IToken[] PassingTest5 =
      {
         new Token(TokenKind.TokensTag,   "%TOKENS%"),
         new Token(TokenKind.String,      "a"),
         new Token(TokenKind.Equal,       "="),
         new Token(TokenKind.String,      "tkA"),
         
         new Token(TokenKind.EbnfTag,     "%EBNF%"), 
         new Token(TokenKind.Identifier,  "<Syntax>"),
         new Token(TokenKind.Assign,      "::="),
         new Token(TokenKind.Identifier,  "<Statement>"),
         new Token(TokenKind.LeftBrace,   "{"),
         new Token(TokenKind.Identifier,  "<Statement>"),
         new Token(TokenKind.RightBrace,  "}"),
         new Token(TokenKind.Period,      "."),

         new Token(TokenKind.Identifier,  "<Statement>"),
         new Token(TokenKind.Assign,      "::="),
         new Token(TokenKind.String,      "PRODNAME"),          
         new Token(TokenKind.String,      "::="),
         new Token(TokenKind.Identifier,  "<Expression>"),
         new Token(TokenKind.String,      "."),
         new Token(TokenKind.Period,      "."),

         new Token(TokenKind.Identifier,  "<Expression>"),
         new Token(TokenKind.Assign,      "::="),
         new Token(TokenKind.Identifier,  "<Term>"),
         new Token(TokenKind.LeftBrace,   "{"),
         new Token(TokenKind.String,      "|"),
         new Token(TokenKind.Identifier,  "<Term>"),
         new Token(TokenKind.RightBrace,  "}"),
         new Token(TokenKind.Period,      "."),

         new Token(TokenKind.Identifier,  "<Term>"),
         new Token(TokenKind.Assign,      "::="),
         new Token(TokenKind.Identifier,  "<Factor>"),
         new Token(TokenKind.LeftBrace,   "{"),
         new Token(TokenKind.Identifier,  "<Factor>"),
         new Token(TokenKind.RightBrace,  "}"),
         new Token(TokenKind.Period,      "."),

         new Token(TokenKind.Identifier,  "<Factor>"),
         new Token(TokenKind.Assign,      "::="),
         new Token(TokenKind.String,      "PRODNAME"),
         new Token(TokenKind.String,      "|"),
         new Token(TokenKind.String,      "STRING"),
         new Token(TokenKind.String,      "|"),
         new Token(TokenKind.LeftParen,   "("),
         new Token(TokenKind.Identifier,  "<Expression>"),
         new Token(TokenKind.RightParen,  ")"),
         new Token(TokenKind.String,      "|"),
         new Token(TokenKind.LeftBracket, "["),
         new Token(TokenKind.Identifier,  "<Expression>"),
         new Token(TokenKind.RightBracket, "]"),
         new Token(TokenKind.String,      "|"),
         new Token(TokenKind.LeftBrace,   "{"),
         new Token(TokenKind.Identifier,  "<Expression>"),
         new Token(TokenKind.RightBrace,  "}"),
         new Token(TokenKind.Period,      "."),
         new Token(TokenKind.Eof,        "<eof>")
      };

      private static IToken[][] _allPassingTests =
      {
         PassingTest1, PassingTest2, PassingTest3, PassingTest4, PassingTest5
      };

      [TestCaseSource(nameof(_allPassingTests))]
      public void Parser_WhenValidInput_DoesNotThrowException(IToken[] testData)
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
