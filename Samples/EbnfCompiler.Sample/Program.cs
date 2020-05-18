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

         parser.ParseGoal();
      }
   }
}
