using ff14bot;
using ff14bot.Objects;
using System.ComponentModel;
using System.Configuration;

namespace Boko.Models
{
    public class MainSettingsModel : BaseModel
    {
        private static LocalPlayer Me => Core.Player;
        private static MainSettingsModel _instance;
        public static MainSettingsModel Instance => _instance ?? (_instance = new MainSettingsModel());

        private MainSettingsModel() : base(@"Settings/" + Me.Name + "/Boko/Main_Settings.json")
        {
        }

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