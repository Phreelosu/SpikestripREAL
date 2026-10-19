using EntityStates.Railgunner.Reload;
using RoR2;
using RoR2.UI;
using UnityEngine;

namespace PlasmaCoreSpikestripContent.Content.Skills.States;

public class ReloadingAgressive : Reloading
{
	public CrosshairUtils.OverrideRequest crosshairOverrideRequest;

	public GameObject crosshairOverridePrefab;

	public bool pushedButton;

	public bool playedSound;

	public override void OnEnter()
	{
		primaryOverride = Torpor.scriptableObject.MiscSkillDefs[1];
		overlayPrefab = Torpor.reloadOverlayPrefab;
		overlayChildLocatorEntry = "CrosshairExtras";
		failSoundString = "Play_railgunner_m2_reload_basic";
		endNoBoostSoundString = "Play_railgunner_m2_reload_fail";
		boostWindowDuration = 0.25f;
		boostWindowDelay = Random.Range(0.5f, 0.65f);
		boostGracePeriod = 0.015f;
		baseDuration = 1f;
		crosshairOverridePrefab = Torpor.overrideCrosshair;
		if ((bool)base.skillLocator.primary)
		{
			base.skillLocator.primary.UnsetSkillOverride(this, Torpor.scriptableObject.MiscSkillDefs[0], GenericSkill.SkillOverridePriority.Contextual);
		}
		base.OnEnter();
		if ((bool)crosshairOverridePrefab)
		{
			crosshairOverrideRequest = CrosshairUtils.RequestOverrideForBody(base.characterBody, crosshairOverridePrefab, CrosshairUtils.OverridePriority.Skill);
		}
	}

	public override void Update()
	{
		base.Update();
		if (!playedSound && adjustedBoostWindowDelay - base.fixedAge < boostGracePeriod)
		{
			playedSound = true;
			Util.PlaySound("Play_engi_seekerMissile_HUD_close", outer.gameObject);
		}
		if (base.inputBank.skill1.justPressed && !pushedButton && base.fixedAge > Time.fixedDeltaTime)
		{
			AttemptBoost();
		}
	}

	public override void FixedUpdate()
	{
		base.fixedAge += Time.fixedDeltaTime;
		float num = ((duration > 0f) ? (base.fixedAge / duration) : 0f);
		foreach (ActiveReloadBarController reloadUi in reloadUiList)
		{
			reloadUi.SetTValue(num);
			reloadUi.SetIsWindowActive(IsInBoostWindow());
		}
		if (duration <= 0f || num >= 1f)
		{
			Util.PlaySound(endNoBoostSoundString, base.gameObject);
			outer.SetNextState(new Waiting());
		}
		if (base.inputBank.skill1.justPressed && !pushedButton && base.fixedAge > Time.fixedDeltaTime)
		{
			AttemptBoost();
		}
	}

	public override void OnExit()
	{
		crosshairOverrideRequest?.Dispose();
		if (!pushedButton)
		{
			base.skillLocator.primary.RemoveAllStocks();
			base.skillLocator.secondary.RemoveAllStocks();
		}
		base.OnExit();
	}

	public new bool AttemptBoost()
	{
		pushedButton = true;
		if (IsInBoostWindow())
		{
			EntityStateMachine.FindByCustomName(outer.gameObject, "Weapon").SetNextState(new FireSnipeTorpor(priority: true));
			return true;
		}
		base.skillLocator.primary.RemoveAllStocks();
		base.skillLocator.secondary.RemoveAllStocks();
		foreach (ActiveReloadBarController reloadUi in reloadUiList)
		{
			reloadUi.SetWasFailure(wasFailure: true);
		}
		return false;
	}
}
