using EbnfCompiler.Compiler;

namespace EbnfCompiler.AST
{
   public class AstErrorException : CompilerException
   {
      public AstErrorException(string message)
         : base(message, null)
      {
      }
   }
}
