namespace EbnfCompiler.AST
{
   public interface ITerminalSet
   {
      bool IncludesEpsilon { get; set; }
      bool IsEmpty();
      bool Includes(string termName);
      void Add(string termName);
      void Add(ITerminalSet terminalSet);
      string DelimitedText();
   }
}