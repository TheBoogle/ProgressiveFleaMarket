using EFT.InventoryLogic;
using EFT.UI;
using EFT.UI.Ragfair;
using ProgressiveFleaMarket.Classes;
using SPT.Reflection.Patching;
using SPT.Reflection.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace ProgressiveFleaMarket.Patches
{
    internal class PurchaseButtonPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return typeof(OfferView).GetMethod("method_10", BindingFlags.Public | BindingFlags.Instance);
        }

        [PatchPrefix]
        private static bool Prefix(OfferView __instance, ref bool __result)
        {
            Offer offer = __instance.Offer_0;
            Item item = offer.Item;

            if (offer.User.MemberType == EMemberCategory.Trader)
            {
                return true;
            }

            var PurchaseButtonField = typeof(OfferView).GetField("_purchaseButton", BindingFlags.NonPublic | BindingFlags.Instance);
            var LockedButtonField = typeof(OfferView).GetField("_lockedButton", BindingFlags.NonPublic | BindingFlags.Instance);
            var HoverTooltipField = typeof(OfferView).GetField("_hoverTooltipArea", BindingFlags.NonPublic | BindingFlags.Instance);
            var ItemUiContextField = typeof(OfferView).GetField("itemUiContext_0", BindingFlags.NonPublic | BindingFlags.Instance);
            var CanvasGroupField = typeof(OfferView).GetField("_canvasGroup", BindingFlags.NonPublic | BindingFlags.Instance);

            DefaultUIButton PurchaseButton = (DefaultUIButton)PurchaseButtonField.GetValue(__instance);
            GameObject LockedButton = (GameObject)LockedButtonField.GetValue(__instance);
            HoverTooltipAreaClick HoverTooltipArea = (HoverTooltipAreaClick)HoverTooltipField.GetValue(__instance);
            ItemUiContext ItemUiContext = (ItemUiContext)ItemUiContextField.GetValue(__instance);
            CanvasGroup CanvasGroup = (CanvasGroup)CanvasGroupField.GetValue(__instance);

            int Level = PatchConstants.BackEndSession.Profile.Info.Level;

            bool CanBePurchased = ProgressiveFleaMarket.Classes.FleaMarketDecider.CanItemBeListed(item, Level);

            string error = string.Format("You must be level {0} to purchase {1}", FleaMarketDecider.GetLevelForItem(item), item.LocalizedShortName());

            if (item is CompoundItem)
            {
                (item as CompoundItem).TryGetLastForbiddenItem(out Item lastForbiddenItem);

                if (lastForbiddenItem != null)
                {
                    CanBePurchased = false;

                    error = string.Format("You must be level {0} to purchase {1}", FleaMarketDecider.GetLevelForItem(lastForbiddenItem), lastForbiddenItem.LocalizedShortName());
                }
            }

            if (!CanBePurchased)
            {
                HoverTooltipArea.Init(ItemUiContext.Tooltip, error, false);
                HoverTooltipArea.enabled = true;
                HoverTooltipArea.CanvasGroup.blocksRaycasts = true;
                PurchaseButton.GameObject.SetActive(false);
                LockedButton.SetActive(true);
                CanvasGroup.SetUnlockStatus(false);
                return false;
            }

            return true;
        }
    }
}
