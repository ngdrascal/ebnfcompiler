using System;

namespace EbnfCompiler.Sample
{
   public interface IAstTraverser
   {
      event Action<IAstNode> ProcessNode;

      event Action PostProcessNode;

      void Traverse(IAstNode astNode);
   }
}
