using System;
using GrooveSaladSpikestripContent;
using R2API;
using RoR2;
using UnityEngine;
using Spikestrip2_0.Core;

namespace NoopSpikestrip;

internal class VoidItemConsumed : ItemBase<VoidItemConsumed>
{
	public override string ItemName => "Spent Cartridge";

	public override Type DependsOn => typeof(Voiditem);

	public override string ItemToken => "CARTRIDGECONSUMED";

	public override string ItemPickup => "He wanted to be an artist...";

	public override string ItemDescription => "He wanted to be an artist...";

	public override string ItemLore => null;

	public override ItemTier Tier => ItemTier.NoTier;

	public override ItemTag[] ItemTags => new ItemTag[2]
	{
		ItemTag.CannotCopy,
		ItemTag.CannotSteal
	};

	public override Sprite ItemIconSprite => SpikestripContent.NoopBundle.LoadAsset<Sprite>("CartridgeConsumed.png");

	public override bool CanRemove => false;

	public void GenerateMethod()
	{
	}

	public override void AssignItemModels()
	{
	}

	public override void PopulateItemDisplayRules(ItemDisplayRuleDict itemDisplayRuleDict)
	{
	}
}
