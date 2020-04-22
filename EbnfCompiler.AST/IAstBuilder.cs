using System.Collections.Generic;
using EbnfCompiler.Compiler;

namespace EbnfCompiler.AST
{
   public interface IAstBuilder
   {
      IReadOnlyCollection<ITokenDefinition> TokenDefinitions { get; }
      IReadOnlyCollection<IProductionInfo> Productions { get; }

      void AddTokenName(IToken token);
      void SetTokenDef(IToken token);

      void BeginSyntax();
      void EndSyntax();

      void BeginStatement(IToken token);
      void EndStatement();

      void BeginExpression(IToken token);
      void EndExpression();

      void BeginTerm(IToken token);
      void EndTerm();

      void BeginFactor(IToken token);
      void EndFactor();

      void BeginParens(IToken token);
      void EndParens();

      void BeginOption(IToken token);
      void EndOption();

      void BeginKleene(IToken token);
      void EndKleene();

      void FoundProduction(IToken token);
      void FoundTerminal(IToken token);
      void FoundAction(IToken token);
   }
}
