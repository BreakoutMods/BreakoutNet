using System;
using BepInEx;

namespace BreakoutMods.BreakoutNet
{
    public static class BreakoutNet
    {
        public static BreakoutModuleContext ForMod(string modGuid)
        {
            return new BreakoutModuleContext(modGuid, null);
        }

        public static BreakoutModBuilder ForPlugin(BaseUnityPlugin plugin, string modGuid)
        {
            if (plugin == null)
            {
                throw new ArgumentNullException(nameof(plugin));
            }

            return new BreakoutModBuilder(plugin, modGuid);
        }
    }
}
