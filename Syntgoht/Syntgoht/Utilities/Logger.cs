using ff14bot.Helpers;
using System.Windows.Media;

namespace Syntgoht.Utilities
{
    internal class Logger
    {
        internal static void SyntgohtLog(string text, params object[] args)
        {
            Logging.Write(Colors.LawnGreen, $@"[Syntgoht] {text}", args);
        }
    }
}