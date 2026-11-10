using System;
using System.Drawing;
using System.Windows.Forms;

namespace APEX.Compiler.GUI
{
    public partial class MainForm
    {
        private System.ComponentModel.IContainer components;
        private ToolTip pathToolTip;
        private TableLayoutPanel workspace;
        private CardPanel configurationCard, outputCard, summaryCard, artifactCard;
        private TextBox txtCompiler, txtSource, txtOutput;
        private ActionButton btnBrowseCompiler, btnBrowseSource, btnBrowseOutput, btnCompile, btnCheck, btnOpenOutput, btnCancel;
        private ActionButton btnFilterAll, btnFilterWarnings, btnFilterErrors, btnClearLog, btnShowArtifact;
        private Label lblCompilerBadge, lblSummaryTitle, lblSummaryDetails, lblArtifactName, lblArtifactSize, lblStatus, lblVersion;
        private IconView compilerIndicator, summaryIcon, statusIndicator;
        private RichTextBox rtbLog;

        protected override void Dispose(bool disposing)
        {
            if (disposing) components?.Dispose();
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            components = new System.ComponentModel.Container();
            pathToolTip = new ToolTip(components) { AutoPopDelay = 18000, InitialDelay = 350, ReshowDelay = 100, ShowAlways = true };
            SuspendLayout();
            AutoScaleDimensions = new SizeF(96F, 96F);
            AutoScaleMode = AutoScaleMode.Dpi;
            Font = Theme.Body;
            BackColor = Theme.Background;
            ForeColor = Theme.Text;
            ClientSize = new Size(1280, 820);
            MinimumSize = new Size(1000, 720);
            StartPosition = FormStartPosition.CenterScreen;
            Text = "APEX DevTools";
            Name = "MainForm";
            KeyPreview = true;
            DoubleBuffered = true;

            var root = Grid(1, 3);
            root.Name = "applicationLayout";
            root.BackColor = Theme.Background;
            root.Padding = new Padding(24, 12, 24, 0);
            root.RowStyles.Add(new RowStyle(SizeType.Absolute, 108));
            root.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            root.RowStyles.Add(new RowStyle(SizeType.Absolute, 46));
            root.Controls.Add(CreateHeader(), 0, 0);

            workspace = Grid(2, 1);
            workspace.Name = "workspace";
            workspace.BackColor = Theme.Background;
            workspace.Margin = new Padding(0, 12, 0, 12);
            workspace.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 37));
            workspace.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 63));
            configurationCard = CreateConfiguration();
            configurationCard.Margin = new Padding(0, 0, 6, 0);
            outputCard = CreateOutput();
            outputCard.Margin = new Padding(6, 0, 0, 0);
            workspace.Controls.Add(configurationCard, 0, 0);
            workspace.Controls.Add(outputCard, 1, 0);
            root.Controls.Add(workspace, 0, 1);
            root.Controls.Add(CreateFooter(), 0, 2);
            Controls.Add(root);
            AcceptButton = btnCompile;
            ResumeLayout(true);
        }

        private Control CreateHeader()
        {
            var header = Grid(2, 1);
            header.BackColor = Theme.Background;
            header.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            header.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 210));
            header.Paint += (sender, args) => { using (var pen = new Pen(Theme.Divider)) args.Graphics.DrawLine(pen, 0, header.Height - 1, header.Width, header.Height - 1); };
            var title = new Panel { Dock = DockStyle.Fill, Margin = new Padding(0), BackColor = Theme.Background, Padding = new Padding(20, 0, 0, 12) };
            var accent = new Panel { BackColor = Theme.Cyan, Location = new Point(0, 14), Size = new Size(4, 62) };
            var titleLines = Grid(1, 2); titleLines.BackColor = Theme.Background;
            titleLines.RowStyles.Add(new RowStyle(SizeType.Absolute, 56));
            titleLines.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            titleLines.Controls.Add(Caption("Script Compiler", Theme.Title), 0, 0);
            titleLines.Controls.Add(Caption("Black Ops III / PlayStation 4", new Font("Segoe UI", 11F), Theme.Muted), 0, 1);
            title.Controls.Add(titleLines); title.Controls.Add(accent);
            var badges = Grid(2, 1);
            badges.Padding = new Padding(0, 27, 0, 40);
            badges.BackColor = Theme.Background;
            badges.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 57));
            badges.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 43));
            var target = NewCard("targetBadge", Theme.Background, new Padding(4));
            target.BorderColor = Theme.Cyan;
            target.Margin = new Padding(0, 0, 10, 0);
            target.Controls.Add(Caption("BO3 1.33", Theme.Label, Theme.Cyan, ContentAlignment.MiddleCenter));
            var version = NewCard("versionBadge", Theme.Surface, new Padding(4));
            lblVersion = Caption("", Theme.Label, Theme.Muted, ContentAlignment.MiddleCenter);
            lblVersion.Name = "lblVersion";
            version.Controls.Add(lblVersion);
            badges.Controls.Add(target, 0, 0); badges.Controls.Add(version, 1, 0);
            header.Controls.Add(title, 0, 0); header.Controls.Add(badges, 1, 0);
            return header;
        }

        private CardPanel CreateConfiguration()
        {
            var card = NewCard("configurationCard", Theme.Surface, new Padding(16));
            var layout = Grid(1, 5);
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 36));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 40));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 8));
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 152));
            layout.Controls.Add(SectionTitle("Build configuration", Glyph.Settings), 0, 0);
            var compilerState = NewCard("compilerState", Theme.Input, new Padding(10, 6, 10, 6));
            compilerState.BorderColor = Theme.Divider;
            compilerState.Margin = new Padding(0, 4, 0, 0);
            var stateLayout = Grid(2, 1); stateLayout.BackColor = Theme.Input;
            stateLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 26)); stateLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            compilerIndicator = new IconView { Glyph = Glyph.Dot, ForeColor = Theme.Amber, Dock = DockStyle.Fill, Margin = new Padding(0, 2, 6, 2), AccessibleName = "Compiler status" };
            lblCompilerBadge = Caption("Select a compiler", Theme.Body); lblCompilerBadge.Name = "lblCompilerBadge";
            stateLayout.Controls.Add(compilerIndicator, 0, 0); stateLayout.Controls.Add(lblCompilerBadge, 1, 0); compilerState.Controls.Add(stateLayout);
            layout.Controls.Add(compilerState, 0, 1);

            txtCompiler = PathText("txtCompiler", "Compiler executable");
            txtSource = PathText("txtSource", "Source directory");
            txtOutput = PathText("txtOutput", "Output file");
            btnBrowseCompiler = Button("btnBrowseCompiler", "", Glyph.Folder, btnBrowseCompiler_Click, "Browse for compiler executable");
            btnBrowseSource = Button("btnBrowseSource", "", Glyph.Folder, btnBrowseSource_Click, "Browse for source directory");
            btnBrowseOutput = Button("btnBrowseOutput", "", Glyph.Folder, btnBrowseOutput_Click, "Choose GSCC output file");
            var sourceViewport = new Panel { Name = "sourceViewport", Dock = DockStyle.Fill, AutoScroll = false, BackColor = Theme.Surface, Margin = new Padding(0), AutoScrollMinSize = Size.Empty };
            var fields = Grid(1, 3); fields.Dock = DockStyle.Top; fields.Height = 314;
            fields.RowStyles.Add(new RowStyle(SizeType.Absolute, 94));
            fields.RowStyles.Add(new RowStyle(SizeType.Absolute, 110));
            fields.RowStyles.Add(new RowStyle(SizeType.Absolute, 110));
            fields.Controls.Add(PathField("Compiler executable", Glyph.Terminal, txtCompiler, btnBrowseCompiler, "", 0), 0, 0);
            fields.Controls.Add(PathField("Source directory", Glyph.Folder, txtSource, btnBrowseSource, "All .gsc files in this folder", 1), 0, 1);
            fields.Controls.Add(PathField("Output file", Glyph.File, txtOutput, btnBrowseOutput, "Previous output is preserved if a build fails.", 2), 0, 2);
            sourceViewport.Controls.Add(fields);
            sourceViewport.SizeChanged += (sender, args) =>
            {
                float scale = sourceViewport.DeviceDpi / 96F;
                bool compact = sourceViewport.ClientSize.Height < 314 * scale;
                fields.Height = (int)((compact ? 210 : 314) * scale);
                fields.RowStyles[0].Height = (compact ? 70 : 94) * scale;
                fields.RowStyles[1].Height = (compact ? 70 : 110) * scale;
                fields.RowStyles[2].Height = (compact ? 70 : 110) * scale;
            };
            layout.Controls.Add(sourceViewport, 0, 3);
            var actions = Grid(1, 3);
            actions.RowStyles.Add(new RowStyle(SizeType.Absolute, 64));
            actions.RowStyles.Add(new RowStyle(SizeType.Absolute, 46));
            actions.RowStyles.Add(new RowStyle(SizeType.Absolute, 42));

            btnCompile = Button("btnCompile", "Compile GSCC", Glyph.Play, btnCompile_Click, "Compile GSCC (F5)");
            btnCompile.Primary = true; btnCompile.Font = new Font("Segoe UI Semibold", 12F); btnCompile.TabIndex = 3;
            btnCompile.Margin = new Padding(0, 4, 0, 6);
            actions.Controls.Add(btnCompile, 0, 0);
            var secondary = Grid(2, 1); secondary.TabIndex = 4;
            secondary.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50)); secondary.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
            btnCheck = Button("btnCheck", "Check only", Glyph.Check, btnCheck_Click, "Compile and validate without writing output (Shift+F5)");
            btnCheck.Margin = new Padding(0, 2, 4, 6);
            btnOpenOutput = Button("btnOpenOutput", "Open output", Glyph.Folder, btnOpenOutput_Click, "Show the output in File Explorer");
            btnOpenOutput.Margin = new Padding(4, 2, 0, 6); btnOpenOutput.TabIndex = 1;
            secondary.Controls.Add(btnCheck, 0, 0); secondary.Controls.Add(btnOpenOutput, 1, 0);
            actions.Controls.Add(secondary, 0, 1);
            btnCancel = Button("btnCancel", "Cancel build", Glyph.Cancel, btnCancel_Click, "Cancel the current build (Esc)");
            btnCancel.TabIndex = 5; btnCancel.Enabled = false; btnCancel.ForeColor = Theme.Muted; btnCancel.Margin = new Padding(0, 2, 0, 0);
            actions.Controls.Add(btnCancel, 0, 2);
            layout.Controls.Add(actions, 0, 4);
            card.Controls.Add(layout);
            return card;
        }

        private CardPanel CreateOutput()
        {
            var card = NewCard("outputCard", Theme.Surface, new Padding(16));
            var layout = Grid(1, 4);
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 42));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 92));
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 78));
            var toolbar = Grid(2, 1);
            toolbar.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100)); toolbar.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 260));
            toolbar.Controls.Add(SectionTitle("Build output", Glyph.Terminal), 0, 0);
            var filters = Grid(4, 1);
            foreach (int width in new[] { 52, 82, 68, 42 }) filters.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, width));
            btnFilterAll = Button("btnFilterAll", "All", Glyph.None, (s, e) => SelectFilter(LogFilter.All), "Show all build messages");
            btnFilterWarnings = Button("btnFilterWarnings", "Warnings", Glyph.None, (s, e) => SelectFilter(LogFilter.Warnings), "Show warning messages");
            btnFilterErrors = Button("btnFilterErrors", "Errors", Glyph.None, (s, e) => SelectFilter(LogFilter.Errors), "Show error messages");
            btnClearLog = Button("btnClearLog", "", Glyph.Trash, (s, e) => ClearLog(), "Clear build output (Ctrl+L)");
            int column = 0;
            foreach (var filter in new[] { btnFilterAll, btnFilterWarnings, btnFilterErrors, btnClearLog })
            {
                filter.Font = Theme.Small; filter.Margin = new Padding(0, 0, 3, 6); filter.TabIndex = column;
                filters.Controls.Add(filter, column++, 0);
            }
            btnClearLog.Margin = new Padding(7, 0, 0, 6);
            toolbar.Controls.Add(filters, 1, 0); layout.Controls.Add(toolbar, 0, 0);
            summaryCard = NewCard("summaryCard", Color.FromArgb(15, 45, 57), new Padding(16, 10, 12, 10));
            summaryCard.BorderColor = Color.FromArgb(30, 115, 137); summaryCard.Margin = new Padding(0, 4, 0, 10);
            var summary = Grid(2, 1); summary.BackColor = summaryCard.BackColor;
            summary.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 48)); summary.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            summaryIcon = new IconView { Name = "summaryIcon", Glyph = Glyph.Terminal, Dock = DockStyle.Fill, Margin = new Padding(0, 9, 12, 9), ForeColor = Theme.Cyan };
            var lines = Grid(1, 2); lines.BackColor = summaryCard.BackColor;
            lines.RowStyles.Add(new RowStyle(SizeType.Percent, 55)); lines.RowStyles.Add(new RowStyle(SizeType.Percent, 45));
            lblSummaryTitle = Caption("Ready to compile", Theme.Heading, Theme.Cyan); lblSummaryTitle.Name = "lblSummaryTitle";
            lblSummaryDetails = Caption("Choose a source directory to begin.", Theme.Body, Theme.Muted); lblSummaryDetails.Name = "lblSummaryDetails";
            lines.Controls.Add(lblSummaryTitle, 0, 0); lines.Controls.Add(lblSummaryDetails, 0, 1);
            summary.Controls.Add(summaryIcon, 0, 0); summary.Controls.Add(lines, 1, 0); summaryCard.Controls.Add(summary);
            layout.Controls.Add(summaryCard, 0, 1);

            var consoleCard = NewCard("consoleCard", Theme.Console, new Padding(12));
            consoleCard.Margin = new Padding(0, 0, 0, 10);
            rtbLog = new RichTextBox
            {
                Name = "rtbLog", Dock = DockStyle.Fill, BorderStyle = BorderStyle.None, ReadOnly = true, DetectUrls = false,
                WordWrap = true, ScrollBars = RichTextBoxScrollBars.Vertical, BackColor = Theme.Console, ForeColor = Theme.Text,
                Font = Theme.Mono, AccessibleName = "Build output log", AccessibleDescription = "Timestamped compiler messages. Use the filters to show warnings or errors."
            };
            consoleCard.Controls.Add(rtbLog); layout.Controls.Add(consoleCard, 0, 2);
            artifactCard = NewCard("artifactCard", Theme.Input, new Padding(14, 10, 10, 10));
            artifactCard.BorderColor = Theme.Divider;
            var artifact = Grid(3, 1); artifact.BackColor = Theme.Input;
            artifact.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 40)); artifact.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100)); artifact.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 166));
            artifact.Controls.Add(new IconView { Glyph = Glyph.File, ForeColor = Theme.Text, Dock = DockStyle.Fill, Margin = new Padding(0, 7, 14, 7) }, 0, 0);
            var metadata = Grid(1, 2); metadata.BackColor = Theme.Input;
            metadata.RowStyles.Add(new RowStyle(SizeType.Percent, 52)); metadata.RowStyles.Add(new RowStyle(SizeType.Percent, 48));
            lblArtifactName = Caption("No output yet", Theme.Label); lblArtifactName.Name = "lblArtifactName";
            lblArtifactSize = Caption("Compile a script to create a GSCC file.", Theme.Small, Theme.Muted); lblArtifactSize.Name = "lblArtifactSize";
            metadata.Controls.Add(lblArtifactName, 0, 0); metadata.Controls.Add(lblArtifactSize, 0, 1); artifact.Controls.Add(metadata, 1, 0);
            btnShowArtifact = Button("btnShowArtifact", "Show in Explorer", Glyph.Folder, btnOpenOutput_Click, "Show the compiled output in File Explorer");
            btnShowArtifact.ForeColor = Theme.Cyan; btnShowArtifact.Font = Theme.Small; btnShowArtifact.BackColor = Theme.Input; btnShowArtifact.Margin = new Padding(8, 7, 0, 7);
            artifact.Controls.Add(btnShowArtifact, 2, 0); artifactCard.Controls.Add(artifact); layout.Controls.Add(artifactCard, 0, 3);
            card.Controls.Add(layout);
            return card;
        }

        private Control CreateFooter()
        {
            var footer = Grid(3, 1); footer.BackColor = Theme.Background;
            footer.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 22)); footer.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100)); footer.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 132));
            footer.Padding = new Padding(0, 10, 0, 8);
            footer.Paint += (s, e) => { using (var pen = new Pen(Theme.Divider)) e.Graphics.DrawLine(pen, 0, 0, footer.Width, 0); };
            statusIndicator = new IconView { Glyph = Glyph.Dot, ForeColor = Theme.Green, Dock = DockStyle.Fill, Margin = new Padding(0, 3, 6, 3) };
            lblStatus = Caption("Ready", Theme.Body, Theme.Muted); lblStatus.Name = "lblStatus";
            footer.Controls.Add(statusIndicator, 0, 0); footer.Controls.Add(lblStatus, 1, 0);
            footer.Controls.Add(Caption("PS4 BO3 1.33", Theme.Small, Theme.Muted, ContentAlignment.MiddleRight), 2, 0);
            return footer;
        }

        private Control PathField(string label, Glyph glyph, TextBox text, ActionButton browse, string helper, int tabIndex)
        {
            var group = Grid(1, 3); group.TabIndex = tabIndex;
            group.RowStyles.Add(new RowStyle(SizeType.Absolute, 28)); group.RowStyles.Add(new RowStyle(SizeType.Absolute, 42)); group.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            var caption = SectionTitle(label, glyph, Theme.Label);
            group.Controls.Add(caption, 0, 0);
            var field = NewCard(label + "Field", Theme.Input, new Padding(10, 4, 4, 4));
            var inner = Grid(2, 1); inner.BackColor = Theme.Input;
            inner.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100)); inner.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 38));
            browse.BackColor = Theme.Input;
            inner.Controls.Add(text, 0, 0); inner.Controls.Add(browse, 1, 0); field.Controls.Add(inner); group.Controls.Add(field, 0, 1);
            var hint = Caption(helper, Theme.Small, Theme.Muted); hint.AutoEllipsis = false; hint.TextAlign = ContentAlignment.TopLeft; hint.Padding = new Padding(0, 5, 0, 0);
            group.Controls.Add(hint, 0, 2);
            return group;
        }

        private TextBox PathText(string name, string accessibleName) => new TextBox
        {
            Name = name, Anchor = AnchorStyles.Left | AnchorStyles.Right, BorderStyle = BorderStyle.None, ReadOnly = true,
            BackColor = Theme.Input, ForeColor = Theme.Text, Font = Theme.Mono, Margin = new Padding(0, 0, 6, 0),
            TabStop = false, AccessibleName = accessibleName
        };
        private ActionButton Button(string name, string text, Glyph glyph, EventHandler click, string description)
        {
            var button = new ActionButton { Name = name, Text = text, Glyph = glyph, Dock = DockStyle.Fill, Margin = new Padding(0), AccessibleName = string.IsNullOrEmpty(text) ? description : text, AccessibleDescription = description };
            button.Click += click;
            pathToolTip.SetToolTip(button, description);
            return button;
        }
        private static Label Caption(string text, Font font, Color? color = null, ContentAlignment alignment = ContentAlignment.MiddleLeft) => new Label
        {
            Text = text, Font = font, ForeColor = color ?? Theme.Text, BackColor = Color.Transparent, Dock = DockStyle.Fill,
            AutoEllipsis = true, TextAlign = alignment, Margin = new Padding(0), UseMnemonic = false
        };
        private static Control SectionTitle(string text, Glyph glyph, Font font = null)
        {
            var title = Grid(2, 1);
            title.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 32)); title.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            title.Controls.Add(new IconView { Glyph = glyph, ForeColor = Theme.Text, Dock = DockStyle.Fill, Margin = new Padding(0, 6, 10, 6) }, 0, 0);
            title.Controls.Add(Caption(text, font ?? Theme.Heading), 1, 0);
            return title;
        }
        private static CardPanel NewCard(string name, Color color, Padding padding) => new CardPanel { Name = name, BackColor = color, Dock = DockStyle.Fill, Padding = padding, Margin = new Padding(0) };
        private static TableLayoutPanel Grid(int columns, int rows)
        {
            var grid = new TableLayoutPanel
            {
                ColumnCount = columns, RowCount = rows, Dock = DockStyle.Fill, Margin = new Padding(0), Padding = new Padding(0), BackColor = Theme.Surface,
                GrowStyle = TableLayoutPanelGrowStyle.FixedSize
            };
            // Single-row/column containers must fill the cell instead of measuring
            // a nested Panel's default preferred size and clipping their children.
            if (columns == 1) grid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            if (rows == 1) grid.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            return grid;
        }
    }
}
