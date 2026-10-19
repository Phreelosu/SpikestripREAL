using System;
using R2API;
using RoR2;
using UnityEngine;
using Spikestrip2_0.Core;

namespace GrooveSaladSpikestripContent.Content;

public class SingularityConsumed : ItemBase<SingularityConsumed>
{
	public override Type DependsOn => typeof(Singularity);

	public override string ItemToken => "SINGULARITYCONSUMED";

	public override string ItemName => "Desolate Bulwark";

	public override string ItemPickup => "There is no going back..";

	public override string ItemDescription => "There is no going back..";

	public override string ItemLore => null;

	public override ItemTier Tier => ItemTier.NoTier;

	public override ItemTag[] ItemTags => new ItemTag[2]
	{
		ItemTag.CannotCopy,
		ItemTag.CannotSteal
	};

	public override Sprite ItemIconSprite => SpikestripContent.GrooveBundle.LoadAsset<Sprite>("texSingularityConsumedIcon.png");

	public override bool CanRemove => false;

	public override void Init()
	{
		base.Init();
	}

	public override void AssignItemModels()
	{
		ItemPickupModel = SpikestripUtil.nullModel;
		Material material = LegacyResourcesAPI.Load<GameObject>("Prefabs/NetworkedObjects/Chest/LunarChest").GetComponentInChildren<SkinnedMeshRenderer>().material;
		Material material2 = UnityEngine.Object.Instantiate(material);
		material2.color = SpikestripUtil.ColorRGB(107f, 113f, 127f);
		material2.mainTexture = null;
		material2.EnableKeyword("DITHER");
		GameObject gameObject = SpikestripContent.GrooveBundle.LoadAsset<GameObject>("DisplaySingularityConsumed.prefab");
		gameObject.GetComponentInChildren<MeshRenderer>().material = material2;
		Transform transform = gameObject.transform.Find("mdlSingularity");
		transform.localRotation = Util.QuaternionSafeLookRotation(Vector3.forward + Vector3.up * 2f);
		ItemDisplayModel = SpikestripUtil.EmptyPrefab("DisplaySingularityConsumedParent");
		ItemDisplay itemDisplay = ItemDisplayModel.AddComponent<ItemDisplay>();
		itemDisplay.rendererInfos = Array.Empty<CharacterModel.RendererInfo>();
		ItemFollower itemFollower = ItemDisplayModel.AddComponent<ItemFollower>();
		itemFollower.targetObject = ItemDisplayModel;
		itemFollower.followerPrefab = gameObject;
		itemFollower.distanceDampTime = 0.2f;
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
				localPos = new Vector3(-0.83165f, -0.08504f, -0.74548f),
				localAngles = new Vector3(0f, 90f, 90f),
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
				localPos = new Vector3(-0.75448f, -0.07471f, -0.51041f),
				localAngles = new Vector3(0f, 90f, 90f),
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
				localPos = new Vector3(6.06479f, -2.19588f, 5.81212f),
				localAngles = new Vector3(0f, 90f, 90f),
				localScale = new Vector3(0.1f, 0.1f, 0.1f)
			}
		});
		itemDisplayRuleDict.Add("mdlEngi", new ItemDisplayRule[1]
		{
			new ItemDisplayRule
			{
				ruleType = ItemDisplayRuleType.ParentedPrefab,
				followerPrefab = ItemDisplayModel,
				childName = "Base",
				localPos = new Vector3(-0.98999f, -0.20988f, -0.76171f),
				localAngles = new Vector3(0f, 90f, 90f),
				localScale = new Vector3(0.1f, 0.1f, 0.1f)
			}
		});
		itemDisplayRuleDict.Add("mdlMage", new ItemDisplayRule[1]
		{
			new ItemDisplayRule
			{
				ruleType = ItemDisplayRuleType.ParentedPrefab,
				followerPrefab = ItemDisplayModel,
				childName = "Base",
				localPos = new Vector3(-0.80188f, -0.10829f, -0.52509f),
				localAngles = new Vector3(0f, 90f, 90f),
				localScale = new Vector3(0.1f, 0.1f, 0.1f)
			}
		});
		itemDisplayRuleDict.Add("mdlMerc", new ItemDisplayRule[1]
		{
			new ItemDisplayRule
			{
				ruleType = ItemDisplayRuleType.ParentedPrefab,
				followerPrefab = ItemDisplayModel,
				childName = "Base",
				localPos = new Vector3(-0.88968f, -0.11249f, -0.57362f),
				localAngles = new Vector3(0f, 90f, 90f),
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
				localPos = new Vector3(-1.39138f, -0.40221f, -2.17637f),
				localAngles = new Vector3(0f, 90f, 90f),
				localScale = new Vector3(0.1f, 0.1f, 0.1f)
			}
		});
		itemDisplayRuleDict.Add("mdlLoader", new ItemDisplayRule[1]
		{
			new ItemDisplayRule
			{
				ruleType = ItemDisplayRuleType.ParentedPrefab,
				followerPrefab = ItemDisplayModel,
				childName = "Base",
				localPos = new Vector3(-0.96226f, -0.23438f, -0.56522f),
				localAngles = new Vector3(0f, 90f, 90f),
				localScale = new Vector3(0.1f, 0.1f, 0.1f)
			}
		});
		itemDisplayRuleDict.Add("mdlCroco", new ItemDisplayRule[1]
		{
			new ItemDisplayRule
			{
				ruleType = ItemDisplayRuleType.ParentedPrefab,
				followerPrefab = ItemDisplayModel,
				childName = "Base",
				localPos = new Vector3(-5.73058f, 0.80807f, 4.61064f),
				localAngles = new Vector3(0f, 90f, 90f),
				localScale = new Vector3(0.1f, 0.1f, 0.1f)
			}
		});
		itemDisplayRuleDict.Add("mdlCaptain", new ItemDisplayRule[1]
		{
			new ItemDisplayRule
			{
				ruleType = ItemDisplayRuleType.ParentedPrefab,
				followerPrefab = ItemDisplayModel,
				childName = "Base",
				localPos = new Vector3(-0.94503f, 0.13494f, -0.83048f),
				localAngles = new Vector3(0f, 90f, 90f),
				localScale = new Vector3(0.1f, 0.1f, 0.1f)
			}
		});
		itemDisplayRuleDict.Add("mdlBandit2", new ItemDisplayRule[1]
		{
			new ItemDisplayRule
			{
				ruleType = ItemDisplayRuleType.ParentedPrefab,
				followerPrefab = ItemDisplayModel,
				childName = "Base",
				localPos = new Vector3(-0.21872f, 0.95396f, -0.74463f),
				localAngles = new Vector3(0f, 90f, 90f),
				localScale = new Vector3(0.1f, 0.1f, 0.1f)
			}
		});
		itemDisplayRuleDict.Add("mdlRailGunner", new ItemDisplayRule[1]
		{
			new ItemDisplayRule
			{
				ruleType = ItemDisplayRuleType.ParentedPrefab,
				followerPrefab = ItemDisplayModel,
				childName = "Base",
				localPos = new Vector3(-0.60654f, -0.18994f, -0.61619f),
				localAngles = new Vector3(0f, 90f, 90f),
				localScale = new Vector3(0.1f, 0.1f, 0.1f)
			}
		});
		itemDisplayRuleDict.Add("mdlVoidSurvivor", new ItemDisplayRule[1]
		{
			new ItemDisplayRule
			{
				ruleType = ItemDisplayRuleType.ParentedPrefab,
				followerPrefab = ItemDisplayModel,
				childName = "Base",
				localPos = new Vector3(-1.02322f, 1.12996f, 0.12805f),
				localAngles = new Vector3(0f, 90f, 90f),
				localScale = new Vector3(0.1f, 0.1f, 0.1f)
			}
		});
		itemDisplayRuleDict.Add("mdlScav", new ItemDisplayRule[1]
		{
			new ItemDisplayRule
			{
				ruleType = ItemDisplayRuleType.ParentedPrefab,
				followerPrefab = ItemDisplayModel,
				childName = "Base",
				localPos = new Vector3(14.30001f, 2.92203f, 14.97974f),
				localAngles = new Vector3(0f, 90f, 90f),
				localScale = new Vector3(0.1f, 0.1f, 0.1f)
			}
		});
		itemDisplayRuleDict.Add("mdlEngiTurret", new ItemDisplayRule[1]
		{
			new ItemDisplayRule
			{
				ruleType = ItemDisplayRuleType.ParentedPrefab,
				followerPrefab = ItemDisplayModel,
				childName = "Base",
				localPos = new Vector3(-2.40124f, 3.78879f, -0.75998f),
				localAngles = new Vector3(0f, 90f, 90f),
				localScale = new Vector3(0.1f, 0.1f, 0.1f)
			}
		});
	}
}
