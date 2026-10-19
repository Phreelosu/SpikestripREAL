using System.Collections.Generic;
using PlasmaCoreSpikestripContent.Content.Skills;
using R2API.Networking.Interfaces;
using RoR2;
using RoR2.Orbs;
using UnityEngine;
using UnityEngine.Networking;

namespace PlasmaCoreSpikestripContent.Misc;

public class ApplyWeaveDebuffNetworkMessage : INetMessage, ISerializableObject
{
	public GameObject attacker;

	public GameObject target;

	public float damage;

	public void Deserialize(NetworkReader reader)
	{
		attacker = reader.ReadGameObject();
		target = reader.ReadGameObject();
		damage = reader.ReadSingle();
	}

	public void OnReceived()
	{
		if (!NetworkServer.active)
		{
			return;
		}
		CharacterBody component = target.GetComponent<CharacterBody>();
		if (!component)
		{
			return;
		}
		List<HealthComponent> list = new List<HealthComponent>(0);
		foreach (HurtBox readOnlyBullseyes in HurtBox.readOnlyBullseyesList)
		{
			if ((bool)readOnlyBullseyes.healthComponent && readOnlyBullseyes.healthComponent != component.healthComponent && readOnlyBullseyes.healthComponent.body.HasBuff(Weave.scriptableObject.buffs[0]) && !list.Contains(readOnlyBullseyes.healthComponent))
			{
				WeaveOrb weaveOrb = new WeaveOrb();
				weaveOrb.damage = damage;
				weaveOrb.origin = target.transform.position;
				weaveOrb.target = readOnlyBullseyes;
				weaveOrb.attackerBody = attacker.GetComponent<CharacterBody>();
				OrbManager.instance.AddOrb(weaveOrb);
				list.Add(readOnlyBullseyes.healthComponent);
			}
		}
		component.AddTimedBuff(Weave.scriptableObject.buffs[0], 4f);
	}

	public void Serialize(NetworkWriter writer)
	{
		writer.Write(attacker);
		writer.Write(target);
		writer.Write(damage);
	}
}
