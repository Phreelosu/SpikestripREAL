using R2API;
using RoR2;
using UnityEngine;
using Spikestrip2_0.Core;

namespace GrooveSaladSpikestripContent.Content;

public class GasolinePressure : SpikestripGasolineBase<GasolinePressure>
{
	public override string ItemToken => base.ItemToken + "PRESSURE";

	public override string ItemName => "Pressurized Fluid";

	public override string ItemPickup => "Chance on killing an enemy to pull together other nearby enemies.";

	public override string ItemDescription => "<style=cIsUtility>50% chance</style> on killing an enemy to pull together all enemies within <style=cIsUtility>8m</style> <style=cStack>(+4m per stack)</style>.";

	public override string ItemLore => null;

	public override ItemTier Tier => ItemTier.Tier1;

	public override ItemTag[] ItemTags => new ItemTag[3]
	{
		ItemTag.WorldUnique,
		ItemTag.Utility,
		ItemTag.OnKillEffect
	};

	public override Sprite ItemIconSprite => SpikestripContent.GrooveBundle.LoadAsset<Sprite>("texGasolinePressureIcon.png");

	public override float ProcChanceBase => 50f;

	public override float RadiusBase => 8f;

	public override float RadiusStack => 4f;

	public override void Init()
	{
		base.Init();
	}

	public override void OnProcServer(HurtBox hurtBox, Vector3 procOrigin, int stack)
	{
		if ((bool)hurtBox.healthComponent && (bool)hurtBox.healthComponent.body)
		{
			Vector3 vector = hurtBox.transform.position - procOrigin;
			CharacterBody body = hurtBox.healthComponent.body;
			float num = 0f;
			if ((bool)(Object)(object)body.characterMotor)
			{
				num = body.characterMotor.mass;
			}
			else if ((bool)body.rigidbody)
			{
				num = body.rigidbody.mass;
			}
			if (num > 0f)
			{
				hurtBox.healthComponent.TakeDamageForce(-vector.normalized * 30f * num, alwaysApply: true);
			}
		}
	}

	public override void AssignEffectPrefab()
	{
		Texture value = SpikestripUtil.AddressablesLoad<Texture>("aaa1e96a308d0114d8b33961d925fbcf");
		Material original = SpikestripUtil.AddressablesLoad<Material>("a37f2d5c24c83c34a9dc89f5479a294b");
		Material material = Object.Instantiate(original);
		material.SetTexture("_RemapTex", value);
		material.SetColor("_TintColor", SpikestripUtil.ColorRGB(221f, 185f, 66f));
		GameObject gameObject = PrefabAPI.InstantiateClone(SpikestripUtil.AddressablesLoad<GameObject>("a96e011a250b79b4981af530b6e48433"), "PressureExplosionIndicator", false);
		gameObject.transform.Find("Expander/Sphere").GetComponent<MeshRenderer>().material = material;
		ObjectScaleCurve componentInChildren = gameObject.GetComponentInChildren<ObjectScaleCurve>();
		componentInChildren.useOverallCurveOnly = true;
		componentInChildren.overallCurve = AnimationCurve.EaseInOut(0f, 1f, 1f, 0f);
		componentInChildren.timeMax = 0.2f;
		GasolineEffectPrefab = PrefabAPI.InstantiateClone(SpikestripGasolineBase<GasolinePressure>.igniteExplosionVFX, "pressureExplosionVFX", false);
		UnityEngine.Object.DestroyImmediate(GasolineEffectPrefab.GetComponent<ParticleSystemRenderer>());
		UnityEngine.Object.DestroyImmediate(GasolineEffectPrefab.GetComponent<ParticleSystem>());
		UnityEngine.Object.DestroyImmediate(GasolineEffectPrefab.GetComponent<DestroyOnParticleEnd>());
		DestroyOnTimer destroyOnTimer = GasolineEffectPrefab.AddComponent<DestroyOnTimer>();
		destroyOnTimer.duration = 0.2f;
		gameObject.transform.SetParent(GasolineEffectPrefab.transform);
		gameObject.transform.localPosition = Vector3.zero;
		gameObject.transform.localScale = Vector3.one;
		GasolineEffectPrefab.GetComponent<EffectComponent>().soundName = "Play_mage_m2_iceSpear_shoot";
		Light componentInChildren2 = GasolineEffectPrefab.GetComponentInChildren<Light>();
		componentInChildren2.color = SpikestripUtil.ColorRGB(221f, 185f, 66f);
		ContentAddition.AddEffect(GasolineEffectPrefab);
	}

	public override void AssignItemModels()
	{
		ItemPickupModel = CreateGasolineModel(SpikestripUtil.ColorRGB(221f, 185f, 66f));
	}
}
