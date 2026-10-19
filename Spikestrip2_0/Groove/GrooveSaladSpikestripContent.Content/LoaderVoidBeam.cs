using System;
using System.Collections.Generic;
using EntityStates;
using R2API;
using RoR2;
using RoR2.Audio;
using RoR2.Skills;
using ThreeEyedGames;
using UnityEngine;
using UnityEngine.AddressableAssets;
using Spikestrip2_0.Core;

namespace GrooveSaladSpikestripContent.Content;

public class LoaderVoidBeam : SpikestripVoidAltSkillsBase<LoaderVoidBeam>
{
	public class VoidBeam : BaseSkillState
	{
		public static LoopSoundDef lsdConstructBeam = Addressables.LoadAssetAsync<LoopSoundDef>("a4aab1b77369a9b418fd2e6e6a3a856a").WaitForCompletion();

		public static float baseEntryDuration = 0.3f;

		public static float damageCoefficient = 0.75f;

		public static float finalBlastDamageCoefficient = 8f;

		public static float procCoefficient = 0.75f;

		public static float baseFireInterval = 0.15f;

		public static float maxDistance = 50f;

		public static float bulletRadius = 0.5f;

		public static LayerMask stopperMask = LayerIndex.world.mask;

		private Transform muzzleTransform;

		private Vector3 initialBeamScale;

		private bool hasEntered = false;

		private float stopwatch;

		private List<LoopSoundManager.SoundLoopPtr> loopPtrs = new List<LoopSoundManager.SoundLoopPtr>();

		private GameObject beamVFXInstance;

		private GameObject chargeVFXInstance;

		public float entryDuration => baseEntryDuration / attackSpeedStat;

		public float fireInterval => baseFireInterval / attackSpeedStat;

		public Ray GetBeamRay(out float distance)
		{
			Ray aimRay = GetAimRay();
			Vector3 point = aimRay.GetPoint(maxDistance);
			distance = maxDistance;
			if (Physics.Raycast(aimRay.origin, aimRay.direction, out var hitInfo, maxDistance, stopperMask))
			{
				point = hitInfo.point;
				distance = hitInfo.distance;
			}
			Vector3 vector = (muzzleTransform ? muzzleTransform.position : base.transform.position);
			return new Ray(vector, point - vector);
		}

		public override void OnEnter()
		{
			base.OnEnter();
			muzzleTransform = FindModelChild("MuzzleLeft");
			PlayAnimation("Grapple", "FireHookIntro");
			if ((bool)lsdConstructBeam)
			{
				loopPtrs.Add(LoopSoundManager.PlaySoundLoopLocal(base.gameObject, lsdConstructBeam));
				loopPtrs.Add(LoopSoundManager.PlaySoundLoopLocal(base.gameObject, lsdConstructBeam));
			}
			if ((bool)muzzleTransform)
			{
				beamVFXInstance = UnityEngine.Object.Instantiate(beamVFXPrefab, muzzleTransform.position, muzzleTransform.rotation);
				beamVFXInstance.GetComponentInChildren<ObjectScaleCurve>().timeMax = entryDuration;
				initialBeamScale = beamVFXInstance.transform.localScale;
				chargeVFXInstance = UnityEngine.Object.Instantiate(chargeVFXPrefab, muzzleTransform);
				chargeVFXInstance.GetComponent<ObjectScaleCurve>().timeMax = entryDuration;
			}
		}

		public override void FixedUpdate()
		{
			base.FixedUpdate();
			base.characterBody.SetAimTimer(2f);
			attackSpeedStat = base.characterBody.attackSpeed;
			if (!hasEntered)
			{
				stopwatch += Time.fixedDeltaTime;
				if (stopwatch >= entryDuration)
				{
					stopwatch = 0f;
					PlayAnimation("Grapple", "FireHookLoop");
					hasEntered = true;
				}
				return;
			}
			stopwatch -= Time.fixedDeltaTime;
			if (stopwatch <= 0f)
			{
				stopwatch += fireInterval;
				FireBullet();
			}
			if (base.isAuthority && !IsKeyDownAuthority())
			{
				outer.SetNextStateToMain();
			}
		}

		public override void Update()
		{
			base.Update();
			if ((bool)beamVFXInstance)
			{
				float distance;
				Ray beamRay = GetBeamRay(out distance);
				float num = distance / maxDistance;
				beamVFXInstance.transform.SetPositionAndRotation(beamRay.origin, Util.QuaternionSafeLookRotation(beamRay.direction));
				beamVFXInstance.transform.localScale = new Vector3(initialBeamScale.x, initialBeamScale.y, initialBeamScale.z * num);
			}
		}

		public void FireBullet()
		{
			base.characterBody.isSprinting = false;
			if (base.isAuthority)
			{
				float distance;
				Ray beamRay = GetBeamRay(out distance);
				BulletAttack bulletAttack = new BulletAttack();
				bulletAttack.owner = base.gameObject;
				bulletAttack.weapon = base.gameObject;
				bulletAttack.origin = beamRay.origin;
				bulletAttack.aimVector = beamRay.direction;
				bulletAttack.minSpread = 0f;
				bulletAttack.damage = damageCoefficient * damageStat;
				bulletAttack.force = 200f;
				bulletAttack.muzzleName = "MuzzleLeft";
				bulletAttack.hitEffectPrefab = impactVFXPrefab;
				bulletAttack.isCrit = RollCrit();
				bulletAttack.radius = bulletRadius;
				bulletAttack.falloffModel = BulletAttack.FalloffModel.None;
				bulletAttack.stopperMask = stopperMask;
				bulletAttack.procCoefficient = procCoefficient;
				bulletAttack.maxDistance = distance;
				bulletAttack.smartCollision = true;
				bulletAttack.damageColorIndex = DamageColorIndex.Void;
				bulletAttack.Fire();
			}
		}

		public override void OnExit()
		{
			base.OnExit();
			FinalBlast();
			for (int i = 0; i < loopPtrs.Count; i++)
			{
				LoopSoundManager.StopSoundLoopLocal(loopPtrs[i]);
			}
			PlayAnimation("Grapple", "FireHookExit");
			if ((bool)beamVFXInstance)
			{
				VfxKillBehavior.KillVfxObject(beamVFXInstance);
				beamVFXInstance = null;
			}
			if ((bool)chargeVFXInstance)
			{
				VfxKillBehavior.KillVfxObject(chargeVFXInstance);
				chargeVFXInstance = null;
			}
		}

		public void FinalBlast()
		{
			Util.PlaySound("Play_voidRaid_m1_shoot", base.gameObject);
			if (base.isAuthority)
			{
				Ray aimRay = GetAimRay();
				BulletAttack bulletAttack = new BulletAttack();
				bulletAttack.owner = base.gameObject;
				bulletAttack.weapon = base.gameObject;
				bulletAttack.origin = aimRay.origin;
				bulletAttack.aimVector = aimRay.direction;
				bulletAttack.minSpread = 0f;
				bulletAttack.damage = finalBlastDamageCoefficient * damageStat;
				bulletAttack.force = 1000f;
				bulletAttack.muzzleName = "MuzzleLeft";
				bulletAttack.hitEffectPrefab = null;
				bulletAttack.isCrit = RollCrit();
				bulletAttack.radius = 2f;
				bulletAttack.falloffModel = BulletAttack.FalloffModel.None;
				bulletAttack.stopperMask = stopperMask;
				bulletAttack.procCoefficient = 1f;
				bulletAttack.maxDistance = 100f;
				bulletAttack.smartCollision = true;
				bulletAttack.tracerEffectPrefab = finalBlastTracerPrefab;
				bulletAttack.damageColorIndex = DamageColorIndex.Void;
				bulletAttack.Fire();
				if ((bool)(UnityEngine.Object)(object)base.characterMotor)
				{
					base.characterMotor.ApplyForce(-20f * base.characterMotor.mass * aimRay.direction, alwaysApply: false, disableAirControlUntilCollision: true);
				}
			}
		}

		public override InterruptPriority GetMinimumInterruptPriority()
		{
			return InterruptPriority.Pain;
		}
	}

	public static GameObject beamVFXPrefab;

	public static GameObject chargeVFXPrefab;

	public static GameObject impactVFXPrefab;

	public static GameObject finalBlastTracerPrefab;

	public override string SkillName => "「Conduit」";

	public override string SkillDescription => $"Charge a <style=cIsVoid>N?ULL beam</style> for <style=cIsVoid>{SpikestripUtil.Percent(VoidBeam.damageCoefficient / VoidBeam.baseFireInterval)}% damage per second</style>. Finish with a destructive blast for <style=cIsVoid>{SpikestripUtil.Percent(VoidBeam.finalBlastDamageCoefficient)}% damage</style>.";

	public override string SkillToken => "VOIDBEAM";

	public override string PrerequisiteIdentifier => "DefeatSuperRoboBallBoss";

	public override string CharacterName => "Loader";

	public override string AchievementName => "Seas of Infinity";

	public override Sprite SkillIcon => SpikestripContent.GrooveBundle.LoadAsset<Sprite>("texVoidBeamIcon.png");

	public override GameObject OwnerBodyPrefab => SpikestripUtil.LegacyLoad<GameObject>("Prefabs/CharacterBodies/LoaderBody");

	public override SkillFamily[] SkillFamilies => new SkillFamily[1] { LegacyResourcesAPI.Load<SkillFamily>("SkillDefs/LoaderBody/LoaderBodySecondaryFamily") };

	public override Type[] EntityStateTypes => new Type[1] { typeof(VoidBeam) };

	public override SkillDef GetSkillDef()
	{
		SkillDef skillDef = ScriptableObject.CreateInstance<SkillDef>();
		skillDef.activationState = new SerializableEntityStateType(typeof(VoidBeam));
		skillDef.activationStateMachineName = "Hook";
		skillDef.baseMaxStock = 1;
		skillDef.baseRechargeInterval = 5f;
		skillDef.cancelSprintingOnActivation = true;
		skillDef.interruptPriority = InterruptPriority.Any;
		skillDef.isCombatSkill = true;
		skillDef.beginSkillCooldownOnSkillEnd = true;
		return skillDef;
	}

	public override void Init()
	{
		base.Init();
		GameObject gameObject = Addressables.LoadAssetAsync<GameObject>("7c5959f64d841164c97c629f1c331fbb").WaitForCompletion();
		beamVFXPrefab = PrefabAPI.InstantiateClone(gameObject, "VoidBeamVFX", false);
		beamVFXPrefab.transform.localScale = new Vector3(0.3f, 0.3f, 3f);
		Light component = beamVFXPrefab.transform.Find("Point Light, Middle").GetComponent<Light>();
		component.intensity = 5f;
		component.range = 20f;
		Light component2 = beamVFXPrefab.transform.Find("Point Light, End").GetComponent<Light>();
		component2.intensity = 50f;
		component2.range = 10f;
		component2.color = Color.magenta;
		Transform transform = beamVFXPrefab.transform.Find("Billboards");
		UnityEngine.Object.DestroyImmediate(transform.gameObject);
		beamVFXPrefab.transform.Find("SwirlyTrails").localScale = new Vector3(10f, 10f, 5f);
		beamVFXPrefab.transform.Find("MuzzleRayParticles").localScale = new Vector3(3f, 3f, 1f);
		ParticleSystem component3 = beamVFXPrefab.transform.Find("Glows").GetComponent<ParticleSystem>();
		ParticleSystem.MainModule main = component3.main;
		main.scalingMode = ParticleSystemScalingMode.Local;
		component3.transform.localScale = Vector3.one * 5f;
		UnityEngine.Object.DestroyImmediate(beamVFXPrefab.GetComponentInChildren<ShakeEmitter>());
		Material material = Addressables.LoadAssetAsync<Material>("d455aa17a4e250540833bf13401ff514").WaitForCompletion();
		Material material2 = Addressables.LoadAssetAsync<Material>("068112eaf347f5b4d907fe47997d589a").WaitForCompletion();
		GameObject gameObject2 = Addressables.LoadAssetAsync<GameObject>("efe39d83fa295ad47be75e5df3ea1699").WaitForCompletion();
		chargeVFXPrefab = PrefabAPI.InstantiateClone(gameObject2, "VoidBeamChargeVFX", false);
		chargeVFXPrefab.transform.Find("Base").GetComponent<ParticleSystemRenderer>().material = material2;
		chargeVFXPrefab.transform.Find("OrbCore").GetComponent<MeshRenderer>().material = material;
		UnityEngine.Object.DestroyImmediate(chargeVFXPrefab.transform.Find("Sparks, Misc").gameObject);
		GameObject gameObject3 = Addressables.LoadAssetAsync<GameObject>("80e6c8de4aa8da245afe7802e0a4ac81").WaitForCompletion();
		impactVFXPrefab = PrefabAPI.InstantiateClone(gameObject3, "VoidBeamImpactVFX", false);
		impactVFXPrefab.transform.localScale = Vector3.one * 3f;
		UnityEngine.Object.DestroyImmediate(impactVFXPrefab.transform.Find("Nova Sphere").gameObject);
		EffectComponent component4 = impactVFXPrefab.GetComponent<EffectComponent>();
		component4.soundName = "Play_artifactBoss_attack1_explode";
		ContentAddition.AddEffect(impactVFXPrefab);
		Material material3 = Addressables.LoadAssetAsync<Material>("fac3e46d796cd3b44af6e7cc86d2e480").WaitForCompletion();
		GameObject gameObject4 = Addressables.LoadAssetAsync<GameObject>("c9a126836debd42469629414f9d848f9").WaitForCompletion();
		finalBlastTracerPrefab = PrefabAPI.InstantiateClone(gameObject4, "VoidBeamFinalBlastTracer", false);
		finalBlastTracerPrefab.GetComponentInChildren<Decal>().Material = material3;
		VFXAttributes vfxAttributes = finalBlastTracerPrefab.AddComponent<VFXAttributes>();
		vfxAttributes.vfxPriority = VFXAttributes.VFXPriority.Always;
		vfxAttributes.vfxIntensity = VFXAttributes.VFXIntensity.Low;
		ContentAddition.AddEffect(finalBlastTracerPrefab);
	}
}
