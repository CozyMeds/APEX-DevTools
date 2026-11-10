using System;
using System.Collections.Generic;

namespace TreyarchCompiler.Games
{
    internal static class BuiltinSignatures
    {
        private static readonly Dictionary<string, (int Min, int Max, bool Caller, bool Returns)> Signatures =
            new Dictionary<string, (int, int, bool, bool)>(StringComparer.Ordinal)
            {
                ["realwait"] = (1, 1, false, false),
                ["waitrealtime"] = (1, 1, false, false),
                ["isdefined"] = (1, 1, false, true),
                ["vectorscale"] = (2, 2, false, true),
                ["gettime"] = (0, 0, false, true),
                ["firstarraykey"] = (1, 1, false, true),
                ["getfirstarraykey"] = (1, 1, false, true),
                ["nextarraykey"] = (2, 2, false, true),
                ["getnextarraykey"] = (2, 2, false, true),
                ["notify"] = (1, 255, true, false),
                ["endon"] = (1, 1, true, false),
                ["waittill"] = (1, 255, true, false),
                ["waittillmatch"] = (1, 255, true, false)
            };

        internal static bool Validate(string name, int arguments, bool hasCaller)
        {
            var signature = Signatures[name];
            if (signature.Caller != hasCaller)
                throw new ArgumentException("Builtin '" + name + "' " +
                    (signature.Caller ? "requires an object caller." : "cannot have an object caller."));
            if (arguments < signature.Min || arguments > signature.Max)
                throw new ArgumentException("Builtin '" + name + "' expects " +
                    (signature.Min == signature.Max ? signature.Min.ToString() : signature.Min + ".." + signature.Max) +
                    " argument(s), received " + arguments + ".");
            return signature.Returns;
        }
    }
}
