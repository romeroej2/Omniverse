using ff14bot.Helpers;
using System.Windows.Media;

namespace Boko.Utilities
{
    internal class Logger
    {
        internal static void BokoLog(string text, params object[] args)
        {
            Logging.Write(Color.FromRgb(219, 180, 87), $@"[Boko] {text}", args);
        }
    }
}