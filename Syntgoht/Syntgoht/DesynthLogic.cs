using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Buddy.Coroutines;
using ff14bot;
using ff14bot.Behavior;
using ff14bot.Enums;
using ff14bot.Managers;
using Syntgoht.Models;
using Syntgoht.Utilities;
using TreeSharp;

namespace Syntgoht
{
    internal class DesynthLogic
    {
        private InventoryBagId[] _bagIds = null;

        private static readonly Composite DesynthComposite;
        internal static bool Done = false;

        static DesynthLogic()
        {
            DesynthComposite = new Decorator(new PrioritySelector(new ActionRunCoroutine(r => DesynthMethod())));
        }

        public static Composite Execute()
        {
            return DesynthComposite;
        }

        protected static async Task<bool> DesynthMethod()
        {
            if (!Core.Player.DesynthesisUnlocked)
            {
                Logger.SyntgohtLog("You have not unlocked the desynthesis ability.");

                Done = true;
                return true;
            }

            IEnumerable<BagSlot> desynthables = null;
            var havedDsynthables = InventoryManager.FilledSlots.Any(bs => bs.IsDesynthesizable && bs.CanDesynthesize);

            if (havedDsynthables)
            {
                desynthables = InventoryManager.FilledSlots.Where(bs => bs.IsDesynthesizable && bs.CanDesynthesize);
            }
            else
            {
                Logger.SyntgohtLog("You don't have anything to desynthesize.");

                Done = true;
                return true;
            }

            var numItems = desynthables.Count();

            foreach (var bagSlot in desynthables)
            {
                var name = bagSlot.EnglishName;
                var stackSize = bagSlot.Count;
                var stackIndex = 1;
                var consecutiveTimeouts = 0;
                var desythClass = (ClassJobType)bagSlot.Item.RepairClass;

                Logger.SyntgohtLog("You have {0} items in {1} to desynthesize.", stackSize, bagSlot.BagId);

                while (bagSlot.Count > 0 && !MainSettingsModel.Instance.UsePause)
                {
                    var desynthLevel = Core.Player.GetDesynthesisLevel(desythClass);
                    var desynthTarget = "\"" + name + "\", " + stackIndex + " of " + numItems + ",";

                    Logger.SyntgohtLog("Attempting to desynthesize {0} - success chance is {1}%.", desynthTarget, await CommonTasks.GetDesynthesisChance(bagSlot));

                    var currentStackSize = bagSlot.Item.StackSize;
                    var result = await CommonTasks.Desynthesize(bagSlot, MainSettingsModel.Instance.DesynthDelay);

                    RetryDesynth:

                    if (result != DesynthesisResult.Success)
                    {
                        Logger.SyntgohtLog("Unable to desynthesize {0} due to {1} - moving to next bag slot.", desynthTarget, result);

                        goto RetryDesynth;
                    }

                    await Coroutine.Wait(MainSettingsModel.Instance.DesynthTimeout * 1000, () => (!bagSlot.IsFilled || !bagSlot.EnglishName.Equals(name) || bagSlot.Count != currentStackSize));

                    if (bagSlot.IsFilled && bagSlot.EnglishName.Equals(name) && bagSlot.Count == currentStackSize)
                    {
                        consecutiveTimeouts++;

                        Logger.SyntgohtLog("Timed out awaiting desynthesis of {0} ({1} seconds, attempt {2} of {3}).", desynthTarget, MainSettingsModel.Instance.DesynthTimeout, consecutiveTimeouts, MainSettingsModel.Instance.ConsecutiveDesynthTimeoutLimit);

                        if (consecutiveTimeouts >= MainSettingsModel.Instance.ConsecutiveDesynthTimeoutLimit)
                        {
                            Logger.SyntgohtLog("While desynthesizing {0), exceeded consecutive timeout limit - moving to next bag slot.", desynthTarget);

                            goto RetryDesynth;
                        }
                    }
                    else
                    {
                        var desynthLevelPost = Core.Player.GetDesynthesisLevel(desythClass) - desynthLevel;

                        Logger.SyntgohtLog("Desynthed {0} Sucessfully!. Gained {1} {2} Desyth levels, {2}'s desynthesis level is now {3}.", desynthTarget, desynthLevelPost, desythClass, Core.Player.GetDesynthesisLevel(desythClass));

                        consecutiveTimeouts = 0;
                        stackIndex++;
                    }
                }
            }

            return true;
        }
    }
}