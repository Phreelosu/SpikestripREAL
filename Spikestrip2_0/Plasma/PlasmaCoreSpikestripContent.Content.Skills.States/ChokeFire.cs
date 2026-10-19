using EntityStates;
using RoR2;
using RoR2.Projectile;

namespace PlasmaCoreSpikestripContent.Content.Skills.States;

public class ChokeFire : BaseSkillState
{
	public static float baseDuration = 0.1f;

	public float duration = 0.25f;

	public override void OnEnter()
	{
		base.OnEnter();
		duration = baseDuration / base.characterBody.attackSpeed;
		Util.PlayAttackSpeedSound("Play_voidDevastator_m2_secondary_explo", base.gameObject, 1.5f * base.characterBody.attackSpeed);
		EffectManager.SimpleMuzzleFlash(MageChokeArea.muzzleFlashEffect, base.gameObject, "MuzzleLeft", transmit: false);
		EffectManager.SimpleMuzzleFlash(MageChokeArea.muzzleFlashEffect, base.gameObject, "MuzzleRight", transmit: false);
		PlayAnimation("Gesture, Additive", "FireWall");
		if (base.isAuthority)
		{
			FireProjectileInfo fireProjectileInfo = new FireProjectileInfo
			{
				crit = RollCrit(),
				owner = base.gameObject,
				position = base.characterBody.corePosition,
				projectilePrefab = MageChokeArea.scriptableObject.Projectiles[0],
				rotation = Util.QuaternionSafeLookRotation(GetAimRay().direction),
				damage = damageStat,
				damageColorIndex = DamageColorIndex.Void
			};
			ProjectileManager.instance.FireProjectile(fireProjectileInfo);
			base.characterBody.AddBuff(MageChokeArea.scriptableObject.buffs[0]);
		}
	}

	public override void FixedUpdate()
	{
		base.FixedUpdate();
		if (base.age > duration)
		{
			if ((bool)base.skillLocator.secondary)
			{
				base.skillLocator.secondary.SetSkillOverride(base.skillLocator.secondary, MageChokeArea.scriptableObject.MiscSkillDefs[0], GenericSkill.SkillOverridePriority.Contextual);
				base.skillLocator.secondary.stock = base.skillLocator.secondary.maxStock;
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
		return InterruptPriority.PrioritySkill;
	}
}
