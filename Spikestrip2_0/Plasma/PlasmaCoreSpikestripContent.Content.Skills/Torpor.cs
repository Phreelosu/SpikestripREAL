using System;
using GrooveSaladSpikestripContent;
using JetBrains.Annotations;
using On.RoR2;
using PlasmaCoreSpikestripContent.Content.Skills.States;
using PlasmaCoreSpikestripContent.Core;
using R2API;
using RoR2;
using RoR2.Skills;
using RoR2.UI;
using UnityEngine;
using UnityEngine.UI;
using Spikestrip2_0.Core;

namespace PlasmaCoreSpikestripContent.Content.Skills;

public class Torpor : SpikestripVoidAltSkillsBase<Torpor>
{
	public static GameObject muzzleFlashEffect;

	public static SSSkillBaseSO scriptableObject;

	public static GameObject scopeOverlayPrefab;

	public static GameObject reloadOverlayPrefab;

	public static GameObject overrideCrosshair;

	public static RoR2.CharacterCameraParams cameraParams;

	public override string SkillName => "「Torpor」";

	public override string SkillDescription => scriptableObject.Description;

	public override string SkillToken => scriptableObject.SkillToken;

	public override string PrerequisiteIdentifier => scriptableObject.PrerequisiteIdentifier;

	public override string CharacterName => scriptableObject.CharacterName;

	public override string AchievementName => scriptableObject.AchievementName;

	public override Sprite SkillIcon => scriptableObject.SkillIcon;

	public override GameObject OwnerBodyPrefab => SpikestripUtil.LegacyLoad<GameObject>(scriptableObject.OwnerBodyPath);

	public override SkillFamily[] SkillFamilies => new SkillFamily[1] { SpikestripUtil.LegacyLoad<SkillFamily>(scriptableObject.SkillFamilyPath) };

	public override Type[] EntityStateTypes => new Type[5]
	{
		typeof(WindUpScopeTorpor),
		typeof(WindDownScopeTorpor),
		typeof(ActiveScopeTorpor),
		typeof(FireSnipeTorpor),
		typeof(ReloadingAgressive)
	};

	public override SkillDef GetSkillDef()
	{
		return scriptableObject.SkillDefinition;
	}

	public override void Init()
	{
		PlasmaCorePlugin.skillBaseScriptableObjects.TryGetValue(SkillName, out scriptableObject);
		// Icon by literallyeurope (1.0.4), shipped separately in GrooveUnsharedUtils' bundle.
		scriptableObject.SkillIcon = SpikestripContent.UnsharedBundle.LoadAsset<Sprite>("texTorporIcon");
		LanguageAPI.Add("RAILGUNNER_AGRESSIVE_RELOAD_NAME", "Agressive Reload");
		LanguageAPI.Add("RAILGUNNER_AGRESSIVE_RELOAD_DESC", "Time your reload to <style=cIsVoid>fire repeatedly</style>.");
		LanguageAPI.Add("RAILGUNNER_SNIPE_TORPOR_NAME", "Fire 「Torpor」");
		LanguageAPI.Add("RAILGUNNER_SNIPE_TORPOR_DESC", "Fire a shot with your piercing <style=cIsVoid>2500% damage</style> plasma railgun.");
		LanguageAPI.Add("KEYWORD_DORMANT", "<style=cKeywordName>Dormant</style><style=cSub>Using other skills <style=cIsVoid>reduces</style> this skill's cooldown by <style=cIsVoid>1.5s</style>.");
		LanguageAPI.Add("KEYWORD_AGRESSIVE_RELOAD", "<style=cKeywordName>Aggressive Reload</style><style=cSub><style=cSub>Reload your railgun after firing. Executing a  <style=cIsVoid>perfect reload</style> immediately fires <style=cIsVoid>「Torpor」</style> again.");
		LanguageAPI.Add("RAILGUNNER_FLAVOR_SCOPETORPOR", "??? MODULE AWAKE.");
		SkillDef[] miscSkillDefs = scriptableObject.MiscSkillDefs;
		foreach (SkillDef item in miscSkillDefs)
		{
			SpikestripUtil.FillInertSkillDef(item);
			ContentAddition.AddSkillDef(item);
		}
		scopeOverlayPrefab = PrefabAPI.InstantiateClone(SpikestripUtil.AddressablesLoad<GameObject>("db5a0c21c1f689c4292ae5e292fd4f0e"), "TorporScopeOverlay", false);
		scopeOverlayPrefab.transform.GetChild(1).GetComponent<RawImage>().color = new Color32(19, 4, 10, 150);
		scopeOverlayPrefab.transform.GetChild(0).GetComponent<SniperTargetViewer>().visualizerPrefab = scriptableObject.MiscGameObjects[0];
		reloadOverlayPrefab = PrefabAPI.InstantiateClone(SpikestripUtil.AddressablesLoad<GameObject>("6d0dc1abdc7493b4d8875a220e487de4"), "TorporReloadUI", false);
		Image[] componentsInChildren = reloadOverlayPrefab.GetComponentsInChildren<Image>(includeInactive: true);
		componentsInChildren[0].color = new Color32(57, 59, 118, 110);
		componentsInChildren[1].color = new Color32(32, 11, 34, 197);
		componentsInChildren[2].color = new Color32(148, 153, byte.MaxValue, byte.MaxValue);
		componentsInChildren[3].color = new Color32(125, 16, 85, byte.MaxValue);
		componentsInChildren[4].color = new Color32(207, 76, 202, 166);
		componentsInChildren[5].color = new Color32(174, 136, byte.MaxValue, byte.MaxValue);
		componentsInChildren[6].color = new Color32(251, 220, byte.MaxValue, byte.MaxValue);
		componentsInChildren[7].color = new Color32(219, 55, 177, byte.MaxValue);
		overrideCrosshair = scriptableObject.MiscGameObjects[1];
		cameraParams = UnityEngine.Object.Instantiate(SpikestripUtil.AddressablesLoad<RoR2.CharacterCameraParams>("3c56c5a9e0a16c345b3aa02c048553f9"));
		cameraParams.data.fov.value = 40f;
		On.RoR2.CharacterBody.Awake += CharacterBody_Awake;
		base.Init();
	}

	public void CharacterBody_Awake(On.RoR2.CharacterBody.orig_Awake orig, RoR2.CharacterBody self)
	{
		orig(self);
		if (self.baseNameToken == "RAILGUNNER_BODY_NAME")
		{
			self.onSkillActivatedAuthority += CharacterBody_onSkillActivatedAuthority;
		}
	}

	public void CharacterBody_onSkillActivatedAuthority([NotNull] RoR2.GenericSkill obj)
	{
		RoR2.SkillLocator component = obj.GetComponent<RoR2.SkillLocator>();
		if ((bool)component && (bool)component.secondary && component.secondary.skillDef.skillName == SkillDef.skillName)
		{
			RoR2.InputBankTest component2 = obj.GetComponent<RoR2.InputBankTest>();
			if (!component2 || !component2.skill2.down)
			{
				component.secondary.RunRecharge(1.5f);
			}
		}
	}
}
