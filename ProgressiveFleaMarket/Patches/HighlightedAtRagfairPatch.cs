using EFT.InventoryLogic;
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
    internal class HighlightedAtRagfairPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return typeof(RagfairOfferSellHelperClass).GetMethod("HighlightedAtRagfair", BindingFlags.Public | BindingFlags.Instance);
        }

        [PatchPrefix]
        private static bool Prefix(Item item, ref bool __result)
        {
            bool CanBeListed = ProgressiveFleaMarket.Classes.FleaMarketDecider.CanItemBeListed(item, PatchConstants.BackEndSession.Profile.Info.Level);

            if (!CanBeListed)
            {
                __result = false;
                return false;
            }

            return true;
        }
    }
}
