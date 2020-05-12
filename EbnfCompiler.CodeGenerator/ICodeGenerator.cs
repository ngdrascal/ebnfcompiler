using EbnfCompiler.AST;

namespace EbnfCompiler.CodeGenerator
{
   public interface ICodeGenerator
   {
      void Run(IRootNode rootNode);
   }
}
