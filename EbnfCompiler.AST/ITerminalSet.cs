using System.Collections.Generic;

namespace EbnfCompiler.AST
{
   public interface ITerminalSet
   {
      // const string Epsilon = "$EPSILON$";
      string Epsilon { get; }

      bool IncludesEpsilon { get; }

      bool IsEmpty();

      bool Includes(string termName);

      void Add(string termName);

      void Union(ITerminalSet terminalSet, bool includeEpsilon = true);

      IEnumerable<string> AsEnumerable();

      string DelimitedText();
   }
}