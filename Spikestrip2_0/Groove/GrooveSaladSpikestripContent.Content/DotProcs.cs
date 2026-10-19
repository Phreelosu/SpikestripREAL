using System;
using System.Collections.Generic;
using On.RoR2;
using R2API;
using RoR2;
using UnityEngine;
using UnityEngine.Networking;
using Spikestrip2_0.Core;

namespace GrooveSaladSpikestripContent.Content;

public class DotProcs : ItemBase<DotProcs>
{
	public class DotProcsBehaviourServer : RoR2.CharacterBody.ItemBehavior
	{
		public Dictionary<RoR2.DotController, RoR2.ProcChainMask[]> storedProcChainMasks = new Dictionary<RoR2.DotController, RoR2.ProcChainMask[]>();

		public void AddProcChainMask(RoR2.DotController dotController, RoR2.DotController.DotStack dotStack)
		{
			if (!storedProcChainMasks.TryGetValue(dotController, out var value))
			{
				value = new RoR2.ProcChainMask[RoR2.DotController.dotDefs.Length];
				storedProcChainMasks.Add(dotController, value);
			}
			ref RoR2.ProcChainMask reference = ref value[(int)dotStack.dotIndex];
			CompoundOrResetMask(ref reference, dotController, dotStack);
			if (dotStack.dotIndex == RoR2.DotController.DotIndex.Bleed)
			{
				reference.AddProc(ProcType.BleedOnHit);
			}
			if (dotStack.dotIndex == bloodOrangeDotIndex)
			{
				reference.AddProc(bloodOrangeProcType);
			}
			if (dotStack.dotIndex == RoR2.DotController.DotIndex.Fracture)
			{
				reference.AddProc(ProcType.FractureOnHit);
			}
		}

		public void CompoundOrResetMask(ref RoR2.ProcChainMask procChainMask, RoR2.DotController dotController, RoR2.DotController.DotStack dotStack)
		{
			for (int i = 0; i < dotController.dotStackList.Count; i++)
			{
				RoR2.DotController.DotStack dotStack2 = dotController.dotStackList[i];
				if (dotStack2.dotIndex == dotStack.dotIndex && dotStack2 != dotStack)
				{
					procChainMask.mask |= globalServerTrackedProcChainMask.mask;
					return;
				}
			}
			procChainMask.mask = globalServerTrackedProcChainMask.mask;
		}

		private void DotController_OnDestroy(On.RoR2.DotController.orig_OnDestroy orig, RoR2.DotController self)
		{
			if (storedProcChainMasks.ContainsKey(self))
			{
				storedProcChainMasks.Remove(self);
			}
			orig(self);
		}

		public void OnEnable()
		{
			On.RoR2.DotController.OnDestroy += DotController_OnDestroy;
		}

		public void OnDisable()
		{
			On.RoR2.DotController.OnDestroy -= DotController_OnDestroy;
		}
	}

	public static RoR2.ProcChainMask globalServerTrackedProcChainMask = default(RoR2.ProcChainMask);

	public static RoR2.BuffDef bloodOrangeBuff;

	public static RoR2.DotController.DotIndex bloodOrangeDotIndex;

	public static ProcType bloodOrangeProcType = ProcType.BleedOnHit;

	public static DamageColorIndex orangeDamageColorIndex = ColorsAPI.RegisterDamageColor(SpikestripUtil.ColorRGB(225f, 101f, 65f));

	public static GameObject dotProcImpactEffect;

	public override string ItemToken => "DOTPROCS";

	public override string ItemName => "Blood Orange";

	public override string ItemPickup => "Damage-over-time effects activate your 'On Hit' items. High damage hits also inflict damage-over-time.";

	public override string ItemDescription => "Damage-over-time effects activate your <style=cIsDamage>On Hit</style> items at a rate of <style=cIsUtility>50%</style> <style=cStack>(+50% per stack)</style>. Hits that deal <style=cIsDamage>more than 400% damage</style> also deal an additional <style=cIsDamage>600% damage</style> over time.";

	public override string ItemLore => "Order: Blood Orange\r\nTracking Number: 976*****\r\nEstimated Delivery: 03/08/2087\r\nShipping Method: High Priority/Fragile\r\nShipping Address: 178654, Hallmark, Saturn\r\nShipping Details:\r\n\r\nI'd recommend keeping this one in the box...if you do feel the need to take it out, keep a SAFE DISTANCE! Mendelson went and touched the darn thing, started leeching the blood right out of his hand. His lower arm went grey faster than you can rhyme orange with banana. Luckily me and a few pals pulled him away; he's expected to make a full recovery within the month (needed a blood donor though, and wow are those getting pricy these days).\r\n\r\nAnyway, please don't be a Mendelson; I sort of like you. If I didn't like you then you wouldn't be recieving this wonderful gift, after all. :) - JJ\r\n";

	public override ItemTier Tier => ItemTier.Tier3;

	public override ItemTag[] ItemTags => new ItemTag[1] { ItemTag.Utility };

	public override Sprite ItemIconSprite => SpikestripContent.GrooveBundle.LoadAsset<Sprite>("texBloodOrangeIcon.png");

	public override Type ItemBehaviour => typeof(DotProcsBehaviourServer);

	public override bool ServerOnlyBehaviour => true;

	public override void Init()
	{
		base.Init();
		bloodOrangeBuff = ScriptableObject.CreateInstance<RoR2.BuffDef>();
		bloodOrangeBuff.buffColor = Color.white;
		bloodOrangeBuff.canStack = false;
		bloodOrangeBuff.iconSprite = SpikestripContent.GrooveBundle.LoadAsset<Sprite>("texBuffBloodOrangeIcon.png");
		bloodOrangeBuff.isDebuff = false;
		bloodOrangeBuff.isHidden = false;
		ContentAddition.AddBuffDef(bloodOrangeBuff);
		bloodOrangeDotIndex = DotAPI.RegisterDotDef(0.25f, 0.5f, orangeDamageColorIndex, bloodOrangeBuff, (DotAPI.CustomDotBehaviour)null, (DotAPI.CustomDotVisual)null);
		dotProcImpactEffect = PrefabAPI.InstantiateClone(SpikestripUtil.LegacyLoad<GameObject>("Prefabs/Effects/ImpactEffects/SpurtGenericBlood"), "DotProcImpact", false);
		ParticleSystem componentInChildren = dotProcImpactEffect.GetComponentInChildren<ParticleSystem>();
		ParticleSystem.MainModule main = componentInChildren.main;
		main.scalingMode = ParticleSystemScalingMode.Hierarchy;
		componentInChildren.transform.localScale = Vector3.one * 4f;
		ParticleSystemRenderer component = componentInChildren.GetComponent<ParticleSystemRenderer>();
		Material material = UnityEngine.Object.Instantiate(component.material);
		material.SetColor("_TintColor", Color.red);
		component.material = material;
		RoR2.EffectComponent component2 = dotProcImpactEffect.GetComponent<RoR2.EffectComponent>();
		component2.applyScale = true;
		ContentAddition.AddEffect(dotProcImpactEffect);
		On.RoR2.HealthComponent.TakeDamage += HealthComponent_TakeDamage;
		On.RoR2.GlobalEventManager.OnHitEnemy += GlobalEventManager_OnHitEnemy;
		On.RoR2.DotController.OnDotStackAddedServer += DotController_OnDotStackAddedServer;
	}

	private void HealthComponent_TakeDamage(On.RoR2.HealthComponent.orig_TakeDamage orig, RoR2.HealthComponent self, RoR2.DamageInfo damageInfo)
	{
		bool flag = false;
		if (damageInfo.dotIndex != RoR2.DotController.DotIndex.None && (bool)damageInfo.attacker)
		{
			RoR2.CharacterBody component = damageInfo.attacker.GetComponent<RoR2.CharacterBody>();
			DotProcsBehaviourServer component2 = damageInfo.attacker.GetComponent<DotProcsBehaviourServer>();
			RoR2.DotController dotController = (damageInfo.inflictor ? damageInfo.inflictor.GetComponent<RoR2.DotController>() : null);
			if ((bool)component && (bool)component.inventory && (bool)component2 && (bool)dotController)
			{
				int itemCount = component.inventory.GetItemCountEffective(ItemDef);
				if (itemCount > 0)
				{
					damageInfo.procCoefficient += 0.5f * (float)itemCount;
					damageInfo.crit = component.RollCrit();
					if (component2.storedProcChainMasks.TryGetValue(dotController, out var value))
					{
						damageInfo.procChainMask = value[(int)damageInfo.dotIndex];
					}
					RoR2.EffectData effectData = new RoR2.EffectData
					{
						origin = damageInfo.position,
						rotation = RoR2.Util.QuaternionSafeLookRotation((damageInfo.force != Vector3.zero) ? damageInfo.force : UnityEngine.Random.onUnitSphere),
						scale = (self.GetComponent<RoR2.CharacterBody>() ? self.GetComponent<RoR2.CharacterBody>().radius : 1f)
					};
					RoR2.EffectManager.SpawnEffect(dotProcImpactEffect, effectData, transmit: true);
					flag = true;
				}
			}
		}
		globalServerTrackedProcChainMask = damageInfo.procChainMask;
		try
		{
			orig(self, damageInfo);
		}
		finally
		{
			globalServerTrackedProcChainMask = default(RoR2.ProcChainMask);
			if (flag)
			{
				RoR2.GlobalEventManager.instance.OnHitEnemy(damageInfo, self.gameObject);
				RoR2.GlobalEventManager.instance.OnHitAll(damageInfo, self.gameObject);
			}
		}
	}

	private void GlobalEventManager_OnHitEnemy(On.RoR2.GlobalEventManager.orig_OnHitEnemy orig, RoR2.GlobalEventManager self, RoR2.DamageInfo damageInfo, GameObject victim)
	{
		globalServerTrackedProcChainMask = damageInfo.procChainMask;
		try
		{
			orig(self, damageInfo, victim);
			TryApplyOrangeDot(damageInfo, victim);
		}
		finally
		{
			globalServerTrackedProcChainMask = default(RoR2.ProcChainMask);
		}
	}

	public void TryApplyOrangeDot(RoR2.DamageInfo damageInfo, GameObject victim)
	{
		if (damageInfo.procCoefficient != 0f && !damageInfo.rejected && NetworkServer.active && (bool)damageInfo.attacker && (bool)victim && damageInfo.procCoefficient > 0f)
		{
			RoR2.CharacterBody component = damageInfo.attacker.GetComponent<RoR2.CharacterBody>();
			RoR2.Inventory inventory = (component ? component.inventory : null);
			if ((bool)inventory && inventory.GetItemCountEffective(ItemDef) > 0 && !damageInfo.procChainMask.HasProc(bloodOrangeProcType) && damageInfo.damage / component.damage >= 4f)
			{
				InflictDotInfo inflictDotInfo = new InflictDotInfo
				{
					attackerObject = damageInfo.attacker,
					victimObject = victim,
					damageMultiplier = 1f,
					dotIndex = bloodOrangeDotIndex,
					duration = 3f * damageInfo.procCoefficient,
					maxStacksFromAttacker = 1u
				};
				RoR2.DotController.InflictDot(ref inflictDotInfo);
			}
		}
	}

	private void DotController_OnDotStackAddedServer(On.RoR2.DotController.orig_OnDotStackAddedServer orig, RoR2.DotController self, RoR2.DotController.DotStack dotStack)
	{
		orig(self, dotStack);
		RoR2.DotController.DotStack dotStack2 = (RoR2.DotController.DotStack)dotStack;
		DotProcsBehaviourServer dotProcsBehaviourServer = (dotStack2.attackerObject ? dotStack2.attackerObject.GetComponent<DotProcsBehaviourServer>() : null);
		if ((bool)dotProcsBehaviourServer)
		{
			dotProcsBehaviourServer.AddProcChainMask(self, dotStack2);
		}
	}

	public override void AssignItemModels()
	{
		ItemPickupModel = SpikestripUtil.UpdateGameObjectShader(PrefabAPI.InstantiateClone(SpikestripContent.GrooveBundle.LoadAsset<GameObject>("BloodOrange.prefab"), "PickupBloodOrange", false));
		Transform transform = ItemPickupModel.transform.Find("holder/orange");
		MeshRenderer component = transform.GetComponent<MeshRenderer>();
		component.material.EnableKeyword("FORCE_SPEC");
		Material material = UnityEngine.Object.Instantiate(SpikestripUtil.AddressablesLoad<Material>("c6b7321c381a7f74a9d362ba0eb515f3"));
		material.EnableKeyword("DITHER");
		Transform transform2 = ItemPickupModel.transform.Find("holder/blood");
		transform2.GetComponent<MeshRenderer>().material = material;
		Transform transform3 = ItemPickupModel.transform.Find("holder/Point Light");
		Light component2 = transform3.GetComponent<Light>();
		component2.intensity = 30f;
		component2.range = 1.5f;
		RoR2.FlickerLight flickerLight = transform3.gameObject.AddComponent<RoR2.FlickerLight>();
		flickerLight.light = component2;
		flickerLight.sinWaves = new Wave[2]
		{
			new Wave
			{
				amplitude = 0.5f,
				frequency = 1f
			},
			new Wave
			{
				amplitude = 0.2f,
				frequency = 0.5f
			}
		};
		ItemDisplayModel = PrefabAPI.InstantiateClone(ItemPickupModel, "DisplayBloodOrange", false);
		Transform transform4 = ItemPickupModel.transform.Find("holder/blood trails");
		transform4.GetComponent<MeshRenderer>().material = material;
		GameObject gameObject = PrefabAPI.InstantiateClone(SpikestripUtil.LegacyLoad<GameObject>("prefabs/temporaryvisualeffects/CrippleEffect").transform.Find("Visual/Drips").gameObject, "blood drips", false);
		gameObject.transform.SetParent(ItemPickupModel.transform.Find("holder"));
		gameObject.transform.localPosition = new Vector3(0f, -0.5f, 0f);
		gameObject.transform.localScale = new Vector3(1.5f, 1f, 1.5f);
		ParticleSystemRenderer component3 = gameObject.GetComponent<ParticleSystemRenderer>();
		component3.mesh = null;
		component3.material = material;
		UnityEngine.Object.DestroyImmediate(ItemPickupModel.transform.Find("holder/pin").gameObject);
		UnityEngine.Object.DestroyImmediate(ItemDisplayModel.transform.Find("holder/blood trails").gameObject);
		ItemDisplayModel.transform.Find("holder/Point Light").GetComponent<Light>().range = 0.5f;
		RoR2.ItemDisplay itemDisplay = ItemDisplayModel.AddComponent<RoR2.ItemDisplay>();
		itemDisplay.rendererInfos = SpikestripUtil.GenerateRendererInfos(ItemDisplayModel);
	}

	public override void PopulateItemDisplayRules(ItemDisplayRuleDict itemDisplayRuleDict)
	{
		itemDisplayRuleDict.Add("mdlCommandoDualies", new RoR2.ItemDisplayRule[1]
		{
			new RoR2.ItemDisplayRule
			{
				ruleType = ItemDisplayRuleType.ParentedPrefab,
				followerPrefab = ItemDisplayModel,
				childName = "Chest",
				localPos = new Vector3(0.23863f, 0.52669f, -0.10322f),
				localAngles = new Vector3(316.8034f, 288.1698f, 69.26473f),
				localScale = new Vector3(0.0835f, 0.0835f, 0.0835f)
			}
		});
		itemDisplayRuleDict.Add("mdlHuntress", new RoR2.ItemDisplayRule[1]
		{
			new RoR2.ItemDisplayRule
			{
				ruleType = ItemDisplayRuleType.ParentedPrefab,
				followerPrefab = ItemDisplayModel,
				childName = "Chest",
				localPos = new Vector3(0.2561f, 0.30105f, -0.00152f),
				localAngles = new Vector3(324.428f, 209.1001f, 115.9055f),
				localScale = new Vector3(0.0626f, 0.0626f, 0.0626f)
			}
		});
		itemDisplayRuleDict.Add("mdlToolbot", new RoR2.ItemDisplayRule[1]
		{
			new RoR2.ItemDisplayRule
			{
				ruleType = ItemDisplayRuleType.ParentedPrefab,
				followerPrefab = ItemDisplayModel,
				childName = "Chest",
				localPos = new Vector3(2.08912f, 3.46538f, -1.49216f),
				localAngles = new Vector3(346.9209f, 270.5325f, 92.01743f),
				localScale = new Vector3(0.60822f, 0.60822f, 0.60822f)
			}
		});
		itemDisplayRuleDict.Add("mdlEngi", new RoR2.ItemDisplayRule[1]
		{
			new RoR2.ItemDisplayRule
			{
				ruleType = ItemDisplayRuleType.ParentedPrefab,
				followerPrefab = ItemDisplayModel,
				childName = "Chest",
				localPos = new Vector3(0.01806f, 0.61597f, -0.25157f),
				localAngles = new Vector3(27.34335f, 265.6688f, 82.72406f),
				localScale = new Vector3(0.11389f, 0.11389f, 0.11389f)
			}
		});
		itemDisplayRuleDict.Add("mdlMage", new RoR2.ItemDisplayRule[1]
		{
			new RoR2.ItemDisplayRule
			{
				ruleType = ItemDisplayRuleType.ParentedPrefab,
				followerPrefab = ItemDisplayModel,
				childName = "Chest",
				localPos = new Vector3(0.13598f, 0.42375f, -0.07561f),
				localAngles = new Vector3(1.81937f, 245.0162f, 94.79094f),
				localScale = new Vector3(0.06598f, 0.06598f, 0.06598f)
			}
		});
		itemDisplayRuleDict.Add("mdlMerc", new RoR2.ItemDisplayRule[1]
		{
			new RoR2.ItemDisplayRule
			{
				ruleType = ItemDisplayRuleType.ParentedPrefab,
				followerPrefab = ItemDisplayModel,
				childName = "Chest",
				localPos = new Vector3(0.16931f, 0.40814f, -0.14655f),
				localAngles = new Vector3(340.1609f, 272.3493f, 82.76094f),
				localScale = new Vector3(0.07271f, 0.07271f, 0.07271f)
			}
		});
		itemDisplayRuleDict.Add("mdlTreebot", new RoR2.ItemDisplayRule[1]
		{
			new RoR2.ItemDisplayRule
			{
				ruleType = ItemDisplayRuleType.ParentedPrefab,
				followerPrefab = ItemDisplayModel,
				childName = "FlowerBase",
				localPos = new Vector3(-0.30251f, 1.02777f, 0.34856f),
				localAngles = new Vector3(48.67057f, 327.506f, 60.1874f),
				localScale = new Vector3(0.22236f, 0.22236f, 0.22236f)
			}
		});
		itemDisplayRuleDict.Add("mdlLoader", new RoR2.ItemDisplayRule[1]
		{
			new RoR2.ItemDisplayRule
			{
				ruleType = ItemDisplayRuleType.ParentedPrefab,
				followerPrefab = ItemDisplayModel,
				childName = "UpperArmR",
				localPos = new Vector3(-0.00546f, 0.2461f, -0.1817f),
				localAngles = new Vector3(326.3639f, 297.9528f, 135.4316f),
				localScale = new Vector3(0.05818f, 0.05818f, 0.05818f)
			}
		});
		itemDisplayRuleDict.Add("mdlCroco", new RoR2.ItemDisplayRule[1]
		{
			new RoR2.ItemDisplayRule
			{
				ruleType = ItemDisplayRuleType.ParentedPrefab,
				followerPrefab = ItemDisplayModel,
				childName = "Chest",
				localPos = new Vector3(-2.86652f, 1.99875f, 4.62465f),
				localAngles = new Vector3(358.9185f, 62.00133f, 150.2076f),
				localScale = new Vector3(0.8035f, 0.8035f, 0.8035f)
			}
		});
		itemDisplayRuleDict.Add("mdlCaptain", new RoR2.ItemDisplayRule[1]
		{
			new RoR2.ItemDisplayRule
			{
				ruleType = ItemDisplayRuleType.ParentedPrefab,
				followerPrefab = ItemDisplayModel,
				childName = "Chest",
				localPos = new Vector3(0.19629f, 0.51207f, -0.24692f),
				localAngles = new Vector3(5.26093f, 90.45467f, 32.77518f),
				localScale = new Vector3(0.07032f, 0.07032f, 0.07032f)
			}
		});
		itemDisplayRuleDict.Add("mdlBandit2", new RoR2.ItemDisplayRule[1]
		{
			new RoR2.ItemDisplayRule
			{
				ruleType = ItemDisplayRuleType.ParentedPrefab,
				followerPrefab = ItemDisplayModel,
				childName = "ClavicleR",
				localPos = new Vector3(0.18718f, 0.01198f, -0.12282f),
				localAngles = new Vector3(9.06273f, 236.7618f, 147.7266f),
				localScale = new Vector3(0.05524f, 0.05524f, 0.05524f)
			}
		});
		itemDisplayRuleDict.Add("mdlRailGunner", new RoR2.ItemDisplayRule[1]
		{
			new RoR2.ItemDisplayRule
			{
				ruleType = ItemDisplayRuleType.ParentedPrefab,
				followerPrefab = ItemDisplayModel,
				childName = "ClavicleR",
				localPos = new Vector3(0.04502f, 0.21427f, -0.15626f),
				localAngles = new Vector3(331.2475f, 286.7206f, 105.9696f),
				localScale = new Vector3(0.05321f, 0.05321f, 0.05321f)
			}
		});
		itemDisplayRuleDict.Add("mdlVoidSurvivor", new RoR2.ItemDisplayRule[1]
		{
			new RoR2.ItemDisplayRule
			{
				ruleType = ItemDisplayRuleType.ParentedPrefab,
				followerPrefab = ItemDisplayModel,
				childName = "UpperArmL",
				localPos = new Vector3(0.19285f, 0.12577f, -0.07271f),
				localAngles = new Vector3(331.0556f, 205.244f, 142.6228f),
				localScale = new Vector3(0.07048f, 0.07048f, 0.07048f)
			}
		});
		itemDisplayRuleDict.Add("mdlScav", new RoR2.ItemDisplayRule[1]
		{
			new RoR2.ItemDisplayRule
			{
				ruleType = ItemDisplayRuleType.ParentedPrefab,
				followerPrefab = ItemDisplayModel,
				childName = "Chest",
				localPos = new Vector3(7.54384f, 4.36956f, 2.97733f),
				localAngles = new Vector3(330.2531f, 292.9532f, 19.69187f),
				localScale = new Vector3(1.16353f, 1.16353f, 1.16353f)
			}
		});
		itemDisplayRuleDict.Add("mdlEngiTurret", new RoR2.ItemDisplayRule[1]
		{
			new RoR2.ItemDisplayRule
			{
				ruleType = ItemDisplayRuleType.ParentedPrefab,
				followerPrefab = ItemDisplayModel,
				childName = "Head",
				localPos = new Vector3(0.949f, 1.03647f, -0.23108f),
				localAngles = new Vector3(2.10367f, 340.7558f, 29.70926f),
				localScale = new Vector3(0.23194f, 0.23194f, 0.23194f)
			}
		});
	}
}
