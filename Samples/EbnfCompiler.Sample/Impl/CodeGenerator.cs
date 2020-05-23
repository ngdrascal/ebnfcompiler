using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Mono.Cecil;
using Mono.Cecil.Cil;

namespace EbnfCompiler.Sample.Impl
{
    public class CodeGenerator : ICodeGenerator
    {
        private const string MsCorLib =
            "C:\\Program Files (x86)\\Reference Assemblies\\Microsoft\\Framework\\.NETFramework\\v4.7.2\\mscorlib.dll";

        private readonly List<AssemblyDefinition> _assemblies = new List<AssemblyDefinition>();
        private AssemblyDefinition _assemblyDef;
        private ILProcessor _ilProcessor;
        private MethodDefinition _mainMethod;
        private Dictionary<string, TypeReference> _knownTypes;

        public void Run(IRootNode rootNode, string moduleName, Stream output)
        {
            var builtInTypes = new List<(string type, string MetadataName)>()
            {
                ("any", "System.Object"),
                ("number", "System.Single"),
                ("string", "System.String"),
                ("void", "System.Void")
            };

            var assemblyName = new AssemblyNameDefinition(moduleName, new Version(1, 0));
            _assemblyDef = AssemblyDefinition.CreateAssembly(assemblyName, moduleName, ModuleKind.Console);

            _knownTypes = new Dictionary<string, TypeReference>();

            var assembly = AssemblyDefinition.ReadAssembly(MsCorLib);
            _assemblies.Add(assembly);

            foreach (var (typeSymbol, metadataName) in builtInTypes)
            {
                var typeDefinition = ResolveTypeDefinition(metadataName);
                var typeReference = _assemblyDef.MainModule.ImportReference(typeDefinition);
                _knownTypes.Add(typeSymbol, typeReference);
            }

            var objectType = _knownTypes["any"];
            var typeDef = new TypeDefinition("", "Program", TypeAttributes.Abstract | TypeAttributes.Sealed, objectType);
            _assemblyDef.MainModule.Types.Add(typeDef);

            var consoleTypeDef = ResolveTypeDefinition("System.Console");
            _assemblyDef.MainModule.ImportReference(consoleTypeDef);
            
            var voidType = _knownTypes["void"];
            _mainMethod = new MethodDefinition("Main", MethodAttributes.Static | MethodAttributes.Private, voidType);
            typeDef.Methods.Add(_mainMethod);
            _assemblyDef.EntryPoint = _mainMethod;

            var consoleWriteLineRef = ResolveMethod("System.Console", "WriteLine", new[] {"System.String"});

            _ilProcessor = _mainMethod.Body.GetILProcessor();
            // ilProcessor.Emit(OpCodes.Ldstr, "Hello, world!");
            // ilProcessor.Emit(OpCodes.Call, consoleWriteLineRef);


            _ilProcessor.Emit(OpCodes.Ret);

            _assemblyDef.Write(output);
        }

        private void TraverseTree(IRootNode rootNode)
        {
            var traverser = new AstTraverser();
            traverser.ProcessNode += (node) =>
            {
                switch (node.AstNodeType)
                {
                    case AstNodeTypes.VarStatement:
                        // var varStmtNode = astNode.AsVarStatement();
                        // EmitVarDeclation(varStmtNode.Variable);
                        // EmitExpression(varStmtNode.Expression);
                        break;

                    case AstNodeTypes.PrintStatement:
                        break;

                    case AstNodeTypes.UnaryOperator:
                        break;

                    case AstNodeTypes.BinaryOperator:
                        break;

                    case AstNodeTypes.NumberLiteral:
                        break;

                    case AstNodeTypes.StringLiteral:
                        break;

                    case AstNodeTypes.Variable:
                        break;
                }
            };
        }

        private void EmitLocalVar(string varName, string typeName)
        {
            var typeDef = _knownTypes[typeName];
            var local = new VariableDefinition(typeDef);
            
            _mainMethod.Body.Variables.Add(local);
        }

        private TypeReference ResolveTypeDefinition(string metadataName)
        {
            var foundTypes = _assemblies.SelectMany(a => a.Modules)
                                        .SelectMany(m => m.Types)
                                        .Where(t => t.FullName == metadataName)
                                        .ToArray();

            return foundTypes.Length switch
            {
                1 => foundTypes[0],
                0 => throw new Exception($"Type '{metadataName}' not found."),
                _ => throw new Exception($"Ambiguous type '{metadataName}'.")
            };
        }

        private MethodReference ResolveMethod(string typeName, string methodName, string[] parameterTypeNames)
        {
            var foundTypes = _assemblies.SelectMany(a => a.Modules)
                                        .SelectMany(m => m.Types)
                                        .Where(t => t.FullName == typeName)
                                        .ToArray();

            switch (foundTypes.Length)
            {
                case 1:
                {
                    var foundType = foundTypes[0];
                    var methods = foundType.Methods.Where(m => m.Name == methodName);

                    foreach (var method in methods)
                    {
                        if (method.Parameters.Count != parameterTypeNames.Length)
                            continue;

                        var allParamsMatch = true;
                        for (var i = 0; i < parameterTypeNames.Length; i++)
                        {
                            if (method.Parameters[i].ParameterType.FullName == parameterTypeNames[i])
                                continue;

                            allParamsMatch = false;
                            break;
                        }

                        if (!allParamsMatch)
                            continue;

                        return _assemblyDef.MainModule.ImportReference(method);
                    }

                    throw new Exception($"Method '{typeName}.{methodName}' not found.");
                }
                case 0:
                    throw new Exception($"Type '{typeName}' not found.");
                default:
                    throw new Exception($"Ambiguous type '{typeName}'.");
            }
        }
    }
}
