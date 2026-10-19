using System;
using GrooveSaladSpikestripContent;
using PlasmaCoreSpikestripContent.Content.Skills.States;
using PlasmaCoreSpikestripContent.Core;
using R2API;
using RoR2;
using RoR2.Skills;
using UnityEngine;
using Spikestrip2_0.Core;

namespace PlasmaCoreSpikestripContent.Content.Skills;

public class Weave : SpikestripVoidAltSkillsBase<Weave>
{
	public static GameObject muzzleFlashEffect;

	public static SSSkillBaseSO scriptableObject;

	public override string SkillName => "「Weave」";

	public override string SkillDescription => scriptableObject.Description;

	public override string SkillToken => scriptableObject.SkillToken;

	public override string PrerequisiteIdentifier => scriptableObject.PrerequisiteIdentifier;

	public override string CharacterName => scriptableObject.CharacterName;

	public override string AchievementName => scriptableObject.AchievementName;

	public override Sprite SkillIcon => scriptableObject.SkillIcon;

	public override GameObject OwnerBodyPrefab => SpikestripUtil.LegacyLoad<GameObject>(scriptableObject.OwnerBodyPath);

	public override SkillFamily[] SkillFamilies => new SkillFamily[1] { SpikestripUtil.LegacyLoad<SkillFamily>(scriptableObject.SkillFamilyPath) };

	public override Type[] EntityStateTypes => new Type[1] { typeof(FireWeave) };

	public override SkillDef GetSkillDef()
	{
		return scriptableObject.SkillDefinition;
	}

	public override void Init()
	{
		PlasmaCorePlugin.skillBaseScriptableObjects.TryGetValue(SkillName, out scriptableObject);
		GameObject[] effects = scriptableObject.Effects;
		foreach (GameObject effectPrefab in effects)
		{
			ContentAddition.AddEffect(effectPrefab);
		}
		ContentAddition.AddBuffDef(scriptableObject.buffs[0]);
		LanguageAPI.Add("KEYWORD_ENTANGLE", "<style=cKeywordName>Entangled</style><style=cSub> enemies suffer <style=cIsVoid>50% base damage</style> when an ally is hit by this attack.</style>");
		base.Init();
	}
}
