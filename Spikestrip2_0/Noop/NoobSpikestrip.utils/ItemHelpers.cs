using R2API;
using RoR2;

namespace NoobSpikestrip.utils;

internal class ItemHelpers
{
	public static void AddMaxShieldHelper(CharacterBody sender, RecalculateStatsAPI.StatHookEventArgs args, int inventoryCount, float baseStackHPPercent, float addStackHPPercent)
	{
		if (inventoryCount > 0)
		{
			if (sender.inventory.GetItemCountEffective(RoR2Content.Items.ShieldOnly) > 0)
			{
				float num = 1f + (float)sender.inventory.GetItemCountEffective(RoR2Content.Items.BoostHp) * 0.1f + (float)(sender.inventory.GetItemCountEffective(RoR2Content.Items.Pearl) + sender.inventory.GetItemCountEffective(RoR2Content.Items.ShinyPearl)) * 0.1f;
				float num2 = (sender.baseMaxHealth + sender.levelMaxHealth * (sender.level - 1f) + (float)sender.inventory.GetItemCountEffective(RoR2Content.Items.Knurl) * 40f + (float)((sender.inventory.GetItemCountEffective(RoR2Content.Items.Infusion) > 0) ? sender.inventory.infusionBonus : 0u)) * num / ((float)sender.inventory.GetItemCountEffective(RoR2Content.Items.CutHp) + 1f) * (float)((sender.inventory.GetItemCountEffective(RoR2Content.Items.InvadingDoppelganger) <= 0) ? 1 : 10);
				args.baseShieldAdd += num2 * baseStackHPPercent + num2 * addStackHPPercent * (float)(inventoryCount - 1);
			}
			else
			{
				args.baseShieldAdd += sender.maxHealth * baseStackHPPercent + sender.maxHealth * addStackHPPercent * (float)(inventoryCount - 1);
			}
		}
	}
}
