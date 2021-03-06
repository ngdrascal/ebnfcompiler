﻿namespace EbnfCompiler.Sample.Impl
{
    public class Token : IToken
    {
        public ISourceLocation Location { get; }
        public string Image { get; set; }
        public TokenKind TokenKind { get; set; }

        public Token()
        {
            Location = new SourceLocation();
            Image = string.Empty;
            TokenKind = TokenKind.Error;
        }

        public Token(TokenKind tokenKind, string image) : this()
        {
            TokenKind = tokenKind;
            Image = image;
        }

        public override string ToString()
        {
            return $"Image: {Image}, TokenKind: {TokenKind}, Locations: {Location.StartLine}, {Location.StartColumn}";
        }
    }
}
