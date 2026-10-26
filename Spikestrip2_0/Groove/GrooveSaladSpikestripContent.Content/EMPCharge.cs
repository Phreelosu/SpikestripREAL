using System;
using EntityStates;
using EntityStates.Captain.Weapon;
using R2API;
using RoR2;
using RoR2.Projectile;
using RoR2.Skills;
using UnityEngine;
using UnityEngine.AddressableAssets;
using Spikestrip2_0.Core;

namespace GrooveSaladSpikestripContent.Content;

public class EMPCharge : SpikestripAltSkillsBase<EMPCharge>
{
	public class FireMegatazer : AimThrowableBase
	{
		private Animator modelAnimator;

		public static float damageCoef = 3f;

		public FireMegatazer()
		{
			base.maxDistance = 40f;
			base.rayRadius = 1f;
			base.projectilePrefab = megaTazerProjectilePrefab;
			base.endpointVisualizerPrefab = indicator;
			base.setFuse = true;
			damageCoefficient = damageCoef;
		}

		public override EntityState PickNextState()
		{
			return new AnimationCooldownMegatazer();
		}

		public override void ModifyProjectile(ref FireProjectileInfo fireProjectileInfo)
		{
			base.ModifyProjectile(ref fireProjectileInfo);
			fireProjectileInfo.force = 5000f;
		}

		public override void OnEnter()
		{
			base.OnEnter();
			if ((bool)base.characterBody)
			{
				base.characterBody.hideCrosshair = false;
			}
			modelAnimator = GetModelAnimator();
			if ((bool)modelAnimator)
			{
				modelAnimator.SetBool("PrepAirstrike", value: true);
			}
			base.PlayAnimation("Gesture, Override", "CallAirstrike2");
			base.PlayAnimation("Gesture, Additive", "CallAirstrike2");
		}

		public override void FixedUpdate()
		{
			base.FixedUpdate();
			base.characterBody.SetAimTimer(2f);
			if (base.isAuthority && base.characterBody.isSprinting)
			{
				EntityState entityState = ((AimThrowableBase)this).PickNextState();
				if (entityState != null)
				{
					outer.SetNextState(entityState);
				}
				else
				{
					outer.SetNextStateToMain();
				}
			}
		}

		public override void OnExit()
		{
			base.OnExit();
			base.PlayAnimation("Gesture, Override", "CallAirstrike1");
			base.PlayAnimation("Gesture, Additive", "CallAirstrike1");
		}
	}

	public class AnimationCooldownMegatazer : BaseState
	{
		private Animator modelAnimator;

		public static float baseDuration = 0.4f;

		private float duration;

		public override void OnEnter()
		{
			base.OnEnter();
			duration = baseDuration;
			modelAnimator = GetModelAnimator();
		}

		public override void FixedUpdate()
		{
			base.FixedUpdate();
			if (base.fixedAge >= duration && base.isAuthority)
			{
				EntityStateMachine entityStateMachine = EntityStateMachine.FindByCustomName(base.gameObject, "Skillswap");
				bool flag = (bool)entityStateMachine && entityStateMachine.state != null && (entityStateMachine.state.GetType() == typeof(SetupAirstrike) || entityStateMachine.state.GetType() == typeof(SetupAirstrike));
				if ((bool)modelAnimator && !flag)
				{
					modelAnimator.SetBool("PrepAirstrike", value: false);
				}
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

	public static GameObject megaTazerGhostPrefab;

	public static GameObject megaTazerProjectilePrefab;

	public static GameObject megaTazerNova;

	public static GameObject indicator;

	public override string SkillName => "EMP Charge";

	public override string SkillDescription => "<style=cIsDamage>Shocking</style>. Aim a grenade that deals <style=cIsDamage>" + FireMegatazer.damageCoef * 100f + "% damage</style> and applies <style=cIsUtility>heavy force</style> to ALL characters in the vicinity.";

	public override string SkillToken => "EMPCHARGE";

	public override Sprite SkillIcon => SpikestripContent.GrooveBundle.LoadAsset<Sprite>("texEMPChargeIcon.png");

	public override GameObject OwnerBodyPrefab => SpikestripUtil.LegacyLoad<GameObject>("Prefabs/CharacterBodies/CaptainBody");

	public override SkillFamily[] SkillFamilies => new SkillFamily[1] { LegacyResourcesAPI.Load<SkillFamily>("SkillDefs/CaptainBody/CaptainSecondarySkillFamily") };

	public override Type[] EntityStateTypes => new Type[2]
	{
		typeof(FireMegatazer),
		typeof(AnimationCooldownMegatazer)
	};

	public override SkillDef GetSkillDef()
	{
		SkillDef skillDef = ScriptableObject.CreateInstance<SkillDef>();
		skillDef.activationState = new SerializableEntityStateType(typeof(FireMegatazer));
		skillDef.activationStateMachineName = "Weapon";
		skillDef.baseMaxStock = 1;
		skillDef.baseRechargeInterval = 20f;
		skillDef.cancelSprintingOnActivation = true;
		skillDef.beginSkillCooldownOnSkillEnd = true;
		skillDef.mustKeyPress = true;
		skillDef.keywordTokens = new string[1] { "KEYWORD_SHOCKING" };
		skillDef.interruptPriority = InterruptPriority.Skill;
		skillDef.isCombatSkill = true;
		skillDef.canceledFromSprinting = false;
		return skillDef;
	}

	public override void Init()
	{
		base.Init();
		Material material = Addressables.LoadAssetAsync<Material>("82f8ddec4a6ec484d829ad21959b9a6b").WaitForCompletion();
		GameObject gameObject = Addressables.LoadAssetAsync<GameObject>("a96e011a250b79b4981af530b6e48433").WaitForCompletion();
		indicator = PrefabAPI.InstantiateClone(gameObject, "megaTazerIndicator", false);
		indicator.transform.Find("Expander/Sphere").GetComponent<MeshRenderer>().material = material;
		megaTazerNova = PrefabAPI.InstantiateClone(LegacyResourcesAPI.Load<GameObject>("prefabs/effects/CaptainTazerNova"), "MegaTazerNova", false);
		megaTazerNova.GetComponent<EffectComponent>().soundName = "Play_artifactBoss_takehit";
		megaTazerNova.GetComponent<VFXAttributes>().vfxPriority = VFXAttributes.VFXPriority.Always;
		ContentAddition.AddEffect(megaTazerNova);
		Material material2 = Addressables.LoadAssetAsync<Material>("759fab04020dd6740823b2d20e60ead0").WaitForCompletion();
		megaTazerGhostPrefab = PrefabAPI.InstantiateClone(LegacyResourcesAPI.Load<GameObject>("prefabs/projectileghosts/BanditGrenadeGhost"), "MegaTazerGhost", false);
		megaTazerGhostPrefab.transform.Find("Capsule").localScale = new Vector3(0.4640817f, 0.6f, 0.4640817f);
		TrailRenderer component = megaTazerGhostPrefab.transform.Find("Capsule/GameObject").GetComponent<TrailRenderer>();
		component.startColor = Color.white;
		component.material = material2;
		TrailRenderer component2 = megaTazerGhostPrefab.transform.Find("Capsule/GameObject (1)").GetComponent<TrailRenderer>();
		component2.startColor = Color.white;
		component2.material = material2;
		megaTazerProjectilePrefab = PrefabAPI.InstantiateClone(LegacyResourcesAPI.Load<GameObject>("prefabs/projectiles/CommandoGrenadeProjectile"), "MegaTazerProjectilePrefab", true);
		// Same combo as vanilla Captain's Power Tazer
		megaTazerProjectilePrefab.GetComponent<ProjectileDamage>().damageType = new DamageTypeCombo(DamageType.Shock5s, DamageTypeExtended.Electrical, DamageSource.Secondary);
		SpikestripUtil.SetProjectileGhost(megaTazerProjectilePrefab.GetComponent<ProjectileController>(), megaTazerGhostPrefab);
		ProjectileImpactExplosion component3 = megaTazerProjectilePrefab.GetComponent<ProjectileImpactExplosion>();
		component3.blastAttackerFiltering = AttackerFiltering.AlwaysHit;
		component3.blastDamageCoefficient = 1f;
		component3.blastProcCoefficient = 1f;
		component3.blastRadius = 11f;
		component3.bonusBlastForce = 1000f * Vector3.down;
		component3.explosionEffect = megaTazerNova;
		component3.falloffModel = BlastAttack.FalloffModel.None;
		component3.impactEffect = megaTazerNova;
		component3.lifetimeRandomOffset = 0f;
		if ((bool)megaTazerProjectilePrefab.GetComponent<ProjectileStickOnImpact>())
		{
			UnityEngine.Object.DestroyImmediate(megaTazerProjectilePrefab.GetComponent<ProjectileStickOnImpact>());
		}
		PrefabAPI.RegisterNetworkPrefab(megaTazerProjectilePrefab);
		ContentAddition.AddProjectile(megaTazerProjectilePrefab);
	}
}
