using System.Collections.Generic;
using System.Linq;
using EbnfCompiler.Compiler;

namespace EbnfCompiler.AST.Impl
{
   public class AstBuilder : IAstBuilder
   {
      private readonly IAstNodeFactory _astNodeFactory;
      private readonly IProdInfoFactory _prodInfoFactory;
      private readonly IDebugTracer _tracer;
      private readonly List<ITokenDefinition> _tokenDefinitions;
      private readonly Stack<IAstNode> _stack;
      private TokenDefinition _lastTokenInfo;

      public AstBuilder(IAstNodeFactory astNodeFactory,
                        IProdInfoFactory prodInfoFactory,
                        Stack<IAstNode> stack,
                        IDebugTracer tracer)
      {
         _astNodeFactory = astNodeFactory;
         _prodInfoFactory = prodInfoFactory;
         _tracer = tracer;

         _tokenDefinitions = new List<ITokenDefinition>();
         _stack = stack;

         _tokenDefinitions = new List<ITokenDefinition>();
      }

      public IReadOnlyCollection<ITokenDefinition> TokenDefinitions => _tokenDefinitions.AsReadOnly();
      public IReadOnlyCollection<IProductionInfo> Productions => _prodInfoFactory.AllProductions;
      public ISyntaxNode SyntaxTree { get; private set; }

      public void AddTokenName(IToken token)
      {
         if (TokenDefinitions.Count(p => p.Image == token.Image) == 0)
         {
            _lastTokenInfo = new TokenDefinition { Image = token.Image };
            _tokenDefinitions.Add(_lastTokenInfo);
         }
         else
            Error("Token already defined: " + token.Image, token);
      }

      public void SetTokenDef(IToken token)
      {
         _lastTokenInfo.Definition = token.Image;
      }

      public void BeginSyntax(IToken token)
      {
         _tracer.BeginTrace(nameof(BeginSyntax));

         var actionNode = (_stack.Count > 0 && _stack.Peek() is IActionNode)
            ? _stack.Pop().AsAction() : null;

         var syntax = _astNodeFactory.Create(AstNodeType.Syntax, token).AsSyntax();
         syntax.PreActionNode = actionNode;
         _stack.Push(syntax);
      }

      public void EndSyntax()
      {
         _tracer.EndTrace(nameof(EndSyntax));

         var actionNode = _stack.Peek() is IActionNode ? _stack.Pop().AsAction() : null;

         var syntax = _stack.Pop().AsSyntax();
         syntax.PostActionNode = actionNode;

         _tracer.TraceLine(new string('-', 40));

         FixupProdRefNodes();

         SyntaxTree = syntax;
      }

      public void BeginStatement(IToken token)
      {
         _tracer.BeginTrace(nameof(BeginStatement));

         var actionNode = _stack.Peek() is IActionNode ? _stack.Pop().AsAction() : null;

         var statement = _astNodeFactory.Create(AstNodeType.Statement, token).AsStatement();
         statement.PreActionNode = actionNode;
         _stack.Push(statement);
      }

      public void EndStatement()
      {
         _tracer.EndTrace(nameof(EndStatement));

         var actionNode = _stack.Peek() is IActionNode ? _stack.Pop().AsAction() : null;

         var statement = _stack.Pop().AsStatement();
         statement.PostActionNode = actionNode;

         _stack.Peek().AsSyntax().AppendStatement(statement);

         var prodInfo = _prodInfoFactory.Create(statement.ProdName);
         prodInfo.Statement = statement;
      }

      public void BeginExpression(IToken token)
      {
         _tracer.BeginTrace(nameof(BeginExpression));

         var actionNode = _stack.Peek() is IActionNode ? _stack.Pop().AsAction() : null;

         var expression = _astNodeFactory.Create(AstNodeType.Expression, token).AsExpression();
         expression.PreActionNode = actionNode;
         _stack.Push(expression);
      }

      public void EndExpression()
      {
         _tracer.EndTrace(nameof(EndExpression));

         var actionNode = _stack.Peek() is IActionNode ? _stack.Pop().AsAction() : null;

         var expression = _stack.Pop().AsExpression();
         expression.PostActionNode = actionNode;

         switch (_stack.Peek().AstNodeType)
         {
            case AstNodeType.Statement:
               _stack.Peek().AsStatement().Expression = expression;
               break;

            case AstNodeType.Paren:
               _stack.Peek().AsParen().Expression = expression;
               break;

            case AstNodeType.Option:
               _stack.Peek().AsOption().Expression = expression;
               break;

            case AstNodeType.KleeneStar:
               _stack.Peek().AsKleene().Expression = expression;
               break;
         }
      }

      public void BeginTerm(IToken token)
      {
         _tracer.BeginTrace(nameof(BeginTerm));

         var actionNode = _stack.Peek() is IActionNode ? _stack.Pop().AsAction() : null;

         var term = _astNodeFactory.Create(AstNodeType.Term, token).AsTerm();
         term.PreActionNode = actionNode;
         _stack.Push(term);
      }

      public void EndTerm()
      {
         _tracer.EndTrace(nameof(EndTerm));

         var actionNode = _stack.Peek() is IActionNode ? _stack.Pop().AsAction() : null;

         var term = _stack.Pop().AsTerm();
         term.PostActionNode = actionNode;
         _stack.Peek().AsExpression().AppendTerm(term);
      }

      public void BeginFactor(IToken token)
      {
         _tracer.BeginTrace(nameof(BeginFactor));

         var actionNode = _stack.Peek() is IActionNode ? _stack.Pop().AsAction() : null;

         var factor = _astNodeFactory.Create(AstNodeType.Factor, token).AsFactor();
         factor.PreActionNode = actionNode;
         _stack.Push(factor);
      }

      public void EndFactor()
      {
         _tracer.EndTrace(nameof(EndFactor));

         var actionNode = _stack.Peek() is IActionNode ? _stack.Pop().AsAction() : null;

         var factor = _stack.Pop().AsFactor();
         factor.PostActionNode = actionNode;
         _stack.Peek().AsTerm().AppendFactor(factor);
      }

      public void BeginParens(IToken token)
      {
         _tracer.BeginTrace(nameof(BeginParens));

         var lParen = _astNodeFactory.Create(AstNodeType.Paren, token);
         _stack.Push(lParen);
      }

      public void EndParens()
      {
         _tracer.EndTrace(nameof(EndParens));

         var lParen = _stack.Pop();
         _stack.Peek().AsFactor().FactorExpr = lParen;
      }

      public void BeginOption(IToken token)
      {
         _tracer.BeginTrace(nameof(BeginOption));

         var option = _astNodeFactory.Create(AstNodeType.Option, token);
         _stack.Push(option);
      }

      public void EndOption()
      {
         _tracer.EndTrace(nameof(EndOption));

         var option = _stack.Pop();
         _stack.Peek().AsFactor().FactorExpr = option;
      }

      public void BeginKleene(IToken token)
      {
         _tracer.BeginTrace(nameof(BeginKleene));

         var lKleene = _astNodeFactory.Create(AstNodeType.KleeneStar, token);
         _stack.Push(lKleene);
      }

      public void EndKleene()
      {
         _tracer.EndTrace(nameof(EndKleene));

         var lKleene = _stack.Pop();
         _stack.Peek().AsFactor().FactorExpr = lKleene;
      }

      public void FoundProduction(IToken token)
      {
         var prodRef = _astNodeFactory.Create(AstNodeType.ProdRef, token);

         _stack.Peek().AsFactor().FactorExpr = prodRef;
      }

      public void FoundTerminal(IToken token)
      {
         var terminal = _astNodeFactory.Create(AstNodeType.Terminal, token);

         _stack.Peek().AsFactor().FactorExpr = terminal;
      }

      public void FoundAction(IToken token)
      {
         var actionNode = _astNodeFactory.Create(AstNodeType.Action, token);
         _stack.Push(actionNode);
      }

      private void Error(string message, IToken token)
      {
         throw new SemanticErrorException(message, token);
      }

      private void FixupProdRefNodes()
      {
         foreach (var node in _astNodeFactory.AllNodes.Where(p => p.AstNodeType == AstNodeType.ProdRef))
         {
            var prodRefNode = (IProdRefNode)node;
            var prodInfo = Productions.First(p => p.Name == prodRefNode.ProdName);
            prodRefNode.Expression = prodInfo.Statement.Expression;
         }
      }
   }
}
