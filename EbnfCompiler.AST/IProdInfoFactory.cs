using System.Collections.Generic;

namespace EbnfCompiler.AST
{
   public interface IProdInfoFactory
   {
      IProductionInfo Create(string name);

      IReadOnlyCollection<IProductionInfo> AllProductions { get; }
   }
}
