using EFT.InventoryLogic;
using RootMotion.FinalIK;
using SPT.Reflection.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace ProgressiveFleaMarket.Classes
{
    internal class FleaMarketDecider
    {
        public static bool CanItemBeListed(Item item, int PlayerLevel)
        {
            return PlayerLevel >= FleaMarketDecider.GetLevelForItem(item);
        }

        public static int GetLevelForItem(Item item)
        {
            int itemLevelRequired = 1;

            if (item is ArmoredEquipmentItemClass) itemLevelRequired = Math.Max(GetLevelForArmorItem((ArmoredEquipmentItemClass)item), itemLevelRequired);

            if (item is ArmorPlateItemClass) itemLevelRequired = Math.Max(GetLevelForArmorPlate((ArmorPlateItemClass)item), itemLevelRequired);

            if (item is KeyItemClass) itemLevelRequired = Math.Max(GetLevelForKey((KeyItemClass)item), itemLevelRequired);

            if (item is BackpackItemClass) itemLevelRequired = Math.Max(GetLevelForBackpack((BackpackItemClass)item), itemLevelRequired);

            if (item is AmmoItemClass) itemLevelRequired = Math.Max(GetLevelForAmmo((AmmoItemClass)item), itemLevelRequired);

            if (item is ThrowWeapItemClass) itemLevelRequired = Math.Max(GetLevelForGrenade((ThrowWeapItemClass)item), itemLevelRequired);

            Plugin.PGMConfig.ForcedLevels.TryGetValue(item.TemplateId, out int ForcedLevel);

            foreach (var item1 in Plugin.PGMConfig.ForcedLevels)
            {
                if (item.Template.IsChildOf(item1.Key))
                {
                    ForcedLevel = item1.Value;
                }
            }

            if (ForcedLevel > 0) itemLevelRequired = ForcedLevel;

            return itemLevelRequired;
        }
        
        private static int GetLevelForArmorItem(ArmoredEquipmentItemClass item)
        {
            if (item.Armor == null) return 1;

            int LevelRequired = 1;

            foreach (var item1 in Plugin.PGMConfig.ArmorClassLevels)
            {
                if (item.Armor.Template.ArmorClass == item1.Class)
                {
                    LevelRequired = item1.LevelRequired;
                    break;
                }
            }

            foreach (var item1 in Plugin.PGMConfig.ArmorZoneMultipliers)
            {
                foreach (var item2 in item.Armor.Template.ArmorColliders)
                {
                    if (item2.ToString().ToLower().Contains(item1.Key.ToLower()))
                    {
                        LevelRequired *= item1.Value;
                        break;
                    }
                }

            }

            LevelRequired = Math.Min(LevelRequired, 50);

            return LevelRequired;
        }

        private static int GetLevelForArmorPlate(ArmorPlateItemClass item)
        {
            int LevelRequired = 1;

            foreach (var item1 in Plugin.PGMConfig.ArmorClassLevels)
            {
                if (item.Armor.Template.ArmorClass == item1.Class)
                {
                    LevelRequired = item1.LevelRequired;
                    break;
                }
            }

            foreach (var item1 in item.Armor.GetArmorPlateColliders())
            {
                if (item1.ToString().ToLower().Contains("side"))
                {
                    LevelRequired /= 3; // Makes side plates less intense against the level requirement
                    break;
                }
            }

            return LevelRequired;
        }

        private static bool IsValueWithinRange(int X, int Min, int Max)
        {
            return X >= Min && X <= Max;
        }

        private static void GetLevelRequiredFromRangeLevel(int Value, Plugin.RangeLevelInfo[] RangeInfos, out int LevelRequired)
        {
            foreach (var item in RangeInfos)
            {
                if (IsValueWithinRange(Value, item.RangeStart, item.RangeEnd))
                {
                    LevelRequired = item.LevelRequired;
                    return;
                }
            }

            LevelRequired = 1;
        }

        private static int GetLevelForBackpack(BackpackItemClass item)
        {
            int ContainerSize = item.Grids.Sum(new Func<StashGridClass, int>((x) => x.GridWidth * x.GridHeight));

            GetLevelRequiredFromRangeLevel(ContainerSize, Plugin.PGMConfig.BackpackSlotCount, out int LevelRequired);

            return LevelRequired;
        }

        private static int GetLevelForAmmo(AmmoItemClass item)
        {
            int Penetration = item.PenetrationPower;

            GetLevelRequiredFromRangeLevel(Penetration, Plugin.PGMConfig.AmmoPenLevels, out int LevelRequired);

            return LevelRequired;
        }

        private static int GetLevelForGrenade(ThrowWeapItemClass item)
        {
            if (item.MinTimeToContactExplode != -1) return 30; // This is an impact grenade

            if (item.ThrowType == ThrowWeapType.frag_grenade) return 15;

            return 1;
        }

        private static int GetLevelForKey(KeyItemClass item)
        {
            return 1;
        }
    }
}
