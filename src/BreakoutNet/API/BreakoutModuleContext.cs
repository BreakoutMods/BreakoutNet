using System;
using BepInEx;

namespace BreakoutMods.BreakoutNet
{
    public sealed class BreakoutModuleContext
    {
        internal BreakoutModuleContext(string modGuid, BreakoutModApp owner)
        {
            if (string.IsNullOrWhiteSpace(modGuid))
            {
                throw new ArgumentException("Mod GUID cannot be empty.", nameof(modGuid));
            }

            ModGuid = modGuid;
            Owner = owner;
            Events = new BreakoutEvents(modGuid, owner);
            Hooks = new BreakoutHooks(modGuid, owner);
        }

        public string ModGuid { get; }

        public BreakoutEvents Events { get; }

        public BreakoutHooks Hooks { get; }

        internal BreakoutModApp Owner { get; }
    }
}
