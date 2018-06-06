using ff14bot;
using ff14bot.Objects;
using System.ComponentModel;
using System.Configuration;
using TrelloNet;

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

        private string _trelloToken;

        private Token _trelloTokenData;

        [Setting]
        [DefaultValue("")]
        public string TrelloToken
        { get { return _trelloToken; } set { _trelloToken = value; OnPropertyChanged(); } }

        [Setting]
        [DefaultValue(null)]
        public Token TrelloTokenData
        { get { return _trelloTokenData; } set { _trelloTokenData = value; OnPropertyChanged(); } }

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