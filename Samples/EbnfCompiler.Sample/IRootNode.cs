using System.Collections.Generic;

namespace EbnfCompiler.Sample
{
    public interface IRootNode
    {
        IReadOnlyList<IAstNode> Statements { get; }

        void AppendStatement(IAstNode node);
    }

    public class RootNode : IRootNode
    {
        private List<IAstNode> _statements = new List<IAstNode>();

        public IReadOnlyList<IAstNode> Statements => _statements.AsReadOnly();

        public void AppendStatement(IAstNode node)
        {
            _statements.Add(node);
        }
    }
}
