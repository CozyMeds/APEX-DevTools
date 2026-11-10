using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace APEX.Compiler.GUI
{
    internal static class NativeWindowTheme
    {
        private static Icon brandIcon;
        // Keep standard Windows resizing, snapping, accessibility and system-menu behavior.
        internal static void Apply(Form form)
        {
            if (Environment.OSVersion.Platform != PlatformID.Win32NT) return;
            try
            {
                int enabled = 1;
                if (DwmSetWindowAttribute(form.Handle, 20, ref enabled, sizeof(int)) != 0)
                    DwmSetWindowAttribute(form.Handle, 19, ref enabled, sizeof(int));
                int caption = ColorTranslator.ToWin32(Theme.Background);
                DwmSetWindowAttribute(form.Handle, 35, ref caption, sizeof(int));
            }
            catch (DllNotFoundException) { }
            catch (EntryPointNotFoundException) { }
            if (brandIcon != null) { form.Icon = brandIcon; return; }
            using (var bitmap = new Bitmap(32, 32))
            {
                using (var graphics = Graphics.FromImage(bitmap))
                {
                    graphics.Clear(Color.Transparent);
                    GlyphPainter.Draw(graphics, Glyph.Apex, new Rectangle(2, 2, 28, 28), Theme.Cyan);
                }
                IntPtr handle = bitmap.GetHicon();
                try { using (var icon = Icon.FromHandle(handle)) brandIcon = (Icon)icon.Clone(); form.Icon = brandIcon; }
                finally { DestroyIcon(handle); }
            }
        }

        [DllImport("dwmapi.dll", PreserveSig = true)]
        private static extern int DwmSetWindowAttribute(IntPtr window, int attribute, ref int value, int size);
        [DllImport("user32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool DestroyIcon(IntPtr icon);
    }
}
