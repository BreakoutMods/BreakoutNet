using System;

namespace BreakoutMods.BreakoutNet
{
    public static class BreakoutSettingsSync
    {
        public static void RegisterServerSettings<TSettings>(string settingsName, Func<TSettings> getSettings)
            where TSettings : IBreakoutSerializable, new()
        {
            BreakoutSettingsSyncRegistry.RegisterServerSettings(settingsName, getSettings);
        }

        public static void BroadcastNow()
        {
            BreakoutSettingsSyncRegistry.BroadcastServerSettings();
        }

        public static class Client
        {
            public static void Register<TSettings>(string settingsName, Action<TSettings> applySettings)
                where TSettings : IBreakoutSerializable, new()
            {
                BreakoutSettingsSyncRegistry.RegisterClientSettings(settingsName, applySettings);
            }
        }
    }
}
