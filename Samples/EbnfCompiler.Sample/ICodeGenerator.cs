using System.Collections.Generic;
using System.IO;

namespace EbnfCompiler.Sample
{
   public interface ICodeGenerator
   {
      void Run(IRootNode rootNode, string moduleName, List<string> references, Stream output);
   }
}
