using System.Collections.Generic;
using PlasmaCoreSpikestripContent.Content.Skills;
using PlasmaCoreSpikestripContent.Core;
using PlasmaCoreSpikestripContent.Misc;
using RoR2;
using RoR2.Orbs;
using RoR2.Projectile;
using UnityEngine;
using UnityEngine.Networking;

namespace PlasmaCoreSpikestripContent.Components;

public class ChokeMainZone : NetworkBehaviour
{
	public enum BounceType
	{
		CloseToCenter,
		Random
	}

	public static int baseMaxTicks = 10;

	public int maxTicks = 10;

	public int ticksFired;

	public static float baseTickTime = 1f;

	public float tickTime = 1f;

	public float stopwatch;

	public float radius = 20f;

	public List<HealthComponent> allCharacters;

	public List<HealthComponent> forceTargets = new List<HealthComponent>(0);

	public CharacterBody attackerBody;

	public float crushRange = 4f;

	public BounceType bounceType = BounceType.Random;

	public GameObject destroyEffect;

	// Only read on the server, so it never needed syncing (the original was never weaved).
	public bool impactServer;

	public bool impactLocal;

	public GameObject effectPrefab;

	public static List<ChokeMainZone> instances = new List<ChokeMainZone>(0);

	public void Impact()
	{
		if (NetworkServer.active)
		{
			impactServer = true;
			maxTicks = (int)((float)baseMaxTicks * attackerBody.attackSpeed);
			tickTime = baseTickTime / attackerBody.attackSpeed;
			FireTendrils();
		}
		Rigidbody component = GetComponent<Rigidbody>();
		if ((bool)component)
		{
			component.velocity = Vector3.zero;
			component.useGravity = false;
		}
	}

	private void Start()
	{
		attackerBody = GetComponent<ProjectileController>().owner.GetComponent<CharacterBody>();
		if (!attackerBody)
		{
			Object.Destroy(base.gameObject);
		}
		instances.Add(this);
	}

	private void OnDestroy()
	{
		instances.Remove(this);
		// The attacker may have died, or the scene may be unloading, before the zone is destroyed.
		if ((bool)attackerBody && attackerBody.hasAuthority && (bool)attackerBody.mainHurtBox && (bool)OrbManager.instance)
		{
			OrbManager.instance.AddOrb(new ChokeReturnOrb
			{
				duration = 0.5f,
				target = attackerBody.mainHurtBox,
				origin = base.transform.position
			});
		}
	}

	private void FixedUpdate()
	{
		if (!impactServer || !NetworkServer.active)
		{
			return;
		}
		if (!impactLocal && (bool)effectPrefab)
		{
			impactLocal = true;
			EffectManager.SpawnEffect(effectPrefab, new EffectData
			{
				origin = base.transform.position
			}, transmit: true);
		}
		stopwatch += Time.fixedDeltaTime;
		if (!(stopwatch > tickTime))
		{
			return;
		}
		stopwatch -= tickTime;
		FireTendrils();
		if (ticksFired >= maxTicks)
		{
			if ((bool)destroyEffect)
			{
				EffectData effectData = new EffectData
				{
					scale = 1f,
					origin = base.transform.position,
					start = base.transform.position,
					rotation = Quaternion.identity
				};
				EffectManager.SpawnEffect(destroyEffect, effectData, transmit: true);
			}
			stopwatch = -100000f;
			Object.Destroy(base.gameObject);
		}
	}

	public void FireTendrils()
	{
		Collider[] array = Physics.OverlapSphere(base.transform.position, radius, LayerIndex.entityPrecise.mask);
		List<HurtBox> list = new List<HurtBox>();
		Collider[] array2 = array;
		for (int i = 0; i < array2.Length; i++)
		{
			HurtBox component = array2[i].GetComponent<HurtBox>();
			if (!component || !component.healthComponent)
			{
				continue;
			}
			bool flag = false;
			for (int j = 0; j < list.Count; j++)
			{
				if (list[j].healthComponent == component.healthComponent)
				{
					if (!list[j].isBullseye && component.isBullseye)
					{
						list[j] = component;
					}
					flag = true;
					break;
				}
			}
			if (!flag)
			{
				list.Add(component);
			}
		}
		allCharacters = new List<HealthComponent>();
		for (int k = 0; k < list.Count; k++)
		{
			if (list[k].healthComponent.body.HasBuff(MageChokeArea.scriptableObject.buffs[0]) || FriendlyFireManager.ShouldSplashHitProceed(list[k].healthComponent, attackerBody.teamComponent.teamIndex))
			{
				allCharacters.Add(list[k].healthComponent);
				continue;
			}
			list.Remove(list[k]);
			k--;
		}
		foreach (HealthComponent forceTarget in forceTargets)
		{
			if ((bool)forceTarget.body.mainHurtBox && forceTarget.body.HasBuff(MageChokeArea.scriptableObject.buffs[1]))
			{
				list.Add(forceTarget.body.mainHurtBox);
				allCharacters.Add(forceTarget);
				forceTarget.body.RemoveOldestTimedBuff(MageChokeArea.scriptableObject.buffs[1]);
			}
		}
		PlasmaLog.Log("FireTendrils: Sorting " + list.Count + " colliders", allLogOnly: true);
		list = SortHurtBoxByDistance(list);
		if (list.Count > 0)
		{
			Util.PlayAttackSpeedSound("Play_voidJailer_m2_shoot", base.gameObject, 0.75f);
			while (list.Count > 0)
			{
				List<HurtBox> currentOrbHurtBoxes = new List<HurtBox>();
				HurtBox item = list[0];
				list.Remove(item);
				currentOrbHurtBoxes.Add(item);
				FindBounceTarget(ref list, ref currentOrbHurtBoxes);
				ChokeOrb chokeOrb = new ChokeOrb();
				chokeOrb.origin = base.transform.position;
				chokeOrb.target = currentOrbHurtBoxes[0];
				currentOrbHurtBoxes.Remove(currentOrbHurtBoxes[0]);
				chokeOrb.futureTargets = currentOrbHurtBoxes;
				chokeOrb.chokeMainZone = this;
				OrbManager.instance.AddOrb(chokeOrb);
			}
		}
		ticksFired++;
	}

	public List<HurtBox> SortHurtBoxByDistance(List<HurtBox> hurtBoxes)
	{
		PlasmaLog.Log("SortHurtBoxByDistance: Starting sort", allLogOnly: true);
		List<HurtBox> list = new List<HurtBox>();
		foreach (HurtBox hurtBox in hurtBoxes)
		{
			if (list.Count == 0)
			{
				list.Add(hurtBox);
				continue;
			}
			for (int i = 0; i < list.Count && !(list[i] == hurtBox); i++)
			{
				if (i == list.Count - 1)
				{
					list.Add(hurtBox);
					break;
				}
				if ((base.transform.position - hurtBox.transform.position).magnitude > (base.transform.position - list[i].transform.position).magnitude && (base.transform.position - hurtBox.transform.position).magnitude < (base.transform.position - list[i + 1].transform.position).magnitude)
				{
					list.Insert(i, hurtBox);
					break;
				}
			}
		}
		if (PlasmaLog.debugAll)
		{
			PlasmaLog.Log("SortHurtBoxByDistance: Outputting results of sort", allLogOnly: true);
			foreach (HurtBox item in list)
			{
				PlasmaLog.Log(item.gameObject.name + " for HealthComponent of " + item.healthComponent.gameObject.name + " at distance: " + (base.transform.position - item.transform.position).magnitude, allLogOnly: true);
			}
			PlasmaLog.Log("SortHurtBoxByDistance: Done.", allLogOnly: true);
		}
		return list;
	}

	public void FindBounceTarget(ref List<HurtBox> hurtBoxes, ref List<HurtBox> currentOrbHurtBoxes)
	{
		if (hurtBoxes.Count <= 0)
		{
			return;
		}
		List<HurtBox> list = new List<HurtBox>();
		foreach (HurtBox hurtBox in hurtBoxes)
		{
			if ((hurtBox.transform.position - currentOrbHurtBoxes[currentOrbHurtBoxes.Count - 1].transform.position).sqrMagnitude < crushRange * crushRange)
			{
				if (bounceType == BounceType.CloseToCenter)
				{
					currentOrbHurtBoxes.Add(hurtBox);
					hurtBoxes.Remove(hurtBox);
					FindBounceTarget(ref hurtBoxes, ref currentOrbHurtBoxes);
				}
				else if (bounceType == BounceType.Random)
				{
					list.Add(hurtBox);
				}
			}
		}
		if (bounceType == BounceType.Random && list.Count > 0)
		{
			HurtBox item = list[Random.Range(0, list.Count - 1)];
			currentOrbHurtBoxes.Add(item);
			hurtBoxes.Remove(item);
			FindBounceTarget(ref hurtBoxes, ref currentOrbHurtBoxes);
		}
	}
}
