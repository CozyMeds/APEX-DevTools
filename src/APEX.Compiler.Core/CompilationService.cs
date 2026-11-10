using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using TreyarchCompiler;

namespace ApexCompiler.Core
{
    [DataContract]
    public sealed class Diagnostic
    {
        [DataMember(Name = "file")] public string File { get; set; }
        [DataMember(Name = "line")] public int? Line { get; set; }
        [DataMember(Name = "column")] public int? Column { get; set; }
        [DataMember(Name = "code")] public string Code { get; set; } = "APEX001";
        [DataMember(Name = "severity")] public string Severity { get; set; } = "error";
        [DataMember(Name = "message")] public string Message { get; set; }
        public override string ToString() => (File == null ? "" : File + (Line.HasValue ? "(" + Line + "," + Column + ")" : "") + ": ") + Severity + " " + Code + ": " + Message;
    }

    [DataContract]
    public sealed class CompilationResult
    {
        [DataMember(Name = "success")] public bool Success { get; set; }
        [DataMember(Name = "version")] public string Version { get; set; } = typeof(CompilationService).Assembly.GetName().Version.ToString(3);
        [DataMember(Name = "output")] public string Output { get; set; }
        [DataMember(Name = "bytes")] public int Bytes { get; set; }
        [DataMember(Name = "sources")] public string[] Sources { get; set; } = new string[0];
        [DataMember(Name = "diagnostics")] public List<Diagnostic> Diagnostics { get; set; } = new List<Diagnostic>();
        public byte[] CompiledScript { get; set; }
    }

    public static class CompilationService
    {
        private sealed class Source
        {
            internal string Path;
            internal int StartLine;
            internal int Lines;
        }

        public static CompilationResult CompilePath(string input, IEnumerable<string> symbols = null)
        {
            var result = new CompilationResult();
            var sources = new List<Source>();
            try
            {
                input = Path.GetFullPath(input);
                var paths = File.Exists(input) ? new[] { input } : Directory.Exists(input)
                    ? Directory.GetFiles(input, "*", SearchOption.AllDirectories).Where(p => p.EndsWith(".gsc", StringComparison.OrdinalIgnoreCase)).OrderBy(p => p, StringComparer.OrdinalIgnoreCase).ThenBy(p => p, StringComparer.Ordinal).ToArray()
                    : throw new FileNotFoundException("Source directory or file not found: " + input);
                paths = paths.Where(p => p.EndsWith(".gsc", StringComparison.OrdinalIgnoreCase)).ToArray();
                if (paths.Length == 0) throw new InvalidOperationException("No .gsc source files found.");
                result.Sources = paths;
                var text = new StringBuilder();
                int line = 0;
                foreach (var path in paths)
                {
                    string original = File.ReadAllText(path).Replace("\r\n", "\n").Replace('\r', '\n');
                    var preprocessor = new ConditionalBlocks();
                    preprocessor.LoadConditionalTokens(new List<string>(new[] { "BO3" }.Concat(symbols ?? Enumerable.Empty<string>())));
                    string processed;
                    try { processed = preprocessor.ParseSource(original); }
                    catch (CBSyntaxException ex)
                    {
                        string prefix = original.Substring(0, Math.Min(ex.ErrorPosition, original.Length));
                        int localLine = prefix.Count(c => c == '\n');
                        int column = prefix.Length - prefix.LastIndexOf('\n');
                        result.Diagnostics.Add(new Diagnostic { File = path, Line = localLine + 1, Column = column, Code = "APEX002", Message = ex.Message });
                        return result;
                    }
                    int lines = original.Count(c => c == '\n') + 1;
                    sources.Add(new Source { Path = path, StartLine = line, Lines = lines });
                    text.Append(processed).Append('\n');
                    line += lines;
                }
                var compiled = Compiler.Compile(false, text.ToString());
                if (!string.IsNullOrEmpty(compiled.Error))
                {
                    var source = sources.LastOrDefault(s => s.StartLine <= compiled.ErrorLine);
                    result.Diagnostics.Add(new Diagnostic
                    {
                        File = source?.Path,
                        Line = source == null ? (int?)null : Math.Min(compiled.ErrorLine - source.StartLine + 1, source.Lines),
                        Column = compiled.ErrorColumn < 0 ? (int?)null : compiled.ErrorColumn + 1,
                        Message = compiled.Error
                    });
                    return result;
                }
                GsccValidator.Validate(compiled.CompiledScript);
                result.CompiledScript = compiled.CompiledScript;
                result.Bytes = compiled.CompiledScript.Length;
                result.Success = true;
            }
            catch (Exception ex) { result.Diagnostics.Add(new Diagnostic { Message = ex.Message }); }
            return result;
        }
    }
}
