using EntityStates;
using GrooveSaladSpikestripContent;
using RoR2;
using UnityEngine;
using UnityEngine.Networking;
using Spikestrip2_0.Core;

namespace PlasmaCoreSpikestripContent.Content.Monsters.States;

public class Closed : BaseState
{
	public BuffDef buffDef;

	public override void OnEnter()
	{
		buffDef = SigmaConstruct.instance.scriptableObject.Buffs[0];
		base.OnEnter();
		PlayAnimation("Body", "RevealedToHidden");
		Util.PlaySound("Play_minorConstruct_hide", base.gameObject);
		if ((bool)buffDef && NetworkServer.active)
		{
			base.characterBody.AddBuff(buffDef);
		}
		Transform transform = FindModelChild("Shield");
		if ((bool)transform)
		{
			transform.gameObject.SetActive(value: true);
		}
	}

	public override void FixedUpdate()
	{
		base.FixedUpdate();
		if ((base.isAuthority && !base.characterBody.outOfCombat) || !base.characterBody.outOfDanger)
		{
			outer.SetNextState(new Opened());
		}
	}

	public override void OnExit()
	{
		base.OnExit();
		if ((bool)buffDef && NetworkServer.active)
		{
			base.characterBody.RemoveBuff(buffDef);
		}
		Transform transform = FindModelChild("Shield");
		if ((bool)transform)
		{
			transform.gameObject.SetActive(value: false);
		}
	}
}
