using BepInEx;
using BepInEx.Configuration;
using RiskOfOptions;
using RiskOfOptions.OptionConfigs;
using RiskOfOptions.Options;
using RoR2;
using System;
using System.Security;
using System.Security.Permissions;

[module: UnverifiableCode]
[assembly: SecurityPermission(SecurityAction.RequestMinimum, SkipVerification = true)]

namespace Infinite_Shrine_of_the_Mountain
{
    [BepInPlugin("com.OkIGotIt.Infinite_Shrine_of_the_Mountain", "Infinite Shrine of the Mountain", "1.0.3")]
    [BepInDependency("com.rune580.riskofoptions", BepInDependency.DependencyFlags.SoftDependency)]
    public class Infinite_Shrine_of_the_Mountain : BaseUnityPlugin
    {
        public static ConfigEntry<int> maxpurchase { get; set; }
        public static ConfigEntry<float> delaytime { get; set; }
        public void Awake()
        {
            maxpurchase = Config.Bind<int>("Shrine of the Mountain", "Max uses", 2147483647, new ConfigDescription("Changes how many times you can use Shrine of the Mountain", new AcceptableValueRange<int>(0, 2147483647)));
            delaytime = Config.Bind<float>("Shrine of the Mountain", "Delay time", 2, new ConfigDescription("Changes the amount of time between Shrine of the Mountain uses", new AcceptableValueRange<float>(0, 2147483647)));

            On.RoR2.PurchaseInteraction.OnInteractionBegin += PurchaseInteraction_OnInteractionBegin;
            if (BepInEx.Bootstrap.Chainloader.PluginInfos.ContainsKey("com.rune580.riskofoptions"))
                RiskOfOptionsInGameConfig();
		}

        public static void RiskOfOptionsInGameConfig()
        {
            ModSettingsManager.AddOption(new IntSliderOption(maxpurchase, new IntSliderConfig
            {
                min = 0,
                max = 250
            }));
            ModSettingsManager.AddOption(new SliderOption(delaytime, new SliderConfig
            {
                min = 0f,
                max = 600f,
                formatString = "{0:0}s"
            }));
        }

        private void PurchaseInteraction_OnInteractionBegin(On.RoR2.PurchaseInteraction.orig_OnInteractionBegin orig, PurchaseInteraction self, Interactor activator)
        {
            orig(self, activator);
            if (self.displayNameToken.ToLower() == "shrine_boss_name")
            {
                var bossBehavior = self.GetComponent<ShrineBossBehavior>();
                bossBehavior.maxPurchaseCount = maxpurchase.Value;
                bossBehavior.refreshTimer = delaytime.Value;
            }
        }
    }
}