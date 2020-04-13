﻿using System.Collections.Generic;

namespace EbnfCompiler.AST
{
   public interface IProductionInfo
   {
      string Name { get; }

      IExpressionNode Expression { get; set; }

      ITerminalSet FirstSet { get; }

      void AddReference(string prodName);

      IReadOnlyList<string> ReferencedBy { get; }
   }
}
