using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Text;
using EbnfCompiler.AST;
using EbnfCompiler.AST.Impl;
using EbnfCompiler.CodeGenerator;
using Microsoft.Extensions.Logging;
using NUnit.Framework;

namespace EbnfCompiler.Driver
{
   /*

      <Syntax>     ::= <Statement> { <Statement> } .

      <Statement>  ::= "PRODNAME" "::=" <Expression> "." .

      <Expression> ::= <Term> { "|" <Term> } .

      <Term>       ::= <Factor> { <Factor> } .

      <Factor>     ::= "PRODNAME" |
                       "STRING" |
                       "(" <Expression> ")" |
                       "[" <Expression> "]" |
                       "{" <Expression> "}" .

   */

   [TestFixture, ExcludeFromCodeCoverage]
   public class Driver
   {
      private const string TestCase1A = @"
         %TOKENS%
            ""a"" = ""tkA""
         %EBNF%
            <S> ::= ""a"" .
      ";

      private const string TestCase1B = @"
         %TOKENS%" + @"
            ""a"" = ""tkA""
            ""b"" = ""tkB""
         %EBNF%
            <S> ::= ""a"" ""b"".
      ";

      private const string TestCase1C = @"
         %TOKENS%" + @"
            ""a"" = ""tkA""
         %EBNF%
            <S> ::= ( ""a"" ).
      ";

      private const string TestCase1D = @"
         %TOKENS%" + @"
            ""a"" = ""tkA""
            ""b"" = ""tkB""
         %EBNF%
            <S> ::= [ ""a"" ] <T> .
            <T> ::= ""b"" .
      ";

      private const string TestCase1E = @"
         %TOKENS%" + @"
            ""a"" = ""tkA""
         %EBNF%
            <S> ::= { ""a"" }.
      ";

      private const string TestCase1F = @"
         %TOKENS%
            ""a"" = ""tkA""
            ""b"" = ""tkB""
         %EBNF%
            <S> ::= <T> <U>.
            <T> ::= ""a"" .
            <U> ::= ""b"" .
      ";

      private const string TestCase1G = @"
         %TOKENS%" + @"
            ""a"" = ""tkA""
         %EBNF%
            <S> ::= { <T> }.
            <T> ::= ""a"" .
      ";

      private const string TestCase1 = @"
         %TOKENS%
            ""a"" = ""tkA""
         %EBNF%
            <S> ::= <T> .
            <T> ::= [ ""a"" ] ""b"" .
      ";

      private const string TestCase2A = @"
         %TOKENS%
            ""a"" = ""tkA""
            ""b"" = ""tkB""
         %EBNF%
            <S> ::= ""a"" | ""b"" .
      ";

      private const string TestCase2B = @"
         %TOKENS%
            ""a"" = ""tkA""
            ""b"" = ""tkB""
         %EBNF%
            <S> ::= <U> | <T> .
            <T> ::= [""a""] .
            <U> ::= ""b"" .
      ";

      private const string TestCase3 = @"
         %TOKENS%
            ""a"" = ""tkA""
            ""b"" = ""tkB""
         %EBNF%
            <S>  ::= ""a"" ""b"" | ""c"" .
      ";

      private const string TestCase4 = @"
         %TOKENS%
            ""IDENTIFIER"" = ""tkIdent""
            ""STRING""     = ""tkString""
            ""ACTION""     = ""tkAction""
            ""%TOKENS%""   = ""tkTokens""
            ""%EBNF%""     = ""tkEbnf""
            "".""          = ""tkPeriod""
            ""|""          = ""tkOr""
            ""(""          = ""tkLParen""
            "")""          = ""tkRParen""
            ""{""          = ""tkLBrace""
            ""}""          = ""tkRBrace""
            ""[""          = ""tkLBracket""
            ""]""          = ""tkRBracket""
            ""::=""        = ""tkAssign""
            ""=""          = ""tkEqual""
         %EBNF%
            <Syntax>     ::= <Statement> { <Statement> } .

            <Statement>  ::= ""IDENTIFIER"" ""::="" <Expression> ""."" .

            <Expression> ::= <Term> { ""|"" <Term> } .

            <Term>       ::= <Factor> { <Factor> } .

            <Factor>     ::= ""IDENTIFIER"" |
                             ""STRING"" |
                             ""("" <Expression> "")"" |
                             ""["" <Expression> ""]"" |
                             ""{"" <Expression> ""}"" .
      ";

      [Test/*, Ignore("Just for experimenting")*/]
      public void Test01()
      {
         var loggerFactory = LoggerFactory.Create(builder =>
         {
            builder
               .AddFilter("PARSER", LogLevel.Information)
               .AddFilter("CSGEN", LogLevel.Trace)
               .AddDebug();
         });
         var logger = loggerFactory.CreateLogger("PARSER");
         var tracer = new DebugTracer(logger);

         var encoding = new UTF8Encoding();
         using var stream = new MemoryStream();
         stream.Write(encoding.GetBytes(TestCase2A));
         stream.Seek(0, SeekOrigin.Begin);

         var scanner = new Scanner.Scanner(stream);
         var astBuilder = new AstBuilder(new AstNodeFactory(tracer),
                                         new ProdInfoFactory(tracer), new Stack<IAstNode>(), tracer);
         var parser = new Parser.Parser(scanner, astBuilder);
         var (tokens, productions) = parser.ParseGoal();

         var traverser = new AstTraverser(tracer);
         var fc = new IcSharpGenerator(productions, tokens, traverser, loggerFactory.CreateLogger("CSGEN"));
         fc.Run();

         foreach (var prod in astBuilder.Productions)
         {
            //    tracer.TraceLine($"\nAST for <{prod.Name}>");
            //    traverser.Traverse(prod.RightHandSide);
            //    
            //    // tracer.TraceLine(new string('-', 40));
            //    // tracer.TraceLine($"First of <{prod.Name}>: {prod.RightHandSide.FirstSet}");
         }
      }
   }
}
