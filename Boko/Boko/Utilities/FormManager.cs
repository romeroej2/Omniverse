using Boko.Models;
using Boko.Views;

namespace Boko.Utilities
{
    internal class FormManager
    {
        private static BokoWindow _form;

        public static void SaveFormInstances()
        {
            MainSettingsModel.Instance.Save();
        }

        private static BokoWindow Form
        {
            get
            {
                if (_form != null) return _form;
                _form = new BokoWindow();
                _form.Closed += (sender, args) => _form = null;
                return _form;
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