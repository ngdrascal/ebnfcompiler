using System.Collections.Generic;

namespace EbnfCompiler.AST.Impl
{
   public class ProdInfoFactory : IProdInfoFactory
   {
      private readonly IDebugTracer _tracer;
      private readonly List<IProductionInfo> _allProductions = new List<IProductionInfo>();

      public ProdInfoFactory(IDebugTracer tracer)
      {
         _tracer = tracer;
      }

      public IProductionInfo Create(string name)
      {
         var prodInfo = new ProductionInfo(name, _tracer);
         _allProductions.Add(prodInfo);

         return prodInfo;
      }

      public IReadOnlyCollection<IProductionInfo> AllProductions => 
         _allProductions.AsReadOnly();
   }
}