using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Commands;
using CounterStrikeSharp.API.Modules.Utils;
using StoreApi;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Store_AdBonus
{
    public class Store_AdBonusConfig : BasePluginConfig
    {
        [JsonPropertyName("ad_texts")]
        public List<string> AdTexts { get; set; } = ["YourAd1", "YourAd2"];

        [JsonPropertyName("bonus_credits")]
        public int BonusCredits { get; set; } = 100;

        [JsonPropertyName("interval_in_seconds")]
        public int IntervalSeconds { get; set; } = 300;
    }

    public class Store_AdBonus : BasePlugin, IPluginConfig<Store_AdBonusConfig>
    {
        public override string ModuleName => "Store Module [Name Bonus]";
        public override string ModuleVersion => "0.0.1";
        public override string ModuleAuthor => "Nathy";

        private IStoreApi? storeApi;
        private float intervalInSeconds;
        public Store_AdBonusConfig Config { get; set; } = null!;

        public override void OnAllPluginsLoaded(bool hotReload)
        {
            storeApi = IStoreApi.Capability.Get();

            if (storeApi == null)
            {
                return;
            }

            intervalInSeconds = Config.IntervalSeconds;
            StartCreditTimer();
        }

        public void OnConfigParsed(Store_AdBonusConfig config)
        {
            Config = config;
        }

        private void StartCreditTimer()
        {
            AddTimer(intervalInSeconds, () =>
            {
                GrantCreditsToEligiblePlayers();
                StartCreditTimer();
            });
        }

        private void GrantCreditsToEligiblePlayers()
        {
            var players = Utilities.GetPlayers();

            foreach (var player in players)
            {
                if (player != null && !player.IsBot && player.IsValid)
                {
                    foreach (var adText in Config.AdTexts)
                    {
                        if (player.PlayerName.Contains(adText))
                        {
                            storeApi?.GivePlayerCredits(player, Config.BonusCredits);
                            player.PrintToChat(Localizer["Prefix"] + Localizer["You have been awarded", Config.BonusCredits, adText]);
                            break;
                        }
                    }
                }
            }
        }
    }
}
