using System.Collections.Generic;
using PlasmaCoreSpikestripContent.Components;
using PlasmaCoreSpikestripContent.Content.Skills;
using RoR2;
using RoR2.Orbs;
using UnityEngine;

namespace PlasmaCoreSpikestripContent.Misc;

public class ChokeOrb : Orb
{
	public static float damageCoef = 1.5f;

	public static float baseForce = 1f;

	public static float massForceBonusMult = 2f;

	public List<HurtBox> futureTargets;

	public ChokeMainZone chokeMainZone;

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
		EffectManager.SpawnEffect(MageChokeArea.scriptableObject.Effects[0], effectData, transmit: true);
		base.Begin();
	}

	public override void OnArrival()
	{
		base.OnArrival();
		if (!chokeMainZone || !chokeMainZone.attackerBody)
		{
			return;
		}
		if ((bool)target && (bool)target.healthComponent && (bool)target.healthComponent.body)
		{
			int num = 0;
			for (int i = 0; i < chokeMainZone.allCharacters.Count; i++)
			{
				if ((bool)chokeMainZone.allCharacters[i])
				{
					if (chokeMainZone.allCharacters[i].gameObject.transform.position.sqrMagnitude - target.transform.position.sqrMagnitude > chokeMainZone.crushRange * chokeMainZone.crushRange)
					{
						num++;
					}
				}
				else
				{
					chokeMainZone.allCharacters.RemoveAt(0);
					i--;
				}
			}
			float num2 = 2f;
			if ((bool)target.healthComponent.gameObject.GetComponent<Rigidbody>())
			{
				num2 = target.healthComponent.gameObject.GetComponent<Rigidbody>().mass;
			}
			if (FriendlyFireManager.ShouldDirectHitProceed(target.healthComponent, chokeMainZone.attackerBody.teamComponent.teamIndex))
			{
				Vector3 vector = chokeMainZone.transform.position - target.gameObject.transform.position;
				DamageInfo damageInfo = new DamageInfo
				{
					damage = chokeMainZone.attackerBody.damage * damageCoef * (1f + (float)num * 0.25f),
					crit = chokeMainZone.attackerBody.RollCrit(),
					inflictor = chokeMainZone.attackerBody.gameObject,
					attacker = chokeMainZone.attackerBody.gameObject,
					force = vector.normalized * (Mathf.Min(vector.magnitude, 20f) * baseForce * (num2 * massForceBonusMult)),
					position = target.transform.position,
					procCoefficient = 1f,
					damageType = DamageSource.Secondary,
					damageColorIndex = DamageColorIndex.Void
				};
				target.healthComponent.TakeDamage(damageInfo);
				GlobalEventManager.instance.OnHitEnemy(damageInfo, target.healthComponent.gameObject);
				GlobalEventManager.instance.OnHitAll(damageInfo, target.healthComponent.gameObject);
			}
			else if (target.healthComponent.body.HasBuff(MageChokeArea.scriptableObject.buffs[0]))
			{
				target.healthComponent.body.RemoveBuff(MageChokeArea.scriptableObject.buffs[0]);
				Vector3 vector2 = chokeMainZone.transform.position - target.gameObject.transform.position;
				target.healthComponent.TakeDamageForce(vector2.normalized * (Mathf.Min(vector2.magnitude, 20f) * baseForce * (num2 * massForceBonusMult)), alwaysApply: true);
			}
			Util.PlaySound("Play_item_proc_novaonheal_spawn", target.gameObject);
			if (futureTargets.Count <= 0)
			{
				return;
			}
			while (!futureTargets[0])
			{
				futureTargets.Remove(futureTargets[0]);
				if (futureTargets.Count == 0)
				{
					return;
				}
			}
			ChokeOrb chokeOrb = new ChokeOrb();
			chokeOrb.origin = target.transform.position;
			chokeOrb.target = futureTargets[0];
			futureTargets.Remove(futureTargets[0]);
			chokeOrb.futureTargets = futureTargets;
			chokeOrb.chokeMainZone = chokeMainZone;
			OrbManager.instance.AddOrb(chokeOrb);
		}
		else
		{
			if (futureTargets.Count <= 0)
			{
				return;
			}
			while (!futureTargets[0])
			{
				futureTargets.Remove(futureTargets[0]);
				if (futureTargets.Count == 0)
				{
					return;
				}
			}
			ChokeOrb chokeOrb2 = new ChokeOrb();
			chokeOrb2.origin = origin;
			chokeOrb2.target = futureTargets[0];
			futureTargets.Remove(futureTargets[0]);
			chokeOrb2.futureTargets = futureTargets;
			chokeOrb2.chokeMainZone = chokeMainZone;
			OrbManager.instance.AddOrb(chokeOrb2);
		}
	}
}
