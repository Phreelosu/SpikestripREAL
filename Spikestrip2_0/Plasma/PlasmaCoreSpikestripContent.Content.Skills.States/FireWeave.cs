using System.Runtime.CompilerServices;
using EntityStates;
using EntityStates.Commando.CommandoWeapon;
using PlasmaCoreSpikestripContent.Misc;
using R2API.Networking;
using R2API.Networking.Interfaces;
using RoR2;
using RoR2.Skills;
using UnityEngine;

namespace PlasmaCoreSpikestripContent.Content.Skills.States;

public class FireWeave : BaseSkillState, SteppedSkillDef.IStepSetter
{
	public static float damageCoefficient = 0.5f;

	public static float damageCoefficientBounce = 0.5f;

	public static float force = 200f;

	public static float baseDuration = 0.15f;

	public static float recoilAmplitude = 1.25f;

	public static float spreadBloomValue = 0.25f;

	public static string firePistolSoundString = "Play_commando_R";

	private int pistol;

	private Ray aimRay;

	private float duration;

	void SteppedSkillDef.IStepSetter.SetStep(int i)
	{
		pistol = i;
	}

	private unsafe void FireBullet(string targetMuzzle)
	{
		Util.PlaySound(FirePistol2.firePistolSoundString, base.gameObject);
		if ((bool)FirePistol2.muzzleEffectPrefab)
		{
			EffectManager.SimpleMuzzleFlash(FirePistol2.muzzleEffectPrefab, base.gameObject, targetMuzzle, transmit: false);
		}
		AddRecoil(-0.4f * recoilAmplitude, -0.8f * recoilAmplitude, -0.3f * recoilAmplitude, 0.3f * recoilAmplitude);
		if (base.isAuthority)
		{
			BulletAttack bulletAttack = new BulletAttack();
			bulletAttack.owner = base.gameObject;
			bulletAttack.weapon = base.gameObject;
			bulletAttack.origin = aimRay.origin;
			bulletAttack.aimVector = aimRay.direction;
			bulletAttack.minSpread = 0f;
			bulletAttack.maxSpread = base.characterBody.spreadBloomAngle;
			bulletAttack.damage = damageCoefficient * damageStat;
			bulletAttack.force = force;
			bulletAttack.tracerEffectPrefab = Weave.scriptableObject.Effects[0];
			bulletAttack.muzzleName = targetMuzzle;
			bulletAttack.hitEffectPrefab = Weave.scriptableObject.Effects[2];
			bulletAttack.isCrit = Util.CheckRoll(critStat, base.characterBody.master);
			bulletAttack.radius = 0.1f;
			bulletAttack.smartCollision = true;
			bulletAttack.hitCallback = delegate(BulletAttack _bulletAttack, ref BulletAttack.BulletHit info)
			{
				//IL_0012: Expected O, but got Ref
				//IL_0022: Expected O, but got Ref
				bool result = BulletAttack.defaultHitCallback(_bulletAttack, ref info);
				HealthComponent healthComponent = (info.hitHurtBox ? info.hitHurtBox.healthComponent : null);
				if ((bool)healthComponent && healthComponent.alive && FriendlyFireManager.ShouldDirectHitProceed(healthComponent, GetTeam()))
				{
					NetMessageExtensions.Send((INetMessage)(object)new ApplyWeaveDebuffNetworkMessage
					{
						attacker = base.characterBody.gameObject,
						damage = damageCoefficientBounce * damageStat,
						target = healthComponent.body.gameObject
					}, (NetworkDestination)2);
				}
				return result;
			};
			bulletAttack.Fire();
		}
		base.characterBody.AddSpreadBloom(spreadBloomValue);
	}

	public override void OnEnter()
	{
		base.OnEnter();
		duration = baseDuration / attackSpeedStat;
		aimRay = GetAimRay();
		StartAimMode(aimRay, 3f);
		if (pistol % 2 == 0)
		{
			PlayAnimation("Gesture Additive, Left", "FirePistol, Left");
			FireBullet("MuzzleLeft");
		}
		else
		{
			PlayAnimation("Gesture Additive, Right", "FirePistol, Right");
			FireBullet("MuzzleRight");
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

	public override InterruptPriority GetMinimumInterruptPriority()
	{
		return InterruptPriority.Skill;
	}
}
