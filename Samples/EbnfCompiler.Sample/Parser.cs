using System.Linq;
namespace EbnfCompiler.Sample
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
            TokenKind.Var, TokenKind.Print
         };
         MatchOneOf(firstSetOfStatement1);
         ParseStatement();
         _astBuilder.StmtEnd(_scanner.CurrentToken);
         Match(TokenKind.SemiColon);
         _scanner.Advance();

         var firstSetOfKleeneStar1 = new[]
         {
            TokenKind.Var, TokenKind.Print
         };
         while (firstSetOfKleeneStar1.Contains(_scanner.CurrentToken.TokenKind))
         {
            ParseStatement();
            _astBuilder.StmtEnd(_scanner.CurrentToken);
            Match(TokenKind.SemiColon);
            _scanner.Advance();

         }
      }

      private void ParseStatement()
      {
         var firstSetOfStatement2 = new[]
         {
            TokenKind.Var, TokenKind.Print
         };
         MatchOneOf(firstSetOfStatement2);
         switch (_scanner.CurrentToken.TokenKind)
         {
            case TokenKind.Var:
               ParseVarDeclaration();
               break;
            case TokenKind.Print:
               ParsePrintStmt();
               break;
         }
      }

      private void ParseVarDeclaration()
      {
         Match(TokenKind.Var);
         _scanner.Advance();

         _astBuilder.VarStmtStart(_scanner.CurrentToken);
         Match(TokenKind.Var);
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
         var firstSetOfOption1 = new[]
         {
            TokenKind.Plus, TokenKind.Minus
         };
         if (firstSetOfOption1.Contains(_scanner.CurrentToken.TokenKind))
         {
            ParseSign();
         }
         ParseTerm();
         _astBuilder.UnaryOpEnd(_scanner.CurrentToken);
         var firstSetOfKleeneStar2 = new[]
         {
            TokenKind.Plus, TokenKind.Minus
         };
         while (firstSetOfKleeneStar2.Contains(_scanner.CurrentToken.TokenKind))
         {
            _astBuilder.BinaryOp(_scanner.CurrentToken);
            ParseTermOperator();
            ParseTerm();
            _astBuilder.BinaryOpEnd(_scanner.CurrentToken);
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
         var firstSetOfKleeneStar3 = new[]
         {
            TokenKind.Asterisk, TokenKind.ForwardSlash
         };
         while (firstSetOfKleeneStar3.Contains(_scanner.CurrentToken.TokenKind))
         {
            _astBuilder.BinaryOp(_scanner.CurrentToken);
            ParseFactorOperator();
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

      private void ParseTermOperator()
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

      private void ParseFactorOperator()
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
         _scanner.Advance();

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
