using System;
using System.IO;
using System.Text;
using EbnfCompiler.Sample.Impl;
using NUnit.Framework;

namespace EbnfCompiler.Sample.UnitTests
{
    [TestFixture]
    public class CodeGeneratorTests
    {
        [Test]
        public void Test()
        {
            try
            {
                var encoding = new UTF8Encoding();
                using var inStream = new MemoryStream();
                inStream.Write(encoding.GetBytes("var n : number = 2 * 5 - 3; Print(\"n = \", n);"));
                inStream.Seek(0, SeekOrigin.Begin);

                IScanner scanner = new Scanner(inStream);

                IAstBuilder astBuilder = new AstBuilder();

                var parser = new Parser(scanner, astBuilder);

                var rootNode = parser.ParseGoal();

                ISemanticChecks semanticChecks = new SemanticChecks();
                semanticChecks.Check(rootNode);

                using var outputStream = File.Create("hello.exe");
                ICodeGenerator codeGen = new CodeGenerator();

                codeGen.Run(rootNode, "hello", outputStream);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                Console.WriteLine();
                throw;
            }
        }
    }
}
