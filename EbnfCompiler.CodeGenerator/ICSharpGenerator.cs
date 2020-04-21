using System;
using System.Collections.Generic;
using EbnfCompiler.AST;
using Microsoft.Extensions.Logging;

namespace EbnfCompiler.CodeGenerator
{
   public interface ICSharpGenerator
   {
      void Calculate(IProductionInfo productionInfo);
   }

   internal class Context
   {
      public Context(AstNodeType nodeType)
      {
         NodeType = nodeType;
      }

      public AstNodeType NodeType { get; }

      public override string ToString()
      {
         return NodeType.ToString();
      }
   }

   public class IcSharpGenerator : ICSharpGenerator
   {
      private readonly IAstTraverser _traverser;
      private readonly ILogger _log;
      private readonly Stack<Context> _stack;
      private int _indentLevel;
      public string Output { get; private set; }

      public IcSharpGenerator(IAstTraverser traverser, ILogger log)
      {
         _traverser = traverser;
         _log = log;
         _stack = new Stack<Context>();
      }

      public void Calculate(IProductionInfo productionInfo)
      {
         _traverser.PreProcess += node =>
         {
            _stack.Push(new Context(node.AstNodeType));

            switch (node.AstNodeType)
            {
               case AstNodeType.Expression:
                  break;

               case AstNodeType.Term:
                  break;

               case AstNodeType.Factor:
                  break;

               case AstNodeType.ProdRef:
                  AppendLine($"Parse{((IProdRefNode) node).ProdName}();");
                  break;

               case AstNodeType.Terminal:
                  AppendLine($"Match(\"{((ITerminalNode) node).TermName}\");");
                  break;

               case AstNodeType.Action:
                  AppendLine($"semantics.{((IActionNode) node).ActionName}();");
                  break;

               case AstNodeType.Paren:
                  break;

               case AstNodeType.Option:
                  AppendLine($"if (node.FirstSet.Includes(\"\"))");
                  AppendLine("{");
                  Indent();
                  break;

               case AstNodeType.KleeneStar:
                  AppendLine($"while (node.FirstSet.Includes(\"\"))");
                  AppendLine("{");
                  break;
            }
         };

         _traverser.PostProcess += () =>
         {
            var context = _stack.Pop();
            if (context.NodeType == AstNodeType.Option || context.NodeType == AstNodeType.KleeneStar)
            {
               Outdent();
               AppendLine("}");
            }
         };

         Indent();
         AppendLine($"void Parse{productionInfo.Name}()");
         AppendLine("{");

         _traverser.Traverse(productionInfo.RightHandSide);

         AppendLine("}");
         Outdent();
      }

      private void Indent()
      {
         _indentLevel++;
      }

      private void Outdent()
      {
         _indentLevel--;
      }

      private void Append(string s)
      {
         Output += s;
      }

      private void AppendLine(string s = "")
      {
         var indent = new string(' ', _indentLevel);
         Append($"{indent}{s}");

         _log.LogDebug(Output);
         Output = String.Empty;
      }
   }
}
