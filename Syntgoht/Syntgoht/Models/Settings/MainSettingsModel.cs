using ff14bot;
using ff14bot.Objects;
using System.ComponentModel;
using System.Configuration;

namespace Syntgoht.Models
{
    public class MainSettingsModel : BaseModel
    {
        private static LocalPlayer Me => Core.Player;
        private static MainSettingsModel _instance;
        public static MainSettingsModel Instance => _instance ?? (_instance = new MainSettingsModel());

        private MainSettingsModel() : base(@"Settings/" + Me.Name + "/Syntgoht/Main_Settings.json")
        {
        }

        private bool usePause;

        [Setting]
        [DefaultValue(6000)]
        public int DesynthDelay { get; set; }

        [Setting]
        [DefaultValue(10)]
        public int DesynthTimeout { get; set; }

        [Setting]
        [DefaultValue(3)]
        public int ConsecutiveDesynthTimeoutLimit { get; set; }

        [Setting]
        [DefaultValue(false)]
        public bool UsePause
        { get { return usePause; } set { usePause = value; OnPropertyChanged(); } }

        private SelectedTheme _selectedTheme;

        [Setting]
        [DefaultValue(SelectedTheme.Pink)]
        public SelectedTheme Theme
        { get { return _selectedTheme; } set { _selectedTheme = value; OnPropertyChanged(); } }
    }

    public enum SelectedTheme
    {
        Pink,
        Blue,
        Green,
        Red,
        Yellow
    }
}