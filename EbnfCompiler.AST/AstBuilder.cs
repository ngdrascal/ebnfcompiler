using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using EbnfCompiler.Compiler;

namespace EbnfCompiler.AST
{
   public class AstBuilder : IAstBuilder
   {
      private readonly List<INode> _nodes;
      private readonly Stack<INode> _stack;
      private INode _currentNode;
      private IProductionInfo _currentProd; // index into symbol table
      private TokenDefinition _lastTokenInfo;

      public ICollection<ITokenDefinition> TokenDefinitions { get; private set; }

      public IDictionary<string, IProductionInfo> Productions { get; private set; }

      public AstBuilder()
      {
         _nodes = new List<INode>();
         _stack = new Stack<INode>();
         _currentNode = null;

         TokenDefinitions = new Collection<ITokenDefinition>();
         Productions = new Dictionary<string, IProductionInfo>();
      }

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
      }

      public void EndSyntax()
      {
         FixupProdRefNodes();

         foreach (var production in Productions)
         {
            BuildReferences(production.Key, production.Value.AltHead);
            ComputeFirst(production.Key);
         }

         foreach (var production in Productions)
            Debug.WriteLine(production.Value.ToString());
      }

      public void BeginStatement(IToken token)
      {
         _currentProd = new ProductionInfo(token.Image);
         Productions.Add(token.Image, _currentProd);
      }

      public void EndStatement()
      {
         _currentProd.AltHead = (AltHeadNode)_currentNode;
         _currentNode = null;
      }

      public void BeginExpression(IToken token)
      {
         var node = new AltHeadNode(token);
         _nodes.Add(node);

         AppendNode(node);

         _stack.Push(node);
      }

      public void EndExpression()
      {
         _currentNode = _stack.Pop();
      }

      public void BeginTerm(IToken token)
      {
         var node = new AlternativeNode(token);
         _nodes.Add(node);

         // set n to the altHead
         var head = (AltHeadNode)_stack.Peek();
         head.AltCount++;

         // if there are not any existing alternatives
         if (head.FirstAlt == null)
            head.FirstAlt = node;
         else // find the last alternative
         {
            var alt = head.FirstAlt;
            while (alt.NextAlt != null)
               alt = alt.NextAlt;
            alt.NextAlt = node;
         }

         _currentNode = node;
      }

      public void EndTerm()
      {
         // point back to the head of the current set of alternatives
         _currentNode = _stack.Peek();
      }

      public void BeginParens(IToken token)
      {
         var node = new LParenNode(token);
         _nodes.Add(node);

         AppendNode(node);

         _stack.Push(node);
      }

      public void EndParens(IToken token)
      {
         var node = new RParenNode(token);
         _nodes.Add(node);

         AppendNode(node);

         node.Mate = (LParenNode)_stack.Peek();
         ((LParenNode)_stack.Peek()).Mate = node;

         _stack.Pop();
      }

      public void BeginOption(IToken token)
      {
         var node = new LOptionNode(token);
         _nodes.Add(node);

         AppendNode(node);

         _stack.Push(node);
      }

      public void EndOption(IToken token)
      {
         var node = new ROptionNode(token);
         _nodes.Add(node);

         AppendNode(node);

         node.Mate = (LOptionNode)(_stack.Peek());
         ((LOptionNode)_stack.Peek()).Mate = node;

         _stack.Pop();
      }

      public void BeginKleene(IToken token)
      {
         var node = new LKleeneNode(token);
         _nodes.Add(node);

         AppendNode(node);

         _stack.Push(node);
      }

      public void EndKleene(IToken token)
      {
         var node = new RKleeneNode(token);
         _nodes.Add(node);

         AppendNode(node);

         node.Mate = (LKleeneNode)_stack.Peek();
         ((LKleeneNode)_stack.Peek()).Mate = node;

         _stack.Pop();
      }

      public void FoundProduction(IToken token)
      {
         var node = new ProdRefNode(token);
         _nodes.Add(node);
         AppendNode(node);
      }

      public void FoundTerminal(IToken token)
      {
         //var enumImage = String.Empty;

         //if (_tokens.ContainsKey(token.Image))
         //   enumImage = _tokens[token.Image].Definition;
         //else
         //   Error("Undefined terminal: '" + token.Image + "'", token);

         var node = new TerminalNode(token, token.Image);
         AppendNode(node);
      }

      public void FoundAction(IToken token)
      {
         var node = new ActionNode(token);
         _nodes.Add(node);
         AppendNode(node);
      }

      private void Error(string message)
      {
         throw new SemanticErrorException(message);
      }

      private void Error(string message, INode node)
      {
         throw new SemanticErrorException(message, node);
      }

      private void Error(string message, IToken token)
      {
         throw new SemanticErrorException(message, token);
      }

      private void AppendNode(INode node)
      {
         if (_currentNode != null)
            _currentNode.Next = node;

         _currentNode = node;
      }

      private void FixupProdRefNodes()
      {
         foreach (var node in _nodes.Where(p => p.NodeType == NodeType.ProdRef))
         {
            var prodRefNode = (ProdRefNode)node;
            var prodInfo = Productions.First(p => p.Key == prodRefNode.ProdName).Value;
            prodRefNode.AltHead = prodInfo.AltHead;
         }
      }

      private void Traverse(string prodName, INode node, ITerminalSet firstSet, bool rollup = true)
      {
         while (node != null)
         {
            switch (node.NodeType)
            {
               case NodeType.AltHead:
                  Debug.WriteLine("AltHead");

                  // figure the first set for each alternative and add them to first set of
                  // the head
                  var alt = ((IAltHeadNode)node).FirstAlt;
                  while (alt != null)
                  {
                     Traverse(prodName, alt.Next, alt.FirstSet);
                     Debug.WriteLine("First(alt) = " + alt.FirstSet);

                     try
                     {
                        ((AltHeadNode)node).FirstSet.Add(alt.FirstSet);
                     }
                     catch (Exception)
                     {
                        Error("Duplicate terminal in first set of <" + prodName + "> " + '\n' +
                              "First(<" + prodName + ">)=[" +
                              ((AltHeadNode)node).FirstSet.DelimitedText() + ']',
                              node);
                     }

                     if (alt.FirstSet.IncludesEpsilon)
                        ((AltHeadNode)node).FirstSet.IncludesEpsilon = true;

                     Debug.WriteLine("First(head) = " + ((AltHeadNode)node).FirstSet);

                     alt = alt.NextAlt;
                  }

                  if (rollup)
                  {
                     // add the first set of the head to the first set of <firstSet>
                     try
                     {
                        firstSet.Add(((AltHeadNode)node).FirstSet);
                     }
                     catch (Exception)
                     {
                        Error("Duplicate terminal in first set of <" + prodName + "> " +
                              '\n' +
                              "First(<" + prodName + ">)=[" + firstSet.DelimitedText() + "]",
                           node);
                     }

                     if (((AltHeadNode)node).FirstSet.IncludesEpsilon)
                        firstSet.IncludesEpsilon = true;

                     Debug.WriteLine("First(firstSet) = " + firstSet);
                  }

                  break;

               case NodeType.ProdRef:
                  Debug.WriteLine("ProdRef - " + ((ProdRefNode)node).ProdName);
                  ComputeFirst(((ProdRefNode)node).ProdName);
                  var prodInfo = Productions.First(p => p.Key == ((ProdRefNode)node).ProdName).Value;
                  try
                  {
                     firstSet.Add(prodInfo.AltHead.FirstSet);
                  }
                  catch (Exception)
                  {
                     Error("Duplicate terminal in first set of <" + prodName + "> " +
                           "found in <" + ((ProdRefNode)node).ProdName + ">" + '\n' +
                           "First(<" + prodName + ">)=[" + firstSet.DelimitedText() + "]" + '\n' +
                           "First(<" + ((ProdRefNode)node).ProdName + ">)=[" +
                           prodInfo.AltHead.FirstSet.DelimitedText() + "]", node);
                  }
                  firstSet.IncludesEpsilon = prodInfo.AltHead.FirstSet.IncludesEpsilon;
                  if (!prodInfo.AltHead.FirstSet.IncludesEpsilon)
                     rollup = false;
                  break;

               case NodeType.TermName:
                  Debug.WriteLine("TermName - " + ((TerminalNode)node).TermName);

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

               case NodeType.Alternative:
                  Debug.WriteLine("Alternative");
                  // should never get here
                  break;

               case NodeType.ActName:
                  Debug.WriteLine("ActName");
                  // do nothing
                  break;

               case NodeType.LParen:
                  Debug.WriteLine("ntLParens");
                  // do nothing
                  break;

               case NodeType.BeginOption:
                  Debug.WriteLine("BeginOption");
                  // do nothing
                  break;

               case NodeType.BeginKleene:
                  Debug.WriteLine("BeginKleene");
                  // do nothing
                  break;

               case NodeType.RParen:
                  Debug.WriteLine("ntRParens");
                  if (!firstSet.IncludesEpsilon)
                     rollup = false;
                  break;

               case NodeType.EndOption:
               case NodeType.EndKleene:
                  Debug.WriteLine(node.NodeType == NodeType.EndOption ? "EndOption" : "EndKleene");

                  if (node.Next == null)
                     firstSet.IncludesEpsilon = true;
                  break;
            }

            node = node.Next;
            if (rollup)
               continue;

            // skip until no more nodes or a AltHead node
            while ((node != null) && (node.NodeType != NodeType.AltHead))
               node = node.Next;
         }
      }

      private void ComputeFirst(string prodName)
      {
         if (!Productions.TryGetValue(prodName, out var prodInfo))
            Error("Production not defined:" + prodName);

         if (prodInfo == null || !prodInfo.AltHead.FirstSet.IsEmpty())
            return;

         Debug.WriteLine("BEGIN PRODUCTION - " + prodName);
         Traverse(prodName, prodInfo.AltHead, prodInfo.AltHead.FirstSet);
         Debug.WriteLine("END PRODUCTION - " + prodName);
      }

      private void BuildReferences(string prodName, IAltHeadNode altHead)
      {
         var alt = altHead.FirstAlt;
         while (alt != null)
         {
            var node = alt.Next;

            while (node != null)
            {
               switch (node.NodeType)
               {
                  case NodeType.AltHead:
                     BuildReferences(prodName, (AltHeadNode)node);
                     break;

                  case NodeType.Alternative:
                     Error("Programming Error: an alternative should never follow an alternative");
                     break;

                  case NodeType.ProdRef:
                     var name = ((ProdRefNode)node).ProdName;
                     if (!Productions.ContainsKey(name))
                        Error("Undefined production: <" + name + ">", node);

                     Productions[name].AddReference(prodName);
                     break;

                  case NodeType.TermName:
                     break;

                  case NodeType.BeginOption:
                     break;

                  case NodeType.EndOption:
                     break;

                  case NodeType.BeginKleene:
                     break;

                  case NodeType.EndKleene:
                     break;
               }
               node = node.Next;
            }

            alt = alt.NextAlt;
         }
      }
   }
}
