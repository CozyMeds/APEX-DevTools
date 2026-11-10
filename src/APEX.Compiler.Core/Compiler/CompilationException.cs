using System;

namespace TreyarchCompiler
{
    internal sealed class CompilationException : Exception
    {
        internal int Line { get; }
        internal int Column { get; }
        internal CompilationException(string message, int line, int column) : base(message)
        {
            Line = line;
            Column = column;
        }
    }
}
