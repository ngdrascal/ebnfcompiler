using System.Linq;

namespace EbnfCompiler.Sample.Impl
{
   public partial class Parser
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

      private void MatchOneOf(TokenKind[] tokenSet)
      {
         if (!tokenSet.Contains(_scanner.CurrentToken.TokenKind))
            throw new SyntaxErrorException(tokenSet, _scanner.CurrentToken);
      }

      public IRootNode ParseGoal()
      {
         ParseStatementList();
         Match(TokenKind.Eof);
         return BuildRootNode();
      }

      private void ParseStatementList()
      {
         var firstSetOfStatement1 = new[]
         {
            TokenKind.Let, TokenKind.Print
         };
         MatchOneOf(firstSetOfStatement1);
         ParseStatement();
         _astBuilder.StmtEnd(_scanner.CurrentToken);
         Match(TokenKind.Semicolon);
         _scanner.Advance();
         var firstSetOfKleeneStar1 = new[]
         {
            TokenKind.Let, TokenKind.Print
         };
         while (firstSetOfKleeneStar1.Contains(_scanner.CurrentToken.TokenKind))
         {
            ParseStatement();
            _astBuilder.StmtEnd(_scanner.CurrentToken);
            Match(TokenKind.Semicolon);
            _scanner.Advance();
         }
      }

      private void ParseStatement()
      {
         var firstSetOfStatement2 = new[]
         {
            TokenKind.Let, TokenKind.Print
         };
         MatchOneOf(firstSetOfStatement2);
         switch (_scanner.CurrentToken.TokenKind)
         {
            case TokenKind.Let:
               ParseVarDeclaration();
               break;
            case TokenKind.Print:
               ParsePrintStmt();
               break;
         }
      }

      private void ParseVarDeclaration()
      {
         Match(TokenKind.Let);

         _astBuilder.VarStmtStart(_scanner.CurrentToken);
         Match(TokenKind.Let);
         _scanner.Advance();
         _astBuilder.VarStmtIdent(_scanner.CurrentToken);
         Match(TokenKind.Identifier);
         _scanner.Advance();
         Match(TokenKind.Colon);
         _scanner.Advance();
         _astBuilder.VarStmtType(_scanner.CurrentToken);
         ParseType();
         Match(TokenKind.Assign);
         _scanner.Advance();
         ParseExpression();
         _astBuilder.VarStmtEnd(_scanner.CurrentToken);
      }

      private void ParseType()
      {
         var firstSetOfStatement4 = new[]
         {
            TokenKind.Number, TokenKind.String
         };
         MatchOneOf(firstSetOfStatement4);
         switch (_scanner.CurrentToken.TokenKind)
         {
            case TokenKind.Number:
               Match(TokenKind.Number);
               _scanner.Advance();
               break;
            case TokenKind.String:
               Match(TokenKind.String);
               _scanner.Advance();
               break;
         }
      }

      private void ParseExpression()
      {
         var firstSetOfStatement5 = new[]
         {
            TokenKind.Plus, TokenKind.Minus, TokenKind.LeftParen, TokenKind.Identifier, TokenKind.NumberLiteral, TokenKind.StringLiteral
         };
         MatchOneOf(firstSetOfStatement5);
         switch (_scanner.CurrentToken.TokenKind)
         {
            case TokenKind.Plus:
            case TokenKind.Minus:
               ParseSign();
               ParseTerm();
               _astBuilder.UnaryOpEnd(_scanner.CurrentToken);
               var firstSetOfKleeneStar2 = new[]
               {
                  TokenKind.Plus, TokenKind.Minus
               };
               while (firstSetOfKleeneStar2.Contains(_scanner.CurrentToken.TokenKind))
               {
                  _astBuilder.BinaryOp(_scanner.CurrentToken);
                  ParseAddOperator();
                  ParseTerm();
                  _astBuilder.BinaryOpEnd(_scanner.CurrentToken);
               }
               break;
            case TokenKind.LeftParen:
            case TokenKind.Identifier:
            case TokenKind.NumberLiteral:
            case TokenKind.StringLiteral:
               ParseTerm();
               var firstSetOfKleeneStar3 = new[]
               {
                  TokenKind.Plus, TokenKind.Minus
               };
               while (firstSetOfKleeneStar3.Contains(_scanner.CurrentToken.TokenKind))
               {
                  _astBuilder.BinaryOp(_scanner.CurrentToken);
                  ParseAddOperator();
                  ParseTerm();
                  _astBuilder.BinaryOpEnd(_scanner.CurrentToken);
               }
               break;
         }
      }

      private void ParseTerm()
      {
         var firstSetOfStatement6 = new[]
         {
            TokenKind.LeftParen, TokenKind.Identifier, TokenKind.NumberLiteral, TokenKind.StringLiteral
         };
         MatchOneOf(firstSetOfStatement6);
         ParseFactor();
         var firstSetOfKleeneStar4 = new[]
         {
            TokenKind.Asterisk, TokenKind.ForwardSlash
         };
         while (firstSetOfKleeneStar4.Contains(_scanner.CurrentToken.TokenKind))
         {
            _astBuilder.BinaryOp(_scanner.CurrentToken);
            ParseMultOperator();
            ParseFactor();
            _astBuilder.BinaryOpEnd(_scanner.CurrentToken);
         }
      }

      private void ParseFactor()
      {
         var firstSetOfStatement7 = new[]
         {
            TokenKind.LeftParen, TokenKind.Identifier, TokenKind.NumberLiteral, TokenKind.StringLiteral
         };
         MatchOneOf(firstSetOfStatement7);
         switch (_scanner.CurrentToken.TokenKind)
         {
            case TokenKind.LeftParen:
               Match(TokenKind.LeftParen);
               _scanner.Advance();
               ParseExpression();
               Match(TokenKind.RightParen);
               _scanner.Advance();
               break;
            case TokenKind.Identifier:
               _astBuilder.FactIdent(_scanner.CurrentToken);
               Match(TokenKind.Identifier);
               _scanner.Advance();
               break;
            case TokenKind.NumberLiteral:
            case TokenKind.StringLiteral:
               ParseConstantLiteral();
               break;
         }
      }

      private void ParseSign()
      {
         var firstSetOfStatement8 = new[]
         {
            TokenKind.Plus, TokenKind.Minus
         };
         MatchOneOf(firstSetOfStatement8);
         _astBuilder.UnaryOp(_scanner.CurrentToken);
         switch (_scanner.CurrentToken.TokenKind)
         {
            case TokenKind.Plus:
               Match(TokenKind.Plus);
               _scanner.Advance();
               break;
            case TokenKind.Minus:
               Match(TokenKind.Minus);
               _scanner.Advance();
               break;
         }
      }

      private void ParseAddOperator()
      {
         var firstSetOfStatement9 = new[]
         {
            TokenKind.Plus, TokenKind.Minus
         };
         MatchOneOf(firstSetOfStatement9);
         switch (_scanner.CurrentToken.TokenKind)
         {
            case TokenKind.Plus:
               Match(TokenKind.Plus);
               _scanner.Advance();
               break;
            case TokenKind.Minus:
               Match(TokenKind.Minus);
               _scanner.Advance();
               break;
         }
      }

      private void ParseMultOperator()
      {
         var firstSetOfStatement10 = new[]
         {
            TokenKind.Asterisk, TokenKind.ForwardSlash
         };
         MatchOneOf(firstSetOfStatement10);
         switch (_scanner.CurrentToken.TokenKind)
         {
            case TokenKind.Asterisk:
               Match(TokenKind.Asterisk);
               _scanner.Advance();
               break;
            case TokenKind.ForwardSlash:
               Match(TokenKind.ForwardSlash);
               _scanner.Advance();
               break;
         }
      }

      private void ParseConstantLiteral()
      {
         var firstSetOfStatement11 = new[]
         {
            TokenKind.NumberLiteral, TokenKind.StringLiteral
         };
         MatchOneOf(firstSetOfStatement11);
         switch (_scanner.CurrentToken.TokenKind)
         {
            case TokenKind.NumberLiteral:
               _astBuilder.NumLiteral(_scanner.CurrentToken);
               Match(TokenKind.NumberLiteral);
               _scanner.Advance();
               break;
            case TokenKind.StringLiteral:
               _astBuilder.StrLiteral(_scanner.CurrentToken);
               Match(TokenKind.StringLiteral);
               _scanner.Advance();
               break;
         }
      }

      private void ParsePrintStmt()
      {
         Match(TokenKind.Print);

         _astBuilder.PrintStart(_scanner.CurrentToken);
         Match(TokenKind.Print);
         _scanner.Advance();
         Match(TokenKind.LeftParen);
         _scanner.Advance();
         ParseExprList();
         Match(TokenKind.RightParen);
         _scanner.Advance();
      }

      private void ParseExprList()
      {
         var firstSetOfStatement13 = new[]
         {
            TokenKind.Plus, TokenKind.Minus, TokenKind.LeftParen, TokenKind.Identifier, TokenKind.NumberLiteral, TokenKind.StringLiteral
         };
         MatchOneOf(firstSetOfStatement13);
         ParseExpression();
         _astBuilder.PrintExprEnd(_scanner.CurrentToken);
         while (_scanner.CurrentToken.TokenKind == TokenKind.Comma)
         {
            Match(TokenKind.Comma);
            _scanner.Advance();
            ParseExpression();
            _astBuilder.PrintExprEnd(_scanner.CurrentToken);
         }
      }
   }
}
