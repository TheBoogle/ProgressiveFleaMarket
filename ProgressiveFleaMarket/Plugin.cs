using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using Newtonsoft.Json;
using ProgressiveFleaMarket.Patches;
using SPT.Common.Http;
using SPT.Common.Utils;
using SPT.Reflection.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine.Diagnostics;

namespace ProgressiveFleaMarket
{
    [BepInPlugin("com.boogle.progressivefleamarket", "Progressive Flea Market", "1.0.0")]
    public class Plugin : BaseUnityPlugin
    {
        public interface IFleaMarketInfo { }

        public class ArmorClassInfo : IFleaMarketInfo
        {
            public int Class { get; set; }
            public int LevelRequired { get; set; }
        }

        public class RangeLevelInfo : IFleaMarketInfo
        {
            public int RangeStart { get; set; }
            public int RangeEnd { get; set; }
            public int LevelRequired { get; set; }
        }

        public class ProgressFleaMarketConfig : IFleaMarketInfo
        {
            public Dictionary<string, int> ForcedLevels { get; set; }
            public Dictionary<string, int> ArmorZoneMultipliers { get; set; }
            public string[] BannedFleaMarketItems { get; set; }

            public ArmorClassInfo[] ArmorClassLevels { get; set; }
            public RangeLevelInfo[] AmmoPenLevels { get; set; }
            public RangeLevelInfo[] BackpackSlotCount { get; set; }
        }

        public static ProgressFleaMarketConfig PGMConfig;

        private static T UpdateInfoFromServer<T>(string route) where T : class, IFleaMarketInfo
        {
            var json = RequestHandler.GetJson(route);

            return JsonConvert.DeserializeObject<T>(json);
        }
        public void Awake()
        {
            try
            {
                PGMConfig = UpdateInfoFromServer<ProgressFleaMarketConfig>("/ProgressiveFleaMarket/GetConfig");
                new CanBeSelectedAtRagfairPatch().Enable();
                new HighlightedAtRagfairPatch().Enable();
                new PurchaseButtonPatch().Enable();
                new TryGetLastForbiddenItemPatch().Enable();
            }
            catch (Exception ex)
            {
                Logger.LogError($"A PATCH IN {GetType().Name} FAILED. SUBSEQUENT PATCHES HAVE NOT LOADED");
                Logger.LogError($"{GetType().Name}: {ex}");
                throw;
            }
            Logger.LogInfo("Progressive Flea Market loaded!");
        }
    }
}
