using System;
using R2API;
using RoR2;
using RoR2.CharacterAI;
using RoR2.Navigation;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Networking;
using Spikestrip2_0.Core;

namespace GrooveSaladSpikestripContent.Content;

public class BlueLemurian : ItemBase<BlueLemurian>
{
	public class BlueLemurianController : MonoBehaviour
	{
	}

	public class BlueLemurianBehaviourServer : CharacterBody.ItemBehavior
	{
		private float resummonCooldown;

		private Deployable deployableInstance = null;

		public void FixedUpdate()
		{
			if (body.master.GetDeployableCount(BlueLemurianDeployableSlot) == 0)
			{
				resummonCooldown -= Time.fixedDeltaTime;
				if (resummonCooldown <= 0f)
				{
					DirectorSpawnRequest directorSpawnRequest = new DirectorSpawnRequest(blueLemurianSpawnCard, new DirectorPlacementRule
					{
						placementMode = DirectorPlacementRule.PlacementMode.Approximate,
						minDistance = 3f,
						maxDistance = 40f,
						spawnOnTarget = base.transform
					}, RoR2Application.rng);
					directorSpawnRequest.summonerBodyObject = base.gameObject;
					directorSpawnRequest.onSpawnedServer = ModifiySpawnedBlueLemurian;
					DirectorCore.instance.TrySpawnObject(directorSpawnRequest);
					resummonCooldown = 10f;
				}
			}
		}

		private void ModifiySpawnedBlueLemurian(SpawnCard.SpawnResult spawnResult)
		{
			GameObject spawnedInstance = spawnResult.spawnedInstance;
			if (!spawnedInstance)
			{
				return;
			}
			CharacterMaster component = spawnedInstance.GetComponent<CharacterMaster>();
			if ((bool)component)
			{
				component.inventory.GiveItemPermanent(RoR2Content.Items.BoostDamage, (int)((float)stack * damageBonusPerStack + damageBonusBase) * 10);
				component.inventory.GiveItemPermanent(RoR2Content.Items.BoostHp, (int)((float)stack * healthBonusPerStack + healthBonusBase) * 10);
				deployableInstance = component.GetComponent<Deployable>();
				if ((bool)deployableInstance)
				{
					deployableInstance.onUndeploy = new UnityEvent();
					deployableInstance.onUndeploy.AddListener(component.TrueKill);
					body.master.AddDeployable(deployableInstance, BlueLemurianDeployableSlot);
				}
			}
		}

		public void OnDestroy()
		{
			if ((bool)deployableInstance)
			{
				deployableInstance.onUndeploy.Invoke();
			}
		}
	}

	public static DeployableAPI.GetDeployableSameSlotLimit GetBlueLemurianSlotLimit = (DeployableAPI.GetDeployableSameSlotLimit)((CharacterMaster self, int multiplier) => multiplier);

	public static DeployableSlot BlueLemurianDeployableSlot = DeployableAPI.RegisterDeployableSlot(GetBlueLemurianSlotLimit);

	public static GameObject blueLemurianMasterObject;

	public static GameObject blueLemurianBodyObject;

	public static CharacterSpawnCard blueLemurianSpawnCard;

	public static Material matBlueLemurian;

	public static float healthBonusBase = 2f;

	public static float damageBonusBase = 0f;

	public static float healthBonusPerStack = 1f;

	public static float damageBonusPerStack = 1f;

	public override string ItemToken => "BLUELEMURIAN";

	public override string ItemName => "Smouldering Comet";

	public override string ItemPickup => "Recruit a friendly Blue Lemurian that drops Lunar Coins on kills.";

	public override string ItemDescription => "<style=cIsUtility>Recruit a friendly Blue Lemurian</style> with bonus <style=cIsDamage>" + (damageBonusPerStack + damageBonusBase) * 100f + "% <style=cStack>(+" + damageBonusPerStack * 100f + "% per stack)</style> damage</style> and <style=cIsHealing>" + (healthBonusPerStack + healthBonusBase) * 100f + "% <style=cStack>(+" + healthBonusPerStack * 100f + "% per stack)</style> health</style>. Enemies slain by the Blue Lemurian are <style=cIsUtility>guaranteed to drop Lunar Coins</style>.";

	public override string ItemLore => null;

	public override ItemTier Tier => ItemTier.Tier2;

	public override ItemTag[] ItemTags => new ItemTag[3]
	{
		ItemTag.Damage,
		ItemTag.Utility,
		ItemTag.AIBlacklist
	};

	public override Sprite ItemIconSprite => SpikestripContent.GrooveBundle.LoadAsset<Sprite>("texCometIcon.png");

	public override Type ItemBehaviour => typeof(BlueLemurianBehaviourServer);

	public override bool ServerOnlyBehaviour => true;

	public override void Init()
	{
		base.Init();
		blueLemurianMasterObject = PrefabAPI.InstantiateClone(LegacyResourcesAPI.Load<GameObject>("prefabs/charactermasters/LemurianMaster"), "BlueLemurianMaster", true);
		Deployable deployable = blueLemurianMasterObject.AddComponent<Deployable>();
		blueLemurianBodyObject = PrefabAPI.InstantiateClone(LegacyResourcesAPI.Load<GameObject>("prefabs/characterbodies/LemurianBody"), "BlueLemurianBody", true);
		blueLemurianBodyObject.AddComponent<BlueLemurianController>();
		ModelLocator component = blueLemurianBodyObject.GetComponent<ModelLocator>();
		// The Lemurian's mesh, material, animator controller and avatar now come from its ModelSkinController at runtime.
		// Bake them into the prefab (which also removes the skin controller, so it can't swap the blue material back).
		GameObject lemurianModel = LegacyResourcesAPI.Load<GameObject>("prefabs/characterbodies/LemurianBody").GetComponent<ModelLocator>().modelTransform.gameObject;
		SpikestripUtil.BakeDefaultSkin(lemurianModel, component.modelTransform.gameObject);
		Transform transform = component.modelTransform.Find("LemurianMesh");
		SkinnedMeshRenderer component2 = transform.GetComponent<SkinnedMeshRenderer>();
		matBlueLemurian = UnityEngine.Object.Instantiate(component2.sharedMaterial);
		matBlueLemurian.color = SpikestripUtil.ColorRGB(180f, 233f, 255f);
		matBlueLemurian.SetTexture("_EmTex", SpikestripContent.GrooveBundle.LoadAsset<Texture>("texBlueLemurianEmission.png"));
		CharacterModel component3 = component.modelTransform.GetComponent<CharacterModel>();
		component3.baseRendererInfos = new CharacterModel.RendererInfo[1]
		{
			new CharacterModel.RendererInfo
			{
				renderer = component2,
				defaultMaterial = matBlueLemurian,
				defaultShadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.On,
				ignoreOverlays = false
			}
		};
		component2.sharedMaterial = matBlueLemurian;
		CharacterBody component4 = blueLemurianBodyObject.GetComponent<CharacterBody>();
		component4.baseMoveSpeed = 12f;
		component4.baseRegen = 1.5f;
		component4.levelRegen = 0.3f;
		component4.portraitIcon = SpikestripContent.GrooveBundle.LoadAsset<Texture>("texBlueLemurianIcon");
		component4.baseNameToken = "BLUELEMURIAN_BODY_NAME";
		LanguageAPI.Add(component4.baseNameToken, "Blue Lemurian");
		DeathRewards component5 = blueLemurianBodyObject.GetComponent<DeathRewards>();
		component5.logUnlockableDef = null;
		CharacterMaster component6 = blueLemurianMasterObject.GetComponent<CharacterMaster>();
		component6.bodyPrefab = blueLemurianBodyObject;
		CreateAI();
		ContentAddition.AddMaster(blueLemurianMasterObject);
		PrefabAPI.RegisterNetworkPrefab(blueLemurianMasterObject);
		ContentAddition.AddBody(blueLemurianBodyObject);
		PrefabAPI.RegisterNetworkPrefab(blueLemurianBodyObject);
		blueLemurianSpawnCard = ScriptableObject.CreateInstance<CharacterSpawnCard>();
		blueLemurianSpawnCard.prefab = blueLemurianMasterObject;
		blueLemurianSpawnCard.sendOverNetwork = true;
		blueLemurianSpawnCard.hullSize = HullClassification.Human;
		blueLemurianSpawnCard.nodeGraphType = MapNodeGroup.GraphType.Ground;
		blueLemurianSpawnCard.requiredFlags = NodeFlags.None;
		blueLemurianSpawnCard.forbiddenFlags = NodeFlags.NoCharacterSpawn;
		blueLemurianSpawnCard.directorCreditCost = 0;
		blueLemurianSpawnCard.occupyPosition = false;
		blueLemurianSpawnCard.eliteRules = SpawnCard.EliteRules.Default;
		blueLemurianSpawnCard.noElites = false;
		blueLemurianSpawnCard.forbiddenAsBoss = false;
		GlobalEventManager.onCharacterDeathGlobal += GlobalEventManager_onCharacterDeathGlobal;
	}

	public void CreateAI()
	{
		AISkillDriver[] components = blueLemurianMasterObject.GetComponents<AISkillDriver>();
		for (int num = components.Length - 1; num >= 0; num--)
		{
			if (components[num].customName != "ChaseAndBiteOffNodegraphWhileSlowingDown" && components[num].customName != "ChaseAndBiteOffNodegraph")
			{
				UnityEngine.Object.Destroy(components[num]);
			}
			else
			{
				components[num].minUserHealthFraction = 0.3f;
			}
		}
		AISkillDriver aISkillDriver = blueLemurianMasterObject.AddComponent<AISkillDriver>();
		aISkillDriver.customName = "ReturnToOwnerLeash";
		aISkillDriver.minDistance = 100f;
		aISkillDriver.moveTargetType = AISkillDriver.TargetType.CurrentLeader;
		aISkillDriver.movementType = AISkillDriver.MovementType.ChaseMoveTarget;
		aISkillDriver.aimType = AISkillDriver.AimType.AtCurrentLeader;
		aISkillDriver.driverUpdateTimerOverride = 3f;
		aISkillDriver.resetCurrentEnemyOnNextDriverSelection = true;
		aISkillDriver.skillSlot = SkillSlot.None;
		AISkillDriver aISkillDriver2 = blueLemurianMasterObject.AddComponent<AISkillDriver>();
		aISkillDriver2.customName = "EngageMelee";
		aISkillDriver2.skillSlot = SkillSlot.None;
		aISkillDriver2.minDistance = 6f;
		aISkillDriver2.maxDistance = 15f;
		aISkillDriver2.aimType = AISkillDriver.AimType.AtMoveTarget;
		aISkillDriver2.activationRequiresAimConfirmation = true;
		aISkillDriver2.movementType = AISkillDriver.MovementType.ChaseMoveTarget;
		aISkillDriver2.moveTargetType = AISkillDriver.TargetType.CurrentEnemy;
		aISkillDriver2.minUserHealthFraction = 0.3f;
		AISkillDriver aISkillDriver3 = blueLemurianMasterObject.AddComponent<AISkillDriver>();
		aISkillDriver3.customName = "StrafeAndShoot";
		aISkillDriver3.skillSlot = SkillSlot.Primary;
		aISkillDriver3.requireSkillReady = true;
		aISkillDriver3.minDistance = 15f;
		aISkillDriver3.maxDistance = 35f;
		aISkillDriver3.selectionRequiresTargetLoS = true;
		aISkillDriver3.aimType = AISkillDriver.AimType.AtMoveTarget;
		aISkillDriver3.activationRequiresAimConfirmation = true;
		aISkillDriver3.movementType = AISkillDriver.MovementType.StrafeMovetarget;
		aISkillDriver3.moveTargetType = AISkillDriver.TargetType.CurrentEnemy;
		AISkillDriver aISkillDriver4 = blueLemurianMasterObject.AddComponent<AISkillDriver>();
		aISkillDriver4.customName = "IdleStrafe";
		aISkillDriver4.minDistance = 15f;
		aISkillDriver4.maxDistance = 35f;
		aISkillDriver4.selectionRequiresTargetLoS = true;
		aISkillDriver4.moveTargetType = AISkillDriver.TargetType.CurrentEnemy;
		aISkillDriver4.movementType = AISkillDriver.MovementType.StrafeMovetarget;
		aISkillDriver4.aimType = AISkillDriver.AimType.AtMoveTarget;
		aISkillDriver4.skillSlot = SkillSlot.None;
		AISkillDriver aISkillDriver5 = blueLemurianMasterObject.AddComponent<AISkillDriver>();
		aISkillDriver5.customName = "FleeLowHealth";
		aISkillDriver5.maxDistance = 15f;
		aISkillDriver5.moveTargetType = AISkillDriver.TargetType.CurrentEnemy;
		aISkillDriver5.movementType = AISkillDriver.MovementType.FleeMoveTarget;
		aISkillDriver5.aimType = AISkillDriver.AimType.MoveDirection;
		aISkillDriver5.skillSlot = SkillSlot.None;
		aISkillDriver5.maxUserHealthFraction = 0.3f;
		AISkillDriver aISkillDriver6 = blueLemurianMasterObject.AddComponent<AISkillDriver>();
		aISkillDriver6.customName = "ReturnToLeaderFinal";
		aISkillDriver6.minDistance = 15f;
		aISkillDriver6.moveTargetType = AISkillDriver.TargetType.CurrentLeader;
		aISkillDriver6.movementType = AISkillDriver.MovementType.ChaseMoveTarget;
		aISkillDriver6.aimType = AISkillDriver.AimType.AtMoveTarget;
		aISkillDriver6.skillSlot = SkillSlot.None;
		AISkillDriver aISkillDriver7 = blueLemurianMasterObject.AddComponent<AISkillDriver>();
		aISkillDriver7.customName = "IdleByLeader";
		aISkillDriver7.aimType = AISkillDriver.AimType.AtCurrentLeader;
		aISkillDriver7.moveTargetType = AISkillDriver.TargetType.CurrentLeader;
		aISkillDriver7.movementType = AISkillDriver.MovementType.Stop;
		aISkillDriver7.skillSlot = SkillSlot.None;
	}

	private void GlobalEventManager_onCharacterDeathGlobal(DamageReport damageReport)
	{
		if (NetworkServer.active)
		{
			GameObject attacker = damageReport.attacker;
			if ((bool)attacker && (bool)attacker.GetComponent<BlueLemurianController>())
			{
				PickupDropletController.CreatePickupDroplet(PickupCatalog.FindPickupIndex("LunarCoin.Coin0"), damageReport.victim.transform.position, Vector3.up * 10f);
			}
		}
	}

	public override void AssignItemModels()
	{
		ItemPickupModel = PrefabAPI.InstantiateClone(SpikestripContent.GrooveBundle.LoadAsset<GameObject>("PickupComet.prefab"), "PickupComet", false);
		MeshRenderer componentInChildren = ItemPickupModel.GetComponentInChildren<MeshRenderer>();
		Material material = LegacyResourcesAPI.Load<GameObject>("prefabs/pickupmodels/PickupPearl").GetComponentInChildren<MeshRenderer>().material;
		Material material2 = UnityEngine.Object.Instantiate(material);
		material2.color = SpikestripUtil.ColorRGB(101f, 104f, 137f);
		material2.DisableKeyword("SPLATMAP");
		GameObject gameObject = UnityEngine.Object.Instantiate(LegacyResourcesAPI.Load<GameObject>("prefabs/pickupmodels/PickupLunarCoin").transform.Find("Coin5Mesh/Particle System").gameObject, ItemPickupModel.transform);
		ParticleSystem component = gameObject.GetComponent<ParticleSystem>();
		ParticleSystem.MainModule main = component.main;
		main.scalingMode = ParticleSystemScalingMode.Hierarchy;
		gameObject.transform.localScale = new Vector3(1.5f, 1.5f, 2f);
		gameObject.transform.localPosition = new Vector3(0f, 0.5f, 0.3f);
		GameObject gameObject2 = UnityEngine.Object.Instantiate(LegacyResourcesAPI.Load<GameObject>("prefabs/pickupmodels/PickupLunarCoin").transform.Find("Coin5Mesh/Particle System").gameObject, ItemPickupModel.transform);
		ParticleSystem component2 = gameObject2.GetComponent<ParticleSystem>();
		ParticleSystem.MainModule main2 = component2.main;
		main2.scalingMode = ParticleSystemScalingMode.Hierarchy;
		gameObject2.transform.localScale = new Vector3(1.5f, 1.5f, 2f);
		gameObject2.transform.localPosition = new Vector3(0f, 0.5f, 0.3f);
		Material material3 = LegacyResourcesAPI.Load<GameObject>("prefabs/projectileghosts/SawmerangGhost").transform.Find("Mesh/mdlVFXDonut2").GetComponent<MeshRenderer>().sharedMaterials[1];
		gameObject2.GetComponent<ParticleSystemRenderer>().material = material3;
		componentInChildren.material = material2;
		ItemDisplayModel = SpikestripUtil.EmptyPrefab("DisplayCometParent");
		ItemDisplay itemDisplay = ItemDisplayModel.AddComponent<ItemDisplay>();
		itemDisplay.rendererInfos = Array.Empty<CharacterModel.RendererInfo>();
		ItemFollower itemFollower = ItemDisplayModel.AddComponent<ItemFollower>();
		itemFollower.targetObject = ItemDisplayModel;
		itemFollower.followerPrefab = ItemPickupModel;
		itemFollower.distanceDampTime = 0.15f;
		itemFollower.distanceMaxSpeed = 200f;
	}

	public override void PopulateItemDisplayRules(ItemDisplayRuleDict itemDisplayRuleDict)
	{
		itemDisplayRuleDict.Add("mdlCommandoDualies", new ItemDisplayRule[1]
		{
			new ItemDisplayRule
			{
				ruleType = ItemDisplayRuleType.ParentedPrefab,
				followerPrefab = ItemDisplayModel,
				childName = "Base",
				localPos = new Vector3(-0.47388f, 0.28232f, -0.74573f),
				localAngles = new Vector3(90f, 161f, 0f),
				localScale = new Vector3(0.1f, 0.1f, 0.1f)
			}
		});
		itemDisplayRuleDict.Add("mdlHuntress", new ItemDisplayRule[1]
		{
			new ItemDisplayRule
			{
				ruleType = ItemDisplayRuleType.ParentedPrefab,
				followerPrefab = ItemDisplayModel,
				childName = "Base",
				localPos = new Vector3(-0.3799f, 0.12539f, -0.5105f),
				localAngles = new Vector3(90f, 161f, 0f),
				localScale = new Vector3(0.1f, 0.1f, 0.1f)
			}
		});
		itemDisplayRuleDict.Add("mdlToolbot", new ItemDisplayRule[1]
		{
			new ItemDisplayRule
			{
				ruleType = ItemDisplayRuleType.ParentedPrefab,
				followerPrefab = ItemDisplayModel,
				childName = "Base",
				localPos = new Vector3(4.13126f, 1.57749f, 5.81278f),
				localAngles = new Vector3(90f, 161f, 0f),
				localScale = new Vector3(0.14379f, 0.14379f, 0.14379f)
			}
		});
		itemDisplayRuleDict.Add("mdlEngi", new ItemDisplayRule[1]
		{
			new ItemDisplayRule
			{
				ruleType = ItemDisplayRuleType.ParentedPrefab,
				followerPrefab = ItemDisplayModel,
				childName = "Base",
				localPos = new Vector3(-0.84157f, 0.10333f, -0.91052f),
				localAngles = new Vector3(90f, 161f, 0f),
				localScale = new Vector3(0.13107f, 0.13107f, 0.13107f)
			}
		});
		itemDisplayRuleDict.Add("mdlMage", new ItemDisplayRule[1]
		{
			new ItemDisplayRule
			{
				ruleType = ItemDisplayRuleType.ParentedPrefab,
				followerPrefab = ItemDisplayModel,
				childName = "Base",
				localPos = new Vector3(-0.36147f, -0.10829f, -0.52512f),
				localAngles = new Vector3(90f, 161f, 0f),
				localScale = new Vector3(0.09723f, 0.09723f, 0.09723f)
			}
		});
		itemDisplayRuleDict.Add("mdlMerc", new ItemDisplayRule[1]
		{
			new ItemDisplayRule
			{
				ruleType = ItemDisplayRuleType.ParentedPrefab,
				followerPrefab = ItemDisplayModel,
				childName = "Base",
				localPos = new Vector3(-0.48162f, 0.03988f, -0.57371f),
				localAngles = new Vector3(90f, 161f, 0f),
				localScale = new Vector3(0.1f, 0.1f, 0.1f)
			}
		});
		itemDisplayRuleDict.Add("mdlTreebot", new ItemDisplayRule[1]
		{
			new ItemDisplayRule
			{
				ruleType = ItemDisplayRuleType.ParentedPrefab,
				followerPrefab = ItemDisplayModel,
				childName = "Base",
				localPos = new Vector3(-1.16754f, 0.21993f, -2.00235f),
				localAngles = new Vector3(90f, 161f, 0f),
				localScale = new Vector3(0.20793f, 0.20793f, 0.20793f)
			}
		});
		itemDisplayRuleDict.Add("mdlLoader", new ItemDisplayRule[1]
		{
			new ItemDisplayRule
			{
				ruleType = ItemDisplayRuleType.ParentedPrefab,
				followerPrefab = ItemDisplayModel,
				childName = "Base",
				localPos = new Vector3(-0.66794f, -0.0829f, -0.56528f),
				localAngles = new Vector3(90f, 161f, 0f),
				localScale = new Vector3(0.12612f, 0.12612f, 0.12612f)
			}
		});
		itemDisplayRuleDict.Add("mdlCroco", new ItemDisplayRule[1]
		{
			new ItemDisplayRule
			{
				ruleType = ItemDisplayRuleType.ParentedPrefab,
				followerPrefab = ItemDisplayModel,
				childName = "Base",
				localPos = new Vector3(-4.89192f, -2.91616f, 5.46685f),
				localAngles = new Vector3(90f, 0f, 0f),
				localScale = new Vector3(0.2386f, 0.2386f, 0.2386f)
			}
		});
		itemDisplayRuleDict.Add("mdlCaptain", new ItemDisplayRule[1]
		{
			new ItemDisplayRule
			{
				ruleType = ItemDisplayRuleType.ParentedPrefab,
				followerPrefab = ItemDisplayModel,
				childName = "Base",
				localPos = new Vector3(-0.62068f, 0.06768f, -0.83758f),
				localAngles = new Vector3(90f, 161f, 0f),
				localScale = new Vector3(0.13429f, 0.13429f, 0.13429f)
			}
		});
		itemDisplayRuleDict.Add("mdlBandit2", new ItemDisplayRule[1]
		{
			new ItemDisplayRule
			{
				ruleType = ItemDisplayRuleType.ParentedPrefab,
				followerPrefab = ItemDisplayModel,
				childName = "Base",
				localPos = new Vector3(-0.08996f, -0.2715f, -0.61259f),
				localAngles = new Vector3(90f, 161f, 0f),
				localScale = new Vector3(0.11187f, 0.11187f, 0.11187f)
			}
		});
		itemDisplayRuleDict.Add("mdlRailGunner", new ItemDisplayRule[1]
		{
			new ItemDisplayRule
			{
				ruleType = ItemDisplayRuleType.ParentedPrefab,
				followerPrefab = ItemDisplayModel,
				childName = "Base",
				localPos = new Vector3(-0.43685f, -0.16056f, -0.27313f),
				localAngles = new Vector3(90f, 161f, 0f),
				localScale = new Vector3(0.11187f, 0.11187f, 0.11187f)
			}
		});
		itemDisplayRuleDict.Add("mdlVoidSurvivor", new ItemDisplayRule[1]
		{
			new ItemDisplayRule
			{
				ruleType = ItemDisplayRuleType.ParentedPrefab,
				followerPrefab = ItemDisplayModel,
				childName = "Base",
				localPos = new Vector3(-0.34489f, 0.34979f, 0.31015f),
				localAngles = new Vector3(50.65422f, 3.82423f, 15.17577f),
				localScale = new Vector3(0.11187f, 0.11187f, 0.11187f)
			}
		});
		itemDisplayRuleDict.Add("mdlScav", new ItemDisplayRule[1]
		{
			new ItemDisplayRule
			{
				ruleType = ItemDisplayRuleType.ParentedPrefab,
				followerPrefab = ItemDisplayModel,
				childName = "Base",
				localPos = new Vector3(-6.95107f, 3.02618f, 11.47887f),
				localAngles = new Vector3(0f, 90f, 90f),
				localScale = new Vector3(0.37861f, 0.37861f, 0.37861f)
			}
		});
		itemDisplayRuleDict.Add("mdlEngiTurret", new ItemDisplayRule[1]
		{
			new ItemDisplayRule
			{
				ruleType = ItemDisplayRuleType.ParentedPrefab,
				followerPrefab = ItemDisplayModel,
				childName = "Base",
				localPos = new Vector3(-1.68009f, 3.21618f, -0.29491f),
				localAngles = new Vector3(0f, 0f, 0f),
				localScale = new Vector3(0.21709f, 0.21709f, 0.21709f)
			}
		});
	}
}
