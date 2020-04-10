using System.Collections.Generic;

namespace EbnfCompiler.AST
{
   public interface IProductionInfo
   {
      void AddReference(string prodName);
      string Name { get; }
      IAltHeadNode AltHead { get; set; }
      IReadOnlyList<string> ReferencedBy { get; }
   }
}
