using System.Collections.Generic;
using System.Linq;

namespace EbnfCompiler.AST
{
   public class ProductionInfo : IProductionInfo
   {
      private readonly IDebugTracer _tracer;
      private ITerminalSet _firstSetInternal;
      private readonly List<string> _referencedBy = new List<string>();

      public ProductionInfo(string name, IDebugTracer tracer)
      {
         _tracer = tracer;
         Name = name;
         ExpressionAst = null;
      }

      public string Name { get; }

      public IExpressionAstNode ExpressionAst { get; set; }

      public ITerminalSet FirstSet
      {
         get
         {
            _tracer.BeginTrace(message: $"First: {GetType().Name}: {this}");

            if (_firstSetInternal == null)
               _firstSetInternal = ExpressionAst.FirstSet;

            _tracer.EndTrace($"First: {GetType().Name} = {_firstSetInternal} ");

            return _firstSetInternal;
         }
      }

      public IReadOnlyList<string> ReferencedBy => _referencedBy.AsReadOnly();

      public void AddReference(string prodName)
      {
         if (ReferencedBy.Contains(prodName))
            return;

         _referencedBy.Add(prodName);
      }

      public override string ToString()
      {
         return $"<{Name}> ::= {ExpressionAst?.ToString()}.";
      }
   }
}
