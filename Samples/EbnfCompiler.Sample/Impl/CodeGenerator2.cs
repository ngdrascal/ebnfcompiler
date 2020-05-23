using System;
using System.IO;
using System.Reflection;
using System.Reflection.Emit;

namespace EbnfCompiler.Sample.Impl
{
    public class CodeGenerator2 : ICodeGenerator
    {
        public void Run(IRootNode rootNode, string moduleName, Stream output)
        {
            var assemblyName = new AssemblyName(moduleName);
            var assemblyBuilder = AssemblyBuilder.DefineDynamicAssembly(assemblyName, AssemblyBuilderAccess.Run);

            var moduleBuilder = assemblyBuilder.DefineDynamicModule(assemblyName.Name);

            var typeBuilder = moduleBuilder.DefineType("Program", TypeAttributes.Class | TypeAttributes.Public);

            //var methodBuilder = typeBuilder.DefineMethod();

        }
    }
}
