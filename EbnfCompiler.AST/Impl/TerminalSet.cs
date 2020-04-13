using System.Collections.Generic;
using System.Linq;

namespace EbnfCompiler.AST
{
   public class TerminalSet : ITerminalSet
   {
      private readonly HashSet<string> _terminals;

      public TerminalSet()
      {
         _terminals = new HashSet<string>();
      }

      public string Epsilon => "$EPSILON$";

      public bool IncludesEpsilon => Includes(Epsilon);

      public bool IsEmpty()
      {
         return _terminals.Count == 0;
      }

      public bool Includes(string termName)
      {
         return _terminals.Contains(termName);
      }

      public void Add(string termName)
      {
         _terminals.Add(termName);
      }

      public void Union(ITerminalSet terminalSet, bool includeEpsilon = true)
      {
         foreach (var term in terminalSet.AsEnumerable())
         {
            if ((term != Epsilon) || (term == Epsilon && includeEpsilon))
               _terminals.Add(term);
         }
      }

      public IEnumerable<string> AsEnumerable()
      {
         return _terminals.AsEnumerable();
      }

      public string DelimitedText()
      {
         return string.Join(",", _terminals.ToArray());
      }

      public override string ToString()
      {
         return $"[ {DelimitedText()} ]";
      }
   }
}
