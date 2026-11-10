namespace APEX.Compiler.GUI
{
    partial class MainForm
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
                components.Dispose();
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.pathToolTip = new System.Windows.Forms.ToolTip(this.components);

            this.lblTitle = new System.Windows.Forms.Label();
            this.lblSubtitle = new System.Windows.Forms.Label();
            this.pnlHeaderLine = new System.Windows.Forms.Panel();
            this.lblConfiguration = new System.Windows.Forms.Label();
            this.lblConfigurationHelp = new System.Windows.Forms.Label();
            this.lblCompilerBadge = new System.Windows.Forms.Label();
            this.lblCompiler = new System.Windows.Forms.Label();
            this.pnlCompilerPath = new System.Windows.Forms.Panel();
            this.txtCompiler = new System.Windows.Forms.TextBox();
            this.btnBrowseCompiler = new System.Windows.Forms.Button();
            this.lblSource = new System.Windows.Forms.Label();
            this.pnlSourcePath = new System.Windows.Forms.Panel();
            this.txtSource = new System.Windows.Forms.TextBox();
            this.btnBrowseSource = new System.Windows.Forms.Button();
            this.lblOutput = new System.Windows.Forms.Label();
            this.pnlOutputPath = new System.Windows.Forms.Panel();
            this.txtOutput = new System.Windows.Forms.TextBox();
            this.btnBrowseOutput = new System.Windows.Forms.Button();
            this.pnlActionDivider = new System.Windows.Forms.Panel();
            this.pnlActions = new System.Windows.Forms.TableLayoutPanel();
            this.btnCompile = new System.Windows.Forms.Button();
            this.btnCancel = new System.Windows.Forms.Button();
            this.btnOpenOutput = new System.Windows.Forms.Button();
            this.lblConsole = new System.Windows.Forms.Label();
            this.lblConsoleHint = new System.Windows.Forms.Label();
            this.pnlConsoleSurface = new System.Windows.Forms.Panel();
            this.rtbLog = new System.Windows.Forms.RichTextBox();
            this.pnlFooterLine = new System.Windows.Forms.Panel();
            this.lblStatusIndicator = new System.Windows.Forms.Label();
            this.lblStatus = new System.Windows.Forms.Label();
            this.lblVersion = new System.Windows.Forms.Label();
            this.pnlCompilerPath.SuspendLayout();
            this.pnlSourcePath.SuspendLayout();
            this.pnlOutputPath.SuspendLayout();
            this.pnlActions.SuspendLayout();
            this.pnlConsoleSurface.SuspendLayout();
            this.SuspendLayout();

            // Header: product branding and platform.
            this.lblTitle.Font = new System.Drawing.Font("Segoe UI Semibold", 21F, System.Drawing.FontStyle.Bold);
            this.lblTitle.ForeColor = System.Drawing.Color.FromArgb(54, 208, 255);
            this.lblTitle.Location = new System.Drawing.Point(28, 25);
            this.lblTitle.Name = "lblTitle";
            this.lblTitle.Size = new System.Drawing.Size(560, 42);
            this.lblTitle.Text = "APEX COMPILER";

            this.lblSubtitle.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.lblSubtitle.ForeColor = System.Drawing.Color.FromArgb(198, 211, 225);
            this.lblSubtitle.Location = new System.Drawing.Point(30, 72);
            this.lblSubtitle.Name = "lblSubtitle";
            this.lblSubtitle.Size = new System.Drawing.Size(400, 20);
            this.lblSubtitle.Text = "BLACK OPS III  /  PLAYSTATION 4";

            this.pnlHeaderLine.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            this.pnlHeaderLine.BackColor = System.Drawing.Color.FromArgb(54, 208, 255);
            this.pnlHeaderLine.Location = new System.Drawing.Point(28, 105);
            this.pnlHeaderLine.Name = "pnlHeaderLine";
            this.pnlHeaderLine.Size = new System.Drawing.Size(864, 2);

            // Configuration heading and compiler availability badge.
            this.lblConfiguration.Font = new System.Drawing.Font("Segoe UI Semibold", 10F, System.Drawing.FontStyle.Bold);
            this.lblConfiguration.ForeColor = System.Drawing.Color.FromArgb(54, 208, 255);
            this.lblConfiguration.Location = new System.Drawing.Point(28, 122);
            this.lblConfiguration.Name = "lblConfiguration";
            this.lblConfiguration.Size = new System.Drawing.Size(390, 24);
            this.lblConfiguration.Text = "BUILD CONFIGURATION";

            this.lblConfigurationHelp.Font = new System.Drawing.Font("Segoe UI", 8.5F);
            this.lblConfigurationHelp.ForeColor = System.Drawing.Color.FromArgb(155, 173, 194);
            this.lblConfigurationHelp.Location = new System.Drawing.Point(28, 147);
            this.lblConfigurationHelp.Name = "lblConfigurationHelp";
            this.lblConfigurationHelp.Size = new System.Drawing.Size(570, 19);
            this.lblConfigurationHelp.Text = "Choose the compiler, source directory and destination file.";

            this.lblCompilerBadge.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
            this.lblCompilerBadge.BackColor = System.Drawing.Color.FromArgb(59, 53, 38);
            this.lblCompilerBadge.Font = new System.Drawing.Font("Segoe UI Semibold", 8F, System.Drawing.FontStyle.Bold);
            this.lblCompilerBadge.ForeColor = System.Drawing.Color.FromArgb(226, 179, 98);
            this.lblCompilerBadge.Location = new System.Drawing.Point(746, 125);
            this.lblCompilerBadge.Name = "lblCompilerBadge";
            this.lblCompilerBadge.Size = new System.Drawing.Size(146, 25);
            this.lblCompilerBadge.Text = "SELECT COMPILER";
            this.lblCompilerBadge.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;

            // Compiler path: a display-only field backed by the existing saved setting.
            this.lblCompiler.Font = new System.Drawing.Font("Segoe UI Semibold", 8.5F, System.Drawing.FontStyle.Bold);
            this.lblCompiler.ForeColor = System.Drawing.Color.FromArgb(198, 211, 225);
            this.lblCompiler.Location = new System.Drawing.Point(28, 176);
            this.lblCompiler.Name = "lblCompiler";
            this.lblCompiler.Size = new System.Drawing.Size(360, 18);
            this.lblCompiler.Text = "COMPILER EXECUTABLE";

            this.pnlCompilerPath.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            this.pnlCompilerPath.BackColor = System.Drawing.Color.FromArgb(38, 53, 71);
            this.pnlCompilerPath.Location = new System.Drawing.Point(28, 198);
            this.pnlCompilerPath.Name = "pnlCompilerPath";
            this.pnlCompilerPath.Size = new System.Drawing.Size(708, 34);
            this.pnlCompilerPath.Controls.Add(this.txtCompiler);

            this.txtCompiler.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            this.txtCompiler.BackColor = System.Drawing.Color.FromArgb(38, 53, 71);
            this.txtCompiler.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.txtCompiler.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.txtCompiler.ForeColor = System.Drawing.Color.FromArgb(198, 211, 225);
            this.txtCompiler.Location = new System.Drawing.Point(12, 8);
            this.txtCompiler.Name = "txtCompiler";
            this.txtCompiler.ReadOnly = true;
            this.txtCompiler.Size = new System.Drawing.Size(684, 20);
            this.txtCompiler.TabIndex = 0;
            this.txtCompiler.TabStop = false;

            this.btnBrowseCompiler.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
            this.btnBrowseCompiler.BackColor = System.Drawing.Color.FromArgb(8, 127, 170);
            this.btnBrowseCompiler.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnBrowseCompiler.FlatAppearance.BorderSize = 0;
            this.btnBrowseCompiler.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(12, 149, 194);
            this.btnBrowseCompiler.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnBrowseCompiler.Font = new System.Drawing.Font("Segoe UI Semibold", 9F, System.Drawing.FontStyle.Bold);
            this.btnBrowseCompiler.ForeColor = System.Drawing.Color.White;
            this.btnBrowseCompiler.Location = new System.Drawing.Point(752, 198);
            this.btnBrowseCompiler.Name = "btnBrowseCompiler";
            this.btnBrowseCompiler.Size = new System.Drawing.Size(140, 34);
            this.btnBrowseCompiler.TabIndex = 0;
            this.btnBrowseCompiler.Text = "Browse";
            this.btnBrowseCompiler.UseVisualStyleBackColor = false;
            this.btnBrowseCompiler.Click += new System.EventHandler(this.btnBrowseCompiler_Click);

            // Source directory.
            this.lblSource.Font = new System.Drawing.Font("Segoe UI Semibold", 8.5F, System.Drawing.FontStyle.Bold);
            this.lblSource.ForeColor = System.Drawing.Color.FromArgb(198, 211, 225);
            this.lblSource.Location = new System.Drawing.Point(28, 241);
            this.lblSource.Name = "lblSource";
            this.lblSource.Size = new System.Drawing.Size(360, 18);
            this.lblSource.Text = "SOURCE DIRECTORY";

            this.pnlSourcePath.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            this.pnlSourcePath.BackColor = System.Drawing.Color.FromArgb(38, 53, 71);
            this.pnlSourcePath.Location = new System.Drawing.Point(28, 263);
            this.pnlSourcePath.Name = "pnlSourcePath";
            this.pnlSourcePath.Size = new System.Drawing.Size(708, 34);
            this.pnlSourcePath.Controls.Add(this.txtSource);

            this.txtSource.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            this.txtSource.BackColor = System.Drawing.Color.FromArgb(38, 53, 71);
            this.txtSource.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.txtSource.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.txtSource.ForeColor = System.Drawing.Color.FromArgb(198, 211, 225);
            this.txtSource.Location = new System.Drawing.Point(12, 8);
            this.txtSource.Name = "txtSource";
            this.txtSource.ReadOnly = true;
            this.txtSource.Size = new System.Drawing.Size(684, 20);
            this.txtSource.TabIndex = 1;
            this.txtSource.TabStop = false;

            this.btnBrowseSource.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
            this.btnBrowseSource.BackColor = System.Drawing.Color.FromArgb(8, 127, 170);
            this.btnBrowseSource.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnBrowseSource.FlatAppearance.BorderSize = 0;
            this.btnBrowseSource.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(12, 149, 194);
            this.btnBrowseSource.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnBrowseSource.Font = new System.Drawing.Font("Segoe UI Semibold", 9F, System.Drawing.FontStyle.Bold);
            this.btnBrowseSource.ForeColor = System.Drawing.Color.White;
            this.btnBrowseSource.Location = new System.Drawing.Point(752, 263);
            this.btnBrowseSource.Name = "btnBrowseSource";
            this.btnBrowseSource.Size = new System.Drawing.Size(140, 34);
            this.btnBrowseSource.TabIndex = 1;
            this.btnBrowseSource.Text = "Browse";
            this.btnBrowseSource.UseVisualStyleBackColor = false;
            this.btnBrowseSource.Click += new System.EventHandler(this.btnBrowseSource_Click);

            // GSCC destination.
            this.lblOutput.Font = new System.Drawing.Font("Segoe UI Semibold", 8.5F, System.Drawing.FontStyle.Bold);
            this.lblOutput.ForeColor = System.Drawing.Color.FromArgb(198, 211, 225);
            this.lblOutput.Location = new System.Drawing.Point(28, 306);
            this.lblOutput.Name = "lblOutput";
            this.lblOutput.Size = new System.Drawing.Size(360, 18);
            this.lblOutput.Text = "OUTPUT FILE";

            this.pnlOutputPath.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            this.pnlOutputPath.BackColor = System.Drawing.Color.FromArgb(38, 53, 71);
            this.pnlOutputPath.Location = new System.Drawing.Point(28, 328);
            this.pnlOutputPath.Name = "pnlOutputPath";
            this.pnlOutputPath.Size = new System.Drawing.Size(708, 34);
            this.pnlOutputPath.Controls.Add(this.txtOutput);

            this.txtOutput.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            this.txtOutput.BackColor = System.Drawing.Color.FromArgb(38, 53, 71);
            this.txtOutput.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.txtOutput.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.txtOutput.ForeColor = System.Drawing.Color.FromArgb(198, 211, 225);
            this.txtOutput.Location = new System.Drawing.Point(12, 8);
            this.txtOutput.Name = "txtOutput";
            this.txtOutput.ReadOnly = true;
            this.txtOutput.Size = new System.Drawing.Size(684, 20);
            this.txtOutput.TabIndex = 2;
            this.txtOutput.TabStop = false;

            this.btnBrowseOutput.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
            this.btnBrowseOutput.BackColor = System.Drawing.Color.FromArgb(8, 127, 170);
            this.btnBrowseOutput.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnBrowseOutput.FlatAppearance.BorderSize = 0;
            this.btnBrowseOutput.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(12, 149, 194);
            this.btnBrowseOutput.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnBrowseOutput.Font = new System.Drawing.Font("Segoe UI Semibold", 9F, System.Drawing.FontStyle.Bold);
            this.btnBrowseOutput.ForeColor = System.Drawing.Color.White;
            this.btnBrowseOutput.Location = new System.Drawing.Point(752, 328);
            this.btnBrowseOutput.Name = "btnBrowseOutput";
            this.btnBrowseOutput.Size = new System.Drawing.Size(140, 34);
            this.btnBrowseOutput.TabIndex = 2;
            this.btnBrowseOutput.Text = "Browse";
            this.btnBrowseOutput.UseVisualStyleBackColor = false;
            this.btnBrowseOutput.Click += new System.EventHandler(this.btnBrowseOutput_Click);

            // Two related actions share one responsive row.
            this.pnlActionDivider.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            this.pnlActionDivider.BackColor = System.Drawing.Color.FromArgb(40, 56, 72);
            this.pnlActionDivider.Location = new System.Drawing.Point(28, 376);
            this.pnlActionDivider.Name = "pnlActionDivider";
            this.pnlActionDivider.Size = new System.Drawing.Size(864, 1);

            this.pnlActions.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            this.pnlActions.ColumnCount = 3;
            this.pnlActions.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 45F));
            this.pnlActions.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 35F));
            this.pnlActions.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 20F));
            this.pnlActions.Location = new System.Drawing.Point(28, 392);
            this.pnlActions.Margin = new System.Windows.Forms.Padding(0);
            this.pnlActions.Name = "pnlActions";
            this.pnlActions.RowCount = 1;
            this.pnlActions.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.pnlActions.Size = new System.Drawing.Size(864, 46);
            this.pnlActions.Controls.Add(this.btnCompile, 0, 0);
            this.pnlActions.Controls.Add(this.btnOpenOutput, 1, 0);
            this.pnlActions.Controls.Add(this.btnCancel, 2, 0);
            this.btnCancel.Text = "CANCEL";
            this.btnCancel.AccessibleName = "Cancel compilation";
            this.btnCancel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.btnCancel.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnCancel.ForeColor = System.Drawing.Color.FromArgb(226, 179, 98);
            this.btnCancel.Enabled = false;
            this.btnCancel.TabIndex = 5;
            this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);

            this.btnCompile.BackColor = System.Drawing.Color.FromArgb(7, 136, 185);
            this.btnCompile.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnCompile.Dock = System.Windows.Forms.DockStyle.Fill;
            this.btnCompile.FlatAppearance.BorderSize = 0;
            this.btnCompile.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(12, 157, 209);
            this.btnCompile.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnCompile.Font = new System.Drawing.Font("Segoe UI Semibold", 10F, System.Drawing.FontStyle.Bold);
            this.btnCompile.ForeColor = System.Drawing.Color.White;
            this.btnCompile.Margin = new System.Windows.Forms.Padding(0, 0, 6, 0);
            this.btnCompile.Name = "btnCompile";
            this.btnCompile.TabIndex = 3;
            this.btnCompile.Text = "▶   COMPILE GSCC";
            this.btnCompile.UseVisualStyleBackColor = false;
            this.btnCompile.Click += new System.EventHandler(this.btnCompile_Click);

            this.btnOpenOutput.BackColor = System.Drawing.Color.FromArgb(23, 31, 42);
            this.btnOpenOutput.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnOpenOutput.Dock = System.Windows.Forms.DockStyle.Fill;
            this.btnOpenOutput.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(38, 126, 163);
            this.btnOpenOutput.FlatAppearance.BorderSize = 1;
            this.btnOpenOutput.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(38, 53, 71);
            this.btnOpenOutput.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnOpenOutput.Font = new System.Drawing.Font("Segoe UI Semibold", 9F, System.Drawing.FontStyle.Bold);
            this.btnOpenOutput.ForeColor = System.Drawing.Color.FromArgb(54, 208, 255);
            this.btnOpenOutput.Margin = new System.Windows.Forms.Padding(6, 0, 0, 0);
            this.btnOpenOutput.Name = "btnOpenOutput";
            this.btnOpenOutput.TabIndex = 4;
            this.btnOpenOutput.Text = "OPEN OUTPUT FOLDER";
            this.btnOpenOutput.UseVisualStyleBackColor = false;
            this.btnOpenOutput.Click += new System.EventHandler(this.btnOpenOutput_Click);

            // Build console; panel provides inner padding and flexes with the window.
            this.lblConsole.Font = new System.Drawing.Font("Segoe UI Semibold", 10F, System.Drawing.FontStyle.Bold);
            this.lblConsole.ForeColor = System.Drawing.Color.FromArgb(54, 208, 255);
            this.lblConsole.Location = new System.Drawing.Point(28, 454);
            this.lblConsole.Name = "lblConsole";
            this.lblConsole.Size = new System.Drawing.Size(390, 24);
            this.lblConsole.Text = "BUILD CONSOLE";

            this.lblConsoleHint.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
            this.lblConsoleHint.Font = new System.Drawing.Font("Segoe UI", 8F);
            this.lblConsoleHint.ForeColor = System.Drawing.Color.FromArgb(155, 173, 194);
            this.lblConsoleHint.Location = new System.Drawing.Point(722, 458);
            this.lblConsoleHint.Name = "lblConsoleHint";
            this.lblConsoleHint.Size = new System.Drawing.Size(170, 20);
            this.lblConsoleHint.Text = "OUTPUT LOG";
            this.lblConsoleHint.TextAlign = System.Drawing.ContentAlignment.MiddleRight;

            this.pnlConsoleSurface.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            this.pnlConsoleSurface.BackColor = System.Drawing.Color.FromArgb(12, 19, 29);
            this.pnlConsoleSurface.Location = new System.Drawing.Point(28, 482);
            this.pnlConsoleSurface.Name = "pnlConsoleSurface";
            this.pnlConsoleSurface.Padding = new System.Windows.Forms.Padding(12, 10, 12, 10);
            this.pnlConsoleSurface.Size = new System.Drawing.Size(864, 171);
            this.pnlConsoleSurface.Controls.Add(this.rtbLog);

            this.rtbLog.BackColor = System.Drawing.Color.FromArgb(12, 19, 29);
            this.rtbLog.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.rtbLog.Dock = System.Windows.Forms.DockStyle.Fill;
            this.rtbLog.DetectUrls = false;
            this.rtbLog.Font = new System.Drawing.Font("Consolas", 9.5F);
            this.rtbLog.ForeColor = System.Drawing.Color.FromArgb(110, 245, 139);
            this.rtbLog.Location = new System.Drawing.Point(12, 10);
            this.rtbLog.Name = "rtbLog";
            this.rtbLog.ReadOnly = true;
            this.rtbLog.Size = new System.Drawing.Size(840, 151);
            this.rtbLog.TabIndex = 5;
            this.rtbLog.Text = "Ready to compile...";
            this.rtbLog.WordWrap = false;

            // Footer: status and version only.
            this.pnlFooterLine.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            this.pnlFooterLine.BackColor = System.Drawing.Color.FromArgb(40, 56, 72);
            this.pnlFooterLine.Location = new System.Drawing.Point(28, 671);
            this.pnlFooterLine.Name = "pnlFooterLine";
            this.pnlFooterLine.Size = new System.Drawing.Size(864, 1);

            this.lblStatusIndicator.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left;
            this.lblStatusIndicator.Font = new System.Drawing.Font("Segoe UI", 12F);
            this.lblStatusIndicator.ForeColor = System.Drawing.Color.FromArgb(88, 214, 154);
            this.lblStatusIndicator.Location = new System.Drawing.Point(28, 681);
            this.lblStatusIndicator.Name = "lblStatusIndicator";
            this.lblStatusIndicator.Size = new System.Drawing.Size(18, 26);
            this.lblStatusIndicator.Text = "●";
            this.lblStatusIndicator.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;

            this.lblStatus.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left;
            this.lblStatus.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.lblStatus.ForeColor = System.Drawing.Color.FromArgb(198, 211, 225);
            this.lblStatus.Location = new System.Drawing.Point(53, 683);
            this.lblStatus.Name = "lblStatus";
            this.lblStatus.Size = new System.Drawing.Size(390, 22);
            this.lblStatus.Text = "Ready";
            this.lblStatus.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;

            this.lblVersion.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right;
            this.lblVersion.Font = new System.Drawing.Font("Segoe UI", 8.5F);
            this.lblVersion.ForeColor = System.Drawing.Color.FromArgb(155, 173, 194);
            this.lblVersion.Location = new System.Drawing.Point(677, 683);
            this.lblVersion.Name = "lblVersion";
            this.lblVersion.Size = new System.Drawing.Size(215, 22);
            this.lblVersion.Text = "APEX COMPILER";
            this.lblVersion.TextAlign = System.Drawing.ContentAlignment.MiddleRight;

            // Form.
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(23, 31, 42);
            this.ClientSize = new System.Drawing.Size(920, 720);
            this.Controls.Add(this.lblTitle);
            this.Controls.Add(this.lblSubtitle);
            this.Controls.Add(this.pnlHeaderLine);
            this.Controls.Add(this.lblConfiguration);
            this.Controls.Add(this.lblConfigurationHelp);
            this.Controls.Add(this.lblCompilerBadge);
            this.Controls.Add(this.lblCompiler);
            this.Controls.Add(this.pnlCompilerPath);
            this.Controls.Add(this.btnBrowseCompiler);
            this.Controls.Add(this.lblSource);
            this.Controls.Add(this.pnlSourcePath);
            this.Controls.Add(this.btnBrowseSource);
            this.Controls.Add(this.lblOutput);
            this.Controls.Add(this.pnlOutputPath);
            this.Controls.Add(this.btnBrowseOutput);
            this.Controls.Add(this.pnlActionDivider);
            this.Controls.Add(this.pnlActions);
            this.Controls.Add(this.lblConsole);
            this.Controls.Add(this.lblConsoleHint);
            this.Controls.Add(this.pnlConsoleSurface);
            this.Controls.Add(this.pnlFooterLine);
            this.Controls.Add(this.lblStatusIndicator);
            this.Controls.Add(this.lblStatus);
            this.Controls.Add(this.lblVersion);
            this.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.ForeColor = System.Drawing.Color.FromArgb(198, 211, 225);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.Sizable;
            this.MinimumSize = new System.Drawing.Size(800, 760);
            this.Name = "MainForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "APEX Compiler";
            this.pnlCompilerPath.ResumeLayout(false);
            this.pnlCompilerPath.PerformLayout();
            this.pnlSourcePath.ResumeLayout(false);
            this.pnlSourcePath.PerformLayout();
            this.pnlOutputPath.ResumeLayout(false);
            this.pnlOutputPath.PerformLayout();
            this.pnlActions.ResumeLayout(false);
            this.pnlConsoleSurface.ResumeLayout(false);
            this.ResumeLayout(false);
        }

        #endregion

        private System.Windows.Forms.ToolTip pathToolTip;
        private System.Windows.Forms.Label lblTitle;
        private System.Windows.Forms.Label lblSubtitle;
        private System.Windows.Forms.Panel pnlHeaderLine;
        private System.Windows.Forms.Label lblConfiguration;
        private System.Windows.Forms.Label lblConfigurationHelp;
        private System.Windows.Forms.Label lblCompilerBadge;
        private System.Windows.Forms.Label lblCompiler;
        private System.Windows.Forms.Panel pnlCompilerPath;
        private System.Windows.Forms.TextBox txtCompiler;
        private System.Windows.Forms.Button btnBrowseCompiler;
        private System.Windows.Forms.Label lblSource;
        private System.Windows.Forms.Panel pnlSourcePath;
        private System.Windows.Forms.TextBox txtSource;
        private System.Windows.Forms.Button btnBrowseSource;
        private System.Windows.Forms.Label lblOutput;
        private System.Windows.Forms.Panel pnlOutputPath;
        private System.Windows.Forms.TextBox txtOutput;
        private System.Windows.Forms.Button btnBrowseOutput;
        private System.Windows.Forms.Panel pnlActionDivider;
        private System.Windows.Forms.TableLayoutPanel pnlActions;
        private System.Windows.Forms.Button btnCompile;
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.Button btnOpenOutput;
        private System.Windows.Forms.Label lblConsole;
        private System.Windows.Forms.Label lblConsoleHint;
        private System.Windows.Forms.Panel pnlConsoleSurface;
        private System.Windows.Forms.RichTextBox rtbLog;
        private System.Windows.Forms.Panel pnlFooterLine;
        private System.Windows.Forms.Label lblStatusIndicator;
        private System.Windows.Forms.Label lblStatus;
        private System.Windows.Forms.Label lblVersion;
    }
}
