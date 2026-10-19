using EntityStates;
using RoR2;
using UnityEngine;

namespace PlasmaCoreSpikestripContent.Content.Skills.States;

public class Choke : BaseSkillState
{
	public static float baseDuration = 0.25f;

	public float duration = 0.25f;

	public static GameObject muzzleflash;

	public override void OnEnter()
	{
		base.OnEnter();
		duration = baseDuration / base.characterBody.attackSpeed;
		((EntityState)this).PlayAnimation("Gesture, Additive", "PrepWall", "PrepWall.playbackRate", duration);
		Util.PlayAttackSpeedSound("Play_voidDevastator_m1_chargeUp", base.gameObject, 1.5f * base.characterBody.attackSpeed);
		EffectManager.SimpleMuzzleFlash(MageChokeArea.muzzleFlashEffect, base.gameObject, "MuzzleLeft", transmit: false);
		EffectManager.SimpleMuzzleFlash(MageChokeArea.muzzleFlashEffect, base.gameObject, "MuzzleRight", transmit: false);
	}

	public override void FixedUpdate()
	{
		base.FixedUpdate();
		if (base.isAuthority && base.age > duration && !IsKeyDownAuthority())
		{
			outer.SetNextState(new ChokeFire());
		}
	}

	public override void OnExit()
	{
		base.OnExit();
	}

	public override InterruptPriority GetMinimumInterruptPriority()
	{
		return InterruptPriority.PrioritySkill;
	}
}
