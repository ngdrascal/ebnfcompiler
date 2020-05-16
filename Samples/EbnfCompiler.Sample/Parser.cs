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

      public IRootNode ParseGoal()
      {
         ParseStatementList();
         Match(TokenKind.Eof);
         return BuildRootNode();
      }

      private void ParseStatementList()
      {
         ParseStatement();
         _astBuilder.StmtEnd(_scanner.CurrentToken);
         Match(TokenKind.SemiColon);
         _scanner.Advance();

         var firstSetOfKleeneStar1 = new[]
         {
            TokenKind.Var, TokenKind.Print, TokenKind.PrintLine
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
            TokenKind.Plus, TokenKind.Minus, TokenKind.LeftParen, TokenKind.Identifier, TokenKind.NumberLiteral, TokenKind.StringLiteral
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
