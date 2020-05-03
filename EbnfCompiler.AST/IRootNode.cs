using System.Collections.Generic;

namespace EbnfCompiler.AST
{
   public interface IRootNode
   {
      IReadOnlyCollection<ITokenDefinition> TokenDefs { get; }
      ISyntaxNode Syntax { get; }
   }
}
