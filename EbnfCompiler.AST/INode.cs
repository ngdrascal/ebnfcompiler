using EbnfCompiler.Compiler;

namespace EbnfCompiler.AST
{
   public enum NodeType
   {
      Expression, Term, ProdRef, TermName, ActName,
      LParen, RParen, BeginOption, EndOption,
      BeginKleene, EndKleene
   };

   public interface INode
   {
      ISourceLocation Location { get; set; }
      NodeType NodeType { get; }
      string Image { get; }
      INode Next { get; set; }
      ITerminalSet FirstSet { get; }
      string ToString();
   }
}
