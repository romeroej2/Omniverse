using Boko.Models;

namespace Boko.ViewModels
{
    public class SettingsViewModel : BaseViewModel
    {
        public static MainSettingsModel Settings => MainSettingsModel.Instance;
        public static ChocoboSettingsModel ChocoboSettings => ChocoboSettingsModel.Instance;
    }
}