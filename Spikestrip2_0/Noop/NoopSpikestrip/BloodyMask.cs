using System;
using System.Collections.ObjectModel;
using GrooveSaladSpikestripContent;
using On.RoR2;
using R2API;
using RoR2;
using UnityEngine;
using Spikestrip2_0.Core;

namespace NoopSpikestrip;

internal class BloodyMask : ItemBase<BloodyMask>
{
	public class MaskBehavior : RoR2.CharacterBody.ItemBehavior, IOnIncomingDamageServerReceiver
	{
		public float Timer;

		public GameObject effect = SpikestripUtil.AddressablesLoad<GameObject>("0734bb48bb67cb6468a02ee1dfc4628e");

		public void FixedUpdate()
		{
			int num = SpikestripUtil.StackScaling(5, 3, stack);
			int buffCount = body.GetBuffCount(bloody);
			if (body.outOfCombat || body.HasBuff(bloodycd))
			{
				return;
			}
			Timer -= Time.fixedDeltaTime;
			if (Timer <= 0f && buffCount < num)
			{
				Timer += 2f;
				if ((bool)body)
				{
					body.AddBuff(bloody);
				}
			}
		}

		public void OnIncomingDamageServer(RoR2.DamageInfo damageInfo)
		{
			if (damageInfo.attacker != body.gameObject)
			{
				body.AddTimedBuff(bloodycd, 5f);
				body.SetBuffCount(bloody.buffIndex, 0);
				Timer = 0f;
			}
		}

		public void Start()
		{
			if ((bool)body.healthComponent)
			{
				ref IOnIncomingDamageServerReceiver[] onIncomingDamageReceivers = ref body.healthComponent.onIncomingDamageReceivers;
				IOnIncomingDamageServerReceiver value = this;
				SpikestripUtil.AppendArrayIfMissing(ref onIncomingDamageReceivers, in value);
			}
		}

		public void OnDestroy()
		{
			if ((bool)body && (bool)body.healthComponent)
			{
				ref IOnIncomingDamageServerReceiver[] onIncomingDamageReceivers = ref body.healthComponent.onIncomingDamageReceivers;
				IOnIncomingDamageServerReceiver value = this;
				SpikestripUtil.RemoveFromArray(ref onIncomingDamageReceivers, in value);
				for (int num = body.GetBuffCount(bloody); num >= 0; num--)
				{
					body.RemoveBuff(bloody);
				}
			}
		}
	}

	public static RoR2.BuffDef bloody;

	public static RoR2.BuffDef bloodycd;

	public override string ItemName => "Bloody Surgical Mask";

	public override string ItemToken => "BloodMask";

	public override string ItemPickup => "Gain a stacking chance to bleed enemies during combat. Taking damage removes all stacks and puts the item on cooldown.";

	public override string ItemDescription => "For every <style=cIsUtility>2 seconds</style> spent in combat, gain a <style=cIsdamage>5% chance to bleed enemies</style>. The buff can be stacked <style=cIsUtility>5</style> <style=cStack>(+3 per stack)</style> times. Upon getting hit lose all stacks and go on a 5 second cooldown";

	public override string ItemLore => "Silence. All that could be heard was a dull beeping sound. As she reached out, an assistant quickly prepared a scalpal for her. This was her 100th successful surgery without fail. It had to be. \n\nThe patient had a bullet lodged in between the ribcage and the left lung. She made an incision. Tension filled the room; even she looked nervous. She reached out her hand again, and was supplied with tweezers. She carefully maneuvered the tweezers down towards the bullet. This was it. At least, it should've been.\n\nA cough.\n\nHarmless to most, sure. However, it startled her enough to make her lose her focus. The tweezers punctured his lungs. In a matter of seconds, the room was filled with an ear piercing beep.\n\nHe is gone.\n\n100 perfect surgeries, gone.\n\nOne cough.\n\nShe Snapped.\n\nWith a crazed look in her eyes, she lunged at him. Still stunned from what he had done, he didn't even see her coming until it was too late.\n\nWas it really worth the pay Jacob? <style=cMono>\n=======================================================================================\nFrom the testimony of security guard Michael D. Kristie \nTrial 3748: Erica S. Steam V Jacob L. Smith</style>";

	public override ItemTier Tier => ItemTier.Tier2;

	public override Sprite ItemIconSprite => SpikestripContent.NoopBundle.LoadAsset<Sprite>("SurgeonIcon.png");

	public override bool ServerOnlyBehaviour => true;

	public override Type ItemBehaviour => typeof(MaskBehavior);

	public override void AssignItemModels()
	{
		GameObject gameObject = SpikestripContent.NoopBundle.LoadAsset<GameObject>("Surgical.prefab");
		ItemPickupModel = SpikestripUtil.UpdateGameObjectShader(gameObject);
		ItemDisplayModel = PrefabAPI.InstantiateClone(gameObject, "CartDisplay", false);
		RoR2.ItemDisplay itemDisplay = ItemDisplayModel.AddComponent<RoR2.ItemDisplay>();
		itemDisplay.rendererInfos = SpikestripUtil.GenerateRendererInfos(ItemDisplayModel);
		ItemDisplayModel = SpikestripUtil.UpdateGameObjectShader(ItemDisplayModel);
	}

	public override void Init()
	{
		base.Init();
		bloody = ScriptableObject.CreateInstance<RoR2.BuffDef>();
		bloody.canStack = true;
		bloody.buffColor = SpikestripUtil.ColorRGB(166f, 18f, 45f);
		bloody.iconSprite = SpikestripContent.NoopBundle.LoadAsset<Sprite>("Noopfakebuff.png");
		ContentAddition.AddBuffDef(bloody);
		bloodycd = ScriptableObject.CreateInstance<RoR2.BuffDef>();
		bloodycd.canStack = false;
		bloodycd.isCooldown = true;
		bloodycd.iconSprite = SpikestripContent.NoopBundle.LoadAsset<Sprite>("Noopfakecd.png");
		bloodycd.buffColor = SpikestripUtil.ColorRGB(82f, 81f, 81f);
		ContentAddition.AddBuffDef(bloodycd);
		RecalculateStatsAPI.GetStatCoefficients += RecalculateStatsAPI_GetStatCoefficients;
	}

	public static int FetchTotalItemCount(TeamIndex index, bool requiresAlive, bool requiresConnected = true)
	{
		ReadOnlyCollection<RoR2.CharacterMaster> readOnlyInstancesList = RoR2.CharacterMaster.readOnlyInstancesList;
		int num = 0;
		for (int i = 0; i < readOnlyInstancesList.Count; i++)
		{
			RoR2.CharacterMaster characterMaster = readOnlyInstancesList[i];
			if (characterMaster.teamIndex == index && (!requiresAlive || characterMaster.hasBody) && (!requiresConnected || !characterMaster.playerCharacterMasterController || characterMaster.playerCharacterMasterController.isConnected))
			{
				num += characterMaster.inventory.GetTotalItemCount();
			}
		}
		return num;
	}

	private void RecalculateStatsAPI_GetStatCoefficients(RoR2.CharacterBody sender, RecalculateStatsAPI.StatHookEventArgs args)
	{
		args.bleedChanceAdd += 5f * sender.GetBuffCount(bloody);
	}

	public override void PopulateItemDisplayRules(ItemDisplayRuleDict itemDisplayRuleDict)
	{
		itemDisplayRuleDict.Add("mdlCommandoDualies", new RoR2.ItemDisplayRule[1]
		{
			new RoR2.ItemDisplayRule
			{
				ruleType = ItemDisplayRuleType.ParentedPrefab,
				followerPrefab = ItemDisplayModel,
				childName = "Head",
				localPos = new Vector3(0.00373f, 0.18251f, 0.1307f),
				localAngles = new Vector3(356.662f, 1.95787f, 358.1002f),
				localScale = new Vector3(0.16f, 0.16f, 0.16f)
			}
		});
		itemDisplayRuleDict.Add("mdlHuntress", new RoR2.ItemDisplayRule[1]
		{
			new RoR2.ItemDisplayRule
			{
				ruleType = ItemDisplayRuleType.ParentedPrefab,
				followerPrefab = ItemDisplayModel,
				childName = "Head",
				localPos = new Vector3(0.00106f, 0.16048f, 0.07194f),
				localAngles = new Vector3(355.0983f, 5.07518f, 0.06199f),
				localScale = new Vector3(0.14f, 0.14f, 0.14f)
			}
		});
		itemDisplayRuleDict.Add("mdlToolbot", new RoR2.ItemDisplayRule[1]
		{
			new RoR2.ItemDisplayRule
			{
				ruleType = ItemDisplayRuleType.ParentedPrefab,
				followerPrefab = ItemDisplayModel,
				childName = "Head",
				localPos = new Vector3(0.44267f, 2.86875f, -0.89489f),
				localAngles = new Vector3(307.3764f, 185.3539f, 355.2917f),
				localScale = new Vector3(0.66148f, 0.7329f, 1f)
			}
		});
		itemDisplayRuleDict.Add("mdlEngi", new RoR2.ItemDisplayRule[1]
		{
			new RoR2.ItemDisplayRule
			{
				ruleType = ItemDisplayRuleType.ParentedPrefab,
				followerPrefab = ItemDisplayModel,
				childName = "HeadCenter",
				localPos = new Vector3(-0.00864f, -0.00226f, 0.08915f),
				localAngles = new Vector3(356.3204f, 354.9214f, 359.2408f),
				localScale = new Vector3(0.16128f, 0.16128f, 0.16128f)
			}
		});
		itemDisplayRuleDict.Add("mdlMage", new RoR2.ItemDisplayRule[1]
		{
			new RoR2.ItemDisplayRule
			{
				ruleType = ItemDisplayRuleType.ParentedPrefab,
				followerPrefab = ItemDisplayModel,
				childName = "Head",
				localPos = new Vector3(-0.00069f, 0.01666f, 0.08016f),
				localAngles = new Vector3(15.68266f, 7.53128f, 0.85813f),
				localScale = new Vector3(0.1f, 0.1f, 0.1f)
			}
		});
		itemDisplayRuleDict.Add("mdlMerc", new RoR2.ItemDisplayRule[1]
		{
			new RoR2.ItemDisplayRule
			{
				ruleType = ItemDisplayRuleType.ParentedPrefab,
				followerPrefab = ItemDisplayModel,
				childName = "Head",
				localPos = new Vector3(-0.00162f, 0.07753f, 0.12121f),
				localAngles = new Vector3(10.95928f, 356.7965f, 0.48696f),
				localScale = new Vector3(0.12f, 0.12f, 0.12f)
			}
		});
		itemDisplayRuleDict.Add("mdlTreebot", new RoR2.ItemDisplayRule[1]
		{
			new RoR2.ItemDisplayRule
			{
				ruleType = ItemDisplayRuleType.ParentedPrefab,
				followerPrefab = ItemDisplayModel,
				childName = "CalfBackR",
				localPos = new Vector3(-0.00362f, 0.64628f, -0.20254f),
				localAngles = new Vector3(307.4982f, 86.70875f, 271.2927f),
				localScale = new Vector3(0.2f, 0.2f, 0.2f)
			}
		});
		itemDisplayRuleDict.Add("mdlLoader", new RoR2.ItemDisplayRule[1]
		{
			new RoR2.ItemDisplayRule
			{
				ruleType = ItemDisplayRuleType.ParentedPrefab,
				followerPrefab = ItemDisplayModel,
				childName = "Head",
				localPos = new Vector3(-0.0026f, 0.04271f, 0.08011f),
				localAngles = new Vector3(9.05266f, 358.1694f, 2.71994f),
				localScale = new Vector3(0.13f, 0.13f, 0.13f)
			}
		});
		itemDisplayRuleDict.Add("mdlCroco", new RoR2.ItemDisplayRule[1]
		{
			new RoR2.ItemDisplayRule
			{
				ruleType = ItemDisplayRuleType.ParentedPrefab,
				followerPrefab = ItemDisplayModel,
				childName = "Head",
				localPos = new Vector3(-0.04091f, 0.49214f, 0.46709f),
				localAngles = new Vector3(16.75191f, 358.5439f, 358.2056f),
				localScale = new Vector3(2f, 2f, 2f)
			}
		});
		itemDisplayRuleDict.Add("mdlCaptain", new RoR2.ItemDisplayRule[1]
		{
			new RoR2.ItemDisplayRule
			{
				ruleType = ItemDisplayRuleType.ParentedPrefab,
				followerPrefab = ItemDisplayModel,
				childName = "Head",
				localPos = new Vector3(0.00116f, 0.02018f, 0.11301f),
				localAngles = new Vector3(10.43888f, 358.6753f, 1.22045f),
				localScale = new Vector3(0.1f, 0.1f, 0.1f)
			}
		});
		itemDisplayRuleDict.Add("mdlBandit2", new RoR2.ItemDisplayRule[1]
		{
			new RoR2.ItemDisplayRule
			{
				ruleType = ItemDisplayRuleType.ParentedPrefab,
				followerPrefab = ItemDisplayModel,
				childName = "Chest",
				localPos = new Vector3(-0.00349f, -0.11756f, -0.14204f),
				localAngles = new Vector3(42.05348f, 51.29253f, 235.9125f),
				localScale = new Vector3(0.1f, 0.1f, 0.1f)
			}
		});
		itemDisplayRuleDict.Add("mdlScav", new RoR2.ItemDisplayRule[1]
		{
			new RoR2.ItemDisplayRule
			{
				ruleType = ItemDisplayRuleType.ParentedPrefab,
				followerPrefab = ItemDisplayModel,
				childName = "Head",
				localPos = new Vector3(5.09172f, 4.10613f, -0.67357f),
				localAngles = new Vector3(300.3369f, 149.0084f, 208.0921f),
				localScale = new Vector3(2f, 2f, 2f)
			}
		});
		itemDisplayRuleDict.Add("mdlRailGunner", new RoR2.ItemDisplayRule[1]
		{
			new RoR2.ItemDisplayRule
			{
				ruleType = ItemDisplayRuleType.ParentedPrefab,
				followerPrefab = ItemDisplayModel,
				childName = "GunScope",
				localPos = new Vector3(-0.07483f, -0.15121f, 0.31329f),
				localAngles = new Vector3(352.7529f, 354.4491f, 358.4098f),
				localScale = new Vector3(0.05f, 0.05f, 0.05f)
			}
		});
		itemDisplayRuleDict.Add("mdlVoidSurvivor", new RoR2.ItemDisplayRule[1]
		{
			new RoR2.ItemDisplayRule
			{
				ruleType = ItemDisplayRuleType.ParentedPrefab,
				followerPrefab = ItemDisplayModel,
				childName = "Head",
				localPos = new Vector3(0.06568f, 0.16389f, 0.04438f),
				localAngles = new Vector3(298.6464f, 73.51062f, 357.4098f),
				localScale = new Vector3(0.15f, 0.15f, 0.15f)
			}
		});
	}
}
