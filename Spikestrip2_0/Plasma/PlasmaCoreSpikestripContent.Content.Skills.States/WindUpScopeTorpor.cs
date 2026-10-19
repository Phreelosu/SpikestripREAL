using EntityStates.Railgunner.Scope;
using GrooveSaladSpikestripContent;
using UnityEngine;
using Spikestrip2_0.Core;

namespace PlasmaCoreSpikestripContent.Content.Skills.States;

public class WindUpScopeTorpor : BaseWindUp
{
	public override void OnEnter()
	{
		scopeOverlayPrefab = Torpor.scopeOverlayPrefab;
		enterSoundString = "Play_railgunner_m2_scope_in";
		crosshairOverridePrefab = SpikestripUtil.AddressablesLoad<GameObject>("8323d172701659846b133b5a84b01111");
		cameraParams = Torpor.cameraParams;
		baseDuration = 0.1f;
		base.OnEnter();
	}

	public override BaseActive GetNextState()
	{
		return new ActiveScopeTorpor();
	}
}
