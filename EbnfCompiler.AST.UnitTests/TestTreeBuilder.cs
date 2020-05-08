using System;
using System.Text;

namespace EbnfCompiler.AST.UnitTests
{
   /*public interface ISyntaxBuilder
   {
      ISyntaxBuilder Syntax();
      IStatementBuilder Begin();
      string Build();
   }

   public interface IStatementBuilder
   {
      IStatementBuilder Statement(string prodName);
      IStmtExprBuilder Begin();
      ISyntaxBuilder EndStmt();
   }

   public interface IStmtExprBuilder
   {
      IStmtExprBuilder Expression();
      ITermBuilder Begin();
      IStatementBuilder EndExpr();
   }

   public interface ITermBuilder
   {
      ITermBuilder Term();
      IFactorBuilder Begin();
      IStmtExprBuilder EndTerm();
   }

   public interface IFactorBuilder
   {
      IFactorBuilder Factor();
      IFactorChild With();
      ITermBuilder EndFactor();
   }

   public interface IFactorChild
   {
      IFactorBuilder Terminal(string image);
      IFactorBuilder ProductionRef(string image);
      IStmtExprBuilder Paren();
      IOptionBuilder Option();
      IStmtExprBuilder Kleene();
   }

   public interface IOptionBuilder
   {
      IFactExprBuilder Begin();
      IFactorBuilder EndOpt();
   }

   public interface IFactExprBuilder
   {
      IFactExprBuilder Expression();
      ITermBuilder Begin();
      IFactorBuilder EndExpr();
   }

   class Driver
   {
      void Go()
      {
         ISyntaxBuilder syntax = null;

         // syntax
         //    .Begin().Statement("<S>")
         //       .Begin().Expression()
         //          .Begin().Term()
         //             .Begin().Factor()
         //                .With().Terminal("a")
         //                .With().Option()
         //                   .Begin().Expression()
         //                      .Begin().Term()
         //                         .Begin().Factor()
         //                            .With().Terminal("b")
         //                         .EndFactor()
         //                      .EndTerm()
         //                   .EndExpr()
         //                .EndOpt()
         //             .EndFact()
         //          .EndTerm()
         //       .EndExpr()
         //    .EndStmt()
         // .Build();
      }
   }*/

   public class SyntaxBuilder : IDisposable
   {
      private bool _isDisposed;
      private Syntax _root;

      public Syntax Syntax(params Statement[] statements)
      {
         _root = new Syntax(statements);
         return _root;
      }

      public Statement Statement(string prodName, Expression expression)
      {
         return new Statement(prodName, expression);
      }

      public Expression Expression(params Term[] terms)
      {
         return new Expression(terms);
      }

      public Term Term(params Factor[] factors)
      {
         return new Term(factors);
      }

      public Factor Factor(IFactExpr factExpr)
      {
         return new Factor(factExpr);
      }

      public Terminal Terminal(string image)
      {
         return new Terminal(image);
      }

      public ProdRef ProdRef(string name)
      {
         return new ProdRef(name);
      }

      public string Build()
      {
         return _root.Build();
      }

      public void Dispose()
      {
         Dispose(true);
         GC.SuppressFinalize(this);
      }

      protected virtual void Dispose(bool disposing)
      {
         if (_isDisposed) return;

         if (disposing)
         {
         }

         _isDisposed = true;
      }
   }

   public interface IBuild
   {
      string Build();
   }

   public class Syntax : IBuild
   {
      private readonly Statement[] _statements;
      private static int _counter;

      public Syntax(params Statement[] statements)
      {
         _statements = statements;
      }

      public string Build()
      {
         var sb = new StringBuilder();

         foreach (var stmt in _statements)
         {
            sb.Append(stmt.Build()).AppendLine();
         }

         return sb.ToString();
      }
   }

   public class Statement : IBuild
   {
      private readonly string _prodName;
      private readonly Expression _expression;
      private static int _counter;

      public Statement(string prodName, Expression expression)
      {
         _prodName = prodName;
         _expression = expression;
         NodeName = "stmt" + ++_counter;
      }

      public string NodeName { get; }

      public string Build()
      {
         var sb = new StringBuilder();
         sb.Append(_expression.Build()).AppendLine();
         sb.Append($"var {NodeName} = new StatementNode(new Token(), tracer)").AppendLine();
         sb.Append($"{NodeName}.Expression = {_expression.NodeName}").AppendLine();

         return sb.ToString();
      }
   }

   public class Expression : IBuild
   {
      private readonly Term[] _terms;
      private static int _counter;

      public Expression(params Term[] terms)
      {
         _terms = terms;
         NodeName = "expr" + ++_counter;
      }

      public string NodeName { get; }

      public string Build()
      {
         var sb = new StringBuilder();

         foreach (var term in _terms)
         {
            sb.Append(term.Build()).AppendLine();
         }

         return sb.ToString();
      }
   }

   public class Term : IBuild
   {
      private readonly Factor[] _factors;
      private static int _counter;

      public Term(params Factor[] factors)
      {
         _factors = factors;
         NodeName = "term" + ++_counter;
      }

      public string NodeName { get; }

      public string Build()
      {
         var sb = new StringBuilder();

         sb.Append($"var {NodeName} = new TermNode(new Token(), tracer)").AppendLine();

         foreach (var fact in _factors)
         {
            sb.Append(fact.Build()).AppendLine();
            sb.Append($"{NodeName}.AppendFactor({fact.NodeName})").AppendLine();
         }

         return sb.ToString();
      }
   }

   public class Factor : IBuild
   {
      private readonly IFactExpr _factExpr;
      private static int _counter;

      public Factor(IFactExpr factExpr)
      {
         _factExpr = factExpr;
         NodeName = "factor" + ++_counter;
      }

      public string NodeName { get; }

      public string Build()
      {
         var sb = new StringBuilder();

         sb.Append($"var {NodeName} = new FactorNode(new Token(), tracer);").AppendLine();
         sb.Append(_factExpr.Build()).AppendLine();
         sb.Append($"{NodeName}.FactExpr = {_factExpr.NodeName};").AppendLine();

         return sb.ToString();
      }
   }

   public interface IFactExpr
   {
      public string NodeName { get; }
      string Build();
   }

   public class Terminal : IFactExpr, IBuild
   {
      private readonly string _image;
      private static int _counter;

      public Terminal(string image)
      {
         _image = image;
         NodeName = "terminal" + ++_counter;
      }

      public string NodeName { get; }

      public string Build()
      {
         return $"var {NodeName} = new TerminalNode(new Token(TokenKind.String, \"{_image}\"), tracer);";
      }
   }

   public class ProdRef : IFactExpr, IBuild
   {
      private readonly string _name;
      private static int _counter;

      public ProdRef(string name)
      {
         _name = name;
         NodeName = "prodRef" + ++_counter;
      }

      public string NodeName { get; }

      public string Build()
      {
         return $"var {NodeName} = new ProRefNode(new Token(TokenKind.String, \"{_name}\"), tracer);";
      }
   }

   public class Paren : IFactExpr, IBuild
   {
      private readonly Expression _expr;
      private static int _counter;

      public Paren(Expression expr)
      {
         _expr = expr;
         NodeName = "prodRef" + ++_counter;
      }

      public string NodeName { get; }

      public string Build()
      {
         return _expr.Build();
      }
   }
}
