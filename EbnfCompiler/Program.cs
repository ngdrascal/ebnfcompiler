using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using EbnfCompiler.AST;
using EbnfCompiler.AST.Impl;
using EbnfCompiler.CodeGenerator;
using EbnfCompiler.Compiler;
using Microsoft.Extensions.Logging;

namespace EbnfCompiler
{
   [ExcludeFromCodeCoverage]
   internal class Program
   {
      static void Main(string[] args)
      {
         var loggerFactory = LoggerFactory.Create(builder =>
         {
            builder.ClearProviders();
         });
         var nullLogger = loggerFactory.CreateLogger("NULL");
         var tracer = new DebugTracer(nullLogger);

         using var inStream = new FileStream(args[0], FileMode.Open);
         inStream.Seek(0, SeekOrigin.Begin);

         var scanner = new Scanner.Scanner(inStream);

         var astBuilder = new AstBuilder(new AstNodeFactory(tracer),
                                         new ProdInfoFactory(tracer),
                                         new Stack<IAstNode>(), tracer);

         var parser = new Parser.Parser(scanner, astBuilder);

         var rootNode = parser.ParseGoal();

         var traverser = new AstTraverser(tracer);
         var outFileName = Path.GetDirectoryName(args[0]) + "\\Parser.cs";
         using var outStream = new FileStream(outFileName, FileMode.Create);
         using var streamWriter = new StreamWriter(outStream);

         var gen = new CSharpGenerator(traverser, streamWriter);
         gen.Run(rootNode);

      }
   }
}
