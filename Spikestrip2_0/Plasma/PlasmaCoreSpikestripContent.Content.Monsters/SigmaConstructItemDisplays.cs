using Spikestrip2_0.Core;
using System.Collections.Generic;
using GrooveSaladSpikestripContent;
using RoR2;
using UnityEngine;

namespace PlasmaCoreSpikestripContent.Content.Monsters;

public class SigmaConstructItemDisplays
{
	public static List<ItemDisplayRuleSet.KeyAssetRuleGroup> list = new List<ItemDisplayRuleSet.KeyAssetRuleGroup>();

	public static ItemDisplayRuleSet RegisterDisplays()
	{
		ItemDisplayRuleSet itemDisplayRuleSet = ScriptableObject.CreateInstance<ItemDisplayRuleSet>();
		itemDisplayRuleSet.name = "idrsSigmaConstruct";
		list.Add(new ItemDisplayRuleSet.KeyAssetRuleGroup
		{
			keyAsset = CommonDisplayReferences.AffixRed,
			displayRuleGroup = new DisplayRuleGroup
			{
				rules = new ItemDisplayRule[3]
				{
					new ItemDisplayRule
					{
						ruleType = ItemDisplayRuleType.ParentedPrefab,
						followerPrefab = CommonDisplayReferences.AffixRedHornDisplay,
						childName = "Muzzle1",
						localPos = new Vector3(-0.00095f, -0.00292f, 0.00199f),
						localAngles = new Vector3(295.0197f, 136.041f, 43.07499f),
						localScale = new Vector3(0.001f, 0.001f, 0.001f),
						limbMask = LimbFlags.None
					},
					new ItemDisplayRule
					{
						ruleType = ItemDisplayRuleType.ParentedPrefab,
						followerPrefab = CommonDisplayReferences.AffixRedHornDisplay,
						childName = "Muzzle2",
						localPos = new Vector3(-0.00095f, -0.00292f, 0.00199f),
						localAngles = new Vector3(295.0197f, 136.041f, 43.07499f),
						localScale = new Vector3(0.001f, 0.001f, 0.001f),
						limbMask = LimbFlags.None
					},
					new ItemDisplayRule
					{
						ruleType = ItemDisplayRuleType.ParentedPrefab,
						followerPrefab = CommonDisplayReferences.AffixRedHornDisplay,
						childName = "Muzzle3",
						localPos = new Vector3(-0.00095f, -0.00292f, 0.00199f),
						localAngles = new Vector3(295.0197f, 136.041f, 43.07499f),
						localScale = new Vector3(0.001f, 0.001f, 0.001f),
						limbMask = LimbFlags.None
					}
				}
			}
		});
		list.Add(new ItemDisplayRuleSet.KeyAssetRuleGroup
		{
			keyAsset = CommonDisplayReferences.AffixBlue,
			displayRuleGroup = new DisplayRuleGroup
			{
				rules = new ItemDisplayRule[3]
				{
					new ItemDisplayRule
					{
						ruleType = ItemDisplayRuleType.ParentedPrefab,
						followerPrefab = CommonDisplayReferences.AffixBlueHornDisplay,
						childName = "Muzzle1",
						localPos = new Vector3(0f, -0.00318f, 0.00213f),
						localAngles = new Vector3(14.87754f, 0f, 180f),
						localScale = new Vector3(0.003f, 0.003f, 0.003f),
						limbMask = LimbFlags.None
					},
					new ItemDisplayRule
					{
						ruleType = ItemDisplayRuleType.ParentedPrefab,
						followerPrefab = CommonDisplayReferences.AffixBlueHornDisplay,
						childName = "Muzzle2",
						localPos = new Vector3(0f, -0.00318f, 0.00213f),
						localAngles = new Vector3(14.87754f, 0f, 180f),
						localScale = new Vector3(0.003f, 0.003f, 0.003f),
						limbMask = LimbFlags.None
					},
					new ItemDisplayRule
					{
						ruleType = ItemDisplayRuleType.ParentedPrefab,
						followerPrefab = CommonDisplayReferences.AffixBlueHornDisplay,
						childName = "Muzzle3",
						localPos = new Vector3(0f, -0.00318f, 0.00213f),
						localAngles = new Vector3(14.87754f, 0f, 180f),
						localScale = new Vector3(0.003f, 0.003f, 0.003f),
						limbMask = LimbFlags.None
					}
				}
			}
		});
		list.Add(new ItemDisplayRuleSet.KeyAssetRuleGroup
		{
			keyAsset = CommonDisplayReferences.AffixWhite,
			displayRuleGroup = new DisplayRuleGroup
			{
				rules = new ItemDisplayRule[1]
				{
					new ItemDisplayRule
					{
						ruleType = ItemDisplayRuleType.ParentedPrefab,
						followerPrefab = CommonDisplayReferences.AffixWhiteCrownDisplay,
						childName = "Head",
						localPos = new Vector3(0f, 0.00599f, 0f),
						localAngles = new Vector3(270f, 0f, 0f),
						localScale = new Vector3(0.001f, 0.001f, 0.001f),
						limbMask = LimbFlags.None
					}
				}
			}
		});
		list.Add(new ItemDisplayRuleSet.KeyAssetRuleGroup
		{
			keyAsset = CommonDisplayReferences.AffixPoison,
			displayRuleGroup = new DisplayRuleGroup
			{
				rules = new ItemDisplayRule[1]
				{
					new ItemDisplayRule
					{
						ruleType = ItemDisplayRuleType.ParentedPrefab,
						followerPrefab = CommonDisplayReferences.AffixPoisonCrownDisplay,
						childName = "Head",
						localPos = new Vector3(0f, 0.0007f, 0f),
						localAngles = new Vector3(270f, 0f, 0f),
						localScale = new Vector3(0.00138f, 0.00138f, 0.00138f),
						limbMask = LimbFlags.None
					}
				}
			}
		});
		list.Add(new ItemDisplayRuleSet.KeyAssetRuleGroup
		{
			keyAsset = CommonDisplayReferences.AffixHaunted,
			displayRuleGroup = new DisplayRuleGroup
			{
				rules = new ItemDisplayRule[1]
				{
					new ItemDisplayRule
					{
						ruleType = ItemDisplayRuleType.ParentedPrefab,
						followerPrefab = CommonDisplayReferences.AffixHauntedCrownDisplay,
						childName = "Head",
						localPos = new Vector3(0f, 0.00421f, 0f),
						localAngles = new Vector3(270f, 0f, 0f),
						localScale = new Vector3(0.0015f, 0.0015f, 0.0015f),
						limbMask = LimbFlags.None
					}
				}
			}
		});
		list.Add(new ItemDisplayRuleSet.KeyAssetRuleGroup
		{
			keyAsset = CommonDisplayReferences.AffixVoid,
			displayRuleGroup = new DisplayRuleGroup
			{
				rules = new ItemDisplayRule[1]
				{
					new ItemDisplayRule
					{
						ruleType = ItemDisplayRuleType.ParentedPrefab,
						followerPrefab = CommonDisplayReferences.AffixVoidDisplay,
						childName = "Collision",
						localPos = new Vector3(0f, 0.05299f, 0.00058f),
						localAngles = new Vector3(352.8472f, 359.7008f, 359.7534f),
						localScale = new Vector3(0.2f, 0.2f, 0.2f),
						limbMask = LimbFlags.None
					}
				}
			}
		});
		list.Add(new ItemDisplayRuleSet.KeyAssetRuleGroup
		{
			keyAsset = CommonDisplayReferences.AffixEarth,
			displayRuleGroup = new DisplayRuleGroup
			{
				rules = new ItemDisplayRule[3]
				{
					new ItemDisplayRule
					{
						ruleType = ItemDisplayRuleType.ParentedPrefab,
						followerPrefab = CommonDisplayReferences.AffixEarthAntlersDisplay,
						childName = "Muzzle1",
						localPos = new Vector3(0f, -0.00522f, -0.0003f),
						localAngles = new Vector3(290f, 180f, 0f),
						localScale = new Vector3(0.008f, 0.008f, 0.008f),
						limbMask = LimbFlags.None
					},
					new ItemDisplayRule
					{
						ruleType = ItemDisplayRuleType.ParentedPrefab,
						followerPrefab = CommonDisplayReferences.AffixEarthAntlersDisplay,
						childName = "Muzzle2",
						localPos = new Vector3(0f, -0.00522f, -0.0003f),
						localAngles = new Vector3(290f, 180f, 0f),
						localScale = new Vector3(0.008f, 0.008f, 0.008f),
						limbMask = LimbFlags.None
					},
					new ItemDisplayRule
					{
						ruleType = ItemDisplayRuleType.ParentedPrefab,
						followerPrefab = CommonDisplayReferences.AffixEarthAntlersDisplay,
						childName = "Muzzle3",
						localPos = new Vector3(0f, -0.00522f, -0.0003f),
						localAngles = new Vector3(290f, 180f, 0f),
						localScale = new Vector3(0.008f, 0.008f, 0.008f),
						limbMask = LimbFlags.None
					}
				}
			}
		});
		itemDisplayRuleSet.keyAssetRuleGroups = list.ToArray();
		return itemDisplayRuleSet;
	}
}
