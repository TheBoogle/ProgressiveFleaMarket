using Diz.LanguageExtensions;
using EFT.InventoryLogic;
using ProgressiveFleaMarket.Classes;
using SPT.Reflection.Patching;
using SPT.Reflection.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace ProgressiveFleaMarket.Patches
{
    internal class TryGetLastForbiddenItemPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return typeof(CompoundItem).GetMethod("TryGetLastForbiddenItem", BindingFlags.Public | BindingFlags.Instance);
        }

        [PatchPrefix]
        private static bool Prefix(CompoundItem __instance, out Item item, bool __result)
        {
            var ContainedItemsField = typeof(CompoundItem).GetField("_containedItems", BindingFlags.NonPublic | BindingFlags.Static);

            List<Item> ContainedItems = (List<Item>)ContainedItemsField.GetValue(null);

            ContainedItems.Clear();
            __instance.GetAllItemsNonAlloc(ContainedItems, false, false);
            for (int i = ContainedItems.Count - 1; i >= 0; i--)
            {
                bool CanBeListed = FleaMarketDecider.CanItemBeListed(ContainedItems[i], PatchConstants.BackEndSession.Profile.Info.Level);

                if (CanBeListed)
                {
                    ContainedItems.RemoveAt(i);
                }
            }
            if (ContainedItems.Count == 0)
            {
                item = null;
                __result = false;
            }
            item = ContainedItems.LastOrDefault<Item>();
            ContainedItems.Clear();

            __result = false;

            return false;
        }
    }
}
