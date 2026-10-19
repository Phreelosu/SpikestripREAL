using System;
using GrooveSaladSpikestripContent;
using On.RoR2;
using R2API;
using RoR2;
using UnityEngine;
using Spikestrip2_0.Core;

namespace NoopSpikestrip;

internal class Voiditem : ItemBase<Voiditem>
{
	public class CartridgeMasterBehaviour : MonoBehaviour
	{
		public RoR2.CharacterMaster master;

		public int consumedDebuffs;

		public void OnDebuffConsumed()
		{
			RoR2.CharacterBody body = master.GetBody();
			if (!body)
			{
				return;
			}
			Vector3 corePosition = body.corePosition;
			if (RoR2.Util.CheckRoll(10f, master))
			{
				master.inventory.GiveItemPermanent(BLESSING.instance.ItemDef);
				RoR2.GenericPickupController.SendPickupMessage(master, new RoR2.UniquePickup { pickupIndex = RoR2.PickupCatalog.FindPickupIndex(BLESSING.instance.ItemDef.itemIndex) });
				RoR2.EffectManager.SpawnEffect(Blessed, new RoR2.EffectData { origin = corePosition }, transmit: true);
			}
			RoR2.EffectManager.SpawnEffect(woolieEffect, new RoR2.EffectData { origin = corePosition }, transmit: true);
			RoR2.Util.PlaySound("Play_UI_podSteamBurst", body.gameObject);
			if (++consumedDebuffs >= 10)
			{
				consumedDebuffs = 0;
				master.inventory.RemoveItemPermanent(Voiditem.instance.ItemDef.itemIndex, 1);
				master.inventory.GiveItemPermanent(VoidItemConsumed.instance.ItemDef);
				RoR2.CharacterMasterNotificationQueue.PushItemTransformNotification(master, Voiditem.instance.ItemDef.itemIndex, VoidItemConsumed.instance.ItemDef.itemIndex, RoR2.CharacterMasterNotificationQueue.TransformationType.Default);
			}
		}
	}

	public static GameObject woolieEffect;

	public RoR2.BuffDef ticktick;

	public static GameObject Blessed;

	public override string ItemName => "Abyssal Cartridge";

	public override string ItemToken => "AbyssalMedkit";

	public override string ItemPickup => "Prevent a small amount of debuffs before breaking. Each debuff prevented has a chance to bless ALL of your stats. <style=cIsVoid>Corrupts all Medkits</style>.";

	public override string ItemDescription => "Prevent <style=cIsUtility>10 debuffs</style> before breaking. Each debuff prevented has a 10% chance to grant you a <style=cIsHealing>Blessing</style>, increasing <style=cIsUtility>ALL stats</style> by <style=cIsUtility>3%</style>. <style=cIsVoid>Corrupts all Medkits</style>.";

	public override string ItemLore => "The brain. A piece of the spine. A Couple of organs. \n\n Wrap it in skin and flesh. Add Drugs to remove the pain. \n\n ... \n\n I'm Sorry Samuel, I really am. \n\n <style=cEvent>I wont forget you, just like all the others</style>";

	public override ItemTier Tier => ItemTier.VoidTier1;

	public override Sprite ItemIconSprite => SpikestripContent.NoopBundle.LoadAsset<Sprite>("CartridgeIcon.png");

	public override RoR2.ItemDef[] ItemsToCorrupt => new RoR2.ItemDef[1] { SpikestripUtil.LegacyLoad<RoR2.ItemDef>("ItemDefs/Medkit") };

	public override void AssignItemModels()
	{
		GameObject gameObject = SpikestripContent.NoopBundle.LoadAsset<GameObject>("Cartridge.prefab");
		ItemPickupModel = SpikestripUtil.UpdateGameObjectShader(gameObject);
		ItemDisplayModel = PrefabAPI.InstantiateClone(gameObject, "CartDisplay", false);
		RoR2.ItemDisplay itemDisplay = ItemDisplayModel.AddComponent<RoR2.ItemDisplay>();
		itemDisplay.rendererInfos = SpikestripUtil.GenerateRendererInfos(ItemDisplayModel);
		ItemDisplayModel = SpikestripUtil.UpdateGameObjectShader(ItemDisplayModel);
	}

	public override void Init()
	{
		base.Init();
		On.RoR2.CharacterMaster.OnInventoryChanged += CharacterMaster_OnInventoryChanged;
		On.RoR2.DotController.InflictDot_refInflictDotInfo += DotController_InflictDot_refInflictDotInfo;
		On.RoR2.CharacterBody.AddTimedBuff_BuffDef_float_int += CharacterBody_AddTimedBuff_BuffDef_float_int;
		On.RoR2.CharacterBody.AddTimedBuff_BuffDef_float += CharacterBody_AddTimedBuff_BuffDef_float;
		ticktick = ScriptableObject.CreateInstance<RoR2.BuffDef>();
		ticktick.canStack = true;
		ContentAddition.AddBuffDef(ticktick);
		woolieEffect = SpikestripContent.NoopBundle.LoadAsset<GameObject>("CartridgeEffect.prefab");
		woolieEffect.AddComponent<RoR2.EffectComponent>();
		woolieEffect.AddComponent<RoR2.DestroyOnTimer>();
		RoR2.EffectComponent component = woolieEffect.GetComponent<RoR2.EffectComponent>();
		RoR2.DestroyOnTimer component2 = woolieEffect.GetComponent<RoR2.DestroyOnTimer>();
		component2.enabled = true;
		component2.duration = 4f;
		ParticleSystemRenderer component3 = woolieEffect.GetComponent<ParticleSystemRenderer>();
		component3.SetSharedMaterials(new Material[1] { SpikestripUtil.AddressablesLoad<Material>("40c323c18bdbce445a02361e3c26cedf") }, 1);
		ContentAddition.AddEffect(woolieEffect);
		Blessed = PrefabAPI.InstantiateClone(SpikestripUtil.AddressablesLoad<GameObject>("732a7bd18ca8d9e4ca6019c217e6b694"), "Blessing");
		UnityEngine.Object.Destroy(Blessed.transform.Find("PP").gameObject);
		UnityEngine.Object.Destroy(Blessed.transform.Find("TextCamScaler").gameObject);
		Light componentInChildren = Blessed.transform.GetComponentInChildren<Light>();
		componentInChildren.color = SpikestripUtil.ColorRGB(216f, 0f, 255f);
		ParticleSystemRenderer component4 = Blessed.transform.Find("Big Feathers").GetComponent<ParticleSystemRenderer>();
		component4.SetSharedMaterials(new Material[1] { SpikestripUtil.AddressablesLoad<Material>("7eee3c438f4dd364c97a6118a6dafda9") }, 1);
		SkinnedMeshRenderer component5 = Blessed.transform.Find("WingEffect/HippoEffectWingsMesh").GetComponent<SkinnedMeshRenderer>();
		component5.SetSharedMaterials(new Material[1] { SpikestripUtil.AddressablesLoad<Material>("420aca1cbdc52ed45a8e01255b66f01f") }, 1);
		ContentAddition.AddEffect(Blessed);
	}

	private void CharacterMaster_OnInventoryChanged(On.RoR2.CharacterMaster.orig_OnInventoryChanged orig, RoR2.CharacterMaster self)
	{
		orig(self);
		bool hasItem = self.inventory.GetItemCountEffective(ItemDef) > 0;
		CartridgeMasterBehaviour behaviour = self.GetComponent<CartridgeMasterBehaviour>();
		if (hasItem && !behaviour)
		{
			self.gameObject.AddComponent<CartridgeMasterBehaviour>().master = self;
		}
		else if (!hasItem && (bool)behaviour)
		{
			UnityEngine.Object.Destroy(behaviour);
		}
	}

	private static bool TryConsumeDebuff(RoR2.CharacterBody body)
	{
		if ((bool)body && (bool)body.master && body.master.TryGetComponent(out CartridgeMasterBehaviour behaviour))
		{
			behaviour.OnDebuffConsumed();
			return true;
		}
		return false;
	}

	private void CharacterBody_AddTimedBuff_BuffDef_float(On.RoR2.CharacterBody.orig_AddTimedBuff_BuffDef_float orig, RoR2.CharacterBody self, RoR2.BuffDef buffDef, float duration)
	{
		if (!((bool)buffDef && buffDef.isDebuff && TryConsumeDebuff(self)))
		{
			orig(self, buffDef, duration);
		}
	}

	private void CharacterBody_AddTimedBuff_BuffDef_float_int(On.RoR2.CharacterBody.orig_AddTimedBuff_BuffDef_float_int orig, RoR2.CharacterBody self, RoR2.BuffDef buffDef, float duration, int maxStacks)
	{
		if (!((bool)buffDef && buffDef.isDebuff && TryConsumeDebuff(self)))
		{
			orig(self, buffDef, duration, maxStacks);
		}
	}

	private void DotController_InflictDot_refInflictDotInfo(On.RoR2.DotController.orig_InflictDot_refInflictDotInfo orig, ref InflictDotInfo inflictDotInfo)
	{
		if (!TryConsumeDebuff(inflictDotInfo.victimObject ? inflictDotInfo.victimObject.GetComponent<RoR2.CharacterBody>() : null))
		{
			orig(ref inflictDotInfo);
		}
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
				localPos = new Vector3(-0.00608f, 0.45582f, -0.16325f),
				localAngles = new Vector3(24.28383f, 128.0119f, 329.8718f),
				localScale = new Vector3(0.1f, 0.1f, 0.1f)
			}
		});
		itemDisplayRuleDict.Add("mdlHuntress", new RoR2.ItemDisplayRule[1]
		{
			new RoR2.ItemDisplayRule
			{
				ruleType = ItemDisplayRuleType.ParentedPrefab,
				followerPrefab = ItemDisplayModel,
				childName = "UpperArmR",
				localPos = new Vector3(-0.02015f, 0.21122f, -0.08182f),
				localAngles = new Vector3(315.5638f, 92.46681f, 278.3044f),
				localScale = new Vector3(0.1f, 0.1f, 0.1f)
			}
		});
		itemDisplayRuleDict.Add("mdlToolbot", new RoR2.ItemDisplayRule[1]
		{
			new RoR2.ItemDisplayRule
			{
				ruleType = ItemDisplayRuleType.ParentedPrefab,
				followerPrefab = ItemDisplayModel,
				childName = "Head",
				localPos = new Vector3(-0.02789f, -0.69875f, 1.28177f),
				localAngles = new Vector3(23.25583f, 230.0303f, 207.5309f),
				localScale = new Vector3(1f, 1f, 1f)
			}
		});
		itemDisplayRuleDict.Add("mdlEngi", new RoR2.ItemDisplayRule[1]
		{
			new RoR2.ItemDisplayRule
			{
				ruleType = ItemDisplayRuleType.ParentedPrefab,
				followerPrefab = ItemDisplayModel,
				childName = "UpperArmR",
				localPos = new Vector3(-0.11937f, 0.1524f, -0.0133f),
				localAngles = new Vector3(53.8207f, 347.5613f, 85.64195f),
				localScale = new Vector3(0.1f, 0.1f, 0.1f)
			}
		});
		itemDisplayRuleDict.Add("mdlMage", new RoR2.ItemDisplayRule[1]
		{
			new RoR2.ItemDisplayRule
			{
				ruleType = ItemDisplayRuleType.ParentedPrefab,
				followerPrefab = ItemDisplayModel,
				childName = "Chest",
				localPos = new Vector3(-0.01049f, 0.36774f, -0.17101f),
				localAngles = new Vector3(355.5203f, 134.1092f, 3.37996f),
				localScale = new Vector3(0.1f, 0.1f, 0.1f)
			}
		});
		itemDisplayRuleDict.Add("mdlMerc", new RoR2.ItemDisplayRule[1]
		{
			new RoR2.ItemDisplayRule
			{
				ruleType = ItemDisplayRuleType.ParentedPrefab,
				followerPrefab = ItemDisplayModel,
				childName = "UpperArmR",
				localPos = new Vector3(-0.20663f, 0.04246f, -0.07763f),
				localAngles = new Vector3(31.24286f, 131.7437f, 248.5761f),
				localScale = new Vector3(0.1f, 0.1f, 0.1f)
			}
		});
		itemDisplayRuleDict.Add("mdlTreebot", new RoR2.ItemDisplayRule[1]
		{
			new RoR2.ItemDisplayRule
			{
				ruleType = ItemDisplayRuleType.ParentedPrefab,
				followerPrefab = ItemDisplayModel,
				childName = "CalfBackR",
				localPos = new Vector3(-0.00362f, 0.64628f, -0.20254f),
				localAngles = new Vector3(307.4982f, 86.70875f, 271.2927f),
				localScale = new Vector3(0.2f, 0.2f, 0.2f)
			}
		});
		itemDisplayRuleDict.Add("mdlLoader", new RoR2.ItemDisplayRule[1]
		{
			new RoR2.ItemDisplayRule
			{
				ruleType = ItemDisplayRuleType.ParentedPrefab,
				followerPrefab = ItemDisplayModel,
				childName = "Neck",
				localPos = new Vector3(0.00083f, 0.25068f, 0.21676f),
				localAngles = new Vector3(349.7862f, 314.0446f, 10.93832f),
				localScale = new Vector3(0.1f, 0.1f, 0.1f)
			}
		});
		itemDisplayRuleDict.Add("mdlCroco", new RoR2.ItemDisplayRule[1]
		{
			new RoR2.ItemDisplayRule
			{
				ruleType = ItemDisplayRuleType.ParentedPrefab,
				followerPrefab = ItemDisplayModel,
				childName = "Stomach",
				localPos = new Vector3(-0.31814f, -0.56531f, 2.82175f),
				localAngles = new Vector3(39.24161f, 245.8687f, 244.0103f),
				localScale = new Vector3(1.5f, 1.5f, 1.5f)
			}
		});
		itemDisplayRuleDict.Add("mdlCaptain", new RoR2.ItemDisplayRule[1]
		{
			new RoR2.ItemDisplayRule
			{
				ruleType = ItemDisplayRuleType.ParentedPrefab,
				followerPrefab = ItemDisplayModel,
				childName = "Chest",
				localPos = new Vector3(-0.04328f, 0.43248f, -0.21186f),
				localAngles = new Vector3(35f, 119.0965f, 299.0688f),
				localScale = new Vector3(0.1f, 0.1f, 0.1f)
			}
		});
		itemDisplayRuleDict.Add("mdlBandit2", new RoR2.ItemDisplayRule[1]
		{
			new RoR2.ItemDisplayRule
			{
				ruleType = ItemDisplayRuleType.ParentedPrefab,
				followerPrefab = ItemDisplayModel,
				childName = "Chest",
				localPos = new Vector3(-0.00349f, -0.11756f, -0.14204f),
				localAngles = new Vector3(42.05348f, 51.29253f, 235.9125f),
				localScale = new Vector3(0.1f, 0.1f, 0.1f)
			}
		});
		itemDisplayRuleDict.Add("mdlScav", new RoR2.ItemDisplayRule[1]
		{
			new RoR2.ItemDisplayRule
			{
				ruleType = ItemDisplayRuleType.ParentedPrefab,
				followerPrefab = ItemDisplayModel,
				childName = "Backpack",
				localPos = new Vector3(2.26324f, 9.42422f, -4.77379f),
				localAngles = new Vector3(47.34174f, 85.9677f, 269.8065f),
				localScale = new Vector3(2f, 2f, 2f)
			}
		});
		itemDisplayRuleDict.Add("mdlEngiTurret", new RoR2.ItemDisplayRule[1]
		{
			new RoR2.ItemDisplayRule
			{
				ruleType = ItemDisplayRuleType.ParentedPrefab,
				followerPrefab = ItemDisplayModel,
				childName = "LegBar2",
				localPos = new Vector3(0.00966f, 0.22048f, 0.27884f),
				localAngles = new Vector3(318.6975f, 269.9536f, 268.9743f),
				localScale = new Vector3(0.5f, 0.5f, 0.5f)
			}
		});
		itemDisplayRuleDict.Add("mdlMEL-T2", new RoR2.ItemDisplayRule[1]
		{
			new RoR2.ItemDisplayRule
			{
				ruleType = ItemDisplayRuleType.ParentedPrefab,
				followerPrefab = ItemDisplayModel,
				childName = "Head",
				localPos = new Vector3(0.00075f, -0.02811f, -0.00794f),
				localAngles = new Vector3(1.9252f, 45.82498f, 180.1591f),
				localScale = new Vector3(0.02f, 0.02f, 0.02f)
			}
		});
		itemDisplayRuleDict.Add("mdlRailGunner", new RoR2.ItemDisplayRule[1]
		{
			new RoR2.ItemDisplayRule
			{
				ruleType = ItemDisplayRuleType.ParentedPrefab,
				followerPrefab = ItemDisplayModel,
				childName = "Backpack",
				localPos = new Vector3(0.01912f, 0.42946f, -0.02326f),
				localAngles = new Vector3(0f, 132.2009f, 1.06482f),
				localScale = new Vector3(0.1f, 0.1f, 0.1f)
			}
		});
		itemDisplayRuleDict.Add("mdlVoidSurvivor", new RoR2.ItemDisplayRule[1]
		{
			new RoR2.ItemDisplayRule
			{
				ruleType = ItemDisplayRuleType.ParentedPrefab,
				followerPrefab = ItemDisplayModel,
				childName = "Pelvis",
				localPos = new Vector3(-0.14203f, 0.06374f, -0.029f),
				localAngles = new Vector3(312.0996f, 333.6005f, 95.96362f),
				localScale = new Vector3(0.1f, 0.1f, 0.1f)
			}
		});
	}
}