using System;

namespace EbnfCompiler.AST
{
   public interface IAstTraverser
   {
      event Action<IAstNode> PreProcess;

      event Action PostProcess;

      void Traverse(IAstNode astNode);
   }
}
