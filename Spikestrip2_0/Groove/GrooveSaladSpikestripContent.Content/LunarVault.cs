using System.Collections.Generic;
using System.Text;
using EntityStates;
using EntityStates.LunarGolem;
using HG;
using On.RoR2;
using R2API;
using R2API.Networking;
using R2API.Networking.Interfaces;
using RoR2;
using RoR2.Hologram;
using RoR2.Navigation;
using RoR2.UI;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;
using Spikestrip2_0.Core;

namespace GrooveSaladSpikestripContent.Content;

public class LunarVault : InteractableBase<LunarVault>
{
	public class LunarVaultBehaviour : MonoBehaviour
	{
		public List<GameObject> bubbles = new List<GameObject>();

		public float currentPercent;

		public bool available = true;

		public bool locked;

		public void SetNewAvailable(bool newAvailable)
		{
			if (NetworkServer.active)
			{
				available = newAvailable;
				NetMessageExtensions.Send((INetMessage)(object)new SyncLunarVaultAvailable(newAvailable, GetComponent<NetworkIdentity>().netId), (NetworkDestination)1);
			}
		}

		public void Awake()
		{
			RoR2.PurchaseInteraction component = GetComponent<RoR2.PurchaseInteraction>();
			component.onPurchase.AddListener(OnUse);
		}

		public void OnUse(RoR2.Interactor interactor)
		{
			if (NetworkServer.active)
			{
				SetNewAvailable(newAvailable: false);
				RoR2.EntityStateMachine component = GetComponent<RoR2.EntityStateMachine>();
				if ((bool)component)
				{
					component.SetNextState(new LunarVaultActivation());
				}
			}
		}

		private void Start()
		{
			RoR2.ShopTerminalBehavior component = GetComponent<RoR2.ShopTerminalBehavior>();
			component.SetPickupIndex(RoR2.PickupCatalog.FindPickupIndex("LunarCoin.Coin0"));
			for (int i = 0; i < bubbles.Count; i++)
			{
				RoR2.ObjectScaleCurve component2 = bubbles[i].GetComponent<RoR2.ObjectScaleCurve>();
				if ((bool)component2)
				{
					component2.timeMax = Random.Range(1f, LunarVaultOpening.duration * LunarVaultOpening.flashAlphaFraction);
				}
			}
		}
	}

	public class LunarVaultActivation : BaseState
	{
		public static float duration = 1f;

		public override void OnEnter()
		{
			base.OnEnter();
			RoR2.Util.PlaySound(Shell.preShieldSoundString, base.gameObject);
			RoR2.EffectManager.SpawnEffect(Shell.preShieldEffect, new RoR2.EffectData
			{
				origin = base.transform.position,
				rotation = base.transform.rotation
			}, transmit: false);
		}

		public override void FixedUpdate()
		{
			base.FixedUpdate();
			if (base.fixedAge >= duration && base.isAuthority)
			{
				outer.SetNextState(new LunarVaultOpening());
			}
		}

		public override void OnExit()
		{
			RoR2.Util.PlaySound(Shell.shieldActivateSoundString, base.gameObject);
			base.OnExit();
		}
	}

	public class LunarVaultOpening : BaseState
	{
		public static float duration = 90f;

		public static float flashAlphaFraction = 0.95f;

		public static float minAlpha = 0.4f;

		public static AnimationCurve alphaCurve = AnimationCurve.EaseInOut(0f, 1f, flashAlphaFraction, minAlpha);

		private float chargingAge;

		private Transform mdlTransform;

		private Material materialInstance;

		private GameObject openingIndicator;

		private RoR2.PickupDisplay pickupDisplay;

		public float completionPercentage => Mathf.Clamp(Mathf.FloorToInt(chargingAge / duration * 99f), 0, 99);

		public override void OnEnter()
		{
			base.OnEnter();
			ObjectivePanelController.collectObjectiveSources += ObjectivePanelController_collectObjectiveSources;
			mdlTransform = base.transform.Find("mdlBazaarBlueprintTable");
			pickupDisplay = base.gameObject.GetComponentInChildren<RoR2.PickupDisplay>();
			SetBubblesActive(newActive: true);
			base.transform.Find("LunarVaultParticleObject").gameObject.SetActive(value: true);
			GameObject gameObject = mdlTransform.Find("Display/MainBubbleMesh").gameObject;
			MeshRenderer component = gameObject.GetComponent<MeshRenderer>();
			materialInstance = Object.Instantiate(component.material);
			component.material = materialInstance;
			RoR2.Util.PlaySound("Play_lunar_golem_attack1_aliveLoop", base.gameObject);
			openingIndicator = Object.Instantiate(lunarVaultOpeningIndicator, base.transform.position, Quaternion.identity);
			openingIndicator.GetComponent<RoR2.PositionIndicator>().targetTransform = gameObject.transform;
			openingIndicator.GetComponent<LunarVaultOpeningIndicatorController>().trackedLunarVaultBehaviour = GetComponent<LunarVaultBehaviour>();
		}

		private void ObjectivePanelController_collectObjectiveSources(RoR2.CharacterMaster master, List<ObjectivePanelController.ObjectiveSourceDescriptor> list)
		{
			list.Add(new ObjectivePanelController.ObjectiveSourceDescriptor
			{
				master = master,
				objectiveType = typeof(OpenLunarVaultObjective),
				source = base.gameObject
			});
		}

		public override void FixedUpdate()
		{
			base.FixedUpdate();
			RoR2.PurchaseInteraction component = GetComponent<RoR2.PurchaseInteraction>();
			bool flag = (bool)component && !component.lockGameObject;
			if (flag)
			{
				chargingAge += Time.fixedDeltaTime;
			}
			LunarVaultBehaviour component2 = GetComponent<LunarVaultBehaviour>();
			component2.currentPercent = completionPercentage;
			component2.locked = !flag;
			if (chargingAge >= duration && NetworkServer.active)
			{
				outer.SetNextStateToMain();
			}
		}

		public override void Update()
		{
			base.Update();
			float num = Mathf.Clamp01(chargingAge / duration);
			if ((bool)materialInstance)
			{
				float value = ((num <= flashAlphaFraction) ? alphaCurve.Evaluate(num) : GetFlashingAlpha());
				materialInstance.SetFloat("_ExternalAlpha", value);
			}
			if ((bool)pickupDisplay)
			{
				pickupDisplay.spinSpeed = Mathf.Lerp(30f, 90f, num);
			}
		}

		public override void OnExit()
		{
			if ((bool)openingIndicator)
			{
				EntityState.Destroy(openingIndicator);
			}
			RoR2.Util.PlaySound("Stop_lunar_golem_attack1_aliveLoop", base.gameObject);
			base.transform.Find("LunarVaultParticleObject").gameObject.SetActive(value: false);
			ObjectivePanelController.collectObjectiveSources -= ObjectivePanelController_collectObjectiveSources;
			if (NetworkServer.active)
			{
				GetComponent<RoR2.PurchaseInteraction>().SetAvailable(newAvailable: false);
				GetComponent<RoR2.ShopTerminalBehavior>().DropPickup();
				GetComponent<RoR2.ShopTerminalBehavior>().SetNoPickup();
			}
			if ((bool)pickupDisplay)
			{
				RoR2.EffectManager.SpawnEffect(openingEffect, new RoR2.EffectData
				{
					origin = pickupDisplay.transform.position,
					rotation = Quaternion.identity
				}, transmit: false);
			}
			mdlTransform.Find("Display/MainBubbleMesh").gameObject.SetActive(value: false);
			mdlTransform.Find("BubbleParticles").gameObject.SetActive(value: false);
			if ((bool)materialInstance)
			{
				Object.Destroy(materialInstance);
			}
			base.OnExit();
		}

		public void SetBubblesActive(bool newActive)
		{
			LunarVaultBehaviour component = GetComponent<LunarVaultBehaviour>();
			for (int i = 0; i < component.bubbles.Count; i++)
			{
				component.bubbles[i].SetActive(newActive);
			}
		}

		public float GetFlashingAlpha()
		{
			if (Mathf.FloorToInt(Time.fixedTime * 10f) % 2 != 0)
			{
				return minAlpha;
			}
			return 0f;
		}
	}

	private class OpenLunarVaultObjective : ObjectivePanelController.ObjectiveTracker
	{
		public override string GenerateString()
		{
			LunarVaultBehaviour component = ((GameObject)sourceDescriptor.source).GetComponent<LunarVaultBehaviour>();
			string text = string.Format(RoR2.Language.GetString("OBJECTIVE_LUNARVAULT_OPEN"), component.currentPercent);
			if (component.locked && (int)(Time.time * 8f) % 2 == 0)
			{
				text = $"<style=cLunarObjective>{text}</style>";
			}
			return text;
		}

		public override bool IsDirty()
		{
			return true;
		}
	}

	public class LunarVaultOpeningIndicatorController : MonoBehaviour
	{
		[Header("Cached Values")]
		public SpriteRenderer[] iconSprites;

		public TextMeshPro chargingText;

		public LunarVaultBehaviour trackedLunarVaultBehaviour;

		[Header("Color Values")]
		public Color spriteBaseColor;

		public Color spriteFlashColor;

		public Color spriteChargingColor;

		public Color spriteChargedColor;

		public Color textBaseColor;

		public Color textChargingColor;

		[Header("Options")]
		public bool disableTextWhenNotCharging;

		public bool disableTextWhenCharged;

		public bool flashWhenNotCharging;

		public float flashFrequency;

		[Header("Control Values - Don't assign via inspector!")]
		public bool isCharging;

		public bool isCharged;

		private float flashStopwatch;

		public uint chargeValue { get; set; }

		private void Update()
		{
			if ((bool)trackedLunarVaultBehaviour)
			{
				chargeValue = (uint)trackedLunarVaultBehaviour.currentPercent;
				isCharging = !trackedLunarVaultBehaviour.locked;
			}
			Color color = spriteBaseColor;
			Color color2 = textBaseColor;
			if (!isCharged)
			{
				if (flashWhenNotCharging)
				{
					flashStopwatch += Time.deltaTime;
					color = (((int)(flashStopwatch * flashFrequency) % 2 == 0) ? spriteFlashColor : spriteBaseColor);
				}
				if (isCharging)
				{
					color = spriteChargingColor;
					color2 = textChargingColor;
				}
				if (disableTextWhenNotCharging)
				{
					chargingText.enabled = isCharging;
				}
				else
				{
					chargingText.enabled = true;
				}
			}
			else
			{
				color = spriteChargedColor;
				if (disableTextWhenCharged)
				{
					chargingText.enabled = false;
				}
			}
			if (chargingText.enabled)
			{
				StringBuilder stringBuilder = HG.StringBuilderPool.RentStringBuilder();
				stringBuilder.AppendUint(chargeValue, 1u, 3u).Append("%");
				chargingText.SetText(stringBuilder);
				HG.StringBuilderPool.ReturnStringBuilder(stringBuilder);
			}
			SpriteRenderer[] array = iconSprites;
			for (int i = 0; i < array.Length; i++)
			{
				array[i].color = color;
			}
			chargingText.color = color2;
		}
	}

	public class SyncLunarVaultAvailable : INetMessage, ISerializableObject
	{
		private bool NewAvailable;

		private NetworkInstanceId GameObjectID;

		public SyncLunarVaultAvailable()
		{
		}

		public SyncLunarVaultAvailable(bool newAvailable, NetworkInstanceId gameObjectID)
		{
			NewAvailable = newAvailable;
			GameObjectID = gameObjectID;
		}

		public void Serialize(NetworkWriter writer)
		{
			writer.Write(NewAvailable);
			writer.Write(GameObjectID);
		}

		public void Deserialize(NetworkReader reader)
		{
			NewAvailable = reader.ReadBoolean();
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
				LunarVaultBehaviour component = gameObject.GetComponent<LunarVaultBehaviour>();
				if ((bool)component)
				{
					component.available = NewAvailable;
				}
			}
		}
	}

	public static GameObject lunarVaultOpeningIndicator;

	public static GameObject openingEffect;

	public static GameObject activateEffect;

	public static Material matNewtBubble = LegacyResourcesAPI.Load<GameObject>("prefabs/networkedobjects/BlueprintStation").transform.Find("mdlBazaarBlueprintTable/BubbleMesh").gameObject.GetComponent<MeshRenderer>().material;

	public override string InteractableName => "Lunar Vault";

	public override void ModifySpawnCard()
	{
		base.ModifySpawnCard();
		InteractableSpawnCard.hullSize = HullClassification.Human;
		InteractableSpawnCard.requiredFlags = NodeFlags.None;
		InteractableSpawnCard.forbiddenFlags = NodeFlags.NoChestSpawn;
		InteractableSpawnCard.directorCreditCost = 5;
		InteractableSpawnCard.occupyPosition = true;
		InteractableSpawnCard.eliteRules = RoR2.SpawnCard.EliteRules.Default;
		InteractableSpawnCard.orientToFloor = true;
		InteractableSpawnCard.slightlyRandomizeOrientation = false;
		InteractableSpawnCard.skipSpawnWhenSacrificeArtifactEnabled = true;
	}

	public override void ModifyDirectorCardAndAddInteractable()
	{
		base.ModifyDirectorCardAndAddInteractable();
		DirectorCard.selectionWeight = 1;
		AddInteractable("Chests");
	}

	public override void SetHooks()
	{
		On.RoR2.PurchaseInteraction.GetInteractability += PurchaseInteraction_GetInteractability;
	}

	private static Interactability PurchaseInteraction_GetInteractability(On.RoR2.PurchaseInteraction.orig_GetInteractability orig, RoR2.PurchaseInteraction self, RoR2.Interactor activator)
	{
		LunarVaultBehaviour component = self.gameObject.GetComponent<LunarVaultBehaviour>();
		if ((bool)component && !component.available)
		{
			return Interactability.Disabled;
		}
		return orig(self, activator);
	}

	public static void EffectPrefabs()
	{
		openingEffect = PrefabAPI.InstantiateClone(LegacyResourcesAPI.Load<GameObject>("prefabs/effects/CleanseEffect"), "LunarVaultOpenEffect", false);
		RoR2.EffectComponent component = openingEffect.GetComponent<RoR2.EffectComponent>();
		component.soundName = "Play_lunar_wisp_death";
		openingEffect.transform.localScale = Vector3.one * 1.5f;
		openingEffect.transform.Find("ShockwaveIn").gameObject.GetComponent<ParticleSystemRenderer>().material = matNewtBubble;
		ContentAddition.AddEffect(openingEffect);
		activateEffect = PrefabAPI.InstantiateClone(LegacyResourcesAPI.Load<GameObject>("prefabs/effects/MonstersOnShrineUse"), "LunarVaultActivateEffect", false);
		RoR2.EffectComponent component2 = activateEffect.GetComponent<RoR2.EffectComponent>();
		component2.soundName = "";
		activateEffect.transform.Find("Rings").gameObject.GetComponent<ParticleSystemRenderer>().material = matNewtBubble;
		Object.Destroy(activateEffect.transform.Find("BlastDark_Ps").gameObject);
		Object.Destroy(activateEffect.transform.Find("Chunks01_Ps").gameObject);
		Object.Destroy(activateEffect.transform.Find("Chunks01_Ps (1)").gameObject);
		Light componentInChildren = activateEffect.GetComponentInChildren<Light>();
		componentInChildren.intensity *= 0.75f;
		componentInChildren.color = new Color(0.6039216f, 0.6745098f, 1f);
		ContentAddition.AddEffect(activateEffect);
	}

	public static void OpeningIndicatorPrefab()
	{
		GameObject gameObject = LegacyResourcesAPI.Load<GameObject>("prefabs/positionindicators/PillarChargingPositionIndicator");
		ChargeIndicatorController component = gameObject.GetComponent<ChargeIndicatorController>();
		lunarVaultOpeningIndicator = PrefabAPI.InstantiateClone(gameObject, "LunarVaultOpeningIndicator", false);
		Object.Destroy(lunarVaultOpeningIndicator.GetComponent<ChargeIndicatorController>());
		GameObject gameObject2 = lunarVaultOpeningIndicator.transform.Find("BoingyScaler/TeleporterSprite").gameObject;
		SpriteRenderer component2 = gameObject2.GetComponent<SpriteRenderer>();
		component2.sprite = LegacyResourcesAPI.Load<Sprite>("textures/miscicons/texUnlockIcon");
		gameObject2.transform.localScale *= 0.5f;
		RectTransform rectTransform = (RectTransform)gameObject2.transform.Find("TextMeshPro");
		rectTransform.localScale *= 2f;
		rectTransform.localPosition = new Vector3(0f, 4f, 0f);
		LunarVaultOpeningIndicatorController lunarVaultOpeningIndicatorController = lunarVaultOpeningIndicator.AddComponent<LunarVaultOpeningIndicatorController>();
		lunarVaultOpeningIndicatorController.iconSprites = new SpriteRenderer[2]
		{
			component2,
			lunarVaultOpeningIndicator.transform.Find("BoingyScaler/OutsideFrameArrow/Sprite").gameObject.GetComponent<SpriteRenderer>()
		};
		lunarVaultOpeningIndicatorController.chargingText = lunarVaultOpeningIndicator.GetComponentInChildren<TextMeshPro>();
		lunarVaultOpeningIndicatorController.spriteBaseColor = new Color(0.6039216f, 0.6745098f, 1f);
		lunarVaultOpeningIndicatorController.spriteChargedColor = component.spriteChargedColor;
		lunarVaultOpeningIndicatorController.spriteChargingColor = new Color(0.6039216f, 0.6745098f, 1f);
		lunarVaultOpeningIndicatorController.spriteFlashColor = component.textChargingColor;
		lunarVaultOpeningIndicatorController.textBaseColor = component.textBaseColor;
		lunarVaultOpeningIndicatorController.textChargingColor = component.textChargingColor;
		lunarVaultOpeningIndicatorController.disableTextWhenCharged = true;
		lunarVaultOpeningIndicatorController.disableTextWhenNotCharging = true;
		lunarVaultOpeningIndicatorController.flashFrequency = 8f;
		lunarVaultOpeningIndicatorController.flashWhenNotCharging = true;
		lunarVaultOpeningIndicator.SetActive(value: true);
	}

	public override void AssignPrefab()
	{
		EffectPrefabs();
		OpeningIndicatorPrefab();
		NetworkingAPI.RegisterMessageType<SyncLunarVaultAvailable>();
		ContentAddition.AddEntityState(typeof(LunarVaultOpening), out _);
		ContentAddition.AddEntityState(typeof(LunarVaultActivation), out _);
		LanguageAPI.Add("OBJECTIVE_LUNARVAULT_OPEN", "Open the <style=cLunarObjective>Lunar Vault</style> ({0}%)");
		InteractablePrefab = PrefabAPI.InstantiateClone(LegacyResourcesAPI.Load<GameObject>("prefabs/networkedobjects/BlueprintStation"), "LunarVault", true);
		Object.Destroy(InteractablePrefab.GetComponent<RoR2.BlueprintTerminal>());
		Object.Destroy(InteractablePrefab.GetComponent<HologramProjector>());
		Object.Destroy(InteractablePrefab.GetComponent<RoR2.BlueprintTerminal>());
		Object.Destroy(InteractablePrefab.transform.Find("HologramPivot").gameObject);
		Transform transform = InteractablePrefab.transform.Find("mdlBazaarBlueprintTable");
		Color color = new Color(41f / 51f, 76f / 85f, 1f);
		MeshRenderer component = transform.gameObject.GetComponent<MeshRenderer>();
		Material material = Object.Instantiate(component.material);
		material.color = color;
		component.material = material;
		GameObject gameObject = transform.transform.Find("Display").gameObject;
		Transform transform2 = gameObject.transform.Find("MainBubbleMesh");
		transform2.transform.localScale = new Vector3(2.5f, transform2.transform.localScale.y, transform2.transform.localScale.z);
		Object.Destroy(transform2.gameObject.GetComponent<SphereCollider>());
		MeshCollider meshCollider = transform2.gameObject.AddComponent<MeshCollider>();
		meshCollider.isTrigger = false;
		meshCollider.sharedMesh = transform2.gameObject.GetComponent<MeshFilter>().sharedMesh;
		LunarVaultBehaviour lunarVaultBehaviour = InteractablePrefab.AddComponent<LunarVaultBehaviour>();
		GameObject gameObject2 = PrefabAPI.InstantiateClone(LegacyResourcesAPI.Load<GameObject>("prefabs/networkedobjects/chest/MultiShopEquipmentTerminal").GetComponentInChildren<RoR2.PickupDisplay>().gameObject, "LunarVaultPickupDisplay", false);
		RoR2.PickupDisplay component2 = gameObject2.GetComponent<RoR2.PickupDisplay>();
		component2.spinSpeed = 30f;
		component2.highlight = gameObject2.GetComponent<RoR2.Highlight>();
		gameObject2.transform.SetParent(transform);
		gameObject2.transform.localPosition = new Vector3(0f, 0f, 5.15f);
		gameObject2.transform.localScale = Vector3.one * 0.97f;
		MeshRenderer[] componentsInChildren = transform.gameObject.GetComponentsInChildren<MeshRenderer>();
		foreach (MeshRenderer meshRenderer in componentsInChildren)
		{
			if (meshRenderer.gameObject.name == "BubbleMesh")
			{
				RoR2.ObjectScaleCurve objectScaleCurve = meshRenderer.gameObject.AddComponent<RoR2.ObjectScaleCurve>();
				objectScaleCurve.useOverallCurveOnly = true;
				objectScaleCurve.overallCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
				objectScaleCurve.timeMax = 0.2f;
				lunarVaultBehaviour.bubbles.Add(meshRenderer.gameObject);
				meshRenderer.gameObject.SetActive(value: false);
			}
		}
		GameObject gameObject3 = transform.Find("BubbleParticles").gameObject;
		lunarVaultBehaviour.bubbles.Add(gameObject3);
		gameObject3.SetActive(value: false);
		GameObject gameObject4 = PrefabAPI.InstantiateClone(LegacyResourcesAPI.Load<GameObject>("prefabs/networkedobjects/MoonBatteryMass").transform.Find("Model/mdlMoonBattery/ChargingFX/InnerParticles").gameObject, "LunarVaultParticleObject", false);
		ParticleSystemRenderer component3 = gameObject4.GetComponent<ParticleSystemRenderer>();
		Material material2 = Object.Instantiate(LegacyResourcesAPI.Load<GameObject>("prefabs/effects/tracers/TracerCaptainDefenseMatrix").transform.Find("TracerHead/Flash").gameObject.GetComponent<ParticleSystemRenderer>().material);
		material2.SetColor("_TintColor", new Color(1f / 17f, 0.1764706f, 1f));
		component3.material = material2;
		gameObject4.transform.SetParent(InteractablePrefab.transform);
		gameObject4.transform.localRotation = RoR2.Util.QuaternionSafeLookRotation(Vector3.down);
		gameObject4.transform.localPosition = new Vector3(0f, 6f, 0f);
		gameObject4.transform.localScale = Vector3.one * 0.6f;
		gameObject4.SetActive(value: false);
		RoR2.EntityStateMachine entityStateMachine = InteractablePrefab.AddComponent<RoR2.EntityStateMachine>();
		entityStateMachine.customName = "Main";
		entityStateMachine.initialStateType = new SerializableEntityStateType(typeof(Idle));
		entityStateMachine.mainStateType = new SerializableEntityStateType(typeof(Idle));
		RoR2.NetworkStateMachine networkStateMachine = InteractablePrefab.AddComponent<RoR2.NetworkStateMachine>();
		networkStateMachine.stateMachines = new RoR2.EntityStateMachine[1] { entityStateMachine };
		RoR2.ShopTerminalBehavior shopTerminalBehavior = InteractablePrefab.AddComponent<RoR2.ShopTerminalBehavior>();
		shopTerminalBehavior.dropTransform = gameObject2.transform;
		shopTerminalBehavior.dropVelocity = Vector3.up * 10f;
		shopTerminalBehavior.pickupDisplay = component2;
		shopTerminalBehavior.selfGeneratePickup = false;
		RoR2.PurchaseInteraction component4 = InteractablePrefab.GetComponent<RoR2.PurchaseInteraction>();
		component4.displayNameToken = "LUNARVAULT_NAME";
		component4.contextToken = "LUNARVAULT_CONTEXT";
		component4.costType = CostTypeIndex.None;
		component4.onPurchase = new RoR2.PurchaseEvent();
		LanguageAPI.Add(component4.displayNameToken, InteractableName);
		LanguageAPI.Add(component4.contextToken, "Open Lunar Vault");
		transform.transform.localScale = Vector3.one;
	}
}
