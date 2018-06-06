using System;
using System.Collections.Generic;
using System.Linq;
using ff14bot;
using ff14bot.Objects;

namespace Boko.Utilities
{
    internal class Constants
    {
        internal static LocalPlayer Me => Core.Player;
        internal static GameObject Target => Core.Player.CurrentTarget;
    }
}