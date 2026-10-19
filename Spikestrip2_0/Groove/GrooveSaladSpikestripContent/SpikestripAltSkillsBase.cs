using System;
using System.Collections.Generic;
using HG;
using On.RoR2;
using On.RoR2.UI;
using R2API;
using RoR2;
using RoR2.EntitlementManagement;
using RoR2.Skills;
using RoR2.UI;
using UnityEngine;
using UnityEngine.EventSystems;
using Spikestrip2_0.Core;

namespace GrooveSaladSpikestripContent;

public abstract class SpikestripAltSkillsBase<T> : ContentBase<T> where T : SpikestripAltSkillsBase<T>
{
	public struct SkillResetInfo
	{
		public int skillSlotIndex;

		public BodyIndex bodyIndex;

		public uint defaultVariantIndex;
	}

	public List<SkillResetInfo> skillResetInfos = new List<SkillResetInfo>();

	public SkillDef SkillDef;

	public override string ConfigSection => "Alternate Skills";

	public override string ConfigName => SkillName;

	public abstract string SkillName { get; }

	public abstract string SkillDescription { get; }

	public abstract string SkillToken { get; }

	public abstract Sprite SkillIcon { get; }

	public abstract GameObject OwnerBodyPrefab { get; }

	public abstract SkillFamily[] SkillFamilies { get; }

	public abstract Type[] EntityStateTypes { get; }

	public virtual RoR2.UnlockableDef UnlockableDef { get; } = null;

	public override void Init()
	{
		SkillDef = GetSkillDef();
		SkillDef.skillName = "SPIKESTRIPSKILL_" + SkillToken;
		SkillDef.skillNameToken = "SPIKESTRIPSKILL_" + SkillToken + "_NAME";
		SkillDef.skillDescriptionToken = "SPIKESTRIPSKILL_" + SkillToken + "_DESCRIPTION";
		SkillDef.icon = SkillIcon;
		LanguageAPI.Add(SkillDef.skillNameToken, SkillName);
		LanguageAPI.Add(SkillDef.skillDescriptionToken, SkillDescription);
		ContentAddition.AddSkillDef(SkillDef);
		for (int i = 0; i < SkillFamilies.Length; i++)
		{
			SkillFamily skillFamily = SkillFamilies[i];
			SkillFamily.Variant value = new SkillFamily.Variant
			{
				skillDef = SkillDef,
				unlockableDef = UnlockableDef,
				viewableNode = new RoR2.ViewablesCatalog.Node(SkillDef.skillName, isFolder: false)
			};
			ArrayUtils.ArrayAppend(ref skillFamily.variants, in value);
		}
		for (int j = 0; j < EntityStateTypes.Length; j++)
		{
			ContentAddition.AddEntityState(EntityStateTypes[j], out _);
		}
		if (RequiredExpansionHolder != null)
		{
			On.RoR2.UnlockableCatalog.GenerateUnlockableMetaData += UnlockableCatalog_GenerateUnlockableMetaData;
			RoR2.PreGameController.onPreGameControllerSetRuleBookGlobal += PreGameController_onPreGameControllerSetRuleBookGlobal;
			RoR2.NetworkUser.onLoadoutChangedGlobal += NetworkUser_onLoadoutChangedGlobal;
			On.RoR2.Loadout.BodyLoadoutManager.BodyLoadout.IsSkillVariantLocked += BodyLoadout_IsSkillVariantLocked;
			On.RoR2.UI.LoadoutPanelController.Row.FromSkillSlot += Row_FromSkillSlot;
			RoR2.BodyCatalog.availability.CallWhenAvailable(OnBodyCatalogAvailable);
		}
	}

	private void UnlockableCatalog_GenerateUnlockableMetaData(On.RoR2.UnlockableCatalog.orig_GenerateUnlockableMetaData orig, RoR2.UnlockableDef[] unlockableDefs)
	{
		orig(unlockableDefs);
		RoR2.UnlockableCatalog.TryAddExpansionMapping(UnlockableDef, RequiredExpansionHolder);
	}

	private void NetworkUser_onLoadoutChangedGlobal(RoR2.NetworkUser networkUser)
	{
		RoR2.LocalUser localUser = (networkUser ? networkUser.localUser : null);
		if (localUser == null)
		{
			return;
		}
		bool flag = (RoR2.PreGameController.instance ? RoR2.PreGameController.instance.readOnlyRuleBook : (RoR2.Run.instance ? RoR2.Run.instance.ruleBook : null))?.IsChoiceActive(RequiredExpansionHolder.enabledChoice) ?? false;
		bool flag2 = !RequiredExpansionHolder.requiredEntitlement || EntitlementManager.localUserEntitlementTracker.UserHasEntitlement(localUser, RequiredExpansionHolder.requiredEntitlement);
		if (!flag || !flag2)
		{
			RoR2.UserProfile userProfile = localUser?.userProfile;
			if (userProfile != null)
			{
				EnforceExpansionResets(userProfile);
			}
		}
	}

	private void PreGameController_onPreGameControllerSetRuleBookGlobal(RoR2.PreGameController preGameController, RoR2.RuleBook ruleBook)
	{
		RoR2.LocalUser localUser = ((RoR2.UI.MPEventSystem)EventSystem.current).localUser;
		bool flag = ruleBook?.IsChoiceActive(RequiredExpansionHolder.enabledChoice) ?? false;
		bool flag2 = !RequiredExpansionHolder.requiredEntitlement || EntitlementManager.localUserEntitlementTracker.UserHasEntitlement(localUser, RequiredExpansionHolder.requiredEntitlement);
		if ((!flag || !flag2) && localUser != null && localUser.userProfile != null)
		{
			EnforceExpansionResets(localUser.userProfile);
		}
	}

	public void EnforceExpansionResets(RoR2.UserProfile userProfile)
	{
		RoR2.Loadout loadout = new RoR2.Loadout();
		userProfile.CopyLoadout(loadout);
		for (int i = 0; i < skillResetInfos.Count; i++)
		{
			SkillResetInfo skillResetInfo = skillResetInfos[i];
			uint skillVariant = loadout.bodyLoadoutManager.GetSkillVariant(skillResetInfo.bodyIndex, skillResetInfo.skillSlotIndex);
			SkillFamily skillFamily = RoR2.BodyCatalog.GetBodyPrefabSkillSlots(skillResetInfo.bodyIndex)[skillResetInfo.skillSlotIndex].skillFamily;
			if ((bool)skillFamily && skillFamily.variants[skillVariant].skillDef == SkillDef)
			{
				loadout.bodyLoadoutManager.SetSkillVariant(skillResetInfo.bodyIndex, skillResetInfo.skillSlotIndex, skillResetInfo.defaultVariantIndex);
			}
		}
		userProfile.SetLoadout(loadout);
	}

	private void OnBodyCatalogAvailable()
	{
		BodyIndex bodyIndex = RoR2.BodyCatalog.FindBodyIndex(OwnerBodyPrefab);
		if (bodyIndex == BodyIndex.None)
		{
			return;
		}
		RoR2.GenericSkill[] components = OwnerBodyPrefab.GetComponents<RoR2.GenericSkill>();
		for (int i = 0; i < components.Length; i++)
		{
			RoR2.GenericSkill genericSkill = components[i];
			for (int j = 0; j < SkillFamilies.Length; j++)
			{
				SkillFamily skillFamily = SkillFamilies[j];
				if (genericSkill.skillFamily == skillFamily)
				{
					skillResetInfos.Add(new SkillResetInfo
					{
						bodyIndex = bodyIndex,
						skillSlotIndex = i,
						defaultVariantIndex = skillFamily.defaultVariantIndex
					});
				}
			}
		}
	}

	private RoR2.UI.LoadoutPanelController.Row Row_FromSkillSlot(On.RoR2.UI.LoadoutPanelController.Row.orig_FromSkillSlot orig, RoR2.UI.LoadoutPanelController owner, BodyIndex bodyIndex, int skillSlotIndex, RoR2.GenericSkill skillSlot)
	{
		RoR2.UI.LoadoutPanelController.Row obj = orig(owner, bodyIndex, skillSlotIndex, skillSlot);
		SkillFamily skillFamily = skillSlot.skillFamily;
		RoR2.UI.LoadoutPanelController.Row row = obj;
		if (row != null)
		{
			Transform buttonContainer = row.buttonContainerTransform;
			for (int i = 0; i < skillFamily.variants.Length && i < buttonContainer.childCount; i++)
			{
				if (skillFamily.variants[i].skillDef == SkillDef)
				{
					RoR2.UI.HGButton hGButton = buttonContainer.GetChild(i).GetComponent<RoR2.UI.HGButton>();
					if ((bool)hGButton)
					{
						LockSkillFromExpansion lockSkillFromExpansion = hGButton.gameObject.AddComponent<LockSkillFromExpansion>();
						lockSkillFromExpansion.requiredExpansion = RequiredExpansionHolder;
					}
				}
			}
		}
		return obj;
	}

	private bool BodyLoadout_IsSkillVariantLocked(On.RoR2.Loadout.BodyLoadoutManager.BodyLoadout.orig_IsSkillVariantLocked orig, RoR2.Loadout.BodyLoadoutManager.BodyLoadout self, int skillSlotIndex, RoR2.UserProfile userProfile)
	{
		RoR2.Loadout.BodyLoadoutManager.BodyLoadout bodyLoadout = self;
		SkillFamily skillFamily = bodyLoadout.GetSkillFamily(skillSlotIndex);
		if ((bool)skillFamily)
		{
			uint skillVariant = bodyLoadout.GetSkillVariant(skillSlotIndex);
			if ((bool)SkillDef && skillFamily.variants[skillVariant].skillDef == SkillDef)
			{
				bool flag = (RoR2.PreGameController.instance ? RoR2.PreGameController.instance.readOnlyRuleBook : (RoR2.Run.instance ? RoR2.Run.instance.ruleBook : null))?.IsChoiceActive(RequiredExpansionHolder.enabledChoice) ?? false;
				RoR2.LocalUser firstLocalUser = RoR2.LocalUserManager.GetFirstLocalUser();
				bool flag2 = !RequiredExpansionHolder.requiredEntitlement || EntitlementManager.localUserEntitlementTracker.UserHasEntitlement(firstLocalUser, RequiredExpansionHolder.requiredEntitlement);
				return orig(self, skillSlotIndex, userProfile) && flag && flag2;
			}
		}
		return orig(self, skillSlotIndex, userProfile);
	}

	public abstract SkillDef GetSkillDef();
}
