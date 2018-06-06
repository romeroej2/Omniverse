using Syntgoht.Utilities;
using ff14bot;
using ff14bot.Objects;
using System;
using System.ComponentModel;
using System.Configuration;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using HotkeyManager = ff14bot.Managers.HotkeyManager;

namespace Syntgoht.Models.Hotkeys
{
    public class SyntgohtHotkeysModel : BaseModel
    {
        private static LocalPlayer Me => Core.Player;
        private static SyntgohtHotkeysModel _instance;
        public static SyntgohtHotkeysModel Instance => _instance ?? (_instance = new SyntgohtHotkeysModel());

        private SyntgohtHotkeysModel() : base(@"Settings/" + Me.Name + "/Syntgoht/Syntgoht_Hotkeys.json")
        {
        }

        private Keys pause;

        private ModifierKeys pauseModifier;

        [Setting]
        [DefaultValue(Keys.None)]
        public Keys PauseKey
        { get { return pause; } set { pause = value; OnPropertyChanged(); } }

        [Setting]
        [DefaultValue(ModifierKeys.None)]
        public ModifierKeys PauseModifier
        { get { return pauseModifier; } set { pauseModifier = value; OnPropertyChanged(); } }

        public void RegisterAll()
        {
            HotkeyManager.Register("Syntgoht_Pause", PauseKey, PauseModifier, hk =>
            {
                MainSettingsModel.Instance.UsePause = !MainSettingsModel.Instance.UsePause;
                Core.OverlayManager.AddToast(() => MainSettingsModel.Instance.UsePause ? "Syntgoht Paused!" : "Syntgoht Resumed!", TimeSpan.FromMilliseconds(750), Color.FromRgb(110, 225, 214), Colors.White, new FontFamily("High Tower Text Italic"), new FontWeight(), 52);

                Logger.SyntgohtLog(MainSettingsModel.Instance.UsePause ? "Syntgoht Paused!" : "Syntgoht Resumed!");
            });
        }

        public static void UnregisterAll()
        {
            HotkeyManager.Unregister("Syntgoht_Pause");
        }
    }
}