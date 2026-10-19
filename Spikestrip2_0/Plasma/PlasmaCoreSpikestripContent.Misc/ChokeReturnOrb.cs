using PlasmaCoreSpikestripContent.Content.Skills;
using RoR2;
using RoR2.Orbs;

namespace PlasmaCoreSpikestripContent.Misc;

public class ChokeReturnOrb : Orb
{
	public override void Begin()
	{
		EffectData effectData = new EffectData
		{
			scale = 1f,
			origin = origin,
			start = origin,
			rotation = Util.QuaternionSafeLookRotation(target.transform.position - origin),
			genericFloat = base.duration
		};
		effectData.SetHurtBoxReference(target);
		EffectManager.SpawnEffect(MageChokeArea.scriptableObject.Effects[4], effectData, transmit: true);
		base.Begin();
	}

	public override void OnArrival()
	{
		base.OnArrival();
		if ((bool)target && (bool)target.healthComponent && (bool)target.healthComponent.body.skillLocator)
		{
			GenericSkill secondary = target.healthComponent.body.skillLocator.secondary;
			if ((bool)secondary)
			{
				secondary.UnsetSkillOverride(target.healthComponent.body.skillLocator.secondary, MageChokeArea.scriptableObject.MiscSkillDefs[0], GenericSkill.SkillOverridePriority.Contextual);
				target.healthComponent.body.skillLocator.secondary.AddOneStock();
			}
		}
	}
}
