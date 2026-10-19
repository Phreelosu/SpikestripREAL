using System;
using System.Runtime.CompilerServices;
using BepInEx.Bootstrap;
using MysticsItems.Items;
using On.RoR2;
using R2API;
using RoR2;
using UnityEngine;
using UnityEngine.Networking;
using Spikestrip2_0.Core;

namespace GrooveSaladSpikestripContent.Content;

public class DoubleItemsEquip : EquipmentBase<DoubleItemsEquip>
{
	public class DoubledItemsInventoryServer : MonoBehaviour
	{
		public RoR2.Inventory trackedInventory;

		public RoR2.Run.FixedTimeStamp expiresAt;

		public int[] grantedStacks = RoR2.ItemCatalog.RequestItemStackArray();

		public void DoubleItems(Func<ItemIndex, bool> filter)
		{
			for (ItemIndex itemIndex = (ItemIndex)0; (int)itemIndex < grantedStacks.Length; itemIndex++)
			{
				int count = trackedInventory.GetItemCountPermanent(itemIndex) + trackedInventory.GetItemCountTemp(itemIndex);
				if (count > 0 && filter(itemIndex))
				{
					grantedStacks[(int)itemIndex] = count;
					trackedInventory.GiveItemTemp(itemIndex, count);
				}
			}
		}

		public void RemoveGrantedStacks(RoR2.Inventory inventory, Func<ItemIndex, bool> filter = null)
		{
			for (int i = 0; i < grantedStacks.Length; i++)
			{
				if (grantedStacks[i] > 0 && (filter == null || filter((ItemIndex)i)))
				{
					inventory.RemoveItemTemp((ItemIndex)i, grantedStacks[i]);
				}
			}
		}

		private void FixedUpdate()
		{
			if (!trackedInventory || expiresAt.hasPassed)
			{
				Destroy(this);
			}
		}

		private void OnDestroy()
		{
			if (NetworkServer.active && (bool)trackedInventory)
			{
				RemoveGrantedStacks(trackedInventory);
			}
			RoR2.ItemCatalog.ReturnItemStackArray(grantedStacks);
		}
	}

	public static RoR2.BuffDef buff;

	public static float buffDuration = 8f;

	public override string EquipmentToken => "DOUBLEITEMS";

	public override string EquipmentName => "Quartz Rabbit";

	public override string EquipmentPickup => "Double your items for a short time.";

	public override string EquipmentDescription => "<style=cIsUtility>Double your items</style> for <style=cIsUtility>" + buffDuration + "</style> seconds. The copies are <style=cIsUtility>temporary items</style>.";

	public override string EquipmentLore => "Order: Quartz Rabbit\nTracking Number: 777*****\nEstimated Delivery: 12/10/2077\nShipping Method: Standard\nShipping Address: Obscurity Museum, Solus\nShipping Details:\n\nThe object will be shipped in a small, wooden box. The inside is velvet to help the gemstone surface. When I found this in my studies, I was floored. Despite being decades old, the surface was pristine and beautifully milky. Touching the surface reflected the image of my glove throughout all the cuts on the gem. It was like I was seeing double!\n\nThe expedition I found it on was really expensive, so I hope this thing is worth it, being this far out in the galaxy.";

	public override Sprite EquipmentIcon => SpikestripContent.GrooveBundle.LoadAsset<Sprite>("texRabbitIcon.png");

	public override float Cooldown => 100f;

	public override void Init()
	{
		base.Init();
		buff = ScriptableObject.CreateInstance<RoR2.BuffDef>();
		buff.name = "DoubleItemsBuff";
		buff.buffColor = SpikestripUtil.ColorRGB(197f, 206f, 247f);
		buff.canStack = true;
		buff.iconSprite = SpikestripContent.GrooveBundle.LoadAsset<Sprite>("texBuffRabbit.png");
		buff.isDebuff = false;
		buff.startSfx = LegacyResourcesAPI.Load<RoR2.NetworkSoundEventDef>("networksoundeventdefs/nseLunarSecondaryProjectileBounce");
		ContentAddition.AddBuffDef(buff);
		On.RoR2.Inventory.AddItemsFrom_Inventory_Func2 += Inventory_AddItemsFrom;
		if (Chainloader.PluginInfos.ContainsKey("com.themysticsword.mysticsitems"))
		{
			RoR2.RoR2Application.onLoad += FixCookie;
			On.RoR2.CharacterBody.AddTimedBuff_BuffDef_float += CookieGuard;
		}
	}

	[MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
	private void FixCookie()
	{
		if (!Cookie.ignoredBuffDefs.Contains(buff))
		{
			Cookie.ignoredBuffDefs.Add(buff);
		}
	}

	private void CookieGuard(On.RoR2.CharacterBody.orig_AddTimedBuff_BuffDef_float orig, RoR2.CharacterBody self, RoR2.BuffDef buffDef, float duration)
	{
		if (buffDef == buff)
		{
			FixCookie();
		}
		orig(self, buffDef, duration);
	}

	private void Inventory_AddItemsFrom(On.RoR2.Inventory.orig_AddItemsFrom_Inventory_Func2 orig, RoR2.Inventory self, RoR2.Inventory other, Func<ItemIndex, bool> filter)
	{
		orig(self, other, filter);
		if (!NetworkServer.active || self == other)
		{
			return;
		}
		foreach (DoubledItemsInventoryServer doubled in other.GetComponents<DoubledItemsInventoryServer>())
		{
			doubled.RemoveGrantedStacks(self, filter);
		}
	}

	private bool DoubledItemsFilter(ItemIndex itemIndex)
	{
		RoR2.ItemDef itemDef = RoR2.ItemCatalog.GetItemDef(itemIndex);
		return (bool)itemDef && !itemDef.hidden && itemDef.tier != ItemTier.NoTier && itemDef.DoesNotContainTag(ItemTag.EquipmentRelated) && itemDef.DoesNotContainTag(ItemTag.CannotDuplicate);
	}

	public override bool OnActivationServer(RoR2.EquipmentSlot equipmentSlot)
	{
		RoR2.CharacterBody body = equipmentSlot.characterBody;
		body.AddTimedBuff(buff, buffDuration);
		if ((bool)body.inventory)
		{
			DoubledItemsInventoryServer doubled = body.inventory.gameObject.AddComponent<DoubledItemsInventoryServer>();
			doubled.trackedInventory = body.inventory;
			doubled.expiresAt = RoR2.Run.FixedTimeStamp.now + buffDuration;
			doubled.DoubleItems(DoubledItemsFilter);
		}
		return true;
	}

	public override void AssignEquipmentValues()
	{
		EquipmentPickupModel = SpikestripContent.GrooveBundle.LoadAsset<GameObject>("PickupRabbit.prefab");
		Material material = UnityEngine.Object.Instantiate(LegacyResourcesAPI.Load<GameObject>("prefabs/characterbodies/TimeCrystalBody").transform.Find("ModelBase/Mesh").GetComponent<MeshRenderer>().material);
		material.color = SpikestripUtil.ColorRGB(0f, 207f, 178f);
		material.EnableKeyword("DITHER");
		EquipmentPickupModel.GetComponent<MeshRenderer>().material = material;
		EquipmentDisplayModel = EquipmentPickupModel;
		RoR2.ItemDisplay itemDisplay = EquipmentDisplayModel.AddComponent<RoR2.ItemDisplay>();
		itemDisplay.rendererInfos = SpikestripUtil.GenerateRendererInfos(EquipmentDisplayModel);
	}

	public override void PopulateItemDisplayRules(ItemDisplayRuleDict itemDisplayRuleDict)
	{
		itemDisplayRuleDict.Add("mdlCommandoDualies", new RoR2.ItemDisplayRule[1]
		{
			new RoR2.ItemDisplayRule
			{
				ruleType = ItemDisplayRuleType.ParentedPrefab,
				followerPrefab = EquipmentDisplayModel,
				childName = "Chest",
				localPos = new Vector3(-0.10464f, 0.41384f, -0.13422f),
				localAngles = new Vector3(301.2265f, 9.97476f, 64.75301f),
				localScale = new Vector3(0.05161f, 0.05161f, 0.05161f)
			}
		});
		itemDisplayRuleDict.Add("mdlHuntress", new RoR2.ItemDisplayRule[1]
		{
			new RoR2.ItemDisplayRule
			{
				ruleType = ItemDisplayRuleType.ParentedPrefab,
				followerPrefab = EquipmentDisplayModel,
				childName = "Chest",
				localPos = new Vector3(0.07331f, 0.09957f, -0.13935f),
				localAngles = new Vector3(296.04f, 78.63948f, 305.8893f),
				localScale = new Vector3(0.03572f, 0.03572f, 0.03572f)
			}
		});
		itemDisplayRuleDict.Add("mdlToolbot", new RoR2.ItemDisplayRule[1]
		{
			new RoR2.ItemDisplayRule
			{
				ruleType = ItemDisplayRuleType.ParentedPrefab,
				followerPrefab = EquipmentDisplayModel,
				childName = "Chest",
				localPos = new Vector3(-1.13952f, 3.10915f, -1.71712f),
				localAngles = new Vector3(11.43073f, 258.3306f, 354.4617f),
				localScale = new Vector3(0.35349f, 0.35548f, 0.35349f)
			}
		});
		itemDisplayRuleDict.Add("mdlEngi", new RoR2.ItemDisplayRule[1]
		{
			new RoR2.ItemDisplayRule
			{
				ruleType = ItemDisplayRuleType.ParentedPrefab,
				followerPrefab = EquipmentDisplayModel,
				childName = "Chest",
				localPos = new Vector3(-0.1401f, 0.49593f, -0.22314f),
				localAngles = new Vector3(299.7665f, 343.1718f, 80.44689f),
				localScale = new Vector3(0.04542f, 0.04542f, 0.04542f)
			}
		});
		itemDisplayRuleDict.Add("mdlMage", new RoR2.ItemDisplayRule[1]
		{
			new RoR2.ItemDisplayRule
			{
				ruleType = ItemDisplayRuleType.ParentedPrefab,
				followerPrefab = EquipmentDisplayModel,
				childName = "Chest",
				localPos = new Vector3(-0.0166f, 0.28951f, -0.09f),
				localAngles = new Vector3(336.0153f, 102.7926f, 1.66666f),
				localScale = new Vector3(0.04529f, 0.04529f, 0.04529f)
			}
		});
		itemDisplayRuleDict.Add("mdlMerc", new RoR2.ItemDisplayRule[1]
		{
			new RoR2.ItemDisplayRule
			{
				ruleType = ItemDisplayRuleType.ParentedPrefab,
				followerPrefab = EquipmentDisplayModel,
				childName = "Chest",
				localPos = new Vector3(-0.12026f, 0.25495f, -0.18075f),
				localAngles = new Vector3(281.6689f, 94.88833f, 6.49087f),
				localScale = new Vector3(0.03735f, 0.03735f, 0.03735f)
			}
		});
		itemDisplayRuleDict.Add("mdlTreebot", new RoR2.ItemDisplayRule[1]
		{
			new RoR2.ItemDisplayRule
			{
				ruleType = ItemDisplayRuleType.ParentedPrefab,
				followerPrefab = EquipmentDisplayModel,
				childName = "PlatformBase",
				localPos = new Vector3(-0.18961f, 0.6615f, -0.85533f),
				localAngles = new Vector3(278.1245f, 2.02145f, 44.69693f),
				localScale = new Vector3(0.14681f, 0.14681f, 0.14681f)
			}
		});
		itemDisplayRuleDict.Add("mdlLoader", new RoR2.ItemDisplayRule[1]
		{
			new RoR2.ItemDisplayRule
			{
				ruleType = ItemDisplayRuleType.ParentedPrefab,
				followerPrefab = EquipmentDisplayModel,
				childName = "Chest",
				localPos = new Vector3(-0.0816f, 0.1227f, -0.32083f),
				localAngles = new Vector3(348.1631f, 266.5335f, 357.129f),
				localScale = new Vector3(0.0494f, 0.0494f, 0.0494f)
			}
		});
		itemDisplayRuleDict.Add("mdlCroco", new RoR2.ItemDisplayRule[1]
		{
			new RoR2.ItemDisplayRule
			{
				ruleType = ItemDisplayRuleType.ParentedPrefab,
				followerPrefab = EquipmentDisplayModel,
				childName = "SpineChest2",
				localPos = new Vector3(1.29099f, -0.12404f, -0.63704f),
				localAngles = new Vector3(9.35126f, 170.8928f, 22.2057f),
				localScale = new Vector3(0.46903f, 0.46903f, 0.46903f)
			}
		});
		itemDisplayRuleDict.Add("mdlCaptain", new RoR2.ItemDisplayRule[1]
		{
			new RoR2.ItemDisplayRule
			{
				ruleType = ItemDisplayRuleType.ParentedPrefab,
				followerPrefab = EquipmentDisplayModel,
				childName = "Chest",
				localPos = new Vector3(-0.23306f, 0.20106f, -0.2339f),
				localAngles = new Vector3(340.635f, 280.717f, 20.46477f),
				localScale = new Vector3(0.0713f, 0.0713f, 0.0713f)
			}
		});
		itemDisplayRuleDict.Add("mdlBandit2", new RoR2.ItemDisplayRule[1]
		{
			new RoR2.ItemDisplayRule
			{
				ruleType = ItemDisplayRuleType.ParentedPrefab,
				followerPrefab = EquipmentDisplayModel,
				childName = "Chest",
				localPos = new Vector3(-0.03611f, 0.22581f, -0.20179f),
				localAngles = new Vector3(334.3909f, 264.8929f, 13.43124f),
				localScale = new Vector3(0.03578f, 0.03578f, 0.03578f)
			}
		});
		itemDisplayRuleDict.Add("mdlRailGunner", new RoR2.ItemDisplayRule[1]
		{
			new RoR2.ItemDisplayRule
			{
				ruleType = ItemDisplayRuleType.ParentedPrefab,
				followerPrefab = EquipmentDisplayModel,
				childName = "Backpack",
				localPos = new Vector3(0.25853f, -0.34302f, -0.00495f),
				localAngles = new Vector3(353.8461f, 20.65903f, 338.4302f),
				localScale = new Vector3(0.03625f, 0.03625f, 0.03625f)
			}
		});
		itemDisplayRuleDict.Add("mdlVoidSurvivor", new RoR2.ItemDisplayRule[1]
		{
			new RoR2.ItemDisplayRule
			{
				ruleType = ItemDisplayRuleType.ParentedPrefab,
				followerPrefab = EquipmentDisplayModel,
				childName = "Chest",
				localPos = new Vector3(-0.09911f, 0.00126f, -0.17635f),
				localAngles = new Vector3(332.3642f, 258.2487f, 57.30189f),
				localScale = new Vector3(0.04068f, 0.04068f, 0.04068f)
			}
		});
		itemDisplayRuleDict.Add("mdlScav", new RoR2.ItemDisplayRule[1]
		{
			new RoR2.ItemDisplayRule
			{
				ruleType = ItemDisplayRuleType.ParentedPrefab,
				followerPrefab = EquipmentDisplayModel,
				childName = "Chest",
				localPos = new Vector3(6.09614f, 4.80222f, 0.56806f),
				localAngles = new Vector3(337.2996f, 213.5197f, 12.63128f),
				localScale = new Vector3(0.65811f, 0.65811f, 0.65811f)
			}
		});
		itemDisplayRuleDict.Add("mdlEquipmentDrone", new RoR2.ItemDisplayRule[1]
		{
			new RoR2.ItemDisplayRule
			{
				ruleType = ItemDisplayRuleType.ParentedPrefab,
				followerPrefab = EquipmentDisplayModel,
				childName = "GunBarrelBase",
				localPos = new Vector3(0.01294f, 0.2215f, 0.98674f),
				localAngles = new Vector3(287.7457f, 240.6001f, 118.2206f),
				localScale = new Vector3(0.27575f, 0.27575f, 0.27575f)
			}
		});
	}
}
