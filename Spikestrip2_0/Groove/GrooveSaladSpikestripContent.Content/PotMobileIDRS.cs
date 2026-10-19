using Spikestrip2_0.Core;
using RoR2;
using UnityEngine;

namespace GrooveSaladSpikestripContent.Content;

public class PotMobileIDRS
{
    //all of this is literally based on moff's potmobile enemy
    public static ItemDisplayRuleSet GetIDRS()
	{
		ItemDisplayRuleSet itemDisplayRuleSet = ScriptableObject.CreateInstance<ItemDisplayRuleSet>();
		itemDisplayRuleSet.keyAssetRuleGroups = new ItemDisplayRuleSet.KeyAssetRuleGroup[7]
		{
			new ItemDisplayRuleSet.KeyAssetRuleGroup
			{
				keyAsset = CommonDisplayReferences.AffixRed,
				displayRuleGroup = new DisplayRuleGroup
				{
					rules = new ItemDisplayRule[2]
					{
						new ItemDisplayRule
						{
							followerPrefab = CommonDisplayReferences.AffixRedHornDisplay,
							ruleType = ItemDisplayRuleType.ParentedPrefab,
							childName = "Base",
							localPos = new Vector3(-0.88359f, 0.94198f, 2.43068f),
							localAngles = new Vector3(356.5854f, 170.5019f, 341.4054f),
							localScale = new Vector3(0.77906f, 0.77906f, 0.77906f)
						},
						new ItemDisplayRule
						{
							followerPrefab = CommonDisplayReferences.AffixRedHornDisplay,
							ruleType = ItemDisplayRuleType.ParentedPrefab,
							childName = "Base",
							localPos = new Vector3(0.88359f, 0.94198f, 2.43068f),
							localAngles = new Vector3(356.5854f, 190f, 19.49145f),
							localScale = new Vector3(-0.77906f, 0.77906f, 0.77906f)
						}
					}
				}
			},
			new ItemDisplayRuleSet.KeyAssetRuleGroup
			{
				keyAsset = CommonDisplayReferences.AffixWhite,
				displayRuleGroup = new DisplayRuleGroup
				{
					rules = new ItemDisplayRule[1]
					{
						new ItemDisplayRule
						{
							followerPrefab = CommonDisplayReferences.AffixWhiteCrownDisplay,
							ruleType = ItemDisplayRuleType.ParentedPrefab,
							childName = "Base",
							localPos = new Vector3(0f, 0.31566f, 3.41049f),
							localAngles = new Vector3(1.92298f, 0.13519f, 185.022f),
							localScale = new Vector3(0.24473f, 0.24473f, 0.24473f)
						}
					}
				}
			},
			new ItemDisplayRuleSet.KeyAssetRuleGroup
			{
				keyAsset = CommonDisplayReferences.AffixBlue,
				displayRuleGroup = new DisplayRuleGroup
				{
					rules = new ItemDisplayRule[2]
					{
						new ItemDisplayRule
						{
							followerPrefab = CommonDisplayReferences.AffixBlueHornDisplay,
							ruleType = ItemDisplayRuleType.ParentedPrefab,
							childName = "Base",
							localPos = new Vector3(0f, 1.0708f, 2.83309f),
							localAngles = new Vector3(355.3744f, 0f, 180f),
							localScale = new Vector3(1.55276f, 1.55276f, 1.55276f)
						},
						new ItemDisplayRule
						{
							followerPrefab = CommonDisplayReferences.AffixBlueHornDisplay,
							ruleType = ItemDisplayRuleType.ParentedPrefab,
							childName = "Base",
							localPos = new Vector3(0f, 0.21005f, 2.84763f),
							localAngles = new Vector3(0f, 0f, 180f),
							localScale = new Vector3(1.03231f, 1.03231f, 1.03231f)
						}
					}
				}
			},
			new ItemDisplayRuleSet.KeyAssetRuleGroup
			{
				keyAsset = CommonDisplayReferences.AffixPoison,
				displayRuleGroup = new DisplayRuleGroup
				{
					rules = new ItemDisplayRule[1]
					{
						new ItemDisplayRule
						{
							followerPrefab = CommonDisplayReferences.AffixPoisonCrownDisplay,
							ruleType = ItemDisplayRuleType.ParentedPrefab,
							childName = "Base",
							localPos = new Vector3(0f, -3E-05f, 2.84774f),
							localAngles = new Vector3(0f, 0f, 1f),
							localScale = new Vector3(0.35079f, 0.35079f, 0.35079f)
						}
					}
				}
			},
			new ItemDisplayRuleSet.KeyAssetRuleGroup
			{
				keyAsset = CommonDisplayReferences.AffixHaunted,
				displayRuleGroup = new DisplayRuleGroup
				{
					rules = new ItemDisplayRule[1]
					{
						new ItemDisplayRule
						{
							followerPrefab = CommonDisplayReferences.AffixHauntedCrownDisplay,
							ruleType = ItemDisplayRuleType.ParentedPrefab,
							childName = "Base",
							localPos = new Vector3(0f, -0.10108f, 3.29707f),
							localAngles = new Vector3(6.83707f, 0f, 180f),
							localScale = new Vector3(0.49848f, 0.49848f, 0.49848f)
						}
					}
				}
			},
			new ItemDisplayRuleSet.KeyAssetRuleGroup
			{
				keyAsset = CommonDisplayReferences.AffixEarth,
				displayRuleGroup = new DisplayRuleGroup
				{
					rules = new ItemDisplayRule[1]
					{
						new ItemDisplayRule
						{
							followerPrefab = CommonDisplayReferences.AffixEarthAntlersDisplay,
							ruleType = ItemDisplayRuleType.ParentedPrefab,
							childName = "Base",
							localPos = new Vector3(0f, -8E-05f, 2.81019f),
							localAngles = new Vector3(61.29994f, 180f, 181f),
							localScale = new Vector3(5.54461f, 5.54461f, 5.54461f)
						}
					}
				}
			},
			new ItemDisplayRuleSet.KeyAssetRuleGroup
			{
				keyAsset = CommonDisplayReferences.AffixVoid,
				displayRuleGroup = new DisplayRuleGroup
				{
					rules = new ItemDisplayRule[1]
					{
						new ItemDisplayRule
						{
							followerPrefab = CommonDisplayReferences.AffixVoidDisplay,
							ruleType = ItemDisplayRuleType.ParentedPrefab,
							childName = "Base",
							localPos = new Vector3(0f, 1.72267f, 2.14544f),
							localAngles = new Vector3(29.02781f, 0f, 1f),
							localScale = new Vector3(1.91415f, 1.91416f, 1.91415f)
						}
					}
				}
			}
		};
		return itemDisplayRuleSet;
	}
}
