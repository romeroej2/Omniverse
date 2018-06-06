using System.Windows.Media;

namespace Syntgoht.ViewModels
{
    public class OverlayViewModel : BaseViewModel
    {
        private static OverlayViewModel _instance;
        public static OverlayViewModel Instance => _instance ?? (_instance = new OverlayViewModel());

        private volatile string isPausedString;
        private SolidColorBrush botBasePausedForegroundColor;

        public string IsPausedString
        { get { return isPausedString; } set { isPausedString = value; OnPropertyChanged(); } }

        public SolidColorBrush BotBasePausedForegroundColor
        { get { return botBasePausedForegroundColor; } set { botBasePausedForegroundColor = value; OnPropertyChanged(); } }
    }
}