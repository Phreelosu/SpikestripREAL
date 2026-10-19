using System;
using System.Collections.Generic;
using GrooveSaladSpikestripContent;
using On.RoR2.UI.LogBook;
using R2API;
using RoR2;
using RoR2.ExpansionManagement;
using UnityEngine;
using Spikestrip2_0.Core;

namespace NoopSpikestrip;

internal class BLESSING : ItemBase<BLESSING>
{
	public override string ItemName => "Blessing";

	public override string ItemToken => "BLESSING_NAME";

	public override string ItemPickup => "Stare into the abyss for long enough, and soon it starts to stare back...";

	public override string ItemDescription => "Stare into the abyss for long enough, and soon it starts to stare back...";

	public override string ItemLore => null;

	public override Type DependsOn => typeof(Voiditem);

	public override ItemTier Tier => ItemTier.VoidTier1;

	public override ItemTag[] ItemTags => new ItemTag[3]
	{
		ItemTag.CannotCopy,
		ItemTag.CannotSteal,
		ItemTag.WorldUnique
	};

	public override Sprite ItemIconSprite => SpikestripContent.NoopBundle.LoadAsset<Sprite>("texBlessingIcon.png");

	public override bool CanRemove => false;

	public override void Init()
	{
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Expected O, but got Unknown
		base.Init();
		RecalculateStatsAPI.GetStatCoefficients += new RecalculateStatsAPI.StatHookEventHandler(RecalculateStatsAPI_GetStatCoefficients);
		LogBookController.CanSelectItemEntry += LogBookController_CanSelectItemEntry;
	}

	private bool LogBookController_CanSelectItemEntry(LogBookController.orig_CanSelectItemEntry orig, ItemDef itemDef, Dictionary<ExpansionDef, bool> expansionAvailability)
	{
		return !(itemDef == ItemDef) && orig(itemDef, expansionAvailability);
	}

	internal void RecalculateStatsAPI_GetStatCoefficients(CharacterBody sender, RecalculateStatsAPI.StatHookEventArgs args)
	{
		int itemCount = sender.inventory ? sender.inventory.GetItemCountEffective(ItemDef) : 0;
		if (itemCount > 0)
		{
			float bonus = 0.03f * itemCount;
			args.healthMultAdd += bonus;
			args.moveSpeedMultAdd += bonus;
			args.damageMultAdd += bonus;
			args.attackSpeedMultAdd += bonus;
			args.critAdd += bonus * 100f;
			args.baseRegenAdd += bonus * sender.level;
			args.armorAdd += (sender.baseArmor + (sender.level - 1f) * sender.levelArmor) * bonus;
		}
	}

	public override void AssignItemModels()
	{
	}

	public override void PopulateItemDisplayRules(ItemDisplayRuleDict itemDisplayRuleDict)
	{
	}
}
