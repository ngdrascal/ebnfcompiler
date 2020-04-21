using System;
using System.Collections.Generic;
using System.Linq;
using EbnfCompiler.AST;
using EbnfCompiler.AST.Impl;

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
         var indent = new string(' ', _indentLevel);
         Append(indent);
         Append(s);
         Append("\n");
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

      private void GenTerm(IAstNode node)
      {
         while (node != null)
         {
            switch (node.AstNodeType)
            {
               case AstNodeType.Expression:
                  break;

               case AstNodeType.Term:
                  break;

               case AstNodeType.ProdRef:
                  AppendLine("Parse" + ((IProdRefNode)node).ProdName + "();");
                  break;

               case AstNodeType.Terminal:
                  AppendLine("Match(" + ((ITerminalNode)node).TermName + ");");
                  break;

               case AstNodeType.Action:
                  AppendLine("semantics." + ((IActionNode)node).ActionName + "();");
                  break;

               case AstNodeType.Paren:
                  //            if node^.next^.firstSetOfHead.IsEmpty then begin
                  //               FirstOf(node^.next, node^.next^.firstSetOfHead);
                  //            end;

                  var parenNode = ((IParenNode) (node));
                  if (!parenNode.Expression.FirstSet.IncludesEpsilon)
                  {
                     AppendLine("CheckTokenOneOf([" + parenNode.Expression.FirstSet.DelimitedText() + "]);");
                  }
                  GenAlt((IExpressionNode)node.AsExpression());
                  break;

               case AstNodeType.Option:
                  //            if node^.next^.firstSetOfHead.IsEmpty then begin
                  //               FirstOf(node^.next, node^.next^.firstSetOfHead);
                  //            end;
                  var optionNode = ((IOptionNode)(node));
                  AppendLine("if token.kind in [" + optionNode.Expression.FirstSet.DelimitedText() + "] then begin");
                  Indent();
                  GenAlt(optionNode.Expression);
                  Outdent();
                  AppendLine("}");
                  break;

               case AstNodeType.KleeneStar:
                  //            if node^.next^.firstSetOfHead.IsEmpty then begin
                  //               FirstOf(node^.next, node^.next^.firstSetOfHead);
                  //            end;
                  var kleeneNode = ((IKleeneStarNode)(node));
                  AppendLine("while token.kind in [" +
                             kleeneNode.Expression.FirstSet.DelimitedText() +
                           "] do begin");
                  Indent();
                  GenAlt(kleeneNode.Expression);
                  Outdent();
                  AppendLine("}");
                  break;
            }
            //node = node.Next;
         }
      }

      private void GenAltCase(ITermNode node)
      {
         Indent();
         AppendLine("case " + node.FirstSet.DelimitedText() + " :");
         Indent();
         GenTerm(node.NextTerm);
         Outdent();
         AppendLine("}");
         Outdent();
      }

      private void GenAlt(IExpressionNode expression)
      {
         // if there is more than one term
         if (expression.FirstTerm?.NextTerm != null)
         {
            if (!expression.FirstSet.IncludesEpsilon)
            {
               AppendLine("CheckTokenOneOf([" + expression.FirstSet.DelimitedText() + "]);");
            }
         
            AppendLine("case token.kind of");
         
            var term = expression.FirstTerm;
            while (term != null)
            {
               GenAltCase(term);
               term = term.NextTerm;
            }
         
            AppendLine("}");
         }
         else
            GenTerm(expression.FirstTerm);
      }

      private void GenMethodBody(IProductionInfo prodInfo)
      {
         AppendLine("// First = " + prodInfo.RightHandSide.FirstSet);
         AppendLine("void Parse" + prodInfo.Name + ";");
         AppendLine("{");

         Indent();

         var term = prodInfo.RightHandSide.FirstTerm;
         while (term != null)
         {
            if (term.NextTerm.AstNodeType != AstNodeType.Terminal)
               break;
            term = term.NextTerm;
         }

         if (term == null)
            AppendLine("CheckTokenOneOf([" + prodInfo.RightHandSide.FirstSet.DelimitedText() + "]);");

         GenAlt(prodInfo.RightHandSide);
         Outdent();
         AppendLine("}");
         AppendLine();
      }

      private void GenImplementation()
      {
         AppendLine("using System.Collections.Generic;");
         AppendLine("using System.Linq;");
         AppendLine("using EbnfCompiler.AST;");
         AppendLine("using EbnfCompiler.Compiler;");
         AppendLine("using EbnfCompiler.Scanner;");
         AppendLine();
         AppendLine("namespace Compiler.Parser");
         AppendLine();
         AppendLine("////////////////////////////////////////////////////////////////////////////////");
         AppendLine("// CLASS: SyntaxErrorException");
         AppendLine("////////////////////////////////////////////////////////////////////////////////");
         AppendLine();
         AppendLine("   public class SyntaxErrorException : Exception");
         AppendLine("   {");
         AppendLine("      private readonly IToken _token;");
         AppendLine();
         AppendLine("      public SyntaxErrorException(string message, IToken token)");
         AppendLine("         : base(message)");
         AppendLine("      {");
         AppendLine("         _token = token;");
         AppendLine("      }");
         AppendLine();
         AppendLine("      public IToken Token => _token;");
         AppendLine("   }");
         AppendLine();
         AppendLine();
         AppendLine("////////////////////////////////////////////////////////////////////////////////");
         AppendLine("// CLASS: Parser");
         AppendLine("////////////////////////////////////////////////////////////////////////////////");
         AppendLine();
         AppendLine("   public class SyntaxErrorException : Exception");
         AppendLine("   {");

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
