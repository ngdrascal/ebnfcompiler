﻿using System;

namespace EbnfCompiler.Sample.Impl
{
    public class CompilerException : Exception
    {
        protected CompilerException(string message, ISourceLocation location)
           : base($"({location.StartLine},{location.StartColumn}):{message}")
        {
            Location = location;
        }

        public ISourceLocation Location { get; }
    }
}
