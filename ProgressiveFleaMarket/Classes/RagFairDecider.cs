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

            switch (item)
            {
                case ArmorPlateItemClass armorPlateItem:
                    itemLevelRequired = Math.Max(GetLevelForArmorPlate(armorPlateItem), itemLevelRequired);
                    break;
                case ArmoredEquipmentItemClass armoredItem:
                    itemLevelRequired = Math.Max(GetLevelForArmorItem(armoredItem), itemLevelRequired);
                    break;
                case KeyItemClass keyItem:
                    itemLevelRequired = Math.Max(GetLevelForKey(keyItem), itemLevelRequired);
                    break;
                case BackpackItemClass backpackItem:
                    itemLevelRequired = Math.Max(GetLevelForBackpack(backpackItem), itemLevelRequired);
                    break;
                case AmmoItemClass ammoItem:
                    itemLevelRequired = Math.Max(GetLevelForAmmo(ammoItem), itemLevelRequired);
                    break;
                case ThrowWeapItemClass throwWeaponItem:
                    itemLevelRequired = Math.Max(GetLevelForGrenade(throwWeaponItem), itemLevelRequired);
                    break;
            }

            Plugin.PGMConfig.ForcedLevels.TryGetValue(item.TemplateId, out int forcedLevel);

            foreach (var forcedLevelEntry in Plugin.PGMConfig.ForcedLevels)
            {
                if (item.Template.IsChildOf(forcedLevelEntry.Key))
                {
                    forcedLevel = forcedLevelEntry.Value;
                }
            }

            if (forcedLevel > 0)
            {
                itemLevelRequired = forcedLevel;
            }

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
