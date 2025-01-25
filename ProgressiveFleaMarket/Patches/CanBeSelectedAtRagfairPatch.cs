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
    internal class CanBeSelectedAtRagfairPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return typeof(RagFairClass).GetMethod("CanBeSelectedAtRagfair", BindingFlags.Public | BindingFlags.Static);
        }

        [PatchPrefix]
        private static bool Prefix(Item item, TraderControllerClass itemController, out string error, ref bool __result)
        {
            bool CanBeListed = FleaMarketDecider.CanItemBeListed(item, PatchConstants.BackEndSession.Profile.Info.Level);

            if (!CanBeListed)
            {
                error = string.Format("You must be level {0} to list {1}", FleaMarketDecider.GetLevelForItem(item), item.LocalizedShortName());
                __result = false;

                return false;
            } else
            {
                error = null;
                __result = true;
            }

            return true;
        }
    }
}
