﻿using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using EbnfCompiler.Compiler;

namespace EbnfCompiler.AST
{
   public class AstBuilder : IAstBuilder
   {
      private readonly IAstNodeFactory _astNodeFactory;
      private readonly IProdInfoFactory _prodInfoFactory;
      private readonly IDebugTracer _tracer;
      private readonly Stack<INode> _stack;
      private IProductionInfo _currentProd; // index into symbol table
      private TokenDefinition _lastTokenInfo;

      public AstBuilder(IAstNodeFactory astNodeFactory, 
                        IProdInfoFactory prodInfoFactory, IDebugTracer tracer)
      {
         _astNodeFactory = astNodeFactory;
         _prodInfoFactory = prodInfoFactory;
         _tracer = tracer;

         _stack = new Stack<INode>();

         TokenDefinitions = new Collection<ITokenDefinition>();
         Productions = new Dictionary<string, IProductionInfo>();
      }

      public ICollection<ITokenDefinition> TokenDefinitions { get; }

      public IDictionary<string, IProductionInfo> Productions { get; }

      public void AddTokenName(IToken token)
      {
         if (TokenDefinitions.Count(p => p.Image == token.Image) == 0)
         {
            _lastTokenInfo = new TokenDefinition { Image = token.Image };
            TokenDefinitions.Add(_lastTokenInfo);
         }
         else
            Error("Token already defined: " + token.Image, token);
      }

      public void SetTokenDef(IToken token)
      {
         _lastTokenInfo.Definition = token.Image;
      }

      public void BeginSyntax()
      {
         _tracer.BeginTrace(nameof(BeginSyntax));
      }

      public void EndSyntax()
      {
         _tracer.EndTrace(nameof(EndSyntax));

         _tracer.TraceLine(new string(' ', 40));

         FixupProdRefNodes();

         // _tracer.TraceLine(new string('-', 40));
         // foreach (var production in Productions)
         //    _tracer.TraceLine(production.Value.ToString());
      }

      public void BeginStatement(IToken token)
      {
         _tracer.BeginTrace(nameof(BeginStatement));

         _currentProd = _prodInfoFactory.Create(token.Image);
         Productions.Add(token.Image, _currentProd);

         var statement = _astNodeFactory.Create(NodeType.Statement, token);

         _stack.Push(statement);
      }

      public void EndStatement()
      {
         _tracer.EndTrace(nameof(EndStatement));

         var statement = _stack.Peek().AsStatement();
         _currentProd.Expression = statement.Expression;
      }

      public void BeginExpression(IToken token)
      {
         _tracer.BeginTrace(nameof(BeginExpression));

         var expression = _astNodeFactory.Create(NodeType.Expression, token);

         _stack.Push(expression);
      }

      public void EndExpression()
      {
         _tracer.EndTrace(nameof(EndExpression));

         var expression = _stack.Pop().AsExpression();

         switch (_stack.Peek().NodeType)
         {
            case NodeType.Statement:
               _stack.Peek().AsStatement().Expression = expression;
               break;

            case NodeType.LParen:
               _stack.Peek().AsLParen().Expression = expression;
               break;

            case NodeType.BeginOption:
               _stack.Peek().AsLOption().Expression = expression;
               break;

            case NodeType.BeginKleeneStar:
               _stack.Peek().AsLKleeneStarNode().Expression = expression;
               break;
         }
      }

      public void BeginTerm(IToken token)
      {
         _tracer.BeginTrace(nameof(BeginTerm));

         var term = _astNodeFactory.Create(NodeType.Term, token).AsTerm();

         var expr = _stack.Peek().AsExpression();
         expr.AppendTerm(term);

         _stack.Push(term);
      }

      public void EndTerm()
      {
         _tracer.EndTrace(nameof(EndTerm));

         _stack.Pop().AsTerm();
      }

      public void BeginFactor(IToken token)
      {
         _tracer.BeginTrace(nameof(BeginFactor));

         var factor = _astNodeFactory.Create(NodeType.Factor, token);

         _stack.Push(factor);
      }

      public void EndFactor()
      {
         _tracer.EndTrace(nameof(EndFactor));

         var factor = _stack.Pop().AsFactor();
         _stack.Peek().AsTerm().AppendFactor(factor);
      }

      public void BeginParens(IToken token)
      {
         _tracer.BeginTrace(nameof(BeginParens));

         var lParen = _astNodeFactory.Create(NodeType.LParen, token);

         _stack.Peek().AsFactor().FactorExpr = lParen;
         _stack.Push(lParen);
      }

      public void EndParens(IToken token)
      {
         _tracer.EndTrace(nameof(EndParens));

         _stack.Pop();
      }

      public void BeginOption(IToken token)
      {
         _tracer.BeginTrace(nameof(BeginOption));

         var lOption = _astNodeFactory.Create(NodeType.BeginOption, token);

         _stack.Peek().AsFactor().FactorExpr = lOption; 
         _stack.Push(lOption);
      }

      public void EndOption(IToken token)
      {
         _tracer.EndTrace(nameof(EndOption));

         _stack.Pop();
      }

      public void BeginKleene(IToken token)
      {
         _tracer.BeginTrace(nameof(BeginKleene));

         var lKleene = _astNodeFactory.Create(NodeType.BeginKleeneStar, token);

         _stack.Peek().AsFactor().FactorExpr = lKleene;
         _stack.Push(lKleene);
      }

      public void EndKleene(IToken token)
      {
         _tracer.EndTrace(nameof(EndKleene));

         _stack.Pop();
      }

      public void FoundProduction(IToken token)
      {
         var prodRef = _astNodeFactory.Create(NodeType.ProdRef, token);

         _stack.Peek().AsFactor().FactorExpr = prodRef;
      }

      public void FoundTerminal(IToken token)
      {
         var terminal = _astNodeFactory.Create(NodeType.Terminal, token);

         _stack.Peek().AsFactor().FactorExpr = terminal;
      }

      public void FoundAction(IToken token)
      {
         _astNodeFactory.Create(NodeType.Action, token);
      }

      private void Error(string message, IToken token)
      {
         throw new SemanticErrorException(message, token);
      }

      private void FixupProdRefNodes()
      {
         foreach (var node in _astNodeFactory.AllNodes.Where(p => p.NodeType == NodeType.ProdRef))
         {
            var prodRefNode = (ProdRefNode)node;
            var prodInfo = Productions.First(p => p.Key == prodRefNode.ProdName).Value;
            prodRefNode.Expression = prodInfo.Expression;
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
