using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using ApexCompiler.Core;
using Xunit;

namespace ApexCompiler.Tests
{
    public class CliIntegrationTests
    {
        private static (int Exit, string Output, string Error) Run(params string[] args)
        {
            var directory = new DirectoryInfo(AppDomain.CurrentDomain.BaseDirectory);
            while (directory != null && !File.Exists(Path.Combine(directory.FullName, "APEX.DevTools.sln"))) directory = directory.Parent;
            Assert.NotNull(directory);
            string executable = Path.Combine(directory.FullName, "src", "APEX.Compiler", "bin", "Release", "net48", "APEX.Compiler.exe");
            Assert.True(File.Exists(executable), "Build the solution in Release before running CLI integration tests.");
            string arguments = string.Join(" ", args.Select(WindowsArguments.Quote));
            string fileName = executable;
            if (Path.DirectorySeparatorChar != '\\')
            {
                fileName = Environment.GetEnvironmentVariable("DOTNET_HOST_PATH") ?? "dotnet";
                string runtime = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "APEX.Compiler.Tests.runtimeconfig.json");
                arguments = "exec --runtimeconfig " + WindowsArguments.Quote(runtime) + " " + WindowsArguments.Quote(executable) + " " + arguments;
            }
            using (var process = Process.Start(new ProcessStartInfo(fileName, arguments)
            {
                UseShellExecute = false, CreateNoWindow = true, RedirectStandardOutput = true, RedirectStandardError = true
            }))
            {
                var output = process.StandardOutput.ReadToEndAsync();
                var error = process.StandardError.ReadToEndAsync();
                if (!process.WaitForExit(15000))
                {
                    process.Kill();
                    throw new TimeoutException("CLI did not finish within 15 seconds.");
                }
                return (process.ExitCode, output.GetAwaiter().GetResult(), error.GetAwaiter().GetResult());
            }
        }

        [Fact]
        public void VersionAndUsageHaveStableExitCodes()
        {
            Assert.Equal("1.2.0", Run("--version").Output.Trim());
            Assert.Equal(0, Run("--help").Exit);
            Assert.Equal(2, Run().Exit);
        }

        [Fact]
        public void JsonCheckEmitsOneDocumentAndWritesNoFiles()
        {
            using (var directory = new TemporaryDirectory())
            {
                string path = Path.Combine(directory.Path, "source with spaces.gsc");
                File.WriteAllText(path, "#ifdef MY_TOKEN\nmain(){return;}\n#else\ninvalid\n#endif");
                var result = Run("--json", "--check", "--define", "MY_TOKEN", path);
                Assert.Equal(0, result.Exit);
                Assert.Equal("", result.Error);
                Assert.StartsWith("{", result.Output.Trim());
                Assert.Contains("\"success\":true", result.Output);
                Assert.DoesNotContain("Source:", result.Output);
                Assert.Single(Directory.GetFiles(directory.Path));
            }
        }

        [Fact]
        public void FailingCompilationPreservesExistingDestination()
        {
            using (var directory = new TemporaryDirectory())
            {
                string source = Path.Combine(directory.Path, "main.gsc");
                string output = Path.Combine(directory.Path, "output.gscc");
                File.WriteAllText(source, "main(){ x = ; }");
                File.WriteAllText(output, "previous build");
                var result = Run("--json", source, output);
                Assert.Equal(1, result.Exit);
                Assert.Contains("\"success\":false", result.Output);
                Assert.Equal("previous build", File.ReadAllText(output));
            }
        }

        [Fact]
        public void CliCreatesValidatedOutputInANewDirectory()
        {
            using (var directory = new TemporaryDirectory())
            {
                string source = Path.Combine(directory.Path, "main.gsc");
                string output = Path.Combine(directory.Path, "new folder", "output.gscc");
                File.WriteAllText(source, "main(){return;}");
                var result = Run(source, output);
                Assert.Equal(0, result.Exit);
                GsccValidator.Validate(File.ReadAllBytes(output));
            }
        }
    }
}
