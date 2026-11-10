using System;
using System.Collections.Generic;
using System.IO;

namespace ApexCompiler.Core
{
    // Structural validation is deliberately independent of the mutable opcode map.
    // It does not promise that a script will execute successfully in the game VM.
    public static class GsccValidator
    {
        private const ulong Magic = 0x1C000A0D43534780UL;

        public static void Validate(byte[] data)
        {
            if (data == null || data.Length < 0x50) Fail("Script header is incomplete.");
            using (var reader = new BinaryReader(new MemoryStream(data, false)))
            {
                if (reader.ReadUInt64() != Magic) Fail("Unexpected PS4 BO3 script magic.");
                uint includes = U32(reader, 0x0c), code = U32(reader, 0x14), strings = U32(reader, 0x18);
                uint exports = U32(reader, 0x20), imports = U32(reader, 0x24), size = U32(reader, 0x30);
                uint name = U32(reader, 0x34);
                ushort stringCount = U16(reader, 0x38), exportCount = U16(reader, 0x3a), importCount = U16(reader, 0x3c);
                reader.BaseStream.Position = 0x44;
                byte includeCount = reader.ReadByte();
                Range(data, code, size, "bytecode");
                Range(data, exports, (ulong)exportCount * 20, "exports");
                if (exports < 0x50 || (ulong)exports + (ulong)exportCount * 20 > code)
                    Fail("Export table overlaps the bytecode.");
                if ((ulong)code + size > imports || imports > includes || includes > strings || strings > data.Length)
                    Fail("Script sections overlap or are out of order.");
                if (name < 0x50 || name >= exports) Fail("Script name points outside the string data.");
                NullTerminated(data, name, exports);
                var starts = new HashSet<uint>();
                uint previous = 0;
                for (int i = 0; i < exportCount; i++)
                {
                    uint start = U32(reader, exports + (uint)i * 20 + 4);
                    if (start < code || start >= (ulong)code + size || !starts.Add(start) || (i > 0 && start <= previous))
                        Fail("Invalid or duplicate function bytecode offset.");
                    previous = start;
                }
                ulong position = imports;
                for (int i = 0; i < importCount; i++)
                {
                    Range(data, position, 12, "import entry");
                    ushort count = U16(reader, position + 8);
                    position += 12;
                    Range(data, position, (ulong)count * 4, "import references");
                    for (int j = 0; j < count; j++) Reference(data, U32(reader, position + (uint)j * 4), code, size, 2);
                    position += (ulong)count * 4;
                    if (position > includes) Fail("Import table overlaps the includes.");
                }
                Range(data, includes, (ulong)includeCount * 4, "include references");
                if ((ulong)includes + (ulong)includeCount * 4 > strings) Fail("Include table overlaps the string fixups.");
                for (int i = 0; i < includeCount; i++)
                {
                    uint pointer = U32(reader, includes + (uint)i * 4);
                    if (pointer < (ulong)includes + (ulong)includeCount * 4 || pointer >= strings)
                        Fail("Invalid include string offset.");
                    NullTerminated(data, pointer, strings);
                }
                position = strings;
                for (int i = 0; i < stringCount; i++)
                {
                    Range(data, position, 8, "string fixup");
                    uint pointer = U32(reader, position);
                    reader.BaseStream.Position = (long)position + 4;
                    byte count = reader.ReadByte();
                    if (count == 0 || count > 250 || pointer < name || pointer >= exports)
                        Fail("Invalid string fixup entry.");
                    NullTerminated(data, pointer, exports);
                    position += 8;
                    Range(data, position, (ulong)count * 4, "string references");
                    for (int j = 0; j < count; j++) Reference(data, U32(reader, position + (uint)j * 4), code, size, 4);
                    position += (ulong)count * 4;
                }
            }
        }

        private static void Reference(byte[] data, uint address, uint code, uint size, uint width)
        {
            Range(data, address, width, "bytecode reference");
            if (address < code || (ulong)address + width > (ulong)code + size)
                Fail("Reference points outside the bytecode.");
        }
        private static void Range(byte[] data, ulong offset, ulong count, string section)
        {
            if (offset > (ulong)data.Length || count > (ulong)data.Length - offset)
                Fail("Invalid " + section + " range.");
        }
        private static void NullTerminated(byte[] data, uint offset, uint end)
        {
            if (end > data.Length || offset >= end) Fail("Invalid string range.");
            for (uint i = offset; i < end; i++) if (data[i] == 0) return;
            Fail("Unterminated string.");
        }
        private static uint U32(BinaryReader reader, ulong offset) { reader.BaseStream.Position = (long)offset; return reader.ReadUInt32(); }
        private static ushort U16(BinaryReader reader, ulong offset) { reader.BaseStream.Position = (long)offset; return reader.ReadUInt16(); }
        private static void Fail(string message) { throw new InvalidDataException(message); }
    }
}
