using System.Collections;
using System.Collections.Generic;
using R2API;
using RoR2;
using UnityEngine;
using Spikestrip2_0.Core;

namespace GrooveSaladSpikestripContent.Content;

public class Arson : ItemBase<Arson>
{
	public static List<ItemDef> gasolineItemDefs = new List<ItemDef> { SpikestripUtil.LegacyLoad<ItemDef>("ItemDefs/IgniteOnKill") };

	public override string ItemToken => "ARSON";

	public override string ItemName => "Arsonist's Satchel";

	public override string ItemPickup => "Gain random 'Gasoline' items at the start of each stage.";

	public override string ItemDescription => "Gain <style=cIsUtility>2</style> <style=cStack>(+1 per stack)</style> random <style=cIsUtility>'Gasoline' items</style> at the start of each stage.";

	public override string ItemLore => null;

	public override ItemTier Tier => ItemTier.Tier2;

	public override ItemTag[] ItemTags => new ItemTag[5]
	{
		ItemTag.Utility,
		ItemTag.OnStageBeginEffect,
		ItemTag.AIBlacklist,
		ItemTag.BrotherBlacklist,
		ItemTag.CannotCopy
	};

	public override Sprite ItemIconSprite => SpikestripContent.GrooveBundle.LoadAsset<Sprite>("texArsonIcon.png");


	public override void Init()
	{
		base.Init();
		Stage.onServerStageBegin += Stage_onServerStageBegin;
	}

	private void Stage_onServerStageBegin(Stage stage)
	{
		for (int i = 0; i < PlayerCharacterMasterController.instances.Count; i++)
		{
			PlayerCharacterMasterController playerCharacterMasterController = PlayerCharacterMasterController.instances[i];
			if (!playerCharacterMasterController)
			{
				continue;
			}
			Inventory inventory = (playerCharacterMasterController.master ? playerCharacterMasterController.master.inventory : null);
			if ((bool)inventory)
			{
				int itemCount = inventory.GetItemCountEffective(ItemDef);
				if (itemCount > 0)
				{
					int count = SpikestripUtil.StackScaling(2, 1, itemCount);
					((MonoBehaviour)playerCharacterMasterController.master).StartCoroutine(GrantGasolineServer(playerCharacterMasterController.master, inventory, count));
				}
			}
		}
	}

	public IEnumerator GrantGasolineServer(CharacterMaster master, Inventory inventory, int count)
	{
		while (count > 0)
		{
			yield return new WaitForSeconds(1f);
			ItemDef selectedItemDef = Run.instance.treasureRng.NextElementUniform(gasolineItemDefs);
			inventory.GiveItemPermanent(selectedItemDef);
			PickupIndex pickupIndex = PickupCatalog.FindPickupIndex(selectedItemDef.itemIndex);
			if ((bool)master)
			{
				GenericPickupController.SendPickupMessage(master, new RoR2.UniquePickup { pickupIndex = pickupIndex });
			}
			count--;
		}
	}

	public override void AssignItemModels()
	{
		ItemPickupModel = SpikestripUtil.UpdateGameObjectShader(SpikestripContent.GrooveBundle.LoadAsset<GameObject>("ArsonistsSatchelPickup.prefab"));
		Transform transform = ItemPickupModel.transform.Find("SatchelPickupGlass");
		transform.GetComponent<MeshRenderer>().material = SpikestripUtil.infusionGlassMaterial;
		ItemDisplayModel = SpikestripUtil.UpdateGameObjectShader(SpikestripContent.GrooveBundle.LoadAsset<GameObject>("ArsonistsSatchelDisplay.prefab"));
		Transform transform2 = ItemDisplayModel.transform.Find("SatchelDisplay2Glass");
		transform2.GetComponent<MeshRenderer>().material = SpikestripUtil.infusionGlassMaterial;
		ItemDisplay itemDisplay = ItemDisplayModel.AddComponent<ItemDisplay>();
		itemDisplay.rendererInfos = SpikestripUtil.GenerateRendererInfos(ItemDisplayModel);
	}

	public override void PopulateItemDisplayRules(ItemDisplayRuleDict itemDisplayRuleDict)
	{
		itemDisplayRuleDict.Add("mdlCommandoDualies", new ItemDisplayRule[1]
		{
			new ItemDisplayRule
			{
				ruleType = ItemDisplayRuleType.ParentedPrefab,
				followerPrefab = ItemDisplayModel,
				childName = "Chest",
				localPos = new Vector3(0.23606f, -0.2273f, -0.04909f),
				localAngles = new Vector3(2.5127f, 269.1554f, 356.4477f),
				localScale = new Vector3(0.30388f, 0.30388f, 0.30388f)
			}
		});
		itemDisplayRuleDict.Add("mdlHuntress", new ItemDisplayRule[1]
		{
			new ItemDisplayRule
			{
				ruleType = ItemDisplayRuleType.ParentedPrefab,
				followerPrefab = ItemDisplayModel,
				childName = "Chest",
				localPos = new Vector3(0.17173f, -0.29898f, 0.15772f),
				localAngles = new Vector3(1.45841f, 247.5903f, 12.2867f),
				localScale = new Vector3(0.27634f, 0.27634f, 0.27634f)
			}
		});
		itemDisplayRuleDict.Add("mdlToolbot", new ItemDisplayRule[1]
		{
			new ItemDisplayRule
			{
				ruleType = ItemDisplayRuleType.ParentedPrefab,
				followerPrefab = ItemDisplayModel,
				childName = "Chest",
				localPos = new Vector3(3.35741f, -1.79558f, 0.26201f),
				localAngles = new Vector3(4.6551f, 266.0097f, 4.26354f),
				localScale = new Vector3(2.44493f, 2.44493f, 2.44493f)
			}
		});
		itemDisplayRuleDict.Add("mdlEngi", new ItemDisplayRule[1]
		{
			new ItemDisplayRule
			{
				ruleType = ItemDisplayRuleType.ParentedPrefab,
				followerPrefab = ItemDisplayModel,
				childName = "Chest",
				localPos = new Vector3(0.26784f, -0.46835f, -0.0937f),
				localAngles = new Vector3(359.8552f, 276.7462f, 358.8177f),
				localScale = new Vector3(0.41169f, 0.41169f, 0.41169f)
			}
		});
		itemDisplayRuleDict.Add("mdlMage", new ItemDisplayRule[1]
		{
			new ItemDisplayRule
			{
				ruleType = ItemDisplayRuleType.ParentedPrefab,
				followerPrefab = ItemDisplayModel,
				childName = "Chest",
				localPos = new Vector3(0.22036f, -0.30703f, -0.16499f),
				localAngles = new Vector3(5.78869f, 286.6394f, 357.0594f),
				localScale = new Vector3(0.27919f, 0.27919f, 0.27919f)
			}
		});
		itemDisplayRuleDict.Add("mdlMerc", new ItemDisplayRule[1]
		{
			new ItemDisplayRule
			{
				ruleType = ItemDisplayRuleType.ParentedPrefab,
				followerPrefab = ItemDisplayModel,
				childName = "Chest",
				localPos = new Vector3(0.25888f, -0.2089f, -0.00476f),
				localAngles = new Vector3(11.96696f, 283.2347f, 3.86942f),
				localScale = new Vector3(0.26217f, 0.26217f, 0.26217f)
			}
		});
		itemDisplayRuleDict.Add("mdlTreebot", new ItemDisplayRule[1]
		{
			new ItemDisplayRule
			{
				ruleType = ItemDisplayRuleType.ParentedPrefab,
				followerPrefab = ItemDisplayModel,
				childName = "PlatformBase",
				localPos = new Vector3(-1.00588f, -0.02891f, -0.205f),
				localAngles = new Vector3(354.2916f, 97.05015f, 354.2017f),
				localScale = new Vector3(0.40692f, 0.40692f, 0.40692f)
			}
		});
		itemDisplayRuleDict.Add("mdlLoader", new ItemDisplayRule[1]
		{
			new ItemDisplayRule
			{
				ruleType = ItemDisplayRuleType.ParentedPrefab,
				followerPrefab = ItemDisplayModel,
				childName = "MechUpperArmL",
				localPos = new Vector3(0.10299f, 0.61095f, 0.22914f),
				localAngles = new Vector3(20.13691f, 280.0426f, 151.4142f),
				localScale = new Vector3(0.26758f, 0.26758f, 0.26758f)
			}
		});
		itemDisplayRuleDict.Add("mdlCroco", new ItemDisplayRule[1]
		{
			new ItemDisplayRule
			{
				ruleType = ItemDisplayRuleType.ParentedPrefab,
				followerPrefab = ItemDisplayModel,
				childName = "SpineChest2",
				localPos = new Vector3(-0.29003f, 2.3309f, 2.78043f),
				localAngles = new Vector3(85.39041f, 149.4303f, 337.5902f),
				localScale = new Vector3(1.61891f, 1.61891f, 1.61891f)
			}
		});
		itemDisplayRuleDict.Add("mdlCaptain", new ItemDisplayRule[1]
		{
			new ItemDisplayRule
			{
				ruleType = ItemDisplayRuleType.ParentedPrefab,
				followerPrefab = ItemDisplayModel,
				childName = "Chest",
				localPos = new Vector3(0.32244f, -0.13995f, -0.08176f),
				localAngles = new Vector3(20.97677f, 289.485f, 7.6021f),
				localScale = new Vector3(0.34443f, 0.34443f, 0.34443f)
			}
		});
		itemDisplayRuleDict.Add("mdlBandit2", new ItemDisplayRule[1]
		{
			new ItemDisplayRule
			{
				ruleType = ItemDisplayRuleType.ParentedPrefab,
				followerPrefab = ItemDisplayModel,
				childName = "Chest",
				localPos = new Vector3(0.18257f, -0.11415f, 0.02641f),
				localAngles = new Vector3(359.8872f, 269.6226f, 6.8642f),
				localScale = new Vector3(0.23078f, 0.23078f, 0.23078f)
			}
		});
		itemDisplayRuleDict.Add("mdlRailGunner", new ItemDisplayRule[1]
		{
			new ItemDisplayRule
			{
				ruleType = ItemDisplayRuleType.ParentedPrefab,
				followerPrefab = ItemDisplayModel,
				childName = "Backpack",
				localPos = new Vector3(-0.00596f, -0.38906f, -0.08976f),
				localAngles = new Vector3(350.9116f, 357.3613f, 359.0928f),
				localScale = new Vector3(0.30691f, 0.27502f, 0.30691f)
			}
		});
		itemDisplayRuleDict.Add("mdlVoidSurvivor", new ItemDisplayRule[1]
		{
			new ItemDisplayRule
			{
				ruleType = ItemDisplayRuleType.ParentedPrefab,
				followerPrefab = ItemDisplayModel,
				childName = "Chest",
				localPos = new Vector3(0.32079f, -0.14121f, -0.11763f),
				localAngles = new Vector3(27.74245f, 277.7913f, 2.221f),
				localScale = new Vector3(0.2915f, 0.2915f, 0.2915f)
			}
		});
		itemDisplayRuleDict.Add("mdlScav", new ItemDisplayRule[1]
		{
			new ItemDisplayRule
			{
				ruleType = ItemDisplayRuleType.ParentedPrefab,
				followerPrefab = ItemDisplayModel,
				childName = "Backpack",
				localPos = new Vector3(-11.76367f, 8.26526f, 1.85352f),
				localAngles = new Vector3(-0.00315f, 90.01396f, 354.5456f),
				localScale = new Vector3(1.98546f, 1.98546f, 1.98546f)
			}
		});
		itemDisplayRuleDict.Add("mdlEngiTurret", new ItemDisplayRule[1]
		{
			new ItemDisplayRule
			{
				ruleType = ItemDisplayRuleType.ParentedPrefab,
				followerPrefab = ItemDisplayModel,
				childName = "Base",
				localPos = new Vector3(-1.12108f, 1.46076f, 0.33407f),
				localAngles = new Vector3(6.41269f, 112.7309f, 354.4511f),
				localScale = new Vector3(0.4207f, 0.4207f, 0.4207f)
			}
		});
	}
}
