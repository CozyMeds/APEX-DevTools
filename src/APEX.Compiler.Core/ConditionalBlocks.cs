using System;
using System.Collections.Generic;

internal sealed class CBSyntaxException : Exception
{
    public readonly int ErrorPosition;
    public CBSyntaxException(string message, int position) : base(message) { ErrorPosition = position; }
}

// Replace disabled text with spaces so parser locations refer to the original source.
internal sealed class ConditionalBlocks
{
    private readonly HashSet<string> tokens = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
    private sealed class Frame
    {
        internal bool ParentActive;
        internal bool Condition;
        internal bool SeenElse;
        internal int Position;
    }

    public void LoadConditionalTokens(List<string> values)
    {
        tokens.Clear();
        foreach (var value in values) tokens.Add(value.Trim());
    }

    public string ParseSource(string input)
    {
        var result = input.ToCharArray();
        var stack = new Stack<Frame>();
        bool active = true, escaped = false;
        int state = 0; // Code, string, line comment, block comment, developer comment.
        for (int i = 0; i < input.Length; i++)
        {
            char c = input[i];
            if (!active && c != '\r' && c != '\n') result[i] = ' ';
            if (state == 1)
            {
                if (escaped) escaped = false;
                else if (c == '\\') escaped = true;
                else if (c == '"') state = 0;
                continue;
            }
            if (state == 2) { if (c == '\r' || c == '\n') state = 0; continue; }
            if (state == 3 || state == 4)
            {
                if (i + 1 < input.Length && c == (state == 3 ? '*' : '#') && input[i + 1] == '/')
                {
                    if (!active) result[i + 1] = ' ';
                    i++;
                    state = 0;
                }
                continue;
            }
            if (c == '"') { state = 1; continue; }
            if (c == '/' && i + 1 < input.Length)
            {
                int nextState = input[i + 1] == '/' ? 2 : input[i + 1] == '*' ? 3 : input[i + 1] == '#' ? 4 : 0;
                if (nextState != 0)
                {
                    state = nextState;
                    if (!active) result[i + 1] = ' ';
                    i++;
                    continue;
                }
            }
            if (c != '#') continue;
            int end = i + 1;
            while (end < input.Length && (char.IsLetterOrDigit(input[end]) || input[end] == '_')) end++;
            string directive = input.Substring(i + 1, end - i - 1);
            if (directive != "ifdef" && directive != "ifndef" && directive != "else" && directive != "endif") continue;
            if (directive == "ifdef" || directive == "ifndef")
            {
                while (end < input.Length && (input[end] == ' ' || input[end] == '\t')) end++;
                int start = end;
                if (end >= input.Length || (!char.IsLetter(input[end]) && input[end] != '_'))
                    throw new CBSyntaxException("Expected a conditional identifier.", i);
                while (end < input.Length && (char.IsLetterOrDigit(input[end]) || input[end] == '_')) end++;
                bool condition = tokens.Contains(input.Substring(start, end - start));
                if (directive == "ifndef") condition = !condition;
                stack.Push(new Frame { ParentActive = active, Condition = condition, Position = i });
                active = active && condition;
            }
            else
            {
                if (stack.Count == 0) throw new CBSyntaxException("Unexpected #" + directive + ".", i);
                var frame = stack.Peek();
                if (directive == "else")
                {
                    if (frame.SeenElse) throw new CBSyntaxException("Duplicate #else.", i);
                    frame.SeenElse = true;
                    active = frame.ParentActive && !frame.Condition;
                }
                else
                {
                    stack.Pop();
                    active = frame.ParentActive;
                }
            }
            for (int j = i; j < end; j++) if (result[j] != '\r' && result[j] != '\n') result[j] = ' ';
            i = end - 1;
        }
        if (stack.Count != 0) throw new CBSyntaxException("Expected #endif.", stack.Peek().Position);
        return new string(result);
    }
}
