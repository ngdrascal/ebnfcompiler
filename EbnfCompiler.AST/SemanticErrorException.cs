using EbnfCompiler.Compiler;

namespace EbnfCompiler.AST
{
   public class SemanticErrorException : CompilerException
   {
      public SemanticErrorException(string message)
         : base(message, null)
      {
      }

      public SemanticErrorException(string message, INode node)
         : base(message, node.Location)
      {
      }


      public SemanticErrorException(string message, IToken token)
         : base(message, token.Location)
      {
      }
   }
}
