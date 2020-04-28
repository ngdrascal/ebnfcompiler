using System.Collections.Generic;
using System.Linq;
using EbnfCompiler.AST;
using EbnfCompiler.AST.Impl;
using Microsoft.Extensions.Logging;

namespace EbnfCompiler.CodeGenerator
{
   internal class ContextBase
   {
      public ContextBase(AstNodeType nodeType)
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
      private readonly ISyntaxNode _syntaxTree;
      private readonly IReadOnlyCollection<ITokenDefinition> _tokens;
      private readonly IAstTraverser _traverser;
      private readonly ILogger _log;
      private readonly Stack<ContextBase> _stack;
      private int _indentLevel;
      private string Output { get; set; }

      public CSharpGenerator(ISyntaxNode syntaxTree, IReadOnlyCollection<ITokenDefinition> tokens,
                              IAstTraverser traverser, ILogger log)
      {
         _syntaxTree = syntaxTree;
         _tokens = tokens;
         _traverser = traverser;
         _log = log;
         _stack = new Stack<ContextBase>();
      }

      public void Run()
      {
         _traverser.ProcessNode += ProcessNode;
         _traverser.PostProcessNode += PostProcessNode;

         _traverser.Traverse(_syntaxTree);
      }

      private void ProcessNode(IAstNode node)
      {
         ContextBase context;
         switch (node.AstNodeType)
         {
            case AstNodeType.Syntax:
               context = new ContextBase(node.AstNodeType);
               _stack.Push(context);

               PrintUsings();
               PrintNamespaceHeader();
               PrintMatchMethod();

               PrintParseGoal(node.AsSyntax().FirstStatement.ProdName,
                              node.AsSyntax().PreActionNode?.ActionName,
                              node.AsSyntax().PostActionNode?.ActionName);
               break;

            case AstNodeType.Statement:
               context = new ContextBase(node.AstNodeType);
               _stack.Push(context);

               context.Properties.Add("PreActionName", node.AsStatement().PreActionNode?.ActionName);
               context.Properties.Add("PostActionName", node.AsStatement().PostActionNode?.ActionName);

               PrintMethodHeader(node.AsStatement().ProdName, context.Properties["PreActionName"]?.ToString());
               break;

            case AstNodeType.Expression:
               _stack.Push(new ContextBase(node.AstNodeType));
               _stack.Peek().GenerateSwitch = node.AsExpression().TermCount > 1;

               if (node.AsExpression().PreActionNode != null)
                  PrintAction(node.AsExpression().PreActionNode.ActionName);

               if (node.AsExpression().TermCount > 1)
                  PrintTermSwitch();
               break;

            case AstNodeType.Term:
               context = new ContextBase(node.AstNodeType);
               context.GenerateSwitch = _stack.Peek().GenerateSwitch;
               _stack.Push(context);

               if (context.GenerateSwitch)
               {
                  PrintTermCase(node.FirstSet);
                  Indent();
               }
               break;

            case AstNodeType.Factor:
               _stack.Push(new ContextBase(node.AstNodeType));

               if (node.AsFactor().PostActionNode != null)
                  PrintAction(node.AsFactor().PostActionNode.ActionName);

               break;

            case AstNodeType.ProdRef:
               _stack.Push(new ContextBase(node.AstNodeType));
               PrintProdRef(node.AsProdRef().ProdName);
               break;

            case AstNodeType.Terminal:
               _stack.Push(new ContextBase(node.AstNodeType));
               PrintMatchTerminal(node.AsTerminal().TermName);
               break;

            case AstNodeType.Action:
               _stack.Push(new ContextBase(node.AstNodeType));
               //PrintAction(node.AsActionNode().ActionName);
               break;

            case AstNodeType.Paren:
               _stack.Push(new ContextBase(node.AstNodeType));
               break;

            case AstNodeType.Option:
               _stack.Push(new ContextBase(node.AstNodeType));
               PrintOption(node.FirstSet);
               break;

            case AstNodeType.KleeneStar:
               _stack.Push(new ContextBase(node.AstNodeType));
               PrintKleene(node.FirstSet);
               break;
         }
      }

      private void PostProcessNode()
      {
         var context = _stack.Pop();

         switch (context.NodeType)
         {
            case AstNodeType.Syntax:
               PrintNamespaceFooter();
               break;

            case AstNodeType.Statement:
               PrintMethodFooter(context.Properties["PostActionName"]?.ToString());
               break;

            case AstNodeType.Expression:
               break;

            case AstNodeType.Term:
               if (context.GenerateSwitch)
               {
                  PrintLine("break;");
                  Outdent();
                  PrintLine("}");
               }
               break;

            case AstNodeType.Factor:
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
         PrintLine("using System.Collections.Generic;");
         PrintLine("using System.Linq;");
         PrintLine("using EbnfCompiler.AST;");
         PrintLine("using EbnfCompiler.Compiler;");
         PrintLine("using EbnfCompiler.Scanner;");
         PrintLine();
      }

      private void PrintNamespaceHeader()
      {
         PrintLine("namespace EbnfCompiler.Parser");
         PrintLine("{");
         Indent();
      }

      private void PrintNamespaceFooter()
      {
         Outdent();
         PrintLine("}");
      }

      private void PrintMatchMethod()
      {
         PrintLine("private void Match(TokenKind tokenKind)");
         PrintLine("{");
         Indent();
         PrintLine("if (_scanner.CurrentToken.TokenKind != tokenKind)");
         Indent();
         PrintLine("throw new SyntaxErrorException(\"Expecting: {tokenKind}\", _scanner.CurrentToken);");
         Outdent();
         Outdent();
         PrintLine("}");
      }

      private void PrintParseGoal(string prodName, string preActionName, string postActionName)
      {
         PrintLine();
         PrintLine($"public void ParseGoal()");
         PrintLine("{");
         Indent();

         if (!string.IsNullOrEmpty(preActionName))
            PrintAction(preActionName);
         
         PrintProdRef(prodName);
         
         if (!string.IsNullOrEmpty(postActionName))
            PrintAction(postActionName);

         PrintMethodFooter(postActionName);
      }

      private void PrintMethodHeader(string name, string preActionName)
      {
         PrintLine();
         PrintLine($"private void Parse{name}()");
         PrintLine("{");
         Indent();
         if (!string.IsNullOrEmpty(preActionName))
            PrintAction(preActionName);
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
         PrintLine($"Parse{name}();");
      }

      private void PrintMatchTerminal(string name)
      {
         var tokenDef = _tokens.FirstOrDefault(p => p.Image.Equals(name))?.Definition;
         if (tokenDef == null)
            throw new SemanticErrorException($"Token definition for \"{name}\" not found.");

         PrintLine($"Match(TokenKind.{tokenDef});");
         PrintLine("_scanner.Advance()");
      }

      private void PrintAction(string actionName)
      {
         PrintLine($"semantics.{actionName}();");
      }

      private void PrintOption(ITerminalSet firstSet)
      {
         if (firstSet.AsEnumerable().Count(p => p != "$EPSILON$") > 1)
         {
            PrintFirstSet(firstSet);

            PrintLine("if (startTokens.Contains(_scanner.CurrentToken.TokenKind))");
         }
         else
         {
            var defs = SetAsTokenDefinitions(firstSet);
            PrintLine($"if (_scanner.CurrentToken.TokenKind == TokenKind.{defs.First()})");
         }

         PrintLine("{");
         Indent();
      }

      private void PrintKleene(ITerminalSet firstSet)
      {
         if (firstSet.AsEnumerable().Count() > 1)
         {
            PrintFirstSet(firstSet);
            PrintLine("while (startTokens.Contains(_scanner.CurrentToken.TokenKind))");
         }
         else
         {
            var defs = SetAsTokenDefinitions(firstSet);
            PrintLine($"while (_scanner.CurrentToken.TokenKind == TokenKind.{defs.First()})");
         }

         PrintLine("{");
         Indent();
      }

      private void PrintFirstSet(ITerminalSet firstSet)
      {
         var tokens = SetAsTokenDefinitions(firstSet);

         PrintLine("var startTokens = new[]");
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

      private void Append(string s)
      {
         Output += s;
      }

      private void PrintLine(string s = "")
      {
         var indent = new string(' ', _indentLevel);
         Append($"{indent}{s}");

         if (Output.Equals(string.Empty))
            Output = " ";

         _log.LogDebug(Output);
         Output = string.Empty;
      }
   }
}
