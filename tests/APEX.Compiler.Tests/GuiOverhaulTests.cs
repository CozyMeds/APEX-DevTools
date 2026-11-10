#if NETFRAMEWORK
using System;
using System.CodeDom.Compiler;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using APEX.Compiler.GUI;
using ApexCompiler.Core;
using Microsoft.CSharp;
using Xunit;
using Settings = APEX.Compiler.GUI.Properties.Settings;

namespace ApexCompiler.Tests
{
    public class GuiOverhaulTests
    {
        private static string Repository()
        {
            var directory = new DirectoryInfo(AppDomain.CurrentDomain.BaseDirectory);
            while (directory != null && !File.Exists(Path.Combine(directory.FullName, "APEX.DevTools.sln"))) directory = directory.Parent;
            Assert.NotNull(directory);
            return directory.FullName;
        }

        private static T Field<T>(MainForm form, string name) => (T)typeof(MainForm).GetField(name, BindingFlags.Instance | BindingFlags.NonPublic).GetValue(form);

        private static async Task Finished(MainForm form)
        {
            var timer = Stopwatch.StartNew();
            while (Field<CancellationTokenSource>(form, "buildCancellation") != null)
            {
                if (timer.Elapsed > TimeSpan.FromSeconds(20)) throw new TimeoutException("GUI compilation did not finish.");
                await Task.Delay(40);
            }
        }

        private static void WithForm(string compiler, string source, string output, Func<MainForm, Task> scenario)
        {
            Exception failure = null;
            MainForm running = null;
            var thread = new Thread(() =>
            {
                string oldCompiler = Settings.Default.CompilerPath, oldSource = Settings.Default.SourcePath, oldOutput = Settings.Default.OutputPath;
                try
                {
                    Settings.Default.CompilerPath = compiler; Settings.Default.SourcePath = source; Settings.Default.OutputPath = output;
                    using (var form = new MainForm())
                    {
                        running = form;
                        form.Shown += async (sender, args) =>
                        {
                            try { await scenario(form); }
                            catch (Exception ex) { failure = ex; }
                            finally { form.Close(); }
                        };
                        Application.Run(form);
                    }
                }
                catch (Exception ex) { failure = ex; }
                finally
                {
                    Settings.Default.CompilerPath = oldCompiler; Settings.Default.SourcePath = oldSource; Settings.Default.OutputPath = oldOutput;
                    Settings.Default.Save();
                }
            }) { IsBackground = true };
            thread.SetApartmentState(ApartmentState.STA);
            thread.Start();
            if (!thread.Join(30000))
            {
                if (running != null && !running.IsDisposed) running.BeginInvoke(new Action(() => running.Close()));
                Assert.True(thread.Join(5000), "GUI message loop did not close after its deadline.");
                throw new TimeoutException("GUI test exceeded its deadline.");
            }
            Assert.Null(failure);
        }

        private static void Sources(string directory)
        {
            Directory.CreateDirectory(directory);
            File.WriteAllText(Path.Combine(directory, "main.gsc"), "main(){helper();} helper(){return;}");
        }

        [Fact]
        public void RealCompileUpdatesLiveSummaryAndArtifactAndCapturesPreview()
        {
            using (var directory = new TemporaryDirectory())
            {
                string source = Path.Combine(directory.Path, "Source scripts"); Sources(source);
                string output = Path.Combine(directory.Path, "Build output", "APEX.gscc");
                string compiler = Path.Combine(Repository(), "src", "APEX.Compiler", "bin", "Release", "net48", "APEX.Compiler.exe");
                WithForm(compiler, source, output, async form =>
                {
                    // Hosted runners may initially constrain the window to a small desktop.
                    form.ClientSize = new Size(1280, 860);
                    form.PerformLayout();
                    AssertResultTextVisible(form);
                    Capture(form, "gui-ready.png");
                    Assert.Equal("Ready to compile", Field<Label>(form, "lblSummaryTitle").Text);
                    Assert.False(Field<Button>(form, "btnShowArtifact").Enabled);
                    Field<Button>(form, "btnCompile").PerformClick();
                    Assert.False(Field<Button>(form, "btnCheck").Enabled);
                    Assert.True(Field<Button>(form, "btnCancel").Enabled);
                    await Finished(form);
                    GsccValidator.Validate(File.ReadAllBytes(output));
                    Assert.Equal("Build successful", Field<Label>(form, "lblSummaryTitle").Text);
                    Assert.Contains("0 warnings", Field<Label>(form, "lblSummaryDetails").Text);
                    Assert.Equal("APEX.gscc", Field<Label>(form, "lblArtifactName").Text);
                    Assert.Equal(new FileInfo(output).Length.ToString("N0") + " bytes", Field<Label>(form, "lblArtifactSize").Text);
                    Assert.True(Field<Button>(form, "btnShowArtifact").Enabled);
                    Assert.False(Field<Button>(form, "btnCancel").Enabled);
                    Assert.DoesNotContain("APEX_", Field<RichTextBox>(form, "rtbLog").Text);
                    AssertResultTextVisible(form);
                    Capture(form, "gui-success.png");
                    form.Size = form.MinimumSize;
                    form.PerformLayout();
                    Assert.True(Field<Control>(form, "configurationCard").Width > 280);
                    Assert.True(Field<RichTextBox>(form, "rtbLog").Height > 120);
                    var configuration = Field<CardPanel>(form, "configurationCard");
                    var compile = Field<Button>(form, "btnCompile");
                    configuration.ScrollControlIntoView(compile);
                    Rectangle buttonBounds = configuration.RectangleToClient(compile.RectangleToScreen(compile.ClientRectangle));
                    Assert.True(configuration.ClientRectangle.Contains(buttonBounds), "Compile action must remain reachable at minimum window size.");
                    AssertResultTextVisible(form);
                    AssertActionVisible(form, "btnCancel");
                    Capture(form, "gui-minimum.png");
                });
            }
        }

        [Fact]
        public void CheckOnlyNeedsNoOutputPathAndWritesNothing()
        {
            using (var directory = new TemporaryDirectory())
            {
                string source = Path.Combine(directory.Path, "source"); Sources(source);
                string compiler = Path.Combine(Repository(), "src", "APEX.Compiler", "bin", "Release", "net48", "APEX.Compiler.exe");
                var files = Directory.GetFiles(directory.Path, "*", SearchOption.AllDirectories);
                WithForm(compiler, source, "", async form =>
                {
                    Field<Button>(form, "btnCheck").PerformClick();
                    await Finished(form);
                    Assert.Equal("Check passed", Field<Label>(form, "lblSummaryTitle").Text);
                    Assert.Contains("No output written", Field<Label>(form, "lblSummaryDetails").Text);
                    Assert.False(Field<Button>(form, "btnShowArtifact").Enabled);
                    Assert.Equal(files, Directory.GetFiles(directory.Path, "*", SearchOption.AllDirectories));
                    Capture(form, "gui-check.png");
                });
            }
        }

        [Fact]
        public void LogFiltersPreserveMessagesAndWarningSummary()
        {
            using (var directory = new TemporaryDirectory())
            {
                string source = Path.Combine(directory.Path, "source"); Sources(source);
                string compiler = Path.Combine(directory.Path, "WarningCompiler.exe");
                string output = Path.Combine(directory.Path, "output.gscc");
                byte[] valid = TreyarchCompiler.Compiler.Compile(false, "main(){return;}").CompiledScript;
                using (var provider = new CSharpCodeProvider())
                {
                    string code = "using System; using System.IO; class Fixture { static void Main(string[] args) { " +
                        "Console.WriteLine(\"Source selected\"); Console.WriteLine(\"Warning: an example warning\");" +
                        "Console.Error.WriteLine(\"example.gsc(1,1): warning APEX100: another warning\");" +
                        "File.WriteAllBytes(args[1], Convert.FromBase64String(\"" + Convert.ToBase64String(valid) + "\")); }}";
                    var compiled = provider.CompileAssemblyFromSource(new CompilerParameters(new[] { "System.dll" }, compiler) { GenerateExecutable = true }, code);
                    Assert.False(compiled.Errors.HasErrors);
                }
                WithForm(compiler, source, output, async form =>
                {
                    Field<Button>(form, "btnCompile").PerformClick();
                    await Finished(form);
                    Assert.Equal("Build successful", Field<Label>(form, "lblSummaryTitle").Text);
                    Assert.Contains("2 warnings", Field<Label>(form, "lblSummaryDetails").Text);
                    var log = Field<RichTextBox>(form, "rtbLog");
                    Field<Button>(form, "btnFilterWarnings").PerformClick();
                    Assert.Contains("an example warning", log.Text);
                    Assert.Contains("another warning", log.Text);
                    Assert.DoesNotContain("[INFO]", log.Text);
                    Field<Button>(form, "btnFilterErrors").PerformClick();
                    Assert.Equal("", log.Text);
                    Field<Button>(form, "btnFilterAll").PerformClick();
                    Assert.Contains("Source selected", log.Text);
                    Assert.Contains("an example warning", log.Text);
                    Field<Button>(form, "btnClearLog").PerformClick();
                    Assert.Equal("", log.Text);
                });
            }
        }

        [Fact]
        public void FailedChecksHaveAnErrorFilterAndPreserveExistingOutput()
        {
            using (var directory = new TemporaryDirectory())
            {
                string source = Path.Combine(directory.Path, "source"); Sources(source);
                File.WriteAllText(Path.Combine(source, "main.gsc"), "main(){x=;}");
                string output = Path.Combine(directory.Path, "previous.gscc"); File.WriteAllText(output, "previous build");
                string compiler = Path.Combine(Repository(), "src", "APEX.Compiler", "bin", "Release", "net48", "APEX.Compiler.exe");
                WithForm(compiler, source, output, async form =>
                {
                    Field<Button>(form, "btnCheck").PerformClick();
                    await Finished(form);
                    Assert.Equal("Check failed", Field<Label>(form, "lblSummaryTitle").Text);
                    Assert.Equal("previous build", File.ReadAllText(output));
                    Field<Button>(form, "btnFilterErrors").PerformClick();
                    var text = Field<RichTextBox>(form, "rtbLog").Text;
                    Assert.Contains("[ERROR]", text);
                    Assert.DoesNotContain("[INFO]", text);
                    Capture(form, "gui-error.png");
                });
            }
        }

        private static void AssertResultTextVisible(MainForm form)
        {
            foreach (string name in new[] { "lblVersion", "lblSummaryTitle", "lblSummaryDetails", "lblArtifactName", "lblArtifactSize" })
            {
                var label = Field<Label>(form, name);
                Assert.True(label.Height >= TextRenderer.MeasureText(label.Text, label.Font).Height, name + " must have room for its text.");
                var screen = label.RectangleToScreen(label.ClientRectangle);
                for (Control parent = label.Parent; parent != null; parent = parent.Parent)
                    Assert.True(parent.RectangleToScreen(parent.ClientRectangle).Contains(screen), name + " must not be clipped by " + parent.GetType().Name);
            }
            foreach (var label in Descendants(form).OfType<Label>().Where(l => l.Text == "Build output" || l.Text == "Build configuration"))
                Assert.True(label.Height >= TextRenderer.MeasureText(label.Text, label.Font).Height, label.Text + " heading must remain visible.");
        }
        private static System.Collections.Generic.IEnumerable<Control> Descendants(Control parent)
        {
            foreach (Control child in parent.Controls)
            {
                yield return child;
                foreach (var next in Descendants(child)) yield return next;
            }
        }
        private static void AssertActionVisible(MainForm form, string name)
        {
            var button = Field<Button>(form, name);
            var screen = button.RectangleToScreen(button.ClientRectangle);
            for (Control parent = button.Parent; parent != null; parent = parent.Parent)
                Assert.True(parent.RectangleToScreen(parent.ClientRectangle).Contains(screen), name + " must remain visible without scrolling.");
        }

        private static void Capture(MainForm form, string name)
        {
            string directory = Path.Combine(Repository(), "artifacts", "gui-preview");
            Directory.CreateDirectory(directory);
            form.PerformLayout();
            form.Refresh();
            using (var bitmap = new Bitmap(form.ClientSize.Width, form.ClientSize.Height))
            {
                form.Controls[0].DrawToBitmap(bitmap, new Rectangle(Point.Empty, bitmap.Size));
                bitmap.Save(Path.Combine(directory, name), ImageFormat.Png);
            }
        }
    }

    public class BuildLogTests
    {
        [Fact]
        public void FilteringKeepsSeverityCountsAndDoesNotDiscardHistory()
        {
            var log = new BuildLog();
            log.Add(new BuildLogEntry("info", LogSeverity.Info));
            log.Add(new BuildLogEntry("warning", LogSeverity.Warning));
            log.Add(new BuildLogEntry("error", LogSeverity.Error));
            Assert.Single(log.Visible(LogFilter.Warnings));
            Assert.Single(log.Visible(LogFilter.Errors));
            Assert.Equal(3, log.Visible(LogFilter.All).Length);
            Assert.Equal(1, log.WarningCount); Assert.Equal(1, log.ErrorCount);
        }
        [Fact]
        public void HighVolumeLogsStayBoundedButKeepTotals()
        {
            var log = new BuildLog();
            for (int i = 0; i < 5000; i++) log.Add(new BuildLogEntry(new string('x', 5000), LogSeverity.Warning));
            Assert.True(log.Visible(LogFilter.All).Length <= 1000);
            Assert.True(log.Visible(LogFilter.All).Sum(e => e.Message.Length) <= 160000);
            Assert.Equal(5000, log.WarningCount);
            log.Clear();
            Assert.Empty(log.Visible(LogFilter.All)); Assert.Equal(0, log.WarningCount);
        }
        [Theory]
        [InlineData("Warning: warning", false, 2)]
        [InlineData("file.gsc(1,2): warning APEX100: warning", true, 2)]
        [InlineData("compiler failure", true, 3)]
        [InlineData("Compiled source", false, 0)]
        public void StandardErrorWarningsAreNotMisclassified(string text, bool standardError, int expected) => Assert.Equal((LogSeverity)expected, BuildLog.Classify(text, standardError));
    }
}
#endif
