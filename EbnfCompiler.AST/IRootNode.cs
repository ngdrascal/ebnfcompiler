using System.Collections.Generic;

namespace EbnfCompiler.AST
{
   public interface IRootNode
   {
      IReadOnlyCollection<ITokenDefinition> TokenDefs { get; }
      ISyntaxNode Syntax { get; }
   }

   public class RootNode : IRootNode
   {
      public RootNode(IReadOnlyCollection<ITokenDefinition> tokenDefs, ISyntaxNode syntax)
      {
         TokenDefs = tokenDefs;
         Syntax = syntax;
      }

      public IReadOnlyCollection<ITokenDefinition> TokenDefs { get; private set; }
      public ISyntaxNode Syntax { get; private set; }
   }
}
