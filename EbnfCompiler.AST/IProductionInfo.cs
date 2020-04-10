using System.Collections.Generic;

namespace EbnfCompiler.AST
{
   public interface IProductionInfo
   {
      void AddReference(string prodName);
      string Name { get; }
      IExpressionNode Expression { get; set; }
      IReadOnlyList<string> ReferencedBy { get; }
   }
}
