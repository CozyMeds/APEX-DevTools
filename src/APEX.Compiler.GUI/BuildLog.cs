using System;
using System.Collections.Generic;
using System.Linq;

namespace APEX.Compiler.GUI
{
    internal enum LogSeverity { Info, Success, Warning, Error }
    internal enum LogFilter { All, Warnings, Errors }

    internal sealed class BuildLogEntry
    {
        internal DateTime Timestamp { get; }
        internal LogSeverity Severity { get; }
        internal string Message { get; }
        internal BuildLogEntry(string message, LogSeverity severity)
        {
            Timestamp = DateTime.Now;
            Severity = severity;
            Message = message.Length > 4096 ? message.Substring(0, 4096) + "…" : message;
        }
    }

    internal sealed class BuildLog
    {
        private readonly Queue<BuildLogEntry> entries = new Queue<BuildLogEntry>();
        private int characters;
        internal int WarningCount { get; private set; }
        internal int ErrorCount { get; private set; }
        internal bool Add(BuildLogEntry entry)
        {
            if (entry.Severity == LogSeverity.Warning) WarningCount++;
            if (entry.Severity == LogSeverity.Error) ErrorCount++;
            entries.Enqueue(entry);
            characters += entry.Message.Length;
            bool trimmed = false;
            while (entries.Count > 1000 || characters > 160000)
            {
                characters -= entries.Dequeue().Message.Length;
                trimmed = true;
            }
            return trimmed;
        }
        internal BuildLogEntry[] Visible(LogFilter filter) => entries.Where(e => Matches(e, filter)).ToArray();
        internal static bool Matches(BuildLogEntry entry, LogFilter filter) => filter == LogFilter.All ||
            (filter == LogFilter.Warnings && entry.Severity == LogSeverity.Warning) ||
            (filter == LogFilter.Errors && entry.Severity == LogSeverity.Error);
        internal void Clear() { entries.Clear(); characters = 0; WarningCount = 0; ErrorCount = 0; }
        internal static LogSeverity Classify(string message, bool standardError)
        {
            if (message.StartsWith("Warning:", StringComparison.OrdinalIgnoreCase) || message.IndexOf(": warning ", StringComparison.OrdinalIgnoreCase) >= 0)
                return LogSeverity.Warning;
            return standardError ? LogSeverity.Error : LogSeverity.Info;
        }
    }
}
