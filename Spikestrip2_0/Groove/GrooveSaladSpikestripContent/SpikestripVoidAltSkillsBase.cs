using On.RoR2.UI;
using R2API;
using RoR2;
using RoR2.Achievements;
using RoR2.UI;
using UnityEngine;
using UnityEngine.Events;
using Spikestrip2_0.Core;

namespace GrooveSaladSpikestripContent;

public abstract class SpikestripVoidAltSkillsBase<T> : SpikestripAltSkillsBase<T> where T : SpikestripVoidAltSkillsBase<T>
{
	public class SpecificVoidEndingAchievement<A> : BaseAchievement where A : SpikestripVoidAltSkillsBase<A>
	{
		public class SpecificCompleteWave50ServerAchievement : BaseServerAchievement
		{
			public override void OnInstall()
			{
				base.OnInstall();
				InfiniteTowerRun.onAllEnemiesDefeatedServer += OnAllEnemiesDefeatedServer;
			}

			public override void OnUninstall()
			{
				InfiniteTowerRun.onAllEnemiesDefeatedServer -= OnAllEnemiesDefeatedServer;
				base.OnUninstall();
			}

			private void OnAllEnemiesDefeatedServer(InfiniteTowerWaveController waveController)
			{
				InfiniteTowerRun infiniteTowerRun = Run.instance as InfiniteTowerRun;
				if ((bool)infiniteTowerRun && infiniteTowerRun.waveIndex >= 50)
				{
					Grant();
				}
			}
		}

		public override BodyIndex LookUpRequiredBodyIndex()
		{
			return BodyCatalog.FindBodyIndex(ContentBase<A>.instance.OwnerBodyPrefab);
		}

		public override void OnBodyRequirementMet()
		{
			base.OnBodyRequirementMet();
			SetServerTracked(shouldTrack: true);
			Run.onClientGameOverGlobal += OnClientGameOverGlobal;
		}

		public override void OnBodyRequirementBroken()
		{
			Run.onClientGameOverGlobal -= OnClientGameOverGlobal;
			SetServerTracked(shouldTrack: false);
			base.OnBodyRequirementBroken();
		}

		private void OnClientGameOverGlobal(Run run, RunReport runReport)
		{
			if ((bool)runReport.gameEnding && runReport.gameEnding == DLC1Content.GameEndings.VoidEnding)
			{
				Grant();
			}
		}
	}

	public static Color voidSkillColor = SpikestripUtil.ColorRGB(176f, 72f, 214f);

	public UnlockableDef VoidUnlockableDef;

	public AchievementDef VoidAchievementDef;

	public override string ConfigSection => "Alternate Skills";

	public override string ConfigName => SkillName;

	public abstract string PrerequisiteIdentifier { get; }

	public abstract string CharacterName { get; }

	public abstract string AchievementName { get; }

	public virtual string ChildLocatorString { get; }

	public override UnlockableDef UnlockableDef => VoidUnlockableDef;

	public unsafe override void Init()
	{
		//IL_01e8: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f2: Expected O, but got Unknown
		VoidUnlockableDef = ScriptableObject.CreateInstance<UnlockableDef>();
		base.Init();
		string text = "Spikestrip" + CharacterName + "DefeatVoid";
		string achievementNameToken = "ACHIEVEMENT_" + text.ToUpper() + "_NAME";
		string achievementDescriptionToken = "ACHIEVEMENT_" + text.ToUpper() + "_DESCRIPTION";
		VoidUnlockableDef.cachedName = "SpikestripSkills." + CharacterName + "." + SkillName;
		VoidUnlockableDef.achievementIcon = null;
		VoidUnlockableDef.nameToken = SkillDef.skillNameToken;
		VoidUnlockableDef.displayModelPrefab = SpikestripUtil.nullModel;
		VoidUnlockableDef.achievementIcon = SkillIcon;
		VoidUnlockableDef.getHowToUnlockString = () => Language.GetStringFormatted("UNLOCK_VIA_ACHIEVEMENT_FORMAT", Language.GetString(achievementNameToken), Language.GetString(achievementDescriptionToken));
		VoidUnlockableDef.getUnlockedString = () => Language.GetStringFormatted("UNLOCKED_FORMAT", Language.GetString(achievementNameToken), Language.GetString(achievementDescriptionToken));
		ContentAddition.AddUnlockableDef(VoidUnlockableDef);
		VoidAchievementDef = new AchievementDef
		{
			identifier = text,
			unlockableRewardIdentifier = VoidUnlockableDef.cachedName,
			prerequisiteAchievementIdentifier = PrerequisiteIdentifier,
			nameToken = achievementNameToken,
			descriptionToken = achievementDescriptionToken,
			type = typeof(SpecificVoidEndingAchievement<T>),
			serverTrackerType = typeof(SpecificVoidEndingAchievement<T>.SpecificCompleteWave50ServerAchievement),
			achievedIcon = SkillIcon
		};
		UnlockableAPI.AddAchievement(VoidAchievementDef);
		LanguageAPI.Add(achievementNameToken, CharacterName + ": " + AchievementName);
		LanguageAPI.Add(achievementDescriptionToken, $"As {CharacterName}, escape the Planetarium or complete wave 50 in Simulacrum.");
		On.RoR2.UI.CharacterSelectController.RebuildLocal += CharacterSelectController_RebuildLocal;
		On.RoR2.UI.SkillIcon.LateUpdate += SkillIcon_Update;
		On.RoR2.UI.LoadoutPanelController.Row.AddButton += Row_AddButton;
	}

	private void CharacterSelectController_RebuildLocal(On.RoR2.UI.CharacterSelectController.orig_RebuildLocal orig, RoR2.UI.CharacterSelectController self, bool deselectButtons)
	{
		orig(self, deselectButtons);
		for (int i = 0; i < self.skillStripAllocator.elements.Count; i++)
		{
			RectTransform rectTransform = self.skillStripAllocator.elements[i];
			if ((bool)rectTransform)
			{
				RoR2.UI.HGTextMeshProUGUI component = rectTransform.Find("Inner/SkillDescriptionPanel/SkillName").GetComponent<RoR2.UI.HGTextMeshProUGUI>();
				RoR2.UI.HGTextMeshProUGUI component2 = rectTransform.Find("Inner/SkillDescriptionPanel/SkillDescription").GetComponent<RoR2.UI.HGTextMeshProUGUI>();
				if (component.text == Language.GetString(SkillDef.skillNameToken) && component2.text == Language.GetString(SkillDef.skillDescriptionToken))
				{
					component.color = voidSkillColor;
				}
			}
		}
	}

	private void SkillIcon_Update(On.RoR2.UI.SkillIcon.orig_LateUpdate orig, RoR2.UI.SkillIcon self)
	{
		orig(self);
		if ((bool)self.targetSkill && self.targetSkill.skillDef == SkillDef && (bool)self.tooltipProvider)
		{
			Color.RGBToHSV(voidSkillColor, out var H, out var S, out var V);
			V = ((V > 0.7f) ? 0.7f : V);
			Color titleColor = Color.HSVToRGB(H, S, V);
			self.tooltipProvider.titleColor = titleColor;
		}
	}

	private void Row_AddButton(On.RoR2.UI.LoadoutPanelController.Row.orig_AddButton orig, RoR2.UI.LoadoutPanelController.Row self, RoR2.UI.LoadoutPanelController owner, Sprite icon, string titleToken, string bodyToken, Color tooltipColor, UnityAction callback, string unlockableName, ViewablesCatalog.Node viewableNode, bool isWIP, int defIndex)
	{
		if (titleToken == SkillDef.skillNameToken)
		{
			tooltipColor = voidSkillColor;
		}
		orig(self, owner, icon, titleToken, bodyToken, tooltipColor, callback, unlockableName, viewableNode, isWIP, defIndex);
	}
}
