using EntityStates.Railgunner.Scope;
using GrooveSaladSpikestripContent;
using RoR2;
using RoR2.Audio;
using UnityEngine;
using Spikestrip2_0.Core;

namespace PlasmaCoreSpikestripContent.Content.Skills.States;

public class ActiveScopeTorpor : BaseActive
{
	public override void OnEnter()
	{
		cameraParams = Torpor.cameraParams;
		loopSound = SpikestripUtil.AddressablesLoad<LoopSoundDef>("95cebc5fd9fc83444937c4e509ab9453");
		crosshairOverridePrefab = Torpor.overrideCrosshair;
		scopeOverlayPrefab = Torpor.scopeOverlayPrefab;
		primaryOverride = Torpor.scriptableObject.MiscSkillDefs[0];
		base.OnEnter();
	}

	public override BaseWindDown GetNextState()
	{
		return new WindDownScopeTorpor();
	}

	public override void OnExit()
	{
		if (loopPtr.isValid)
		{
			LoopSoundManager.StopSoundLoopLocal(loopPtr);
		}
		SkillLocator skillLocator = base.skillLocator;
		GenericSkill genericSkill = ((skillLocator != null) ? skillLocator.primary : null);
		if ((bool)genericSkill)
		{
			genericSkill.onSkillChanged -= base.TryOverrideSkill;
		}
		if ((bool)overriddenSkill)
		{
			overriddenSkill.UnsetSkillOverride(this, primaryOverride, GenericSkill.SkillOverridePriority.Contextual);
		}
		EndScopeParamsOverride(0f);
		Animator modelAnimator = GetModelAnimator();
		if ((bool)modelAnimator)
		{
			modelAnimator.SetBool(BaseScopeState.mecanimBoolName, value: false);
		}
		RemoveOverlay(0f);
		crosshairOverrideRequest?.Dispose();
	}
}
