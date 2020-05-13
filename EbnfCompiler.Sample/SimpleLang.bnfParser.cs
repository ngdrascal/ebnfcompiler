using System.Linq;
namespace EbnfCompiler.Sample
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

      public void ParseGoal()
      {
         ParseStatementList();
      }

      private void ParseStatementList()
      {
         ParseStatement();
         Match(TokenKind.Semi);
         _scanner.Advance();

         var firstSetOfKleeneStar1 = new[]
         {
            TokenKind.Var, TokenKind.Print, TokenKind.PrintLine
         };
         while (firstSetOfKleeneStar1.Contains(_scanner.CurrentToken.TokenKind))
         {
            ParseStatement();
            Match(TokenKind.Semi);
            _scanner.Advance();

         }
      }

      private void ParseStatement()
      {
         switch (_scanner.CurrentToken.TokenKind)
         {
            case TokenKind.Var:
               ParseVarDeclaration();
               break;
            case TokenKind.Print:
               ParsePrintStmt();
               break;
            case TokenKind.PrintLine:
               ParsePrintlineStmt();
               break;
         }
      }

      private void ParseVarDeclaration()
      {
         Match(TokenKind.Var);
         _scanner.Advance();

         Match(TokenKind.Designator);
         _scanner.Advance();

         Match(TokenKind.Colon);
         _scanner.Advance();

         ParseType();
         Match(TokenKind.Assign);
         _scanner.Advance();

         ParseExpression();
      }

      private void ParseType()
      {
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
         var firstSetOfOption1 = new[]
         {
            TokenKind.Plus, TokenKind.Minus
         };
         if (firstSetOfOption1.Contains(_scanner.CurrentToken.TokenKind))
         {
            ParseSign();
         }
         ParseTerm();
         var firstSetOfKleeneStar2 = new[]
         {
            TokenKind.Plus, TokenKind.Minus
         };
         while (firstSetOfKleeneStar2.Contains(_scanner.CurrentToken.TokenKind))
         {
            ParseTermOperator();
            ParseTerm();
         }
      }

      private void ParseTerm()
      {
         ParseFactor();
         var firstSetOfKleeneStar3 = new[]
         {
            TokenKind.Asterisk, TokenKind.ForwardSlash
         };
         while (firstSetOfKleeneStar3.Contains(_scanner.CurrentToken.TokenKind))
         {
            ParseFactorOperator();
            ParseFactor();
         }
      }

      private void ParseFactor()
      {
         switch (_scanner.CurrentToken.TokenKind)
         {
            case TokenKind.LeftParen:
               Match(TokenKind.LeftParen);
               _scanner.Advance();

               ParseExpression();
               Match(TokenKind.RightParen);
               _scanner.Advance();

               break;
            case TokenKind.Designator:
               Match(TokenKind.Designator);
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
         switch (_scanner.CurrentToken.TokenKind)
         {
            case TokenKind.NumberLiteral:
               Match(TokenKind.NumberLiteral);
               _scanner.Advance();

               break;
            case TokenKind.StringLiteral:
               Match(TokenKind.StringLiteral);
               _scanner.Advance();

               break;
         }
      }

      private void ParsePrintStmt()
      {
         Match(TokenKind.Print);
         _scanner.Advance();

         Match(TokenKind.LeftParen);
         _scanner.Advance();

         ParseExpression();
         Match(TokenKind.RightParen);
         _scanner.Advance();

      }

      private void ParsePrintlineStmt()
      {
         Match(TokenKind.PrintLine);
         _scanner.Advance();

         Match(TokenKind.LeftParen);
         _scanner.Advance();

         var firstSetOfOption2 = new[]
         {
            TokenKind.Plus, TokenKind.Minus, TokenKind.LeftParen, TokenKind.Designator, TokenKind.NumberLiteral, TokenKind.StringLiteral
         };
         if (firstSetOfOption2.Contains(_scanner.CurrentToken.TokenKind))
         {
            ParseExpression();
         }
         Match(TokenKind.RightParen);
         _scanner.Advance();

      }
   }
}
