using BepInEx;
using BepInEx.Configuration;
using RoR2;
using System.Security;
using System.Security.Permissions;

[module: UnverifiableCode]
[assembly: SecurityPermission(SecurityAction.RequestMinimum, SkipVerification = true)]

namespace Infinite_Shrine_of_the_Mountain
{
    [BepInDependency("com.bepis.r2api")]
    [BepInPlugin("com.OkIGotIt.Infinite_Shrine_of_the_Mountain", "Infinite Shrine of the Mountain", "1.0.1")]
    public class Infinite_Shrine_of_the_Mountain : BaseUnityPlugin
    {
        public static ConfigEntry<int> maxpurchase { get; set; }
        public static ConfigEntry<float> delaytime { get; set; }
        public void Awake()
        {
            maxpurchase = Config.Bind<int>("Shrine of the Mountain", "Max uses", 2147483647, "Changes how many times you can use Shrine of the Mountain");
            delaytime = Config.Bind<float>("Shrine of the Mountain", "Delay time", 2, "Changes the amount of time between Shrine of the Mountain uses");
            if (maxpurchase.Value < 0 || maxpurchase.Value > 2147483647)
                maxpurchase.Value = 2147483647;
            if (delaytime.Value < 0 || delaytime.Value > 2147483647)
                delaytime.Value = 2;
            On.RoR2.PurchaseInteraction.OnInteractionBegin += PurchaseInteraction_OnInteractionBegin;
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