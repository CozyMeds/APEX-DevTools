using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace APEX.Compiler.GUI
{
    internal static class Theme
    {
        internal static readonly Color Background = Color.FromArgb(14, 21, 31);
        internal static readonly Color Surface = Color.FromArgb(23, 32, 44);
        internal static readonly Color Input = Color.FromArgb(27, 39, 53);
        internal static readonly Color Console = Color.FromArgb(9, 16, 26);
        internal static readonly Color Border = Color.FromArgb(65, 84, 104);
        internal static readonly Color Divider = Color.FromArgb(42, 57, 73);
        internal static readonly Color Text = Color.FromArgb(233, 240, 247);
        internal static readonly Color Muted = Color.FromArgb(158, 175, 193);
        internal static readonly Color Cyan = Color.FromArgb(0, 208, 231);
        internal static readonly Color Green = Color.FromArgb(110, 232, 153);
        internal static readonly Color Amber = Color.FromArgb(241, 196, 105);
        internal static readonly Color Red = Color.FromArgb(255, 125, 136);
        internal static readonly Font Body = new Font("Segoe UI", 10F);
        internal static readonly Font Small = new Font("Segoe UI", 9F);
        internal static readonly Font Label = new Font("Segoe UI Semibold", 10F);
        internal static readonly Font Heading = new Font("Segoe UI Semibold", 14F);
        internal static readonly Font Title = new Font("Segoe UI Semibold", 27F);
        internal static readonly Font Mono = new Font("Consolas", 10F);

        internal static GraphicsPath Rounded(RectangleF bounds, float radius)
        {
            float d = Math.Min(radius * 2, Math.Min(bounds.Width, bounds.Height));
            var path = new GraphicsPath();
            if (d <= 0) { path.AddRectangle(bounds); return path; }
            path.AddArc(bounds.Left, bounds.Top, d, d, 180, 90);
            path.AddArc(bounds.Right - d, bounds.Top, d, d, 270, 90);
            path.AddArc(bounds.Right - d, bounds.Bottom - d, d, d, 0, 90);
            path.AddArc(bounds.Left, bounds.Bottom - d, d, d, 90, 90);
            path.CloseFigure();
            return path;
        }
    }

    internal enum Glyph { None, Apex, Terminal, Folder, File, Play, Check, Cancel, Trash, Settings, Success, Error, Clock, Dot }

    internal static class GlyphPainter
    {
        internal static void Draw(Graphics graphics, Glyph glyph, Rectangle bounds, Color color)
        {
            if (glyph == Glyph.None || bounds.Width <= 0 || bounds.Height <= 0) return;
            int side = Math.Min(bounds.Width, bounds.Height);
            bounds = new Rectangle(bounds.Left + (bounds.Width - side) / 2, bounds.Top + (bounds.Height - side) / 2, side, side);
            var state = graphics.Save();
            graphics.SmoothingMode = SmoothingMode.AntiAlias;
            graphics.TranslateTransform(bounds.Left, bounds.Top);
            graphics.ScaleTransform(bounds.Width / 24F, bounds.Height / 24F);
            using (var pen = new Pen(color, 1.65F) { StartCap = LineCap.Round, EndCap = LineCap.Round, LineJoin = LineJoin.Round })
            using (var brush = new SolidBrush(color))
            {
                switch (glyph)
                {
                    case Glyph.Apex:
                        graphics.FillPolygon(brush, new[] { new PointF(2, 22), new PointF(10.8F, 2), new PointF(14.4F, 7), new PointF(7, 22) });
                        graphics.FillPolygon(brush, new[] { new PointF(13, 8), new PointF(22, 22), new PointF(17, 22), new PointF(11, 13) });
                        break;
                    case Glyph.Terminal:
                        graphics.DrawLines(pen, new[] { new Point(4, 6), new Point(10, 12), new Point(4, 18) });
                        graphics.DrawLine(pen, 13, 18, 21, 18); break;
                    case Glyph.Folder:
                        graphics.DrawPolygon(pen, new[] { new Point(2, 7), new Point(2, 4), new Point(9, 4), new Point(12, 7), new Point(22, 7), new Point(22, 20), new Point(2, 20) }); break;
                    case Glyph.File:
                        graphics.DrawPolygon(pen, new[] { new Point(5, 2), new Point(14, 2), new Point(20, 8), new Point(20, 22), new Point(5, 22) });
                        graphics.DrawLines(pen, new[] { new Point(14, 2), new Point(14, 8), new Point(20, 8) }); break;
                    case Glyph.Play:
                        graphics.FillPolygon(brush, new[] { new Point(7, 3), new Point(21, 12), new Point(7, 21) }); break;
                    case Glyph.Success:
                    case Glyph.Cancel:
                    case Glyph.Error:
                    case Glyph.Clock:
                        graphics.DrawEllipse(pen, 2, 2, 20, 20);
                        if (glyph == Glyph.Success) graphics.DrawLines(pen, new[] { new Point(6, 12), new Point(10, 16), new Point(18, 8) });
                        else if (glyph == Glyph.Cancel) graphics.DrawLine(pen, 5, 5, 19, 19);
                        else if (glyph == Glyph.Error) { graphics.DrawLine(pen, 8, 8, 16, 16); graphics.DrawLine(pen, 8, 16, 16, 8); }
                        else { graphics.DrawLine(pen, 12, 6, 12, 12); graphics.DrawLine(pen, 12, 12, 16, 14); }
                        break;
                    case Glyph.Check:
                        graphics.DrawLines(pen, new[] { new Point(3, 12), new Point(9, 18), new Point(21, 5) }); break;
                    case Glyph.Trash:
                        graphics.DrawLine(pen, 3, 6, 21, 6);
                        graphics.DrawLines(pen, new[] { new Point(6, 6), new Point(7, 22), new Point(17, 22), new Point(18, 6) });
                        graphics.DrawLines(pen, new[] { new Point(9, 6), new Point(9, 2), new Point(15, 2), new Point(15, 6) });
                        graphics.DrawLine(pen, 10, 10, 10, 18); graphics.DrawLine(pen, 14, 10, 14, 18); break;
                    case Glyph.Settings:
                        var points = new PointF[32];
                        for (int i = 0; i < points.Length; i++)
                        {
                            double angle = i * Math.PI / 16;
                            double radius = i % 4 == 0 || i % 4 == 3 ? 10.5 : 8.5;
                            points[i] = new PointF(12 + (float)(Math.Cos(angle) * radius), 12 + (float)(Math.Sin(angle) * radius));
                        }
                        graphics.DrawPolygon(pen, points); graphics.DrawEllipse(pen, 8, 8, 8, 8); break;
                    case Glyph.Dot:
                        graphics.FillEllipse(brush, 5, 5, 14, 14); break;
                }
            }
            graphics.Restore(state);
        }
    }

    internal sealed class IconView : Control
    {
        private Glyph glyph;
        internal Glyph Glyph { get => glyph; set { glyph = value; Invalidate(); } }
        internal IconView()
        {
            SetStyle(ControlStyles.SupportsTransparentBackColor | ControlStyles.UserPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);
            BackColor = Color.Transparent;
            TabStop = false;
            AccessibleRole = AccessibleRole.Graphic;
        }
        protected override void OnPaint(PaintEventArgs e) => GlyphPainter.Draw(e.Graphics, Glyph, ClientRectangle, ForeColor);
    }

    internal sealed class CardPanel : Panel
    {
        private Color borderColor = Theme.Border;
        internal Color BorderColor { get => borderColor; set { borderColor = value; Invalidate(); } }
        internal CardPanel()
        {
            SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);
            BackColor = Theme.Surface;
        }
        protected override void OnPaintBackground(PaintEventArgs e)
        {
            e.Graphics.Clear(Parent?.BackColor ?? Theme.Background);
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            using (var path = Theme.Rounded(new RectangleF(.5F, .5F, Math.Max(0, Width - 1), Math.Max(0, Height - 1)), 8F * DeviceDpi / 96F))
            using (var fill = new SolidBrush(BackColor))
            using (var pen = new Pen(BorderColor))
            {
                e.Graphics.FillPath(fill, path);
                e.Graphics.DrawPath(pen, path);
            }
        }
    }

    internal sealed class ActionButton : Button
    {
        private bool hover, pressed, selected;
        internal bool Primary { get; set; }
        internal Glyph Glyph { get; set; }
        internal bool Selected { get => selected; set { selected = value; Invalidate(); } }
        internal ActionButton()
        {
            SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);
            FlatStyle = FlatStyle.Flat;
            FlatAppearance.BorderSize = 0;
            BackColor = Theme.Surface;
            ForeColor = Theme.Text;
            Font = Theme.Label;
            Cursor = Cursors.Hand;
            UseVisualStyleBackColor = false;
        }
        protected override void OnMouseEnter(EventArgs e) { hover = true; Invalidate(); base.OnMouseEnter(e); }
        protected override void OnMouseLeave(EventArgs e) { hover = false; pressed = false; Invalidate(); base.OnMouseLeave(e); }
        protected override void OnMouseDown(MouseEventArgs e) { pressed = true; Invalidate(); base.OnMouseDown(e); }
        protected override void OnMouseUp(MouseEventArgs e) { pressed = false; Invalidate(); base.OnMouseUp(e); }
        protected override void OnEnabledChanged(EventArgs e) { Invalidate(); base.OnEnabledChanged(e); }
        protected override void OnGotFocus(EventArgs e) { Invalidate(); base.OnGotFocus(e); }
        protected override void OnLostFocus(EventArgs e) { Invalidate(); base.OnLostFocus(e); }
        protected override void OnPaint(PaintEventArgs e)
        {
            var background = Primary || Selected ? Theme.Cyan : hover ? Theme.Input : BackColor;
            var foreground = Primary || Selected ? Theme.Console : ForeColor;
            var border = Primary || Selected ? Theme.Cyan : Theme.Border;
            if (!Enabled) { background = Theme.Surface; foreground = Color.FromArgb(106, 125, 144); border = Theme.Divider; }
            else if (pressed) background = Color.FromArgb(Primary || Selected ? 0 : 35, Primary || Selected ? 175 : 49, Primary || Selected ? 199 : 64);
            e.Graphics.Clear(Parent?.BackColor ?? Theme.Surface);
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            using (var path = Theme.Rounded(new RectangleF(.5F, .5F, Math.Max(0, Width - 1), Math.Max(0, Height - 1)), 6F * DeviceDpi / 96F))
            using (var brush = new SolidBrush(background))
            using (var pen = new Pen(Focused && Enabled ? Theme.Cyan : border, Focused && Enabled ? 2 : 1))
            {
                e.Graphics.FillPath(brush, path);
                e.Graphics.DrawPath(pen, path);
            }
            var flags = TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.SingleLine | TextFormatFlags.EndEllipsis | TextFormatFlags.NoPrefix | TextFormatFlags.NoPadding;
            int icon = Glyph == Glyph.None ? 0 : (int)(18F * DeviceDpi / 96F);
            int gap = icon == 0 || string.IsNullOrEmpty(Text) ? 0 : (int)(8F * DeviceDpi / 96F);
            int textWidth = string.IsNullOrEmpty(Text) ? 0 : TextRenderer.MeasureText(Text, Font, Size.Empty, TextFormatFlags.NoPadding).Width;
            int availableWidth = Math.Max(0, Width - 16 - icon - gap);
            int combined = icon + gap + Math.Min(textWidth, availableWidth);
            int start = Math.Max(8, (Width - combined) / 2);
            GlyphPainter.Draw(e.Graphics, Glyph, new Rectangle(start, (Height - icon) / 2, icon, icon), foreground);
            if (!string.IsNullOrEmpty(Text)) TextRenderer.DrawText(e.Graphics, Text, Font,
                new Rectangle(start + icon + gap, 0, Math.Max(0, Width - start * 2 - icon - gap), Height), foreground, flags);
        }
    }
}
