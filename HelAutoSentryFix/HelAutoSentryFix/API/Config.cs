using BepInEx;
using BepInEx.Configuration;

namespace HelSentryFix {
    internal static partial class ConfigManager {
        static ConfigManager() {
            string text = Path.Combine(Paths.ConfigPath, $"{Module.Name}.cfg");
            ConfigFile configFile = new ConfigFile(text, true);

            debug = configFile.Bind(
                "Debug",
                "enable",
                false,
                "Enables debug messages when true.");

            sniperSentry_persistentID = configFile.Bind(
                "Sniper Sentry",
                "persistentID",
                54,
                "PersistentID of sniper sentry item");
            sniperSentry_forceNoPentration = configFile.Bind(
                "Sniper Sentry",
                "forceNoPentration",
                true,
                "Ignores the penetration on datablock and forces it to not penetrate.");
        }

        public static bool Debug {
            get { return debug.Value; }
            set { debug.Value = value; }
        }
        private static ConfigEntry<bool> debug;

        public static bool SniperSentry_forceNoPentration {
            get { return sniperSentry_forceNoPentration.Value; }
            set { sniperSentry_forceNoPentration.Value = value; }
        }
        private static ConfigEntry<bool> sniperSentry_forceNoPentration;

        public static int SniperSentry_persistentID {
            get { return sniperSentry_persistentID.Value; }
            set { sniperSentry_persistentID.Value = value; }
        }
        private static ConfigEntry<int> sniperSentry_persistentID;
    }
}