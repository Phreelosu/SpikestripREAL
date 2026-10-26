using EntityStates;
using EntityStates.Railgunner.Reload;
using EntityStates.Railgunner.Weapon;
using GrooveSaladSpikestripContent;
using RoR2;
using UnityEngine;
using Spikestrip2_0.Core;

namespace PlasmaCoreSpikestripContent.Content.Skills.States;

public class FireSnipeTorpor : BaseFireSnipe
{
	public bool highInteruptPriority;

	public FireSnipeTorpor()
	{
	}

	public FireSnipeTorpor(bool priority)
	{
		highInteruptPriority = priority;
	}

	public override void OnEnter()
	{
		crosshairOverridePrefab = Torpor.overrideCrosshair;
		hitEffectPrefab = SpikestripUtil.AddressablesLoad<GameObject>("59aeb9d13a08cee479e7d33108a8a3e9");
		force = 300f;
		fireSoundString = "Play_voidman_m2_shoot";
		damageCoefficient = 2f;
		critDamageMultiplier = 1f;
		bulletRadius = 0.1f;
		bulletCount = 8;
		baseDuration = 0.25f;
		animationStateName = "FireSniper";
		animationPlaybackRateParam = "FireSniper.playbackRate";
		animationLayerName = "Gesture, Override";
		recoilAmplitudeY = 5f;
		recoilAmplitudeX = 1.5f;
		queueReload = false;
		procCoefficient = 1f;
		piercingDamageCoefficientPerTarget = 0.5f;
		muzzleName = "MuzzleRight";
		muzzleFlashPrefab = SpikestripUtil.AddressablesLoad<GameObject>("899041d000ece5e49970ee1866cace48");
		minSpread = 0f;
		maxSpread = 3f;
		maxDistance = 2000f;
		isPiercing = true;
		useSmartCollision = true;
		useSecondaryStocks = true;
		tracerEffectPrefab = SpikestripUtil.AddressablesLoad<GameObject>("47a345efb5b31f54aadb66df3be99b26");
		spreadYawScale = 1f;
		spreadPitchScale = 0.75f;
		spreadBloomValue = 1f;
		selfKnockbackForce = 200f;
		piercingDamageCoefficientPerTarget = 1f;
		// Newer BaseFireSnipe looks this up as a network sound and throws on null; vanilla snipes get it from their state config.
		headshotSoundString = "Play_railgunner_m2_headshot";
		base.OnEnter();
		Animator modelAnimator = GetModelAnimator();
		if ((bool)modelAnimator)
		{
			modelAnimator.SetBool("isScoped", value: true);
		}
	}

	public override void ModifyBullet(BulletAttack bulletAttack)
	{
		bulletAttack.sniper = false;
		// Vanilla's scoped shot (FireSnipeHeavy) counts as the secondary
		bulletAttack.damageType.damageSource = DamageSource.Secondary;
		bulletAttack.isCrit = base.characterBody.RollCrit();
		bulletAttack.falloffModel = BulletAttack.FalloffModel.None;
		EntityStateMachine entityStateMachine = EntityStateMachine.FindByCustomName(base.gameObject, "Reload");
		if ((bool)entityStateMachine)
		{
			if (entityStateMachine.state is Boosted boosted)
			{
				bulletAttack.damage += boosted.GetBonusDamage();
				boosted.ConsumeBoost(queueReload: false);
			}
			if (base.skillLocator.primary.stock <= 0)
			{
				base.skillLocator.secondary.AddOneStock();
				entityStateMachine.SetNextState(new ReloadingAgressive());
			}
		}
		if (isPiercing)
		{
			bulletAttack.stopperMask = LayerIndex.world.mask;
		}
		bulletAttack.modifyOutgoingDamageCallback = delegate(BulletAttack _bulletAttack, ref BulletAttack.BulletHit hitInfo, DamageInfo damageInfo)
		{
			_bulletAttack.damage *= piercingDamageCoefficientPerTarget;
			wasMiss = false;
			if (damageInfo.crit)
			{
				damageInfo.damage *= critDamageMultiplier;
				wasAtLeastOneWeakpoint = true;
			}
		};
		EntityStateMachine entityStateMachine2 = EntityStateMachine.FindByCustomName(base.gameObject, "Backpack");
		EntityState entityState = InstantiateBackpackState();
		if ((bool)entityStateMachine2 && entityState != null)
		{
			entityStateMachine2.SetNextState(entityState);
		}
	}

	public override void OnExit()
	{
		base.OnExit();
		Animator modelAnimator = GetModelAnimator();
		if ((bool)modelAnimator)
		{
			modelAnimator.SetBool("isScoped", value: false);
		}
	}

	public override InterruptPriority GetMinimumInterruptPriority()
	{
		if (highInteruptPriority)
		{
			return InterruptPriority.PrioritySkill;
		}
		return base.GetMinimumInterruptPriority();
	}
}
