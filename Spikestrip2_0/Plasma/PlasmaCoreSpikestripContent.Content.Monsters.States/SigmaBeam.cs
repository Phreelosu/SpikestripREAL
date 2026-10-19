using EntityStates;
using RoR2;
using RoR2.Audio;
using UnityEngine;

namespace PlasmaCoreSpikestripContent.Content.Monsters.States;

public class SigmaBeam : BaseState
{
	public static GameObject muzzleFlashEffect;

	public static GameObject hitEffectPrefab;

	public static GameObject laserEffect;

	public GameObject laserEffectInstance1;

	public GameObject laserEffectInstance2;

	public GameObject laserEffectInstance3;

	public Transform muzzle1;

	public Transform muzzle2;

	public Transform muzzle3;

	public static float damageCoefficient = 0.8f;

	public static float maxRange = 40f;

	public static float procCoefficient = 0.75f;

	public static float force = 80f;

	public static float width = 0.5f;

	public static float beamEffectDistanceMult = 1.25f;

	public static string animationLayer = "Gesture";

	public static string animationState = "Speen";

	public static LoopSoundDef loopSoundDef;

	public LoopSoundManager.SoundLoopPtr loopPtr;

	public static float laserStart = 0.5f;

	public static float laserEnd = 4.5f;

	public bool hasStarted;

	public static float baseDuration = 5f;

	public static float duration = 5f;

	public float stopwatch;

	public static float baseFireDuration = 0.1f;

	public static float fireDuration = 0.025f;

	public float fireStopwatch;

	public override void OnEnter()
	{
		base.OnEnter();
		base.PlayAnimation(animationLayer, animationState);
		duration = baseDuration / attackSpeedStat;
		fireDuration = baseFireDuration / attackSpeedStat;
	}

	public void StartEffects()
	{
		if ((bool)loopSoundDef)
		{
			loopPtr = LoopSoundManager.PlaySoundLoopLocal(base.gameObject, loopSoundDef);
		}
		Transform modelTransform = GetModelTransform();
		if (!modelTransform)
		{
			return;
		}
		ChildLocator component = modelTransform.GetComponent<ChildLocator>();
		if (!component || !laserEffect)
		{
			return;
		}
		muzzle1 = component.FindChild("Muzzle1");
		if ((bool)muzzle1)
		{
			if ((bool)muzzleFlashEffect)
			{
				EffectManager.SimpleMuzzleFlash(muzzleFlashEffect, base.gameObject, "Muzzle1", transmit: false);
			}
			laserEffectInstance1 = Object.Instantiate(laserEffect, Vector3.zero, Quaternion.identity);
			laserEffectInstance1.transform.position = muzzle1.transform.position;
			laserEffectInstance1.transform.rotation = muzzle1.transform.rotation;
		}
		muzzle2 = component.FindChild("Muzzle2");
		if ((bool)muzzle2)
		{
			if ((bool)muzzleFlashEffect)
			{
				EffectManager.SimpleMuzzleFlash(muzzleFlashEffect, base.gameObject, "Muzzle2", transmit: false);
			}
			laserEffectInstance2 = Object.Instantiate(laserEffect, Vector3.zero, Quaternion.identity);
			laserEffectInstance2.transform.position = muzzle2.transform.position;
			laserEffectInstance2.transform.rotation = muzzle2.transform.rotation;
		}
		muzzle3 = component.FindChild("Muzzle3");
		if ((bool)muzzle3)
		{
			if ((bool)muzzleFlashEffect)
			{
				EffectManager.SimpleMuzzleFlash(muzzleFlashEffect, base.gameObject, "Muzzle3", transmit: false);
			}
			laserEffectInstance3 = Object.Instantiate(laserEffect, Vector3.zero, Quaternion.identity);
			laserEffectInstance3.transform.position = muzzle3.transform.position;
			laserEffectInstance3.transform.rotation = muzzle3.transform.rotation;
		}
	}

	public override void FixedUpdate()
	{
		base.FixedUpdate();
		bool flag = false;
		stopwatch += Time.fixedDeltaTime;
		fireStopwatch += Time.fixedDeltaTime;
		if (stopwatch > duration)
		{
			outer.SetNextStateToMain();
		}
		else if (!hasStarted && stopwatch > laserStart)
		{
			hasStarted = true;
			StartEffects();
		}
		else if (stopwatch > laserEnd)
		{
			if ((bool)laserEffectInstance1)
			{
				EntityState.Destroy(laserEffectInstance1);
			}
			if ((bool)laserEffectInstance2)
			{
				EntityState.Destroy(laserEffectInstance2);
			}
			if ((bool)laserEffectInstance3)
			{
				EntityState.Destroy(laserEffectInstance3);
			}
			LoopSoundManager.StopSoundLoopLocal(loopPtr);
		}
		if (base.isAuthority && fireStopwatch > fireDuration)
		{
			fireStopwatch -= fireDuration;
			flag = true;
		}
		if (!hasStarted || !(stopwatch <= laserEnd))
		{
			return;
		}
		if ((bool)muzzle1)
		{
			Ray ray = new Ray(muzzle1.position, muzzle1.up);
			if (flag)
			{
				FireBullet(muzzle1, ray, "Muzzle1");
			}
			if ((bool)laserEffectInstance1)
			{
				laserEffectInstance1.transform.position = muzzle1.transform.position;
				laserEffectInstance1.transform.rotation = muzzle1.transform.rotation;
				laserEffectInstance1.transform.GetComponent<LineRenderer>().SetPosition(1, GetBeamLength(ray));
			}
		}
		if ((bool)muzzle2)
		{
			Ray ray2 = new Ray(muzzle2.position, muzzle2.up);
			if (flag)
			{
				FireBullet(muzzle2, ray2, "Muzzle2");
			}
			if ((bool)laserEffectInstance2)
			{
				laserEffectInstance2.transform.position = muzzle2.transform.position;
				laserEffectInstance2.transform.rotation = muzzle2.transform.rotation;
				laserEffectInstance2.transform.GetComponent<LineRenderer>().SetPosition(1, GetBeamLength(ray2));
			}
		}
		if ((bool)muzzle3)
		{
			Ray ray3 = new Ray(muzzle3.position, muzzle3.up);
			if (flag)
			{
				FireBullet(muzzle3, ray3, "Muzzle3");
			}
			if ((bool)laserEffectInstance3)
			{
				laserEffectInstance3.transform.position = muzzle3.transform.position;
				laserEffectInstance3.transform.rotation = muzzle3.transform.rotation;
				laserEffectInstance3.transform.GetComponent<LineRenderer>().SetPosition(1, GetBeamLength(ray3));
			}
		}
	}

	public void FireBullet(Transform modelTransform, Ray laserRay, string targetMuzzle)
	{
		if (base.isAuthority)
		{
			BulletAttack bulletAttack = new BulletAttack();
			bulletAttack.owner = base.gameObject;
			bulletAttack.weapon = base.gameObject;
			bulletAttack.origin = laserRay.origin;
			bulletAttack.aimVector = laserRay.direction;
			bulletAttack.minSpread = 0f;
			bulletAttack.maxSpread = 0f;
			bulletAttack.bulletCount = 1u;
			bulletAttack.damage = damageCoefficient * damageStat;
			bulletAttack.procCoefficient = procCoefficient;
			bulletAttack.force = force;
			bulletAttack.muzzleName = targetMuzzle;
			bulletAttack.hitEffectPrefab = hitEffectPrefab;
			bulletAttack.isCrit = Util.CheckRoll(critStat, base.characterBody.master);
			bulletAttack.HitEffectNormal = false;
			bulletAttack.radius = width;
			bulletAttack.maxDistance = maxRange;
			ModifyBullet(bulletAttack);
			bulletAttack.Fire();
		}
	}

	public void ModifyBullet(BulletAttack bulletAttack)
	{
	}

	public Vector3 GetBeamLength(Ray ray)
	{
		Vector3 result = new Vector3(0f, maxRange, 0f);
		if (Physics.SphereCast(ray.origin, width, ray.direction, out var hitInfo, maxRange, (int)LayerIndex.world.mask | (int)LayerIndex.entityPrecise.mask))
		{
			result.y = hitInfo.distance * beamEffectDistanceMult;
		}
		return result;
	}

	public Vector3 GetBeamEndPoint(Ray ray)
	{
		Vector3 point = ray.GetPoint(maxRange);
		if (Physics.SphereCast(ray.origin, width, ray.direction, out var hitInfo, maxRange, (int)LayerIndex.world.mask | (int)LayerIndex.entityPrecise.mask))
		{
			point = hitInfo.point;
		}
		return point;
	}

	public override void OnExit()
	{
		LoopSoundManager.StopSoundLoopLocal(loopPtr);
		base.PlayAnimation(animationLayer, "None");
		base.OnExit();
		if ((bool)laserEffectInstance1)
		{
			EntityState.Destroy(laserEffectInstance1);
		}
		if ((bool)laserEffectInstance2)
		{
			EntityState.Destroy(laserEffectInstance2);
		}
		if ((bool)laserEffectInstance3)
		{
			EntityState.Destroy(laserEffectInstance3);
		}
	}
}
