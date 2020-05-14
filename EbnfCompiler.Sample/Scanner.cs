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
         v,
         va,
         var,
         Ident,
         String,
         Assign,
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
                     case 'v':
                        _state = State.v;
                        CurrentToken.Image = "v";
                        break;
                     case '(':
                        CurrentToken.TokenKind = TokenKind.LeftParen;
                        CurrentToken.Image = "(";
                        _state = State.Done;
                        break;
                     case ')':
                        CurrentToken.TokenKind = TokenKind.RightParen;
                        CurrentToken.Image = ")";
                        _state = State.Done;
                        break;
                     case ':':
                        CurrentToken.TokenKind = TokenKind.Assign;
                        CurrentToken.Image = _currentCh.ToString();
                        _state = State.Assign;
                        break;
                     case '=':
                        CurrentToken.TokenKind = TokenKind.Assign;
                        CurrentToken.Image = _currentCh.ToString();
                        _state = State.Done;
                        break;
                     case '"':
                        CurrentToken.TokenKind = TokenKind.String;
                        CurrentToken.Image = string.Empty;
                        _state = State.String;
                        break;
                     case ChEof:
                        CurrentToken.TokenKind = TokenKind.Eof;
                        CurrentToken.Image = "<eof>";
                        _state = State.Done;
                        break;
                     case '/':
                        CurrentToken.Image = string.Empty;
                        _state = State.Comment;
                        break;

                     default:
                        CurrentToken.TokenKind = TokenKind.Error;
                        _state = State.Done;
                        break;
                  }
                  SetStartPosition(CurrentToken);
                  _currentCh = NextChar();
                  break;

               case State.v:
                  if (_currentCh == 'a')
                  {
                     CurrentToken.Image += _currentCh;
                     _currentCh = NextChar();
                     _state = State.va;
                  }
                  else if (Regex.IsMatch(_currentCh.ToString(), @"^[b-zA-Z0-9_]$"))
                  {
                     CurrentToken.Image += _currentCh;
                     _currentCh = NextChar();
                     _state = State.Ident;
                  }
                  else if (Regex.IsMatch(_currentCh.ToString(), @"^[\x10\x13\x1A\x20]$"))
                  {
                     CurrentToken.TokenKind = TokenKind.Designator;
                     _currentCh = NextChar();
                     _state = State.Done;
                  }
                  break;

               case State.va:
                  if (_currentCh == 'r')
                  {
                     CurrentToken.Image += _currentCh;
                     _currentCh = NextChar();
                     _state = State.var;
                  }
                  else if (Regex.IsMatch(_currentCh.ToString(), @"^[a-qs-zA-Z0-9_]$"))
                  {
                     CurrentToken.Image += _currentCh;
                     _currentCh = NextChar();
                     _state = State.Ident;
                  }
                  else if (Regex.IsMatch(_currentCh.ToString(), @"^[\x10\x13\x20]$"))
                  {
                     CurrentToken.TokenKind = TokenKind.Designator;
                     _currentCh = NextChar();
                     _state = State.Done;
                  }
                  break;

               case State.var:
                  if (Regex.IsMatch(_currentCh.ToString(), @"^[\x10\x13\x1A\x20]$"))
                  {
                     CurrentToken.TokenKind = TokenKind.Var;
                     _currentCh = NextChar();
                     _state = State.Done;
                  }
                  break;

               case State.Ident:
                  if (Regex.IsMatch(_currentCh.ToString(), @"^[a-zA-Z0-9_]$"))
                  {
                     CurrentToken.Image += _currentCh;
                     _currentCh = NextChar();
                  }
                  else if (_currentCh == '>')
                  {
                     CurrentToken.Image += _currentCh;
                     _currentCh = NextChar();
                     _state = State.Done;
                  }
                  else
                  {
                     CurrentToken.TokenKind = TokenKind.Error;
                     _state = State.Done;
                  }
                  break;

               case State.String:

                  if (Regex.IsMatch(_currentCh.ToString(), @"^[\x20-!#-~]$"))
                  {
                     CurrentToken.Image += _currentCh;
                     _currentCh = NextChar();
                  }
                  else if (_currentCh == '"')
                  {
                     _currentCh = NextChar();
                     _state = State.Done;
                  }
                  else
                  {
                     CurrentToken.TokenKind = TokenKind.Error;
                     _state = State.Done;
                  }
                  break;

               case State.Assign:
                  switch (_currentCh)
                  {
                     case '=':
                        CurrentToken.Image += _currentCh;
                        _currentCh = NextChar();
                        _state = State.Done;
                        break;
                     default:
                        CurrentToken.TokenKind = TokenKind.Error;
                        _state = State.Done;
                        break;
                  }
                  break;

               case State.Comment:
                  switch (_currentCh)
                  {
                     case '/':
                        _currentCh = NextChar();
                        while (_currentCh != ChCr && _currentCh != ChEof)
                           _currentCh = NextChar();

                        SkipWhiteSpace();
                        _state = State.Start;
                        break;
                     default:
                        CurrentToken.TokenKind = TokenKind.Error;
                        _state = State.Done;
                        break;
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
