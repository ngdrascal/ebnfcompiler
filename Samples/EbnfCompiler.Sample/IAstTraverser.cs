using System;

namespace EbnfCompiler.Sample
{
    public interface IAstTraverser
    {
        event Action<IAstNode> ProcessNode;

        event Action<AstNodeTypes> PostProcessNode;

        void Traverse(IAstNode astNode);
    }
}
