using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using ApexCompiler.Core;

namespace APEX.Compiler.GUI
{
    public partial class MainForm : Form
    {
        private CancellationTokenSource buildCancellation;
        private bool closeAfterBuild, userCancelled, building;
        private readonly Queue<BuildLogEntry> pendingLogs = new Queue<BuildLogEntry>();
        private readonly BuildLog buildLog = new BuildLog();
        private readonly System.Windows.Forms.Timer logTimer;
        private readonly Stopwatch buildTimer = new Stopwatch();
        private LogFilter logFilter = LogFilter.All;
        private int droppedLogs;
        private string currentArtifactPath;

        public MainForm()
        {
            InitializeComponent();
            lblVersion.Text = "v" + typeof(MainForm).Assembly.GetName().Version.ToString(3);
            txtSource.Text = Properties.Settings.Default.SourcePath;
            txtOutput.Text = Properties.Settings.Default.OutputPath;
            RefreshCompilerDisplay();
            foreach (var field in new[] { txtCompiler, txtSource, txtOutput })
            {
                field.TextChanged += (sender, args) =>
                {
                    pathToolTip.SetToolTip(field, field.Text);
                    field.AccessibleDescription = field.Text;
                    if (field == txtOutput) RefreshArtifact();
                };
                pathToolTip.SetToolTip(field, field.Text);
                field.AccessibleDescription = field.Text;
            }
            RefreshArtifact();
            SelectFilter(LogFilter.All);
            AddLog("Select your source directory, then compile or check your scripts.");
            logTimer = new System.Windows.Forms.Timer(components) { Interval = 75 };
            logTimer.Tick += (sender, args) =>
            {
                DrainLogs();
                if (building) lblSummaryDetails.Text = "Elapsed " + Elapsed() + "  /  Cancel to stop safely";
            };
            logTimer.Start();
            FormClosing += MainForm_FormClosing;
        }

        protected override void OnHandleCreated(EventArgs e)
        {
            base.OnHandleCreated(e);
            NativeWindowTheme.Apply(this);
        }

        protected override bool ProcessCmdKey(ref Message message, Keys keyData)
        {
            if (keyData == Keys.Escape && building) { buildCancellation?.Cancel(); userCancelled = true; return true; }
            if (keyData == Keys.F5) { if (!building) btnCompile.PerformClick(); return true; }
            if (keyData == (Keys.Shift | Keys.F5)) { if (!building) btnCheck.PerformClick(); return true; }
            if (keyData == (Keys.Control | Keys.L)) { if (!building) ClearLog(); return true; }
            return base.ProcessCmdKey(ref message, keyData);
        }

        private string ResolveCompilerPath()
        {
            string configured = Properties.Settings.Default.CompilerPath;
            if (!string.IsNullOrWhiteSpace(configured) && File.Exists(configured)) return configured;
            string sibling = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "APEX.Compiler.exe");
            return File.Exists(sibling) ? sibling : configured ?? string.Empty;
        }

        private void RefreshCompilerDisplay()
        {
            string compiler = ResolveCompilerPath();
            bool found = !string.IsNullOrWhiteSpace(compiler) && File.Exists(compiler);
            txtCompiler.Text = compiler;
            lblCompilerBadge.Text = found ? "Compiler ready" : "Select a compiler";
            compilerIndicator.ForeColor = found ? Theme.Green : Theme.Amber;
        }

        private void btnBrowseCompiler_Click(object sender, EventArgs e)
        {
            using (var dialog = new OpenFileDialog { Title = "Select APEX Compiler", Filter = "Executable files (*.exe)|*.exe", FileName = "APEX.Compiler.exe" })
            {
                if (dialog.ShowDialog(this) != DialogResult.OK) return;
                Properties.Settings.Default.CompilerPath = dialog.FileName;
                RefreshCompilerDisplay();
                SaveSettings();
                AddLog("Compiler selected: " + dialog.FileName);
            }
        }

        private void btnBrowseSource_Click(object sender, EventArgs e)
        {
            using (var dialog = new FolderBrowserDialog { Description = "Select the directory containing your GSC source files", SelectedPath = Directory.Exists(txtSource.Text) ? txtSource.Text : "" })
            {
                if (dialog.ShowDialog(this) != DialogResult.OK) return;
                txtSource.Text = dialog.SelectedPath;
                AddLog("Source directory selected: " + dialog.SelectedPath);
            }
        }

        private void btnBrowseOutput_Click(object sender, EventArgs e)
        {
            using (var dialog = new SaveFileDialog { Title = "Save compiled GSCC", Filter = "GSCC files (*.gscc)|*.gscc", DefaultExt = "gscc", FileName = "APEX.gscc" })
            {
                if (dialog.ShowDialog(this) != DialogResult.OK) return;
                txtOutput.Text = dialog.FileName;
            }
        }

        private async void btnCompile_Click(object sender, EventArgs e) => await RunBuildAsync(false);
        private async void btnCheck_Click(object sender, EventArgs e) => await RunBuildAsync(true);

        private async Task RunBuildAsync(bool checkOnly)
        {
            if (building) return;
            string temporary = null;
            ClearLog();
            SelectFilter(LogFilter.All);
            buildTimer.Restart();
            userCancelled = false;
            try
            {
                if (string.IsNullOrWhiteSpace(ResolveCompilerPath())) throw new FileNotFoundException("Select a compiler executable.");
                string compiler = Path.GetFullPath(ResolveCompilerPath());
                if (!File.Exists(compiler)) throw new FileNotFoundException("Select a compiler executable.");
                if (string.IsNullOrWhiteSpace(txtSource.Text)) throw new ArgumentException("Select a source directory.");
                string source = Path.GetFullPath(txtSource.Text.Trim());
                if (!Directory.Exists(source) && !File.Exists(source)) throw new FileNotFoundException("The selected source directory or file no longer exists.");
                string output = null;
                if (!checkOnly)
                {
                    if (string.IsNullOrWhiteSpace(txtOutput.Text)) throw new ArgumentException("Choose a .gscc output file.");
                    output = Path.GetFullPath(txtOutput.Text.Trim());
                    if (!output.EndsWith(".gscc", StringComparison.OrdinalIgnoreCase)) throw new ArgumentException("Choose a .gscc output file.");
                    string directory = Path.GetDirectoryName(output);
                    Directory.CreateDirectory(directory);
                    temporary = Path.Combine(directory, "APEX_" + Guid.NewGuid().ToString("N") + ".gscc");
                }
                SetBuilding(true);
                ShowSummary(checkOnly ? "Checking scripts..." : "Compiling scripts...", "Elapsed 0.00 s  /  Cancel to stop safely", Glyph.Clock, Theme.Cyan);
                UpdateStatus(checkOnly ? "Checking scripts..." : "Compiling...", Theme.Cyan);
                AddLog(checkOnly ? "Checking GSC sources without writing output..." : "Compiling GSC sources...");
                using (var cancellation = new CancellationTokenSource(TimeSpan.FromMinutes(2)))
                using (var process = new Process())
                {
                    buildCancellation = cancellation;
                    process.StartInfo = new ProcessStartInfo
                    {
                        FileName = compiler,
                        Arguments = checkOnly ? "--check " + WindowsArguments.Quote(source) : WindowsArguments.Quote(source) + " " + WindowsArguments.Quote(temporary),
                        WorkingDirectory = Path.GetDirectoryName(compiler),
                        UseShellExecute = false, CreateNoWindow = true, RedirectStandardOutput = true, RedirectStandardError = true
                    };
                    string temporaryPath = temporary;
                    process.OutputDataReceived += (sender, args) =>
                    {
                        if (args.Data == null) return;
                        // The GUI reports the final destination once replacement succeeds.
                        if (temporaryPath != null && args.Data.StartsWith("Wrote " + temporaryPath, StringComparison.OrdinalIgnoreCase)) return;
                        QueueLog(args.Data, BuildLog.Classify(args.Data, false));
                    };
                    process.ErrorDataReceived += (sender, args) => { if (args.Data != null) QueueLog(args.Data, BuildLog.Classify(args.Data, true)); };
                    process.Start();
                    process.BeginOutputReadLine();
                    process.BeginErrorReadLine();
                    using (cancellation.Token.Register(() =>
                    {
                        try { if (!process.HasExited) process.Kill(); }
                        catch (InvalidOperationException) { }
                        catch (Win32Exception ex) { QueueLog("Could not stop the compiler: " + ex.Message, LogSeverity.Warning); }
                    }))
                    {
                        await Task.Run(() => process.WaitForExit());
                    }
                    DrainLogs();
                    cancellation.Token.ThrowIfCancellationRequested();
                    if (process.ExitCode != 0)
                        throw new InvalidOperationException(checkOnly && process.ExitCode == 2
                            ? "Check only requires a compatible APEX CLI with --check support (v1.2.0 or later)."
                            : "Compiler exited with code " + process.ExitCode + ". Previous output is preserved.");
                    if (checkOnly)
                    {
                        buildTimer.Stop();
                        ShowSummary("Check passed", Elapsed() + "  /  No output written  /  " + WarningSummary(), Glyph.Success, Theme.Green);
                        AddLog("Checks completed successfully. No output was written.", LogSeverity.Success);
                    }
                    else
                    {
                        AddLog("Validating GSCC structure...");
                        AtomicOutput.Promote(temporary, output);
                        long bytes = new FileInfo(output).Length;
                        buildTimer.Stop();
                        ShowSummary("Build successful", Elapsed() + "  /  " + FormatSize(bytes) + "  /  " + WarningSummary(), Glyph.Success, Theme.Green);
                        AddLog("Wrote " + output);
                        AddLog("Build completed successfully.", LogSeverity.Success);
                    }
                    UpdateStatus("Ready", Theme.Green);
                }
            }
            catch (OperationCanceledException)
            {
                buildTimer.Stop();
                string title = userCancelled || closeAfterBuild ? "Build cancelled" : "Build timed out";
                ShowSummary(title, Elapsed() + "  /  Previous output preserved", Glyph.Cancel, Theme.Amber);
                AddLog(title + ". Previous output is preserved.", LogSeverity.Warning);
                UpdateStatus(title, Theme.Amber);
            }
            catch (Exception ex)
            {
                buildTimer.Stop();
                ShowSummary(checkOnly ? "Check failed" : "Build failed", Elapsed() + "  /  Previous output preserved", Glyph.Error, Theme.Red);
                AddLog(ex.Message, LogSeverity.Error);
                UpdateStatus(checkOnly ? "Check failed" : "Build failed", Theme.Red);
            }
            finally
            {
                buildCancellation = null;
                buildTimer.Stop();
                if (temporary != null && File.Exists(temporary))
                {
                    try { File.Delete(temporary); }
                    catch (IOException ex) { AddLog("Could not remove temporary output: " + ex.Message, LogSeverity.Warning); }
                    catch (UnauthorizedAccessException ex) { AddLog("Could not remove temporary output: " + ex.Message, LogSeverity.Warning); }
                }
                SetBuilding(false);
                RefreshArtifact();
                if (closeAfterBuild) Close();
            }
        }

        private string Elapsed() => buildTimer.Elapsed.TotalSeconds.ToString("0.00", CultureInfo.InvariantCulture) + " s";
        private string WarningSummary() => buildLog.WarningCount + (buildLog.WarningCount == 1 ? " warning" : " warnings");
        private static string FormatSize(long bytes) => bytes < 1024 ? bytes + " bytes" : (bytes / 1024D).ToString("0.00", CultureInfo.InvariantCulture) + " KB";

        private void SetBuilding(bool value)
        {
            building = value;
            btnCompile.Enabled = btnCheck.Enabled = btnBrowseSource.Enabled = btnBrowseOutput.Enabled = btnBrowseCompiler.Enabled = btnClearLog.Enabled = !value;
            btnCancel.Enabled = value;
            RefreshArtifact();
        }
        private void btnCancel_Click(object sender, EventArgs e) { userCancelled = true; buildCancellation?.Cancel(); }

        private void ShowSummary(string title, string details, Glyph glyph, Color accent)
        {
            lblSummaryTitle.Text = title; lblSummaryTitle.ForeColor = accent;
            lblSummaryDetails.Text = details;
            summaryIcon.Glyph = glyph; summaryIcon.ForeColor = accent;
            summaryCard.BorderColor = Color.FromArgb(accent.R / 2, accent.G / 2, accent.B / 2);
            Color background = Color.FromArgb(14 + accent.R / 18, 25 + accent.G / 14, 32 + accent.B / 18);
            summaryCard.BackColor = background;
            SetSurface(summaryCard, background);
            summaryCard.Invalidate(true);
        }
        private static void SetSurface(Control parent, Color color)
        {
            foreach (Control child in parent.Controls)
            {
                if (child is TableLayoutPanel) { child.BackColor = color; SetSurface(child, color); }
            }
        }
        private void UpdateStatus(string message, Color color) { lblStatus.Text = message; statusIndicator.ForeColor = color; }

        private void RefreshArtifact()
        {
            currentArtifactPath = null;
            try
            {
                if (!string.IsNullOrWhiteSpace(txtOutput.Text) && File.Exists(txtOutput.Text))
                {
                    currentArtifactPath = Path.GetFullPath(txtOutput.Text);
                    lblArtifactName.Text = Path.GetFileName(currentArtifactPath);
                    lblArtifactSize.Text = new FileInfo(currentArtifactPath).Length.ToString("N0") + " bytes";
                    pathToolTip.SetToolTip(lblArtifactName, currentArtifactPath);
                }
                else { lblArtifactName.Text = "No output yet"; lblArtifactSize.Text = "Compile a script to create a GSCC file."; }
            }
            catch (IOException) { lblArtifactName.Text = "Output unavailable"; lblArtifactSize.Text = "Check the destination and try again."; currentArtifactPath = null; }
            catch (UnauthorizedAccessException) { lblArtifactName.Text = "Output unavailable"; lblArtifactSize.Text = "Access to the output was denied."; currentArtifactPath = null; }
            catch (ArgumentException) { lblArtifactName.Text = "No output yet"; lblArtifactSize.Text = "Choose a valid destination."; }
            btnOpenOutput.Enabled = btnShowArtifact.Enabled = currentArtifactPath != null && !building;
        }

        private void QueueLog(string message, LogSeverity severity)
        {
            lock (pendingLogs)
            {
                if (pendingLogs.Count >= 1000) { droppedLogs++; return; }
                pendingLogs.Enqueue(new BuildLogEntry(message, severity));
            }
        }
        private void DrainLogs()
        {
            var drained = new List<BuildLogEntry>();
            lock (pendingLogs)
            {
                while (pendingLogs.Count != 0) drained.Add(pendingLogs.Dequeue());
                if (droppedLogs != 0)
                {
                    drained.Add(new BuildLogEntry(droppedLogs + " messages omitted to keep the interface responsive.", LogSeverity.Warning));
                    droppedLogs = 0;
                }
            }
            bool trimmed = false;
            foreach (var entry in drained) trimmed |= buildLog.Add(entry);
            if (trimmed) RenderLog();
            else foreach (var entry in drained) if (BuildLog.Matches(entry, logFilter)) AppendEntry(entry);
        }
        private void AddLog(string message, LogSeverity severity = LogSeverity.Info)
        {
            var entry = new BuildLogEntry(message, severity);
            if (buildLog.Add(entry)) RenderLog();
            else if (BuildLog.Matches(entry, logFilter)) AppendEntry(entry);
        }
        private void AppendEntry(BuildLogEntry entry)
        {
            if (rtbLog.IsDisposed) return;
            bool selecting = rtbLog.Focused && rtbLog.SelectionLength != 0;
            int start = rtbLog.SelectionStart, length = rtbLog.SelectionLength;
            rtbLog.SelectionStart = rtbLog.TextLength; rtbLog.SelectionLength = 0;
            rtbLog.SelectionColor = Theme.Muted;
            rtbLog.AppendText("[" + entry.Timestamp.ToString("HH:mm:ss", CultureInfo.InvariantCulture) + "]  ");
            Color color = entry.Severity == LogSeverity.Error ? Theme.Red : entry.Severity == LogSeverity.Warning ? Theme.Amber : entry.Severity == LogSeverity.Success ? Theme.Green : Theme.Cyan;
            rtbLog.SelectionColor = color;
            rtbLog.AppendText(("[" + entry.Severity.ToString().ToUpperInvariant() + "]").PadRight(11));
            rtbLog.SelectionColor = entry.Severity == LogSeverity.Info ? Theme.Text : color;
            int path = entry.Message.IndexOf(":\\", StringComparison.Ordinal);
            if (entry.Severity == LogSeverity.Info && path > 0)
            {
                rtbLog.AppendText(entry.Message.Substring(0, path - 1));
                rtbLog.SelectionColor = Theme.Cyan;
                rtbLog.AppendText(entry.Message.Substring(path - 1));
            }
            else rtbLog.AppendText(entry.Message);
            rtbLog.AppendText(Environment.NewLine);
            if (selecting) rtbLog.Select(start, length);
            else rtbLog.ScrollToCaret();
        }
        private void SelectFilter(LogFilter filter)
        {
            logFilter = filter;
            btnFilterAll.Selected = filter == LogFilter.All;
            btnFilterWarnings.Selected = filter == LogFilter.Warnings;
            btnFilterErrors.Selected = filter == LogFilter.Errors;
            foreach (var button in new[] { btnFilterAll, btnFilterWarnings, btnFilterErrors })
                button.AccessibleDescription = button.Selected ? "Selected message filter" : "Select this message filter";
            RenderLog();
        }
        private void RenderLog()
        {
            rtbLog.Clear();
            foreach (var entry in buildLog.Visible(logFilter)) AppendEntry(entry);
        }
        private void ClearLog()
        {
            lock (pendingLogs) { pendingLogs.Clear(); droppedLogs = 0; }
            buildLog.Clear();
            rtbLog.Clear();
        }

        private void btnOpenOutput_Click(object sender, EventArgs e)
        {
            RefreshArtifact();
            if (currentArtifactPath == null) { AddLog("No compiled output is available yet.", LogSeverity.Warning); return; }
            try { Process.Start("explorer.exe", "/select," + WindowsArguments.Quote(currentArtifactPath)); }
            catch (Win32Exception ex) { AddLog("Could not open File Explorer: " + ex.Message, LogSeverity.Warning); }
        }
        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (buildCancellation != null)
            {
                e.Cancel = true; closeAfterBuild = true; userCancelled = true; buildCancellation.Cancel(); return;
            }
            Properties.Settings.Default.SourcePath = txtSource.Text;
            Properties.Settings.Default.OutputPath = txtOutput.Text;
            SaveSettings();
        }
        private void SaveSettings()
        {
            try { Properties.Settings.Default.Save(); }
            catch (System.Configuration.ConfigurationErrorsException ex) { AddLog("Could not save paths: " + ex.Message, LogSeverity.Warning); }
            catch (IOException ex) { AddLog("Could not save paths: " + ex.Message, LogSeverity.Warning); }
            catch (UnauthorizedAccessException ex) { AddLog("Could not save paths: " + ex.Message, LogSeverity.Warning); }
        }
    }
}
