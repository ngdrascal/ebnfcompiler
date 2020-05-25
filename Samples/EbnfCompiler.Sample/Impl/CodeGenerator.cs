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
      private MethodDefinition _mainMethod;
      private Dictionary<string, TypeReference> _knownTypes;
      private readonly Dictionary<string, VariableDefinition> _localVarDefs = new Dictionary<string, VariableDefinition>();
      private readonly Dictionary<string, MethodReference> _printMethods = new Dictionary<string, MethodReference>();
      private MethodReference _writeLineMethod;

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

         _printMethods.Add("string", ResolveMethod("System.Console", "Write", new[] { "System.String" }));
         _printMethods.Add("number", ResolveMethod("System.Console", "Write", new[] { "System.Single" }));

         _writeLineMethod = ResolveMethod("System.Console", "WriteLine", new string[0]);

         foreach (var stmt in rootNode.Statements)
            TraverseTree(stmt, _mainMethod.Body);

         _mainMethod.Body.GetILProcessor().Emit(OpCodes.Ret);

         _assemblyDef.Write(output);
      }

      private void TraverseTree(IAstNode rootNode, MethodBody methodBody)
      {
         var stack = new Stack<StackElementBase>();

         var traverser = new AstTraverser();

         traverser.ProcessNode += (node) =>
         {
            switch (node.AstNodeType)
            {
               case AstNodeTypes.VarStatement:
                  var varType = _knownTypes[node.AsVarStatement().Variable.TypeName];
                  var varDef = new VariableDefinition(varType);
                  methodBody.Variables.Add(varDef);

                  _localVarDefs.Add(node.AsVarStatement().Variable.Name, varDef);

                  var varStmtNode = new VarStmtElement()
                  {
                     VariableDef = varDef
                  };
                  stack.Push(varStmtNode);
                  break;

               case AstNodeTypes.PrintStatement:
                  var printStmtElement = new PrintStmtElement();
                  stack.Push(printStmtElement);
                  break;

               case AstNodeTypes.PrintExpression:
                  var printExprElement = new PrintExprElement();
                  stack.Push(printExprElement);
                  break;

               case AstNodeTypes.UnaryOperator:
                  var unaryOpNode = new UnaryOperatorElement()
                  {
                     Operator = node.AsUnaryOp().Operator
                  };
                  stack.Push(unaryOpNode);
                  break;

               case AstNodeTypes.BinaryOperator:
                  var binaryOpElement = new BinaryOperatorElement()
                  {
                     Operator = node.AsBinaryOp().Operator,
                     TypeName = node.AsBinaryOp().TypeName
                  };
                  stack.Push(binaryOpElement);
                  break;

               case AstNodeTypes.NumberLiteral:
                  var numValue = node.AsNumberLit().Value;
                  methodBody.GetILProcessor().Emit(OpCodes.Ldc_R4, numValue);

                  var numLitElement = new NumberLiteralElement();
                  stack.Push(numLitElement);
                  break;

               case AstNodeTypes.StringLiteral:
                  var strValue = node.AsStringLit().Value;
                  methodBody.GetILProcessor().Emit(OpCodes.Ldstr, strValue);

                  var strLitElement = new StringLiteralElement();
                  stack.Push(strLitElement);
                  break;

               case AstNodeTypes.VarReference:
                  var localVarDef = _localVarDefs[node.AsVarReferene().Name];
                  methodBody.GetILProcessor().Emit(OpCodes.Ldloc, localVarDef);

                  var varRef = new VarReferenceElement()
                  {
                     TypeName = node.AsVarReferene().TypeName
                  };
                  stack.Push(varRef);
                  break;
            }
         };

         traverser.PostProcessNode += (nodeType) =>
            {
               var ilProcessor = methodBody.GetILProcessor();

               switch (nodeType)
               {
                  case AstNodeTypes.VarStatement:
                     stack.Pop();  // expression
                     var varStmtElement = (VarStmtElement)stack.Pop();
                     var varDef = varStmtElement.VariableDef;
                     ilProcessor.Emit(OpCodes.Stloc, varDef);
                     break;

                  case AstNodeTypes.PrintStatement:
                     ilProcessor.Emit(OpCodes.Call, _writeLineMethod);
                     break;

                  case AstNodeTypes.PrintExpression:
                     var printExpr = stack.Pop();
                     switch (printExpr)
                     {
                        case UnaryOperatorElement _:
                           ilProcessor.Emit(OpCodes.Call, _printMethods["number"]);
                           break;
                        case BinaryOperatorElement binaryOp:
                           ilProcessor.Emit(OpCodes.Call, _printMethods[binaryOp.TypeName]);
                           break;
                        case NumberLiteralElement _:
                           ilProcessor.Emit(OpCodes.Call, _printMethods["number"]);
                           break;
                        case StringLiteralElement _:
                           ilProcessor.Emit(OpCodes.Call, _printMethods["string"]);
                           break;
                        case VarReferenceElement variable:
                           ilProcessor.Emit(OpCodes.Call, _printMethods[variable.TypeName]);
                           break;
                     }
                     break;

                  case AstNodeTypes.UnaryOperator:
                     stack.Pop();  // operand
                     var unaryOpElement = (UnaryOperatorElement)stack.Pop();
                     switch (unaryOpElement.Operator)
                     {
                        case UnaryOperators.Plus:
                           break;
                        case UnaryOperators.Minus:
                           ilProcessor.Emit(OpCodes.Neg);
                           break;
                     }
                     break;

                  case AstNodeTypes.BinaryOperator:
                     stack.Pop();  // right operand
                     stack.Pop();  // left operand
                     var binaryOpElement = (BinaryOperatorElement)stack.Peek();
                     switch (binaryOpElement.Operator)
                     {
                        case BinaryOperators.Add:
                           ilProcessor.Emit(OpCodes.Add);
                           break;
                        case BinaryOperators.Subtract:
                           ilProcessor.Emit(OpCodes.Sub);
                           break;
                        case BinaryOperators.Multiply:
                           ilProcessor.Emit(OpCodes.Mul);
                           break;
                        case BinaryOperators.Divide:
                           ilProcessor.Emit(OpCodes.Div);
                           break;
                     }
                     break;

                  case AstNodeTypes.NumberLiteral:
                     break;

                  case AstNodeTypes.StringLiteral:
                     break;

                  case AstNodeTypes.VarReference:
                     break;
               }
            };

         traverser.Traverse(rootNode);
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
