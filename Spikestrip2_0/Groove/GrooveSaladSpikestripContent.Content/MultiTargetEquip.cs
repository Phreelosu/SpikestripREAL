using System;
using System.Collections;
using System.Collections.Generic;
using EntityStates;
using HG;
using R2API;
using Rewired.ComponentControls.Effects;
using RoR2;
using RoR2.Orbs;
using TMPro;
using UnityEngine;
using Spikestrip2_0.Core;

namespace GrooveSaladSpikestripContent.Content;

public class MultiTargetEquip : EquipmentBase<MultiTargetEquip>
{
	public class MultiTargetEquipTargetBehaviour : EquipmentMultiTargetBehaviour
	{
		public override float MaxAngleFilter => 50f;

		public override float MaxDistance => 150f;

		public override BullseyeSearch.SortMode SortMode => BullseyeSearch.SortMode.DistanceAndAngle;

		public override TargetOption TargetType => TargetOption.Enemy;

		public override GameObject GetTargetIndicatorPrefab(EquipmentSlot.UserTargetInfo info)
		{
			return (info.hurtBox == currentTargets[0].hurtBox) ? mainIndicator : partialIndicator;
		}
	}

	public class CourierOrb : Orb
	{
		public float speed = 20f;

		public float damageValue;

		public GameObject attacker;

		public TeamIndex teamIndex;

		public bool isCrit;

		public float scale = 1f;

		public ProcChainMask procChainMask;

		public float procCoefficient = 0.2f;

		public DamageColorIndex damageColorIndex;

		public List<EquipmentSlot.UserTargetInfo> safeTargetsList;

		public GameObject effectPrefab;

		public override void Begin()
		{
			target = safeTargetsList[0].hurtBox;
			safeTargetsList.RemoveAt(0);
			base.duration = base.distanceToTarget / speed;
			EffectData effectData = new EffectData
			{
				scale = scale,
				origin = origin,
				genericFloat = base.duration
			};
			effectData.SetHurtBoxReference(target);
			EffectManager.SpawnEffect(effectPrefab, effectData, transmit: true);
		}

		public override void OnArrival()
		{
			if ((bool)target)
			{
				HealthComponent healthComponent = target.healthComponent;
				if ((bool)healthComponent)
				{
					DamageInfo damageInfo = new DamageInfo();
					damageInfo.damage = damageValue;
					damageInfo.attacker = attacker;
					damageInfo.inflictor = null;
					damageInfo.force = Vector3.zero;
					damageInfo.crit = isCrit;
					damageInfo.procChainMask = procChainMask;
					damageInfo.procCoefficient = procCoefficient;
					damageInfo.position = target.transform.position;
					damageInfo.damageColorIndex = damageColorIndex;
					healthComponent.TakeDamage(damageInfo);
					GlobalEventManager.instance.OnHitEnemy(damageInfo, healthComponent.gameObject);
					GlobalEventManager.instance.OnHitAll(damageInfo, healthComponent.gameObject);
				}
				while (safeTargetsList.Count > 0)
				{
					HurtBox hurtBox = safeTargetsList[0].hurtBox;
					if ((bool)hurtBox)
					{
						CourierOrb courierOrb = new CourierOrb();
						courierOrb.attacker = attacker;
						courierOrb.damageColorIndex = damageColorIndex;
						courierOrb.damageValue = damageValue;
						courierOrb.effectPrefab = effectPrefab;
						courierOrb.isCrit = isCrit;
						courierOrb.origin = target.transform.position;
						courierOrb.procChainMask = procChainMask;
						courierOrb.procCoefficient = procCoefficient;
						courierOrb.safeTargetsList = safeTargetsList;
						courierOrb.scale = scale;
						courierOrb.speed = speed;
						courierOrb.teamIndex = teamIndex;
						OrbManager.instance.AddOrb(courierOrb);
						return;
					}
					safeTargetsList.RemoveAt(0);
				}
			}
			EquipmentSlot equipmentSlot = (attacker ? attacker.GetComponent<EquipmentSlot>() : null);
			if ((bool)equipmentSlot)
			{
				((MonoBehaviour)equipmentSlot).StartCoroutine(ReturnCourierSphereServer(equipmentSlot.gameObject));
			}
		}

		private IEnumerator ReturnCourierSphereServer(GameObject ownerObject)
		{
			yield return new WaitForSeconds(0.5f);
			if (!ownerObject)
			{
				yield break;
			}
			CharacterBody characterBody = ownerObject.GetComponent<CharacterBody>();
			Inventory inventory = (characterBody ? characterBody.inventory : null);
			if ((bool)inventory)
			{
				EquipmentIndex equipmentIndex = inventory.currentEquipmentIndex;
				PickupIndex pickupIndex = PickupCatalog.FindPickupIndex(equipmentIndex);
				if (pickupIndex != PickupIndex.none)
				{
					PickupDropletController.CreatePickupDroplet(pickupIndex, characterBody.corePosition, new Vector3(UnityEngine.Random.Range(-4f, 4f), 20f, UnityEngine.Random.Range(-4f, 4f)));
				}
				inventory.SetEquipmentIndex(MultiTargetEquip.instance.EquipmentDef.equipmentIndex);
			}
		}
	}

	public class CourierTeleportInController : MonoBehaviour
	{
		private float age;

		private Material materialInstance;

		private static float timeMax = 2f;

		private static AnimationCurve alphaCurve = AnimationCurve.EaseInOut(0f, 1f, 1f, 0f);

		private MeshRenderer meshRenderer;

		public void Start()
		{
			Util.PlaySound(SpawnTeleporterState.soundString, base.gameObject);
			materialInstance = UnityEngine.Object.Instantiate(LegacyResourcesAPI.Load<Material>("materials/matSummonRoboBall"));
			meshRenderer = GetComponentInChildren<MeshRenderer>();
			Material[] array = meshRenderer.sharedMaterials;
			ArrayUtils.ArrayAppend(ref array, in materialInstance);
			meshRenderer.sharedMaterials = array;
		}

		public void Update()
		{
			age += Time.fixedDeltaTime;
			if (age >= timeMax)
			{
				UnityEngine.Object.Destroy(this);
			}
			if ((bool)materialInstance)
			{
				materialInstance.SetFloat("_ExternalAlpha", alphaCurve.Evaluate(age / timeMax));
			}
		}

		public void OnDestroy()
		{
			Material[] array = meshRenderer.sharedMaterials;
			ArrayUtils.ArrayRemoveAtAndResize(ref array, 1);
			meshRenderer.sharedMaterials = array;
			UnityEngine.Object.Destroy(materialInstance);
		}
	}

	public static Material matTracerBright = LegacyResourcesAPI.Load<GameObject>("prefabs/effects/tracers/TracerCaptainDefenseMatrix").transform.Find("TracerHead/Flash").gameObject.GetComponent<ParticleSystemRenderer>().material;

	public static GameObject orbEffect;

	public static GameObject impactEffect;

	public static GameObject mainIndicator;

	public static GameObject partialIndicator;

	public static Material trailMaterial;

	public static float damageCoef = 7f;

	public override string EquipmentToken => "MULTITARGET";

	public override string EquipmentName => "Courier Sphere";

	public override string EquipmentPickup => "Unleash the Courier Sphere on all monsters in view.";

	public override string EquipmentDescription => "Target all monsters in view with the Courier Sphere, dealing <style=cIsDamage>" + damageCoef * 100f + "% damage</style> to each.";

	public override string EquipmentLore => null;

	public override Sprite EquipmentIcon => SpikestripContent.GrooveBundle.LoadAsset<Sprite>("texCourierSphereIcon.png");

	public override float Cooldown => 30f;

	public override Type EquipmentBehaviour => typeof(MultiTargetEquipTargetBehaviour);

	public override bool CanBeRandomlyTriggered => false;

	public override void Init()
	{
		Material original = LegacyResourcesAPI.Load<GameObject>("prefabs/effects/LevelUpEffect").transform.Find("Spinner").GetComponent<ParticleSystemRenderer>().sharedMaterials[1];
		trailMaterial = UnityEngine.Object.Instantiate(original);
		trailMaterial.SetColor("_TintColor", SpikestripUtil.ColorRGB(177f, 187f, 237f));
		trailMaterial.mainTexture = null;
		base.Init();
		OrbAPI.AddOrb(typeof(CourierOrb));
		impactEffect = PrefabAPI.InstantiateClone(LegacyResourcesAPI.Load<GameObject>("prefabs/effects/omnieffect/OmniExplosionVFXFMJ"), "CourierSphereImpactEffect", false);
		UnityEngine.Object.DestroyImmediate(impactEffect.transform.Find("Chunks (1)").gameObject);
		EffectComponent component = impactEffect.GetComponent<EffectComponent>();
		component.soundName = "Play_captain_m2_tazer_impact";
		ContentAddition.AddEffect(impactEffect);
		orbEffect = PrefabAPI.InstantiateClone(LegacyResourcesAPI.Load<GameObject>("Prefabs/Effects/OrbEffects/WispOrbEffect"), "CourierSphereOrbEffect", false);
		UnityEngine.Object.DestroyImmediate(orbEffect.transform.Find("Mesh").gameObject);
		UnityEngine.Object.DestroyImmediate((UnityEngine.Object)(object)orbEffect.GetComponent<AkEvent>());
		UnityEngine.Object.DestroyImmediate((UnityEngine.Object)(object)orbEffect.GetComponent<AkEvent>());
		UnityEngine.Object.DestroyImmediate((UnityEngine.Object)(object)orbEffect.GetComponent<AkGameObj>());
		Material material = LegacyResourcesAPI.Load<GameObject>("prefabs/projectileghosts/SawmerangGhost").transform.Find("Mesh/mdlVFXDonut2").GetComponent<MeshRenderer>().sharedMaterials[1];
		orbEffect.transform.Find("Flames").GetComponent<ParticleSystemRenderer>().material = material;
		GameObject gameObject = PrefabAPI.InstantiateClone(EquipmentPickupModel.transform.Find("mdlCourierSphere").gameObject, "CourierSphere", false);
		gameObject.transform.SetParent(orbEffect.transform);
		gameObject.transform.localScale = Vector3.one * 0.25f;
		EffectComponent component2 = orbEffect.GetComponent<EffectComponent>();
		component2.soundName = "Play_commando_M2";
		VFXAttributes component3 = orbEffect.GetComponent<VFXAttributes>();
		component3.vfxPriority = VFXAttributes.VFXPriority.Always;
		OrbEffect component4 = orbEffect.GetComponent<OrbEffect>();
		component4.endEffect = impactEffect;
		GameObject gameObject2 = new GameObject("CourierTrail");
		TrailRenderer trailRenderer = gameObject2.AddComponent<TrailRenderer>();
		trailRenderer.startColor = SpikestripUtil.ColorRGB(58f, 58f, 255f);
		trailRenderer.endColor = Color.white;
		trailRenderer.startWidth = 1f;
		trailRenderer.endWidth = 0f;
		trailRenderer.time = 0.5f;
		trailRenderer.widthMultiplier = 0.6f;
		trailRenderer.material = trailMaterial;
		gameObject2.transform.SetParent(orbEffect.transform);
		ContentAddition.AddEffect(orbEffect);
		Color color = SpikestripUtil.ColorRGB(208f, 217f, 237f);
		mainIndicator = PrefabAPI.InstantiateClone(LegacyResourcesAPI.Load<GameObject>("prefabs/WoodSpriteIndicator"), "CourierSphereMainIndicator", false);
		SpriteRenderer componentInChildren = mainIndicator.GetComponentInChildren<SpriteRenderer>();
		componentInChildren.color = color;
		componentInChildren.sprite = SpikestripContent.GrooveBundle.LoadAsset<Sprite>("texCourierSphereIndicatorMain.png");
		TextMeshPro componentInChildren2 = mainIndicator.GetComponentInChildren<TextMeshPro>();
		componentInChildren2.color = color;
		RotateAroundAxis componentInChildren3 = mainIndicator.GetComponentInChildren<RotateAroundAxis>();
		componentInChildren3.fastRotationSpeed = 180f;
		componentInChildren3.speed = RotateAroundAxis.Speed.Fast;
		UnityEngine.Object.DestroyImmediate(mainIndicator.GetComponentInChildren<ObjectScaleCurve>());
		UnityEngine.Object.DestroyImmediate(mainIndicator.GetComponentInChildren<RotateAroundAxis>());
		mainIndicator.transform.Find("Holder").localScale = 0.35f * Vector3.one;
		partialIndicator = PrefabAPI.InstantiateClone(mainIndicator, "CourierSpherePartialIndicator", false);
		UnityEngine.Object.DestroyImmediate(partialIndicator.transform.Find("TextMeshPro").gameObject);
	}

	public Vector3 OriginFromBody(CharacterBody body)
	{
		if ((bool)body.modelLocator)
		{
			Transform modelTransform = body.modelLocator.modelTransform;
			if ((bool)modelTransform)
			{
				CharacterModel component = modelTransform.GetComponent<CharacterModel>();
				if ((bool)component)
				{
					for (int num = component.parentedPrefabDisplays.Count - 1; num >= 0; num--)
					{
						if (component.parentedPrefabDisplays[num].equipmentIndex == EquipmentDef.equipmentIndex)
						{
							GameObject gameObject = component.parentedPrefabDisplays[num].instance;
							if ((bool)gameObject)
							{
								ItemFollower componentInChildren = gameObject.GetComponentInChildren<ItemFollower>();
								if ((bool)componentInChildren && (bool)componentInChildren.followerInstance)
								{
									return componentInChildren.followerInstance.transform.position;
								}
							}
						}
					}
				}
			}
		}
		return body.corePosition;
	}

	public override bool OnActivationServer(EquipmentSlot equipmentSlot)
	{
		CharacterBody characterBody = equipmentSlot.characterBody;
		EquipmentMultiTargetBehaviour component = equipmentSlot.GetComponent<EquipmentMultiTargetBehaviour>();
		if ((bool)characterBody && (bool)component)
		{
			component.UpdateTargets();
			if (component.currentTargets.Count != 0)
			{
				CourierOrb courierOrb = new CourierOrb();
				courierOrb.attacker = equipmentSlot.gameObject;
				courierOrb.effectPrefab = orbEffect;
				courierOrb.damageColorIndex = DamageColorIndex.Item;
				courierOrb.damageValue = characterBody.damage * damageCoef;
				courierOrb.isCrit = characterBody.RollCrit();
				courierOrb.origin = OriginFromBody(characterBody);
				courierOrb.procChainMask = default(ProcChainMask);
				courierOrb.procCoefficient = 1f;
				courierOrb.safeTargetsList = new List<EquipmentSlot.UserTargetInfo>((IEnumerable<EquipmentSlot.UserTargetInfo>)component.currentTargets);
				courierOrb.scale = 1f;
				courierOrb.speed = 100f;
				courierOrb.teamIndex = characterBody.teamComponent.teamIndex;
				OrbManager.instance.AddOrb(courierOrb);
				if ((bool)equipmentSlot.inventory)
				{
					equipmentSlot.inventory.SetEquipmentIndex(EquipmentIndex.None);
				}
				component.InvalidateCurrentTargets();
				return true;
			}
		}
		return false;
	}

	public override void AssignEquipmentValues()
	{
		EquipmentPickupModel = SpikestripUtil.UpdateGameObjectShader(SpikestripContent.GrooveBundle.LoadAsset<GameObject>("CourierSphere.prefab"));
		MeshRenderer componentInChildren = EquipmentPickupModel.GetComponentInChildren<MeshRenderer>();
		componentInChildren.material.EnableKeyword("FORCE_SPEC");
		componentInChildren.material.SetFloat("_SpecularStrength", 0.6f);
		GameObject gameObject = PrefabAPI.InstantiateClone(EquipmentPickupModel, "CourierSphereDisplay", false);
		gameObject.AddComponent<CourierTeleportInController>();
		Transform transform = gameObject.transform.Find("mdlCourierSphere");
		transform.localRotation = Util.QuaternionSafeLookRotation(Vector3.forward);
		EquipmentDisplayModel = SpikestripUtil.EmptyPrefab("DisplayCourierSphereParent");
		RotateAroundAxis rotateAroundAxis = EquipmentDisplayModel.AddComponent<RotateAroundAxis>();
		rotateAroundAxis.rotateAroundAxis = RotateAroundAxis.RotationAxis.Y;
		rotateAroundAxis.speed = RotateAroundAxis.Speed.Slow;
		rotateAroundAxis.slowRotationSpeed = 60f;
		rotateAroundAxis.relativeTo = Space.World;
		GameObject gameObject2 = new GameObject("CourierSphereHolder");
		gameObject2.transform.SetParent(EquipmentDisplayModel.transform);
		gameObject2.transform.localPosition = new Vector3(3f, 0f, 0f);
		gameObject2.transform.localScale = Vector3.one * 0.15f;
		RotateAroundAxis rotateAroundAxis2 = gameObject2.AddComponent<RotateAroundAxis>();
		rotateAroundAxis2.rotateAroundAxis = RotateAroundAxis.RotationAxis.Y;
		rotateAroundAxis2.speed = RotateAroundAxis.Speed.Slow;
		rotateAroundAxis2.slowRotationSpeed = 60f;
		rotateAroundAxis2.relativeTo = Space.World;
		ItemDisplay itemDisplay = gameObject2.AddComponent<ItemDisplay>();
		itemDisplay.rendererInfos = Array.Empty<CharacterModel.RendererInfo>();
		ItemFollower itemFollower = gameObject2.AddComponent<ItemFollower>();
		itemFollower.targetObject = gameObject2;
		itemFollower.followerPrefab = gameObject;
		itemFollower.distanceDampTime = 0.15f;
		itemFollower.distanceMaxSpeed = 200f;
	}

	public override void PopulateItemDisplayRules(ItemDisplayRuleDict itemDisplayRuleDict)
	{
		itemDisplayRuleDict.Add("mdlCommandoDualies", new ItemDisplayRule[1]
		{
			new ItemDisplayRule
			{
				ruleType = ItemDisplayRuleType.ParentedPrefab,
				followerPrefab = EquipmentDisplayModel,
				childName = "Base",
				localPos = new Vector3(0f, 0f, 0f),
				localAngles = new Vector3(0f, 0f, 0f),
				localScale = new Vector3(0.32757f, 0.32757f, 0.32757f)
			}
		});
		itemDisplayRuleDict.Add("mdlHuntress", new ItemDisplayRule[1]
		{
			new ItemDisplayRule
			{
				ruleType = ItemDisplayRuleType.ParentedPrefab,
				followerPrefab = EquipmentDisplayModel,
				childName = "Base",
				localPos = new Vector3(0f, 0f, 0f),
				localAngles = new Vector3(0f, 0f, 0f),
				localScale = new Vector3(0.32757f, 0.32757f, 0.32757f)
			}
		});
		itemDisplayRuleDict.Add("mdlToolbot", new ItemDisplayRule[1]
		{
			new ItemDisplayRule
			{
				ruleType = ItemDisplayRuleType.ParentedPrefab,
				followerPrefab = EquipmentDisplayModel,
				childName = "Base",
				localPos = new Vector3(0f, 0f, 0f),
				localAngles = new Vector3(0f, 0f, 0f),
				localScale = new Vector3(2.281123f, 2.281123f, 2.281123f)
			}
		});
		itemDisplayRuleDict.Add("mdlEngi", new ItemDisplayRule[1]
		{
			new ItemDisplayRule
			{
				ruleType = ItemDisplayRuleType.ParentedPrefab,
				followerPrefab = EquipmentDisplayModel,
				childName = "Base",
				localPos = new Vector3(0f, 0f, 0f),
				localAngles = new Vector3(0f, 0f, 0f),
				localScale = new Vector3(0.32757f, 0.32757f, 0.32757f)
			}
		});
		itemDisplayRuleDict.Add("mdlMage", new ItemDisplayRule[1]
		{
			new ItemDisplayRule
			{
				ruleType = ItemDisplayRuleType.ParentedPrefab,
				followerPrefab = EquipmentDisplayModel,
				childName = "Base",
				localPos = new Vector3(0f, 0f, 0f),
				localAngles = new Vector3(0f, 0f, 0f),
				localScale = new Vector3(0.32757f, 0.32757f, 0.32757f)
			}
		});
		itemDisplayRuleDict.Add("mdlMerc", new ItemDisplayRule[1]
		{
			new ItemDisplayRule
			{
				ruleType = ItemDisplayRuleType.ParentedPrefab,
				followerPrefab = EquipmentDisplayModel,
				childName = "Base",
				localPos = new Vector3(0f, 0f, 0f),
				localAngles = new Vector3(0f, 0f, 0f),
				localScale = new Vector3(0.32757f, 0.32757f, 0.32757f)
			}
		});
		itemDisplayRuleDict.Add("mdlTreebot", new ItemDisplayRule[1]
		{
			new ItemDisplayRule
			{
				ruleType = ItemDisplayRuleType.ParentedPrefab,
				followerPrefab = EquipmentDisplayModel,
				childName = "Base",
				localPos = new Vector3(0f, 0f, -1.5f),
				localAngles = new Vector3(0f, 0f, 0f),
				localScale = new Vector3(0.7898852f, 0.7898852f, 0.7898852f)
			}
		});
		itemDisplayRuleDict.Add("mdlLoader", new ItemDisplayRule[1]
		{
			new ItemDisplayRule
			{
				ruleType = ItemDisplayRuleType.ParentedPrefab,
				followerPrefab = EquipmentDisplayModel,
				childName = "Base",
				localPos = new Vector3(0f, 0f, 0f),
				localAngles = new Vector3(0f, 0f, 0f),
				localScale = new Vector3(0.32757f, 0.32757f, 0.32757f)
			}
		});
		itemDisplayRuleDict.Add("mdlCroco", new ItemDisplayRule[1]
		{
			new ItemDisplayRule
			{
				ruleType = ItemDisplayRuleType.ParentedPrefab,
				followerPrefab = EquipmentDisplayModel,
				childName = "Base",
				localPos = new Vector3(0f, 0f, 0f),
				localAngles = new Vector3(0f, 0f, 0f),
				localScale = new Vector3(3.321737f, 3.321737f, 3.321737f)
			}
		});
		itemDisplayRuleDict.Add("mdlCaptain", new ItemDisplayRule[1]
		{
			new ItemDisplayRule
			{
				ruleType = ItemDisplayRuleType.ParentedPrefab,
				followerPrefab = EquipmentDisplayModel,
				childName = "Base",
				localPos = new Vector3(0f, 0f, 0f),
				localAngles = new Vector3(0f, 0f, 0f),
				localScale = new Vector3(0.32757f, 0.32757f, 0.32757f)
			}
		});
		itemDisplayRuleDict.Add("mdlBandit2", new ItemDisplayRule[1]
		{
			new ItemDisplayRule
			{
				ruleType = ItemDisplayRuleType.ParentedPrefab,
				followerPrefab = EquipmentDisplayModel,
				childName = "Base",
				localPos = new Vector3(0f, 0f, 0f),
				localAngles = new Vector3(0f, 0f, 0f),
				localScale = new Vector3(0.32757f, 0.32757f, 0.32757f)
			}
		});
		itemDisplayRuleDict.Add("mdlRailGunner", new ItemDisplayRule[1]
		{
			new ItemDisplayRule
			{
				ruleType = ItemDisplayRuleType.ParentedPrefab,
				followerPrefab = EquipmentDisplayModel,
				childName = "Base",
				localPos = new Vector3(0f, 0f, 0f),
				localAngles = new Vector3(0f, 0f, 0f),
				localScale = new Vector3(0.32757f, 0.32757f, 0.32757f)
			}
		});
		itemDisplayRuleDict.Add("mdlVoidSurvivor", new ItemDisplayRule[1]
		{
			new ItemDisplayRule
			{
				ruleType = ItemDisplayRuleType.ParentedPrefab,
				followerPrefab = EquipmentDisplayModel,
				childName = "Base",
				localPos = new Vector3(0f, 0f, 0f),
				localAngles = new Vector3(0f, 0f, 0f),
				localScale = new Vector3(0.4437197f, 0.4437197f, 0.4437197f)
			}
		});
		itemDisplayRuleDict.Add("mdlScav", new ItemDisplayRule[1]
		{
			new ItemDisplayRule
			{
				ruleType = ItemDisplayRuleType.ParentedPrefab,
				followerPrefab = EquipmentDisplayModel,
				childName = "Base",
				localPos = new Vector3(0f, 0f, 0f),
				localAngles = new Vector3(0f, 0f, 0f),
				localScale = new Vector3(6.516652f, 6.516652f, 6.516652f)
			}
		});
		itemDisplayRuleDict.Add("mdlEquipmentDrone", new ItemDisplayRule[1]
		{
			new ItemDisplayRule
			{
				ruleType = ItemDisplayRuleType.ParentedPrefab,
				followerPrefab = EquipmentDisplayModel,
				childName = "HeadCenter",
				localPos = new Vector3(0f, 0f, 0f),
				localAngles = new Vector3(0f, 0f, 0f),
				localScale = new Vector3(0.5482084f, 0.5482084f, 0.5482084f)
			}
		});
	}
}
