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

            var moduleBuilder = assemblyBuilder.DefineDynamicModule(moduleName);

            var typeBuilder = moduleBuilder.DefineType("Program", TypeAttributes.Class | TypeAttributes.Public);

            var methodBuilder = typeBuilder.DefineMethod("Main", MethodAttributes.Public | MethodAttributes.Static);

            var ilGenerator = methodBuilder.GetILGenerator();

            // var i : number = 1;
            var i = ilGenerator.DeclareLocal(typeof(float));
            ilGenerator.Emit(OpCodes.Ldc_R4, 1.0);
            ilGenerator.Emit(OpCodes.Stloc, i);

            // var j : number = i + 2;
            var j = ilGenerator.DeclareLocal(typeof(float));
            ilGenerator.Emit(OpCodes.Ldloc, i);
            ilGenerator.Emit(OpCodes.Ldc_R4, 2.0);
            ilGenerator.Emit(OpCodes.Add);
            ilGenerator.Emit(OpCodes.Stloc, j);
        }
    }
}
