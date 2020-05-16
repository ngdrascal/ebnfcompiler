using System.IO;
using System.Text.RegularExpressions;

namespace EbnfCompiler.Sample
{
   public class Scanner : IScanner
   {
      private enum State
      {
         Start,
         Done,
         // ReSharper disable twice IdentifierTypo
         N, Nu, Num, Numb, Numbe, Number,
         P, Pr, Pri, Prin, Print, PrintL, PrintLi, PrintLin, PrintLine,
         S, St, Str, Stri, Strin, String,
         V, Va, Var,
         Ident,
         StringLiteral,
         NumberLiteral1, NumberLiteral2,
         Comment
      };

      private const char ChTab = '\t';
      private const char ChCr = '\r';
      private const char ChLf = '\n';
      private const char ChZero = '\u0000';
      private const char ChEof = '\u001A';
      private const char ChSpace = ' ';

      private readonly StreamReader _input;
      private char _currentCh;
      private State _state;
      private int Line { get; set; }
      private int Column { get; set; }

      public Scanner(Stream stream)
      {
         _input = new StreamReader(stream);

         _currentCh = ' ';
         Line = 1;
         Column = 0;

         Advance();
      }

      public IToken CurrentToken { get; private set; }

      public void Advance()
      {
         CurrentToken = new Token();

         _state = State.Start;
         SkipWhiteSpace();

         do
         {
            switch (_state)
            {
               case State.Start:
                  switch (_currentCh)
                  {
                     case 'n':
                        _state = State.N;
                        CurrentToken.Image = _currentCh.ToString();
                        break;
                     case 'P':
                        _state = State.P;
                        CurrentToken.Image = _currentCh.ToString();
                        break;
                     case 's':
                        _state = State.S;
                        CurrentToken.Image = _currentCh.ToString();
                        break;
                     case 'v':
                        _state = State.V;
                        CurrentToken.Image = "v";
                        break;
                     case '(':
                        _state = State.Done;
                        CurrentToken.TokenKind = TokenKind.LeftParen;
                        CurrentToken.Image = "(";
                        break;
                     case ')':
                        _state = State.Done;
                        CurrentToken.TokenKind = TokenKind.RightParen;
                        CurrentToken.Image = ")";
                        break;
                     case ':':
                        _state = State.Done;
                        CurrentToken.TokenKind = TokenKind.Colon;
                        CurrentToken.Image = _currentCh.ToString();
                        break;
                     case ';':
                        _state = State.Done;
                        CurrentToken.TokenKind = TokenKind.SemiColon;
                        CurrentToken.Image = _currentCh.ToString();
                        break;
                     case '=':
                        _state = State.Done;
                        CurrentToken.TokenKind = TokenKind.Assign;
                        CurrentToken.Image = _currentCh.ToString();
                        break;
                     case '+':
                        _state = State.Done;
                        CurrentToken.TokenKind = TokenKind.Plus;
                        CurrentToken.Image = _currentCh.ToString();
                        break;
                     case '-':
                        _state = State.Done;
                        CurrentToken.TokenKind = TokenKind.Minus;
                        CurrentToken.Image = _currentCh.ToString();
                        break;
                     case '*':
                        _state = State.Done;
                        CurrentToken.TokenKind = TokenKind.Asterisk;
                        CurrentToken.Image = _currentCh.ToString();
                        break;
                     case '/':
                        _state = State.Comment;
                        CurrentToken.Image = _currentCh.ToString();
                        break;
                     case '"':
                        _state = State.StringLiteral;
                        CurrentToken.TokenKind = TokenKind.String;
                        CurrentToken.Image = string.Empty;
                        break;
                     case '0':
                     case '1':
                     case '2':
                     case '3':
                     case '4':
                     case '5':
                     case '6':
                     case '7':
                     case '8':
                     case '9':
                        _state = State.NumberLiteral1;
                        CurrentToken.Image = _currentCh.ToString();
                        break;
                     case ChEof:
                        _state = State.Done;
                        CurrentToken.TokenKind = TokenKind.Eof;
                        CurrentToken.Image = "<eof>";
                        break;

                     default:
                        if (Regex.IsMatch(_currentCh.ToString(), @"^[a-zA-Z]$"))
                        {
                           _state = State.Ident;
                           CurrentToken.Image = _currentCh.ToString();
                        }
                        else
                        {
                           _state = State.Done;
                           CurrentToken.TokenKind = TokenKind.Error;
                        }
                        break;
                  }
                  SetStartPosition(CurrentToken);
                  _currentCh = NextChar();
                  break;

               case State.N:
                  if (_currentCh == 'u')
                  {
                     _state = State.Nu;
                     CurrentToken.Image += _currentCh;
                     _currentCh = NextChar();
                  }
                  else if (Regex.IsMatch(_currentCh.ToString(), @"^[a-tv-zA-Z0-9_]$"))
                  {
                     _state = State.Ident;
                     CurrentToken.Image += _currentCh;
                     _currentCh = NextChar();
                  }
                  else
                  {
                     _state = State.Done;
                     CurrentToken.TokenKind = TokenKind.Identifier;
                  }
                  break;

               case State.Nu:
                  if (_currentCh == 'm')
                  {
                     _state = State.Num;
                     CurrentToken.Image += _currentCh;
                     _currentCh = NextChar();
                  }
                  else if (Regex.IsMatch(_currentCh.ToString(), @"^[a-ln-zA-Z0-9_]$"))
                  {
                     _state = State.Ident;
                     CurrentToken.Image += _currentCh;
                     _currentCh = NextChar();
                  }
                  else
                  {
                     _state = State.Done;
                     CurrentToken.TokenKind = TokenKind.Identifier;
                  }
                  break;

               case State.Num:
                  if (_currentCh == 'b')
                  {
                     _state = State.Numb;
                     CurrentToken.Image += _currentCh;
                     _currentCh = NextChar();
                  }
                  else if (Regex.IsMatch(_currentCh.ToString(), @"^[ac-zA-Z0-9_]$"))
                  {
                     _state = State.Ident;
                     CurrentToken.Image += _currentCh;
                     _currentCh = NextChar();
                  }
                  else
                  {
                     _state = State.Done;
                     CurrentToken.TokenKind = TokenKind.Identifier;
                  }
                  break;

               case State.Numb:
                  if (_currentCh == 'e')
                  {
                     _state = State.Numbe;
                     CurrentToken.Image += _currentCh;
                     _currentCh = NextChar();
                  }
                  else if (Regex.IsMatch(_currentCh.ToString(), @"^[a-df-zA-Z0-9_]$"))
                  {
                     _state = State.Ident;
                     CurrentToken.Image += _currentCh;
                     _currentCh = NextChar();
                  }
                  else
                  {
                     _state = State.Done;
                     CurrentToken.TokenKind = TokenKind.Identifier;
                  }
                  break;

               case State.Numbe:
                  if (_currentCh == 'r')
                  {
                     _state = State.Number;
                     CurrentToken.Image += _currentCh;
                     _currentCh = NextChar();
                  }
                  else if (Regex.IsMatch(_currentCh.ToString(), @"^[a-qs-zA-Z0-9_]$"))
                  {
                     _state = State.Ident;
                     CurrentToken.Image += _currentCh;
                     _currentCh = NextChar();
                  }
                  else
                  {
                     _state = State.Done;
                     CurrentToken.TokenKind = TokenKind.Identifier;
                  }
                  break;

               case State.Number:
                  if (Regex.IsMatch(_currentCh.ToString(), @"^[a-zA-Z0-9_]$"))
                  {
                     _state = State.Ident;
                     CurrentToken.Image += _currentCh;
                     _currentCh = NextChar();
                  }
                  else
                  {
                     _state = State.Done;
                     CurrentToken.TokenKind = TokenKind.Number;
                  }
                  break;

               case State.P:
                  if (_currentCh == 'r')
                  {
                     _state = State.Pr;
                     CurrentToken.Image += _currentCh;
                     _currentCh = NextChar();
                  }
                  else if (Regex.IsMatch(_currentCh.ToString(), @"^[a-qs-zA-Z0-9_]$"))
                  {
                     _state = State.Ident;
                     CurrentToken.Image += _currentCh;
                     _currentCh = NextChar();
                  }
                  else
                  {
                     _state = State.Done;
                     CurrentToken.TokenKind = TokenKind.Identifier;
                  }
                  break;

               case State.Pr:
                  if (_currentCh == 'i')
                  {
                     _state = State.Pri;
                     CurrentToken.Image += _currentCh;
                     _currentCh = NextChar();
                  }
                  else if (Regex.IsMatch(_currentCh.ToString(), @"^[a-hj-zA-Z0-9_]$"))
                  {
                     _state = State.Ident;
                     CurrentToken.Image += _currentCh;
                     _currentCh = NextChar();
                  }
                  else
                  {
                     _state = State.Done;
                     CurrentToken.TokenKind = TokenKind.Identifier;
                  }
                  break;

               case State.Pri:
                  if (_currentCh == 'n')
                  {
                     _state = State.Prin;
                     CurrentToken.Image += _currentCh;
                     _currentCh = NextChar();
                  }
                  else if (Regex.IsMatch(_currentCh.ToString(), @"^[a-mo-zA-Z0-9_]$"))
                  {
                     _state = State.Ident;
                     CurrentToken.Image += _currentCh;
                     _currentCh = NextChar();
                  }
                  else
                  {
                     _state = State.Done;
                     CurrentToken.TokenKind = TokenKind.Identifier;
                  }
                  break;

               case State.Prin:
                  if (_currentCh == 't')
                  {
                     _state = State.Print;
                     CurrentToken.Image += _currentCh;
                     _currentCh = NextChar();
                  }
                  else if (Regex.IsMatch(_currentCh.ToString(), @"^[a-su-zA-Z0-9_]$"))
                  {
                     _state = State.Ident;
                     CurrentToken.Image += _currentCh;
                     _currentCh = NextChar();
                  }
                  else
                  {
                     _state = State.Done;
                     CurrentToken.TokenKind = TokenKind.Identifier;
                  }
                  break;

               case State.Print:
                  if (_currentCh == 'L')
                  {
                     _state = State.PrintL;
                     CurrentToken.Image += _currentCh;
                     _currentCh = NextChar();
                  }
                  else if (Regex.IsMatch(_currentCh.ToString(), @"^[a-zA-KM-Z0-9_]$"))
                  {
                     _state = State.Ident;
                     CurrentToken.Image += _currentCh;
                     _currentCh = NextChar();
                  }
                  else
                  {
                     _state = State.Done;
                     CurrentToken.TokenKind = TokenKind.Print;
                  }
                  break;

               case State.PrintL:
                  if (_currentCh == 'i')
                  {
                     _state = State.PrintLi;
                     CurrentToken.Image += _currentCh;
                     _currentCh = NextChar();
                  }
                  else if (Regex.IsMatch(_currentCh.ToString(), @"^[a-hj-zA-Z0-9_]$"))
                  {
                     _state = State.Ident;
                     CurrentToken.Image += _currentCh;
                     _currentCh = NextChar();
                  }
                  else
                  {
                     _state = State.Done;
                     CurrentToken.TokenKind = TokenKind.Identifier;
                  }
                  break;

               case State.PrintLi:
                  if (_currentCh == 'n')
                  {
                     _state = State.PrintLin;
                     CurrentToken.Image += _currentCh;
                     _currentCh = NextChar();
                  }
                  else if (Regex.IsMatch(_currentCh.ToString(), @"^[a-mo-zA-Z0-9_]$"))
                  {
                     _state = State.Ident;
                     CurrentToken.Image += _currentCh;
                     _currentCh = NextChar();
                  }
                  else
                  {
                     _state = State.Done;
                     CurrentToken.TokenKind = TokenKind.Identifier;
                  }
                  break;

               case State.PrintLin:
                  if (_currentCh == 'e')
                  {
                     _state = State.PrintLine;
                     CurrentToken.Image += _currentCh;
                     _currentCh = NextChar();
                  }
                  else if (Regex.IsMatch(_currentCh.ToString(), @"^[a-df-zA-Z0-9_]$"))
                  {
                     _state = State.Ident;
                     CurrentToken.Image += _currentCh;
                     _currentCh = NextChar();
                  }
                  else
                  {
                     _state = State.Done;
                     CurrentToken.TokenKind = TokenKind.Identifier;
                  }
                  break;

               case State.PrintLine:
                  if (Regex.IsMatch(_currentCh.ToString(), @"^[a-zA-Z0-9_]$"))
                  {
                     _state = State.Ident;
                     CurrentToken.Image += _currentCh;
                     _currentCh = NextChar();
                  }
                  else
                  {
                     _state = State.Done;
                     CurrentToken.TokenKind = TokenKind.PrintLine;
                  }
                  break;

               case State.S:
                  if (_currentCh == 't')
                  {
                     _state = State.St;
                     CurrentToken.Image += _currentCh;
                     _currentCh = NextChar();
                  }
                  else if (Regex.IsMatch(_currentCh.ToString(), @"^[a-su-zA-Z0-9_]$"))
                  {
                     _state = State.Ident;
                     CurrentToken.Image += _currentCh;
                     _currentCh = NextChar();
                  }
                  else
                  {
                     _state = State.Done;
                     CurrentToken.TokenKind = TokenKind.Identifier;
                  }
                  break;

               case State.St:
                  if (_currentCh == 'r')
                  {
                     _state = State.Str;
                     CurrentToken.Image += _currentCh;
                     _currentCh = NextChar();
                  }
                  else if (Regex.IsMatch(_currentCh.ToString(), @"^[a-qs-zA-Z0-9_]$"))
                  {
                     _state = State.Ident;
                     CurrentToken.Image += _currentCh;
                     _currentCh = NextChar();
                  }
                  else
                  {
                     _state = State.Done;
                     CurrentToken.TokenKind = TokenKind.Identifier;
                  }
                  break;

               case State.Str:
                  if (_currentCh == 'i')
                  {
                     _state = State.Stri;
                     CurrentToken.Image += _currentCh;
                     _currentCh = NextChar();
                  }
                  else if (Regex.IsMatch(_currentCh.ToString(), @"^[a-hj-zA-Z0-9_]$"))
                  {
                     _state = State.Ident;
                     CurrentToken.Image += _currentCh;
                     _currentCh = NextChar();
                  }
                  else
                  {
                     _state = State.Done;
                     CurrentToken.TokenKind = TokenKind.Identifier;
                  }
                  break;

               case State.Stri:
                  if (_currentCh == 'n')
                  {
                     _state = State.Strin;
                     CurrentToken.Image += _currentCh;
                     _currentCh = NextChar();
                  }
                  else if (Regex.IsMatch(_currentCh.ToString(), @"^[a-mo-zA-Z0-9_]$"))
                  {
                     _state = State.Ident;
                     CurrentToken.Image += _currentCh;
                     _currentCh = NextChar();
                  }
                  else
                  {
                     _state = State.Done;
                     CurrentToken.TokenKind = TokenKind.Identifier;
                  }
                  break;

               case State.Strin:
                  if (_currentCh == 'g')
                  {
                     _state = State.String;
                     CurrentToken.Image += _currentCh;
                     _currentCh = NextChar();
                  }
                  else if (Regex.IsMatch(_currentCh.ToString(), @"^[a-fh-zA-Z0-9_]$"))
                  {
                     _state = State.Ident;
                     CurrentToken.Image += _currentCh;
                     _currentCh = NextChar();
                  }
                  else
                  {
                     _state = State.Done;
                     CurrentToken.TokenKind = TokenKind.Identifier;
                  }
                  break;

               case State.String:
                  if (Regex.IsMatch(_currentCh.ToString(), @"^[a-zA-Z0-9_]$"))
                  {
                     _state = State.Ident;
                     CurrentToken.Image += _currentCh;
                     _currentCh = NextChar();
                  }
                  else
                  {
                     _state = State.Done;
                     CurrentToken.TokenKind = TokenKind.String;
                  }
                  break;

               case State.V:
                  if (_currentCh == 'a')
                  {
                     _state = State.Va;
                     CurrentToken.Image += _currentCh;
                     _currentCh = NextChar();
                  }
                  else if (Regex.IsMatch(_currentCh.ToString(), @"^[b-zA-Z0-9_]$"))
                  {
                     _state = State.Ident;
                     CurrentToken.Image += _currentCh;
                     _currentCh = NextChar();
                  }
                  else
                  {
                     _state = State.Done;
                     CurrentToken.TokenKind = TokenKind.Identifier;
                  }
                  break;

               case State.Va:
                  if (_currentCh == 'r')
                  {
                     _state = State.Var;
                     CurrentToken.Image += _currentCh;
                     _currentCh = NextChar();
                  }
                  else if (Regex.IsMatch(_currentCh.ToString(), @"^[a-qs-zA-Z0-9_]$"))
                  {
                     _state = State.Ident;
                     CurrentToken.Image += _currentCh;
                     _currentCh = NextChar();
                  }
                  else
                  {
                     _state = State.Done;
                     CurrentToken.TokenKind = TokenKind.Identifier;
                  }
                  break;

               case State.Var:
                  if (Regex.IsMatch(_currentCh.ToString(), @"^[a-zA-Z0-9_]$"))
                  {
                     _state = State.Ident;
                     CurrentToken.Image += _currentCh;
                     _currentCh = NextChar();
                  }
                  else
                  {
                     _state = State.Done;
                     CurrentToken.TokenKind = TokenKind.Var;
                  }
                  break;

               case State.Ident:
                  if (Regex.IsMatch(_currentCh.ToString(), @"^[a-zA-Z0-9_]$"))
                  {
                     CurrentToken.Image += _currentCh;
                     _currentCh = NextChar();
                  }
                  else
                  {
                     _state = State.Done;
                     CurrentToken.TokenKind = TokenKind.Identifier;
                  }
                  break;

               case State.StringLiteral:
                  if (Regex.IsMatch(_currentCh.ToString(), @"^[\x20-!#-~]$"))
                  {
                     CurrentToken.Image += _currentCh;
                     _currentCh = NextChar();
                  }
                  else if (_currentCh == '"')
                  {
                     _state = State.Done;
                     CurrentToken.TokenKind = TokenKind.StringLiteral;
                     _currentCh = NextChar();
                  }
                  else
                  {
                     _state = State.Done;
                     CurrentToken.TokenKind = TokenKind.Error;
                  }
                  break;

               case State.NumberLiteral1:
                  if (Regex.IsMatch(_currentCh.ToString(), @"^[0-9]$"))
                  {
                     CurrentToken.Image += _currentCh;
                     _currentCh = NextChar();
                  }
                  else if (_currentCh == '.')
                  {
                     _state = State.NumberLiteral2;
                     CurrentToken.Image += _currentCh;
                     _currentCh = NextChar();
                  }
                  else
                  {
                     _state = State.Done;
                     CurrentToken.TokenKind = TokenKind.NumberLiteral;
                  }
                  break;

               case State.NumberLiteral2:
                  if (Regex.IsMatch(_currentCh.ToString(), @"^[0-9]$"))
                  {
                     CurrentToken.Image += _currentCh;
                     _currentCh = NextChar();
                  }
                  else
                  {
                     _state = State.Done;
                     CurrentToken.TokenKind = TokenKind.NumberLiteral;
                  }
                  break;

               case State.Comment:
                  if (_currentCh == '/')
                  {
                     _state = State.Start;
                     _currentCh = NextChar();
                     while (_currentCh != ChCr && _currentCh != ChEof)
                        _currentCh = NextChar();

                     SkipWhiteSpace();
                  }
                  else
                  {
                     CurrentToken.TokenKind = TokenKind.ForwardSlash;
                     _state = State.Done;
                  }
                  break;
            }
         } while (_state != State.Done);

         SetStopPosition(CurrentToken);
      }

      private void SetStartPosition(IToken token)
      {
         token.Location.StartLine = Line;
         token.Location.StartColumn = Column;
         token.Location.StopLine = token.Location.StartLine;
         token.Location.StopColumn = token.Location.StartColumn;
      }

      private void SetStopPosition(IToken token)
      {
         token.Location.StopLine = Line;
         token.Location.StopColumn = Column;
      }

      private void SkipWhiteSpace()
      {
         while (_currentCh == ChSpace || _currentCh == ChTab || _currentCh == ChCr ||
                _currentCh == ChLf || _currentCh == ChZero)
            _currentCh = NextChar();
      }

      private char NextChar()
      {
         if (_input.EndOfStream)
            return ChEof;

         if (_input.Peek() == ChLf)
         {
            _input.Read();
            //_input.Read();

            Line++;
            Column = 0;

            return ChLf;
         }

         Column++;
         return (char)_input.Read();
      }
   }
}
