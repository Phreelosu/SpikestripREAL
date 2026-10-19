using EntityStates;
using EntityStates.Destructible;
using R2API;
using RoR2;
using RoR2.Skills;
using UnityEngine;
using UnityEngine.Networking;
using Spikestrip2_0.Core;

namespace GrooveSaladSpikestripContent.Content;

public class PotMobileSkills
{
    //all of this is literally based on moff's potmobile enemy
    public class PotMobileDeath : GenericCharacterDeath
	{
		public static float chargeDuration;

		public static float explosionRadius = 12f;

		public static float explosionDamageCoefficient = 5f;

		public static float explosionProcCoefficient = 2f;

		public static float explosionForce = 200f;

		public override bool shouldAutoDestroy => false;

		public override void OnEnter()
		{
			base.OnEnter();
			if (!base.cachedModelTransform)
			{
				if (NetworkServer.active)
				{
					DestroyBodyAsapServer();
				}
			}
			else if ((bool)ExplosivePotDeath.chargePrefab)
			{
				Object.Instantiate(ExplosivePotDeath.chargePrefab, base.transform);
			}
		}

		public override void FixedUpdate()
		{
			base.FixedUpdate();
			if (base.fixedAge >= ExplosivePotDeath.chargeDuration)
			{
				if (NetworkServer.active)
				{
					ExplodeServer();
					DestroyBodyAsapServer();
				}
				DestroyModel();
			}
		}

		private void ExplodeServer()
		{
			if ((bool)base.cachedModelTransform)
			{
				if ((bool)ExplosivePotDeath.explosionEffectPrefab)
				{
					EffectManager.SpawnEffect(ExplosivePotDeath.explosionEffectPrefab, new EffectData
					{
						origin = base.transform.position,
						scale = explosionRadius,
						rotation = Quaternion.identity
					}, transmit: true);
				}
				BlastAttack blastAttack = new BlastAttack();
				blastAttack.attacker = base.gameObject;
				blastAttack.baseDamage = damageStat * explosionDamageCoefficient;
				blastAttack.radius = explosionRadius;
				blastAttack.falloffModel = BlastAttack.FalloffModel.Linear;
				blastAttack.procCoefficient = explosionProcCoefficient;
				blastAttack.teamIndex = base.teamComponent.teamIndex;
				blastAttack.damageType = DamageType.ClayGoo;
				blastAttack.position = base.transform.position;
				blastAttack.baseForce = explosionForce;
				blastAttack.attackerFiltering = AttackerFiltering.NeverHitSelf;
				blastAttack.Fire();
			}
		}

		public override InterruptPriority GetMinimumInterruptPriority()
		{
			return InterruptPriority.Immobilize;
		}
	}

	public class PotMobileSuicide : BaseState
	{
		public override void OnEnter()
		{
			base.OnEnter();
			if ((bool)base.healthComponent && NetworkServer.active)
			{
				base.healthComponent.Suicide((GameObject)null, (GameObject)null, DamageType.Generic);
			}
		}

		public override InterruptPriority GetMinimumInterruptPriority()
		{
			return InterruptPriority.PrioritySkill;
		}
	}

	public class PotMobileJump : BaseState
	{
		public static GameObject explosionEffectPrefab = jumpEffect;

		public static float duration = 0.5f;

		public static float explosionRadius = 4f;

		public static float explosionDamageCoefficient = 3f;

		public static float explosionProcCoefficient = 1f;

		public static float explosionForce = 200f;

		public override void OnEnter()
		{
			base.OnEnter();
			if (base.isAuthority)
			{
				Explode();
			}
		}

		public override void FixedUpdate()
		{
			base.FixedUpdate();
			if (base.isAuthority && base.fixedAge >= duration)
			{
				outer.SetNextStateToMain();
			}
		}

		private void Explode()
		{
			if ((bool)explosionEffectPrefab)
			{
				EffectManager.SpawnEffect(explosionEffectPrefab, new EffectData
				{
					origin = base.transform.position,
					scale = explosionRadius * 1.5f,
					rotation = Quaternion.identity
				}, transmit: true);
			}
			BlastAttack blastAttack = new BlastAttack();
			blastAttack.attacker = base.gameObject;
			blastAttack.baseDamage = damageStat * explosionDamageCoefficient;
			blastAttack.radius = explosionRadius;
			blastAttack.falloffModel = BlastAttack.FalloffModel.Linear;
			blastAttack.procCoefficient = explosionProcCoefficient;
			blastAttack.teamIndex = base.teamComponent.teamIndex;
			blastAttack.damageType = DamageType.ClayGoo;
			blastAttack.position = base.transform.position;
			blastAttack.baseForce = explosionForce;
			blastAttack.attackerFiltering = AttackerFiltering.NeverHitSelf;
			blastAttack.Fire();
			if ((bool)(Object)(object)base.characterMotor)
			{
				base.characterMotor.ApplyForce(Vector3.up * base.characterMotor.mass * 20f);
			}
		}

		public override InterruptPriority GetMinimumInterruptPriority()
		{
			return InterruptPriority.Skill;
		}
	}

	public class PotMobileSpawn : BaseState
	{
		public static float duration = 2f;

		public static AnimationCurve curve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

		private Vector3 startPositionVector;

		private Vector3 endPositionVector;

		private Vector3 startRotationVector;

		private Vector3 endRotationVector;

		private Transform mdlTransform;

		public override void OnEnter()
		{
			base.OnEnter();
			if ((bool)base.characterBody)
			{
				EffectManager.SpawnEffect(LegacyResourcesAPI.Load<GameObject>("prefabs/effects/ClayBruiserSpawnEffect"), new EffectData
				{
					origin = base.characterBody.footPosition,
					scale = 1f,
					rotation = Quaternion.identity
				}, transmit: false);
			}
			mdlTransform = GetModelBaseTransform();
			if ((bool)mdlTransform)
			{
				endPositionVector = mdlTransform.localPosition;
				startPositionVector = endPositionVector + new Vector3(0f, -5f, 0f);
			}
		}

		public override void Update()
		{
			base.Update();
			if ((bool)mdlTransform)
			{
				float t = curve.Evaluate(base.fixedAge / duration);
				mdlTransform.localPosition = Vector3.Lerp(startPositionVector, endPositionVector, t);
			}
		}

		public override void FixedUpdate()
		{
			base.FixedUpdate();
			if (base.isAuthority && base.fixedAge >= duration)
			{
				outer.SetNextStateToMain();
			}
		}

		public override void OnExit()
		{
			base.OnExit();
			if ((bool)mdlTransform)
			{
				mdlTransform.localPosition = endPositionVector;
			}
		}

		public override InterruptPriority GetMinimumInterruptPriority()
		{
			return InterruptPriority.Immobilize;
		}
	}

	public static SkillFamily suicideSkillFamily;

	public static SkillDef suicideSkillDef;

	public static SkillFamily jumpSkillFamily;

	public static SkillDef jumpSkillDef;

	public static GameObject jumpEffect;

	public static void RegisterSkills(GameObject bodyPrefab)
	{
		ContentAddition.AddEntityState(typeof(PotMobileDeath), out _);
		ContentAddition.AddEntityState(typeof(PotMobileSuicide), out _);
		ContentAddition.AddEntityState(typeof(PotMobileJump), out _);
		ContentAddition.AddEntityState(typeof(PotMobileSpawn), out _);
		CharacterDeathBehavior component = bodyPrefab.GetComponent<CharacterDeathBehavior>();
		component.deathState = new SerializableEntityStateType(typeof(PotMobileDeath));
		EntityStateMachine entityStateMachine = EntityStateMachine.FindByCustomName(bodyPrefab, "Body");
		entityStateMachine.initialStateType = new SerializableEntityStateType(typeof(PotMobileSpawn));
		suicideSkillDef = ScriptableObject.CreateInstance<SkillDef>();
		suicideSkillDef.activationState = new SerializableEntityStateType(typeof(PotMobileSuicide));
		suicideSkillDef.activationStateMachineName = "Weapon";
		suicideSkillDef.isCombatSkill = true;
		SkillFamily.Variant variant = new SkillFamily.Variant
		{
			skillDef = suicideSkillDef
		};
		suicideSkillFamily = ScriptableObject.CreateInstance<SkillFamily>();
		suicideSkillFamily.variants = new SkillFamily.Variant[1] { variant };
		GenericSkill genericSkill = bodyPrefab.GetComponents<GenericSkill>()[0];
		genericSkill._skillFamily = suicideSkillFamily;
		genericSkill.skillName = "Suicide";
		jumpSkillDef = ScriptableObject.CreateInstance<SkillDef>();
		jumpSkillDef.activationState = new SerializableEntityStateType(typeof(PotMobileJump));
		jumpSkillDef.activationStateMachineName = "Weapon";
		jumpSkillDef.isCombatSkill = true;
		jumpSkillDef.baseRechargeInterval = 4f;
		SkillFamily.Variant variant2 = new SkillFamily.Variant
		{
			skillDef = jumpSkillDef
		};
		jumpSkillFamily = ScriptableObject.CreateInstance<SkillFamily>();
		jumpSkillFamily.variants = new SkillFamily.Variant[1] { variant2 };
		GenericSkill genericSkill2 = bodyPrefab.GetComponents<GenericSkill>()[1];
		genericSkill2._skillFamily = jumpSkillFamily;
		genericSkill2.skillName = "Jump";
		ContentAddition.AddSkillFamily(suicideSkillFamily);
		ContentAddition.AddSkillDef(suicideSkillDef);
		ContentAddition.AddSkillFamily(jumpSkillFamily);
		ContentAddition.AddSkillDef(jumpSkillDef);
		jumpEffect = PrefabAPI.InstantiateClone(LegacyResourcesAPI.Load<GameObject>("prefabs/effects/omnieffect/OmniExplosionVFXCommandoGrenade"), "PotMobileJumpExplosion", false);
		Object.DestroyImmediate(jumpEffect.transform.Find("Nova Sphere").gameObject);
		Object.DestroyImmediate(jumpEffect.transform.Find("Nova Sphere (1)").gameObject);
		Light componentInChildren = jumpEffect.GetComponentInChildren<Light>();
		componentInChildren.color = SpikestripUtil.ColorRGB(207f, 135f, 0f);
		ContentAddition.AddEffect(jumpEffect);
	}
}
