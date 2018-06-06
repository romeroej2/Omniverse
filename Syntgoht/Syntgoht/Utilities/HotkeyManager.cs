using Syntgoht.Models.Hotkeys;

namespace Syntgoht.Utilities
{
    internal class HotkeyManager
    {
        public static void RegisterHotkeys()
        {
            SyntgohtHotkeysModel.Instance.RegisterAll();
        }

        public static void UnregisterAllHotkeys()
        {
            SyntgohtHotkeysModel.UnregisterAll();
        }
    }
}