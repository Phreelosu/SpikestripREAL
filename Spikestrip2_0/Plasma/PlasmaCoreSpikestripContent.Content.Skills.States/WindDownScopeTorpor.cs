using EntityStates.Railgunner.Scope;
using GrooveSaladSpikestripContent;
using UnityEngine;
using Spikestrip2_0.Core;

namespace PlasmaCoreSpikestripContent.Content.Skills.States;

public class WindDownScopeTorpor : BaseWindDown
{
	public override void OnEnter()
	{
		scopeOverlayPrefab = Torpor.scopeOverlayPrefab;
		enterSoundString = "Play_railgunner_m2_scope_out";
		crosshairOverridePrefab = SpikestripUtil.AddressablesLoad<GameObject>("68aca88fb0a2954408252bc25017e154");
		cameraParams = Torpor.cameraParams;
		baseDuration = 0.2f;
		base.OnEnter();
	}
}
