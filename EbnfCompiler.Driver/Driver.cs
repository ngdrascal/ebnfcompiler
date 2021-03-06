﻿using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Text;
using EbnfCompiler.AST;
using EbnfCompiler.AST.Impl;
using EbnfCompiler.CodeGenerator;
using EbnfCompiler.Compiler;
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
	         ""IDENTIFIER"" = ""Identifier""
	         ""STRING""     = ""String""
	         ""ACTION""     = ""Action""
	         ""%TOKENS%""   = ""Tokens""
	         ""%EBNF%""     = ""Ebnf""
	         "".""          = ""Period""
	         ""|""          = ""Or""
	         ""(""          = ""LeftParen""
	         "")""          = ""RightParen""
	         ""{""          = ""LeftBrace""
	         ""}""          = ""RightBrace""
	         ""[""          = ""LeftBracket""
	         ""]""          = ""RightBracket""
	         ""::=""        = ""Assign""
	         ""=""          = ""Equal""

         %EBNF%
	         <Input>      ::= <Tokens> <Grammar> .

	         <Tokens>     ::= ""%TOKENS%"" { <TokenDef> } .

	         <TokenDef>   ::= #AddTokenName# ""STRING"" ""="" #SetTokenDef# ""STRING"" .

	         <Grammar>    ::= ""%EBNF%"" <Syntax> .

	         <Syntax>     ::= #BeginSyntax# <Statement> { <Statement> } #EndSyntax# .

	         <Statement>  ::= #BeginStatement# ""IDENTIFIER"" ""::="" <Expression> ""."" #EndStatement# .

	         <Expression> ::= #BeginExpression# <Term> { ""|"" <Term> } #EndExpression# .

	         <Term>       ::= #BeginTerm# <Factor> { <Factor> #EndTerm# } .

	         <Factor>     ::= #BeginFactor# 
                        (#FoundProduction(token)# ""IDENTIFIER"" |
					          #FoundTerminal(token)# ""STRING"" |
					          #BeginParens# ""("" <Expression> "")"" #EndParens# |
					          #BeginOption# ""["" <Expression> ""]"" #EndOption# |
					          #BeginKleene# ""{"" <Expression> ""}"" #EndKleene#) #EndFactor# .

	         <Action>     ::= [ #FoundAction# ""ACTION"" ] .

      ";

      private const string TestCase4A = @"
         %TOKENS%
            ""a"" = ""tkA""
            ""b"" = ""tkB""
         %EBNF%
            <S> ::= #A0# ( #A1# ""a"" #A2# ) | ( #B3# ""b"" #B4#)  .
       ";

      [Test, Ignore("Just for experimenting")]
      public void Test01()
      {
         var loggerFactory = LoggerFactory.Create(builder =>
         {
            builder
               .AddFilter("ASTBuilder", LogLevel.Warning)
               .AddFilter("ASTTraverser", LogLevel.Warning)
               .AddFilter("CSGEN", LogLevel.Trace)
               .AddDebug();
         });
         var builderLogger = loggerFactory.CreateLogger("ASTBuilder");
         var builderTracer = new DebugTracer(builderLogger);

         var encoding = new UTF8Encoding();
         using var stream = new MemoryStream();
         stream.Write(encoding.GetBytes(TestCase1B));
         stream.Seek(0, SeekOrigin.Begin);

         var scanner = new Scanner.Scanner(stream);
         var astBuilder = new AstBuilder(new AstNodeFactory(builderTracer),
                                         new ProdInfoFactory(builderTracer), new Stack<IAstNode>(), builderTracer);
         var parser = new Parser.Parser(scanner, astBuilder);
         var rootNode = parser.ParseGoal();

         var traverserLogger = loggerFactory.CreateLogger("ASTTraverser");
         var traverserTracer = new DebugTracer(traverserLogger);
         var traverser = new AstTraverser(traverserTracer);

         var output = new MemoryStream();
         var streamWriter = new StreamWriter(stream);
         var gen = new CSharpGenerator(traverser, streamWriter);

         gen.Run(rootNode);
      }
   }
}
