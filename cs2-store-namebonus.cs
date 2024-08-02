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
        public List<string> AdTexts { get; set; } = new List<string> { "YourAd1", "YourAd2" };

        [JsonPropertyName("bonus_credits")]
        public int BonusCredits { get; set; } = 100;

        [JsonPropertyName("interval_in_seconds")]
        public int IntervalSeconds { get; set; } = 300;

        [JsonPropertyName("show_ad_message")]
        public bool ShowAdMessage { get; set; } = true;

        [JsonPropertyName("ad_message_delay_seconds")]
        public int AdMessageDelaySeconds { get; set; } = 120;

        [JsonPropertyName("ad_message")]
        public string AdMessage { get; set; } = "Add '{blue}YourAd{white}' to your nickname and earn bonus credits!";
    }

    public class Store_AdBonus : BasePlugin, IPluginConfig<Store_AdBonusConfig>
    {
        public override string ModuleName => "Store Module [Name Bonus]";
        public override string ModuleVersion => "0.0.1";
        public override string ModuleAuthor => "Nathy";

        private IStoreApi? storeApi;
        private float intervalInSeconds;
        public Store_AdBonusConfig Config { get; set; } = null!;

        private static readonly Dictionary<string, char> ColorMap = new Dictionary<string, char>
        {
            { "{default}", ChatColors.Default },
            { "{white}", ChatColors.White },
            { "{darkred}", ChatColors.DarkRed },
            { "{green}", ChatColors.Green },
            { "{lightyellow}", ChatColors.LightYellow },
            { "{lightblue}", ChatColors.LightBlue },
            { "{olive}", ChatColors.Olive },
            { "{lime}", ChatColors.Lime },
            { "{red}", ChatColors.Red },
            { "{lightpurple}", ChatColors.LightPurple },
            { "{purple}", ChatColors.Purple },
            { "{grey}", ChatColors.Grey },
            { "{yellow}", ChatColors.Yellow },
            { "{gold}", ChatColors.Gold },
            { "{silver}", ChatColors.Silver },
            { "{blue}", ChatColors.Blue },
            { "{darkblue}", ChatColors.DarkBlue },
            { "{bluegrey}", ChatColors.BlueGrey },
            { "{magenta}", ChatColors.Magenta },
            { "{lightred}", ChatColors.LightRed },
            { "{orange}", ChatColors.Orange }
        };

        public override void OnAllPluginsLoaded(bool hotReload)
        {
            storeApi = IStoreApi.Capability.Get();

            if (storeApi == null)
            {
                return;
            }

            intervalInSeconds = Config.IntervalSeconds;
            StartCreditTimer();

            if (Config.ShowAdMessage)
            {
                StartAdMessageTimer();
            }
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

        private void StartAdMessageTimer()
        {
            AddTimer(Config.AdMessageDelaySeconds, () =>
            {
                BroadcastAdMessage();
                StartAdMessageTimer();
            });
        }

        private void BroadcastAdMessage()
        {
            var message = ReplaceColorPlaceholders(Config.AdMessage);
            var players = Utilities.GetPlayers();
            foreach (var player in players)
            {
                if (player != null && !player.IsBot && player.IsValid)
                {
                    player.PrintToChat(Localizer["Prefix"] + message);
                }
            }
        }

        private string ReplaceColorPlaceholders(string message)
        {
            if (!string.IsNullOrEmpty(message) && message[0] != ' ')
            {
                message = " " + message;
            }

            foreach (var colorPlaceholder in ColorMap)
            {
                message = message.Replace(colorPlaceholder.Key, colorPlaceholder.Value.ToString());
            }

            return message;
        }
    }
}
