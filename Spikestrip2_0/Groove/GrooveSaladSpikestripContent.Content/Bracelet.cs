using System;
using R2API;
using RoR2;
using UnityEngine;
using Spikestrip2_0.Core;

namespace GrooveSaladSpikestripContent.Content;

public class Bracelet : ItemBase<Bracelet>
{
	public class BraceletBehaviourServer : CharacterBody.ItemBehavior, IOnDamageDealtServerReceiver
	{
		private int _storedHitAmount;

		public int StoredHitAmount
		{
			get
			{
				return _storedHitAmount;
			}
			set
			{
				_storedHitAmount = value;
				body.MarkAllStatsDirty();
			}
		}

		public void OnDamageDealtServer(DamageReport damageReport)
		{
			if (damageReport.damageInfo.crit)
			{
				if (StoredHitAmount > 0)
				{
					EffectManager.SimpleImpactEffect(impactEffect, damageReport.damageInfo.position, -damageReport.damageInfo.force, transmit: true);
				}
				StoredHitAmount = 0;
			}
			else if (damageReport.damageInfo.procCoefficient > 0f)
			{
				StoredHitAmount++;
			}
		}
	}

	public static GameObject impactEffect;

	public override string ItemToken => "BRACELET";

	public override string ItemName => "Pewter Bracelet";

	public override string ItemPickup => "Boost your next 'Critical Strike' with each hit.";

	public override string ItemDescription => "Boost the damage of your next <style=cIsDamage>Critical Strike</style> by <style=cIsDamage>10%</style> <style=cStack>(+10% per stack)</style> on hit.";

	public override string ItemLore => "\"Balance. Balance is essential. What is taken must be returned. What is returned may freely be taken. Freedom is power. Master freedom.\n\nMetal. The metal must be perfect. This metal is perfect. Alloy without deviation. This is perfect.\n\nKnowledge. Knowledge is strength. This will be strength. This will be knowledge.\"\n\n- Unidentifed Manuscript";

	public override ItemTier Tier => ItemTier.Tier1;

	public override ItemTag[] ItemTags => new ItemTag[1] { ItemTag.Damage };

	public override Sprite ItemIconSprite => SpikestripContent.GrooveBundle.LoadAsset<Sprite>("texBraceleteIcon.png");

	public override Type ItemBehaviour => typeof(BraceletBehaviourServer);

	public override bool ServerOnlyBehaviour => true;

	public override void Init()
	{
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Expected O, but got Unknown
		base.Init();
		RecalculateStatsAPI.GetStatCoefficients += new RecalculateStatsAPI.StatHookEventHandler(RecalculateStatsAPI_GetStatCoefficients);
		impactEffect = PrefabAPI.InstantiateClone(LegacyResourcesAPI.Load<GameObject>("Prefabs/Effects/ImpactEffects/ImpactEngiTurret"), "BraceletImpact", false);
		Texture value = SpikestripUtil.AddressablesLoad<Texture>("aaa1e96a308d0114d8b33961d925fbcf");
		Transform transform = impactEffect.transform.Find("Ring");
		transform.transform.localScale = Vector3.one * 2f;
		ParticleSystemRenderer component = transform.GetComponent<ParticleSystemRenderer>();
		Material material = UnityEngine.Object.Instantiate(component.material);
		material.SetTexture("_RemapTex", value);
		material.SetColor("_TintColor", SpikestripUtil.ColorRGB(169f, 177f, 178f));
		material.DisableKeyword("VERTEXCOLOR");
		component.material = material;
		UnityEngine.Object.DestroyImmediate(impactEffect.transform.Find("Flames").gameObject);
		UnityEngine.Object.DestroyImmediate(impactEffect.transform.Find("Flash").gameObject);
		EffectComponent component2 = impactEffect.GetComponent<EffectComponent>();
		component2.soundName = "Play_lunar_golem_attack1_explode";
		VFXAttributes component3 = impactEffect.GetComponent<VFXAttributes>();
		component3.vfxPriority = VFXAttributes.VFXPriority.Medium;
		ContentAddition.AddEffect(impactEffect);
	}

	private void RecalculateStatsAPI_GetStatCoefficients(CharacterBody sender, RecalculateStatsAPI.StatHookEventArgs args)
	{
		BraceletBehaviourServer component = sender.GetComponent<BraceletBehaviourServer>();
		if ((bool)component)
		{
			args.critDamageMultAdd += (float)(component.StoredHitAmount * component.stack) * 0.1f;
		}
	}

	public override void AssignItemModels()
	{
		ItemPickupModel = SpikestripUtil.UpdateGameObjectShader(SpikestripContent.GrooveBundle.LoadAsset<GameObject>("Bracelet.prefab"));
		MeshRenderer componentInChildren = ItemPickupModel.GetComponentInChildren<MeshRenderer>();
		componentInChildren.material.EnableKeyword("FORCE_SPEC");
		componentInChildren.transform.Rotate(Vector3.forward + Vector3.right, 30f, Space.World);
		ItemDisplayModel = ItemPickupModel;
		ItemDisplay itemDisplay = ItemDisplayModel.AddComponent<ItemDisplay>();
		itemDisplay.rendererInfos = SpikestripUtil.GenerateRendererInfos(ItemDisplayModel);
	}

	public override void PopulateItemDisplayRules(ItemDisplayRuleDict itemDisplayRuleDict)
	{
		itemDisplayRuleDict.Add("mdlCommandoDualies", new ItemDisplayRule[1]
		{
			new ItemDisplayRule
			{
				ruleType = ItemDisplayRuleType.ParentedPrefab,
				followerPrefab = ItemDisplayModel,
				childName = "LowerArmR",
				localPos = new Vector3(-0.01296f, 0.24017f, 0.00526f),
				localAngles = new Vector3(296.3238f, 318.3338f, 118.7911f),
				localScale = new Vector3(-0.06544f, 0.06544f, 0.06544f)
			}
		});
		itemDisplayRuleDict.Add("mdlHuntress", new ItemDisplayRule[1]
		{
			new ItemDisplayRule
			{
				ruleType = ItemDisplayRuleType.ParentedPrefab,
				followerPrefab = ItemDisplayModel,
				childName = "LowerArmR",
				localPos = new Vector3(0.00115f, 0.18716f, 0.01183f),
				localAngles = new Vector3(295.7848f, 303.1091f, 117.5068f),
				localScale = new Vector3(-0.04198f, 0.04198f, 0.04198f)
			}
		});
		itemDisplayRuleDict.Add("mdlToolbot", new ItemDisplayRule[1]
		{
			new ItemDisplayRule
			{
				ruleType = ItemDisplayRuleType.ParentedPrefab,
				followerPrefab = ItemDisplayModel,
				childName = "LowerArmR",
				localPos = new Vector3(-0.05534f, 2.73487f, -0.02675f),
				localAngles = new Vector3(60.15063f, 241.5285f, 322.095f),
				localScale = new Vector3(-0.48151f, 0.48151f, 0.48151f)
			}
		});
		itemDisplayRuleDict.Add("mdlEngi", new ItemDisplayRule[1]
		{
			new ItemDisplayRule
			{
				ruleType = ItemDisplayRuleType.ParentedPrefab,
				followerPrefab = ItemDisplayModel,
				childName = "LowerArmR",
				localPos = new Vector3(-0.00512f, 0.17752f, 0.00714f),
				localAngles = new Vector3(61.34073f, 38.02362f, 311.2928f),
				localScale = new Vector3(-0.08298f, 0.08298f, 0.08298f)
			}
		});
		itemDisplayRuleDict.Add("mdlMage", new ItemDisplayRule[1]
		{
			new ItemDisplayRule
			{
				ruleType = ItemDisplayRuleType.ParentedPrefab,
				followerPrefab = ItemDisplayModel,
				childName = "LowerArmR",
				localPos = new Vector3(0.01251f, 0.12581f, -0.00238f),
				localAngles = new Vector3(301.2477f, 110.9512f, 125.8598f),
				localScale = new Vector3(-0.10286f, 0.10286f, 0.10286f)
			}
		});
		itemDisplayRuleDict.Add("mdlMerc", new ItemDisplayRule[1]
		{
			new ItemDisplayRule
			{
				ruleType = ItemDisplayRuleType.ParentedPrefab,
				followerPrefab = ItemDisplayModel,
				childName = "LowerArmR",
				localPos = new Vector3(-0.00369f, 0.17648f, -0.04642f),
				localAngles = new Vector3(301.4718f, 339.5358f, 128.8567f),
				localScale = new Vector3(-0.07139f, 0.07139f, 0.07139f)
			}
		});
		itemDisplayRuleDict.Add("mdlTreebot", new ItemDisplayRule[1]
		{
			new ItemDisplayRule
			{
				ruleType = ItemDisplayRuleType.ParentedPrefab,
				followerPrefab = ItemDisplayModel,
				childName = "FlowerBase",
				localPos = new Vector3(0.87026f, -0.22491f, -0.68869f),
				localAngles = new Vector3(49.40244f, 52.55454f, 20.06599f),
				localScale = new Vector3(-0.12984f, 0.12984f, 0.12984f)
			}
		});
		itemDisplayRuleDict.Add("mdlLoader", new ItemDisplayRule[1]
		{
			new ItemDisplayRule
			{
				ruleType = ItemDisplayRuleType.ParentedPrefab,
				followerPrefab = ItemDisplayModel,
				childName = "LowerArmR",
				localPos = new Vector3(-0.00548f, 0.21343f, 0.00553f),
				localAngles = new Vector3(292.8608f, 268.1949f, 131.9294f),
				localScale = new Vector3(-0.05568f, 0.05568f, 0.05568f)
			}
		});
		itemDisplayRuleDict.Add("mdlCroco", new ItemDisplayRule[1]
		{
			new ItemDisplayRule
			{
				ruleType = ItemDisplayRuleType.ParentedPrefab,
				followerPrefab = ItemDisplayModel,
				childName = "LowerArmR",
				localPos = new Vector3(0.1771f, 2.02554f, -0.13919f),
				localAngles = new Vector3(302.9836f, 196.5584f, 126.9992f),
				localScale = new Vector3(-0.70847f, 0.70847f, 0.70847f)
			}
		});
		itemDisplayRuleDict.Add("mdlCaptain", new ItemDisplayRule[1]
		{
			new ItemDisplayRule
			{
				ruleType = ItemDisplayRuleType.ParentedPrefab,
				followerPrefab = ItemDisplayModel,
				childName = "LowerArmR",
				localPos = new Vector3(-0.00668f, 0.25148f, -0.00859f),
				localAngles = new Vector3(294.4885f, 318.7417f, 117.3321f),
				localScale = new Vector3(-0.06895f, 0.0721f, 0.06895f)
			}
		});
		itemDisplayRuleDict.Add("mdlBandit2", new ItemDisplayRule[1]
		{
			new ItemDisplayRule
			{
				ruleType = ItemDisplayRuleType.ParentedPrefab,
				followerPrefab = ItemDisplayModel,
				childName = "LowerArmR",
				localPos = new Vector3(0.00795f, 0.14443f, -0.00136f),
				localAngles = new Vector3(301.3001f, 191.1591f, 160.5972f),
				localScale = new Vector3(-0.06286f, 0.06286f, 0.06286f)
			}
		});
		itemDisplayRuleDict.Add("mdlRailGunner", new ItemDisplayRule[1]
		{
			new ItemDisplayRule
			{
				ruleType = ItemDisplayRuleType.ParentedPrefab,
				followerPrefab = ItemDisplayModel,
				childName = "LowerArmR",
				localPos = new Vector3(-0.00676f, 0.12313f, -0.00958f),
				localAngles = new Vector3(302.8572f, 298.891f, 140.0498f),
				localScale = new Vector3(-0.03564f, 0.03564f, 0.03564f)
			}
		});
		itemDisplayRuleDict.Add("mdlVoidSurvivor", new ItemDisplayRule[1]
		{
			new ItemDisplayRule
			{
				ruleType = ItemDisplayRuleType.ParentedPrefab,
				followerPrefab = ItemDisplayModel,
				childName = "ForeArmL",
				localPos = new Vector3(-0.00505f, 0.22645f, 0.00544f),
				localAngles = new Vector3(295.0421f, 257.7892f, 126.0312f),
				localScale = new Vector3(-0.06972f, 0.06972f, 0.06972f)
			}
		});
		itemDisplayRuleDict.Add("mdlScav", new ItemDisplayRule[1]
		{
			new ItemDisplayRule
			{
				ruleType = ItemDisplayRuleType.ParentedPrefab,
				followerPrefab = ItemDisplayModel,
				childName = "LowerArmL",
				localPos = new Vector3(0.11453f, 2.38999f, 0.02713f),
				localAngles = new Vector3(303.8772f, 102.5493f, 149.8182f),
				localScale = new Vector3(-0.74277f, 0.74277f, 0.74277f)
			}
		});
		itemDisplayRuleDict.Add("mdlEngiTurret", new ItemDisplayRule[1]
		{
			new ItemDisplayRule
			{
				ruleType = ItemDisplayRuleType.ParentedPrefab,
				followerPrefab = ItemDisplayModel,
				childName = "Neck",
				localPos = new Vector3(0.008f, 0.07348f, -0.00554f),
				localAngles = new Vector3(57.76626f, 227.158f, 320.98f),
				localScale = new Vector3(-0.28215f, 0.28215f, 0.28215f)
			}
		});
	}
}
