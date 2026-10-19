using System;
using System.Collections.Generic;
using GrooveSaladSpikestripContent;
using NoopSpikestripContent.Utils;
using R2API;
using RoR2;
using UnityEngine;
using Spikestrip2_0.Core;

namespace NoopSpikestrip;

internal class Wool : ItemBase<Wool>
{
	public class WoolBehaviour : CharacterBody.ItemBehavior
	{
		public float Timer;

		private GameObject WoolIndicator;

		private bool indicatorEnabled
		{
			get
			{
				return WoolIndicator;
			}
			set
			{
				if (indicatorEnabled != value)
				{
					if (value)
					{
						WoolIndicator = UnityEngine.Object.Instantiate(FocusClone, body.corePosition, Quaternion.identity);
						WoolIndicator.GetComponent<NetworkedBodyAttachment>().AttachToGameObjectAndSpawn(base.gameObject);
					}
					else
					{
						UnityEngine.Object.Destroy(WoolIndicator);
						WoolIndicator = null;
					}
				}
			}
		}

		public void FixedUpdate()
		{
			Timer += Time.fixedDeltaTime;
			if (!((double)Timer >= 0.05))
			{
				return;
			}
			Timer = 0f;
			TeamIndex teamIndex = body.teamComponent.teamIndex;
			int num = 0;
			for (TeamIndex teamIndex2 = TeamIndex.Neutral; teamIndex2 < TeamIndex.EndOfTheCycle; teamIndex2++)
			{
				if (teamIndex2 != teamIndex && teamIndex2 != TeamIndex.Neutral)
				{
					foreach (TeamComponent teamMember in TeamComponent.GetTeamMembers(teamIndex2))
					{
						if ((teamMember.transform.position - body.corePosition).sqrMagnitude <= 400f)
						{
							num++;
						}
					}
				}
			}
			SetBuffCount(num);
		}

		public void SetBuffCount(int count)
		{
			int buffCount = body.GetBuffCount(MyAgonyInABuff);
			if (buffCount == count)
			{
				return;
			}
			if (buffCount < count)
			{
				for (int i = 0; i < count - buffCount; i++)
				{
					body.AddBuff(MyAgonyInABuff);
				}
			}
			else if (buffCount > count)
			{
				for (int j = 0; j < buffCount - count; j++)
				{
					body.RemoveBuff(MyAgonyInABuff);
				}
			}
		}

		private void Start()
		{
			indicatorEnabled = true;
		}

		private void OnDestroy()
		{
			indicatorEnabled = false;
		}
	}

	public static BuffDef MyAgonyInABuff;

	public static GameObject FocusClone;

	public override string ItemName => "Wool";

	public override string ItemToken => "ARMORWOOL";

	public override string ItemPickup => "Gain armor for every nearby enemy.";

	public override string ItemDescription => "<style=cIsHealing>Increase armor</style> by <style=cIsHealing>5</style> <style=cStack>(+5 per stack)</style> for every enemy in a <style=cIsUtility>20m</style> radius. ";

	public override string ItemLore => "\"huhahwa?\" \n\n\"Oh my guhawd, how are you this bad, dude?\" \n\n\"Dude, I don't think you understand the spawners in this map are like placed in the epic spots, Dude.\"\n\n\"3... 2... Wait for it... wait for it- Yeah.\"\n\nHeyimnoob was blown up by Creeper\n\n\"WHYYYY ARE THERE SO MANY CREEPERS\"\n\n\"WAIT, how did you know that Firewa- Firewaze- Is this you- are you Herobrine?\"\n\"...\"\n\"...\"\n\n\"Uh... Listen\"\n\n\"Spell Firewaze backwards. Think about it.\"";

	private static LanguageAPI.LanguageOverlay woolNameOverlay;

	private void UpdateWoolName()
	{
		woolNameOverlay?.Remove();
		woolNameOverlay = null;
		string profileName = LocalUserManager.GetFirstLocalUser()?.userProfile?.name;
		if (!string.IsNullOrWhiteSpace(profileName))
		{
			woolNameOverlay = LanguageAPI.AddOverlay(ItemDef.nameToken, profileName + "'s Wool");
		}
	}

	public override ItemTier Tier => ItemTier.Tier1;

	public override Sprite ItemIconSprite => SpikestripContent.NoopBundle.LoadAsset<Sprite>("NeedleIcon.png");

	public override Type ItemBehaviour => typeof(WoolBehaviour);

	public override ItemTag[] ItemTags => new ItemTag[1] { ItemTag.Utility };

	public override bool ServerOnlyBehaviour => true;

	public override void Init()
	{
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Expected O, but got Unknown
		base.Init();
		RecalculateStatsAPI.GetStatCoefficients += new RecalculateStatsAPI.StatHookEventHandler(RecalculateStatsAPI_GetStatCoefficients);
		LocalUserManager.onUserSignIn += (LocalUser _) => UpdateWoolName();
		LocalUserManager.onUserSignOut += (LocalUser _) => UpdateWoolName();
		MyAgonyInABuff =ScriptableObject.CreateInstance<BuffDef>();
		MyAgonyInABuff.canStack = true;
		MyAgonyInABuff.iconSprite = SpikestripContent.NoopBundle.LoadAsset<Sprite>("texWoolBuffIcon.png");
		MyAgonyInABuff.buffColor = SpikestripUtil.ColorRGB(136f, 154f, 184f);
		ContentAddition.AddBuffDef(MyAgonyInABuff);
		FocusClone = PrefabAPI.InstantiateClone(SpikestripUtil.LegacyLoad<GameObject>("Prefabs/NetworkedObjects/NearbyDamageBonusIndicator"), "WoolEffect", true);
		Transform transform = FocusClone.transform.Find("Radius, Spherical");
		transform.localScale = Vector3.one * 20f * 2f;
		MaterialControllerComponents.HGIntersectionController hGIntersectionController = transform.gameObject.AddComponent<MaterialControllerComponents.HGIntersectionController>();
		MeshRenderer meshRenderer = (MeshRenderer)(hGIntersectionController.renderer = transform.GetComponent<MeshRenderer>());
		Material original = SpikestripUtil.AddressablesLoad<Material>("d0eb35f70367cdc4882f3bb794b65f2b");
		Material material = UnityEngine.Object.Instantiate(original);
		material.SetTexture("_RemapTex", SpikestripUtil.AddressablesLoad<Texture>("385005992afbfce4089807386adc07b0"));
		material.SetColor("_TintColor", SpikestripUtil.ColorRGB(50f, 82f, 115f));
		meshRenderer.material = material;
		hGIntersectionController.material = material;
		material.SetFloat("_BrightnessBoost", 0.1f);
	}

	private void RecalculateStatsAPI_GetStatCoefficients(CharacterBody sender, RecalculateStatsAPI.StatHookEventArgs args)
	{
		if ((bool)sender.inventory)
		{
			args.armorAdd += (float)(5 * (sender.inventory.GetItemCountEffective(ItemDef) * sender.GetBuffCount(MyAgonyInABuff)));
		}
	}

	public override void AssignItemModels()
	{
		GameObject gameObject = SpikestripContent.NoopBundle.LoadAsset<GameObject>("NoopNeedle.prefab");
		ItemPickupModel = SpikestripUtil.UpdateGameObjectShader(gameObject);
		ItemDisplayModel = PrefabAPI.InstantiateClone(gameObject, "Wooldisplay", false);
		ItemDisplay itemDisplay = ItemDisplayModel.AddComponent<ItemDisplay>();
		itemDisplay.rendererInfos = SpikestripUtil.GenerateRendererInfos(ItemDisplayModel, allRendererTypes: true);
		ItemDisplayModel = SpikestripUtil.UpdateGameObjectShader(ItemDisplayModel);
		List<string> list = new List<string>();
		list.Add("NeedleRig/needle/fabric1.1/fabric1.2/fabric1.3/fabric1.4/fabric1.4_end");
		list.Add("NeedleRig/needle/fabric1.1/fabric1.2/fabric1.3/fabric1.4");
		list.Add("NeedleRig/needle/fabric1.1/fabric1.2/fabric1.3");
		list.Add("NeedleRig/needle/fabric1.1/fabric1.2");
		list.Add("NeedleRig/needle/fabric1.1");
		list.Add("NeedleRig/needle/fabric2.1/fabric2.2/fabric2.3/fabric2.4/fabric2.4_end");
		list.Add("NeedleRig/needle/fabric2.1/fabric2.2/fabric2.3/fabric2.4");
		list.Add("NeedleRig/needle/fabric2.1/fabric2.2/fabric2.3");
		list.Add("NeedleRig/needle/fabric2.1/fabric2.2");
		list.Add("NeedleRig/needle/fabric2.1");
		list.Add("NeedleRig/needle/fabric3.1/fabric3.2/fabric3.3/fabric3.4/fabric3.4_end");
		list.Add("NeedleRig/needle/fabric3.1/fabric3.2/fabric3.3/fabric3.4");
		list.Add("NeedleRig/needle/fabric3.1/fabric3.2/fabric3.3");
		list.Add("NeedleRig/needle/fabric3.1/fabric3.2");
		list.Add("NeedleRig/needle/fabric3.1");
		for (int i = 0; i < list.Count; i++)
		{
			DynamicBone dynamicBone = ItemDisplayModel.transform.Find(list[i]).gameObject.AddComponent<DynamicBone>();
			dynamicBone.m_UpdateRate = 60f;
			dynamicBone.enabled = true;
			dynamicBone.m_Damping = 0.05f;
			dynamicBone.m_Elasticity = 0f;
			dynamicBone.m_Stiffness = 0f;
			dynamicBone.m_Root = ItemDisplayModel.transform.Find(list[i]);
			dynamicBone.m_Gravity = new Vector3(0f, -0.3f, 0f);
		}
	}

	public override void PopulateItemDisplayRules(ItemDisplayRuleDict itemDisplayRuleDict)
	{
		itemDisplayRuleDict.Add("mdlCommandoDualies", new ItemDisplayRule[1]
		{
			new ItemDisplayRule
			{
				ruleType = ItemDisplayRuleType.ParentedPrefab,
				followerPrefab = ItemDisplayModel,
				childName = "Head",
				localPos = new Vector3(-0.07427f, 0.28406f, 0.25414f),
				localAngles = new Vector3(346.5472f, 88.74113f, 78.83873f),
				localScale = new Vector3(0.02f, 0.02f, 0.02f)
			}
		});
		itemDisplayRuleDict.Add("mdlHuntress", new ItemDisplayRule[1]
		{
			new ItemDisplayRule
			{
				ruleType = ItemDisplayRuleType.ParentedPrefab,
				followerPrefab = ItemDisplayModel,
				childName = "Head",
				localPos = new Vector3(0.11369f, 0.28323f, -0.16288f),
				localAngles = new Vector3(11.18791f, 261.8953f, 54.63395f),
				localScale = new Vector3(0.02f, 0.02f, 0.02f)
			}
		});
		itemDisplayRuleDict.Add("mdlToolbot", new ItemDisplayRule[1]
		{
			new ItemDisplayRule
			{
				ruleType = ItemDisplayRuleType.ParentedPrefab,
				followerPrefab = ItemDisplayModel,
				childName = "Head",
				localPos = new Vector3(0.44511f, 3.15501f, -1.2433f),
				localAngles = new Vector3(1.94447f, 267.1521f, 31.34281f),
				localScale = new Vector3(0.2f, 0.2f, 0.2f)
			}
		});
		itemDisplayRuleDict.Add("mdlEngi", new ItemDisplayRule[1]
		{
			new ItemDisplayRule
			{
				childName = "HeadCenter",
				localPos = new Vector3(-0.0911f, 0.00289f, 0.2191f),
				localAngles = new Vector3(353.7177f, 90.51432f, 104.3286f),
				localScale = new Vector3(0.02f, 0.02f, 0.02f)
			}
		});
		itemDisplayRuleDict.Add("mdlMage", new ItemDisplayRule[1]
		{
			new ItemDisplayRule
			{
				ruleType = ItemDisplayRuleType.ParentedPrefab,
				followerPrefab = ItemDisplayModel,
				childName = "Head",
				localPos = new Vector3(0.00177f, 0.19975f, -0.2197f),
				localAngles = new Vector3(3.98929f, 267.1229f, 39.76271f),
				localScale = new Vector3(0.02f, 0.02f, 0.02f)
			}
		});
		itemDisplayRuleDict.Add("mdlMerc", new ItemDisplayRule[1]
		{
			new ItemDisplayRule
			{
				ruleType = ItemDisplayRuleType.ParentedPrefab,
				followerPrefab = ItemDisplayModel,
				childName = "Pelvis",
				localPos = new Vector3(-0.00927f, 0.20246f, 0.07204f),
				localAngles = new Vector3(7.90918f, 260.0016f, 267.6993f),
				localScale = new Vector3(0.03f, 0.03f, 0.03f)
			}
		});
		itemDisplayRuleDict.Add("mdlTreebot", new ItemDisplayRule[1]
		{
			new ItemDisplayRule
			{
				ruleType = ItemDisplayRuleType.ParentedPrefab,
				followerPrefab = ItemDisplayModel,
				childName = "WeaponPlatform",
				localPos = new Vector3(0.04246f, -0.68576f, 0.57164f),
				localAngles = new Vector3(359.4588f, 285.2304f, 214.6219f),
				localScale = new Vector3(0.05f, 0.05f, 0.05f)
			}
		});
		itemDisplayRuleDict.Add("mdlLoader", new ItemDisplayRule[1]
		{
			new ItemDisplayRule
			{
				ruleType = ItemDisplayRuleType.ParentedPrefab,
				followerPrefab = ItemDisplayModel,
				childName = "Head",
				localPos = new Vector3(0.00482f, 0.27142f, -0.14343f),
				localAngles = new Vector3(2.69595f, 264.5459f, 50.40258f),
				localScale = new Vector3(0.02f, 0.02f, 0.02f)
			}
		});
		itemDisplayRuleDict.Add("mdlCroco", new ItemDisplayRule[1]
		{
			new ItemDisplayRule
			{
				ruleType = ItemDisplayRuleType.ParentedPrefab,
				followerPrefab = ItemDisplayModel,
				childName = "Pelvis",
				localPos = new Vector3(0.00145f, 2.2478f, 2.90663f),
				localAngles = new Vector3(357.4109f, 267.9452f, 276.0185f),
				localScale = new Vector3(0.15f, 0.15f, 0.15f)
			}
		});
		itemDisplayRuleDict.Add("mdlCaptain", new ItemDisplayRule[4]
		{
			new ItemDisplayRule
			{
				ruleType = ItemDisplayRuleType.ParentedPrefab,
				followerPrefab = ItemDisplayModel,
				childName = "Head",
				localPos = new Vector3(-0.26511f, 0.22384f, -0.1208f),
				localAngles = new Vector3(359.8135f, 332.0312f, 48.96695f),
				localScale = new Vector3(0.02f, 0.02f, 0.02f)
			},
			new ItemDisplayRule
			{
				ruleType = ItemDisplayRuleType.ParentedPrefab,
				followerPrefab = ItemDisplayModel,
				childName = "Head",
				localPos = new Vector3(-0.2532f, 0.17021f, -0.16034f),
				localAngles = new Vector3(359.8135f, 332.0312f, 48.96695f),
				localScale = new Vector3(0.02f, 0.02f, 0.02f)
			},
			new ItemDisplayRule
			{
				ruleType = ItemDisplayRuleType.ParentedPrefab,
				followerPrefab = ItemDisplayModel,
				childName = "Head",
				localPos = new Vector3(0.25728f, 0.22964f, -0.12746f),
				localAngles = new Vector3(357.0807f, 214.2728f, 48.67583f),
				localScale = new Vector3(0.02f, 0.02f, 0.02f)
			},
			new ItemDisplayRule
			{
				ruleType = ItemDisplayRuleType.ParentedPrefab,
				followerPrefab = ItemDisplayModel,
				childName = "Head",
				localPos = new Vector3(0.27044f, 0.13057f, -0.18171f),
				localAngles = new Vector3(357.0807f, 214.2728f, 63.80047f),
				localScale = new Vector3(0.02f, 0.02f, 0.02f)
			}
		});
		itemDisplayRuleDict.Add("mdlBandit2", new ItemDisplayRule[1]
		{
			new ItemDisplayRule
			{
				ruleType = ItemDisplayRuleType.ParentedPrefab,
				followerPrefab = ItemDisplayModel,
				childName = "Chest",
				localPos = new Vector3(0.16195f, 0.32996f, -0.27335f),
				localAngles = new Vector3(357.2213f, 251.3981f, 61.89581f),
				localScale = new Vector3(0.02f, 0.02f, 0.02f)
			}
		});
		itemDisplayRuleDict.Add("mdlScav", new ItemDisplayRule[1]
		{
			new ItemDisplayRule
			{
				ruleType = ItemDisplayRuleType.ParentedPrefab,
				followerPrefab = ItemDisplayModel,
				childName = "Head",
				localPos = new Vector3(-5.77488f, 5.88629f, -1.52399f),
				localAngles = new Vector3(355.0395f, 100.0505f, 321.2255f),
				localScale = new Vector3(0.5f, 0.5f, 0.5f)
			}
		});
		itemDisplayRuleDict.Add("mdlEngiTurret", new ItemDisplayRule[1]
		{
			new ItemDisplayRule
			{
				ruleType = ItemDisplayRuleType.ParentedPrefab,
				followerPrefab = ItemDisplayModel,
				childName = "Head",
				localPos = new Vector3(-0.04615f, 1.58001f, -1.84334f),
				localAngles = new Vector3(1.73079f, 270.4828f, 40.38388f),
				localScale = new Vector3(0.2f, 0.2f, 0.2f)
			}
		});
		itemDisplayRuleDict.Add("mdlMEL-T2", new ItemDisplayRule[1]
		{
			new ItemDisplayRule
			{
				ruleType = ItemDisplayRuleType.ParentedPrefab,
				followerPrefab = ItemDisplayModel,
				childName = "Body",
				localPos = new Vector3(-0.00209f, 0.06079f, -0.07634f),
				localAngles = new Vector3(3.24919f, 273.0562f, 60.90771f),
				localScale = new Vector3(0.005f, 0.005f, 0.005f)
			}
		});
		itemDisplayRuleDict.Add("mdlRailGunner", new ItemDisplayRule[1]
		{
			new ItemDisplayRule
			{
				ruleType = ItemDisplayRuleType.ParentedPrefab,
				followerPrefab = ItemDisplayModel,
				childName = "Backpack",
				localPos = new Vector3(-0.33793f, 0.29391f, 0.08776f),
				localAngles = new Vector3(0f, 0f, 90.19666f),
				localScale = new Vector3(0.02f, 0.02f, 0.02f)
			}
		});
		itemDisplayRuleDict.Add("mdlVoidSurvivor", new ItemDisplayRule[1]
		{
			new ItemDisplayRule
			{
				ruleType = ItemDisplayRuleType.ParentedPrefab,
				followerPrefab = ItemDisplayModel,
				childName = "Head",
				localPos = new Vector3(0.11872f, 0.1176f, 0.11456f),
				localAngles = new Vector3(13.19359f, 121.3558f, 35.55536f),
				localScale = new Vector3(0.03f, 0.03f, 0.03f)
			}
		});
	}
}
