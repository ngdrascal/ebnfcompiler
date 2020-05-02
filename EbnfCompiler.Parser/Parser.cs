using System.Linq;
using EbnfCompiler.AST;
using EbnfCompiler.Compiler;
using EbnfCompiler.Scanner;

namespace EbnfCompiler.Parser
{
   public class Parser
   {
      private readonly IScanner _scanner;
      private readonly IAstBuilder _astBuilder;

      public Parser(IScanner scanner, IAstBuilder astBuilder)
      {
         _scanner = scanner;
         _astBuilder = astBuilder;
      }

      private void Match(TokenKind tokenKind)
      {
         if (_scanner.CurrentToken.TokenKind != tokenKind)
            throw new SyntaxErrorException(tokenKind, _scanner.CurrentToken);
      }

      public IRootNode ParseGoal()
      {
         ParseInput();
         Match(TokenKind.Eof);
         _scanner.Advance();

         return new RootNode(_astBuilder.TokenDefinitions, _astBuilder.SyntaxTree);
      }

      // <Input> ::= <Tokens> <Grammar> .
      private void ParseInput()
      {
         ParseTokens();
         ParseGrammar();
      }

      // <tokens> ::= "%TOKENS%" { <TokenDef> } .
      private void ParseTokens()
      {
         Match(TokenKind.TokensTag);
         _scanner.Advance();
         while (_scanner.CurrentToken.TokenKind == TokenKind.String)
            ParseTokenDef();
      }

      // <TokenDef> ::= "STRING" "=" "STRING" .
      private void ParseTokenDef()
      {
         _astBuilder.AddTokenName(_scanner.CurrentToken);
         Match(TokenKind.String);
         _scanner.Advance();
         Match(TokenKind.Equal);
         _scanner.Advance();
         _astBuilder.SetTokenDef(_scanner.CurrentToken);
         Match(TokenKind.String);
         _scanner.Advance();
      }

      // <Grammar> ::= "%EBNF%" <Syntax> .
      private void ParseGrammar()
      {
         Match(TokenKind.EbnfTag);
         _scanner.Advance();
         ParseSyntax();
      }

      // <Syntax> ::= <statement> { <Statement> } .
      private void ParseSyntax()
      {
         ParseAction();

         _astBuilder.BeginSyntax(_scanner.CurrentToken);

         ParseStatement();

         var statementStartTokens = new[]
         {
            TokenKind.Identifier, TokenKind.Action
         };

         while (statementStartTokens.Contains(_scanner.CurrentToken.TokenKind))
            ParseStatement();

         ParseAction();

         _astBuilder.EndSyntax();
      }

      // <Statement> ::= "PRODNAME" "::=" <Expression> "." .
      private void ParseStatement()
      {
         ParseAction();

         _astBuilder.BeginStatement(_scanner.CurrentToken);

         Match(TokenKind.Identifier);
         _scanner.Advance();

         Match(TokenKind.Assign);
         _scanner.Advance();

         ParseExpression();

         Match(TokenKind.Period);
         _scanner.Advance();

         ParseAction();

         _astBuilder.EndStatement();
      }

      // <Expression> ::= <Term> { "|" <Term> } .
      private void ParseExpression()
      {
         ParseAction();
         
         _astBuilder.BeginExpression(_scanner.CurrentToken);
         
         ParseTerm();
         
         while (_scanner.CurrentToken.TokenKind == TokenKind.Or)
         {
            _scanner.Advance();
            ParseTerm();
         }

         ParseAction();

         _astBuilder.EndExpression();
      }

      // <Term> ::= <Factor> { <Action> } .
      private void ParseTerm()
      {
         ParseAction();

         _astBuilder.BeginTerm(_scanner.CurrentToken);
         
         ParseFactor();

         var factorStartTokens = new[]
         {
            TokenKind.Identifier, TokenKind.String, TokenKind.Action,
            TokenKind.LeftParen, TokenKind.LeftBracket, TokenKind.LeftBrace
         };
         while (factorStartTokens.Contains(_scanner.CurrentToken.TokenKind))
         {
            ParseFactor();
         }

         ParseAction();

         _astBuilder.EndTerm();
      }

      // <Factor> ::= "PRODNAME" |
      //              "STRING" |
      //              "(" <Expression> ")" |
      //              "[" <Expression> "]" |
      //              "{" <Expression> "}" .
      private void ParseFactor()
      {
         ParseAction();

         _astBuilder.BeginFactor(_scanner.CurrentToken);

         switch (_scanner.CurrentToken.TokenKind)
         {
            case TokenKind.Identifier:
               _astBuilder.FoundProduction(_scanner.CurrentToken);
               _scanner.Advance();
               break;
            case TokenKind.String:
               _astBuilder.FoundTerminal(_scanner.CurrentToken);
               _scanner.Advance();
               break;
            case TokenKind.LeftParen:
               _astBuilder.BeginParens(_scanner.CurrentToken);
               _scanner.Advance();
               ParseExpression();
               Match(TokenKind.RightParen);
               _astBuilder.EndParens();
               _scanner.Advance();
               break;
            case TokenKind.LeftBracket:
               _astBuilder.BeginOption(_scanner.CurrentToken);
               _scanner.Advance();
               ParseExpression();
               Match(TokenKind.RightBracket);
               _astBuilder.EndOption();
               _scanner.Advance();
               break;
            case TokenKind.LeftBrace:
               _astBuilder.BeginKleene(_scanner.CurrentToken);
               _scanner.Advance();
               ParseExpression();
               Match(TokenKind.RightBrace);
               _astBuilder.EndKleene();
               _scanner.Advance();
               break;
         }

         ParseAction();

         _astBuilder.EndFactor();
      }

      // <Action> ::= [ #FoundAction# ""ACTION"" ] .
      // First = [Action, <epsilon>]
      private void ParseAction()
      {
         if (_scanner.CurrentToken.TokenKind == TokenKind.Action)
         {
            _astBuilder.FoundAction(_scanner.CurrentToken);
            Match(TokenKind.Action);
            _scanner.Advance();
         }
      }
   }
}
