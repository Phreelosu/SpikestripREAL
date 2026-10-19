using R2API;
using RoR2;
using UnityEngine;
using Spikestrip2_0.Core;

namespace GrooveSaladSpikestripContent.Content;

public class GasolineFreeze : SpikestripGasolineBase<GasolineFreeze>
{
	public override string ItemToken => base.ItemToken + "FREEZE";

	public override string ItemName => "Chilled CO2";

	public override string ItemPickup => "Chance on killing an enemy to freeze other nearby enemies.";

	public override string ItemDescription => "<style=cIsUtility>50% chance</style> on killing an enemy to <style=cIsUtility>freeze</style> all enemies within <style=cIsUtility>12m</style> <style=cStack>(+4m per stack)</style> for <style=cIsUtility>1.5s</style> <style=cStack>(+0.5s per stack)</style>.";

	public override string ItemLore => null;

	public override ItemTier Tier => ItemTier.Tier1;

	public override ItemTag[] ItemTags => new ItemTag[3]
	{
		ItemTag.WorldUnique,
		ItemTag.Utility,
		ItemTag.OnKillEffect
	};

	public override Sprite ItemIconSprite => SpikestripContent.GrooveBundle.LoadAsset<Sprite>("texGasolineFreezeIcon.png");

	public override float ProcChanceBase => 50f;

	public override void Init()
	{
		base.Init();
	}

	public override void OnProcServer(HurtBox hurtBox, Vector3 procOrigin, int stack)
	{
		if ((bool)hurtBox.healthComponent)
		{
			float num = SpikestripUtil.StackScaling(1.5f, 0.5f, stack);
			SetStateOnHurt component = hurtBox.healthComponent.GetComponent<SetStateOnHurt>();
			if ((bool)component && component.canBeFrozen)
			{
				component.SetFrozen(num);
			}
			CharacterBody body = hurtBox.healthComponent.body;
			if ((bool)body)
			{
				body.AddTimedBuff(RoR2Content.Buffs.Slow80, num);
			}
		}
	}

	public override void AssignEffectPrefab()
	{
		GameObject gameObject = LegacyResourcesAPI.Load<GameObject>("prefabs/effects/AffixWhiteDelayEffect");
		Material material = gameObject.transform.Find("Nova Sphere").GetComponent<Renderer>().material;
		GameObject gameObject2 = LegacyResourcesAPI.Load<GameObject>("prefabs/effects/impacteffects/IceRingExplosion");
		Material material2 = gameObject2.transform.Find("BillboardSplash").GetComponent<Renderer>().material;
		GasolineEffectPrefab = PrefabAPI.InstantiateClone(SpikestripGasolineBase<GasolineFreeze>.igniteExplosionVFX, "freezeExplosionVFX", false);
		GasolineEffectPrefab.GetComponent<ParticleSystemRenderer>().material = material;
		GasolineEffectPrefab.GetComponent<EffectComponent>().soundName = "Play_mage_m2_iceSpear_shoot";
		Light componentInChildren = GasolineEffectPrefab.GetComponentInChildren<Light>();
		componentInChildren.color = SpikestripUtil.ColorRGB(162f, 190f, 232f);
		GasolineEffectPrefab.transform.Find("OmniDirectionals").GetComponent<ParticleSystemRenderer>().material = material2;
		GasolineEffectPrefab.transform.Find("Flames").GetComponent<ParticleSystemRenderer>().material = material2;
		ContentAddition.AddEffect(GasolineEffectPrefab);
	}

	public override void AssignItemModels()
	{
		ItemPickupModel = CreateGasolineModel(SpikestripUtil.ColorRGB(163f, 190f, 211f));
	}
}
