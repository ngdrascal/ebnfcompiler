using System;
using System.Collections.Generic;
using System.Linq;
using EbnfCompiler.AST;

namespace EbnfCompiler.CodeGenerator
{
   public class CodeGenerator : ICodeGenerator
   {
      private Dictionary<string, IProductionInfo> _productions;
      private int _indentLevel;

      public void Generate(Dictionary<string, IProductionInfo> productions)
      {
         _productions = productions;
         _indentLevel = 0;

         GenInterface();
         GenImplementation();
      }

      public string Output { get; private set; }

      private void Indent()
      {
         _indentLevel++;
      }

      private void Outdent()
      {
         _indentLevel--;
      }

      private void Append(string s)
      {
         Output += s;
      }

      private void AppendLine(string s = "")
      {
         var indent = new String(' ', _indentLevel);
         Append(indent);
         Append(s);
         Append("\n");
      }

      private void FirstOf(IAltHeadNode altHead, ITerminalSet target)
      {
         var alt = altHead.FirstAlt;
         while (alt != null)
         {
            var node = alt.Next;
            while ((node != null) && (node.NodeType == NodeType.ActName))
               node = node.Next;

            if (node == null)
               break;

            switch (node.NodeType)
            {
               case NodeType.AltHead:
                  FirstOf((IAltHeadNode)alt.Next, target);
                  break;

               case NodeType.Alternative:
                  throw new Exception("Programming Error: an alternative should never follow an alternative");

               case NodeType.ProdRef:
                  target.Add(_productions[((IProdRefNode)node).ProdName].AltHead.FirstSet);
                  break;

               case NodeType.TermName:
                  target.Add(((ITerminalNode)node).TermName);
                  break;

               case NodeType.ActName:
                  //FirstOf(node^.next, target);
                  break;

               case NodeType.BeginOption:
                  FirstOf((IAltHeadNode)node.Next, target);
                  break;

               case NodeType.EndOption:
                  break;

               case NodeType.BeginKleene:
                  FirstOf((IAltHeadNode)node.Next, target);
                  break;

               case NodeType.EndKleene:
                  break;
            }
            alt = alt.NextAlt;
         }
      }

      private void GenInterface()
      {
         AppendLine();
         AppendLine("unit Parser; interface");
         AppendLine();
         AppendLine("uses");
         AppendLine("    SysUtils");
         AppendLine("   ,Error");
         AppendLine("   ,SourcePosition");
         AppendLine("   ,Scanner");
         AppendLine("   ,Semantics");
         AppendLine("   ;");
         AppendLine();
         AppendLine("type");
         AppendLine("   ESyntaxError = class(ECompilerError)");
         AppendLine("   public");
         AppendLine("      constructor CreateFromToken(msg : String; token: TToken);");
         AppendLine("   end;");
         AppendLine();
         AppendLine("   TTokenKindSet = set of TTokenKind;");
         AppendLine();
         AppendLine("   TParser = class(TObject)");
         AppendLine("   private");
         AppendLine("      fScanner   : TScanner;");
         AppendLine("      fSemantics : TSemantics;");
         AppendLine("      fToken     : TToken;");
         AppendLine("      fPrevToken : TToken;");
         AppendLine("   protected");
         AppendLine("      procedure CheckTokenOneOf(const tokens : TTokenKindSet);");
         AppendLine("      procedure Match(tokenKind : TTokenKind);");
         AppendLine();

         foreach (var production in _productions)
            AppendLine("      procedure Parse" + production + ";");


         AppendLine();
         AppendLine("      property scanner : TScanner read fScanner;");
         AppendLine("      property semantics : TSemantics read fSemantics;");
         AppendLine("      property token : TToken read fToken write fToken;");
         AppendLine("      property prevToken : TToken read fPrevToken write fPrevToken;");
         AppendLine("   public");
         AppendLine("      constructor Create(scanner : TScanner; semantics : TSemantics);");
         AppendLine();
         AppendLine("      procedure ParseGoal;");
         AppendLine("   end;");
         AppendLine();
      }

      private void GenTerm(INode node)
      {
         while (node != null)
         {
            switch (node.NodeType)
            {
               case NodeType.AltHead:
                  break;

               case NodeType.Alternative:
                  break;

               case NodeType.ProdRef:
                  AppendLine("Parse" + ((IProdRefNode)node).ProdName + ";");
                  break;

               case NodeType.TermName:
                  AppendLine("Match(" + ((ITerminalNode)node).TermName + ");");
                  break;

               case NodeType.ActName:
                  AppendLine("semantics." + ((IActionNode)node).ActName + ";");
                  break;

               case NodeType.LParen:
                  //            if node^.next^.firstSetOfHead.IsEmpty then begin
                  //               FirstOf(node^.next, node^.next^.firstSetOfHead);
                  //            end;

                  if (!node.Next.FirstSet.IncludesEpsilon)
                  {
                     AppendLine("CheckTokenOneOf([" + node.Next.FirstSet.DelimitedText() + "]);");
                  }
                  GenAlt((IAltHeadNode)node.Next);
                  break;

               case NodeType.RParen:
                  break;

               case NodeType.BeginOption:
                  //            if node^.next^.firstSetOfHead.IsEmpty then begin
                  //               FirstOf(node^.next, node^.next^.firstSetOfHead);
                  //            end;
                  AppendLine("if token.kind in [" + node.Next.FirstSet.DelimitedText() + "] then begin");
                  Indent();
                  GenAlt((IAltHeadNode)node.Next);
                  break;

               case NodeType.EndOption:
                  Outdent();
                  AppendLine("end;");
                  break;

               case NodeType.BeginKleene:
                  //            if node^.next^.firstSetOfHead.IsEmpty then begin
                  //               FirstOf(node^.next, node^.next^.firstSetOfHead);
                  //            end;
                  AppendLine("while token.kind in [" +
                           node.Next.FirstSet.DelimitedText() +
                           "] do begin");
                  Indent();
                  GenAlt((IAltHeadNode)node.Next);
                  break;

               case NodeType.EndKleene:
                  Outdent();
                  AppendLine("end;");
                  break;
            }
            node = node.Next;
         }

      }

      private void GenAltCase(IAlternativeNode node)
      {
         Indent();
         AppendLine(node.FirstSet.DelimitedText() + " : begin");
         Indent();
         GenTerm(node.Next);
         Outdent();
         AppendLine("end;");
         Outdent();
      }

      private void GenAlt(IAltHeadNode altHead)
      {
         if (altHead.AltCount > 1)
         {
            if (!altHead.FirstSet.IncludesEpsilon)
            {
               AppendLine("CheckTokenOneOf([" + altHead.FirstSet.DelimitedText() + "]);");
            }

            AppendLine("case token.kind of");

            var alt = altHead.FirstAlt;
            while (alt != null)
            {
               GenAltCase(alt);
               alt = alt.NextAlt;
            }

            AppendLine("end;");
         }
         else
            GenTerm(altHead.FirstAlt);
      }

      private void GenMethodBody(IProductionInfo prodInfo)
      {
         AppendLine("procedure TParser.Parse" + prodInfo + ";");
         AppendLine("// First = " + prodInfo.AltHead.FirstSet);
         AppendLine("begin");

         Indent();

         var alt = prodInfo.AltHead.FirstAlt;
         while (alt != null)
         {
            if (alt.Next.NodeType != NodeType.TermName)
               break;
            alt = alt.NextAlt;
         }

         if (alt == null)
            AppendLine("CheckTokenOneOf([" + prodInfo.AltHead.FirstSet.DelimitedText() + "]);");

         GenAlt(prodInfo.AltHead);
         Outdent();
         AppendLine("end;");
         AppendLine();
         AppendLine();
      }

      private void GenImplementation()
      {
         AppendLine("implementation");
         AppendLine();
         AppendLine("{------------------------------------------------------------------------------}");
         AppendLine("{ CLASS: ESyntaxError                                                          }");
         AppendLine("{------------------------------------------------------------------------------}");
         AppendLine();
         AppendLine("constructor ESyntaxError.CreateFromToken(msg : String; token: TToken);");
         AppendLine("begin");
         AppendLine("   inherited Create(msg, token.startLine, token.startColumn,");
         AppendLine("                         token.stopLine, token.stopColumn);");
         AppendLine("end;");
         AppendLine();
         AppendLine("{------------------------------------------------------------------------------}");
         AppendLine("{ CLASS: TParser                                                               }");
         AppendLine("{------------------------------------------------------------------------------}");
         AppendLine();
         AppendLine("procedure TParser.CheckTokenOneOf(const tokens : TTokenKindSet);");
         AppendLine("var");
         AppendLine("   msg : string;");
         AppendLine("   tk  : TTokenKind;");
         AppendLine();
         AppendLine("begin");
         AppendLine("   if not (token.kind in tokens) then begin");
         AppendLine("      for tk := tkIdent to tkError do begin");
         AppendLine("         if tk in tokens then begin");
         AppendLine("           if msg <> '' then begin");
         AppendLine("              msg := msg + '', '';");
         AppendLine("           end;");
         AppendLine("           msg := msg + TokenKindToStr(tk);");
         AppendLine("         end;");
         AppendLine("      end;");
         AppendLine("      raise ESyntaxError.CreateFromToken");
         AppendLine("        ('Expecting on of the following: ' + msg, token);");
         AppendLine("   end;");
         AppendLine("end;");
         AppendLine();
         AppendLine();
         AppendLine("procedure TParser.Match(tokenKind : TTokenKind);");
         AppendLine("begin");
         AppendLine("   if token.kind = tokenKind then begin");
         AppendLine("      prevToken := token;");
         AppendLine("      token := scanner.NextToken;");
         AppendLine("      UpdateSrcPos(token);");
         AppendLine("   end");
         AppendLine("   else begin");
         AppendLine("      raise ESyntaxError.CreateFromToken");
         AppendLine("        ('Expecting:'  + TokenKindToStr(tokenKind), token);");
         AppendLine("   end;");
         AppendLine("end;");
         AppendLine();
         AppendLine();

         foreach (var production in _productions)
            GenMethodBody(production.Value);

         AppendLine("constructor TParser.Create(scanner : TScanner; semantics : TSemantics);");
         AppendLine("begin");
         AppendLine("   inherited Create;");
         AppendLine("   fScanner := scanner;");
         AppendLine("   fSemantics := semantics;");
         AppendLine("end;");
         AppendLine();
         AppendLine();
         AppendLine("procedure TParser.ParseGoal;");
         AppendLine("begin");
         AppendLine("   token := scanner.NextToken;");

         if (_productions.Count > 0)
            AppendLine("   Parse" + _productions.ElementAt(0) + ";");

         AppendLine("   Match(tkEof);");
         AppendLine("end;");
         AppendLine();
         AppendLine("end.");
      }
   }
}
