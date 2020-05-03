using System.Collections.Generic;

namespace EbnfCompiler.AST.Impl
{
   public class RootNode : IRootNode
   {
      public RootNode(IReadOnlyCollection<ITokenDefinition> tokenDefs, ISyntaxNode syntax)
      {
         TokenDefs = tokenDefs;
         Syntax = syntax;
      }

      public IReadOnlyCollection<ITokenDefinition> TokenDefs { get; }
      public ISyntaxNode Syntax { get; }
   }
}