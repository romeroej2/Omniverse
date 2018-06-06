using Syntgoht.Models;
using Syntgoht.Models.Hotkeys;
using Syntgoht.Views;

namespace Syntgoht.Utilities
{
    internal class FormManager
    {
        private static SyntgohtWindow form;

        public static void SaveFormInstances()
        {
            MainSettingsModel.Instance.Save();
            //KeySettingsModel.Instance.Save();
            SyntgohtHotkeysModel.Instance.Save();
        }

        private static SyntgohtWindow Form
        {
            get
            {
                if (form != null) return form;
                form = new SyntgohtWindow();
                form.Closed += (sender, args) => form = null;
                return form;
            }
        }

        public static void OpenForms()
        {
            if (Form.IsVisible)
            {
                Form.Activate();
                return;
            }

            Form.Show();
        }
    }
}