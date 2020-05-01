using System.Collections.Generic;
using System.IO;
using EbnfCompiler.AST;
using EbnfCompiler.AST.Impl;
using EbnfCompiler.CodeGenerator;
using Microsoft.Extensions.Logging;

namespace EbnfCompiler
{
   internal class Program
   {
      static void Main(string[] args)
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

         using var stream = new FileStream(args[0], FileMode.Open);
         stream.Seek(0, SeekOrigin.Begin);

         var scanner = new Scanner.Scanner(stream);

         var astBuilder = new AstBuilder(new AstNodeFactory(tracer), new ProdInfoFactory(tracer), new Stack<IAstNode>(), tracer);

         var parser = new Parser.Parser(scanner, astBuilder);

         var (tokens, ast) = parser.ParseGoal();

         var traverser = new AstTraverser(tracer);
         var gen = new CSharpGenerator(ast, tokens, traverser, loggerFactory.CreateLogger("CSGEN"));
         gen.Run();
      }
   }
}
