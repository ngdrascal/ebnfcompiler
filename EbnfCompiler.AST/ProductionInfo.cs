using System.Collections.Generic;
using System.Linq;

namespace EbnfCompiler.AST
{
   public class ProductionInfo : IProductionInfo
   {
      private readonly List<string>_referencedBy = new List<string>();

      public string Name { get; }
      public IExpressionNode Expression { get; set; }
      public IReadOnlyList<string> ReferencedBy => _referencedBy.AsReadOnly();

      public ProductionInfo(string name)
      {
         Name = name;
         Expression = null;
      }

      public void AddReference(string prodName)
      {
         if (ReferencedBy.Contains(prodName))
            return;

         _referencedBy.Add(prodName);
      }

      public override string ToString()
      {
         return $"<{Name}> ::= {Expression?.ToString()}.";
      }
   }
}
