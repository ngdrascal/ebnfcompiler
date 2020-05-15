using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using EbnfCompiler.AST;
using EbnfCompiler.AST.Impl;

namespace EbnfCompiler.CodeGenerator
{
   internal class Context
   {
      public Context(AstNodeType nodeType)
      {
         NodeType = nodeType;
         Properties = new Dictionary<string, object>();
      }

      public AstNodeType NodeType { get; }

      public Dictionary<string, object> Properties { get; }

      public override string ToString()
      {
         return NodeType.ToString();
      }

      public bool GenerateSwitch { get; set; }
   }

   public class CSharpGenerator : ICodeGenerator
   {
      private readonly IAstTraverser _traverser;
      private readonly StreamWriter _streamWriter;

      private IReadOnlyCollection<ITokenDefinition> _tokens;
      private readonly Stack<Context> _stack;
      private int _indentLevel;

      public CSharpGenerator(IAstTraverser traverser, StreamWriter streamWriter)
      {
         _traverser = traverser;
         _streamWriter = streamWriter;
         _traverser.ProcessNode += ProcessNode;
         _traverser.PostProcessNode += PostProcessNode;

         _stack = new Stack<Context>();
      }

      public void Run(IRootNode rootNode)
      {
         _tokens = rootNode.TokenDefs;
         _traverser.Traverse(rootNode.Syntax);
      }

      private void ProcessNode(IAstNode node)
      {
         Context context;
         switch (node.AstNodeType)
         {
            case AstNodeType.Syntax:
               context = new Context(node.AstNodeType);
               _stack.Push(context);

               PrintUsings();
               PrintNamespaceHeader();
               PrintClassHeader();
               PrintClassProperties();
               PrintConstructor();
               PrintMatchMethod();

               PrintParseGoal(node.AsSyntax().Statements.First().ProdName,
                              node.AsSyntax().PreActionNode?.ActionName,
                              node.AsSyntax().PostActionNode?.ActionName);
               break;

            case AstNodeType.Statement:
               context = new Context(node.AstNodeType);
               _stack.Push(context);

               context.Properties.Add("PreActionName", node.AsStatement().PreActionNode?.ActionName);
               context.Properties.Add("PostActionName", node.AsStatement().PostActionNode?.ActionName);

               PrintMethodHeader(node.AsStatement().ProdName, context.Properties["PreActionName"]?.ToString());
               break;

            case AstNodeType.Expression:
               _stack.Push(new Context(node.AstNodeType));
               _stack.Peek().GenerateSwitch = node.AsExpression().TermCount > 1;

               if (node.AsExpression().PreActionNode != null)
                  PrintAction(node.AsExpression().PreActionNode.ActionName);

               if (node.AsExpression().TermCount > 1)
                  PrintTermSwitch();
               break;

            case AstNodeType.Term:
               context = new Context(node.AstNodeType);
               context.GenerateSwitch = _stack.Peek().GenerateSwitch;
               _stack.Push(context);

               if (context.GenerateSwitch)
               {
                  PrintTermCase(node.FirstSet);
                  Indent();
               }
               break;

            case AstNodeType.Factor:
               context = new Context(node.AstNodeType);
               context.Properties.Add("PostActionName", node.AsFactor().PostActionNode?.ActionName);
               _stack.Push(context);

               if (node.AsFactor().PreActionNode != null)
                  PrintAction(node.AsFactor().PreActionNode.ActionName);
               break;

            case AstNodeType.ProdRef:
               _stack.Push(new Context(node.AstNodeType));
               PrintProdRef(node.AsProdRef().ProdName);
               break;

            case AstNodeType.Terminal:
               _stack.Push(new Context(node.AstNodeType));
               PrintMatchTerminal(node.AsTerminal().TermName);
               break;

            case AstNodeType.Action:
               _stack.Push(new Context(node.AstNodeType));
               break;

            case AstNodeType.Paren:
               _stack.Push(new Context(node.AstNodeType));
               break;

            case AstNodeType.Option:
               _stack.Push(new Context(node.AstNodeType));
               PrintOption(node.FirstSet, node.NodeId);
               break;

            case AstNodeType.KleeneStar:
               _stack.Push(new Context(node.AstNodeType));
               PrintKleene(node.FirstSet, node.NodeId);
               break;
         }
      }

      private void PostProcessNode()
      {
         var context = _stack.Pop();

         switch (context.NodeType)
         {
            case AstNodeType.Syntax:
               PrintClassFooter();
               PrintNamespaceFooter();
               break;

            case AstNodeType.Statement:
               PrintMethodFooter(context.Properties["PostActionName"]?.ToString());
               break;

            case AstNodeType.Expression:
               if (context.GenerateSwitch)
               {
                  Outdent();
                  PrintLine("}");
               }
               break;

            case AstNodeType.Term:
               if (context.GenerateSwitch)
               {
                  PrintLine("break;");
                  Outdent();
               }
               break;

            case AstNodeType.Factor:
               if (context.Properties["PostActionName"] != null)
                  PrintAction(context.Properties["PostActionName"].ToString());
               break;

            case AstNodeType.ProdRef:
               break;

            case AstNodeType.Terminal:
               break;

            case AstNodeType.Action:
               break;

            case AstNodeType.Paren:
               break;

            case AstNodeType.Option:
               Outdent();
               PrintLine("}");
               break;

            case AstNodeType.KleeneStar:
               Outdent();
               PrintLine("}");
               break;
         }
      }

      private void PrintUsings()
      {
         PrintLine("using System.Linq;");
         // PrintLine("using EbnfCompiler.AST;");
         // PrintLine("using EbnfCompiler.Compiler;");
         // PrintLine("using EbnfCompiler.Scanner;");
         // PrintLine();
      }

      private void PrintNamespaceHeader()
      {
         PrintLine("namespace EbnfCompiler.Sample");
         PrintLine("{");
         Indent();
      }

      private void PrintNamespaceFooter()
      {
         Outdent();
         PrintLine("}");
      }

      private void PrintClassHeader()
      {
         PrintLine("public partial class Parser");
         PrintLine("{");
         Indent();
      }

      private void PrintClassProperties()
      {
         PrintLine("private readonly IScanner _scanner;");
         PrintLine("private readonly IAstBuilder _astBuilder;");
      }

      private void PrintClassFooter()
      {
         Outdent();
         PrintLine("}");
      }

      private void PrintConstructor()
      {
         PrintLine();
         PrintLine("public Parser(IScanner scanner, IAstBuilder astBuilder)");
         PrintLine("{");
         Indent();
         PrintLine("_scanner = scanner;");
         PrintLine("_astBuilder = astBuilder;");
         Outdent();
         PrintLine("}");
      }

      private void PrintMatchMethod()
      {
         PrintLine();
         PrintLine("private void Match(TokenKind tokenKind)");
         PrintLine("{");
         Indent();
         PrintLine("if (_scanner.CurrentToken.TokenKind != tokenKind)");
         Indent();
         PrintLine("throw new SyntaxErrorException(tokenKind, _scanner.CurrentToken);");
         Outdent();
         Outdent();
         PrintLine("}");
      }

      private void PrintParseGoal(string prodName, string preActionName, string postActionName)
      {
         PrintLine();
         PrintLine("public IRootNode ParseGoal()");
         PrintLine("{");
         Indent();

         if (!string.IsNullOrEmpty(preActionName))
            PrintAction(preActionName);

         PrintProdRef(prodName);

         PrintLine("Match(TokenKind.Eof);");

         if (!string.IsNullOrEmpty(postActionName))
            PrintAction(postActionName);

         PrintLine("return BuildRootNode();");

         PrintMethodFooter(postActionName);
      }

      private void PrintMethodHeader(string name, string preActionName)
      {
         PrintLine();
         PrintLine($"private void Parse{CamelCaseMethodName(name)}()");
         PrintLine("{");
         Indent();
         if (!string.IsNullOrEmpty(preActionName))
            PrintAction(preActionName);
      }

      private string CamelCaseMethodName(string name)
      {
         var result = String.Empty;
         var toUpperNexChar = true;

         foreach (var ch in name.Substring(1, name.Length - 2))
         {
            if (ch == '-')
            {
               toUpperNexChar = true;
               continue;
            }
            var chStr = ch.ToString();
            result += toUpperNexChar ? chStr.ToUpper() : chStr;
            toUpperNexChar = false;
         }

         return result;
      }
      
      private void PrintMethodFooter(string postActionName)
      {
         if (!string.IsNullOrEmpty(postActionName))
            PrintAction(postActionName);
         Outdent();
         PrintLine("}");
      }

      private void PrintProdRef(string name)
      {
         PrintLine($"Parse{CamelCaseMethodName(name)}();");
      }

      private void PrintMatchTerminal(string name)
      {
         var tokenDef = _tokens.FirstOrDefault(p => p.Image.Equals(name))?.Definition;
         if (tokenDef == null)
            throw new SemanticErrorException($"Token definition for \"{name}\" not found.");

         PrintLine($"Match(TokenKind.{tokenDef});");
         PrintLine("_scanner.Advance();");
         PrintLine();
      }

      private void PrintAction(string actionName)
      {
         var action = CamelCaseMethodName(actionName);
         PrintLine($"_astBuilder.{action}(_scanner.CurrentToken);");
      }

      private void PrintOption(ITerminalSet firstSet, string nodeId)
      {
         if (firstSet.AsEnumerable().Count(p => p != "$EPSILON$") > 1)
         {
            PrintFirstSet(firstSet, nodeId);
            PrintLine($"if (firstSetOf{nodeId}.Contains(_scanner.CurrentToken.TokenKind))");
         }
         else
         {
            var defs = SetAsTokenDefinitions(firstSet);
            PrintLine($"if (_scanner.CurrentToken.TokenKind == TokenKind.{defs.First()})");
         }

         PrintLine("{");
         Indent();
      }

      private void PrintKleene(ITerminalSet firstSet, string nodeId)
      {
         if (firstSet.AsEnumerable().Count() > 1)
         {
            PrintFirstSet(firstSet, nodeId);
            PrintLine($"while (firstSetOf{nodeId}.Contains(_scanner.CurrentToken.TokenKind))");
         }
         else
         {
            var defs = SetAsTokenDefinitions(firstSet);
            PrintLine($"while (_scanner.CurrentToken.TokenKind == TokenKind.{defs.First()})");
         }

         PrintLine("{");
         Indent();
      }

      private void PrintFirstSet(ITerminalSet firstSet, string nodeId)
      {
         var tokens = SetAsTokenDefinitions(firstSet);

         PrintLine($"var firstSetOf{nodeId} = new[]");
         PrintLine("{");
         Indent();
         PrintLine(string.Join(", ", tokens.Select(s => "TokenKind." + s)));
         Outdent();
         PrintLine("};");
      }

      private List<string> SetAsTokenDefinitions(ITerminalSet firstSet)
      {
         var tokens = new List<string>();
         foreach (var token in firstSet.AsEnumerable().Where(t => t != "$EPSILON$"))
         {
            var tokenDef = _tokens.FirstOrDefault(p => p.Image.Equals(token))?.Definition;
            if (tokenDef == null)
               throw new SemanticErrorException($"Token definition for \"{token}\" not found.");

            tokens.Add(tokenDef);
         }

         return tokens;
      }

      private void PrintTermSwitch()
      {
         PrintLine("switch (_scanner.CurrentToken.TokenKind)");
         PrintLine("{");
         Indent();
      }

      private void PrintTermCase(ITerminalSet firstSet)
      {
         var tokens = SetAsTokenDefinitions(firstSet);
         foreach (var token in tokens)
         {
            PrintLine($"case TokenKind.{token}:");
         }
      }

      private void Indent()
      {
         _indentLevel += 3;
      }

      private void Outdent()
      {
         _indentLevel -= 3;
      }

      private void PrintLine()
      {
         _streamWriter.WriteLine();
      }

      private void PrintLine(string s)
      {
         var indent = new string(' ', _indentLevel);
         _streamWriter.WriteLine($"{indent}{s}");
      }
   }
}
