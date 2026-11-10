#if NETFRAMEWORK
using System;
using System.CodeDom.Compiler;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Windows.Forms;
using APEX.Compiler.GUI;
using Microsoft.CSharp;
using Xunit;
using Settings = APEX.Compiler.GUI.Properties.Settings;

namespace ApexCompiler.Tests
{
    public class GuiLifecycleTests
    {
        [Theory]
        [InlineData("invalid")]
        [InlineData("cancel")]
        [InlineData("close")]
        public void InvalidOutputAndInterruptedBuildsPreserveThePreviousBuild(string mode)
        {
            using (var directory = new TemporaryDirectory())
            {
                string source = Path.Combine(directory.Path, "source");
                Directory.CreateDirectory(source);
                File.WriteAllText(Path.Combine(source, "main.gsc"), "main(){return;}");
                string compiler = Path.Combine(directory.Path, "FakeCompiler.exe");
                string output = Path.Combine(directory.Path, "previous.gscc");
                File.WriteAllText(output, "previous build");
                using (var provider = new CSharpCodeProvider())
                {
                    var parameters = new CompilerParameters(new[] { "System.dll" }, compiler) { GenerateExecutable = true };
                    string fakeSource = @"using System; using System.IO; using System.Threading; using System.Diagnostics;
class FakeCompiler { static void Main(string[] args) {
File.WriteAllText(Path.Combine(args[0], ""compiler.pid""), Process.GetCurrentProcess().Id.ToString());
" + (mode == "invalid" ? "File.WriteAllBytes(args[1], new byte[] { 1,2,3,4,5,6,7,8 });" : "Thread.Sleep(60000);") + "}}";
                    var compiled = provider.CompileAssemblyFromSource(parameters, fakeSource);
                    Assert.False(compiled.Errors.HasErrors, "Could not compile the GUI process fixture.");
                }
                Exception failure = null;
                MainForm runningForm = null;
                var thread = new Thread(() =>
                {
                    string oldCompiler = Settings.Default.CompilerPath, oldSource = Settings.Default.SourcePath, oldOutput = Settings.Default.OutputPath;
                    try
                    {
                        Settings.Default.CompilerPath = compiler;
                        Settings.Default.SourcePath = source;
                        Settings.Default.OutputPath = output;
                        using (var form = new MainForm())
                        using (var timer = new System.Windows.Forms.Timer { Interval = 50 })
                        {
                            runningForm = form;
                            var watch = Stopwatch.StartNew();
                            bool interrupted = false;
                            timer.Tick += (sender, args) =>
                            {
                                try
                                {
                                    if (watch.Elapsed > TimeSpan.FromSeconds(15)) throw new TimeoutException("GUI build did not finish.");
                                    string pidPath = Path.Combine(source, "compiler.pid");
                                    if (mode != "invalid" && !interrupted && File.Exists(pidPath))
                                    {
                                        interrupted = true;
                                        if (mode == "close") form.Close();
                                        else Control<Button>(form, "btnCancel").PerformClick();
                                    }
                                    if (BuildFinished(form) && mode != "close")
                                    {
                                        Assert.Equal("previous build", File.ReadAllText(output));
                                        Assert.True(Control<Button>(form, "btnCompile").Enabled);
                                        Assert.False(Control<Button>(form, "btnCancel").Enabled);
                                        form.Close();
                                    }
                                }
                                catch (Exception ex)
                                {
                                    failure = ex;
                                    KillFixture(source);
                                    form.Close();
                                }
                            };
                            form.Shown += (sender, args) =>
                            {
                                Control<Button>(form, "btnCompile").PerformClick();
                                timer.Start();
                            };
                            Application.Run(form);
                        }
                    }
                    catch (Exception ex) { failure = ex; }
                    finally
                    {
                        Settings.Default.CompilerPath = oldCompiler;
                        Settings.Default.SourcePath = oldSource;
                        Settings.Default.OutputPath = oldOutput;
                        Settings.Default.Save();
                    }
                }) { IsBackground = true };
                thread.SetApartmentState(ApartmentState.STA);
                thread.Start();
                if (!thread.Join(25000))
                {
                    KillFixture(source);
                    if (runningForm != null && !runningForm.IsDisposed) runningForm.BeginInvoke(new Action(() => runningForm.Close()));
                    Assert.True(thread.Join(5000), "GUI message loop did not exit.");
                    throw new TimeoutException("GUI lifecycle test exceeded its deadline.");
                }
                Assert.Null(failure);
                Assert.Equal("previous build", File.ReadAllText(output));
                Assert.Empty(Directory.GetFiles(directory.Path, "APEX_*.gscc"));
                string pidFile = Path.Combine(source, "compiler.pid");
                Assert.True(File.Exists(pidFile), "The GUI never started the compiler fixture.");
                try
                {
                    using (var process = Process.GetProcessById(int.Parse(File.ReadAllText(pidFile))))
                        Assert.True(process.HasExited, "The compiler process survived cancellation or closing.");
                }
                catch (ArgumentException) { }
            }
        }

        private static bool BuildFinished(MainForm form) => typeof(MainForm).GetField("buildCancellation", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(form) == null;
        private static T Control<T>(MainForm form, string name) where T : Control => (T)typeof(MainForm).GetField(name, BindingFlags.Instance | BindingFlags.NonPublic).GetValue(form);
        private static void KillFixture(string source)
        {
            string path = Path.Combine(source, "compiler.pid");
            if (!File.Exists(path)) return;
            try { using (var process = Process.GetProcessById(int.Parse(File.ReadAllText(path)))) if (!process.HasExited) process.Kill(); }
            catch (ArgumentException) { }
            catch (InvalidOperationException) { }
        }
    }
}
#endif
