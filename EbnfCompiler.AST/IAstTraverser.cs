using System;

namespace EbnfCompiler.AST
{
   public interface IAstTraverser
   {
      event Action<IAstNode> ProcessNode;

      event Action PostProcessNode;

      void Traverse(IAstNode astNode);
   }
}
