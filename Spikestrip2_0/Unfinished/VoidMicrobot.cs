using GrooveSaladSpikestripContent;
using R2API;
using RoR2;
using UnityEngine;

namespace NoobSpikestripContent.Core.Scrapped;

public class VoidMicrobot : SpikestripItemBase<VoidMicrobot>
{
	public override string ItemName => "fake";

	public override string ItemToken => "PARASITICBOT";

	public override string ItemPickup => "Latch onto a nearby enemy to drain them of life";

	public override string ItemDescription => "WIP";

	public override string ItemLore => "fake";

	public override ItemTier Tier => ItemTier.VoidTier3;

	public override Sprite ItemIconSprite => null;

	public override bool IsEnabled => false;

	public override void PopulateItemDisplayRules(ItemDisplayRuleDict itemDisplayRuleDict)
	{
	}

	public override void AssignItemModels()
	{
	}
}
