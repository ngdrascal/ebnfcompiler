using System.Collections.Generic;
using EbnfCompiler.AST;

namespace EbnfCompiler.CodeGenerator
{
   public interface ICodeGenerator
   {
      void Generate(Dictionary<string, IProductionInfo> productions);
   }
}
