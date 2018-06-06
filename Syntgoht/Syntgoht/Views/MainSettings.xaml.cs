using ff14bot;
using ff14bot.Managers;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace Syntgoht.Views
{
    public partial class MainSettings : UserControl
    {
        public MainSettings()
        {
            InitializeComponent();
        }

        private static string bot = "Syntgoht";

        private void LaunchRoutineSelect(object sender, RoutedEventArgs e)
        {
            RoutineManager.PreferedRoutine = "";
            RoutineManager.PickRoutine();
            System.Threading.Tasks.Task.Factory.StartNew(async () =>
            {
                await TreeRoot.StopGently(" " + "Preparing to switch Combat Routine.");
                BotManager.SetCurrent(BotManager.Bots.FirstOrDefault(r => r.Name == bot));
                TreeRoot.Start();
            });
        }
    }
}