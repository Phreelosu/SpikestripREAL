using EntityStates;
using EntityStates.Mage.Weapon;
using RoR2;
using RoR2.Projectile;
using RoR2.Skills;
using UnityEngine;
using UnityEngine.Networking;

namespace PlasmaCoreSpikestripContent.Content.Skills.States;

public class FireVoidBolts : BaseState, SteppedSkillDef.IStepSetter
{
	[SerializeField]
	public static float procCoefficient = 1f;

	[SerializeField]
	public static float damageCoefficient = 2.2f;

	[SerializeField]
	public static float force = 20f;

	public static float attackSpeedAltAnimationThreshold = 1f;

	[SerializeField]
	public static float baseDuration = 0.25f;

	[SerializeField]
	public static string attackSoundString = "Play_mage_m1_shoot";

	[SerializeField]
	public static float attackSoundPitch = 1f;

	public static float bloom = 0.2f;

	private float duration;

	private bool hasFiredGauntlet;

	private string muzzleString;

	private Transform muzzleTransform;

	private Animator animator;

	private ChildLocator childLocator;

	private FireFireBolt.Gauntlet gauntlet;

	public void SetStep(int i)
	{
		gauntlet = (FireFireBolt.Gauntlet)i;
	}

	public override void OnEnter()
	{
		base.OnEnter();
		duration = baseDuration / attackSpeedStat;
		Util.PlayAttackSpeedSound(attackSoundString, base.gameObject, attackSoundPitch);
		base.characterBody.SetAimTimer(2f);
		animator = GetModelAnimator();
		if ((bool)animator)
		{
			childLocator = animator.GetComponent<ChildLocator>();
		}
		switch (gauntlet)
		{
		case FireFireBolt.Gauntlet.Right:
			muzzleString = "MuzzleRight";
			if (attackSpeedStat < attackSpeedAltAnimationThreshold)
			{
				PlayCrossfade("Gesture, Additive", "Cast1Right", "FireGauntlet.playbackRate", duration, 0.1f);
				PlayAnimation("Gesture Left, Additive", "Empty");
				PlayAnimation("Gesture Right, Additive", "Empty");
			}
			else
			{
				((EntityState)this).PlayAnimation("Gesture Right, Additive", "FireGauntletRight", "FireGauntlet.playbackRate", duration);
				((EntityState)this).PlayAnimation("Gesture, Additive", "HoldGauntletsUp", "FireGauntlet.playbackRate", duration);
				FireGauntlet();
			}
			break;
		case FireFireBolt.Gauntlet.Left:
			muzzleString = "MuzzleLeft";
			if (attackSpeedStat < attackSpeedAltAnimationThreshold)
			{
				PlayCrossfade("Gesture, Additive", "Cast1Left", "FireGauntlet.playbackRate", duration, 0.1f);
				PlayAnimation("Gesture Left, Additive", "Empty");
				PlayAnimation("Gesture Right, Additive", "Empty");
			}
			else
			{
				((EntityState)this).PlayAnimation("Gesture Left, Additive", "FireGauntletLeft", "FireGauntlet.playbackRate", duration);
				((EntityState)this).PlayAnimation("Gesture, Additive", "HoldGauntletsUp", "FireGauntlet.playbackRate", duration);
				FireGauntlet();
			}
			break;
		}
	}

	public override void OnExit()
	{
		base.OnExit();
		if ((bool)base.skillLocator.secondary && base.skillLocator.secondary.stock <= 0)
		{
			base.skillLocator.secondary.UnsetSkillOverride(base.skillLocator.secondary, MageChokeArea.scriptableObject.MiscSkillDefs[0], GenericSkill.SkillOverridePriority.Contextual);
			base.skillLocator.secondary.RemoveAllStocks();
		}
	}

	private void FireGauntlet()
	{
		if (!hasFiredGauntlet)
		{
			base.characterBody.AddSpreadBloom(bloom);
			hasFiredGauntlet = true;
			Ray aimRay = GetAimRay();
			if ((bool)childLocator)
			{
				muzzleTransform = childLocator.FindChild(muzzleString);
			}
			if ((bool)MageChokeArea.muzzleFlashEffect)
			{
				EffectManager.SimpleMuzzleFlash(MageChokeArea.muzzleFlashEffect, base.gameObject, muzzleString, transmit: false);
			}
			if (base.isAuthority)
			{
				ProjectileManager.instance.FireProjectile(MageChokeArea.scriptableObject.Projectiles[1], aimRay.origin, Util.QuaternionSafeLookRotation(aimRay.direction), base.gameObject, damageCoefficient * damageStat, 0f, Util.CheckRoll(critStat, base.characterBody.master), DamageColorIndex.Default, (GameObject)null, -1f);
			}
		}
	}

	public override void FixedUpdate()
	{
		base.FixedUpdate();
		if (animator.GetFloat("FireGauntlet.fire") > 0f && !hasFiredGauntlet)
		{
			FireGauntlet();
		}
		if (base.fixedAge >= duration && base.isAuthority)
		{
			outer.SetNextStateToMain();
		}
	}

	public override InterruptPriority GetMinimumInterruptPriority()
	{
		return InterruptPriority.Skill;
	}

	public override void OnSerialize(NetworkWriter writer)
	{
		base.OnSerialize(writer);
		writer.Write((byte)gauntlet);
	}

	public override void OnDeserialize(NetworkReader reader)
	{
		base.OnDeserialize(reader);
		gauntlet = (FireFireBolt.Gauntlet)reader.ReadByte();
	}
}
