using System;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using EbnfCompiler.AST.Impl;
using EbnfCompiler.Compiler;

namespace EbnfCompiler.AST.UnitTests
{
   [ExcludeFromCodeCoverage]
   public class SyntaxBuilder : IDisposable
   {
      private readonly IDebugTracer _tracer;
      private bool _isDisposed;
      private Syntax _root;

      public SyntaxBuilder(IDebugTracer tracer)
      {
         _tracer = tracer;
      }

      public void Syntax(params Statement[] statements)
      {
         _root = new Syntax(_tracer, statements);
      }

      public Statement Statement(string prodName, Expression expression)
      {
         return new Statement(_tracer, prodName, expression);
      }

      public Expression Expression(params Term[] terms)
      {
         return new Expression(_tracer, terms);
      }

      public Term Term(params Factor[] factors)
      {
         return new Term(_tracer, factors);
      }

      public Factor Factor(IFactExpr factExpr)
      {
         return new Factor(_tracer, factExpr);
      }

      public Factor Factor(string processName, string postProcessName, IFactExpr factExpr)
      {
         return new Factor(_tracer, factExpr)
         {
            PreActionName = processName,
            PostActionName = postProcessName
         };
      }

      public Terminal Terminal(string image)
      {
         return new Terminal(image, _tracer);
      }

      public ProdRef ProdRef(string name)
      {
         return new ProdRef(_tracer, name);
      }

      public Paren Paren(Expression expression)
      {
         return new Paren(_tracer, expression);
      }

      public Option Option(Expression expression)
      {
         return new Option(_tracer, expression);
      }

      public Kleene Kleene(Expression expression)
      {
         return new Kleene(_tracer, expression);
      }

      public string BuildCode()
      {
         return _root.BuildCode();
      }

      public ISyntaxNode BuildTree()
      {
         return _root.BuildTree();
      }

      public void Dispose()
      {
         Dispose(true);
         GC.SuppressFinalize(this);
      }

      private void Dispose(bool disposing)
      {
         if (_isDisposed) return;

         if (disposing)
         {
         }

         _isDisposed = true;
      }
   }

   [ExcludeFromCodeCoverage]
   public class Syntax
   {
      private readonly IDebugTracer _tracer;
      private readonly Statement[] _statements;
      private static int _counter;

      public Syntax(IDebugTracer tracer, params Statement[] statements)
      {
         _tracer = tracer;
         _statements = statements;
         NodeName = "syntax" + ++_counter;
      }

      public string NodeName { get; }

      public string BuildCode()
      {
         var sb = new StringBuilder();

         sb.Append($"var {NodeName} = new SyntaxNode(new Token(), tracer);").AppendLine();

         foreach (var stmt in _statements)
         {
            sb.Append(stmt.BuildCode()).AppendLine();
         }

         return sb.ToString();
      }

      public ISyntaxNode BuildTree()
      {
         var syntaxNode = new SyntaxNode(new Token(), _tracer);
         foreach (var stmt in _statements)
         {
            var stmtNode = stmt.BuildTree();
            syntaxNode.AppendStatement(stmtNode);
         }

         return syntaxNode;
      }
   }

   [ExcludeFromCodeCoverage]
   public class Statement
   {
      private readonly IDebugTracer _tracer;
      private readonly string _prodName;
      private readonly Expression _expression;
      private static int _counter;

      public Statement(IDebugTracer tracer, string prodName, Expression expression)
      {
         _tracer = tracer;
         _prodName = prodName;
         _expression = expression;
         NodeName = "stmt" + ++_counter;
      }

      public string NodeName { get; }

      public string BuildCode()
      {
         var sb = new StringBuilder();
         sb.Append($"var {NodeName} = new StatementNode(new Token(TokenKind.Identifier, \"{_prodName}\"), tracer);").AppendLine();
         sb.Append(_expression.BuildCode());
         sb.Append($"{NodeName}.Expression = {_expression.NodeName};").AppendLine();

         return sb.ToString();
      }

      public IStatementNode BuildTree()
      {
         var stmtNode = new StatementNode(new Token(TokenKind.Identifier, _prodName), _tracer);
         var exprNode = _expression.BuildTree();
         stmtNode.Expression = exprNode;

         return stmtNode;
      }
   }

   [ExcludeFromCodeCoverage]
   public class Expression
   {
      private readonly IDebugTracer _tracer;
      private readonly Term[] _terms;
      private static int _counter;

      public Expression(IDebugTracer tracer, params Term[] terms)
      {
         _tracer = tracer;
         _terms = terms;
         NodeName = "expr" + ++_counter;
      }

      public string NodeName { get; }

      public string BuildCode()
      {
         var sb = new StringBuilder();

         sb.Append($"var {NodeName} = new ExpressionNode(new Token(), tracer);").AppendLine();

         foreach (var term in _terms)
         {
            sb.Append(term.BuildCode());
            sb.Append($"{NodeName}.AppendTerm({term.NodeName});").AppendLine();
         }

         return sb.ToString();
      }

      public IExpressionNode BuildTree()
      {
         var exprNode = new ExpressionNode(new Token(), _tracer);

         foreach (var term in _terms)
         {
            var termNode = term.BuildTree();
            exprNode.AppendTerm(termNode);
         }

         return exprNode;
      }
   }

   [ExcludeFromCodeCoverage]
   public class Term
   {
      private readonly IDebugTracer _tracer;
      private readonly Factor[] _factors;
      private static int _counter;

      public Term(IDebugTracer tracer, params Factor[] factors)
      {
         _tracer = tracer;
         _factors = factors;
         NodeName = "term" + ++_counter;
      }

      public string NodeName { get; }

      public string BuildCode()
      {
         var sb = new StringBuilder();

         sb.Append($"var {NodeName} = new TermNode(new Token(), tracer);").AppendLine();

         foreach (var fact in _factors)
         {
            sb.Append(fact.BuildCode());
            sb.Append($"{NodeName}.AppendFactor({fact.NodeName});").AppendLine();
         }

         return sb.ToString();
      }

      public ITermNode BuildTree()
      {
         var termNode = new TermNode(new Token(), _tracer);
         foreach (var fact in _factors)
         {
            var factorNode = fact.BuildTree();
            termNode.AppendFactor(factorNode);
         }

         return termNode;
      }
   }

   [ExcludeFromCodeCoverage]
   public class Factor
   {
      private readonly IDebugTracer _tracer;
      private readonly IFactExpr _factExpr;
      private static int _counter;

      public Factor(IDebugTracer tracer, IFactExpr factExpr)
      {
         _tracer = tracer;
         _factExpr = factExpr;
         NodeName = "factor" + ++_counter;
      }

      public string NodeName { get; }
      public string PreActionName { get; set; }
      public string PostActionName { get; set; }

      public string BuildCode()
      {
         var sb = new StringBuilder();

         sb.Append($"var {NodeName} = new FactorNode(new Token(), tracer);").AppendLine();
         if (!string.IsNullOrEmpty(PreActionName))
         {
            var action = new Action(PreActionName);
            sb.Append(action.BuildCode());
            sb.Append($"{NodeName}.PreAction = {action.NodeName};");
         }

         if (!string.IsNullOrEmpty(PostActionName))
         {
            var action = new Action(PostActionName);
            sb.Append(action.BuildCode());
            sb.Append($"{NodeName}.PreAction = {action.NodeName};");
         }

         sb.Append(_factExpr.BuildCode());
         sb.Append($"{NodeName}.FactorExpr = {_factExpr.NodeName};").AppendLine();

         return sb.ToString();
      }

      public IFactorNode BuildTree()
      {
         var factorNode = new FactorNode(new Token(), _tracer)
         {
            PreActionNode = string.IsNullOrEmpty(PreActionName)
               ? null
               : new ActionNode(new Token(TokenKind.Action, PreActionName), _tracer),
            PostActionNode = string.IsNullOrEmpty(PostActionName)
               ? null
               : new ActionNode(new Token(TokenKind.Action, PostActionName), _tracer)
         };

         var factExprNode = _factExpr.BuildTree();
         factorNode.FactorExpr = factExprNode;

         return factorNode;
      }
   }

   public interface IFactExpr
   {
      public string NodeName { get; }

      string BuildCode();

      IAstNode BuildTree();
   }

   [ExcludeFromCodeCoverage]
   public class Terminal : IFactExpr
   {
      private readonly IDebugTracer _tracer;
      private readonly string _image;
      private static int _counter;

      public Terminal(string image, IDebugTracer tracer)
      {
         _tracer = tracer;
         _image = image;
         NodeName = "terminal" + ++_counter;
      }

      public string NodeName { get; }

      public string BuildCode()
      {
         return $"var {NodeName} = new TerminalNode(new Token(TokenKind.String, \"{_image}\"), tracer);\n";
      }

      public IAstNode BuildTree()
      {
         return new TerminalNode(new Token(TokenKind.String, _image), _tracer);
      }
   }

   [ExcludeFromCodeCoverage]
   public class ProdRef : IFactExpr
   {
      private readonly IDebugTracer _tracer;
      private readonly string _name;
      private static int _counter;

      public ProdRef(IDebugTracer tracer, string name)
      {
         _tracer = tracer;
         _name = name;
         NodeName = "prodRef" + ++_counter;
      }

      public string NodeName { get; }

      public string BuildCode()
      {
         return $"var {NodeName} = new ProdRefNode(new Token(TokenKind.String, \"{_name}\"), tracer);\n";
      }

      public IAstNode BuildTree()
      {
         return new ProdRefNode(new Token(TokenKind.String, _name), _tracer);
      }
   }

   [ExcludeFromCodeCoverage]
   public class Paren : IFactExpr
   {
      private readonly IDebugTracer _tracer;
      private readonly Expression _expr;
      private static int _counter;

      public Paren(IDebugTracer tracer, Expression expr)
      {
         _tracer = tracer;
         _expr = expr;
         NodeName = "paren" + ++_counter;
      }

      public string NodeName { get; }

      public string BuildCode()
      {
         var sb = new StringBuilder();

         sb.Append($"var {NodeName} = new ParenNode(new Token(), tracer);").AppendLine();
         sb.Append(_expr.BuildCode());
         sb.Append($"{NodeName}.Expression = {_expr.NodeName};").AppendLine();

         return sb.ToString();
      }

      public IAstNode BuildTree()
      {
         var parenNode = new ParenNode(new Token(), _tracer);
         var exprNode = _expr.BuildTree();
         parenNode.Expression = exprNode;
         return parenNode;
      }
   }

   [ExcludeFromCodeCoverage]
   public class Option : IFactExpr
   {
      private readonly IDebugTracer _tracer;
      private readonly Expression _expr;
      private static int _counter;

      public Option(IDebugTracer tracer, Expression expr)
      {
         _tracer = tracer;
         _expr = expr;
         NodeName = "option" + ++_counter;
      }

      public string NodeName { get; }

      public string BuildCode()
      {
         var sb = new StringBuilder();

         sb.Append($"var {NodeName} = new OptionNode(new Token(), tracer);").AppendLine();
         sb.Append(_expr.BuildCode());
         sb.Append($"{NodeName}.Expression = {_expr.NodeName};").AppendLine();

         return sb.ToString();
      }

      public IAstNode BuildTree()
      {
         var optionNode = new OptionNode(new Token(), _tracer);
         var exprNode = _expr.BuildTree();
         optionNode.Expression = exprNode;
         return optionNode;
      }
   }

   [ExcludeFromCodeCoverage]
   public class Kleene : IFactExpr
   {
      private readonly IDebugTracer _tracer;
      private readonly Expression _expr;
      private static int _counter;

      public Kleene(IDebugTracer tracer, Expression expr)
      {
         _tracer = tracer;
         _expr = expr;
         NodeName = "kleene" + ++_counter;
      }

      public string NodeName { get; }

      public string BuildCode()
      {
         var sb = new StringBuilder();

         sb.Append($"var {NodeName} = new KleeneNode(new Token(), tracer);").AppendLine();
         sb.Append(_expr.BuildCode());
         sb.Append($"{NodeName}.Expression = {_expr.NodeName};").AppendLine();

         return sb.ToString();
      }

      public IAstNode BuildTree()
      {
         var kleeneNode = new KleeneNode(new Token(), _tracer);
         var exprNode = _expr.BuildTree();
         kleeneNode.Expression = exprNode;
         return kleeneNode;
      }
   }

   [ExcludeFromCodeCoverage]
   public class Action
   {
      private readonly string _name;
      private static int _counter;

      public Action(string name)
      {
         _name = name;
         NodeName = "Action" + ++_counter;
      }

      public string NodeName { get; }

      public string BuildCode()
      {
         return $"var {NodeName} = new ActionNode(new Token(TokenKind.Action, {_name}), tracer);\n";
      }
   }
}
