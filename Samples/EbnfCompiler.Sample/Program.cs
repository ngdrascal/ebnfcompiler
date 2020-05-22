using System.Collections.Generic;
using System.IO;
using EbnfCompiler.Sample.Impl;

namespace EbnfCompiler.Sample
{
   public static class Program
   {
      public static void Main(string[] args)
      {
         using var inStream = new FileStream(args[0], FileMode.Open);
         inStream.Seek(0, SeekOrigin.Begin);

         IScanner scanner = new Scanner(inStream);

         IAstBuilder astBuilder = new AstBuilder();

         var parser = new Parser(scanner, astBuilder);

         var rootNode = parser.ParseGoal();

         using var outputStream = File.Create("hello");
         ICodeGenerator codeGen = new CodeGenerator();

         var references = new List<string>
         {
            "C:\\Program Files (x86)\\Reference Assemblies\\Microsoft\\Framework\\.NETFramework\\v4.7.2\\mscorlib.dll"
         };
         codeGen.Run(rootNode, "hello", references, outputStream);
      }
   }
}
