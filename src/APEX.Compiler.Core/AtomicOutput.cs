using System;
using System.IO;

namespace ApexCompiler.Core
{
    public static class AtomicOutput
    {
        public static void WriteAllBytes(string destination, byte[] data)
        {
            WriteAllBytes(destination, data, (stream, bytes) => stream.Write(bytes, 0, bytes.Length));
        }

        internal static void WriteAllBytes(string destination, byte[] data, Action<FileStream, byte[]> write)
        {
            GsccValidator.Validate(data);
            string target = Path.GetFullPath(destination);
            string directory = Path.GetDirectoryName(target);
            Directory.CreateDirectory(directory);
            string temporary = Path.Combine(directory, ".apex-" + Guid.NewGuid().ToString("N") + ".tmp");
            try
            {
                using (var stream = new FileStream(temporary, FileMode.CreateNew, FileAccess.Write, FileShare.None))
                {
                    write(stream, data);
                    stream.Flush(true);
                }
                Promote(temporary, target);
            }
            finally
            {
                if (File.Exists(temporary)) File.Delete(temporary);
            }
        }

        public static void Promote(string temporary, string destination)
        {
            temporary = Path.GetFullPath(temporary);
            destination = Path.GetFullPath(destination);
            var comparison = Path.DirectorySeparatorChar == '\\' ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal;
            if (!string.Equals(Path.GetDirectoryName(temporary), Path.GetDirectoryName(destination), comparison) ||
                string.Equals(temporary, destination, comparison))
                throw new ArgumentException("Temporary output must be a separate file beside the destination.");
            GsccValidator.Validate(File.ReadAllBytes(temporary));
            if (File.Exists(destination)) File.Replace(temporary, destination, null);
            else File.Move(temporary, destination);
        }
    }
}
