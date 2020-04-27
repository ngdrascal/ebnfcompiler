using System.Collections.Generic;
using System.Linq;

namespace EbnfCompiler.AST.Impl
{
   public class ProductionInfo : IProductionInfo
   {
      private readonly IDebugTracer _tracer;
      private readonly List<string> _referencedBy = new List<string>();

      public ProductionInfo(string name, IDebugTracer tracer)
      {
         _tracer = tracer;
         Name = name;
         Statement = null;
      }

      public string Name { get; }

      public IStatementNode Statement { get; set; }

      public ITerminalSet FirstSet
      {
         get
         {
            _tracer.BeginTrace(message: $"First: {GetType().Name}: {this}");

            _tracer.EndTrace($"First: {GetType().Name} = {Statement.FirstSet} ");

            return Statement.FirstSet;
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
         return $"<{Name}> ::= {Statement?.ToString()}.";
      }
   }
}
