using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Text;
using EbnfCompiler.AST;
using EbnfCompiler.AST.Impl;
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
      private const string TestCase1 = @"
         %TOKENS%
            ""a"" = ""tkA""
         %EBNF%
            <S> ::= <T> <U> .
            <T> ::= [ ""a"" ] ""b"" .
      ";

      private const string TestCase2 = @"
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
         %EBNF%
            <S>  ::= ""a"" ""b"" | ""c"" .
      ";

      private const string TestCase4 = @"
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

      [Test/*, Ignore("Just for experimenting")*/]
      public void Test01()
      {
         var loggerFactory = LoggerFactory.Create(builder =>
         {
            builder
               .AddFilter("EBNF", LogLevel.Trace)
               .AddDebug();
         });
         var logger = loggerFactory.CreateLogger("EBNF");
         var tracer = new DebugTracer(logger);

         var encoding = new UTF8Encoding();
         using var stream = new MemoryStream();
         stream.Write(encoding.GetBytes(TestCase4));
         stream.Seek(0, SeekOrigin.Begin);

         var scanner = new Scanner.Scanner(stream);
         var astBuilder = new AstBuilder(new AstNodeFactory(tracer),
                                         new ProdInfoFactory(tracer), new Stack<IAstNode>(), tracer);
         var parser = new Parser.Parser(scanner, astBuilder);
         parser.ParseGoal();

         var traverser = new AstTraverser(tracer);
         traverser.PreProcess += (node) => { tracer.TraceLine($"Push({node.AstNodeType})"); };
         traverser.PostProcess += () => { tracer.TraceLine($"Pop()"); };

         foreach (var prod in astBuilder.Productions)
         {
            tracer.TraceLine($"\nAST for <{prod.Name}>");
            traverser.Traverse(prod.RightHandSide);
            
            // tracer.TraceLine(new string('-', 40));
            // tracer.TraceLine($"First of <{prod.Name}>: {prod.RightHandSide.FirstSet}");
         }
      }
   }
}
