using System;
using System.Collections.Generic;
using On.RoR2;
using R2API;
using RoR2;
using RoR2.Projectile;
using UnityEngine;
using UnityEngine.Networking;
using Spikestrip2_0.Core;

namespace GrooveSaladSpikestripContent.Content;

public class WarpedElite : EliteBase<WarpedElite>
{
	public class GravityAffixBuffBehaviourServer : RoR2.CharacterBody.ItemBehavior
	{
		public static float influenceRadius = 12f;

		public static float baseRechargeTime = 1f;

		public static float baseAttemptFireTime = 0.05f;

		private float rechargeTimer;

		private float attemptFireTimer;

		private float currentRechargeTime => baseRechargeTime / body.attackSpeed;

		public void FixedUpdate()
		{
			attemptFireTimer += Time.fixedDeltaTime;
			rechargeTimer -= Time.fixedDeltaTime;
			if (attemptFireTimer > baseAttemptFireTime)
			{
				attemptFireTimer -= baseAttemptFireTime;
				if (rechargeTimer <= 0f && DeflectNearbyProjectiles())
				{
					rechargeTimer = currentRechargeTime;
				}
			}
		}

		private bool DeflectNearbyProjectiles()
		{
			Vector3 corePosition = body.corePosition;
			TeamIndex teamIndex = body.teamComponent.teamIndex;
			float num = influenceRadius * influenceRadius;
			List<ProjectileController> instancesList = RoR2.InstanceTracker.GetInstancesList<ProjectileController>();
			for (int i = 0; i < instancesList.Count; i++)
			{
				ProjectileController projectileController = instancesList[i];
				if (projectileController.teamFilter.teamIndex != teamIndex && (projectileController.transform.position - corePosition).sqrMagnitude < num && ScrewWithProjectile(projectileController))
				{
					RoR2.EffectManager.SpawnEffect(bubblePopEffect, new RoR2.EffectData
					{
						origin = projectileController.transform.position,
						scale = 0.8f,
						rotation = RoR2.Util.QuaternionSafeLookRotation(projectileController.transform.up)
					}, transmit: true);
					return true;
				}
			}
			return false;
		}

		public bool ScrewWithProjectile(ProjectileController controller)
		{
			ProjectileSimple component = controller.GetComponent<ProjectileSimple>();
			if ((bool)component && component.desiredForwardSpeed > 0f)
			{
				component.desiredForwardSpeed = 0f - component.desiredForwardSpeed;
				component.SetForwardSpeed(component.desiredForwardSpeed);
				return true;
			}
			ProjectileTargetComponent component2 = controller.GetComponent<ProjectileTargetComponent>();
			if ((bool)component2 && (bool)controller.owner && component2.target != controller.owner.transform)
			{
				component2.target = controller.owner.transform;
				return true;
			}
			BoomerangProjectile component3 = controller.GetComponent<BoomerangProjectile>();
			if ((bool)component3 && component3.boomerangState != BoomerangProjectile.BoomerangState.FlyBack)
			{
				component3.NetworkboomerangState = BoomerangProjectile.BoomerangState.FlyBack;
				component3.onFlyBack?.Invoke();
				return true;
			}
			return false;
		}
	}

	public class GravityBuffBehaviour : RoR2.CharacterBody.ItemBehavior
	{
		public static float antiGravCoef = 100f;

		public ICharacterGravityParameterProvider characterGravityParameterProvider;

		public ICharacterFlightParameterProvider characterFlightParameterProvider;

		public GameObject vfxInstance;

		public void Start()
		{
			characterGravityParameterProvider = GetComponent<ICharacterGravityParameterProvider>();
			characterFlightParameterProvider = GetComponent<ICharacterFlightParameterProvider>();
			if (characterGravityParameterProvider != null)
			{
				RoR2.CharacterGravityParameters gravityParameters = characterGravityParameterProvider.gravityParameters;
				gravityParameters.channeledAntiGravityGranterCount++;
				characterGravityParameterProvider.gravityParameters = gravityParameters;
			}
			if (characterFlightParameterProvider != null)
			{
				RoR2.CharacterFlightParameters flightParameters = characterFlightParameterProvider.flightParameters;
				flightParameters.channeledFlightGranterCount++;
				characterFlightParameterProvider.flightParameters = flightParameters;
			}
			vfxInstance = UnityEngine.Object.Instantiate(gravityBubbleVFXPrefab, body.corePosition, Quaternion.identity);
			Vector3 vector = Vector3.one * body.bestFitRadius * 0.7f;
			vfxInstance.transform.localScale = vector;
			vfxInstance.GetComponent<RoR2.ObjectScaleCurve>().baseScale = vector;
		}

		public void FixedUpdate()
		{
			if (NetworkServer.active)
			{
				ServerFixedUpdate();
			}
		}

		public void ServerFixedUpdate()
		{
			RoR2.HealthComponent healthComponent = body.healthComponent;
			if ((bool)healthComponent)
			{
				healthComponent.TakeDamageForce(-Physics.gravity * Time.fixedDeltaTime * antiGravCoef, alwaysApply: true);
			}
		}

		public void LateUpdate()
		{
			if ((bool)vfxInstance)
			{
				vfxInstance.transform.position = body.corePosition;
			}
		}

		public void OnDestroy()
		{
			if (characterGravityParameterProvider != null)
			{
				RoR2.CharacterGravityParameters gravityParameters = characterGravityParameterProvider.gravityParameters;
				gravityParameters.channeledAntiGravityGranterCount--;
				characterGravityParameterProvider.gravityParameters = gravityParameters;
			}
			if (characterFlightParameterProvider != null)
			{
				RoR2.CharacterFlightParameters flightParameters = characterFlightParameterProvider.flightParameters;
				flightParameters.channeledFlightGranterCount--;
				characterFlightParameterProvider.flightParameters = flightParameters;
			}
			if ((bool)vfxInstance)
			{
				RoR2.EffectManager.SpawnEffect(bubblePopEffect, new RoR2.EffectData
				{
					origin = body.corePosition,
					scale = vfxInstance.transform.localScale.magnitude * 0.4f
				}, transmit: false);
				UnityEngine.Object.Destroy(vfxInstance);
			}
		}
	}

	public static Color BubblesTint = SpikestripUtil.ColorRGB(165f, 72f, 191f);

	public static RoR2.BuffDef gravityBuff;

	public static GameObject gravityBubbleVFXPrefab;

	public static GameObject bubblePopEffect;

	public override string EliteName => "Warped";

	public override Color EliteColor => SpikestripUtil.ColorRGB(203f, 141f, 239f);

	public override EliteTierDefinition MainEliteTierDefinition => EliteBase<WarpedElite>.tierOneEliteDefault;

	public override EliteTierDefinition[] ExtraEliteTierDefitions => new EliteTierDefinition[1] { EliteBase<WarpedElite>.honorEliteDefault };

	public override Sprite AffixBuffSprite => SpikestripContent.GrooveBundle.LoadAsset<Sprite>("texBuffAffixGravity.png");

	public override Sprite EquipmentIcon => SpikestripContent.GrooveBundle.LoadAsset<Sprite>("texAffixGravityIcon.png");

	public override string EquipmentName => "Misplaced Faith";

	public override string AffixDescriptionMainWord => "gravity";

	public override Texture2D EliteRampTexture => SpikestripContent.GrooveBundle.LoadAsset<Texture2D>("texRampEliteGravity.png");

	public override Type AffixBuffBehaviour => typeof(GravityAffixBuffBehaviourServer);

	public override bool ServerOnlyAffixBuffBehaviour => true;

	public override void Init()
	{
		base.Init();
		gravityBuff = ScriptableObject.CreateInstance<RoR2.BuffDef>();
		gravityBuff.name = "GravityBuff";
		gravityBuff.buffColor = SpikestripUtil.ColorRGB(219f, 176f, 252f);
		gravityBuff.canStack = false;
		gravityBuff.iconSprite = SpikestripContent.GrooveBundle.LoadAsset<Sprite>("texBuffGravity.png");
		gravityBuff.isDebuff = true;
		ContentAddition.AddBuffDef(gravityBuff);
		BuffBehaviours.Register(gravityBuff, typeof(GravityBuffBehaviour), serverOnly: false);
		gravityBubbleVFXPrefab = PrefabAPI.InstantiateClone(LegacyResourcesAPI.Load<GameObject>("prefabs/networkedobjects/BlueprintStation").transform.Find("mdlBazaarBlueprintTable/BubbleMesh").gameObject, "GravityBubbleVFX", false);
		RoR2.ObjectScaleCurve objectScaleCurve = gravityBubbleVFXPrefab.AddComponent<RoR2.ObjectScaleCurve>();
		objectScaleCurve.overallCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
		objectScaleCurve.timeMax = 0.3f;
		objectScaleCurve.useOverallCurveOnly = true;
		MeshRenderer component = gravityBubbleVFXPrefab.GetComponent<MeshRenderer>();
		Material material = UnityEngine.Object.Instantiate(component.material);
		material.SetColor("_TintColor", BubblesTint);
		component.material = material;
		bubblePopEffect = PrefabAPI.InstantiateClone(LegacyResourcesAPI.Load<GameObject>("prefabs/effects/muzzleflashes/Bandit2SmokeBomb"), "GravityBubblePop", false);
		Renderer[] componentsInChildren = bubblePopEffect.GetComponentsInChildren<Renderer>();
		foreach (Renderer renderer in componentsInChildren)
		{
			renderer.material = material;
		}
		ParticleSystem[] componentsInChildren2 = bubblePopEffect.GetComponentsInChildren<ParticleSystem>();
		foreach (ParticleSystem particleSystem in componentsInChildren2)
		{
			ParticleSystem.MainModule main = particleSystem.main;
			main.scalingMode = ParticleSystemScalingMode.Hierarchy;
		}
		Light componentInChildren = bubblePopEffect.GetComponentInChildren<Light>();
		componentInChildren.color = BubblesTint;
		RoR2.EffectComponent component2 = bubblePopEffect.GetComponent<RoR2.EffectComponent>();
		component2.soundName = "Play_item_lunar_secondaryReplace_explode";
		component2.applyScale = true;
		ContentAddition.AddEffect(bubblePopEffect);
		On.RoR2.CharacterBody.RecalculateStats += CharacterBody_RecalculateStats;
	}

	private void CharacterBody_RecalculateStats(On.RoR2.CharacterBody.orig_RecalculateStats orig, RoR2.CharacterBody self)
	{
		if (self.HasBuff(gravityBuff))
		{
			self._isSprinting = false;
		}
		orig(self);
	}

	public override void OnHitEnemyServer(RoR2.DamageInfo damageInfo, GameObject victim)
	{
		RoR2.CharacterBody component = damageInfo.attacker.GetComponent<RoR2.CharacterBody>();
		RoR2.CharacterBody characterBody = (victim ? victim.GetComponent<RoR2.CharacterBody>() : null);
		if ((bool)component && component.HasBuff(AffixBuff) && (bool)characterBody)
		{
			characterBody.AddTimedBuff(gravityBuff, 4f * damageInfo.procCoefficient);
		}
	}

	public override void AssignEquipmentValues()
	{
		EquipmentPickupModel = CreateAffixModel(SpikestripUtil.ColorRGB(178f, 98f, 200f));
		EquipmentDisplayModel = PrefabAPI.InstantiateClone(LegacyResourcesAPI.Load<GameObject>("prefabs/networkedobjects/BlueprintStation").transform.Find("mdlBazaarBlueprintTable").gameObject, "AffixWarpedDisplay", false);
		UnityEngine.Object.DestroyImmediate(EquipmentDisplayModel.GetComponent<MeshRenderer>());
		UnityEngine.Object.DestroyImmediate(EquipmentDisplayModel.GetComponent<MeshFilter>());
		UnityEngine.Object.DestroyImmediate(EquipmentDisplayModel.GetComponent<MeshCollider>());
		UnityEngine.Object.DestroyImmediate(EquipmentDisplayModel.GetComponent<RoR2.EntityLocator>());
		UnityEngine.Object.DestroyImmediate(EquipmentDisplayModel.transform.Find("Display").gameObject);
		Transform transform = EquipmentDisplayModel.transform.Find("BubbleParticles");
		ParticleSystemRenderer component = transform.GetComponent<ParticleSystemRenderer>();
		ParticleSystem component2 = transform.GetComponent<ParticleSystem>();
		ParticleSystem.MainModule main = component2.main;
		main.scalingMode = ParticleSystemScalingMode.Hierarchy;
		Material material = UnityEngine.Object.Instantiate(component.material);
		material.SetColor("_TintColor", BubblesTint);
		material.EnableKeyword("DITHER");
		component.material = material;
		transform.transform.localScale = Vector3.one * 3.2f;
		int num = 0;
		List<Transform> list = new List<Transform>();
		MeshRenderer[] componentsInChildren = EquipmentDisplayModel.GetComponentsInChildren<MeshRenderer>();
		foreach (MeshRenderer meshRenderer in componentsInChildren)
		{
			if (num >= 3)
			{
				UnityEngine.Object.DestroyImmediate(meshRenderer.gameObject);
				continue;
			}
			num++;
			meshRenderer.material = material;
			list.Add(meshRenderer.transform);
		}
		list[0].localScale = Vector3.one * 2.5f;
		list[0].localPosition = new Vector3(0f, 1f, 0f);
		list[1].localScale = Vector3.one * 1.75f;
		list[1].localPosition = new Vector3(0.6f, -0.5f, 0f);
		list[2].localScale = Vector3.one * 1.25f;
		list[2].localPosition = new Vector3(-0.5f, 0.5f);
		RoR2.ItemDisplay itemDisplay = EquipmentDisplayModel.AddComponent<RoR2.ItemDisplay>();
		itemDisplay.rendererInfos = SpikestripUtil.GenerateRendererInfos(EquipmentDisplayModel);
	}

	public override void PopulateItemDisplayRules(ItemDisplayRuleDict itemDisplayRuleDict)
	{
		RegisterDisplayParent(LegacyResourcesAPI.Load<RoR2.EquipmentDef>("EquipmentDefs/AffixPoison"));
	}
}
