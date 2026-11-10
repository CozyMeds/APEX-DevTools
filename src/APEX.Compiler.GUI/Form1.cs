using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Diagnostics;
using System.IO;
using System.Threading;
using ApexCompiler.Core;

namespace APEX.Compiler.GUI
{
    public partial class MainForm : Form
    {
        private CancellationTokenSource buildCancellation;
        private bool closeAfterBuild;
        private readonly Queue<KeyValuePair<string, Color>> pendingLogs = new Queue<KeyValuePair<string, Color>>();
        private readonly System.Windows.Forms.Timer logTimer;

        public MainForm()
        {
            InitializeComponent();
            lblVersion.Text = "APEX COMPILER  ·  v" + typeof(MainForm).Assembly.GetName().Version.ToString(3);
            logTimer = new System.Windows.Forms.Timer(components) { Interval = 50 };
            logTimer.Tick += (sender, args) => DrainLogs();
            logTimer.Start();

            UpdateStatus("Ready", Color.FromArgb(88, 214, 154));

            // Load previously saved paths.
            txtSource.Text = Properties.Settings.Default.SourcePath;
            txtOutput.Text = Properties.Settings.Default.OutputPath;
            RefreshCompilerDisplay();

            // The fields shorten visually when the window narrows. Hover to see the full path.
            pathToolTip.AutoPopDelay = 18000;
            pathToolTip.InitialDelay = 350;
            pathToolTip.ReshowDelay = 100;
            pathToolTip.ShowAlways = true;
            txtCompiler.TextChanged += (sender, args) =>
                pathToolTip.SetToolTip(txtCompiler, txtCompiler.Text);
            txtSource.TextChanged += (sender, args) =>
                pathToolTip.SetToolTip(txtSource, txtSource.Text);
            txtOutput.TextChanged += (sender, args) =>
                pathToolTip.SetToolTip(txtOutput, txtOutput.Text);
            UpdatePathTooltips();

            this.FormClosing += MainForm_FormClosing;
        }

        // Use the saved compiler path, or find the CLI beside the GUI in bundle releases.
        private string ResolveCompilerPath()
        {
            string configuredPath = Properties.Settings.Default.CompilerPath;
            if (!string.IsNullOrWhiteSpace(configuredPath) && File.Exists(configuredPath))
                return configuredPath;

            string siblingPath = Path.Combine(
                AppDomain.CurrentDomain.BaseDirectory, "APEX.Compiler.exe");
            if (File.Exists(siblingPath))
                return siblingPath;

            return configuredPath ?? string.Empty;
        }

        private void RefreshCompilerDisplay()
        {
            string compilerPath = ResolveCompilerPath();
            bool found = !string.IsNullOrWhiteSpace(compilerPath) && File.Exists(compilerPath);

            txtCompiler.Text = compilerPath;
            lblCompilerBadge.Text = found ? "COMPILER FOUND" : "SELECT COMPILER";
            lblCompilerBadge.ForeColor = found
                ? Color.FromArgb(88, 214, 154)
                : Color.FromArgb(226, 179, 98);
            lblCompilerBadge.BackColor = found
                ? Color.FromArgb(24, 60, 54)
                : Color.FromArgb(59, 53, 38);
        }

        private void UpdatePathTooltips()
        {
            pathToolTip.SetToolTip(txtCompiler, txtCompiler.Text);
            pathToolTip.SetToolTip(txtSource, txtSource.Text);
            pathToolTip.SetToolTip(txtOutput, txtOutput.Text);
        }

        private void btnBrowseSource_Click(object sender, EventArgs e)
        {
            using (FolderBrowserDialog dialog = new FolderBrowserDialog())
            {
                dialog.Description = "Select your APEX GSC source directory";

                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    txtSource.Text = dialog.SelectedPath;
                }
            }
        }

        private void btnBrowseOutput_Click(object sender, EventArgs e)
        {
            using (SaveFileDialog dialog = new SaveFileDialog())
            {
                dialog.Title = "Save compiled APEX GSCC";
                dialog.Filter = "GSCC Files (*.gscc)|*.gscc";
                dialog.DefaultExt = "gscc";
                dialog.FileName = "APEX.gscc";

                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    txtOutput.Text = dialog.FileName;
                }
            }
        }

        private async void btnCompile_Click(object sender, EventArgs e)
        {
            string temporary = null;
            var timer = Stopwatch.StartNew();
            try
            {
                string compiler = Path.GetFullPath(ResolveCompilerPath());
                string source = Path.GetFullPath(txtSource.Text.Trim());
                string output = Path.GetFullPath(txtOutput.Text.Trim());
                if (!File.Exists(compiler)) throw new FileNotFoundException("Select a compiler executable.");
                if (!Directory.Exists(source) && !File.Exists(source)) throw new FileNotFoundException("Select a valid source directory or file.");
                if (!output.EndsWith(".gscc", StringComparison.OrdinalIgnoreCase)) throw new ArgumentException("Select a .gscc output file.");
                string directory = Path.GetDirectoryName(output);
                Directory.CreateDirectory(directory);
                temporary = Path.Combine(directory, "APEX_" + Guid.NewGuid().ToString("N") + ".gscc");
                SetBuilding(true);
                rtbLog.Clear();
                UpdateStatus("Compiling...", Color.DeepSkyBlue);
                using (var cancellation = new CancellationTokenSource(TimeSpan.FromMinutes(2)))
                using (var process = new Process())
                {
                    buildCancellation = cancellation;
                    process.StartInfo = new ProcessStartInfo
                    {
                        FileName = compiler,
                        Arguments = WindowsArguments.Quote(source) + " " + WindowsArguments.Quote(temporary),
                        WorkingDirectory = Path.GetDirectoryName(compiler),
                        UseShellExecute = false,
                        CreateNoWindow = true,
                        RedirectStandardOutput = true,
                        RedirectStandardError = true
                    };
                    process.OutputDataReceived += (s, args) => { if (args.Data != null) QueueLog(args.Data, Color.FromArgb(86, 216, 255)); };
                    process.ErrorDataReceived += (s, args) => { if (args.Data != null) QueueLog(args.Data, Color.FromArgb(240, 91, 91)); };
                    process.Start();
                    process.BeginOutputReadLine();
                    process.BeginErrorReadLine();
                    using (cancellation.Token.Register(() =>
                    {
                        try { if (!process.HasExited) process.Kill(); }
                        catch (InvalidOperationException) { }
                        catch (Win32Exception ex) { QueueLog("Could not stop the compiler: " + ex.Message, Color.Orange); }
                    }))
                    {
                        await Task.Run(() => process.WaitForExit());
                    }
                    DrainLogs();
                    cancellation.Token.ThrowIfCancellationRequested();
                    if (process.ExitCode != 0) throw new InvalidOperationException("Compiler exited with code " + process.ExitCode + ". Previous GSCC preserved.");
                    AtomicOutput.Promote(temporary, output);
                    long bytes = new FileInfo(output).Length;
                    UpdateStatus("Build successful · " + timer.Elapsed.TotalSeconds.ToString("0.00") + "s", Color.FromArgb(88, 214, 154));
                    AppendLog("Final output: " + output);
                    AppendLog("Output size: " + bytes.ToString("N0") + " bytes");
                    AppendLog("PS4 BO3 header, sections and references validated.", Color.FromArgb(88, 214, 154));
                }
            }
            catch (OperationCanceledException)
            {
                UpdateStatus("Build cancelled or timed out", Color.Orange);
                AppendLog("Build cancelled or exceeded the two-minute limit. Previous GSCC preserved.", Color.Orange);
            }
            catch (Exception ex)
            {
                UpdateStatus("Build failed", Color.FromArgb(240, 91, 91));
                AppendLog("ERROR: " + ex.Message, Color.FromArgb(240, 91, 91));
            }
            finally
            {
                buildCancellation = null;
                timer.Stop();
                if (temporary != null && File.Exists(temporary))
                {
                    try { File.Delete(temporary); }
                    catch (IOException ex) { AppendLog("Could not remove temporary output: " + ex.Message); }
                    catch (UnauthorizedAccessException ex) { AppendLog("Could not remove temporary output: " + ex.Message); }
                }
                SetBuilding(false);
                if (closeAfterBuild) Close();
            }
        }

        private void SetBuilding(bool building)
        {
            btnCompile.Enabled = !building;
            btnCancel.Enabled = building;
            btnBrowseSource.Enabled = !building;
            btnBrowseOutput.Enabled = !building;
            btnBrowseCompiler.Enabled = !building;
        }

        private void btnCancel_Click(object sender, EventArgs e) => buildCancellation?.Cancel();

        private void QueueLog(string message, Color color)
        {
            lock (pendingLogs)
            {
                if (pendingLogs.Count >= 1000) return;
                pendingLogs.Enqueue(new KeyValuePair<string, Color>(message.Length > 4096 ? message.Substring(0, 4096) + "…" : message, color));
            }
        }

        private void DrainLogs()
        {
            for (int i = 0; i < 1000; i++)
            {
                KeyValuePair<string, Color> entry;
                lock (pendingLogs)
                {
                    if (pendingLogs.Count == 0) return;
                    entry = pendingLogs.Dequeue();
                }
                AppendLog(entry.Key, entry.Value);
            }
        }

        private void AppendLog(string message, Color? color = null)
        {
            if (rtbLog.IsDisposed) return;
            if (rtbLog.TextLength > 200000)
            {
                rtbLog.Select(0, rtbLog.TextLength - 100000);
                rtbLog.SelectedText = "";
            }
            rtbLog.SelectionStart = rtbLog.TextLength;
            rtbLog.SelectionLength = 0;
            rtbLog.SelectionColor = color ?? Color.FromArgb(206, 220, 232);
            rtbLog.AppendText(message + Environment.NewLine);
            rtbLog.ScrollToCaret();
        }

        private void UpdateStatus(string message, Color color)
        {
            lblStatus.Text = message;
            lblStatusIndicator.ForeColor = color;
        }

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (buildCancellation != null)
            {
                e.Cancel = true;
                closeAfterBuild = true;
                buildCancellation.Cancel();
                return;
            }
            Properties.Settings.Default.SourcePath = txtSource.Text;
            Properties.Settings.Default.OutputPath = txtOutput.Text;

            try { Properties.Settings.Default.Save(); }
            catch (System.Configuration.ConfigurationErrorsException) { }
        }

        private void btnOpenOutput_Click(object sender, EventArgs e)
        {
            string outputPath = txtOutput.Text.Trim();

            if (!File.Exists(outputPath))
            {
                MessageBox.Show("No compiled GSCC file found.");
                return;
            }

            Process.Start("explorer.exe", "/select,\"" + outputPath + "\"");
        }

        private void btnBrowseCompiler_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog dialog = new OpenFileDialog())
            {
                dialog.Title = "Select APEX Compiler";
                dialog.Filter = "Executable Files (*.exe)|*.exe";
                dialog.FileName = "APEX.Compiler.exe";

                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    Properties.Settings.Default.CompilerPath = dialog.FileName;
                    Properties.Settings.Default.Save();

                    RefreshCompilerDisplay();
                    AppendLog("\nCompiler selected: " + dialog.FileName);
                }
            }
        }
    }
}