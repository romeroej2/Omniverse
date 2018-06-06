using Boko.Models;
using Boko.Utilities;
using Buddy.Coroutines;
using ff14bot;
using ff14bot.Enums;
using ff14bot.Managers;
using ff14bot.Objects;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static ff14bot.Managers.ChocoboManager;

namespace Boko
{
    public class ChocoboManager
    {
        private static LocalPlayer Me => Core.Player;
        private static List<uint> _foodAuras = new List<uint> { 536, 537, 538, 539, 540, 541, 542, 543, 544, 545 };

        public static async Task<bool> ChocoboCare()
        {
            LoadThresholdOrders();

            if (!DutyManager.InInstance && ChocoboSettingsModel.Instance.SummonChocobo && Me.IsAlive && Summoned)
            {
                await FeedChocobo();
                if (ExceededThreshold && StanceToSet != 0)
                {
                    await ChangeStance(StanceToSet);
                }
            }

            if (!DutyManager.InInstance && Me.IsAlive && ChocoboSettingsModel.Instance.SummonChocobo && !Summoned && CanSummon && !Me.IsCasting && !MovementManager.IsMoving)
            {
                await SummonChocobo();
            }

            return false;
        }

        #region Properties

        public static bool ExceededThreshold
        {
            get
            {
                var currentHealth = Me.CurrentHealthPercent;
                if (ChocoboSettingsModel.Instance.CheckChocoboHealth && Object != null)
                {
                    if (Object.CurrentHealthPercent < currentHealth)
                        currentHealth = Object.CurrentHealthPercent;
                }
                if (ChocoboSettingsModel.Instance.CheckPartyHealth && PartyManager.NumMembers > 2)
                {
                    foreach (var partyMember in PartyManager.VisibleMembers)
                    {
                        if (partyMember.GameObject != null &&
                            partyMember.GameObject.Type != GameObjectType.BattleNpc &&
                            partyMember.GameObject.CurrentHealthPercent < currentHealth)

                            currentHealth = partyMember.GameObject.CurrentHealthPercent;
                    }
                }

                if (lowestThreshold1 != 0 && currentHealth < LowestThresholdValue1)
                {
                    if (Stance != lowestThreshold1)
                    {
                        StanceToSet = lowestThreshold1;
                        return true;
                    }
                    return false;
                }
                if (lowestThreshold2 != 0 && currentHealth < LowestThresholdValue2)
                {
                    if (Stance != lowestThreshold2 && Stance != lowestThreshold1)
                    {
                        StanceToSet = lowestThreshold2;
                        return true;
                    }
                    return false;
                }
                if (lowestThreshold3 != 0 && currentHealth < LowestThresholdValue3)
                {
                    if (Stance != lowestThreshold3 && Stance != lowestThreshold2 && Stance != lowestThreshold1)
                    {
                        StanceToSet = lowestThreshold3;
                        return true;
                    }
                    return false;
                }
                if (lowestThreshold4 != 0 && currentHealth < LowestThresholdValue4)
                {
                    if (Stance != lowestThreshold4 && Stance != lowestThreshold3 && Stance != lowestThreshold2 && Stance != lowestThreshold1)
                    {
                        StanceToSet = lowestThreshold4;
                        return true;
                    }
                    return false;
                }
                if (highestThreshold1 != 0 && currentHealth > HighestThresholdValue1)
                {
                    if (Stance != highestThreshold1)
                    {
                        StanceToSet = highestThreshold1;
                        return true;
                    }
                    return false;
                }
                if (highestThreshold2 != 0 && currentHealth > HighestThresholdValue2)
                {
                    if (Stance != highestThreshold2 && Stance != highestThreshold1)
                    {
                        StanceToSet = highestThreshold2;
                        return true;
                    }
                    return false;
                }
                if (highestThreshold3 != 0 && currentHealth > HighestThresholdValue3)
                {
                    if (Stance != highestThreshold3 && Stance != highestThreshold2 && Stance != highestThreshold1)
                    {
                        StanceToSet = highestThreshold3;
                        return true;
                    }
                    return false;
                }
                if (highestThreshold4 != 0 && currentHealth > HighestThresholdValue4)
                {
                    if (Stance != highestThreshold4 && Stance != highestThreshold3 && Stance != highestThreshold2 && Stance != highestThreshold1)
                    {
                        StanceToSet = highestThreshold4;

                        return true;
                    }
                    return false;
                }

                return false;
            }
        }

        public static CompanionStance StanceToSet = 0;

        public static CompanionStance lowestThreshold1 = 0;
        public static CompanionStance lowestThreshold2 = 0;
        public static CompanionStance lowestThreshold3 = 0;
        public static CompanionStance lowestThreshold4 = 0;
        public static CompanionStance highestThreshold1 = 0;
        public static CompanionStance highestThreshold2 = 0;
        public static CompanionStance highestThreshold3 = 0;
        public static CompanionStance highestThreshold4 = 0;

        public static int LowestThresholdValue1;
        public static int LowestThresholdValue2;
        public static int LowestThresholdValue3;
        public static int LowestThresholdValue4;
        public static int HighestThresholdValue1 = 100;
        public static int HighestThresholdValue2 = 100;
        public static int HighestThresholdValue3 = 100;
        public static int HighestThresholdValue4 = 100;

        #endregion Properties

        #region FeedChocobo

        private static async Task<bool> FeedChocobo()
        {
            if (Object == null || ChocoboSettingsModel.Instance.Food == FoodMode.None) return false;

            var chocobo = Object as Character;

            foreach (var foodAura in _foodAuras)
            {
                if (chocobo != null && !chocobo.HasAura(foodAura))
                {
                    switch (ChocoboSettingsModel.Instance.Food)
                    {
                        case FoodMode.CurielRoot:
                            if (!ChocoboSettingsModel.Instance.FoodFavored && !chocobo.HasAura(536) || ChocoboSettingsModel.Instance.FoodFavored && !chocobo.HasAura(537))
                            {
                                var curielRoot = InventoryManager.FilledSlots.FirstOrDefault(a => a.Item.Id == 7894);
                                curielRoot?.UseItem();
                            }
                            break;

                        case FoodMode.SylkisBud:
                            if (!ChocoboSettingsModel.Instance.FoodFavored && !chocobo.HasAura(538) || ChocoboSettingsModel.Instance.FoodFavored && !chocobo.HasAura(539))
                            {
                                var sylkisBud = InventoryManager.FilledSlots.FirstOrDefault(a => a.Item.Id == 7895);
                                sylkisBud?.UseItem();
                            }
                            break;

                        case FoodMode.MimettGourd:
                            if (!ChocoboSettingsModel.Instance.FoodFavored && !chocobo.HasAura(540) || ChocoboSettingsModel.Instance.FoodFavored && !chocobo.HasAura(541))
                            {
                                var mimettGourd = InventoryManager.FilledSlots.FirstOrDefault(a => a.Item.Id == 7897);
                                mimettGourd?.UseItem();
                            }
                            break;

                        case FoodMode.Tantalplant:
                            if (!ChocoboSettingsModel.Instance.FoodFavored && !chocobo.HasAura(542) || ChocoboSettingsModel.Instance.FoodFavored && !chocobo.HasAura(543))
                            {
                                var tantalplant = InventoryManager.FilledSlots.FirstOrDefault(a => a.Item.Id == 7898);
                                tantalplant?.UseItem();
                            }
                            break;

                        case FoodMode.PahsanaFruit:

                            if (!ChocoboSettingsModel.Instance.FoodFavored && !chocobo.HasAura(544) || ChocoboSettingsModel.Instance.FoodFavored && !chocobo.HasAura(545))
                            {
                                var pahsanaFruit = InventoryManager.FilledSlots.FirstOrDefault(a => a.Item.Id == 7900);
                                pahsanaFruit?.UseItem();
                            }
                            break;

                        default:
                            return false;
                    }
                }
            }
            return false;
        }

        #endregion FeedChocobo

        #region Summon Chocobo Logic

        public static async Task<bool> SummonChocobo()
        {
            if (InventoryManager.FilledSlots.Any(slot => slot.RawItemId == 4868))
            {
                Logger.BokoLog(@"====> {0}", @"Summoning ");
                Summon();
                await Coroutine.Wait(3000, () => Summoned);
                return false;
            }

            Logger.BokoLog(@"====> {0}", @"No Gysahl Greens; Please purchase some so we can summon our ");
            return false;
        }

        #endregion Summon Chocobo Logic

        #region Change Stance Logic

        private static async Task<bool> ChangeStance(CompanionStance stance)
        {
            if (Me.IsMounted) return false;

            Logger.BokoLog(@"====> {0}", @"Changing stance to " + stance + ".");
            switch (stance)
            {
                case CompanionStance.Healer:
                    HealerStance();
                    await Coroutine.Wait(2000, () => Stance == CompanionStance.Healer);
                    break;

                case CompanionStance.Defender:
                    DefenderStance();
                    await Coroutine.Wait(2000, () => Stance == CompanionStance.Defender);
                    break;

                case CompanionStance.Attacker:
                    AttackerStance();
                    await Coroutine.Wait(2000, () => Stance == CompanionStance.Attacker);
                    break;

                case CompanionStance.Free:
                    FreeStance();
                    await Coroutine.Wait(2000, () => Stance == CompanionStance.Free);
                    break;
            }
            return false;
        }

        #endregion Change Stance Logic

        #region LoadThresholds

        public static void LoadThresholdOrders()
        {
            lowestThreshold1 = LowestThreshold1();

            if (lowestThreshold1 != 0)
            {
                lowestThreshold2 = LowestThreshold2();
            }
            else
            {
                LowestThresholdValue2 = 0;
                lowestThreshold2 = 0;
            }
            if (lowestThreshold2 != 0)
            {
                lowestThreshold3 = LowestThreshold3();
            }
            else
            {
                LowestThresholdValue3 = 0;
                lowestThreshold3 = 0;
            }
            if (lowestThreshold3 != 0)
            {
                lowestThreshold4 = LowestThreshold4();
            }
            else
            {
                LowestThresholdValue4 = 0;
                lowestThreshold4 = 0;
            }

            if (lowestThreshold4 != 0)
            {
                highestThreshold1 = 0;
                highestThreshold2 = 0;
                highestThreshold3 = 0;
                highestThreshold4 = 0;
                HighestThresholdValue1 = 100;
                HighestThresholdValue2 = 100;
                HighestThresholdValue3 = 100;
                HighestThresholdValue4 = 100;
            }
            else
            {
                highestThreshold1 = HighestThreshold1();

                if (highestThreshold1 != 0)
                {
                    highestThreshold2 = HighestThreshold2();
                }
                else
                {
                    HighestThresholdValue2 = 100;
                    highestThreshold2 = 0;
                }
                if (highestThreshold2 != 0)
                {
                    highestThreshold3 = HighestThreshold3();
                }
                else
                {
                    HighestThresholdValue3 = 100;
                    highestThreshold3 = 0;
                }
                if (highestThreshold3 != 0)
                {
                    highestThreshold4 = HighestThreshold4();
                }
                else
                {
                    HighestThresholdValue4 = 100;
                    highestThreshold4 = 0;
                }
            }
        }

        private static CompanionStance LowestThreshold1()
        {
            if (ChocoboSettingsModel.Instance.UseHealerStance && ChocoboSettingsModel.Instance.HealerThresholdUseDrop &&
                (!ChocoboSettingsModel.Instance.UseAttackerStance || !ChocoboSettingsModel.Instance.AttackerThresholdUseDrop || ChocoboSettingsModel.Instance.HealerThreshold <= ChocoboSettingsModel.Instance.AttackerThreshold) &&
                (!ChocoboSettingsModel.Instance.UseDefenderStance || !ChocoboSettingsModel.Instance.DefenderThresholdUseDrop || ChocoboSettingsModel.Instance.HealerThreshold <= ChocoboSettingsModel.Instance.DefenderThreshold) &&
                (!ChocoboSettingsModel.Instance.UseFreeStance || !ChocoboSettingsModel.Instance.FreeThresholdUseDrop || ChocoboSettingsModel.Instance.HealerThreshold <= ChocoboSettingsModel.Instance.FreeThreshold))
            {
                LowestThresholdValue1 = ChocoboSettingsModel.Instance.HealerThreshold;
                return CompanionStance.Healer;
            }
            if (ChocoboSettingsModel.Instance.UseDefenderStance && ChocoboSettingsModel.Instance.DefenderThresholdUseDrop &&
                (!ChocoboSettingsModel.Instance.UseAttackerStance || !ChocoboSettingsModel.Instance.AttackerThresholdUseDrop || ChocoboSettingsModel.Instance.DefenderThreshold <= ChocoboSettingsModel.Instance.AttackerThreshold) &&
                (!ChocoboSettingsModel.Instance.UseFreeStance || !ChocoboSettingsModel.Instance.FreeThresholdUseDrop || ChocoboSettingsModel.Instance.DefenderThreshold <= ChocoboSettingsModel.Instance.FreeThreshold))
            {
                LowestThresholdValue1 = ChocoboSettingsModel.Instance.DefenderThreshold;
                return CompanionStance.Defender;
            }
            if (ChocoboSettingsModel.Instance.UseAttackerStance && ChocoboSettingsModel.Instance.AttackerThresholdUseDrop &&
                (!ChocoboSettingsModel.Instance.UseFreeStance || !ChocoboSettingsModel.Instance.FreeThresholdUseDrop || ChocoboSettingsModel.Instance.AttackerThreshold <= ChocoboSettingsModel.Instance.FreeThreshold))
            {
                LowestThresholdValue1 = ChocoboSettingsModel.Instance.AttackerThreshold;
                return CompanionStance.Attacker;
            }
            if (ChocoboSettingsModel.Instance.UseFreeStance && ChocoboSettingsModel.Instance.FreeThresholdUseDrop)
            {
                LowestThresholdValue1 = ChocoboSettingsModel.Instance.FreeThreshold;
                return CompanionStance.Free;
            }
            LowestThresholdValue1 = 0;
            return 0;
        }

        private static CompanionStance LowestThreshold2()
        {
            if (lowestThreshold1 == CompanionStance.Healer)
            {
                if (ChocoboSettingsModel.Instance.UseDefenderStance && ChocoboSettingsModel.Instance.DefenderThresholdUseDrop &&
                    (!ChocoboSettingsModel.Instance.UseAttackerStance || !ChocoboSettingsModel.Instance.AttackerThresholdUseDrop || ChocoboSettingsModel.Instance.DefenderThreshold <= ChocoboSettingsModel.Instance.AttackerThreshold) &&
                    (!ChocoboSettingsModel.Instance.UseFreeStance || !ChocoboSettingsModel.Instance.FreeThresholdUseDrop || ChocoboSettingsModel.Instance.DefenderThreshold <= ChocoboSettingsModel.Instance.FreeThreshold))
                {
                    LowestThresholdValue2 = ChocoboSettingsModel.Instance.DefenderThreshold;
                    return CompanionStance.Defender;
                }
                if (ChocoboSettingsModel.Instance.UseAttackerStance && ChocoboSettingsModel.Instance.AttackerThresholdUseDrop &&
                    (!ChocoboSettingsModel.Instance.UseFreeStance || !ChocoboSettingsModel.Instance.FreeThresholdUseDrop || ChocoboSettingsModel.Instance.AttackerThreshold <= ChocoboSettingsModel.Instance.FreeThreshold))
                {
                    LowestThresholdValue2 = ChocoboSettingsModel.Instance.AttackerThreshold;
                    return CompanionStance.Attacker;
                }
                if (ChocoboSettingsModel.Instance.UseFreeStance && ChocoboSettingsModel.Instance.FreeThresholdUseDrop)
                {
                    LowestThresholdValue2 = ChocoboSettingsModel.Instance.FreeThreshold;
                    return CompanionStance.Free;
                }
            }
            if (lowestThreshold1 == CompanionStance.Defender)
            {
                if (ChocoboSettingsModel.Instance.UseHealerStance && ChocoboSettingsModel.Instance.HealerThresholdUseDrop &&
                    (!ChocoboSettingsModel.Instance.UseAttackerStance || !ChocoboSettingsModel.Instance.AttackerThresholdUseDrop || ChocoboSettingsModel.Instance.HealerThreshold <= ChocoboSettingsModel.Instance.AttackerThreshold) &&
                    (!ChocoboSettingsModel.Instance.UseFreeStance || !ChocoboSettingsModel.Instance.FreeThresholdUseDrop || ChocoboSettingsModel.Instance.HealerThreshold <= ChocoboSettingsModel.Instance.FreeThreshold))
                {
                    LowestThresholdValue2 = ChocoboSettingsModel.Instance.HealerThreshold;
                    return CompanionStance.Healer;
                }
                if (ChocoboSettingsModel.Instance.UseAttackerStance && ChocoboSettingsModel.Instance.AttackerThresholdUseDrop &&
                    (!ChocoboSettingsModel.Instance.UseFreeStance || !ChocoboSettingsModel.Instance.FreeThresholdUseDrop || ChocoboSettingsModel.Instance.AttackerThreshold <= ChocoboSettingsModel.Instance.FreeThreshold))
                {
                    LowestThresholdValue2 = ChocoboSettingsModel.Instance.AttackerThreshold;
                    return CompanionStance.Attacker;
                }
                if (ChocoboSettingsModel.Instance.UseFreeStance && ChocoboSettingsModel.Instance.FreeThresholdUseDrop)
                {
                    LowestThresholdValue2 = ChocoboSettingsModel.Instance.FreeThreshold;
                    return CompanionStance.Free;
                }
            }
            if (lowestThreshold1 == CompanionStance.Attacker)
            {
                if (ChocoboSettingsModel.Instance.UseHealerStance && ChocoboSettingsModel.Instance.HealerThresholdUseDrop &&
                    (!ChocoboSettingsModel.Instance.UseDefenderStance || !ChocoboSettingsModel.Instance.DefenderThresholdUseDrop || ChocoboSettingsModel.Instance.HealerThreshold <= ChocoboSettingsModel.Instance.DefenderThreshold) &&
                    (!ChocoboSettingsModel.Instance.UseFreeStance || !ChocoboSettingsModel.Instance.FreeThresholdUseDrop || ChocoboSettingsModel.Instance.HealerThreshold <= ChocoboSettingsModel.Instance.FreeThreshold))
                {
                    LowestThresholdValue2 = ChocoboSettingsModel.Instance.HealerThreshold;
                    return CompanionStance.Healer;
                }
                if (ChocoboSettingsModel.Instance.UseDefenderStance && ChocoboSettingsModel.Instance.DefenderThresholdUseDrop &&
                    (!ChocoboSettingsModel.Instance.UseFreeStance || !ChocoboSettingsModel.Instance.FreeThresholdUseDrop || ChocoboSettingsModel.Instance.DefenderThreshold <= ChocoboSettingsModel.Instance.FreeThreshold))
                {
                    LowestThresholdValue2 = ChocoboSettingsModel.Instance.DefenderThreshold;
                    return CompanionStance.Defender;
                }
                if (ChocoboSettingsModel.Instance.UseFreeStance && ChocoboSettingsModel.Instance.FreeThresholdUseDrop)
                {
                    LowestThresholdValue2 = ChocoboSettingsModel.Instance.FreeThreshold;
                    return CompanionStance.Free;
                }
            }
            if (lowestThreshold1 == CompanionStance.Free)
            {
                if (ChocoboSettingsModel.Instance.UseHealerStance && ChocoboSettingsModel.Instance.HealerThresholdUseDrop &&
                    (!ChocoboSettingsModel.Instance.UseAttackerStance || !ChocoboSettingsModel.Instance.AttackerThresholdUseDrop || ChocoboSettingsModel.Instance.HealerThreshold <= ChocoboSettingsModel.Instance.AttackerThreshold) &&
                    (!ChocoboSettingsModel.Instance.UseDefenderStance || !ChocoboSettingsModel.Instance.DefenderThresholdUseDrop || ChocoboSettingsModel.Instance.HealerThreshold <= ChocoboSettingsModel.Instance.DefenderThreshold))
                {
                    LowestThresholdValue2 = ChocoboSettingsModel.Instance.HealerThreshold;
                    return CompanionStance.Healer;
                }
                if (ChocoboSettingsModel.Instance.UseDefenderStance && ChocoboSettingsModel.Instance.DefenderThresholdUseDrop &&
                    (!ChocoboSettingsModel.Instance.UseAttackerStance || !ChocoboSettingsModel.Instance.AttackerThresholdUseDrop || ChocoboSettingsModel.Instance.DefenderThreshold <= ChocoboSettingsModel.Instance.AttackerThreshold))
                {
                    LowestThresholdValue2 = ChocoboSettingsModel.Instance.DefenderThreshold;
                    return CompanionStance.Defender;
                }
                if (ChocoboSettingsModel.Instance.UseAttackerStance && ChocoboSettingsModel.Instance.AttackerThresholdUseDrop)
                {
                    LowestThresholdValue2 = ChocoboSettingsModel.Instance.AttackerThreshold;
                    return CompanionStance.Attacker;
                }
            }
            LowestThresholdValue2 = 0;
            return 0;
        }

        private static CompanionStance LowestThreshold3()
        {
            if (lowestThreshold1 == CompanionStance.Healer || lowestThreshold2 == CompanionStance.Healer)
            {
                if (lowestThreshold1 == CompanionStance.Defender || lowestThreshold2 == CompanionStance.Defender)
                {
                    if (ChocoboSettingsModel.Instance.UseAttackerStance && ChocoboSettingsModel.Instance.AttackerThresholdUseDrop &&
                        (!ChocoboSettingsModel.Instance.UseFreeStance || !ChocoboSettingsModel.Instance.FreeThresholdUseDrop || ChocoboSettingsModel.Instance.AttackerThreshold <= ChocoboSettingsModel.Instance.FreeThreshold))
                    {
                        LowestThresholdValue3 = ChocoboSettingsModel.Instance.AttackerThreshold;
                        return CompanionStance.Attacker;
                    }
                    if (ChocoboSettingsModel.Instance.UseFreeStance && ChocoboSettingsModel.Instance.FreeThresholdUseDrop)
                    {
                        LowestThresholdValue3 = ChocoboSettingsModel.Instance.FreeThreshold;
                        return CompanionStance.Free;
                    }
                }
                if (lowestThreshold1 == CompanionStance.Attacker || lowestThreshold2 == CompanionStance.Attacker)
                {
                    if (ChocoboSettingsModel.Instance.UseDefenderStance && ChocoboSettingsModel.Instance.DefenderThresholdUseDrop &&
                        (!ChocoboSettingsModel.Instance.UseFreeStance || !ChocoboSettingsModel.Instance.FreeThresholdUseDrop || ChocoboSettingsModel.Instance.DefenderThreshold <= ChocoboSettingsModel.Instance.FreeThreshold))
                    {
                        LowestThresholdValue3 = ChocoboSettingsModel.Instance.DefenderThreshold;
                        return CompanionStance.Defender;
                    }
                    if (ChocoboSettingsModel.Instance.UseFreeStance && ChocoboSettingsModel.Instance.FreeThresholdUseDrop)
                    {
                        LowestThresholdValue3 = ChocoboSettingsModel.Instance.FreeThreshold;
                        return CompanionStance.Free;
                    }
                }
                if (lowestThreshold1 == CompanionStance.Free || lowestThreshold2 == CompanionStance.Free)
                {
                    if (ChocoboSettingsModel.Instance.UseDefenderStance && ChocoboSettingsModel.Instance.DefenderThresholdUseDrop &&
                        (!ChocoboSettingsModel.Instance.UseAttackerStance || !ChocoboSettingsModel.Instance.AttackerThresholdUseDrop || ChocoboSettingsModel.Instance.DefenderThreshold <= ChocoboSettingsModel.Instance.AttackerThreshold))
                    {
                        LowestThresholdValue3 = ChocoboSettingsModel.Instance.DefenderThreshold;
                        return CompanionStance.Defender;
                    }
                    if (ChocoboSettingsModel.Instance.UseAttackerStance && ChocoboSettingsModel.Instance.AttackerThresholdUseDrop)
                    {
                        LowestThresholdValue3 = ChocoboSettingsModel.Instance.AttackerThreshold;
                        return CompanionStance.Attacker;
                    }
                }
            }
            if (lowestThreshold1 == CompanionStance.Defender || lowestThreshold2 == CompanionStance.Defender)
            {
                if (lowestThreshold1 == CompanionStance.Attacker || lowestThreshold2 == CompanionStance.Attacker)
                {
                    if (ChocoboSettingsModel.Instance.UseHealerStance && ChocoboSettingsModel.Instance.HealerThresholdUseDrop &&
                        (!ChocoboSettingsModel.Instance.UseFreeStance || !ChocoboSettingsModel.Instance.FreeThresholdUseDrop || ChocoboSettingsModel.Instance.HealerThreshold <= ChocoboSettingsModel.Instance.FreeThreshold))
                    {
                        LowestThresholdValue3 = ChocoboSettingsModel.Instance.HealerThreshold;
                        return CompanionStance.Healer;
                    }
                    if (ChocoboSettingsModel.Instance.UseFreeStance && ChocoboSettingsModel.Instance.FreeThresholdUseDrop)
                    {
                        LowestThresholdValue3 = ChocoboSettingsModel.Instance.FreeThreshold;
                        return CompanionStance.Free;
                    }
                }
                if (lowestThreshold1 == CompanionStance.Free || lowestThreshold2 == CompanionStance.Free)
                {
                    if (ChocoboSettingsModel.Instance.UseHealerStance && ChocoboSettingsModel.Instance.HealerThresholdUseDrop &&
                        (!ChocoboSettingsModel.Instance.UseAttackerStance || !ChocoboSettingsModel.Instance.AttackerThresholdUseDrop || ChocoboSettingsModel.Instance.HealerThreshold <= ChocoboSettingsModel.Instance.AttackerThreshold))
                    {
                        LowestThresholdValue3 = ChocoboSettingsModel.Instance.HealerThreshold;
                        return CompanionStance.Healer;
                    }
                    if (ChocoboSettingsModel.Instance.UseAttackerStance && ChocoboSettingsModel.Instance.AttackerThresholdUseDrop)
                    {
                        LowestThresholdValue3 = ChocoboSettingsModel.Instance.AttackerThreshold;
                        return CompanionStance.Attacker;
                    }
                }
            }
            if (lowestThreshold1 == CompanionStance.Attacker || lowestThreshold2 == CompanionStance.Attacker)
            {
                if (lowestThreshold1 == CompanionStance.Free || lowestThreshold2 == CompanionStance.Free)
                {
                    if (ChocoboSettingsModel.Instance.UseHealerStance && ChocoboSettingsModel.Instance.HealerThresholdUseDrop &&
                        (!ChocoboSettingsModel.Instance.UseDefenderStance || !ChocoboSettingsModel.Instance.DefenderThresholdUseDrop || ChocoboSettingsModel.Instance.HealerThreshold <= ChocoboSettingsModel.Instance.DefenderThreshold))
                    {
                        LowestThresholdValue3 = ChocoboSettingsModel.Instance.HealerThreshold;
                        return CompanionStance.Healer;
                    }
                    if (ChocoboSettingsModel.Instance.UseDefenderStance && ChocoboSettingsModel.Instance.DefenderThresholdUseDrop)
                    {
                        LowestThresholdValue3 = ChocoboSettingsModel.Instance.DefenderThreshold;
                        return CompanionStance.Defender;
                    }
                }
            }
            LowestThresholdValue3 = 0;
            return 0;
        }

        private static CompanionStance LowestThreshold4()
        {
            if (lowestThreshold1 != CompanionStance.Healer && lowestThreshold2 != CompanionStance.Healer && lowestThreshold3 != CompanionStance.Healer)
            {
                if (ChocoboSettingsModel.Instance.UseHealerStance && ChocoboSettingsModel.Instance.HealerThresholdUseDrop)
                {
                    LowestThresholdValue4 = ChocoboSettingsModel.Instance.HealerThreshold;
                    return CompanionStance.Healer;
                }
            }
            if (lowestThreshold1 != CompanionStance.Defender && lowestThreshold2 != CompanionStance.Defender && lowestThreshold3 != CompanionStance.Defender)
            {
                if (ChocoboSettingsModel.Instance.UseDefenderStance && ChocoboSettingsModel.Instance.DefenderThresholdUseDrop)
                {
                    LowestThresholdValue4 = ChocoboSettingsModel.Instance.DefenderThreshold;
                    return CompanionStance.Defender;
                }
            }
            if (lowestThreshold1 != CompanionStance.Attacker && lowestThreshold2 != CompanionStance.Attacker && lowestThreshold3 != CompanionStance.Attacker)
            {
                if (ChocoboSettingsModel.Instance.UseAttackerStance && ChocoboSettingsModel.Instance.AttackerThresholdUseDrop)
                {
                    LowestThresholdValue4 = ChocoboSettingsModel.Instance.AttackerThreshold;
                    return CompanionStance.Attacker;
                }
            }
            if (lowestThreshold1 != CompanionStance.Free && lowestThreshold2 != CompanionStance.Free && lowestThreshold3 != CompanionStance.Free)
            {
                if (ChocoboSettingsModel.Instance.UseFreeStance && ChocoboSettingsModel.Instance.FreeThresholdUseDrop)
                {
                    LowestThresholdValue4 = ChocoboSettingsModel.Instance.FreeThreshold;
                    return CompanionStance.Free;
                }
            }
            LowestThresholdValue4 = 0;
            return 0;
        }

        private static CompanionStance HighestThreshold1()
        {
            if (ChocoboSettingsModel.Instance.UseHealerStance && !ChocoboSettingsModel.Instance.HealerThresholdUseDrop &&
                (!ChocoboSettingsModel.Instance.UseAttackerStance || ChocoboSettingsModel.Instance.AttackerThresholdUseDrop || ChocoboSettingsModel.Instance.HealerThreshold >= ChocoboSettingsModel.Instance.AttackerThreshold) &&
                (!ChocoboSettingsModel.Instance.UseDefenderStance || ChocoboSettingsModel.Instance.DefenderThresholdUseDrop || ChocoboSettingsModel.Instance.HealerThreshold >= ChocoboSettingsModel.Instance.DefenderThreshold) &&
                (!ChocoboSettingsModel.Instance.UseFreeStance || ChocoboSettingsModel.Instance.FreeThresholdUseDrop || ChocoboSettingsModel.Instance.HealerThreshold >= ChocoboSettingsModel.Instance.FreeThreshold))
            {
                HighestThresholdValue1 = ChocoboSettingsModel.Instance.HealerThreshold;
                return CompanionStance.Healer;
            }
            if (ChocoboSettingsModel.Instance.UseDefenderStance && !ChocoboSettingsModel.Instance.DefenderThresholdUseDrop &&
                (!ChocoboSettingsModel.Instance.UseAttackerStance || ChocoboSettingsModel.Instance.AttackerThresholdUseDrop || ChocoboSettingsModel.Instance.DefenderThreshold >= ChocoboSettingsModel.Instance.AttackerThreshold) &&
                (!ChocoboSettingsModel.Instance.UseFreeStance || ChocoboSettingsModel.Instance.FreeThresholdUseDrop || ChocoboSettingsModel.Instance.DefenderThreshold >= ChocoboSettingsModel.Instance.FreeThreshold))
            {
                HighestThresholdValue1 = ChocoboSettingsModel.Instance.DefenderThreshold;
                return CompanionStance.Defender;
            }
            if (ChocoboSettingsModel.Instance.UseAttackerStance && !ChocoboSettingsModel.Instance.AttackerThresholdUseDrop &&
                (!ChocoboSettingsModel.Instance.UseFreeStance || ChocoboSettingsModel.Instance.FreeThresholdUseDrop || ChocoboSettingsModel.Instance.AttackerThreshold >= ChocoboSettingsModel.Instance.FreeThreshold))
            {
                HighestThresholdValue1 = ChocoboSettingsModel.Instance.AttackerThreshold;
                return CompanionStance.Attacker;
            }
            if (ChocoboSettingsModel.Instance.UseFreeStance && !ChocoboSettingsModel.Instance.FreeThresholdUseDrop)
            {
                HighestThresholdValue1 = ChocoboSettingsModel.Instance.FreeThreshold;
                return CompanionStance.Free;
            }
            HighestThresholdValue1 = 100;
            return 0;
        }

        private static CompanionStance HighestThreshold2()
        {
            if (highestThreshold1 == CompanionStance.Healer)
            {
                if (ChocoboSettingsModel.Instance.UseDefenderStance && !ChocoboSettingsModel.Instance.DefenderThresholdUseDrop &&
                    (!ChocoboSettingsModel.Instance.UseAttackerStance || ChocoboSettingsModel.Instance.AttackerThresholdUseDrop || ChocoboSettingsModel.Instance.DefenderThreshold >= ChocoboSettingsModel.Instance.AttackerThreshold) &&
                    (!ChocoboSettingsModel.Instance.UseFreeStance || ChocoboSettingsModel.Instance.FreeThresholdUseDrop || ChocoboSettingsModel.Instance.DefenderThreshold >= ChocoboSettingsModel.Instance.FreeThreshold))
                {
                    HighestThresholdValue2 = ChocoboSettingsModel.Instance.DefenderThreshold;
                    return CompanionStance.Defender;
                }
                if (ChocoboSettingsModel.Instance.UseAttackerStance && !ChocoboSettingsModel.Instance.AttackerThresholdUseDrop &&
                    (!ChocoboSettingsModel.Instance.UseFreeStance || !ChocoboSettingsModel.Instance.FreeThresholdUseDrop || ChocoboSettingsModel.Instance.AttackerThreshold >= ChocoboSettingsModel.Instance.FreeThreshold))
                {
                    HighestThresholdValue2 = ChocoboSettingsModel.Instance.AttackerThreshold;
                    return CompanionStance.Attacker;
                }
                if (ChocoboSettingsModel.Instance.UseFreeStance && !ChocoboSettingsModel.Instance.FreeThresholdUseDrop)
                {
                    HighestThresholdValue2 = ChocoboSettingsModel.Instance.FreeThreshold;
                    return CompanionStance.Free;
                }
            }
            if (highestThreshold1 == CompanionStance.Defender)
            {
                if (ChocoboSettingsModel.Instance.UseHealerStance && !ChocoboSettingsModel.Instance.HealerThresholdUseDrop &&
                    (!ChocoboSettingsModel.Instance.UseAttackerStance || ChocoboSettingsModel.Instance.AttackerThresholdUseDrop || ChocoboSettingsModel.Instance.HealerThreshold >= ChocoboSettingsModel.Instance.AttackerThreshold) &&
                    (!ChocoboSettingsModel.Instance.UseFreeStance || ChocoboSettingsModel.Instance.FreeThresholdUseDrop || ChocoboSettingsModel.Instance.HealerThreshold >= ChocoboSettingsModel.Instance.FreeThreshold))
                {
                    HighestThresholdValue2 = ChocoboSettingsModel.Instance.HealerThreshold;
                    return CompanionStance.Healer;
                }
                if (ChocoboSettingsModel.Instance.UseAttackerStance && !ChocoboSettingsModel.Instance.AttackerThresholdUseDrop &&
                    (!ChocoboSettingsModel.Instance.UseFreeStance || ChocoboSettingsModel.Instance.FreeThresholdUseDrop || ChocoboSettingsModel.Instance.AttackerThreshold >= ChocoboSettingsModel.Instance.FreeThreshold))
                {
                    HighestThresholdValue2 = ChocoboSettingsModel.Instance.AttackerThreshold;
                    return CompanionStance.Attacker;
                }
                if (ChocoboSettingsModel.Instance.UseFreeStance && !ChocoboSettingsModel.Instance.FreeThresholdUseDrop)
                {
                    HighestThresholdValue2 = ChocoboSettingsModel.Instance.FreeThreshold;
                    return CompanionStance.Free;
                }
            }
            if (highestThreshold1 == CompanionStance.Attacker)
            {
                if (ChocoboSettingsModel.Instance.UseHealerStance && !ChocoboSettingsModel.Instance.HealerThresholdUseDrop &&
                    (!ChocoboSettingsModel.Instance.UseDefenderStance || ChocoboSettingsModel.Instance.DefenderThresholdUseDrop || ChocoboSettingsModel.Instance.HealerThreshold >= ChocoboSettingsModel.Instance.DefenderThreshold) &&
                    (!ChocoboSettingsModel.Instance.UseFreeStance || ChocoboSettingsModel.Instance.FreeThresholdUseDrop || ChocoboSettingsModel.Instance.HealerThreshold >= ChocoboSettingsModel.Instance.FreeThreshold))
                {
                    HighestThresholdValue2 = ChocoboSettingsModel.Instance.HealerThreshold;
                    return CompanionStance.Healer;
                }
                if (ChocoboSettingsModel.Instance.UseDefenderStance && !ChocoboSettingsModel.Instance.DefenderThresholdUseDrop &&
                    (!ChocoboSettingsModel.Instance.UseFreeStance || ChocoboSettingsModel.Instance.FreeThresholdUseDrop || ChocoboSettingsModel.Instance.DefenderThreshold >= ChocoboSettingsModel.Instance.FreeThreshold))
                {
                    HighestThresholdValue2 = ChocoboSettingsModel.Instance.DefenderThreshold;
                    return CompanionStance.Defender;
                }
                if (ChocoboSettingsModel.Instance.UseFreeStance && !ChocoboSettingsModel.Instance.FreeThresholdUseDrop)
                {
                    HighestThresholdValue2 = ChocoboSettingsModel.Instance.FreeThreshold;
                    return CompanionStance.Free;
                }
            }
            if (highestThreshold1 == CompanionStance.Free)
            {
                if (ChocoboSettingsModel.Instance.UseHealerStance && !ChocoboSettingsModel.Instance.HealerThresholdUseDrop &&
                    (!ChocoboSettingsModel.Instance.UseAttackerStance || ChocoboSettingsModel.Instance.AttackerThresholdUseDrop || ChocoboSettingsModel.Instance.HealerThreshold >= ChocoboSettingsModel.Instance.AttackerThreshold) &&
                    (!ChocoboSettingsModel.Instance.UseDefenderStance || ChocoboSettingsModel.Instance.DefenderThresholdUseDrop || ChocoboSettingsModel.Instance.HealerThreshold >= ChocoboSettingsModel.Instance.DefenderThreshold))
                {
                    HighestThresholdValue2 = ChocoboSettingsModel.Instance.HealerThreshold;
                    return CompanionStance.Healer;
                }
                if (ChocoboSettingsModel.Instance.UseDefenderStance && !ChocoboSettingsModel.Instance.DefenderThresholdUseDrop &&
                    (!ChocoboSettingsModel.Instance.UseAttackerStance || ChocoboSettingsModel.Instance.AttackerThresholdUseDrop || ChocoboSettingsModel.Instance.DefenderThreshold >= ChocoboSettingsModel.Instance.AttackerThreshold))
                {
                    HighestThresholdValue2 = ChocoboSettingsModel.Instance.DefenderThreshold;
                    return CompanionStance.Defender;
                }
                if (ChocoboSettingsModel.Instance.UseAttackerStance && !ChocoboSettingsModel.Instance.AttackerThresholdUseDrop)
                {
                    HighestThresholdValue2 = ChocoboSettingsModel.Instance.AttackerThreshold;
                    return CompanionStance.Attacker;
                }
            }
            HighestThresholdValue2 = 100;
            return 0;
        }

        private static CompanionStance HighestThreshold3()
        {
            if (highestThreshold1 == CompanionStance.Healer || highestThreshold2 == CompanionStance.Healer)
            {
                if (highestThreshold1 == CompanionStance.Defender || highestThreshold2 == CompanionStance.Defender)
                {
                    if (ChocoboSettingsModel.Instance.UseAttackerStance && !ChocoboSettingsModel.Instance.AttackerThresholdUseDrop &&
                        (!ChocoboSettingsModel.Instance.UseFreeStance || ChocoboSettingsModel.Instance.FreeThresholdUseDrop || ChocoboSettingsModel.Instance.AttackerThreshold >= ChocoboSettingsModel.Instance.FreeThreshold))
                    {
                        HighestThresholdValue3 = ChocoboSettingsModel.Instance.AttackerThreshold;
                        return CompanionStance.Attacker;
                    }
                    if (ChocoboSettingsModel.Instance.UseFreeStance && !ChocoboSettingsModel.Instance.FreeThresholdUseDrop)
                    {
                        HighestThresholdValue3 = ChocoboSettingsModel.Instance.FreeThreshold;
                        return CompanionStance.Free;
                    }
                }
                if (highestThreshold1 == CompanionStance.Attacker || highestThreshold2 == CompanionStance.Attacker)
                {
                    if (ChocoboSettingsModel.Instance.UseDefenderStance && !ChocoboSettingsModel.Instance.DefenderThresholdUseDrop &&
                        (!ChocoboSettingsModel.Instance.UseFreeStance || ChocoboSettingsModel.Instance.FreeThresholdUseDrop || ChocoboSettingsModel.Instance.DefenderThreshold >= ChocoboSettingsModel.Instance.FreeThreshold))
                    {
                        HighestThresholdValue3 = ChocoboSettingsModel.Instance.DefenderThreshold;
                        return CompanionStance.Defender;
                    }
                    if (ChocoboSettingsModel.Instance.UseFreeStance && !ChocoboSettingsModel.Instance.FreeThresholdUseDrop)
                    {
                        HighestThresholdValue3 = ChocoboSettingsModel.Instance.FreeThreshold;
                        return CompanionStance.Free;
                    }
                }
                if (highestThreshold1 == CompanionStance.Free || highestThreshold2 == CompanionStance.Free)
                {
                    if (ChocoboSettingsModel.Instance.UseDefenderStance && !ChocoboSettingsModel.Instance.DefenderThresholdUseDrop &&
                        (!ChocoboSettingsModel.Instance.UseAttackerStance || ChocoboSettingsModel.Instance.AttackerThresholdUseDrop || ChocoboSettingsModel.Instance.DefenderThreshold >= ChocoboSettingsModel.Instance.AttackerThreshold))
                    {
                        HighestThresholdValue3 = ChocoboSettingsModel.Instance.DefenderThreshold;
                        return CompanionStance.Defender;
                    }
                    if (ChocoboSettingsModel.Instance.UseAttackerStance && !ChocoboSettingsModel.Instance.AttackerThresholdUseDrop)
                    {
                        HighestThresholdValue3 = ChocoboSettingsModel.Instance.AttackerThreshold;
                        return CompanionStance.Attacker;
                    }
                }
            }
            if (highestThreshold1 == CompanionStance.Defender || highestThreshold2 == CompanionStance.Defender)
            {
                if (highestThreshold1 == CompanionStance.Attacker || highestThreshold2 == CompanionStance.Attacker)
                {
                    if (ChocoboSettingsModel.Instance.UseHealerStance && !ChocoboSettingsModel.Instance.HealerThresholdUseDrop &&
                        (!ChocoboSettingsModel.Instance.UseFreeStance || ChocoboSettingsModel.Instance.FreeThresholdUseDrop || ChocoboSettingsModel.Instance.HealerThreshold >= ChocoboSettingsModel.Instance.FreeThreshold))
                    {
                        HighestThresholdValue3 = ChocoboSettingsModel.Instance.HealerThreshold;
                        return CompanionStance.Healer;
                    }
                    if (ChocoboSettingsModel.Instance.UseFreeStance && !ChocoboSettingsModel.Instance.FreeThresholdUseDrop)
                    {
                        HighestThresholdValue3 = ChocoboSettingsModel.Instance.FreeThreshold;
                        return CompanionStance.Free;
                    }
                }
                if (highestThreshold1 == CompanionStance.Free || highestThreshold2 == CompanionStance.Free)
                {
                    if (ChocoboSettingsModel.Instance.UseHealerStance && !ChocoboSettingsModel.Instance.HealerThresholdUseDrop &&
                        (!ChocoboSettingsModel.Instance.UseAttackerStance || ChocoboSettingsModel.Instance.AttackerThresholdUseDrop || ChocoboSettingsModel.Instance.HealerThreshold >= ChocoboSettingsModel.Instance.AttackerThreshold))
                    {
                        HighestThresholdValue3 = ChocoboSettingsModel.Instance.HealerThreshold;
                        return CompanionStance.Healer;
                    }
                    if (ChocoboSettingsModel.Instance.UseAttackerStance && !ChocoboSettingsModel.Instance.AttackerThresholdUseDrop)
                    {
                        HighestThresholdValue3 = ChocoboSettingsModel.Instance.AttackerThreshold;
                        return CompanionStance.Attacker;
                    }
                }
            }
            if (highestThreshold1 == CompanionStance.Attacker || highestThreshold2 == CompanionStance.Attacker)
            {
                if (highestThreshold1 == CompanionStance.Free || highestThreshold2 == CompanionStance.Free)
                {
                    if (ChocoboSettingsModel.Instance.UseHealerStance && !ChocoboSettingsModel.Instance.HealerThresholdUseDrop &&
                        (!ChocoboSettingsModel.Instance.UseDefenderStance || ChocoboSettingsModel.Instance.DefenderThresholdUseDrop || ChocoboSettingsModel.Instance.HealerThreshold >= ChocoboSettingsModel.Instance.DefenderThreshold))
                    {
                        HighestThresholdValue3 = ChocoboSettingsModel.Instance.HealerThreshold;
                        return CompanionStance.Healer;
                    }
                    if (ChocoboSettingsModel.Instance.UseDefenderStance && !ChocoboSettingsModel.Instance.DefenderThresholdUseDrop)
                    {
                        HighestThresholdValue3 = ChocoboSettingsModel.Instance.DefenderThreshold;
                        return CompanionStance.Defender;
                    }
                }
            }
            HighestThresholdValue3 = 100;
            return 0;
        }

        private static CompanionStance HighestThreshold4()
        {
            if (highestThreshold1 != CompanionStance.Healer && highestThreshold2 != CompanionStance.Healer && highestThreshold3 != CompanionStance.Healer)
            {
                if (ChocoboSettingsModel.Instance.UseHealerStance && !ChocoboSettingsModel.Instance.HealerThresholdUseDrop)
                {
                    HighestThresholdValue4 = ChocoboSettingsModel.Instance.HealerThreshold;
                    return CompanionStance.Healer;
                }
            }
            if (highestThreshold1 != CompanionStance.Defender && highestThreshold2 != CompanionStance.Defender && highestThreshold3 != CompanionStance.Defender)
            {
                if (ChocoboSettingsModel.Instance.UseDefenderStance && !ChocoboSettingsModel.Instance.DefenderThresholdUseDrop)
                {
                    HighestThresholdValue4 = ChocoboSettingsModel.Instance.DefenderThreshold;
                    return CompanionStance.Defender;
                }
            }
            if (highestThreshold1 != CompanionStance.Attacker && highestThreshold2 != CompanionStance.Attacker && highestThreshold3 != CompanionStance.Attacker)
            {
                if (ChocoboSettingsModel.Instance.UseAttackerStance && !ChocoboSettingsModel.Instance.AttackerThresholdUseDrop)
                {
                    HighestThresholdValue4 = ChocoboSettingsModel.Instance.AttackerThreshold;
                    return CompanionStance.Attacker;
                }
            }
            if (highestThreshold1 != CompanionStance.Free && highestThreshold2 != CompanionStance.Free && highestThreshold3 != CompanionStance.Free)
            {
                if (ChocoboSettingsModel.Instance.UseFreeStance && !ChocoboSettingsModel.Instance.FreeThresholdUseDrop)
                {
                    HighestThresholdValue4 = ChocoboSettingsModel.Instance.FreeThreshold;
                    return CompanionStance.Free;
                }
            }
            HighestThresholdValue4 = 100;
            return 0;
        }

        #endregion LoadThresholds
    }
}