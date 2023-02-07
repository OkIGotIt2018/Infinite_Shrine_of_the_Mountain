using BepInEx;
using BepInEx.Configuration;
using MonoMod.RuntimeDetour;
using RiskOfOptions;
using RiskOfOptions.OptionConfigs;
using RiskOfOptions.Options;
using RoR2;
using System;
using System.Reflection;
using System.Security;
using System.Security.Permissions;

[module: UnverifiableCode]
[assembly: SecurityPermission(SecurityAction.RequestMinimum, SkipVerification = true)]

namespace Infinite_Shrine_of_the_Mountain
{
    [BepInPlugin("com.OkIGotIt.Infinite_Shrine_of_the_Mountain", "Infinite Shrine of the Mountain", "1.0.4")]
    [BepInDependency("com.rune580.riskofoptions", BepInDependency.DependencyFlags.SoftDependency)]
    [BepInDependency("shbones.MoreMountains", BepInDependency.DependencyFlags.SoftDependency)]
    [BepInDependency("com.themysticsword.extrachallengeshrines", BepInDependency.DependencyFlags.SoftDependency)]
    public class Infinite_Shrine_of_the_Mountain : BaseUnityPlugin
    {
        public static ConfigEntry<int> maxpurchase { get; set; }
        public static ConfigEntry<float> delaytime { get; set; }
        public static ConfigEntry<float> spawnweight { get; set; }
        public void Awake()
        {
            maxpurchase = Config.Bind<int>("Shrine of the Mountain", "Max uses", 2147483647, new ConfigDescription("Changes how many times you can use Shrine of the Mountain", new AcceptableValueRange<int>(0, 2147483647)));
            delaytime = Config.Bind<float>("Shrine of the Mountain", "Delay time", 2, new ConfigDescription("Changes the amount of time between Shrine of the Mountain uses", new AcceptableValueRange<float>(0, 2147483647)));
            spawnweight = Config.Bind<float>("Shrine of the Mountain", "SpawnWeight", 1.5f, new ConfigDescription("Increases spawn rate of Shrine of the Mountain"));

            //On.RoR2.PurchaseInteraction.OnInteractionBegin += PurchaseInteraction_OnInteractionBegin;
            Hook PurchaseInteraction_OnInteractionBegin = new Hook(typeof(PurchaseInteraction).GetMethod("OnInteractionBegin", BindingFlags.Public | BindingFlags.Instance), typeof(Infinite_Shrine_of_the_Mountain).GetMethod("PurchaseInteraction_OnInteractionBegin"));
            if (BepInEx.Bootstrap.Chainloader.PluginInfos.ContainsKey("com.rune580.riskofoptions"))
                RiskOfOptionsInGameConfig();
            DirectorCardCategorySelection.calcCardWeight += DirectorCardCategorySelection_calcCardWeight;
		}

        private void DirectorCardCategorySelection_calcCardWeight(DirectorCard card, ref float weight)
		{
			if (card.spawnCard.name != "iscShrineBoss") return;
            weight *= (float)Math.Round(weight * spawnweight.Value);
            card.spawnCard.directorCreditCost = (int)Math.Round(card.spawnCard.directorCreditCost / spawnweight.Value);
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
            ModSettingsManager.AddOption(new StepSliderOption(spawnweight, new StepSliderConfig
            {
                min = 0f,
                max = 10f,
                increment = 0.01f
            }));
        }

        public static void PurchaseInteraction_OnInteractionBegin(Action<PurchaseInteraction, Interactor> orig, PurchaseInteraction self, Interactor activator)
        {
            orig(self, activator);
            if (self.displayNameToken.ToLower() == "shrine_boss_name")
            {
                var bossBehavior = self.GetComponent<ShrineBossBehavior>();
                bossBehavior.maxPurchaseCount = maxpurchase.Value;
                bossBehavior.refreshTimer = delaytime.Value;
            }
            if (BepInEx.Bootstrap.Chainloader.PluginInfos.ContainsKey("shbones.MoreMountains"))
                MoreMountainsCompatibilty(self);
            if (BepInEx.Bootstrap.Chainloader.PluginInfos.ContainsKey("com.themysticsword.extrachallengeshrines"))
                ExtraChallengeShrinesCompatibilty(self);
        }

        private static void MoreMountainsCompatibilty(PurchaseInteraction self)
        {
            if (self.displayNameToken.ToLower() == "interactable_amber_mountain_shrine_name" || self.displayNameToken.ToLower() == "interactable_crimson_mountain_shrine_name")
            {
                var bossBehavior = self.GetComponent<MoreMountains.Interactables.MoreMountainShrineBehavior>();
                bossBehavior.maxPurchaseCount = maxpurchase.Value;
                bossBehavior.refreshTimer = delaytime.Value;
            }
        }
        private static void ExtraChallengeShrinesCompatibilty(PurchaseInteraction self)
        {
            if (self.displayNameToken.ToLower() == "extrachallengeshrines_shrine_crown_name")
            {
                var bossBehavior = self.GetComponent<ExtraChallengeShrines.Interactables.ExtraChallengeShrinesShrineCrownBehaviour>();
                bossBehavior.maxPurchaseCount = maxpurchase.Value;
                bossBehavior.refreshTimer = delaytime.Value;
            }
            if (self.displayNameToken.ToLower() == "extrachallengeshrines_shrine_eye_name")
            {
                var bossBehavior = self.GetComponent<ExtraChallengeShrines.Interactables.ExtraChallengeShrinesShrineEyeBehaviour>();
                bossBehavior.maxPurchaseCount = maxpurchase.Value;
                bossBehavior.refreshTimer = delaytime.Value;
            }
            if (self.displayNameToken.ToLower() == "extrachallengeshrines_shrine_rock_name")
            {
                var bossBehavior = self.GetComponent<ExtraChallengeShrines.Interactables.ExtraChallengeShrinesShrineRockBehaviour>();
                bossBehavior.maxPurchaseCount = maxpurchase.Value;
                bossBehavior.refreshTimer = delaytime.Value;
            }
        }
    }
}