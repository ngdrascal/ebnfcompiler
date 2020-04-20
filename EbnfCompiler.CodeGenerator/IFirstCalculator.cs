using System;
using EbnfCompiler.AST;

namespace EbnfCompiler.CodeGenerator
{
   public interface IFirstCalculator
   {
      ITerminalSet Calculate(IAstNode node);
   }

   public class FirstCalculator : IFirstCalculator
   {
      private readonly IAstTraverser _traverser;

      public FirstCalculator(IAstTraverser traverser)
      {
         _traverser = traverser;
      }

      public ITerminalSet Calculate(IAstNode startingNode)
      {
         _traverser.PreProcess += (node) => { };
         _traverser.PostProcess += () => { };

         throw new NotImplementedException();
      }
   }
}
