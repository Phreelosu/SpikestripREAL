using System;
using HG;
using On.RoR2;
using R2API;
using Rewired.ComponentControls.Effects;
using RoR2;
using UnityEngine;
using UnityEngine.Networking;
using Spikestrip2_0.Core;

namespace GrooveSaladSpikestripContent.Content;

public class InfinityCodex : ItemBase<InfinityCodex>
{
	public static Color logbookColor = SpikestripUtil.ColorRGB(244f, 216f, 255f);

	public static Color effectTint = SpikestripUtil.ColorRGB(207f, 177f, 219f);

	public static DamageColorIndex codexColorIndex = ColorsAPI.RegisterDamageColor(logbookColor);

	public static GameObject impactEffect;

	public override string ItemToken => "INFINITYCODEX";

	public override string ItemName => "Infinity Codex";

	public override string ItemPickup => "Deal extra damage to enemies in your Monster Log Database.";

	public override string ItemDescription => "Deal an additional <style=cIsDamage>50%</style> damage <style=cStack>(+50% per stack)</style> to enemies in your <style=cIsUtility>Monster Log Database</style>.";

	public override string ItemLore => "\"...His conquest was unrivaled, for none could hope to rival him. He knew everything; nothing was outside his reach. Every traitorious plot, every political subtlety, and every nuance of battle, understood in a singular instant of serenity. Some tried of course. They burned him 'till nothing but his bones remained. They stabbed him through the heart with a poisonous blade. But he could not die, and he did not forget. As his empire grew, from horizon to horizon, planet to planet, universe to universe, it became clear that only one other could challenge him - infinity herself...\"\n\n- Galactic Fables, Volume XXV";

	public override ItemTier Tier => ItemTier.Tier3;

	public override ItemTag[] ItemTags => new ItemTag[2]
	{
		ItemTag.Damage,
		ItemTag.AIBlacklist
	};

	public override Sprite ItemIconSprite => SpikestripContent.GrooveBundle.LoadAsset<Sprite>("texInfinityCodexIcon.png");

	public override void Init()
	{
		base.Init();
		impactEffect = PrefabAPI.InstantiateClone(LegacyResourcesAPI.Load<GameObject>("prefabs/effects/impacteffects/Critspark"), "CodexImpact", false);
		ParticleSystemRenderer[] componentsInChildren = impactEffect.GetComponentsInChildren<ParticleSystemRenderer>();
		foreach (ParticleSystemRenderer particleSystemRenderer in componentsInChildren)
		{
			Material material = UnityEngine.Object.Instantiate(particleSystemRenderer.material);
			particleSystemRenderer.material = material;
			ParticleSystem component = particleSystemRenderer.GetComponent<ParticleSystem>();
			ParticleSystem.MainModule main = component.main;
			main.startColor = effectTint;
			particleSystemRenderer.transform.localScale = Vector3.one * 1f;
		}
		GameObject gameObject = PrefabAPI.InstantiateClone(SpikestripUtil.LegacyLoad<GameObject>("Prefabs/Effects/ImpactEffects/ImpactRailgunLight").transform.Find("Daggers").gameObject, "CodexDaggers", false);
		ParticleSystemRenderer component2 = gameObject.GetComponent<ParticleSystemRenderer>();
		Material material2 = UnityEngine.Object.Instantiate(component2.material);
		material2.SetTexture("_RemapTex", SpikestripUtil.AddressablesLoad<Texture>("2574f290f1aba294f9e6879c37bd1350"));
		material2.SetColor("_TintColor", effectTint);
		material2.SetFloat("_Boost", 5f);
		component2.material = material2;
		gameObject.transform.SetParent(impactEffect.transform);
		gameObject.transform.localPosition = Vector3.zero;
		gameObject.transform.Rotate(Vector3.forward, 45f, Space.World);
		RoR2.EffectComponent component3 = impactEffect.GetComponent<RoR2.EffectComponent>();
		component3.soundName = "Play_lunar_exploder_m1_impact";
		ContentAddition.AddEffect(impactEffect);
		On.RoR2.HealthComponent.TakeDamage += HealthComponent_TakeDamage;
	}

	private void HealthComponent_TakeDamage(On.RoR2.HealthComponent.orig_TakeDamage orig, RoR2.HealthComponent self, RoR2.DamageInfo damageInfo)
	{
		float? num = null;
		if (NetworkServer.active)
		{
			RoR2.DeathRewards component = self.GetComponent<RoR2.DeathRewards>();
			RoR2.CharacterBody characterBody = (damageInfo.attacker ? damageInfo.attacker.GetComponent<RoR2.CharacterBody>() : null);
			if ((bool)characterBody && (bool)component && component.logUnlockableDef != null)
			{
				RoR2.CharacterMaster master = characterBody.master;
				if ((bool)master && (bool)master.inventory && master.inventory.GetItemCountEffective(ItemDef) > 0)
				{
					RoR2.PlayerCharacterMasterController component2 = master.GetComponent<RoR2.PlayerCharacterMasterController>();
					if ((bool)component2)
					{
						RoR2.NetworkUser networkUser = component2.networkUser;
						if ((bool)networkUser)
						{
							RoR2.LocalUser localUser = networkUser.localUser;
							if (localUser != null && localUser.userProfile.HasUnlockable(component.logUnlockableDef))
							{
								num = 1f + 0.5f * (float)master.inventory.GetItemCountEffective(ItemDef);
								damageInfo.damage *= num.Value;
								damageInfo.damageColorIndex = codexColorIndex;
								RoR2.EffectManager.SimpleImpactEffect(impactEffect, damageInfo.position, -damageInfo.force, transmit: true);
							}
						}
					}
				}
			}
		}
		orig(self, damageInfo);
		if (num.HasValue)
		{
			damageInfo.damage /= num.Value;
		}
	}

	public override void AssignItemModels()
	{
		Material value = LegacyResourcesAPI.Load<GameObject>("prefabs/networkedobjects/LogPickup").GetComponentInChildren<MeshRenderer>().sharedMaterials[1];
		Material material = SpikestripUtil.artifactShellDistortionMaterial;
		ItemPickupModel = SpikestripUtil.UpdateGameObjectShader(SpikestripContent.GrooveBundle.LoadAsset<GameObject>("InfinityCodex.prefab"));
		Transform transform = ItemPickupModel.transform.Find("Main");
		transform.SetAsFirstSibling();
		MeshRenderer component = transform.GetComponent<MeshRenderer>();
		Material[] array = component.sharedMaterials;
		ArrayUtils.ArrayAppend(ref array, in value);
		component.SetSharedMaterials(array, array.Length);
		Transform transform2 = ItemPickupModel.transform.Find("Sphere");
		transform2.GetComponent<MeshRenderer>().material = value;
		Transform transform3 = ItemPickupModel.transform.Find("Distortion1");
		RotateAroundAxis rotateAroundAxis = transform3.gameObject.AddComponent<RotateAroundAxis>();
		rotateAroundAxis.speed = RotateAroundAxis.Speed.Slow;
		rotateAroundAxis.slowRotationSpeed = 40f;
		rotateAroundAxis.relativeTo = Space.Self;
		rotateAroundAxis.rotateAroundAxis = RotateAroundAxis.RotationAxis.X;
		transform3.GetComponent<MeshRenderer>().material = material;
		Transform transform4 = ItemPickupModel.transform.Find("Distortion2");
		RotateAroundAxis rotateAroundAxis2 = transform4.gameObject.AddComponent<RotateAroundAxis>();
		rotateAroundAxis2.speed = RotateAroundAxis.Speed.Slow;
		rotateAroundAxis2.slowRotationSpeed = 50f;
		rotateAroundAxis2.relativeTo = Space.Self;
		rotateAroundAxis2.rotateAroundAxis = RotateAroundAxis.RotationAxis.Y;
		transform4.GetComponent<MeshRenderer>().material = material;
		Transform transform5 = ItemPickupModel.transform.Find("Distortion3");
		RotateAroundAxis rotateAroundAxis3 = transform5.gameObject.AddComponent<RotateAroundAxis>();
		rotateAroundAxis3.speed = RotateAroundAxis.Speed.Slow;
		rotateAroundAxis3.slowRotationSpeed = 60f;
		rotateAroundAxis3.relativeTo = Space.Self;
		rotateAroundAxis3.rotateAroundAxis = RotateAroundAxis.RotationAxis.Z;
		transform5.GetComponent<MeshRenderer>().material = material;
		ItemDisplayModel = SpikestripUtil.EmptyPrefab("DisplayInfinityCodexParent");
		RoR2.ItemDisplay itemDisplay = ItemDisplayModel.AddComponent<RoR2.ItemDisplay>();
		itemDisplay.rendererInfos = Array.Empty<RoR2.CharacterModel.RendererInfo>();
		RoR2.ItemFollower itemFollower = ItemDisplayModel.AddComponent<RoR2.ItemFollower>();
		itemFollower.targetObject = ItemDisplayModel;
		itemFollower.followerPrefab = ItemPickupModel;
		itemFollower.distanceDampTime = 0.025f;
		itemFollower.distanceMaxSpeed = 0f;
		RotateAroundAxis rotateAroundAxis4 = ItemDisplayModel.AddComponent<RotateAroundAxis>();
		rotateAroundAxis4.speed = RotateAroundAxis.Speed.Slow;
		rotateAroundAxis4.slowRotationSpeed = 40f;
		rotateAroundAxis4.relativeTo = Space.Self;
		rotateAroundAxis4.rotateAroundAxis = RotateAroundAxis.RotationAxis.Z;
	}

	public override void PopulateItemDisplayRules(ItemDisplayRuleDict itemDisplayRuleDict)
	{
		itemDisplayRuleDict.Add("mdlCommandoDualies", new RoR2.ItemDisplayRule[1]
		{
			new RoR2.ItemDisplayRule
			{
				ruleType = ItemDisplayRuleType.ParentedPrefab,
				followerPrefab = ItemDisplayModel,
				childName = "HandL",
				localPos = new Vector3(-0.0118f, 0.10399f, 0.21944f),
				localAngles = new Vector3(90f, 23.1812f, 0f),
				localScale = new Vector3(0.20156f, 0.20156f, 0.20156f)
			}
		});
		itemDisplayRuleDict.Add("mdlHuntress", new RoR2.ItemDisplayRule[1]
		{
			new RoR2.ItemDisplayRule
			{
				ruleType = ItemDisplayRuleType.ParentedPrefab,
				followerPrefab = ItemDisplayModel,
				childName = "Arrow",
				localPos = new Vector3(0.17142f, 0.00257f, -0.11466f),
				localAngles = new Vector3(0.00079f, 270.0016f, 347.0542f),
				localScale = new Vector3(0.154f, 0.154f, 0.154f)
			}
		});
		itemDisplayRuleDict.Add("mdlToolbot", new RoR2.ItemDisplayRule[1]
		{
			new RoR2.ItemDisplayRule
			{
				ruleType = ItemDisplayRuleType.ParentedPrefab,
				followerPrefab = ItemDisplayModel,
				childName = "HandR",
				localPos = new Vector3(0.23248f, 3.41215f, 1.03342f),
				localAngles = new Vector3(90f, 270.6725f, 0f),
				localScale = new Vector3(0.48898f, 0.48898f, 0.48898f)
			}
		});
		itemDisplayRuleDict.Add("mdlEngi", new RoR2.ItemDisplayRule[1]
		{
			new RoR2.ItemDisplayRule
			{
				ruleType = ItemDisplayRuleType.ParentedPrefab,
				followerPrefab = ItemDisplayModel,
				childName = "HandL",
				localPos = new Vector3(-0.01359f, 0.3892f, -0.03742f),
				localAngles = new Vector3(90f, 274.7973f, 0f),
				localScale = new Vector3(0.28403f, 0.28403f, 0.28403f)
			}
		});
		itemDisplayRuleDict.Add("mdlMage", new RoR2.ItemDisplayRule[1]
		{
			new RoR2.ItemDisplayRule
			{
				ruleType = ItemDisplayRuleType.ParentedPrefab,
				followerPrefab = ItemDisplayModel,
				childName = "HandL",
				localPos = new Vector3(0.01081f, 0.3493f, -0.06109f),
				localAngles = new Vector3(90f, 185.4058f, 0f),
				localScale = new Vector3(0.18999f, 0.18999f, 0.18999f)
			}
		});
		itemDisplayRuleDict.Add("mdlMerc", new RoR2.ItemDisplayRule[1]
		{
			new RoR2.ItemDisplayRule
			{
				ruleType = ItemDisplayRuleType.ParentedPrefab,
				followerPrefab = ItemDisplayModel,
				childName = "HandR",
				localPos = new Vector3(-0.01527f, 0.42046f, 0.00802f),
				localAngles = new Vector3(90f, 61.07398f, 0f),
				localScale = new Vector3(0.20442f, 0.20442f, 0.20442f)
			}
		});
		itemDisplayRuleDict.Add("mdlTreebot", new RoR2.ItemDisplayRule[1]
		{
			new RoR2.ItemDisplayRule
			{
				ruleType = ItemDisplayRuleType.ParentedPrefab,
				followerPrefab = ItemDisplayModel,
				childName = "PlatformBase",
				localPos = new Vector3(0.78366f, 2.99204f, -0.30956f),
				localAngles = new Vector3(270.8016f, 112.376f, 140.4246f),
				localScale = new Vector3(0.38452f, 0.38452f, 0.38452f)
			}
		});
		itemDisplayRuleDict.Add("mdlLoader", new RoR2.ItemDisplayRule[1]
		{
			new RoR2.ItemDisplayRule
			{
				ruleType = ItemDisplayRuleType.ParentedPrefab,
				followerPrefab = ItemDisplayModel,
				childName = "MechHandRight",
				localPos = new Vector3(-0.00774f, 0.61206f, -0.01084f),
				localAngles = new Vector3(90f, 273.5195f, 0f),
				localScale = new Vector3(0.28912f, 0.28912f, 0.28912f)
			}
		});
		itemDisplayRuleDict.Add("mdlCroco", new RoR2.ItemDisplayRule[1]
		{
			new RoR2.ItemDisplayRule
			{
				ruleType = ItemDisplayRuleType.ParentedPrefab,
				followerPrefab = ItemDisplayModel,
				childName = "Head",
				localPos = new Vector3(0.01286f, 6.69856f, 0.84139f),
				localAngles = new Vector3(90f, 189.246f, 0f),
				localScale = new Vector3(0.33528f, 0.33528f, 0.33528f)
			}
		});
		itemDisplayRuleDict.Add("mdlCaptain", new RoR2.ItemDisplayRule[1]
		{
			new RoR2.ItemDisplayRule
			{
				ruleType = ItemDisplayRuleType.ParentedPrefab,
				followerPrefab = ItemDisplayModel,
				childName = "HandR",
				localPos = new Vector3(-0.01373f, 0.41272f, 0.01367f),
				localAngles = new Vector3(90f, 240.6622f, 0f),
				localScale = new Vector3(0.22458f, 0.22458f, 0.22458f)
			}
		});
		itemDisplayRuleDict.Add("mdlBandit2", new RoR2.ItemDisplayRule[1]
		{
			new RoR2.ItemDisplayRule
			{
				ruleType = ItemDisplayRuleType.ParentedPrefab,
				followerPrefab = ItemDisplayModel,
				childName = "MainWeapon",
				localPos = new Vector3(0.03278f, 0.48047f, -0.15187f),
				localAngles = new Vector3(90f, 261.3419f, 0f),
				localScale = new Vector3(0.19061f, 0.19061f, 0.19061f)
			}
		});
		itemDisplayRuleDict.Add("mdlRailGunner", new RoR2.ItemDisplayRule[1]
		{
			new RoR2.ItemDisplayRule
			{
				ruleType = ItemDisplayRuleType.ParentedPrefab,
				followerPrefab = ItemDisplayModel,
				childName = "Backpack",
				localPos = new Vector3(0.47912f, 0.24076f, 0.02718f),
				localAngles = new Vector3(357.3892f, 87.60921f, 262.9175f),
				localScale = new Vector3(0.19061f, 0.19061f, 0.19061f)
			}
		});
		itemDisplayRuleDict.Add("mdlVoidSurvivor", new RoR2.ItemDisplayRule[1]
		{
			new RoR2.ItemDisplayRule
			{
				ruleType = ItemDisplayRuleType.ParentedPrefab,
				followerPrefab = ItemDisplayModel,
				childName = "ShoulderL",
				localPos = new Vector3(0.44676f, 0.61637f, -0.66275f),
				localAngles = new Vector3(339.801f, 173.9947f, 81.37047f),
				localScale = new Vector3(0.19061f, 0.19061f, 0.19061f)
			}
		});
		itemDisplayRuleDict.Add("mdlScav", new RoR2.ItemDisplayRule[1]
		{
			new RoR2.ItemDisplayRule
			{
				ruleType = ItemDisplayRuleType.ParentedPrefab,
				followerPrefab = ItemDisplayModel,
				childName = "HandL",
				localPos = new Vector3(0.51037f, 4.94177f, -0.05225f),
				localAngles = new Vector3(90f, 127.6054f, 0f),
				localScale = new Vector3(1.03384f, 1.03384f, 1.03384f)
			}
		});
		itemDisplayRuleDict.Add("mdlEngiTurret", new RoR2.ItemDisplayRule[1]
		{
			new RoR2.ItemDisplayRule
			{
				ruleType = ItemDisplayRuleType.ParentedPrefab,
				followerPrefab = ItemDisplayModel,
				childName = "Head",
				localPos = new Vector3(0.0413f, 0.73552f, -2.28041f),
				localAngles = new Vector3(0.0001f, 180.0001f, 332.1636f),
				localScale = new Vector3(0.46173f, 0.46173f, 0.46173f)
			}
		});
	}
}
