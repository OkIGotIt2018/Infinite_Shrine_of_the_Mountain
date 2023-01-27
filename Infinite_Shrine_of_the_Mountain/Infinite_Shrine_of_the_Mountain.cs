using BepInEx;
using BepInEx.Configuration;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using RiskOfOptions;
using RiskOfOptions.OptionConfigs;
using RiskOfOptions.Options;
using RoR2;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Security;
using System.Security.Permissions;
using UnityEngine;

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
        public static ConfigEntry<float> ChanceIncrease { get; set; }
        public static ConfigEntry<float> MaxChance { get; set; }
        public static ConfigEntry<double> DiminishingRewards { get; set; }
        public static ConfigEntry<int> MaxRewards { get; set; }
        public static ConfigEntry<double> SpawnWeight { get; set; }
        public static ConfigEntry<int> MaxShrines { get; set; }
        public static ConfigEntry<double> HealthStat { get; set; }
        public static ConfigEntry<double> AttackSpeedStat { get; set; }
        public static ConfigEntry<double> MoveSpeedStat { get; set; }
        public static ConfigEntry<double> ArmorStat { get; set; }
        public static ConfigEntry<double> DamageStat { get; set; }
        public static ConfigEntry<double> RegenStat { get; set; }
        public static ConfigEntry<float> HealthStatMax { get; set; }
        public static ConfigEntry<float> AttackSpeedStatMax { get; set; }
        public static ConfigEntry<float> MoveSpeedStatMax { get; set; }
        public static ConfigEntry<float> ArmorStatMax { get; set; }
        public static ConfigEntry<float> DamageStatMax { get; set; }
        public static ConfigEntry<float> RegenStatMax { get; set; }
        public static ConfigEntry<float> BossMultiplier { get; set; }
        public static ConfigEntry<double> StageScaling { get; set; }
        public static ConfigEntry<double> LoopScaling { get; set; }
        public static bool IsTeleporterCharging = false;
        public static int two;
        public void Awake()
        {
            maxpurchase = Config.Bind<int>("Shrine of the Mountain", "Max uses", 2147483647, new ConfigDescription("Changes how many times you can use Shrine of the Mountain", new AcceptableValueRange<int>(0, 2147483647)));
            delaytime = Config.Bind<float>("Shrine of the Mountain", "Delay time", 2, new ConfigDescription("Changes the amount of time between Shrine of the Mountain uses", new AcceptableValueRange<float>(0, 2147483647)));
            ChanceIncrease = Config.Bind<float>("Teleporter Rewards", "ChanceIncrease", 5, new ConfigDescription("Inceases chance per Shrine to convert to red"));
            MaxChance = Config.Bind<float>("Teleporter Rewards", "MaxChance", 35, new ConfigDescription("Max chance to convert to red"));
            DiminishingRewards = Config.Bind<double>("Teleporter Rewards", "DiminishingRewards", 2.5, new ConfigDescription("Decreases chance to get another red after getting one"));
            MaxRewards = Config.Bind<int>("Teleporter Rewards", "MaxRewards", 10, new ConfigDescription("Max amount of red you can get from a teleporter event"));
            SpawnWeight = Config.Bind<double>("Enemy Difficulty", "SpawnWeight", 1.5, new ConfigDescription("Increases spawn rate of Shrine of the Mountain"));
            MaxShrines = Config.Bind<int>("Shrine Spawning", "MaxShrines", 1, new ConfigDescription("Maximum amount of Shrine of the Mountain can be in a level"));
            HealthStat = Config.Bind<double>("Enemy Difficulty", "Health Stat", 0.1, new ConfigDescription("Health Stat Multiplier"));
            AttackSpeedStat = Config.Bind<double>("Enemy Difficulty", "Attack Speed Stat", 0.1, new ConfigDescription("Attack Speed Stat Multiplier"));
            MoveSpeedStat = Config.Bind<double>("Enemy Difficulty", "Move Speed Stat", 0.1, new ConfigDescription("Move Speed Stat Multiplier"));
            ArmorStat = Config.Bind<double>("Enemy Difficulty", "Armor Stat", 0.1, new ConfigDescription("Armor Stat Multiplier"));
            DamageStat = Config.Bind<double>("Enemy Difficulty", "Damage Stat", 0.1, new ConfigDescription("Damage Stat Multiplier"));
            RegenStat = Config.Bind<double>("Enemy Difficulty", "Regen Stat", 0.1, new ConfigDescription("Regen Stat Multiplier"));
            HealthStatMax = Config.Bind<float>("Enemy Difficulty", "Health Stat Max", 5, new ConfigDescription("Health Stat Multiplier Max"));
            AttackSpeedStatMax = Config.Bind<float>("Enemy Difficulty", "Attack Speed Stat Max", 2, new ConfigDescription("Attack Speed Stat Multiplier Max"));
            MoveSpeedStatMax = Config.Bind<float>("Enemy Difficulty", "Move Speed Stat Max", 2, new ConfigDescription("Move Speed Stat Multiplier Max"));
            ArmorStatMax = Config.Bind<float>("Enemy Difficulty", "Armor Stat Max", 2, new ConfigDescription("Armor Stat Multiplier Max"));
            DamageStatMax = Config.Bind<float>("Enemy Difficulty", "Damage Stat Max", 5, new ConfigDescription("Damage Stat Multiplier Max"));
            RegenStatMax = Config.Bind<float>("Enemy Difficulty", "Regen Stat Max", 3, new ConfigDescription("Regen Stat Multiplier Max"));
            BossMultiplier = Config.Bind<float>("Enemy Difficulty", "Boss Multiplier", 2, new ConfigDescription("Increases boss stat multipliers over regular enemies"));
            StageScaling = Config.Bind<double>("Enemy Difficulty", "Stage Scaling", 0.2, new ConfigDescription("Multiplier that gets added over the span of 10 stages cleared"));
            LoopScaling = Config.Bind<double>("Enemy Difficulty", "Loop Scaling", 0.2, new ConfigDescription("Multiplier that gets added over the span of 2 loops cleared"));

            On.RoR2.PurchaseInteraction.OnInteractionBegin += PurchaseInteraction_OnInteractionBegin; 
            if (BepInEx.Bootstrap.Chainloader.PluginInfos.ContainsKey("com.rune580.riskofoptions"))
                RiskOfOptionsInGameConfig();
            On.RoR2.BossGroup.DropRewards += BossGroup_DropRewards;
            On.RoR2.TeleporterInteraction.FixedUpdate += TeleporterInteraction_FixedUpdate;
            Stage.onStageStartGlobal += Stage_onStageStartGlobal;
            DirectorCardCategorySelection.calcCardWeight += DirectorCardCategorySelection_calcCardWeight;
			InteractableSpawnCard ShrineSpawnCard = LegacyResourcesAPI.Load<InteractableSpawnCard>("SpawnCards/InteractableSpawnCard/iscShrineBoss");
			ShrineSpawnCard.maxSpawnsPerStage = MaxShrines.Value;
            WhoKnows();
        }
        //Hello There! Lines 92 to 380 are for the Shrine of the Mountain Rework,
        //Which will probably be split into it's own mod.
        //More than likely only the PurchaseInteraction.OnInteractionBegin and RiskOfOptionsInGameConfig methods will stay in this mod.
        //Also kinda hijacked DirectorCardCategorySelection.calcCardWeight from the game, oops! Also WhoKnows is a mess I'll fix soon.

        private void Stage_onStageStartGlobal(Stage obj)
        {
            IsTeleporterCharging = false;
        }

        public void WhoKnows()
        {
            IL.RoR2.CharacterBody.RecalculateStats += (il) =>
            {
                ILCursor c = new ILCursor(il);
                ILLabel dioend = il.DefineLabel(); //end label

                try
                {
                    c.Index = 0;

                    // find the section that the code will be injected					
                    c.GotoNext(
                        MoveType.Before,
                        x => x.MatchLdarg(0), // 1388	0D5A	ldarg.0
                        x => x.MatchLdsfld(typeof(RoR2Content.Buffs).GetField("Weak")), // 1389	0D5B	ldc.i4.s	0x21
                        x => x.MatchCallOrCallvirt<CharacterBody>("HasBuff") // 1390	0D5D	call	instance bool RoR2.CharacterBody::HasBuff(valuetype RoR2.BuffIndex)
                    );

                    if (c.Index != 0)
                    {
                        c.Index++;

                        // this block is just "If artifact isn't enabled, jump to dioend label". In an item case, this should be the part where you check if you have the items or not.
                        c.Emit(OpCodes.Ldarg_0);
                        c.EmitDelegate<Func<CharacterBody, bool>>((cb) =>
                        {
                            if (cb.teamComponent.teamIndex == TeamIndex.Monster && cb.inventory && two > 0)
                                return true;
                            return false;
                        });
                        c.Emit(OpCodes.Brfalse, dioend);

                        // this.maxHealth
                        c.Emit(OpCodes.Ldarg_0);
                        c.Emit(OpCodes.Ldarg_0);
                        c.Emit(OpCodes.Callvirt, typeof(CharacterBody).GetMethod("get_maxHealth")); // 1414 0DA3	call	instance float32 RoR2.CharacterBody::get_maxHealth()

                        // get the inventory count for the item, calculate multiplier, return a float value
                        // This is essentially `this.maxHealth *= multiplier;`
                        c.Emit(OpCodes.Ldarg_0);
                        c.EmitDelegate<Func<CharacterBody, float>>((cb) =>
                        {
                            float healthmultiplier = 1;
                            if (cb.inventory.GetItemCount(RoR2Content.Items.ShieldOnly) > 0)
                                return healthmultiplier;
                            if (IsTeleporterCharging)
                            {
                                if (cb.isBoss)
                                    healthmultiplier = Mathf.Min((float)(healthmultiplier * (1 + (HealthStat.Value * BossMultiplier.Value * two * Math.Min(StageScaling.Value / 10 * Run.instance.stageClearCount, StageScaling.Value) * Math.Min(LoopScaling.Value / 2 * Run.instance.loopClearCount, LoopScaling.Value)))), HealthStatMax.Value);
                                else
                                    healthmultiplier = Mathf.Min((float)(healthmultiplier * (1 + (HealthStat.Value * two * Math.Min(StageScaling.Value / 10 * Run.instance.stageClearCount, StageScaling.Value) * Math.Min(LoopScaling.Value / 2 * Run.instance.loopClearCount, LoopScaling.Value)))), HealthStatMax.Value);
                            }
                            return healthmultiplier;
                        });
                        c.Emit(OpCodes.Mul);
                        c.Emit(OpCodes.Callvirt, typeof(CharacterBody).GetMethod("set_maxHealth", BindingFlags.Instance | BindingFlags.NonPublic));

                        // this.maxShield
                        c.Emit(OpCodes.Ldarg_0);
                        c.Emit(OpCodes.Ldarg_0);
                        c.Emit(OpCodes.Callvirt, typeof(CharacterBody).GetMethod("get_maxShield")); // 1414 0DA3	call	instance float32 RoR2.CharacterBody::get_maxHealth()

                        // get the inventory count for the item, calculate multiplier, return a float value
                        // This is essentially `this.maxShield *= multiplier;`
                        c.Emit(OpCodes.Ldarg_0);
                        c.EmitDelegate<Func<CharacterBody, float>>((cb) =>
                        {
                            float shieldmultiplier = 1;
                            if (cb.inventory.GetItemCount(RoR2Content.Items.ShieldOnly) == 0)
                                return shieldmultiplier;
                            if (IsTeleporterCharging)
                            {
                                if (cb.isBoss)
                                    shieldmultiplier = Mathf.Min((float)(shieldmultiplier * (1 + (HealthStat.Value * BossMultiplier.Value * two * Math.Min(StageScaling.Value / 10 * Run.instance.stageClearCount, StageScaling.Value) * Math.Min(LoopScaling.Value / 2 * Run.instance.loopClearCount, LoopScaling.Value)))), HealthStatMax.Value);
                                else
                                    shieldmultiplier = Mathf.Min((float)(shieldmultiplier * (1 + (HealthStat.Value * two * Math.Min(StageScaling.Value / 10 * Run.instance.stageClearCount, StageScaling.Value) * Math.Min(LoopScaling.Value / 2 * Run.instance.loopClearCount, LoopScaling.Value)))), HealthStatMax.Value);
                            }
                            return shieldmultiplier;
                        });
                        c.Emit(OpCodes.Mul);
                        c.Emit(OpCodes.Callvirt, typeof(CharacterBody).GetMethod("set_maxShield", BindingFlags.Instance | BindingFlags.NonPublic));

                        // this.attackSpeed
                        c.Emit(OpCodes.Ldarg_0);
                        c.Emit(OpCodes.Ldarg_0);
                        c.Emit(OpCodes.Callvirt, typeof(CharacterBody).GetMethod("get_attackSpeed")); // 1426 0DC7	call	instance float32 RoR2.CharacterBody::get_attackSpeed()

                        // get the inventory count for the item, calculate multiplier, return a float value
                        // This is essentially `this.attackSpeed *= multiplier;`
                        c.Emit(OpCodes.Ldarg_0);
                        c.EmitDelegate<Func<CharacterBody, float>>((cb) =>
                        {
                            float attackSpeedmultiplier = 1;
                            if (IsTeleporterCharging)
                            {
                                if (cb.isBoss)
                                    attackSpeedmultiplier = Mathf.Min((float)(attackSpeedmultiplier * (1 + (AttackSpeedStat.Value * BossMultiplier.Value * two * Math.Min(StageScaling.Value / 10 * Run.instance.stageClearCount, StageScaling.Value) * Math.Min(LoopScaling.Value / 2 * Run.instance.loopClearCount, LoopScaling.Value)))), AttackSpeedStatMax.Value);
                                else
                                    attackSpeedmultiplier = Mathf.Min((float)(attackSpeedmultiplier * (1 + (AttackSpeedStat.Value * two * Math.Min(StageScaling.Value / 10 * Run.instance.stageClearCount, StageScaling.Value) * Math.Min(LoopScaling.Value / 2 * Run.instance.loopClearCount, LoopScaling.Value)))), AttackSpeedStatMax.Value);
                            }
                            return attackSpeedmultiplier;
                        });
                        c.Emit(OpCodes.Mul);
                        c.Emit(OpCodes.Callvirt, typeof(CharacterBody).GetMethod("set_attackSpeed", BindingFlags.Instance | BindingFlags.NonPublic));

                        // this.moveSpeed
                        c.Emit(OpCodes.Ldarg_0);
                        c.Emit(OpCodes.Ldarg_0);
                        c.Emit(OpCodes.Callvirt, typeof(CharacterBody).GetMethod("get_moveSpeed")); // 1406	0D8A	call	instance float32 RoR2.CharacterBody::get_moveSpeed()

                        // get the inventory count for the item, calculate multiplier, return a float value
                        // This is essentially `this.moveSpeed *= multiplier;`
                        c.Emit(OpCodes.Ldarg_0);
                        c.EmitDelegate<Func<CharacterBody, float>>((cb) =>
                        {
                            float moveSpeedmultiplier = 1;
                            if (IsTeleporterCharging)
                            {
                                if (cb.isBoss)
                                    moveSpeedmultiplier = Mathf.Min((float)(moveSpeedmultiplier * (1 + (MoveSpeedStat.Value * BossMultiplier.Value * two * Math.Min(StageScaling.Value / 10 * Run.instance.stageClearCount, StageScaling.Value) * Math.Min(LoopScaling.Value / 2 * Run.instance.loopClearCount, LoopScaling.Value)))), MoveSpeedStatMax.Value);
                                else
                                    moveSpeedmultiplier = Mathf.Min((float)(moveSpeedmultiplier * (1 + (MoveSpeedStat.Value * two * Math.Min(StageScaling.Value / 10 * Run.instance.stageClearCount, StageScaling.Value) * Math.Min(LoopScaling.Value / 2 * Run.instance.loopClearCount, LoopScaling.Value)))), MoveSpeedStatMax.Value);
                            }
                            return moveSpeedmultiplier;
                        });
                        c.Emit(OpCodes.Mul);
                        c.Emit(OpCodes.Callvirt, typeof(CharacterBody).GetMethod("set_moveSpeed", BindingFlags.Instance | BindingFlags.NonPublic));

                        // this.armor
                        c.Emit(OpCodes.Ldarg_0);
                        c.Emit(OpCodes.Ldarg_0);
                        c.Emit(OpCodes.Callvirt, typeof(CharacterBody).GetMethod("get_armor")); // 1438 0DE8	call	instance float32 RoR2.CharacterBody::get_armor()

                        // get the inventory count for the item, calculate multiplier, return a float value
                        // This is essentially `this.armor *= multiplier;`
                        c.Emit(OpCodes.Ldarg_0);
                        c.EmitDelegate<Func<CharacterBody, float>>((cb) =>
                        {
                            float armormultiplier = 1;
                            if (IsTeleporterCharging)
                            {
                                if (cb.isBoss)
                                    armormultiplier = Mathf.Min((float)(armormultiplier * (1 + (ArmorStat.Value * BossMultiplier.Value * two * Math.Min(StageScaling.Value / 10 * Run.instance.stageClearCount, StageScaling.Value) * Math.Min(LoopScaling.Value / 2 * Run.instance.loopClearCount, LoopScaling.Value)))), ArmorStatMax.Value);
                                else
                                    armormultiplier = Mathf.Min((float)(armormultiplier * (1 + (ArmorStat.Value * two * Math.Min(StageScaling.Value / 10 * Run.instance.stageClearCount, StageScaling.Value) * Math.Min(LoopScaling.Value / 2 * Run.instance.loopClearCount, LoopScaling.Value)))), ArmorStatMax.Value);
                            }
                            return armormultiplier;
                        });
                        c.Emit(OpCodes.Mul);
                        c.Emit(OpCodes.Callvirt, typeof(CharacterBody).GetMethod("set_armor", BindingFlags.Instance | BindingFlags.NonPublic));

                        // this.damage
                        c.Emit(OpCodes.Ldarg_0);
                        c.Emit(OpCodes.Ldarg_0);
                        c.Emit(OpCodes.Callvirt, typeof(CharacterBody).GetMethod("get_damage")); // 1444 0DFD	call	instance float32 RoR2.CharacterBody::get_damage()

                        // get the inventory count for the item, calculate multiplier, return a float value
                        // This is essentially `this.damage *= multiplier;`
                        c.Emit(OpCodes.Ldarg_0);
                        c.EmitDelegate<Func<CharacterBody, float>>((cb) =>
                        {
                            float damagemultiplier = 1;
                            if (IsTeleporterCharging)
                            {
                                if (cb.isBoss)
                                    damagemultiplier = Mathf.Min((float)(damagemultiplier * (1 + (DamageStat.Value * BossMultiplier.Value * two * Math.Min(StageScaling.Value / 10 * Run.instance.stageClearCount, StageScaling.Value) * Math.Min(LoopScaling.Value / 2 * Run.instance.loopClearCount, LoopScaling.Value)))), DamageStatMax.Value);
                                else
                                    damagemultiplier = Mathf.Min((float)(damagemultiplier * (1 + (DamageStat.Value * two * Math.Min(StageScaling.Value / 10 * Run.instance.stageClearCount, StageScaling.Value) * Math.Min(LoopScaling.Value / 2 * Run.instance.loopClearCount, LoopScaling.Value)))), DamageStatMax.Value);
                            }
                            return damagemultiplier;
                        });
                        c.Emit(OpCodes.Mul);
                        c.Emit(OpCodes.Callvirt, typeof(CharacterBody).GetMethod("set_damage", BindingFlags.Instance | BindingFlags.NonPublic));

                        // this.regen
                        c.Emit(OpCodes.Ldarg_0);
                        c.Emit(OpCodes.Ldarg_0);
                        c.Emit(OpCodes.Callvirt, typeof(CharacterBody).GetMethod("get_regen")); // 1450 0E0F	call	instance float32 RoR2.CharacterBody::get_regen()

                        // get the inventory count for the item, calculate multiplier, return a float value
                        // This is essentially `this.regen *= multiplier;`
                        c.Emit(OpCodes.Ldarg_0);
                        c.EmitDelegate<Func<CharacterBody, float>>((cb) =>
                        {
                            float regenmultiplier = 1;
                            if (IsTeleporterCharging)
                            {
                                if (cb.isBoss)
                                    regenmultiplier = Mathf.Min((float)(regenmultiplier * (1 + (RegenStat.Value * BossMultiplier.Value * two * Math.Min(StageScaling.Value / 10 * Run.instance.stageClearCount, StageScaling.Value) * Math.Min(LoopScaling.Value / 2 * Run.instance.loopClearCount, LoopScaling.Value)))), RegenStatMax.Value);
                                else
                                    regenmultiplier = Mathf.Min((float)(regenmultiplier * (1 + (RegenStat.Value * two * Math.Min(StageScaling.Value / 10 * Run.instance.stageClearCount, StageScaling.Value) * Math.Min(LoopScaling.Value / 2 * Run.instance.loopClearCount, LoopScaling.Value)))), RegenStatMax.Value);
                            }
                            return regenmultiplier;
                        });
                        c.Emit(OpCodes.Mul);
                        c.Emit(OpCodes.Callvirt, typeof(CharacterBody).GetMethod("set_regen", BindingFlags.Instance | BindingFlags.NonPublic));

                        c.MarkLabel(dioend); // end label
                    }

                }
                catch (Exception ex) { base.Logger.LogError(ex); }
            };
        }

        private void DirectorCardCategorySelection_calcCardWeight(DirectorCard card, ref float weight)
		{
			if (card.spawnCard.name != "iscShrineBoss") return;
            weight *= (float)Math.Round(weight * SpawnWeight.Value);
            card.spawnCard.directorCreditCost = (int)Math.Round(card.spawnCard.directorCreditCost / SpawnWeight.Value);
        }

        private void TeleporterInteraction_FixedUpdate(On.RoR2.TeleporterInteraction.orig_FixedUpdate orig, TeleporterInteraction self)
        {
            orig(self);
            two = self.shrineBonusStacks;
            if (self.activationState == TeleporterInteraction.ActivationState.Charging)
                IsTeleporterCharging = true;
        }

		private void BossGroup_DropRewards(On.RoR2.BossGroup.orig_DropRewards orig, BossGroup self)
		{
			if (!Run.instance)
				return;
			if (self.rng == null)
				return;
			int participatingPlayerCount = Run.instance.participatingPlayerCount;
			if (participatingPlayerCount != 0)
			{
				if (self.dropPosition)
				{
					PickupIndex pickupIndex = PickupIndex.none;
					PickupIndex pickupIndexRed = PickupIndex.none;
					if (self.dropTable)
					{
						pickupIndex = self.dropTable.GenerateDrop(self.rng);
						List<PickupIndex> list = Run.instance.availableTier3DropList;
						pickupIndexRed = self.rng.NextElementUniform<PickupIndex>(list);
					}
					int num = 1 + self.bonusRewardCount;
					if (self.scaleRewardsByPlayerCount)
						num *= participatingPlayerCount;
					float angle = 360f / (float)num;
					Vector3 vector = Quaternion.AngleAxis((float)UnityEngine.Random.Range(0, 360), Vector3.up) * (Vector3.up * 40f + Vector3.forward * 5f);
					Quaternion rotation = Quaternion.AngleAxis(angle, Vector3.up);
					bool flag = self.bossDrops != null && self.bossDrops.Count > 0;
					bool flag2 = self.bossDropTables != null && self.bossDropTables.Count > 0;
					int i = 0;
					double baseChance = ChanceIncrease.Value * self.bonusRewardCount;
					int redCount = 0;
					while (i < num)
					{
						PickupIndex pickupIndex2 = pickupIndex;
						if (self.bossDrops != null && ((flag || flag2) && self.rng.nextNormalizedFloat <= self.bossDropChance))
						{
							if (flag2)
							{
								PickupDropTable pickupDropTable = self.rng.NextElementUniform<PickupDropTable>(self.bossDropTables);
								if (pickupDropTable != null)
									pickupIndex2 = pickupDropTable.GenerateDrop(self.rng);
							}
							else
								pickupIndex2 = self.rng.NextElementUniform<PickupIndex>(self.bossDrops);
						}
						double finalChance = baseChance;
						if (MaxChance.Value < baseChance)
							finalChance = MaxChance.Value;
						float tempfjisad = UnityEngine.Random.Range(0, 100);
						if (tempfjisad < finalChance && redCount < 10)
						{
							baseChance = baseChance - DiminishingRewards.Value;
							redCount++;
							PickupDropletController.CreatePickupDroplet(pickupIndexRed, self.dropPosition.position, vector);
						}
						else
							PickupDropletController.CreatePickupDroplet(pickupIndex2, self.dropPosition.position, vector);
						i++;
						vector = rotation * vector;
					}
					return;
				}
			}
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
                max = 600,
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
            if (BepInEx.Bootstrap.Chainloader.PluginInfos.ContainsKey("shbones.MoreMountains"))
                MoreMountainsCompatibilty(self);
        }

        private void MoreMountainsCompatibilty(PurchaseInteraction self)
        {
            if (self.displayNameToken.ToLower() == "interactable_amber_mountain_shrine_name" || self.displayNameToken.ToLower() == "interactable_crimson_mountain_shrine_name")
            {
                var bossBehavior = self.GetComponent<MoreMountains.Interactables.MoreMountainShrineBehavior>();
                bossBehavior.maxPurchaseCount = maxpurchase.Value;
                //bossBehavior.refreshTimer = delaytime.Value;
            }
        }
    }
}