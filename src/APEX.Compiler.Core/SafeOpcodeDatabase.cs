using System;
using System.IO;
using System.Text;
using T7CompilerLib;

internal static class SafeOpcodeDatabase
{
    // Fixed-size, data-only opcode map. No unsafe object deserialization or reflection-based object graphs.
    internal static T7MetaV2 Load(string path)
    {
        using (var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read))
        using (var reader = new BinaryReader(stream, Encoding.ASCII))
        {
            if (stream.Length != 8 + 4 + 4 + 8 + 4 + 0x4000)
                throw new InvalidDataException("Unexpected opcode map size.");
            var signature = Encoding.ASCII.GetString(reader.ReadBytes(8));
            if (signature != "APEXOPS1")
                throw new InvalidDataException("Invalid opcode map signature.");
            var iv = reader.ReadUInt32();
            var key = reader.ReadUInt32();
            var magic = reader.ReadUInt64();
            var count = reader.ReadUInt32();
            if (iv != 0x4B9ACE2F || key != 0x01000193 ||
                magic != 0x1C000A0D43534780UL || count != 0x4000)
                throw new InvalidDataException("Unexpected PS4 BO3 opcode metadata.");
            byte[] ops = reader.ReadBytes(0x4000);
            if (ops.Length != 0x4000 || stream.Position != stream.Length)
                throw new InvalidDataException("Incomplete opcode map.");
            return new T7MetaV2 { __iv = iv, __key = key, __magic = magic, __ops = ops };
        }
    }
}
