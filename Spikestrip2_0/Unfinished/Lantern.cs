using System;
using System.Collections.Generic;
using GrooveSaladSpikestripContent;
using NoopSpikestripContent.Core;
using On.RoR2;
using R2API;
using RoR2;
using UnityEngine;

namespace NoopSpikestrip;

internal class Lantern : SpikestripItemBase<Lantern>
{
	public class LanternBehavior : RoR2.CharacterBody.ItemBehavior
	{
		public List<RoR2.CharacterBody> Buddies = new List<RoR2.CharacterBody>();
	}

	public RoR2.BuffDef lanternIsWoolie;

	public override string ItemName => "Lantern Of the Damned";

	public override string ItemToken => "LANTERN";

	public override string ItemPickup => "Attacks have a small chance to chain enemies together";

	public override string ItemDescription => "Attacks have a small chance to cause chains to lash out at a nearby enemy, linking them together. Linked enemies share status effects, knockback, and % of damage dealt";

	public override string ItemLore => null;

	public override bool IsEnabled => false;

	public override ItemTier Tier => ItemTier.Tier3;

	public override Sprite ItemIconSprite => null;

	public override Type ItemBehaviour => typeof(LanternBehavior);

	public override bool ServerOnlyBehaviour => true;

	public override void AssignItemModels()
	{
		GameObject originalModel = NoobBase.NoobAssets.LoadAsset<GameObject>("BlueStar.prefab");
		ItemPickupModel = SpikestripContentBase.UpdateGameObjectShader(originalModel);
	}

	public override void PopulateItemDisplayRules(ItemDisplayRuleDict itemDisplayRuleDict)
	{
	}

	public override void Init()
	{
		base.Init();
		lanternIsWoolie = ScriptableObject.CreateInstance<RoR2.BuffDef>();
		lanternIsWoolie.buffColor = SpikestripContentBase.ColorRGB(2f, 2f, 2f);
		lanternIsWoolie.iconSprite = null;
		lanternIsWoolie.isDebuff = true;
		SpikestripContentBase.buffDefContent.Add(lanternIsWoolie);
		On.RoR2.GlobalEventManager.OnHitEnemy += GlobalEventManager_OnHitEnemy;
	}

	private void GlobalEventManager_OnHitEnemy(On.RoR2.GlobalEventManager.orig_OnHitEnemy orig, RoR2.GlobalEventManager self, RoR2.DamageInfo damageInfo, GameObject victim)
	{
		orig(self, damageInfo, victim);
		RoR2.CharacterBody component = damageInfo.attacker.GetComponent<RoR2.CharacterBody>();
		LanternBehavior component2 = damageInfo.attacker.GetComponent<LanternBehavior>();
		RoR2.CharacterBody component3 = victim.GetComponent<RoR2.CharacterBody>();
		if (!component)
		{
			return;
		}
		if (RoR2.Util.CheckRoll(10f, component.master))
		{
			component3.AddBuff(lanternIsWoolie);
			RoR2.BullseyeSearch bullseyeSearch = new RoR2.BullseyeSearch
			{
				teamMaskFilter = RoR2.TeamMask.AllExcept(TeamIndex.Player),
				filterByLoS = true,
				maxDistanceFilter = 20f,
				searchOrigin = component3.corePosition
			};
			List<RoR2.HurtBox> list = (List<RoR2.HurtBox>)bullseyeSearch.GetResults();
			int index = UnityEngine.Random.Range(0, list.Count - 1);
			RoR2.CharacterBody body = list[index].healthComponent.body;
			if ((bool)body)
			{
				body.AddBuff(lanternIsWoolie);
				component2.Buddies.Add(body);
			}
		}
		if (!component3.HasBuff(lanternIsWoolie))
		{
			return;
		}
		float damage = damageInfo.damage * 0.5f;
		RoR2.DamageInfo damageInfo2 = new RoR2.DamageInfo();
		damageInfo2.damage = damage;
		foreach (RoR2.CharacterBody buddy in component2.Buddies)
		{
			buddy.healthComponent.TakeDamage(damageInfo2);
			buddy.healthComponent.TakeDamageForce(damageInfo, alwaysApply: false, disableAirControlUntilCollision: true);
		}
	}
}
