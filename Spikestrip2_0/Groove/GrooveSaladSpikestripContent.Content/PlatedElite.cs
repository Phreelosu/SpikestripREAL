using System;
using System.Collections.Generic;
using On.RoR2;
using On.RoR2.UI;
using R2API;
using R2API.Networking;
using R2API.Networking.Interfaces;
using RoR2;
using RoR2.UI;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.Rendering.PostProcessing;
using UnityEngine.UI;
using Spikestrip2_0.Core;

namespace GrooveSaladSpikestripContent.Content;

public class PlatedElite : EliteBase<PlatedElite>
{
	public class PlatedAffixBuffBehaviour : RoR2.CharacterBody.ItemBehavior, IOnIncomingDamageServerReceiver, IOnTakeDamageServerReceiver
	{
		public List<Image> platingImages = new List<Image>();

		private float lastRecordedHealthServer;

		private float lastRecordedShieldServer;

		private int lastRecordedPlatingCountServer;

		public int remainingPlatingCountSynced;

		public int maxPlatingCount => Mathf.Min(Math.Max(1, (int)(body.bestFitRadius * 3f)), 12);

		public float platingDefenseFraction => 1f / (float)maxPlatingCount;

		public float combinedHealthFraction => body.healthComponent ? ((body.healthComponent.shield + body.healthComponent.health) / body.healthComponent.fullCombinedHealth) : 0f;

		public int currentPlatingCount => Mathf.Min(Mathf.CeilToInt((float)maxPlatingCount * combinedHealthFraction), remainingPlatingCountSynced);

		public void Start()
		{
			if (NetworkServer.active)
			{
				SetRemainingPlatingCount(maxPlatingCount);
			}
			if ((bool)body.healthComponent)
			{
				ref IOnIncomingDamageServerReceiver[] onIncomingDamageReceivers = ref body.healthComponent.onIncomingDamageReceivers;
				IOnIncomingDamageServerReceiver value = this;
				SpikestripUtil.AppendArrayIfMissing(ref onIncomingDamageReceivers, in value);
				ref IOnTakeDamageServerReceiver[] onTakeDamageReceivers = ref body.healthComponent.onTakeDamageReceivers;
				IOnTakeDamageServerReceiver value2 = this;
				SpikestripUtil.AppendArrayIfMissing(ref onTakeDamageReceivers, in value2);
			}
		}

		public void OnDestroy()
		{
			if ((bool)body && (bool)body.healthComponent)
			{
				ref IOnIncomingDamageServerReceiver[] onIncomingDamageReceivers = ref body.healthComponent.onIncomingDamageReceivers;
				IOnIncomingDamageServerReceiver value = this;
				SpikestripUtil.RemoveFromArray(ref onIncomingDamageReceivers, in value);
				ref IOnTakeDamageServerReceiver[] onTakeDamageReceivers = ref body.healthComponent.onTakeDamageReceivers;
				IOnTakeDamageServerReceiver value2 = this;
				SpikestripUtil.RemoveFromArray(ref onTakeDamageReceivers, in value2);
			}
			for (int num = platingImages.Count - 1; num >= 0; num--)
			{
				if ((bool)platingImages[num])
				{
					UnityEngine.Object.Destroy(platingImages[num].gameObject);
				}
				platingImages.RemoveAt(num);
			}
		}

		public void OnIncomingDamageServer(RoR2.DamageInfo damageInfo)
		{
			lastRecordedHealthServer = body.healthComponent.health;
			lastRecordedShieldServer = body.healthComponent.shield;
			lastRecordedPlatingCountServer = currentPlatingCount;
		}

		public void OnTakeDamageServer(RoR2.DamageReport damageReport)
		{
			if (currentPlatingCount < lastRecordedPlatingCountServer && currentPlatingCount != 0)
			{
				float num = ((float)(lastRecordedPlatingCountServer - 1) * platingDefenseFraction - combinedHealthFraction) * body.healthComponent.fullCombinedHealth;
				float a = lastRecordedHealthServer - body.healthComponent.Networkhealth;
				float a2 = lastRecordedShieldServer - body.healthComponent.Networkshield;
				damageReport.damageDealt -= num;
				float num2 = Math.Max(Mathf.Min(a, num), 0f);
				num -= num2;
				body.healthComponent.Networkhealth += num2;
				if (num > 0f)
				{
					float num3 = Math.Max(Mathf.Min(a2, num), 0f);
					body.healthComponent.Networkshield += num3;
				}
				body.healthComponent.ospTimer = 0.5f;
				SetRemainingPlatingCount(remainingPlatingCountSynced - 1);
				RoR2.DamageInfo damageInfo = damageReport.damageInfo;
				RoR2.EffectData effectData = new RoR2.EffectData
				{
					origin = damageInfo.position,
					rotation = RoR2.Util.QuaternionSafeLookRotation((damageInfo.force != Vector3.zero) ? damageInfo.force : UnityEngine.Random.onUnitSphere)
				};
				RoR2.EffectManager.SpawnEffect(platedBlockEffectPrefab, effectData, transmit: true);
			}
		}

		public void UpdateHealthbarPlates(RoR2.UI.HealthBar healthBar)
		{
			if (!healthBar.source)
			{
				return;
			}
			AllocateImages(healthBar, currentPlatingCount);
			for (int i = 0; i < platingImages.Count; i++)
			{
				if ((bool)platingImages[i])
				{
					Image image = platingImages[i];
					image.type = healthBar.barInfoCollection.cullBarInfo.imageType;
					image.sprite = healthBar.barInfoCollection.cullBarInfo.sprite;
					image.color = ((i % 2 == 0) ? healthBarColorTan : healthBarColorGray);
					float num = (float)i * platingDefenseFraction;
					float xMax = Mathf.Min(num + platingDefenseFraction, combinedHealthFraction);
					SetRectPosition((RectTransform)image.transform, num, xMax, 1f);
				}
			}
		}

		public void AllocateImages(RoR2.UI.HealthBar healthBar, int desiredCount)
		{
			if (desiredCount < 0 || !healthBar.barAllocator.containerTransform.gameObject.scene.IsValid())
			{
				return;
			}
			for (int num = platingImages.Count - 1; num >= 0; num--)
			{
				if (num >= desiredCount && (bool)platingImages[num])
				{
					if (Application.isPlaying)
					{
						UnityEngine.Object.Destroy(platingImages[num].gameObject);
					}
					else
					{
						UnityEngine.Object.DestroyImmediate(platingImages[num].gameObject);
					}
					platingImages.RemoveAt(num);
				}
				else if (!platingImages[num])
				{
					platingImages.RemoveAt(num);
				}
			}
			for (int i = platingImages.Count; i < desiredCount; i++)
			{
				Image component = UnityEngine.Object.Instantiate(healthBar.barAllocator.elementPrefab, healthBar.barAllocator.containerTransform).GetComponent<Image>();
				platingImages.Add(component);
				GameObject gameObject = component.gameObject;
				gameObject.SetActive(value: true);
			}
		}

		public static void SetRectPosition(RectTransform rectTransform, float xMin, float xMax, float sizeDelta)
		{
			rectTransform.anchorMin = new Vector2(xMin, 0f);
			rectTransform.anchorMax = new Vector2(xMax, 1f);
			rectTransform.anchoredPosition = Vector2.zero;
			rectTransform.sizeDelta = new Vector2(sizeDelta * 0.5f + 1f, sizeDelta + 1f);
		}

		public void SetRemainingPlatingCount(int count)
		{
			remainingPlatingCountSynced = count;
			NetworkIdentity component = body.GetComponent<NetworkIdentity>();
			if ((bool)component)
			{
				NetMessageExtensions.Send((INetMessage)(object)new SyncRemaingPlatingCount(remainingPlatingCountSynced, component.netId), (NetworkDestination)1);
			}
		}
	}

	public class SyncRemaingPlatingCount : INetMessage, ISerializableObject
	{
		private int NewCount;

		private NetworkInstanceId GameObjectID;

		public SyncRemaingPlatingCount()
		{
		}

		public SyncRemaingPlatingCount(int newCount, NetworkInstanceId gameObjectID)
		{
			NewCount = newCount;
			GameObjectID = gameObjectID;
		}

		public void Serialize(NetworkWriter writer)
		{
			writer.Write(NewCount);
			writer.Write(GameObjectID);
		}

		public void Deserialize(NetworkReader reader)
		{
			NewCount = reader.ReadInt32();
			GameObjectID = reader.ReadNetworkId();
		}

		public void OnReceived()
		{
			if (NetworkServer.active)
			{
				return;
			}
			GameObject gameObject = RoR2.Util.FindNetworkObject(GameObjectID);
			if ((bool)gameObject)
			{
				PlatedAffixBuffBehaviour component = gameObject.GetComponent<PlatedAffixBuffBehaviour>();
				if ((bool)component)
				{
					component.remainingPlatingCountSynced = NewCount;
				}
			}
		}
	}

	public static RoR2.BuffDef damageReductionBuff;

	public static GameObject zeroDamageEffectPrefab;

	public static GameObject platedBlockEffectPrefab;

	public static GameObject damageReductionVFXPrefab;

	public static Color healthBarColorGray = SpikestripUtil.ColorRGB(119f, 113f, 97f);

	public static Color healthBarColorTan = SpikestripUtil.ColorRGB(224f, 206f, 161f);

	public static GameObject ShieldDisplay;

	public override string EliteName => "Plated";

	public override Color EliteColor => SpikestripUtil.ColorRGB(155f, 144f, 122f);

	public override EliteTierDefinition MainEliteTierDefinition => EliteBase<PlatedElite>.tierOneEliteDefault;

	public override EliteTierDefinition[] ExtraEliteTierDefitions => new EliteTierDefinition[1] { EliteBase<PlatedElite>.honorEliteDefault };

	public override Sprite AffixBuffSprite => SpikestripContent.GrooveBundle.LoadAsset<Sprite>("texBuffAffixPlated.png");

	public override Sprite EquipmentIcon => SpikestripContent.GrooveBundle.LoadAsset<Sprite>("texAffixPlatedIcon.png");

	public override string EquipmentName => "Alloy of Subservience";

	public override string AffixDescriptionMainWord => "endurance";

	public override Texture2D EliteRampTexture => SpikestripContent.GrooveBundle.LoadAsset<Texture2D>("texRampElitePlated.png");

	public override Type AffixBuffBehaviour => typeof(PlatedAffixBuffBehaviour);

	public override bool ServerOnlyAffixBuffBehaviour => false;

	public override void Init()
	{
		base.Init();
		NetworkingAPI.RegisterMessageType<SyncRemaingPlatingCount>();
		On.RoR2.UI.HealthBar.UpdateHealthbar += HealthBar_UpdateHealthbar;
		On.RoR2.HealthComponent.TakeDamage += HealthComponent_TakeDamage;
		damageReductionBuff = ScriptableObject.CreateInstance<RoR2.BuffDef>();
		damageReductionBuff.name = "DamageReductionBuff";
		damageReductionBuff.buffColor = EliteColor;
		damageReductionBuff.canStack = true;
		damageReductionBuff.iconSprite = SpikestripContent.GrooveBundle.LoadAsset<Sprite>("texBuffDamageReduction.png");
		damageReductionBuff.isDebuff = true;
		damageReductionBuff.startSfx = LegacyResourcesAPI.Load<RoR2.NetworkSoundEventDef>("networksoundeventdefs/nsePulverizeBuildupBuffApplied");
		ContentAddition.AddBuffDef(damageReductionBuff);
		zeroDamageEffectPrefab = PrefabAPI.InstantiateClone(LegacyResourcesAPI.Load<GameObject>("Prefabs/Effects/BearProc"), "ZeroDamageEffect", false);
		UnityEngine.Object.Destroy(zeroDamageEffectPrefab.transform.Find("Fluff").gameObject);
		zeroDamageEffectPrefab.GetComponent<RoR2.EffectComponent>().soundName = "";
		zeroDamageEffectPrefab.GetComponent<RoR2.VFXAttributes>().vfxPriority = RoR2.VFXAttributes.VFXPriority.Always;
		TextMeshPro componentInChildren = zeroDamageEffectPrefab.GetComponentInChildren<TextMeshPro>();
		componentInChildren.color = Color.gray;
		componentInChildren.text = "INFO_ZERO_DAMAGE";
		RoR2.UI.LanguageTextMeshController componentInChildren2 = zeroDamageEffectPrefab.GetComponentInChildren<RoR2.UI.LanguageTextMeshController>();
		componentInChildren2.token = "INFO_ZERO_DAMAGE";
		zeroDamageEffectPrefab.transform.localScale = Vector3.one * 1.5f;
		ContentAddition.AddEffect(zeroDamageEffectPrefab);
		LanguageAPI.Add("INFO_ZERO_DAMAGE", "0!");
		Color color = SpikestripUtil.ColorRGB(181f, 159f, 105f);
		platedBlockEffectPrefab = PrefabAPI.InstantiateClone(LegacyResourcesAPI.Load<GameObject>("prefabs/effects/impacteffects/CaptainBodyArmorBlockEffect"), "PlatingBlockEffect", false);
		Renderer[] componentsInChildren = platedBlockEffectPrefab.GetComponentsInChildren<Renderer>();
		foreach (Renderer renderer in componentsInChildren)
		{
			if (renderer.gameObject.activeSelf)
			{
				Material material = UnityEngine.Object.Instantiate(renderer.material);
				material.SetColor("_TintColor", color);
				renderer.material = material;
			}
		}
		platedBlockEffectPrefab.GetComponentInChildren<TextMeshPro>().color = color;
		platedBlockEffectPrefab.GetComponent<RoR2.EffectComponent>().soundName = "Play_item_proc_armorReduction_shatter";
		ContentAddition.AddEffect(platedBlockEffectPrefab);
		PostProcessProfile postProcessProfile = ScriptableObject.CreateInstance<PostProcessProfile>();
		Vignette vignette = postProcessProfile.AddSettings<Vignette>();
		vignette.mode.Override(VignetteMode.Classic);
		vignette.color.Override(SpikestripUtil.ColorRGB(255f, 207f, 156f));
		vignette.intensity.Override(0.2f);
		vignette.smoothness.Override(1f);
		damageReductionVFXPrefab = PrefabAPI.InstantiateClone(SpikestripUtil.LegacyLoad<GameObject>("Prefabs/TemporaryVisualEffects/HealingDisabledEffect"), "DamageReductionEffect", false);
		UnityEngine.Object.DestroyImmediate(damageReductionVFXPrefab.transform.Find("Visual").gameObject);
		damageReductionVFXPrefab.GetComponentInChildren<PostProcessVolume>().sharedProfile = postProcessProfile;
		damageReductionVFXPrefab.GetComponentInChildren<RoR2.PostProcessDuration>().maxDuration = 0.4f;
		TempVisualEffectAPI.AddTemporaryVisualEffect(damageReductionVFXPrefab, (RoR2.CharacterBody body) => body.radius, (RoR2.CharacterBody body) => body.HasBuff(damageReductionBuff));
	}

	private void HealthComponent_TakeDamage(On.RoR2.HealthComponent.orig_TakeDamage orig, RoR2.HealthComponent self, RoR2.DamageInfo damageInfo)
	{
		RoR2.CharacterBody characterBody = (damageInfo.attacker ? damageInfo.attacker.GetComponent<RoR2.CharacterBody>() : null);
		if ((bool)characterBody && characterBody.HasBuff(damageReductionBuff))
		{
			damageInfo.damage -= characterBody.damage * (float)characterBody.GetBuffCount(damageReductionBuff);
			if (damageInfo.damage <= 0f)
			{
				damageInfo.rejected = true;
				if (RoR2.SettingsConVars.enableDamageNumbers.value)
				{
					RoR2.EffectData effectData = new RoR2.EffectData
					{
						origin = damageInfo.position,
						rotation = RoR2.Util.QuaternionSafeLookRotation((damageInfo.force != Vector3.zero) ? damageInfo.force : UnityEngine.Random.onUnitSphere)
					};
					RoR2.EffectManager.SpawnEffect(zeroDamageEffectPrefab, effectData, transmit: true);
				}
			}
		}
		orig(self, damageInfo);
	}

	public override void OnHitEnemyServer(RoR2.DamageInfo damageInfo, GameObject victim)
	{
		RoR2.CharacterBody component = damageInfo.attacker.GetComponent<RoR2.CharacterBody>();
		RoR2.CharacterBody characterBody = (victim ? victim.GetComponent<RoR2.CharacterBody>() : null);
		if ((bool)component && component.HasBuff(AffixBuff) && (bool)characterBody)
		{
			characterBody.AddTimedBuff(damageReductionBuff, 8f * damageInfo.procCoefficient);
		}
	}

	private void HealthBar_UpdateHealthbar(On.RoR2.UI.HealthBar.orig_UpdateHealthbar orig, RoR2.UI.HealthBar self, float deltaTime)
	{
		orig(self, deltaTime);
		if ((bool)self.source)
		{
			PlatedAffixBuffBehaviour component = self.source.GetComponent<PlatedAffixBuffBehaviour>();
			if ((bool)component)
			{
				component.UpdateHealthbarPlates(self);
			}
		}
	}

	public override void AssignEquipmentValues()
	{
		EquipmentPickupModel = CreateAffixModel(SpikestripUtil.ColorRGB(114f, 99f, 61f));
		ShieldDisplay = PrefabAPI.InstantiateClone(LegacyResourcesAPI.Load<GameObject>("prefabs/temporaryvisualeffects/ElephantDefense"), "PlatedShieldDisplay", false);
		UnityEngine.Object.DestroyImmediate(ShieldDisplay.GetComponent<RoR2.TemporaryVisualEffect>());
		UnityEngine.Object.DestroyImmediate(ShieldDisplay.GetComponentInChildren<RoR2.VFXAttributes>());
		UnityEngine.Object.DestroyImmediate(ShieldDisplay.GetComponentInChildren<RoR2.ObjectScaleCurve>());
		UnityEngine.Object.DestroyImmediate(ShieldDisplay.GetComponentInChildren<RoR2.ObjectScaleCurve>());
		ShieldDisplay.GetComponentInChildren<RotateObject>().rotationSpeed = new Vector3(0f, 40f, 0f);
		Color color = SpikestripUtil.ColorRGB(144f, 110f, 75f);
		Color color2 = SpikestripUtil.ColorRGB(109f, 86f, 68f);
		MeshRenderer[] componentsInChildren = ShieldDisplay.GetComponentsInChildren<MeshRenderer>();
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			MeshRenderer meshRenderer = componentsInChildren[i];
			Material material = meshRenderer.material;
			material.color = ((i % 2 == 0) ? color2 : color);
			material.DisableKeyword("FRESNEL_EMISSION");
			meshRenderer.material = material;
			meshRenderer.gameObject.transform.localScale = Vector3.one * 0.6f;
		}
		Transform transform = ShieldDisplay.transform.Find("MeshHolder");
		transform.localScale = Vector3.one * 3f;
		transform.rotation = RoR2.Util.QuaternionSafeLookRotation(Vector3.down);
		transform.localPosition = new Vector3(0f, 0f, 0f);
		UnityEngine.Object.DestroyImmediate(transform.Find("Glow").gameObject);
		UnityEngine.Object.DestroyImmediate(transform.Find("Bright Glow").gameObject);
		RoR2.ItemDisplay itemDisplay = ShieldDisplay.AddComponent<RoR2.ItemDisplay>();
		itemDisplay.rendererInfos = SpikestripUtil.GenerateRendererInfos(ShieldDisplay);
		EquipmentDisplayModel = SpikestripContent.GrooveBundle.LoadAsset<GameObject>("ElitePlatingDisplay.prefab");
		MeshRenderer componentInChildren = EquipmentDisplayModel.GetComponentInChildren<MeshRenderer>();
		componentInChildren.material = ShieldDisplay.GetComponentInChildren<MeshRenderer>().material;
		Transform transform2 = EquipmentDisplayModel.transform.Find("mdlElitePlating");
		transform2.localScale = Vector3.one * 1f;
		transform2.rotation = RoR2.Util.QuaternionSafeLookRotation(Vector3.forward);
		transform2.localPosition = new Vector3(0f, -1f, 0f);
		RoR2.ItemDisplay itemDisplay2 = EquipmentDisplayModel.AddComponent<RoR2.ItemDisplay>();
		itemDisplay2.rendererInfos = SpikestripUtil.GenerateRendererInfos(EquipmentDisplayModel);
	}

	public override void PopulateItemDisplayRules(ItemDisplayRuleDict itemDisplayRuleDict)
	{
		RegisterDisplayParent(LegacyResourcesAPI.Load<RoR2.EquipmentDef>("EquipmentDefs/AffixWhite"), ShieldDisplay);
	}
}
