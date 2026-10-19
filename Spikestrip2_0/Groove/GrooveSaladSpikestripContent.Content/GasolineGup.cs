using R2API;
using RoR2;
using UnityEngine;
using Spikestrip2_0.Core;

namespace GrooveSaladSpikestripContent.Content;

public class GasolineGup : SpikestripGasolineBase<GasolineGup>
{
	public class FadeOutGoo : MonoBehaviour
	{
		private CharacterBody body;

		public void Start()
		{
			body = GetComponent<CharacterBody>();
		}

		public void FixedUpdate()
		{
			if ((bool)body && body.HasBuff(gooBuffDef))
			{
				return;
			}
			Transform transform = ((bool)body && (bool)body.modelLocator) ? body.modelLocator.modelTransform : null;
			CharacterModel component = transform ? transform.GetComponent<CharacterModel>() : null;
			if ((bool)component)
			{
				TemporaryOverlayInstance temporaryOverlay = TemporaryOverlayManager.AddOverlay(base.gameObject);
				temporaryOverlay.duration = 0.6f;
				temporaryOverlay.animateShaderAlpha = true;
				temporaryOverlay.alphaCurve = AnimationCurve.EaseInOut(0f, 1f, 1f, 0f);
				temporaryOverlay.destroyComponentOnEnd = true;
				temporaryOverlay.originalMaterial = overlayMaterial;
				temporaryOverlay.AddToCharacterModel(component);
			}
			Object.Destroy(this);
		}
	}

	public static BuffDef gooBuffDef;

	public static Material overlayMaterial;

	public override string ItemToken => base.ItemToken + "GUP";

	public override string ItemName => "Coagulated Mixture";

	public override string ItemPickup => "Killing an enemy slows down other nearby enemies.";

	public override string ItemDescription => "Killing an enemy slows all enemies within <style=cIsDamage>9m</style> <style=cStack>(+3m per stack)</style> for <style=cIsDamage>3s</style> <style=cStack>(+1.5s per stack)</style>, reducing <style=cIsDamage>movement speed</style> by <style=cIsDamage>50%</style>, <style=cIsDamage>armor</style> by <style=cIsDamage>20</style>, and increasing <style=cIsDamage>cooldowns</style>.";

	public override string ItemLore => null;

	public override ItemTier Tier => ItemTier.Tier1;

	public override ItemTag[] ItemTags => new ItemTag[3]
	{
		ItemTag.WorldUnique,
		ItemTag.Utility,
		ItemTag.OnKillEffect
	};

	public override Sprite ItemIconSprite => SpikestripContent.GrooveBundle.LoadAsset<Sprite>("texGasolineGupIcon.png");

	public override float RadiusBase => 9f;

	public override float RadiusStack => 3f;

	public override void Init()
	{
		//IL_00ed: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f7: Expected O, but got Unknown
		base.Init();
		gooBuffDef = ScriptableObject.CreateInstance<BuffDef>();
		gooBuffDef.name = "bdGasolineGup";
		gooBuffDef.buffColor = SpikestripUtil.ColorRGB(219f, 119f, 61f);
		gooBuffDef.canStack = false;
		gooBuffDef.iconSprite = SpikestripContent.GrooveBundle.LoadAsset<Sprite>("texGupBuffIcon.png");
		gooBuffDef.isDebuff = true;
		ContentAddition.AddBuffDef(gooBuffDef);
		overlayMaterial = Object.Instantiate(LegacyResourcesAPI.Load<Material>("materials/matClayGooDebuff"));
		overlayMaterial.SetTexture("_RemapTex", SpikestripContent.GrooveBundle.LoadAsset<Texture>("texGooRamp"));
		overlayMaterial.SetFloat("_ExternalAlpha", 0.4f);
		OverlayMaterials.Register(overlayMaterial, (CharacterModel model) => model.body.HasBuff(gooBuffDef));
		RecalculateStatsAPI.GetStatCoefficients += new RecalculateStatsAPI.StatHookEventHandler(RecalculateStatsAPI_GetStatCoefficients);
	}

	private void RecalculateStatsAPI_GetStatCoefficients(CharacterBody sender, RecalculateStatsAPI.StatHookEventArgs args)
	{
		if (sender.HasBuff(gooBuffDef))
		{
			args.moveSpeedReductionMultAdd += 0.5f;
			args.armorAdd -= 20f;
			args.cooldownReductionAdd -= 2f;
		}
	}

	public override void OnProcServer(HurtBox hurtBox, Vector3 procOrigin, int stack)
	{
		if ((bool)hurtBox.healthComponent)
		{
			float duration = SpikestripUtil.StackScaling(3f, 1.5f, stack);
			if ((bool)hurtBox.healthComponent.body)
			{
				hurtBox.healthComponent.body.AddTimedBuff(gooBuffDef, duration);
				if (!hurtBox.healthComponent.body.GetComponent<FadeOutGoo>())
				{
					hurtBox.healthComponent.body.gameObject.AddComponent<FadeOutGoo>();
				}
			}
		}
	}

	public override void AssignEffectPrefab()
	{
		GameObject gameObject = LegacyResourcesAPI.Load<GameObject>("prefabs/effects/muzzleflashes/GrandParentSunSpawn");
		Material material = Object.Instantiate(gameObject.transform.Find("Particles/Goo, Billboard Huge").GetComponent<Renderer>().material);
		material.SetTexture("_RemapTex", SpikestripContent.GrooveBundle.LoadAsset<Texture>("texGooRampTransparent"));
		material.SetFloat("_ExternalAlpha", 0.5f);
		GameObject gameObject2 = LegacyResourcesAPI.Load<GameObject>("prefabs/effects/ClayBruiserSpawnEffect");
		Material material2 = gameObject2.transform.Find("Fizz").GetComponent<Renderer>().material;
		GasolineEffectPrefab = PrefabAPI.InstantiateClone(SpikestripGasolineBase<GasolineGup>.igniteExplosionVFX, "gooExplosionVFX", false);
		ParticleSystemRenderer component = GasolineEffectPrefab.GetComponent<ParticleSystemRenderer>();
		component.material = material;
		ParticleSystem.MainModule main = GasolineEffectPrefab.GetComponent<ParticleSystem>().main;
		main.simulationSpeed = 2f;
		GasolineEffectPrefab.GetComponent<EffectComponent>().soundName = "Play_clayboss_m2_shoot";
		Light componentInChildren = GasolineEffectPrefab.GetComponentInChildren<Light>();
		componentInChildren.color = SpikestripUtil.ColorRGB(226f, 113f, 47f);
		ParticleSystemRenderer component2 = GasolineEffectPrefab.transform.Find("OmniDirectionals").GetComponent<ParticleSystemRenderer>();
		Material material3 = Object.Instantiate(material2);
		material3.SetTexture("_RemapTex", SpikestripContent.GrooveBundle.LoadAsset<Texture>("texGooRampTransparent"));
		component2.material = material3;
		ContentAddition.AddEffect(GasolineEffectPrefab);
	}

	public override void AssignItemModels()
	{
		ItemPickupModel = CreateGasolineModel(SpikestripUtil.ColorRGB(229f, 110f, 41f));
	}
}
