using System;
using System.Collections.Generic;
using On.RoR2;
using R2API;
using R2API.Networking;
using R2API.Networking.Interfaces;
using RoR2;
using RoR2.Navigation;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.Rendering.PostProcessing;
using Spikestrip2_0.Core;

namespace GrooveSaladSpikestripContent.Content;

public class Singularity : ItemBase<Singularity>
{
	public class DistributeSingularityItemsServer : MonoBehaviour
	{
		public static int maxItemCount = 7;

		public static float duration = 3f;

		public Vector3 origin;

		private float timeBetweenDrops;

		private float itemDropAge;

		private int itemDropCount;

		private RoR2.PickupIndex pickupIndex = RoR2.PickupIndex.none;

		public void Start()
		{
			if (!NetworkServer.active)
			{
				UnityEngine.Object.Destroy(this);
			}
			timeBetweenDrops = duration / (float)maxItemCount;
		}

		public void FixedUpdate()
		{
			itemDropAge += Time.fixedDeltaTime;
			if (itemDropCount < maxItemCount && itemDropAge >= timeBetweenDrops)
			{
				itemDropAge -= timeBetweenDrops;
				RollItem();
				DropItem();
			}
			if (itemDropCount >= maxItemCount)
			{
				UnityEngine.Object.Destroy(this);
			}
		}

		public void RollItem()
		{
			WeightedSelection<List<RoR2.PickupIndex>> weightedSelection = new WeightedSelection<List<RoR2.PickupIndex>>();
			weightedSelection.AddChoice(RoR2.Run.instance.availableTier1DropList, 0.8f);
			weightedSelection.AddChoice(RoR2.Run.instance.availableTier2DropList, 0.2f);
			weightedSelection.AddChoice(RoR2.Run.instance.availableTier3DropList, 0.01f);
			List<RoR2.PickupIndex> list = weightedSelection.Evaluate(RoR2.Run.instance.treasureRng.nextNormalizedFloat);
			pickupIndex = RoR2.Run.instance.treasureRng.NextElementUniform(list);
		}

		public void DropItem()
		{
			if (!(pickupIndex == RoR2.PickupIndex.none))
			{
				float num = 360f / (float)maxItemCount;
				Vector3 vector = Quaternion.AngleAxis(num * (float)itemDropCount, Vector3.up) * Vector3.forward;
				Vector3 position = origin + vector * 8f + Vector3.up * 8f;
				RoR2.PickupDropletController.CreatePickupDroplet(pickupIndex, position, Vector3.zero);
				RoR2.EffectManager.SpawnEffect(effectPrefab, new RoR2.EffectData
				{
					origin = position,
					scale = 2f
				}, transmit: true);
				itemDropCount++;
			}
		}

		public static void FromMasterServer(RoR2.CharacterMaster characterMaster)
		{
			if (!NetworkServer.active)
			{
				return;
			}
			DistributeSingularityItemsServer distributeSingularityItemsServer = characterMaster.gameObject.AddComponent<DistributeSingularityItemsServer>();
			Vector3 vector = (distributeSingularityItemsServer.origin = (characterMaster.bodyInstanceObject ? characterMaster.bodyInstanceObject.transform.position : Vector3.zero));
			int num = RoR2.Run.instance.participatingPlayerCount - 1;
			if (num > 0)
			{
				float num2 = 360f / (float)num;
				for (int i = 0; i < num; i++)
				{
					Vector3 vector2 = Quaternion.AngleAxis(num2 * (float)i, Vector3.up) * Vector3.forward;
					Vector3 position = vector + vector2 * 50f;
					NodeGraph groundNodes = RoR2.SceneInfo.instance.groundNodes;
					NodeGraph.NodeIndex nodeIndex = groundNodes.FindClosestNode(position, HullClassification.BeetleQueen);
					groundNodes.GetNodePosition(nodeIndex, out position);
					DistributeSingularityItemsServer distributeSingularityItemsServer2 = characterMaster.gameObject.AddComponent<DistributeSingularityItemsServer>();
					distributeSingularityItemsServer2.origin = position;
				}
			}
		}
	}

	public class CorruptedHoldoutZoneController : MonoBehaviour
	{
		public class SyncAddController : INetMessage, ISerializableObject
		{
			private NetworkInstanceId GameObjectID;

			public SyncAddController()
			{
			}

			public SyncAddController(NetworkInstanceId gameObjectID)
			{
				GameObjectID = gameObjectID;
			}

			public void Serialize(NetworkWriter writer)
			{
				writer.Write(GameObjectID);
			}

			public void Deserialize(NetworkReader reader)
			{
				GameObjectID = reader.ReadNetworkId();
			}

			public void OnReceived()
			{
				if (!NetworkServer.active)
				{
					GameObject gameObject = RoR2.Util.FindNetworkObject(GameObjectID);
					if ((bool)gameObject && !gameObject.GetComponent<CorruptedHoldoutZoneController>())
					{
						gameObject.AddComponent<CorruptedHoldoutZoneController>();
					}
				}
			}
		}

		private RoR2.HoldoutZoneController holdoutZoneController;

		private void Awake()
		{
			holdoutZoneController = GetComponent<RoR2.HoldoutZoneController>();
		}

		private void OnEnable()
		{
			holdoutZoneController.calcRadius += ApplyRadius;
			holdoutZoneController.calcChargeRate += ApplyRate;
			holdoutZoneController.calcColor += ApplyColor;
		}

		private void OnDisable()
		{
			holdoutZoneController.calcColor -= ApplyColor;
			holdoutZoneController.calcChargeRate -= ApplyRate;
			holdoutZoneController.calcRadius -= ApplyRadius;
		}

		private void ApplyRadius(ref float radius)
		{
			radius *= 1.5f;
		}

		private void ApplyColor(ref Color color)
		{
			color = Color.white * 5f;
		}

		private void ApplyRate(ref float rate)
		{
			rate /= 1.5f;
		}
	}

	public class SingularityStageAttachmentServer : MonoBehaviour
	{
		public bool hasSpawnedItemsServer = false;

		public static void AttachCorruptedStagePP()
		{
			GameObject obj = UnityEngine.Object.Instantiate(corruptedStagePP);
			NetworkServer.Spawn(obj);
		}

		public void Start()
		{
			if (!NetworkServer.active)
			{
				UnityEngine.Object.Destroy(this);
			}
			AttachCorruptedStagePP();
		}
	}

	public static GameObject corruptedStagePP;

	public static GameObject effectPrefab;

	public override string ItemToken => "SINGULARITY";

	public override string ItemName => "Cosmic Bulwark";

	public override string ItemPickup => "Gain " + DistributeSingularityItemsServer.maxItemCount + " items on the next stage... <color=#FF7F7F>BUT the environment is rendered hostile and barren.</color>\n";

	public override string ItemDescription => "<style=cIsUtility>Consume</style> this item on the next stage. Gain <style=cIsUtility>" + DistributeSingularityItemsServer.maxItemCount + " random items</style> for each player, but render the environment <style=cIsDamage>hostile and barren</style>.";

	public override string ItemLore => null;

	public override ItemTier Tier => ItemTier.Lunar;

	public override ItemTag[] ItemTags => new ItemTag[6]
	{
		ItemTag.AIBlacklist,
		ItemTag.Utility,
		ItemTag.CannotCopy,
		ItemTag.OnStageBeginEffect,
		ItemTag.InteractableRelated,
		ItemTag.HoldoutZoneRelated
	};

	public override Sprite ItemIconSprite => SpikestripContent.GrooveBundle.LoadAsset<Sprite>("texSingularityIcon.png");

	public bool ShouldCorruptCurrentStage()
	{
		int itemCountForTeam = RoR2.Util.GetItemCountForTeam(TeamIndex.Player, ItemDef.itemIndex, requiresAlive: false);
		RoR2.SceneDef sceneDefForCurrentScene = RoR2.SceneCatalog.GetSceneDefForCurrentScene();
		return sceneDefForCurrentScene.sceneType == SceneType.Stage && itemCountForTeam != 0;
	}

	public static bool CurrentStageCorruptedServer()
	{
		return NetworkServer.active && (bool)RoR2.Stage.instance && (bool)RoR2.Stage.instance.GetComponent<SingularityStageAttachmentServer>();
	}

	public override void Init()
	{
		base.Init();
		NetworkingAPI.RegisterMessageType<CorruptedHoldoutZoneController.SyncAddController>();
		effectPrefab = PrefabAPI.InstantiateClone(LegacyResourcesAPI.Load<GameObject>("prefabs/effects/MoonExitArenaOrbEffect"), "SingularityEffect", false);
		RoR2.EffectComponent component = effectPrefab.GetComponent<RoR2.EffectComponent>();
		component.applyScale = true;
		component.soundName = "Play_ui_obj_nullWard_complete";
		Light componentInChildren = effectPrefab.GetComponentInChildren<Light>();
		componentInChildren.color = Color.white;
		effectPrefab.transform.Find("PP").gameObject.SetActive(value: false);
		ParticleSystemRenderer component2 = effectPrefab.transform.Find("Sphere").GetComponent<ParticleSystemRenderer>();
		ParticleSystemRenderer component3 = effectPrefab.transform.Find("BrightFlash").GetComponent<ParticleSystemRenderer>();
		component2.material = component3.material;
		ContentAddition.AddEffect(effectPrefab);
		GameObject gameObject = new GameObject("cursedStagePP");
		corruptedStagePP = PrefabAPI.InstantiateClone(gameObject, "cursedStagePP", true);
		UnityEngine.Object.DestroyImmediate(gameObject);
		corruptedStagePP.layer = RoR2.LayerIndex.postProcess.intVal;
		PostProcessVolume postProcessVolume = corruptedStagePP.AddComponent<PostProcessVolume>();
		postProcessVolume.isGlobal = true;
		postProcessVolume.priority = 10f;
		corruptedStagePP.SetActive(value: true);
		PostProcessProfile postProcessProfile = ScriptableObject.CreateInstance<PostProcessProfile>();
		ColorGrading colorGrading = postProcessProfile.AddSettings<ColorGrading>();
		colorGrading.saturation.Override(-40f);
		RampFog rampFog = postProcessProfile.AddSettings<RampFog>();
		rampFog.fogIntensity.Override(0.5f);
		rampFog.fogColorStart.Override(SpikestripUtil.ColorRGB(255f, 255f, 255f, 0f));
		rampFog.fogColorMid.Override(SpikestripUtil.ColorRGB(255f, 255f, 255f, 0.2f));
		rampFog.fogColorEnd.Override(SpikestripUtil.ColorRGB(255f, 255f, 255f));
		rampFog.skyboxStrength.Override(0.5f);
		rampFog.fogOne.Override(0.5f);
		ChromaticAberration chromaticAberration = postProcessProfile.AddSettings<ChromaticAberration>();
		chromaticAberration.intensity.Override(1.25f);
		PostProcessVolume component4 = corruptedStagePP.GetComponent<PostProcessVolume>();
		component4.sharedProfile = postProcessProfile;
		component4.weight = 1f;
		RoR2.Stage.onServerStageBegin += Stage_onServerStageBegin;
		RoR2.SceneDirector.onPrePopulateSceneServer += SceneDirector_onPrePopulateSceneServer;
		RoR2.CharacterMaster.onStartGlobal += CharacterMaster_onStartGlobal;
		On.RoR2.HoldoutZoneController.Start += HoldoutZoneController_Start;
		On.RoR2.TeleporterInteraction.Start += TeleporterInteraction_Start;
		On.RoR2.Stage.RespawnCharacter += Stage_RespawnCharacter;
	}

	private void Stage_RespawnCharacter(On.RoR2.Stage.orig_RespawnCharacter orig, RoR2.Stage self, RoR2.CharacterMaster characterMaster)
	{
		orig(self, characterMaster);
		if (NetworkServer.active)
		{
			SingularityStageAttachmentServer component = self.GetComponent<SingularityStageAttachmentServer>();
			RoR2.Inventory inventory = characterMaster.inventory;
			if ((bool)component && (bool)inventory && !component.hasSpawnedItemsServer && inventory.GetItemCountEffective(ItemDef) != 0)
			{
				component.hasSpawnedItemsServer = true;
				inventory.RemoveItemPermanent(ItemDef.itemIndex, 1);
				inventory.GiveItemPermanent(SingularityConsumed.instance.ItemDef);
				DistributeSingularityItemsServer.FromMasterServer(characterMaster);
				RoR2.CharacterMasterNotificationQueue.PushItemTransformNotification(characterMaster, ItemDef.itemIndex, SingularityConsumed.instance.ItemDef.itemIndex, RoR2.CharacterMasterNotificationQueue.TransformationType.Default);
			}
		}
	}

	private void TeleporterInteraction_Start(On.RoR2.TeleporterInteraction.orig_Start orig, RoR2.TeleporterInteraction self)
	{
		orig(self);
		if (CurrentStageCorruptedServer())
		{
			self.bossGroup.bonusRewardCount++;
			self.shrineBonusStacks++;
		}
	}

	private void HoldoutZoneController_Start(On.RoR2.HoldoutZoneController.orig_Start orig, RoR2.HoldoutZoneController self)
	{
		orig(self);
		if (CurrentStageCorruptedServer())
		{
			self.gameObject.AddComponent<CorruptedHoldoutZoneController>();
			NetworkIdentity component = self.gameObject.GetComponent<NetworkIdentity>();
			if ((bool)component)
			{
				NetMessageExtensions.Send((INetMessage)(object)new CorruptedHoldoutZoneController.SyncAddController(component.netId), (NetworkDestination)1);
			}
		}
	}

	private void CharacterMaster_onStartGlobal(RoR2.CharacterMaster master)
	{
		if (CurrentStageCorruptedServer() && master.teamIndex == TeamIndex.Monster && master.isBoss && (bool)master.inventory)
		{
			master.inventory.GiveItemPermanent(RoR2.RoR2Content.Items.BoostHp, 5);
		}
	}

	private void SceneDirector_onPrePopulateSceneServer(RoR2.SceneDirector obj)
	{
		if (ShouldCorruptCurrentStage())
		{
			Spikestrip2_0.Log.Info(((object)obj)?.ToString() + ": placing no interactables!");
			obj.interactableCredit = 0;
		}
	}

	private void Stage_onServerStageBegin(RoR2.Stage stage)
	{
		if (ShouldCorruptCurrentStage())
		{
			SingularityStageAttachmentServer singularityStageAttachmentServer = stage.gameObject.AddComponent<SingularityStageAttachmentServer>();
		}
	}

	public override void AssignItemModels()
	{
		Material material = LegacyResourcesAPI.Load<GameObject>("prefabs/networkedobjects/chest/LunarChest").GetComponentInChildren<SkinnedMeshRenderer>().material;
		Material material2 = UnityEngine.Object.Instantiate(material);
		material2.color = SpikestripUtil.ColorRGB(107f, 113f, 127f);
		material2.mainTexture = null;
		material2.EnableKeyword("DITHER");
		ItemPickupModel = SpikestripContent.GrooveBundle.LoadAsset<GameObject>("PickupSingularity.prefab");
		ItemPickupModel.GetComponentInChildren<MeshRenderer>().material = material2;
		Transform transform = ItemPickupModel.transform.Find("mdlSingularity");
		transform.localRotation = RoR2.Util.QuaternionSafeLookRotation(Vector3.forward + Vector3.up * 2f);
		Light componentInChildren = ItemPickupModel.GetComponentInChildren<Light>();
		componentInChildren.intensity = 80f;
		componentInChildren.range = 1f;
		ItemDisplayModel = PrefabAPI.InstantiateClone(new GameObject("DisplaySingularity"), "DisplaySingularity", false);
		RoR2.ItemDisplay itemDisplay = ItemDisplayModel.AddComponent<RoR2.ItemDisplay>();
		itemDisplay.rendererInfos = Array.Empty<RoR2.CharacterModel.RendererInfo>();
		RoR2.ItemFollower itemFollower = ItemDisplayModel.AddComponent<RoR2.ItemFollower>();
		itemFollower.targetObject = ItemDisplayModel;
		itemFollower.followerPrefab = ItemPickupModel;
		itemFollower.distanceDampTime = 0.1f;
		itemFollower.distanceMaxSpeed = 200f;
	}

	public override void PopulateItemDisplayRules(ItemDisplayRuleDict itemDisplayRuleDict)
	{
		itemDisplayRuleDict.Add("mdlCommandoDualies", new RoR2.ItemDisplayRule[1]
		{
			new RoR2.ItemDisplayRule
			{
				ruleType = ItemDisplayRuleType.ParentedPrefab,
				followerPrefab = ItemDisplayModel,
				childName = "Base",
				localPos = new Vector3(0.83165f, -0.08504f, -0.74548f),
				localAngles = new Vector3(0f, 90f, 90f),
				localScale = new Vector3(0.2f, 0.2f, 0.2f)
			}
		});
		itemDisplayRuleDict.Add("mdlHuntress", new RoR2.ItemDisplayRule[1]
		{
			new RoR2.ItemDisplayRule
			{
				ruleType = ItemDisplayRuleType.ParentedPrefab,
				followerPrefab = ItemDisplayModel,
				childName = "Base",
				localPos = new Vector3(0.75448f, -0.07471f, -0.51041f),
				localAngles = new Vector3(0f, 90f, 90f),
				localScale = new Vector3(0.2f, 0.2f, 0.2f)
			}
		});
		itemDisplayRuleDict.Add("mdlToolbot", new RoR2.ItemDisplayRule[1]
		{
			new RoR2.ItemDisplayRule
			{
				ruleType = ItemDisplayRuleType.ParentedPrefab,
				followerPrefab = ItemDisplayModel,
				childName = "Base",
				localPos = new Vector3(-6.06479f, -2.19588f, 5.81212f),
				localAngles = new Vector3(0f, 90f, 90f),
				localScale = new Vector3(0.2f, 0.2f, 0.2f)
			}
		});
		itemDisplayRuleDict.Add("mdlEngi", new RoR2.ItemDisplayRule[1]
		{
			new RoR2.ItemDisplayRule
			{
				ruleType = ItemDisplayRuleType.ParentedPrefab,
				followerPrefab = ItemDisplayModel,
				childName = "Base",
				localPos = new Vector3(0.98999f, -0.20988f, -0.76171f),
				localAngles = new Vector3(0f, 90f, 90f),
				localScale = new Vector3(0.2f, 0.2f, 0.2f)
			}
		});
		itemDisplayRuleDict.Add("mdlMage", new RoR2.ItemDisplayRule[1]
		{
			new RoR2.ItemDisplayRule
			{
				ruleType = ItemDisplayRuleType.ParentedPrefab,
				followerPrefab = ItemDisplayModel,
				childName = "Base",
				localPos = new Vector3(0.80188f, -0.10829f, -0.52509f),
				localAngles = new Vector3(0f, 90f, 90f),
				localScale = new Vector3(0.2f, 0.2f, 0.2f)
			}
		});
		itemDisplayRuleDict.Add("mdlMerc", new RoR2.ItemDisplayRule[1]
		{
			new RoR2.ItemDisplayRule
			{
				ruleType = ItemDisplayRuleType.ParentedPrefab,
				followerPrefab = ItemDisplayModel,
				childName = "Base",
				localPos = new Vector3(0.88968f, -0.11249f, -0.57362f),
				localAngles = new Vector3(0f, 90f, 90f),
				localScale = new Vector3(0.2f, 0.2f, 0.2f)
			}
		});
		itemDisplayRuleDict.Add("mdlTreebot", new RoR2.ItemDisplayRule[1]
		{
			new RoR2.ItemDisplayRule
			{
				ruleType = ItemDisplayRuleType.ParentedPrefab,
				followerPrefab = ItemDisplayModel,
				childName = "Base",
				localPos = new Vector3(1.39138f, -0.40221f, -2.17637f),
				localAngles = new Vector3(0f, 90f, 90f),
				localScale = new Vector3(0.2f, 0.2f, 0.2f)
			}
		});
		itemDisplayRuleDict.Add("mdlLoader", new RoR2.ItemDisplayRule[1]
		{
			new RoR2.ItemDisplayRule
			{
				ruleType = ItemDisplayRuleType.ParentedPrefab,
				followerPrefab = ItemDisplayModel,
				childName = "Base",
				localPos = new Vector3(0.96226f, -0.23438f, -0.56522f),
				localAngles = new Vector3(0f, 90f, 90f),
				localScale = new Vector3(0.2f, 0.2f, 0.2f)
			}
		});
		itemDisplayRuleDict.Add("mdlCroco", new RoR2.ItemDisplayRule[1]
		{
			new RoR2.ItemDisplayRule
			{
				ruleType = ItemDisplayRuleType.ParentedPrefab,
				followerPrefab = ItemDisplayModel,
				childName = "Base",
				localPos = new Vector3(5.73058f, 0.80807f, 4.61064f),
				localAngles = new Vector3(0f, 90f, 90f),
				localScale = new Vector3(0.2f, 0.2f, 0.2f)
			}
		});
		itemDisplayRuleDict.Add("mdlCaptain", new RoR2.ItemDisplayRule[1]
		{
			new RoR2.ItemDisplayRule
			{
				ruleType = ItemDisplayRuleType.ParentedPrefab,
				followerPrefab = ItemDisplayModel,
				childName = "Base",
				localPos = new Vector3(0.94503f, 0.13494f, -0.83048f),
				localAngles = new Vector3(0f, 90f, 90f),
				localScale = new Vector3(0.2f, 0.2f, 0.2f)
			}
		});
		itemDisplayRuleDict.Add("mdlBandit2", new RoR2.ItemDisplayRule[1]
		{
			new RoR2.ItemDisplayRule
			{
				ruleType = ItemDisplayRuleType.ParentedPrefab,
				followerPrefab = ItemDisplayModel,
				childName = "Base",
				localPos = new Vector3(0.21872f, 0.95396f, -0.74463f),
				localAngles = new Vector3(0f, 90f, 90f),
				localScale = new Vector3(0.2f, 0.2f, 0.2f)
			}
		});
		itemDisplayRuleDict.Add("mdlRailGunner", new RoR2.ItemDisplayRule[1]
		{
			new RoR2.ItemDisplayRule
			{
				ruleType = ItemDisplayRuleType.ParentedPrefab,
				followerPrefab = ItemDisplayModel,
				childName = "Base",
				localPos = new Vector3(0.60654f, -0.18994f, -0.61619f),
				localAngles = new Vector3(0f, 90f, 90f),
				localScale = new Vector3(0.2f, 0.2f, 0.2f)
			}
		});
		itemDisplayRuleDict.Add("mdlVoidSurvivor", new RoR2.ItemDisplayRule[1]
		{
			new RoR2.ItemDisplayRule
			{
				ruleType = ItemDisplayRuleType.ParentedPrefab,
				followerPrefab = ItemDisplayModel,
				childName = "Base",
				localPos = new Vector3(1.02322f, 1.12996f, 0.12805f),
				localAngles = new Vector3(0f, 90f, 90f),
				localScale = new Vector3(0.2f, 0.2f, 0.2f)
			}
		});
		itemDisplayRuleDict.Add("mdlScav", new RoR2.ItemDisplayRule[1]
		{
			new RoR2.ItemDisplayRule
			{
				ruleType = ItemDisplayRuleType.ParentedPrefab,
				followerPrefab = ItemDisplayModel,
				childName = "Base",
				localPos = new Vector3(-14.30001f, 2.92203f, 14.97974f),
				localAngles = new Vector3(0f, 90f, 90f),
				localScale = new Vector3(0.2f, 0.2f, 0.2f)
			}
		});
		itemDisplayRuleDict.Add("mdlEngiTurret", new RoR2.ItemDisplayRule[1]
		{
			new RoR2.ItemDisplayRule
			{
				ruleType = ItemDisplayRuleType.ParentedPrefab,
				followerPrefab = ItemDisplayModel,
				childName = "Base",
				localPos = new Vector3(2.40124f, 3.78879f, -0.75998f),
				localAngles = new Vector3(0f, 90f, 90f),
				localScale = new Vector3(0.2f, 0.2f, 0.2f)
			}
		});
	}
}
