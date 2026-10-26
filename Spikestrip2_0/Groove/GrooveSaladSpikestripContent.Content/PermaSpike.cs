using System;
using System.Collections.Generic;
using R2API;
using Rewired.ComponentControls.Effects;
using RoR2;
using RoR2.Projectile;
using UnityEngine;
using UnityEngine.Networking;
using Spikestrip2_0.Core;

namespace GrooveSaladSpikestripContent.Content;

public class PermaSpike : ItemBase<PermaSpike>
{
	public class PermaSpikeBehaviourServer : CharacterBody.ItemBehavior
	{
		public static float baseFireInterval = 5f;

		public static float damageCoef = 1.25f;

		public static float stackingDamageCoef = 0.5f;

		private float fireTimer;

		private void FixedUpdate()
		{
			fireTimer += Time.fixedDeltaTime;
			float num = baseFireInterval / body.attackSpeed;
			if (fireTimer >= num)
			{
				fireTimer -= num;
				FireSingle();
			}
		}

		public void FireSingle()
		{
			Transform transform = (body.coreTransform ? body.coreTransform : body.transform);
			Vector3 up = transform.up;
			Vector3 position = body.corePosition;
			ModelLocator modelLocator = body.modelLocator;
			if ((bool)modelLocator && (bool)modelLocator.modelTransform)
			{
				CharacterModel component = modelLocator.modelTransform.GetComponent<CharacterModel>();
				if ((bool)component)
				{
					for (int num = component.parentedPrefabDisplays.Count - 1; num >= 0; num--)
					{
						if (component.parentedPrefabDisplays[num].itemIndex == PermaSpike.instance.ItemDef.itemIndex)
						{
							GameObject instance = component.parentedPrefabDisplays[num].instance;
							up = instance.transform.up;
							position = instance.transform.position;
							EffectManager.SpawnEffect(spawnEffectPrefab, new EffectData
							{
								origin = instance.transform.position,
								scale = 1.5f
							}, transmit: true);
						}
					}
				}
			}
			float num2 = Vector3.Angle(-transform.forward, up);
			float t = Mathf.Clamp01(num2 / 90f);
			float maxSpread = Mathf.Lerp(0f, 45f, t);
			Vector3 forward = Util.ApplySpread(up, 0f, maxSpread, 1f, 1f);
			FireProjectileInfo fireProjectileInfo = new FireProjectileInfo
			{
				crit = body.RollCrit(),
				damage = body.damage * (damageCoef + (float)(stack - 1) * stackingDamageCoef),
				owner = body.gameObject,
				position = position,
				rotation = Quaternion.LookRotation(forward),
				projectilePrefab = projectilePrefab,
				force = 0f,
				speedOverride = 20f,
				useSpeedOverride = true,
				damageTypeOverride = DamageType.SlowOnHit
			};
			ProjectileManager.instance.FireProjectile(fireProjectileInfo);
		}
	}

	public class PermaSpikeDotZone : ProjectileDotZone
	{
		public int maxHits = 30;

		public GameObject destroyEffect;

		public float destroyEffectScale = 1f;

		private int hitCount;

		public new void FixedUpdate()
		{
			if (!NetworkServer.active)
			{
				return;
			}
			totalStopwatch += Time.fixedDeltaTime;
			resetStopwatch += Time.fixedDeltaTime;
			fireStopwatch += Time.fixedDeltaTime;
			if (resetStopwatch >= 1f / resetFrequency)
			{
				ResetOverlap();
				resetStopwatch -= 1f / resetFrequency;
			}
			if (fireStopwatch >= 1f / fireFrequency)
			{
				List<HurtBox> list = new List<HurtBox>();
				attack.Fire(list);
				hitCount += list.Count;
				fireStopwatch -= 1f / fireFrequency;
			}
			if ((lifetime > 0f && totalStopwatch >= lifetime) || hitCount >= maxHits)
			{
				EffectManager.SpawnEffect(destroyEffect, new EffectData
				{
					origin = base.transform.position,
					scale = destroyEffectScale
				}, transmit: true);
				onEnd.Invoke();
				if (!string.IsNullOrEmpty(soundLoopStopString))
				{
					Util.PlaySound(soundLoopStopString, base.gameObject);
				}
				UnityEngine.Object.Destroy(base.gameObject);
			}
		}
	}

	public class InstantiatePermaSpikeVisual : MonoBehaviour
	{
		public static Vector3 offset = Vector3.down * 0.5f;

		public GameObject visualInstance;

		private void Start()
		{
			visualInstance = UnityEngine.Object.Instantiate(permaSpikeVisualPrefab, base.transform.position + offset, Util.QuaternionSafeLookRotation(Vector3.up));
			visualInstance.transform.Rotate(Vector3.up, UnityEngine.Random.Range(0f, 360f), Space.World);
			visualInstance.GetComponentInChildren<TeamAreaIndicator>().teamFilter = GetComponent<TeamFilter>();
		}

		private void Update()
		{
			if ((bool)visualInstance)
			{
				visualInstance.transform.position = base.transform.position + offset;
			}
		}

		private void OnDestroy()
		{
			if ((bool)visualInstance)
			{
				UnityEngine.Object.Destroy(visualInstance);
			}
		}
	}

	public static GameObject projectileGhostPrefab;

	public static GameObject permaSpikeVisualPrefab;

	public static GameObject projectilePrefab;

	public static GameObject destroyEffectPrefab;

	public static GameObject impactEffectPrefab;

	public static GameObject spawnEffectPrefab;

	public static int maximumHitCount = 16;

	public override string ItemToken => "PERMASPIKE";

	public override string ItemName => "Perma-Spike Factory";

	public override string ItemPickup => "Produce near-permanent spike piles for massive damage.";

	public override string ItemDescription => "Slowly produce <style=cIsUtility>near-permanent</style> spike piles that deal up to <style=cIsDamage>" + (float)maximumHitCount * PermaSpikeBehaviourServer.damageCoef * 100f + "%</style> <style=cStack>(+" + (float)maximumHitCount * PermaSpikeBehaviourServer.stackingDamageCoef * 100f + "% per stack)</style> base damage each. Production rate <style=cIsDamage>increases with attack speed</style>.";

	public override string ItemLore => null;

	public override ItemTier Tier => ItemTier.Tier3;

	public override ItemTag[] ItemTags => new ItemTag[2]
	{
		ItemTag.Damage,
		ItemTag.Utility
	};

	public override Sprite ItemIconSprite => SpikestripContent.GrooveBundle.LoadAsset<Sprite>("texPermaSpikeIcon.png");

	public override Type ItemBehaviour => typeof(PermaSpikeBehaviourServer);

	public override bool ServerOnlyBehaviour => true;

	public override void Init()
	{
		base.Init();
		destroyEffectPrefab = PrefabAPI.InstantiateClone(LegacyResourcesAPI.Load<GameObject>("prefabs/effects/impacteffects/CryoCanisterExplosionPrimary"), "PermaSpikeDestructionEffect", false);
		destroyEffectPrefab.GetComponent<EffectComponent>().soundName = "Play_item_use_recycler";
		ContentAddition.AddEffect(destroyEffectPrefab);
		impactEffectPrefab = PrefabAPI.InstantiateClone(LegacyResourcesAPI.Load<GameObject>("prefabs/effects/impacteffects/Hitspark1"), "PermaSpikeImpactEffect", false);
		impactEffectPrefab.GetComponent<EffectComponent>().soundName = "Play_merc_shift_slice";
		ContentAddition.AddEffect(impactEffectPrefab);
		spawnEffectPrefab = PrefabAPI.InstantiateClone(LegacyResourcesAPI.Load<GameObject>("prefabs/effects/impacteffects/BehemothVFX"), "PermaSpikeSpawnedEffect", false);
		spawnEffectPrefab.GetComponent<EffectComponent>().soundName = "Play_MULT_m2_main_explode";
		ContentAddition.AddEffect(spawnEffectPrefab);
		GameObject gameObject = PrefabAPI.InstantiateClone(LegacyResourcesAPI.Load<GameObject>("prefabs/projectiles/SporeGrenadeProjectileDotZone").transform.Find("FX/ScaledOnImpact/TeamAreaIndicator, GroundOnly").gameObject, "PermaSpikeTeamIndicator", false);
		permaSpikeVisualPrefab = SpikestripUtil.UpdateGameObjectShader(PrefabAPI.InstantiateClone(SpikestripContent.GrooveBundle.LoadAsset<GameObject>("mdlPermaSpike.prefab"), "mdlPermaSpike", false));
		permaSpikeVisualPrefab.transform.localScale = Vector3.one * 0.3f;
		permaSpikeVisualPrefab.transform.localPosition = new Vector3(0f, 0f, -0.5f);
		ObjectScaleCurve objectScaleCurve = permaSpikeVisualPrefab.AddComponent<ObjectScaleCurve>();
		objectScaleCurve.useOverallCurveOnly = true;
		objectScaleCurve.timeMax = 0.2f;
		objectScaleCurve.overallCurve = AnimationCurve.EaseInOut(0f, 0.4f, 1f, 1f);
		BoxCollider boxCollider = permaSpikeVisualPrefab.AddComponent<BoxCollider>();
		boxCollider.size = new Vector3(2f, 2f, 2f);
		boxCollider.isTrigger = true;
		MeshRenderer component = permaSpikeVisualPrefab.GetComponent<MeshRenderer>();
		component.material.SetFloat("_SpecularStrength", 0.5f);
		DitherModel ditherModel = permaSpikeVisualPrefab.AddComponent<DitherModel>();
		ditherModel.bounds = boxCollider;
		ditherModel.renderers = new Renderer[1] { component };
		gameObject.transform.SetParent(permaSpikeVisualPrefab.transform);
		gameObject.transform.localPosition = Vector3.zero;
		gameObject.transform.localRotation = Util.QuaternionSafeLookRotation(Vector3.forward);
		gameObject.transform.localScale = 9f * Vector3.one;
		gameObject.GetComponent<TeamAreaIndicator>().teamFilter = null;
		projectilePrefab = PrefabAPI.InstantiateClone(LegacyResourcesAPI.Load<GameObject>("prefabs/projectiles/LunarSecondaryProjectile"), "PermaSpikeProjectile", true);
		UnityEngine.Object.Destroy(projectilePrefab.GetComponent<ProjectileExplosion>());
		UnityEngine.Object.Destroy(projectilePrefab.GetComponent<ProjectileFuse>());
		Rigidbody component2 = projectilePrefab.GetComponent<Rigidbody>();
		component2.useGravity = true;
		RigidbodySoundOnImpact component3 = projectilePrefab.GetComponent<RigidbodySoundOnImpact>();
		component3.networkedSoundEvent = SpikestripUtil.nseFromString("Play_char_land");
		ProjectileController component4 = projectilePrefab.GetComponent<ProjectileController>();
		component4.flightSoundLoop = null;
		component4.startSound = "";
		SpikestripUtil.SetProjectileGhost(component4, null);
		component4.procCoefficient = 0.2f;
		ProjectileSimple component5 = projectilePrefab.GetComponent<ProjectileSimple>();
		component5.lifetime = 999f;
		ProjectileDotZone component6 = projectilePrefab.GetComponent<ProjectileDotZone>();
		UnityEngine.Object.Destroy(component6);
		PermaSpikeDotZone permaSpikeDotZone = projectilePrefab.AddComponent<PermaSpikeDotZone>();
		permaSpikeDotZone.damageCoefficient = 1f;
		permaSpikeDotZone.destroyEffect = destroyEffectPrefab;
		permaSpikeDotZone.destroyEffectScale = 2f;
		permaSpikeDotZone.fireFrequency = 10f;
		permaSpikeDotZone.forceVector = Vector3.zero;
		permaSpikeDotZone.impactEffect = impactEffectPrefab;
		permaSpikeDotZone.lifetime = 120f;
		permaSpikeDotZone.maxHits = maximumHitCount;
		permaSpikeDotZone.overlapProcCoefficient = 1f;
		permaSpikeDotZone.resetFrequency = 10f;
		InstantiatePermaSpikeVisual instantiatePermaSpikeVisual = projectilePrefab.AddComponent<InstantiatePermaSpikeVisual>();
		ContentAddition.AddProjectile(projectilePrefab);
		PrefabAPI.RegisterNetworkPrefab(projectilePrefab);
	}

	public override void AssignItemModels()
	{
		ItemPickupModel = SpikestripUtil.UpdateGameObjectShader(SpikestripContent.GrooveBundle.LoadAsset<GameObject>("DisplaySpactory.prefab"));
		RotateAroundAxis rotateAroundAxis = ItemPickupModel.transform.Find("Gear1").gameObject.AddComponent<RotateAroundAxis>();
		rotateAroundAxis.fastRotationSpeed = 120f;
		rotateAroundAxis.fastRotationSpeed = 60f;
		rotateAroundAxis.relativeTo = Space.Self;
		rotateAroundAxis.rotateAroundAxis = RotateAroundAxis.RotationAxis.Z;
		rotateAroundAxis.speed = RotateAroundAxis.Speed.Fast;
		RotateAroundAxis rotateAroundAxis2 = ItemPickupModel.transform.Find("Gear2").gameObject.AddComponent<RotateAroundAxis>();
		rotateAroundAxis2.fastRotationSpeed = -120f;
		rotateAroundAxis2.fastRotationSpeed = -60f;
		rotateAroundAxis2.relativeTo = Space.Self;
		rotateAroundAxis2.rotateAroundAxis = RotateAroundAxis.RotationAxis.Z;
		rotateAroundAxis2.speed = RotateAroundAxis.Speed.Fast;
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
				childName = "Chest",
				localPos = new Vector3(0.02125f, 0.29282f, -0.27626f),
				localAngles = new Vector3(284.7305f, 6.42226f, 350.2798f),
				localScale = new Vector3(0.03225f, 0.03225f, 0.03225f)
			}
		});
		itemDisplayRuleDict.Add("mdlHuntress", new ItemDisplayRule[1]
		{
			new ItemDisplayRule
			{
				ruleType = ItemDisplayRuleType.ParentedPrefab,
				followerPrefab = ItemDisplayModel,
				childName = "Chest",
				localPos = new Vector3(0.15123f, 0.07813f, -0.09049f),
				localAngles = new Vector3(0f, 35.01055f, 254.2948f),
				localScale = new Vector3(0.0295f, 0.0295f, 0.0295f)
			}
		});
		itemDisplayRuleDict.Add("mdlToolbot", new ItemDisplayRule[1]
		{
			new ItemDisplayRule
			{
				ruleType = ItemDisplayRuleType.ParentedPrefab,
				followerPrefab = ItemDisplayModel,
				childName = "Chest",
				localPos = new Vector3(-0.02351f, 1.23755f, -2.83764f),
				localAngles = new Vector3(358.5504f, 270.2298f, 94.39895f),
				localScale = new Vector3(0.27846f, 0.27846f, 0.27846f)
			}
		});
		itemDisplayRuleDict.Add("mdlEngi", new ItemDisplayRule[1]
		{
			new ItemDisplayRule
			{
				ruleType = ItemDisplayRuleType.ParentedPrefab,
				followerPrefab = ItemDisplayModel,
				childName = "Chest",
				localPos = new Vector3(0.01912f, 0.25917f, -0.4138f),
				localAngles = new Vector3(2.90796f, 90.45725f, 279.1638f),
				localScale = new Vector3(0.04097f, 0.04097f, 0.04097f)
			}
		});
		itemDisplayRuleDict.Add("mdlMage", new ItemDisplayRule[1]
		{
			new ItemDisplayRule
			{
				ruleType = ItemDisplayRuleType.ParentedPrefab,
				followerPrefab = ItemDisplayModel,
				childName = "Chest",
				localPos = new Vector3(0.00255f, 0.13168f, -0.41747f),
				localAngles = new Vector3(2.08449f, 269.7406f, 78.52254f),
				localScale = new Vector3(0.03492f, 0.03492f, 0.03492f)
			}
		});
		itemDisplayRuleDict.Add("mdlMerc", new ItemDisplayRule[1]
		{
			new ItemDisplayRule
			{
				ruleType = ItemDisplayRuleType.ParentedPrefab,
				followerPrefab = ItemDisplayModel,
				childName = "Chest",
				localPos = new Vector3(-0.00367f, 0.15076f, -0.28886f),
				localAngles = new Vector3(1.85309f, 272.9712f, 82.36889f),
				localScale = new Vector3(0.0339f, 0.0339f, 0.0339f)
			}
		});
		itemDisplayRuleDict.Add("mdlTreebot", new ItemDisplayRule[1]
		{
			new ItemDisplayRule
			{
				ruleType = ItemDisplayRuleType.ParentedPrefab,
				followerPrefab = ItemDisplayModel,
				childName = "PlatformBase",
				localPos = new Vector3(-0.00367f, 0.25925f, -0.9625f),
				localAngles = new Vector3(270.8075f, 112.2631f, 247.4078f),
				localScale = new Vector3(0.09746f, 0.09746f, 0.09746f)
			}
		});
		itemDisplayRuleDict.Add("mdlLoader", new ItemDisplayRule[1]
		{
			new ItemDisplayRule
			{
				ruleType = ItemDisplayRuleType.ParentedPrefab,
				followerPrefab = ItemDisplayModel,
				childName = "Chest",
				localPos = new Vector3(-0.00058f, 0.1337f, -0.42667f),
				localAngles = new Vector3(1.52349f, 269.2571f, 77.58772f),
				localScale = new Vector3(0.035f, 0.035f, 0.035f)
			}
		});
		itemDisplayRuleDict.Add("mdlCroco", new ItemDisplayRule[1]
		{
			new ItemDisplayRule
			{
				ruleType = ItemDisplayRuleType.ParentedPrefab,
				followerPrefab = ItemDisplayModel,
				childName = "SpineChest2",
				localPos = new Vector3(-0.18394f, 0.9527f, -0.24538f),
				localAngles = new Vector3(0.10063f, 269.654f, 13.89797f),
				localScale = new Vector3(0.28828f, 0.28828f, 0.28828f)
			}
		});
		itemDisplayRuleDict.Add("mdlCaptain", new ItemDisplayRule[1]
		{
			new ItemDisplayRule
			{
				ruleType = ItemDisplayRuleType.ParentedPrefab,
				followerPrefab = ItemDisplayModel,
				childName = "Chest",
				localPos = new Vector3(-0.04154f, 0.05967f, -0.3635f),
				localAngles = new Vector3(2.97072f, 277.484f, 89.98537f),
				localScale = new Vector3(0.03882f, 0.03882f, 0.03882f)
			}
		});
		itemDisplayRuleDict.Add("mdlBandit2", new ItemDisplayRule[1]
		{
			new ItemDisplayRule
			{
				ruleType = ItemDisplayRuleType.ParentedPrefab,
				followerPrefab = ItemDisplayModel,
				childName = "Chest",
				localPos = new Vector3(-0.00063f, 0.14312f, -0.22772f),
				localAngles = new Vector3(2.18026f, 272.3965f, 97.56212f),
				localScale = new Vector3(0.02704f, 0.02704f, 0.02704f)
			}
		});
		itemDisplayRuleDict.Add("mdlRailGunner", new ItemDisplayRule[1]
		{
			new ItemDisplayRule
			{
				ruleType = ItemDisplayRuleType.ParentedPrefab,
				followerPrefab = ItemDisplayModel,
				childName = "Backpack",
				localPos = new Vector3(0.01332f, -0.04792f, -0.20783f),
				localAngles = new Vector3(3.45252f, 265.3984f, 92.98593f),
				localScale = new Vector3(0.02939f, 0.02939f, 0.02939f)
			}
		});
		itemDisplayRuleDict.Add("mdlVoidSurvivor", new ItemDisplayRule[1]
		{
			new ItemDisplayRule
			{
				ruleType = ItemDisplayRuleType.ParentedPrefab,
				followerPrefab = ItemDisplayModel,
				childName = "Chest",
				localPos = new Vector3(0.02019f, -0.04208f, -0.28914f),
				localAngles = new Vector3(8.48233f, 272.2287f, 117.4198f),
				localScale = new Vector3(0.02829f, 0.02829f, 0.02829f)
			}
		});
		itemDisplayRuleDict.Add("mdlScav", new ItemDisplayRule[1]
		{
			new ItemDisplayRule
			{
				ruleType = ItemDisplayRuleType.ParentedPrefab,
				followerPrefab = ItemDisplayModel,
				childName = "Weapon",
				localPos = new Vector3(0.06079f, 17.01487f, 0.07803f),
				localAngles = new Vector3(2.24629f, 273.3501f, 0.08317f),
				localScale = new Vector3(0.43131f, 0.43131f, 0.43131f)
			}
		});
		itemDisplayRuleDict.Add("mdlEngiTurret", new ItemDisplayRule[1]
		{
			new ItemDisplayRule
			{
				ruleType = ItemDisplayRuleType.ParentedPrefab,
				followerPrefab = ItemDisplayModel,
				childName = "Head",
				localPos = new Vector3(-0.00058f, 1.28464f, -0.23108f),
				localAngles = new Vector3(0f, 270f, 0f),
				localScale = new Vector3(0.14161f, 0.14161f, 0.14161f)
			}
		});
	}
}
