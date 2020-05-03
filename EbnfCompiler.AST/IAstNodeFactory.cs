using System.Collections.Generic;
using EbnfCompiler.Compiler;

namespace EbnfCompiler.AST
{
   public interface IAstNodeFactory
   {
      IAstNode Create(AstNodeType astNodeType, IToken token);

      IReadOnlyCollection<IAstNode> AllNodes { get; }
   }
}
