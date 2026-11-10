using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Json;
using ApexCompiler.Core;

namespace ApexCompiler
{
    internal static class Program
    {
        private static int Main(string[] args)
        {
            bool json = args.Contains("--json");
            var result = new CompilationResult();
            try
            {
                if (args.Length == 1 && args[0] == "--version")
                {
                    Console.WriteLine(result.Version);
                    return 0;
                }
                if (args.Length == 1 && args[0] == "--help") { Usage(); return 0; }
                var paths = new List<string>();
                var symbols = new List<string>();
                bool check = false, positionalOnly = false;
                for (int i = 0; i < args.Length; i++)
                {
                    string arg = args[i];
                    if (positionalOnly) paths.Add(arg);
                    else if (arg == "--") positionalOnly = true;
                    else if (arg == "--json") { }
                    else if (arg == "--check") check = true;
                    else if (arg == "--define" && i + 1 < args.Length) symbols.Add(args[++i]);
                    else if (arg.StartsWith("--", StringComparison.Ordinal)) throw new ArgumentException("Unknown or incomplete option: " + arg);
                    else paths.Add(arg);
                }
                if (paths.Count != (check ? 1 : 2)) throw new ArgumentException("Expected a source path" + (check ? "." : " and an output path."));
                string output = check ? null : Path.GetFullPath(paths[1]);
                if (output != null && !output.EndsWith(".gscc", StringComparison.OrdinalIgnoreCase)) throw new ArgumentException("Output must have a .gscc extension.");
                result = CompilationService.CompilePath(paths[0], symbols);
                result.Output = output;
                if (result.Success && !check)
                {
                    try { AtomicOutput.WriteAllBytes(output, result.CompiledScript); }
                    catch (Exception ex)
                    {
                        result.Success = false;
                        result.Diagnostics.Add(new Diagnostic { Code = "APEX003", File = output, Message = ex.Message });
                    }
                }
                Print(result, json, check);
                return result.Success ? 0 : 1;
            }
            catch (ArgumentException ex)
            {
                result.Diagnostics.Add(new Diagnostic { Code = "APEX000", Message = ex.Message });
                Print(result, json, false);
                if (!json) Usage();
                return 2;
            }
            catch (Exception ex)
            {
                result.Diagnostics.Add(new Diagnostic { Message = ex.Message });
                Print(result, json, false);
                return 1;
            }
        }

        private static void Print(CompilationResult result, bool json, bool check)
        {
            if (json)
            {
                using (var stream = new MemoryStream())
                {
                    new DataContractJsonSerializer(typeof(CompilationResult)).WriteObject(stream, result);
                    Console.WriteLine(System.Text.Encoding.UTF8.GetString(stream.ToArray()));
                }
                return;
            }
            foreach (var source in result.Sources) Console.WriteLine("Source: " + source);
            foreach (var diagnostic in result.Diagnostics) Console.Error.WriteLine(diagnostic);
            if (result.Success) Console.WriteLine(check ? "Compilation and structural validation passed." : "Wrote " + result.Output + " (" + result.Bytes + " bytes)");
        }

        private static void Usage()
        {
            Console.WriteLine("APEX compiler | PS4 BO3 1.33 GSC");
            Console.WriteLine("Usage: APEX.Compiler.exe [--json] [--define TOKEN] <source-directory-or-file> <output.gscc>");
            Console.WriteLine("       APEX.Compiler.exe --check [--json] [--define TOKEN] <source-directory-or-file>");
            Console.WriteLine("       APEX.Compiler.exe --version | --help");
        }
    }
}
