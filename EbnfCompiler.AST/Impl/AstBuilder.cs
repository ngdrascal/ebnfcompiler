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

         IActionNode actionNode = null;
         if (_stack.Count > 0 && _stack.Peek() is IActionNode)
            actionNode = _stack.Pop().AsActionNode();

         var syntax = _astNodeFactory.Create(AstNodeType.Syntax, token).AsSyntax();
         syntax.PreActionNode = actionNode;
         _stack.Push(syntax);
      }

      public void EndSyntax()
      {
         _tracer.EndTrace(nameof(EndSyntax));

         IActionNode actionNode = null;
         if (_stack.Peek() is IActionNode)
            actionNode = _stack.Pop().AsActionNode();

         var syntax = _stack.Pop().AsSyntax();
         syntax.PostActionNode = actionNode;

         _tracer.TraceLine(new string('-', 40));


         var prodInfo = _prodInfoFactory.Create(statement.ProdName);
         prodInfo.RightHandSide = statement.Expression;

         FixupProdRefNodes();
      }

      public void BeginStatement(IToken token)
      {
         _tracer.BeginTrace(nameof(BeginStatement));

         IActionNode actionNode = null;
         if (_stack.Peek() is IActionNode)
            actionNode = _stack.Pop().AsActionNode();

         var statement = _astNodeFactory.Create(AstNodeType.Statement, token).AsStatement();
         statement.PreActionNode = actionNode;
         _stack.Push(statement);
      }

      public void EndStatement()
      {
         _tracer.EndTrace(nameof(EndStatement));

         IActionNode actionNode = null;
         if (_stack.Peek() is IActionNode)
            actionNode = _stack.Pop().AsActionNode();

         var statement = _stack.Pop().AsStatement();
         statement.PostActionNode = actionNode;
      }

      public void BeginExpression(IToken token)
      {
         _tracer.BeginTrace(nameof(BeginExpression));

         IActionNode actionNode = null;
         if (_stack.Peek() is IActionNode)
            actionNode = _stack.Pop().AsActionNode();

         var expression = _astNodeFactory.Create(AstNodeType.Expression, token).AsExpression();
         expression.PreActionNode = actionNode;
         _stack.Push(expression);
      }

      public void EndExpression()
      {
         _tracer.EndTrace(nameof(EndExpression));

         IActionNode actionNode = null;
         if (_stack.Peek() is IActionNode)
            actionNode = _stack.Pop().AsActionNode();

         var expression = _stack.Pop().AsExpression();
         expression.PreActionNode = actionNode;

         switch (_stack.Peek().AstNodeType)
         {
            case AstNodeType.Statement:
               _stack.Peek().AsStatement().Expression = expression;
               break;

            case AstNodeType.Paren:
               _stack.Peek().AsLParen().Expression = expression;
               break;

            case AstNodeType.Option:
               _stack.Peek().AsLOption().Expression = expression;
               break;

            case AstNodeType.KleeneStar:
               _stack.Peek().AsLKleeneStarNode().Expression = expression;
               break;
         }
      }

      public void BeginTerm(IToken token)
      {
         _tracer.BeginTrace(nameof(BeginTerm));

         IActionNode actionNode = null;
         if (_stack.Peek() is IActionNode)
            actionNode = _stack.Pop().AsActionNode();

         var term = _astNodeFactory.Create(AstNodeType.Term, token).AsTerm();
         term.PreActionNode = actionNode;
         _stack.Push(term);
      }

      public void EndTerm()
      {
         _tracer.EndTrace(nameof(EndTerm));

         IActionNode actionNode = null;
         if (_stack.Peek() is IActionNode)
            actionNode = _stack.Pop().AsActionNode();

         var term = _stack.Pop().AsTerm();
         term.PostActionNode = actionNode;
         _stack.Peek().AsExpression().AppendTerm(term);
      }

      public void BeginFactor(IToken token)
      {
         _tracer.BeginTrace(nameof(BeginFactor));

         IActionNode actionNode = null;
         if (_stack.Peek() is IActionNode)
            actionNode = _stack.Pop().AsActionNode();

         var factor = _astNodeFactory.Create(AstNodeType.Factor, token).AsFactor();
         factor.PreActionNode = actionNode;
         _stack.Push(factor);
      }

      public void EndFactor()
      {
         _tracer.EndTrace(nameof(EndFactor));

         IActionNode actionNode = null;
         if (_stack.Peek() is IActionNode)
            actionNode = _stack.Pop().AsActionNode();

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
            prodRefNode.Expression = prodInfo.RightHandSide;
         }
      }

      /*
            private void Traverse(string prodName, INode node, ITerminalSet firstSet, bool rollup = true)
            {
               while (node != null)
               {
                  switch (node.NodeType)
                  {
                     case NodeType.Expression:
                        TraceLine("AltHead");

                        // figure the first set for each alternative and add them to first set of
                        // the head
                        var term = node.AsExpression().FirstTerm;
                        while (term != null)
                        {
                           Traverse(prodName, term.Next, term.FirstSet);
                           TraceLine("First(alt) = " + term.FirstSet);

                           try
                           {
                              ((ExpressionNode)node).FirstSet.Add(term.FirstSet);
                           }
                           catch (Exception)
                           {
                              Error("Duplicate terminal in first set of <" + prodName + "> " + '\n' +
                                    "First(<" + prodName + ">)=[" +
                                    ((ExpressionNode)node).FirstSet.DelimitedText() + ']',
                                 node);
                           }

                           if (term.FirstSet.IncludesEpsilon)
                              ((ExpressionNode)node).FirstSet.IncludesEpsilon = true;

                           TraceLine("First(head) = " + ((ExpressionNode)node).FirstSet);

                           term = term.NextTerm;
                        }

                        if (rollup)
                        {
                           // add the first set of the head to the first set of <firstSet>
                           try
                           {
                              firstSet.Add(((ExpressionNode)node).FirstSet);
                           }
                           catch (Exception)
                           {
                              Error("Duplicate terminal in first set of <" + prodName + "> " +
                                    '\n' +
                                    "First(<" + prodName + ">)=[" + firstSet.DelimitedText() + "]",
                                 node);
                           }

                           if (((ExpressionNode)node).FirstSet.IncludesEpsilon)
                              firstSet.IncludesEpsilon = true;

                           TraceLine("First(firstSet) = " + firstSet);
                        }

                        break;

                     case NodeType.ProdRef:
                        TraceLine("ProdRef - " + ((ProdRefNode)node).ProdName);
                        ComputeFirst(((ProdRefNode)node).ProdName);
                        var prodInfo = Productions.First(p => p.Key == ((ProdRefNode)node).ProdName).Value;
                        try
                        {
                           firstSet.Add(prodInfo.Expression.FirstSet);
                        }
                        catch (Exception)
                        {
                           Error("Duplicate terminal in first set of <" + prodName + "> " +
                                 "found in <" + ((ProdRefNode)node).ProdName + ">" + '\n' +
                                 "First(<" + prodName + ">)=[" + firstSet.DelimitedText() + "]" + '\n' +
                                 "First(<" + ((ProdRefNode)node).ProdName + ">)=[" +
                                 prodInfo.Expression.FirstSet.DelimitedText() + "]", node);
                        }

                        firstSet.IncludesEpsilon = prodInfo.Expression.FirstSet.IncludesEpsilon;
                        if (!prodInfo.Expression.FirstSet.IncludesEpsilon)
                           rollup = false;
                        break;

                     case NodeType.Terminal:
                        TraceLine("TermName - " + ((TerminalNode)node).TermName);

                        if (!firstSet.Includes(((TerminalNode)node).TermName))
                        {
                           firstSet.Add(((TerminalNode)node).TermName);
                           firstSet.IncludesEpsilon = false;
                           rollup = false;
                        }
                        else
                           Error("Duplicate in first set of <" + prodName + ">: " +
                                 ((TerminalNode)node).TermName, node);

                        break;

                     case NodeType.Term:
                        TraceLine("Alternative");
                        // should never get here
                        break;

                     case NodeType.ActName:
                        TraceLine("ActName");
                        // do nothing
                        break;

                     case NodeType.LParen:
                        TraceLine("ntLParens");
                        // do nothing
                        break;

                     case NodeType.BeginOption:
                        TraceLine("BeginOption");
                        // do nothing
                        break;

                     case NodeType.BeginKleeneStar:
                        TraceLine("BeginKleene");
                        // do nothing
                        break;

                     case NodeType.RParen:
                        TraceLine("ntRParens");
                        if (!firstSet.IncludesEpsilon)
                           rollup = false;
                        break;

                     case NodeType.EndOption:
                     case NodeType.EndKleeneStar:
                        TraceLine(node.NodeType == NodeType.EndOption ? "EndOption" : "EndKleene");

                        if (node.Next == null)
                           firstSet.IncludesEpsilon = true;
                        break;
                  }

                  node = node.Next;
                  if (rollup)
                     continue;

                  // skip until no more nodes or a AltHead node
                  while ((node != null) && (node.NodeType != NodeType.Expression))
                     node = node.Next;
               }
            }

            private void ComputeFirst(string prodName)
            {
               if (!Productions.TryGetValue(prodName, out var prodInfo))
                  Error("Production not defined:" + prodName);

               if (prodInfo == null || !prodInfo.Expression.FirstSet.IsEmpty())
                  return;

               TraceLine("BEGIN PRODUCTION - " + prodName);
               Traverse(prodName, prodInfo.Expression, prodInfo.Expression.FirstSet);
               TraceLine("END PRODUCTION - " + prodName);
            }

*/


   }
}
