using System;
using System.Collections.Generic;
using System.IO;
using Mono.Cecil;

namespace EbnfCompiler.Sample.Impl
{
    public class CodeGenerator : ICodeGenerator
    {
        public void Run(IRootNode rootNode, string moduleName, List<string> references, Stream output)
        {
            var assemblyName = new AssemblyNameDefinition(moduleName, new Version(1, 0));
            var assemblyDef = AssemblyDefinition.CreateAssembly(assemblyName, moduleName, ModuleKind.Console);

            var assemblies = new List<AssemblyDefinition>();
            foreach (var reference in references)
            {
                try
                {
                    var assembly = AssemblyDefinition.ReadAssembly(reference);
                    assemblies.Add(assembly);
                }
                catch (BadImageFormatException)
                {

                }
                
            }

            var typeDef = new TypeDefinition("", "Program", TypeAttributes.Abstract | TypeAttributes.Sealed);
            assemblyDef.MainModule.Types.Add(typeDef);


            // var main = new MethodDefinition("Main", MethodAttributes.Static, voidType);

            assemblyDef.Write(output);
        }
    }
}
