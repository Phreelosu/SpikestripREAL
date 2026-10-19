using R2API;
using RoR2;
using UnityEngine;
using Spikestrip2_0.Core;

namespace GrooveSaladSpikestripContent.Content;

public class GasolineHeal : SpikestripGasolineBase<GasolineHeal>
{
	public override string ItemToken => base.ItemToken + "HEAL";

	public override string ItemName => "Essential Oils";

	public override string ItemPickup => "Killing an enemy regenerates nearby allies.";

	public override string ItemDescription => "Killing an enemy regenerates allies within <style=cIsHealing>12m</style> <style=cStack>(+4m per stack)</style> for <style=cIsHealing>5%</style> <style=cStack>(+2.5% per stack)</style> of <style=cIsHealing>maximum health</style>.";

	public override string ItemLore => null;

	public override ItemTier Tier => ItemTier.Tier1;

	public override ItemTag[] ItemTags => new ItemTag[3]
	{
		ItemTag.WorldUnique,
		ItemTag.Healing,
		ItemTag.OnKillEffect
	};

	public override Sprite ItemIconSprite => SpikestripContent.GrooveBundle.LoadAsset<Sprite>("texGasolineHealIcon.png");

	public override float ExplosionDamageBase => 0f;

	public override float ExplosionDamageStack => 0f;

	public override bool InvertTeamFilter => true;

	public override void Init()
	{
		base.Init();
	}

	public override void OnProcServer(HurtBox hurtBox, Vector3 procOrigin, int stack)
	{
		if ((bool)hurtBox.healthComponent && (bool)hurtBox.healthComponent.body)
		{
			hurtBox.healthComponent.body.AddTimedBuff(RoR2Content.Buffs.CrocoRegen, SpikestripUtil.StackScaling(0.5f, 0.25f, stack));
		}
	}

	public override void AssignEffectPrefab()
	{
		GameObject gameObject = LegacyResourcesAPI.Load<GameObject>("prefabs/networkedobjects/TeleporterHealNovaPulse").transform.Find("PulseEffect").gameObject;
		Material material = gameObject.transform.Find("Sphere").GetComponent<Renderer>().material;
		Material material2 = gameObject.transform.Find("Crosses").GetComponent<Renderer>().material;
		GameObject gameObject2 = LegacyResourcesAPI.Load<GameObject>("prefabs/effects/impacteffects/HealthOrbFlash");
		Material material3 = gameObject2.transform.Find("Droplets").GetComponent<Renderer>().material;
		GasolineEffectPrefab = PrefabAPI.InstantiateClone(SpikestripGasolineBase<GasolineHeal>.igniteExplosionVFX, "healExplosionVFX", false);
		GasolineEffectPrefab.GetComponent<ParticleSystemRenderer>().material = material;
		GasolineEffectPrefab.GetComponent<EffectComponent>().soundName = "Play_siphonOnLowHealth_retract";
		Light componentInChildren = GasolineEffectPrefab.GetComponentInChildren<Light>();
		componentInChildren.color = SpikestripUtil.ColorRGB(153f, 239f, 131f);
		GasolineEffectPrefab.transform.Find("OmniDirectionals").GetComponent<ParticleSystemRenderer>().material = material3;
		Transform transform = GasolineEffectPrefab.transform.Find("Flames");
		ParticleSystemRenderer component = transform.GetComponent<ParticleSystemRenderer>();
		component.material = material2;
		component.maxParticleSize = 0.03f;
		transform.transform.localScale = Vector3.one * 4f;
		ContentAddition.AddEffect(GasolineEffectPrefab);
	}

	public override void AssignItemModels()
	{
		ItemPickupModel = CreateGasolineModel(SpikestripUtil.ColorRGB(99f, 176f, 79f));
	}
}
