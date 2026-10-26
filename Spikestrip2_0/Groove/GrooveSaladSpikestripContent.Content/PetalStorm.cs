using System;
using EntityStates;
using R2API;
using RoR2;
using RoR2.Projectile;
using RoR2.Skills;
using RoR2.UI;
using UnityEngine;
using UnityEngine.Networking;
using Spikestrip2_0.Core;

namespace GrooveSaladSpikestripContent.Content;

public class PetalStorm : SpikestripAltSkillsBase<PetalStorm>
{
	public class FireTreebotPlantPrimary : BaseState
	{
		public static float baseDuration = 0.25f;

		public static float baseSpreadBloomIncrease = 0.065f;

		public static float healthCostFraction = 0.05f;

		public static float damageCoefficient = 1.9f;

		private float spreadBloomIncrease;

		private float duration;

		public override void OnEnter()
		{
			base.OnEnter();
			duration = baseDuration / attackSpeedStat;
			spreadBloomIncrease = baseSpreadBloomIncrease / attackSpeedStat;
			Util.PlaySound("Play_treeBot_R_variant_shoot", base.gameObject);
			((EntityState)this).PlayAnimation("Gesture, Additive", "FireFlower", "FireFlower.playbackRate", duration * 2f);
			base.characterBody.AddSpreadBloom(spreadBloomIncrease);
			if (base.characterBody.spreadBloomAngle >= 1f)
			{
				Util.PlaySound("Play_treeBot_shift_charge", base.gameObject);
				if (NetworkServer.active && (bool)base.healthComponent && healthCostFraction >= Mathf.Epsilon)
				{
					DamageInfo damageInfo = new DamageInfo();
					damageInfo.damage = base.healthComponent.combinedHealth * healthCostFraction;
					damageInfo.position = base.characterBody.corePosition;
					damageInfo.force = Vector3.zero;
					damageInfo.damageColorIndex = DamageColorIndex.Default;
					damageInfo.crit = false;
					damageInfo.attacker = null;
					damageInfo.inflictor = null;
					damageInfo.damageType = DamageType.NonLethal;
					damageInfo.procCoefficient = 0f;
					damageInfo.procChainMask = default(ProcChainMask);
					base.healthComponent.TakeDamage(damageInfo);
				}
			}
			Ray aimRay = GetAimRay();
			Vector3 forward = Util.ApplySpread(aimRay.direction, 0f, base.characterBody.spreadBloomAngle, 1f, 1f);
			if (base.isAuthority)
			{
				FireProjectileInfo fireProjectileInfo = new FireProjectileInfo
				{
					projectilePrefab = treebotPlantProjectilePrefab,
					position = aimRay.origin,
					rotation = Util.QuaternionSafeLookRotation(forward),
					owner = base.gameObject,
					damage = damageStat * damageCoefficient,
					crit = Util.CheckRoll(critStat, base.characterBody.master),
					force = 300f
				};
				ProjectileManager.instance.FireProjectile(fireProjectileInfo);
			}
		}

		public override void FixedUpdate()
		{
			base.FixedUpdate();
			if (base.fixedAge >= duration && base.isAuthority)
			{
				outer.SetNextStateToMain();
			}
		}

		public override void OnExit()
		{
			base.OnExit();
		}

		public override InterruptPriority GetMinimumInterruptPriority()
		{
			return InterruptPriority.Skill;
		}
	}

	[RequireComponent(typeof(ProjectileController))]
	public class ProjectileDestroyOnWorld : MonoBehaviour, IProjectileImpactBehavior
	{
		private ProjectileController projectileController;

		public GameObject impactEffect;

		private void Awake()
		{
			projectileController = GetComponent<ProjectileController>();
		}

		public void OnProjectileImpact(ProjectileImpactInfo impactInfo)
		{
			Collider collider = impactInfo.collider;
			if (!collider)
			{
				return;
			}
			HurtBox component = collider.GetComponent<HurtBox>();
			if (!component)
			{
				if (NetworkServer.active && (bool)impactEffect)
				{
					EffectManager.SimpleImpactEffect(impactEffect, impactInfo.estimatedPointOfImpact, -base.transform.forward, !projectileController.isPrediction);
				}
				UnityEngine.Object.Destroy(base.gameObject);
			}
		}
	}

	public static GameObject treebotPlantProjectilePrefab;

	public static GameObject treebotPlantGhostPrefab;

	public static GameObject impactVFX;

	public override string SkillName => "Petal Storm";

	public override string SkillDescription => "Quickly launch <style=cIsDamage>piercing</style> petals for <style=cIsDamage>" + FireTreebotPlantPrimary.damageCoefficient * 100f + "% damage</style>. Costs <style=cIsHealth>" + FireTreebotPlantPrimary.healthCostFraction * 100f + "% HP</style> when fired repeatedly.";

	public override string SkillToken => "PETALSTORM";

	public override Sprite SkillIcon => SpikestripContent.GrooveBundle.LoadAsset<Sprite>("texPetalStormIcon.png");

	public override GameObject OwnerBodyPrefab => SpikestripUtil.LegacyLoad<GameObject>("Prefabs/CharacterBodies/TreebotBody");

	public override SkillFamily[] SkillFamilies => new SkillFamily[1] { LegacyResourcesAPI.Load<SkillFamily>("SkillDefs/TreebotBody/TreebotBodyPrimaryFamily") };

	public override Type[] EntityStateTypes => new Type[1] { typeof(FireTreebotPlantPrimary) };

	public override SkillDef GetSkillDef()
	{
		SkillDef skillDef = ScriptableObject.CreateInstance<SkillDef>();
		skillDef.activationState = new SerializableEntityStateType(typeof(FireTreebotPlantPrimary));
		skillDef.activationStateMachineName = "Weapon";
		skillDef.baseMaxStock = 1;
		skillDef.baseRechargeInterval = 0f;
		skillDef.cancelSprintingOnActivation = true;
		skillDef.interruptPriority = InterruptPriority.Any;
		skillDef.isCombatSkill = true;
		skillDef.keywordTokens = new string[1] { "KEYWORD_PERCENT_HP" };
		return skillDef;
	}

	public override void Init()
	{
		base.Init();
		impactVFX = PrefabAPI.InstantiateClone(LegacyResourcesAPI.Load<GameObject>("prefabs/effects/omnieffect/OmniExplosionVFXTreebotSeedPodMortar"), "treebotPlantImpactVFX", false);
		Transform transform = impactVFX.transform.Find("Petals");
		ParticleSystem component = transform.GetComponent<ParticleSystem>();
		ParticleSystem.MainModule main = component.main;
		main.scalingMode = ParticleSystemScalingMode.Local;
		transform.localScale = Vector3.one * 0.6f;
		ContentAddition.AddEffect(impactVFX);
		treebotPlantGhostPrefab = PrefabAPI.InstantiateClone(LegacyResourcesAPI.Load<GameObject>("prefabs/projectileghosts/SeedpodMortarGhost"), "treebotPlantGhostPrefab", false);
		treebotPlantGhostPrefab.transform.localScale = new Vector3(0.7f, 0.7f, 1f);
		treebotPlantGhostPrefab.transform.Find("Mesh").Rotate(Vector3.up, 180f, Space.Self);
		treebotPlantProjectilePrefab = PrefabAPI.InstantiateClone(LegacyResourcesAPI.Load<GameObject>("prefabs/projectiles/LunarSecondaryProjectile"), "treebotPlantProjectilePrefab", true);
		treebotPlantProjectilePrefab.transform.localScale = new Vector3(0.3f, 0.3f, 0.3f);
		treebotPlantProjectilePrefab.GetComponent<Rigidbody>().angularDrag = 0f;
		treebotPlantProjectilePrefab.GetComponent<Rigidbody>().drag = 0f;
		treebotPlantProjectilePrefab.GetComponent<ProjectileSimple>().desiredForwardSpeed = 80f;
		treebotPlantProjectilePrefab.GetComponent<ProjectileController>().startSound = "";
		treebotPlantProjectilePrefab.GetComponent<ProjectileController>().flightSoundLoop = null;
		SpikestripUtil.SetProjectileGhost(treebotPlantProjectilePrefab.GetComponent<ProjectileController>(), treebotPlantGhostPrefab);
		treebotPlantProjectilePrefab.GetComponent<ProjectileDamage>().damageType.damageSource = DamageSource.Primary;
		treebotPlantProjectilePrefab.AddComponent<ProjectileOverlapAttack>();
		ProjectileOverlapAttack component2 = treebotPlantProjectilePrefab.GetComponent<ProjectileOverlapAttack>();
		component2.damageCoefficient = 1f;
		component2.overlapProcCoefficient = 1f;
		component2.resetInterval = -1f;
		component2.impactEffect = impactVFX;
		component2.fireFrequency = 60f;
		component2.forceVector = Vector3.zero;
		treebotPlantProjectilePrefab.AddComponent<ProjectileDestroyOnWorld>();
		treebotPlantProjectilePrefab.GetComponent<ProjectileDestroyOnWorld>().impactEffect = impactVFX;
		if ((bool)treebotPlantProjectilePrefab.GetComponent<RigidbodySoundOnImpact>())
		{
			UnityEngine.Object.DestroyImmediate(treebotPlantProjectilePrefab.GetComponent<RigidbodySoundOnImpact>());
		}
		if ((bool)treebotPlantProjectilePrefab.GetComponent<ProjectileDotZone>())
		{
			UnityEngine.Object.DestroyImmediate(treebotPlantProjectilePrefab.GetComponent<ProjectileDotZone>());
		}
		if ((bool)treebotPlantProjectilePrefab.GetComponent<ProjectileFuse>())
		{
			UnityEngine.Object.DestroyImmediate(treebotPlantProjectilePrefab.GetComponent<ProjectileFuse>());
		}
		PrefabAPI.RegisterNetworkPrefab(treebotPlantProjectilePrefab);
		ContentAddition.AddProjectile(treebotPlantProjectilePrefab);
		CharacterBody component3 = LegacyResourcesAPI.Load<GameObject>("Prefabs/CharacterBodies/TreebotBody").GetComponent<CharacterBody>();
		component3.spreadBloomDecayTime = 6f;
		component3.spreadBloomCurve = AnimationCurve.Linear(0f, 0f, 1f, 5f);
		CrosshairController component4 = LegacyResourcesAPI.Load<GameObject>("prefabs/crosshair/TreebotCrosshair").GetComponent<CrosshairController>();
		component4.maxSpreadAngle = 1f;
	}
}
