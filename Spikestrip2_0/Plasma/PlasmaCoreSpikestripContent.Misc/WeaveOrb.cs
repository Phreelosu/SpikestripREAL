using PlasmaCoreSpikestripContent.Content.Skills;
using RoR2;
using RoR2.Orbs;
using UnityEngine;

namespace PlasmaCoreSpikestripContent.Misc;

public class WeaveOrb : Orb
{
	public CharacterBody attackerBody;

	public bool isCrit;

	public float damage = 1.5f;

	public override void Begin()
	{
		base.duration = 0.2f;
		EffectData effectData = new EffectData
		{
			scale = 1f,
			origin = origin,
			start = origin,
			rotation = Util.QuaternionSafeLookRotation(target.transform.position - origin),
			genericFloat = base.duration
		};
		effectData.SetHurtBoxReference(target);
		EffectManager.SpawnEffect(Weave.scriptableObject.Effects[1], effectData, transmit: true);
		base.Begin();
	}

	public override void OnArrival()
	{
		base.OnArrival();
		if ((bool)target && (bool)target.healthComponent && (bool)target.healthComponent.body && (bool)target.healthComponent.body.teamComponent && (bool)attackerBody && target.healthComponent.body.teamComponent.teamIndex != attackerBody.teamComponent.teamIndex)
		{
			DamageInfo damageInfo = new DamageInfo
			{
				damage = damage,
				crit = isCrit,
				inflictor = attackerBody.gameObject,
				attacker = attackerBody.gameObject,
				force = Vector3.zero,
				position = target.transform.position,
				procCoefficient = 1f,
				damageType = DamageType.Generic,
				damageColorIndex = DamageColorIndex.Void
			};
			target.healthComponent.TakeDamage(damageInfo);
			GlobalEventManager.instance.OnHitEnemy(damageInfo, target.healthComponent.gameObject);
			GlobalEventManager.instance.OnHitAll(damageInfo, target.healthComponent.gameObject);
		}
	}
}
