using EbnfCompiler.Compiler;

namespace EbnfCompiler.AST
{
   public abstract class Node : INode
   {
      protected readonly TerminalSet _firstSet = new TerminalSet();

      public ISourceLocation Location { get; set; }
      public NodeType NodeType { get; }
      public string Image { get; protected set; }
      public INode Next { get; set; }

      public ITerminalSet FirstSet
      {
         get
         {
            if (_firstSet.IsEmpty())
               CalcFirstSet();
            return _firstSet;
         }
      }

      protected Node(NodeType nodeType, IToken token)
      {
         NodeType = nodeType;
         Location = token.Location;
         Image = token.Image;
      }

      protected abstract void CalcFirstSet();

      public override string ToString()
      {
         return Image;
      }
   }

   public class AltHeadNode : Node, IAltHeadNode
   {
      public int AltCount { get; set; }

      public IAlternativeNode FirstAlt { get; set; }

      public AltHeadNode(IToken token)
         : base(NodeType.AltHead, token)
      {
         Image = string.Empty;
         AltCount = 0;
         FirstAlt = null;
      }

      public override string ToString()
      {
         var result = string.Empty;

         var alt = FirstAlt;
         while (alt != null)
         {
            result += alt.ToString();
            alt = alt.NextAlt;
            if (alt != null)
               result += "|";
         }
         return result;
      }

      protected override void CalcFirstSet()
      {
         _firstSet.IncludesEpsilon = false;
         var alt = FirstAlt;
         while (alt != null)
         {
            _firstSet.Add(alt.FirstSet);
            if (alt.FirstSet.IncludesEpsilon)
            {
               alt.FirstSet.IncludesEpsilon = true;
            }
            alt = alt.NextAlt;
         }
      }
   }

   public class AlternativeNode : Node, IAlternativeNode
   {
      public AlternativeNode(IToken token)
         : base(NodeType.Alternative, token)
      {
      }

      public IAlternativeNode NextAlt { get; set; }

      protected override void CalcFirstSet()
      {
         _firstSet.Add(Next.FirstSet);
         _firstSet.IncludesEpsilon = Next.FirstSet.IncludesEpsilon;
      }

      public override string ToString()
      {
         var result = string.Empty;
         var node = Next;
         while (node != null)
         {
            result += node.ToString();
            node = node.Next;
         }
         return result;
      }
   }

   public class ProdRefNode : Node, IProdRefNode
   {
      public string ProdName { get; }
      public IAltHeadNode AltHead { get; set; }

      public ProdRefNode(IToken token)
         : base(NodeType.ProdRef, token)
      {
         ProdName = token.Image;
         //AltHead = null;
      }

      public override string ToString()
      {
         return "<" + Image + ">";
      }

      protected override void CalcFirstSet()
      {
         //FirstSet.Add(AltHead.FirstSet);
         //FirstSet.IncludesEpsilon = AltHead.FirstSet.IncludesEpsilon;
      }
   }

   public class TerminalNode : Node, ITerminalNode
   {
      public string TermName { get; private set; }

      public TerminalNode(IToken token, string enumImage)
         : base(NodeType.TermName, token)
      {
         Image = '"' + Image + '"';

         TermName = enumImage;
      }

      protected override void CalcFirstSet()
      {
         _firstSet.Add(TermName);
         _firstSet.IncludesEpsilon = false;
      }
   }

   public class ActionNode : Node, IActionNode
   {
      public string ActName { get; private set; }

      public ActionNode(IToken token)
         : base(NodeType.ActName, token)
      {
         Image = '#' + Image + '#';
         ActName = token.Image;
      }

      protected override void CalcFirstSet()
      {
         if (Next != null)
         {
            _firstSet.Add(Next.FirstSet);
            _firstSet.IncludesEpsilon = Next.FirstSet.IncludesEpsilon;
         }
         else
            _firstSet.IncludesEpsilon = false;
      }
   }

   public class LParenNode : Node
   {
      public RParenNode Mate { get; set; }

      public LParenNode(IToken token)
         : base(NodeType.LParen, token)
      {
         Mate = null;
      }

      protected override void CalcFirstSet()
      {
         _firstSet.Add(Next.FirstSet);
         _firstSet.IncludesEpsilon = Next.FirstSet.IncludesEpsilon;
      }
   }

   public class RParenNode : Node
   {
      public LParenNode Mate { get; set; }

      public RParenNode(IToken token)
         : base(NodeType.RParen, token)
      {
         Mate = null;
      }

      protected override void CalcFirstSet()
      {
         if (!Mate.FirstSet.IncludesEpsilon)
            return;

         if (Next == null)
            return;

         Mate.FirstSet.Add(Next.FirstSet);
         Mate.FirstSet.IncludesEpsilon = Next.FirstSet.IncludesEpsilon;
      }
   }

   public class LOptionNode : Node
   {
      public ROptionNode Mate { get; set; }

      public LOptionNode(IToken token)
         : base(NodeType.BeginOption, token)
      {
         Mate = null;
      }

      protected override void CalcFirstSet()
      {
         _firstSet.Add(Next.FirstSet);
         _firstSet.Add(Mate.FirstSet);
         _firstSet.IncludesEpsilon = Mate.FirstSet.IncludesEpsilon;
      }
   }

   public class ROptionNode : Node
   {
      public LOptionNode Mate { get; set; }

      public ROptionNode(IToken token)
         : base(NodeType.EndOption, token)
      {
         Mate = null;
      }

      protected override void CalcFirstSet()
      {
         if (Next != null)
         {
            _firstSet.Add(Next.FirstSet);
            _firstSet.IncludesEpsilon = Next.FirstSet.IncludesEpsilon;
         }
         _firstSet.IncludesEpsilon = true;
      }
   }

   public class LKleeneNode : Node
   {
      public RKleeneNode Mate { get; set; }

      public LKleeneNode(IToken token)
         : base(NodeType.BeginKleene, token)
      {
         Mate = null;
      }

      protected override void CalcFirstSet()
      {
         _firstSet.Add(Next.FirstSet);
         _firstSet.Add(Mate.FirstSet);
         _firstSet.IncludesEpsilon = Mate.FirstSet.IncludesEpsilon;
      }
   }

   public class RKleeneNode : Node
   {
      public LKleeneNode Mate { get; set; }

      public RKleeneNode(IToken token)
         : base(NodeType.EndKleene, token)
      {
         Mate = null;
      }

      protected override void CalcFirstSet()
      {
         if (Next != null)
         {
            _firstSet.Add(Next.FirstSet);
            _firstSet.IncludesEpsilon = Next.FirstSet.IncludesEpsilon;
         }
         else
            _firstSet.IncludesEpsilon = true;
      }
   }
}