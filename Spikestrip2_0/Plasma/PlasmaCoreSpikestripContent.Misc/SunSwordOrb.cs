using PlasmaCoreSpikestripContent.Content.Items;
using RoR2;
using RoR2.Orbs;

namespace PlasmaCoreSpikestripContent.Misc;

public class SunSwordOrb : Orb
{
	public CharacterBody attackerBody;

	public static float sunSwordDamageMult = 15f;

	public override void Begin()
	{
		base.duration = 1f;
		EffectData effectData = new EffectData
		{
			scale = 1f,
			origin = origin,
			start = origin,
			rotation = Util.QuaternionSafeLookRotation(target.transform.position - origin),
			genericFloat = base.duration
		};
		effectData.SetHurtBoxReference(target);
		EffectManager.SpawnEffect(SunSword.effect, effectData, transmit: true);
		base.Begin();
	}

	public override void OnArrival()
	{
		base.OnArrival();
		// The orb flies for a second; the target or attacker may be gone by the time it lands.
		if (!target || !target.healthComponent || !target.healthComponent.body || !attackerBody)
		{
			return;
		}
		InflictDotInfo inflictDotInfo = new InflictDotInfo
		{
			victimObject = target.healthComponent.body.gameObject,
			attackerObject = attackerBody.gameObject,
			totalDamage = 1f * attackerBody.damage * sunSwordDamageMult,
			dotIndex = DotController.DotIndex.StrongerBurn,
			damageMultiplier = 15f,
			duration = 1.5f
		};
		Util.PlaySound("Play_merc_R_slicingBlades_throw", target.gameObject);
		DotController.InflictDot(ref inflictDotInfo);
	}
}
