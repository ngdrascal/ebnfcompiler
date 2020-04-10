using System.Collections.Generic;
using System.Linq;

namespace EbnfCompiler.AST
{
   public class TerminalSet : ITerminalSet
   {
      private readonly HashSet<string> _terminals;
      private bool _includesEpsilon;

      public bool IncludesEpsilon
      {
         get => _includesEpsilon;
         set
         {
            if (_includesEpsilon)
               return;

            _includesEpsilon = value;
         }
      }

      public TerminalSet()
      {
         _terminals = new HashSet<string>();

         IncludesEpsilon = false;
      }

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

      public void Add(ITerminalSet terminalSet)
      {
         foreach (var term in ((TerminalSet)terminalSet)._terminals)
            _terminals.Add(term);
      }

      public string DelimitedText()
      {
         var result = string.Empty;
         var a = _terminals.ToArray();
         for (var i = 0; i < a.Length - 2; i++)
            result += a[i] + ",";

         return result + a[a.Length - 1];
      }

      public override string ToString()
      {
         var result = '[' + DelimitedText();
         if (IncludesEpsilon)
            result += ",<epsilon>";
         return result + ']';
      }
   }
}
