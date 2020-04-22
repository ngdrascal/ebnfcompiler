using System.Collections.Generic;
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
            throw new SyntaxErrorException($"Expecting: {tokenKind}", _scanner.CurrentToken);
      }

      public (IReadOnlyCollection<ITokenDefinition> TokenDefinitions,
              IReadOnlyCollection<IProductionInfo> Productions) ParseGoal()
      {
         _scanner.Advance();
         ParseInput();
         Match(TokenKind.Eof);
         _scanner.Advance();

         return (_astBuilder.TokenDefinitions, _astBuilder.Productions);
      }

      // <Input> ::= <Tokens> <Grammar>
      private void ParseInput()
      {
         ParseTokens();
         ParseGrammar();
      }

      // <tokens> ::= "%TOKENS%" {<TokenDef>}
      private void ParseTokens()
      {
         Match(TokenKind.TokensTag);
         _scanner.Advance();
         while (_scanner.CurrentToken.TokenKind == TokenKind.String)
            ParseTokenDef();
      }

      // <TokenDef> ::= "STRING" "=" "STRING"
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

      // <Grammar> ::= "%EBNF%" <Syntax>
      private void ParseGrammar()
      {
         Match(TokenKind.EbnfTag);
         _scanner.Advance();
         ParseSyntax();
      }

      // <Syntax> ::= <statement> { <Statement> } .
      private void ParseSyntax()
      {
         _astBuilder.BeginSyntax();

         ParseStatement();

         while (_scanner.CurrentToken.TokenKind == TokenKind.Identifier)
            ParseStatement();

         _astBuilder.EndSyntax();
      }

      // <Statement> ::= "PRODNAME" "::=" <Expression> ".".
      private void ParseStatement()
      {
         _astBuilder.BeginStatement(_scanner.CurrentToken);
         _scanner.Advance();

         Match(TokenKind.Assign);
         _scanner.Advance();

         ParseExpression();

         Match(TokenKind.Period);
         _scanner.Advance();

         _astBuilder.EndStatement();
      }

      // <Expression> ::= <Term> {"|" <Term>}.
      private void ParseExpression()
      {
         _astBuilder.BeginExpression(_scanner.CurrentToken);
         ParseTerm();
         while (_scanner.CurrentToken.TokenKind == TokenKind.Or)
         {
            _scanner.Advance();
            ParseTerm();
         }

         _astBuilder.EndExpression();
      }

      // <Term> ::= <Action> <Factor> <Action> {<Factor> <Action>}
      private void ParseTerm()
      {
         _astBuilder.BeginTerm(_scanner.CurrentToken);
         ParseActions();
         ParseFactor();
         ParseActions();
         var factorStartTokens = new[]
         {
            TokenKind.Identifier, TokenKind.String, TokenKind.Action,
            TokenKind.LeftParen, TokenKind.LeftBracket, TokenKind.LeftBrace
         };
         while (factorStartTokens.Contains(_scanner.CurrentToken.TokenKind))
         {
            ParseFactor();
            ParseActions();
         }
         _astBuilder.EndTerm();
      }

      // <Factor> ::= "PRODNAME" |
      //              "STRING" |
      //              "(" <Expression> ")" |
      //              "[" <Expression> "]" |
      //              "{" <Expression> "}".
      private void ParseFactor()
      {
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

         _astBuilder.EndFactor();
      }

      // First = [Action, <epsilon>]
      private void ParseActions()
      {
         while (_scanner.CurrentToken.TokenKind == TokenKind.Action)
         {
            _astBuilder.FoundAction(_scanner.CurrentToken);
            Match(TokenKind.Action);
            _scanner.Advance();
         }
      }
   }
}
