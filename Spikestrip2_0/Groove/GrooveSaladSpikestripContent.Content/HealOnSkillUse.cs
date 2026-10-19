using System;
using R2API;
using R2API.Networking;
using R2API.Networking.Interfaces;
using RoR2;
using UnityEngine;
using UnityEngine.Networking;
using Spikestrip2_0.Core;

namespace GrooveSaladSpikestripContent.Content;

public class HealOnSkillUse : ItemBase<HealOnSkillUse>
{
	public class HealOnSkillUseBehaviour : CharacterBody.ItemBehavior
	{
		public class WaitForHealAuthority : MonoBehaviour
		{
			public GenericSkill trackedSkill;

			private bool sentHeal;

			public void FixedUpdate()
			{
				if (!trackedSkill || ((bool)trackedSkill && !trackedSkill.stateMachine))
				{
					UnityEngine.Object.Destroy(this);
				}
				else if (!((trackedSkill.stateMachine.state ?? trackedSkill.stateMachine.nextState).GetType() == trackedSkill.activationState.stateType) && !sentHeal)
				{
					HealOnSkillUseBehaviour component = GetComponent<HealOnSkillUseBehaviour>();
					if ((bool)component)
					{
						component.HealFromSkillAuthority(trackedSkill);
					}
					sentHeal = true;
					UnityEngine.Object.Destroy(this);
				}
			}
		}

		public class SkillUseHealServer : INetMessage, ISerializableObject
		{
			private float HealAmount;

			private NetworkInstanceId GameObjectID;

			public SkillUseHealServer()
			{
			}

			public SkillUseHealServer(float healAmount, NetworkInstanceId gameObjectID)
			{
				HealAmount = healAmount;
				GameObjectID = gameObjectID;
			}

			public void Serialize(NetworkWriter writer)
			{
				writer.Write(HealAmount);
				writer.Write(GameObjectID);
			}

			public void Deserialize(NetworkReader reader)
			{
				HealAmount = reader.ReadSingle();
				GameObjectID = reader.ReadNetworkId();
			}

			public void OnReceived()
			{
				if (!NetworkServer.active)
				{
					return;
				}
				GameObject gameObject = Util.FindNetworkObject(GameObjectID);
				if ((bool)gameObject)
				{
					HealthComponent component = gameObject.GetComponent<HealthComponent>();
					if ((bool)component)
					{
						component.Heal(HealAmount, default(ProcChainMask));
					}
				}
			}
		}

		public void HealFromSkillAuthority(GenericSkill genericSkill)
		{
			int num = Mathf.Min(genericSkill.rechargeStock, genericSkill.maxStock);
			int num2 = genericSkill.maxStock - num;
			if (genericSkill.stock > num2)
			{
				return;
			}
			HealthComponent healthComponent = body.healthComponent;
			if (!healthComponent)
			{
				return;
			}
			Util.PlaySound("Play_item_proc_crit_heal", base.gameObject);
			float num3 = 0.01f * healthComponent.fullHealth * (float)stack + 4f;
			if (NetworkServer.active)
			{
				healthComponent.Heal(num3, default(ProcChainMask));
				return;
			}
			NetworkIdentity component = body.GetComponent<NetworkIdentity>();
			if ((bool)component)
			{
				NetMessageExtensions.Send((INetMessage)(object)new SkillUseHealServer(num3, component.netId), (NetworkDestination)2);
			}
		}

		public void Start()
		{
			body.onSkillActivatedAuthority += OnSkillActivatedAuthority;
		}

		public void OnDestroy()
		{
			body.onSkillActivatedAuthority -= OnSkillActivatedAuthority;
		}

		private void OnSkillActivatedAuthority(GenericSkill genericSkill)
		{
			// Skills that don't consume stock (e.g. Railgunner scoping) shouldn't trigger the smoothie.
			if (genericSkill.skillDef.stockToConsume <= 0)
			{
				return;
			}
			if (genericSkill.baseRechargeInterval > 0.5f)
			{
				if (!genericSkill.beginSkillCooldownOnSkillEnd)
				{
					HealFromSkillAuthority(genericSkill);
					return;
				}
				WaitForHealAuthority waitForHealAuthority = body.gameObject.AddComponent<WaitForHealAuthority>();
				waitForHealAuthority.trackedSkill = genericSkill;
			}
		}
	}

	public override string ItemToken => "HEALONSKILLUSE";

	public override string ItemName => "Sunrise Smoothie";

	public override string ItemPickup => "Heal on skill use.";

	public override string ItemDescription => "<style=cIsHealing>Heal</style> for <style=cIsHealing>4</style> plus an additional <style=cIsHealing>1% <style=cStack>(+1% per stack)</style></style> of <style=cIsHealing>maximum health</style> upon using any skill.";

	public override string ItemLore => null;

	public override ItemTier Tier => ItemTier.Tier2;

	public override ItemTag[] ItemTags => new ItemTag[1] { ItemTag.Healing };

	public override Sprite ItemIconSprite => SpikestripContent.GrooveBundle.LoadAsset<Sprite>("texSmoothieIcon.png");

	public override Type ItemBehaviour => typeof(HealOnSkillUseBehaviour);

	public override bool ServerOnlyBehaviour => false;

	public override void Init()
	{
		base.Init();
		NetworkingAPI.RegisterMessageType<HealOnSkillUseBehaviour.SkillUseHealServer>();
	}

	public override void AssignItemModels()
	{
		ItemPickupModel = SpikestripUtil.UpdateGameObjectShader(SpikestripContent.GrooveBundle.LoadAsset<GameObject>("Smoothie.prefab"));
		Transform transform = ItemPickupModel.transform.Find("mdlSmoothie");
		Material material = LegacyResourcesAPI.Load<GameObject>("prefabs/pickupmodels/PickupBauble").transform.Find("BaubleSand").GetComponent<MeshRenderer>().material;
		MeshRenderer component = transform.GetComponent<MeshRenderer>();
		component.material = UnityEngine.Object.Instantiate(material);
		component.material.mainTexture = null;
		component.material.color = SpikestripUtil.ColorRGB(94f, 94f, 62f);
		component.material.DisableKeyword("FLOWMAP");
		Transform transform2 = ItemPickupModel.transform.Find("Glass");
		MeshRenderer component2 = transform2.GetComponent<MeshRenderer>();
		Material material2 = LegacyResourcesAPI.Load<GameObject>("prefabs/characterbodies/ArtifactShellBody").transform.Find("ModelBase/ArtifactShellModel").GetComponent<MeshRenderer>().sharedMaterials[0];
		component2.SetSharedMaterials(new Material[2]
		{
			SpikestripUtil.infusionGlassMaterial,
			material2
		}, 2);
		ItemDisplayModel = ItemPickupModel;
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
				childName = "Pelvis",
				localPos = new Vector3(0.16498f, -7E-05f, 0.12158f),
				localAngles = new Vector3(7.23964f, 355.4908f, 192.3736f),
				localScale = new Vector3(0.1275f, 0.1275f, 0.1275f)
			}
		});
		itemDisplayRuleDict.Add("mdlHuntress", new ItemDisplayRule[1]
		{
			new ItemDisplayRule
			{
				ruleType = ItemDisplayRuleType.ParentedPrefab,
				followerPrefab = ItemDisplayModel,
				childName = "Pelvis",
				localPos = new Vector3(0.17725f, 0.00436f, 0.09635f),
				localAngles = new Vector3(359.4211f, 310.0719f, 173.2909f),
				localScale = new Vector3(0.12877f, 0.12877f, 0.12877f)
			}
		});
		itemDisplayRuleDict.Add("mdlToolbot", new ItemDisplayRule[1]
		{
			new ItemDisplayRule
			{
				ruleType = ItemDisplayRuleType.ParentedPrefab,
				followerPrefab = ItemDisplayModel,
				childName = "Chest",
				localPos = new Vector3(-1.75356f, 0.00066f, -1.85672f),
				localAngles = new Vector3(349.1329f, 229.1895f, 17.45403f),
				localScale = new Vector3(0.86257f, 0.86257f, 0.86257f)
			}
		});
		itemDisplayRuleDict.Add("mdlEngi", new ItemDisplayRule[1]
		{
			new ItemDisplayRule
			{
				ruleType = ItemDisplayRuleType.ParentedPrefab,
				followerPrefab = ItemDisplayModel,
				childName = "Pelvis",
				localPos = new Vector3(0.14053f, 0.18327f, 0.18616f),
				localAngles = new Vector3(350.5919f, 310.1438f, 187.89f),
				localScale = new Vector3(0.15255f, 0.15255f, 0.15255f)
			}
		});
		itemDisplayRuleDict.Add("mdlMage", new ItemDisplayRule[1]
		{
			new ItemDisplayRule
			{
				ruleType = ItemDisplayRuleType.ParentedPrefab,
				followerPrefab = ItemDisplayModel,
				childName = "Pelvis",
				localPos = new Vector3(0.23095f, 0.27055f, 0.07664f),
				localAngles = new Vector3(16.19548f, 356.2919f, 183.1194f),
				localScale = new Vector3(0.1112f, 0.1112f, 0.1112f)
			}
		});
		itemDisplayRuleDict.Add("mdlMerc", new ItemDisplayRule[1]
		{
			new ItemDisplayRule
			{
				ruleType = ItemDisplayRuleType.ParentedPrefab,
				followerPrefab = ItemDisplayModel,
				childName = "Pelvis",
				localPos = new Vector3(0.15907f, 0.19584f, 0.12209f),
				localAngles = new Vector3(351.8815f, 296.6431f, 170.4009f),
				localScale = new Vector3(0.12346f, 0.12346f, 0.12346f)
			}
		});
		itemDisplayRuleDict.Add("mdlTreebot", new ItemDisplayRule[1]
		{
			new ItemDisplayRule
			{
				ruleType = ItemDisplayRuleType.ParentedPrefab,
				followerPrefab = ItemDisplayModel,
				childName = "FlowerBase",
				localPos = new Vector3(-0.04297f, 1.1141f, -0.65274f),
				localAngles = new Vector3(354.9275f, 244.3535f, 1.9737f),
				localScale = new Vector3(0.37975f, 0.37975f, 0.37975f)
			}
		});
		itemDisplayRuleDict.Add("mdlLoader", new ItemDisplayRule[1]
		{
			new ItemDisplayRule
			{
				ruleType = ItemDisplayRuleType.ParentedPrefab,
				followerPrefab = ItemDisplayModel,
				childName = "Pelvis",
				localPos = new Vector3(0.23052f, 0.24865f, 0.15326f),
				localAngles = new Vector3(15.60437f, 328.7188f, 176.2106f),
				localScale = new Vector3(0.11998f, 0.11998f, 0.11998f)
			}
		});
		itemDisplayRuleDict.Add("mdlCroco", new ItemDisplayRule[1]
		{
			new ItemDisplayRule
			{
				ruleType = ItemDisplayRuleType.ParentedPrefab,
				followerPrefab = ItemDisplayModel,
				childName = "Pelvis",
				localPos = new Vector3(1.53747f, 2.41937f, 1.04117f),
				localAngles = new Vector3(325.9153f, 301.0302f, 202.5956f),
				localScale = new Vector3(1.47837f, 1.47837f, 1.47837f)
			}
		});
		itemDisplayRuleDict.Add("mdlCaptain", new ItemDisplayRule[1]
		{
			new ItemDisplayRule
			{
				ruleType = ItemDisplayRuleType.ParentedPrefab,
				followerPrefab = ItemDisplayModel,
				childName = "Pelvis",
				localPos = new Vector3(0.37326f, 0.18336f, 0.13371f),
				localAngles = new Vector3(8.75352f, 330.5828f, 172.4541f),
				localScale = new Vector3(0.20691f, 0.20691f, 0.20691f)
			}
		});
		itemDisplayRuleDict.Add("mdlBandit2", new ItemDisplayRule[1]
		{
			new ItemDisplayRule
			{
				ruleType = ItemDisplayRuleType.ParentedPrefab,
				followerPrefab = ItemDisplayModel,
				childName = "Pelvis",
				localPos = new Vector3(0.23159f, 0.04986f, -0.08552f),
				localAngles = new Vector3(16.31518f, 9.15419f, 170.1658f),
				localScale = new Vector3(0.13206f, 0.13206f, 0.13206f)
			}
		});
		itemDisplayRuleDict.Add("mdlRailGunner", new ItemDisplayRule[1]
		{
			new ItemDisplayRule
			{
				ruleType = ItemDisplayRuleType.ParentedPrefab,
				followerPrefab = ItemDisplayModel,
				childName = "Pelvis",
				localPos = new Vector3(0.21485f, 0.28715f, 0.14081f),
				localAngles = new Vector3(30.42123f, 10.19867f, 172.7789f),
				localScale = new Vector3(0.13206f, 0.13206f, 0.13206f)
			}
		});
		itemDisplayRuleDict.Add("mdlVoidSurvivor", new ItemDisplayRule[1]
		{
			new ItemDisplayRule
			{
				ruleType = ItemDisplayRuleType.ParentedPrefab,
				followerPrefab = ItemDisplayModel,
				childName = "Pelvis",
				localPos = new Vector3(0.01283f, 0.1037f, -0.21619f),
				localAngles = new Vector3(8.75063f, 202.0849f, 197.3424f),
				localScale = new Vector3(0.13206f, 0.13206f, 0.13206f)
			}
		});
		itemDisplayRuleDict.Add("mdlScav", new ItemDisplayRule[1]
		{
			new ItemDisplayRule
			{
				ruleType = ItemDisplayRuleType.ParentedPrefab,
				followerPrefab = ItemDisplayModel,
				childName = "Pelvis",
				localPos = new Vector3(-7.4532f, 6.50683f, 0.51419f),
				localAngles = new Vector3(356.5008f, 163.627f, 187.0833f),
				localScale = new Vector3(1.79252f, 1.79252f, 1.79252f)
			}
		});
		itemDisplayRuleDict.Add("mdlEngiTurret", new ItemDisplayRule[1]
		{
			new ItemDisplayRule
			{
				ruleType = ItemDisplayRuleType.ParentedPrefab,
				followerPrefab = ItemDisplayModel,
				childName = "Base",
				localPos = new Vector3(-0.83256f, 1.90687f, 0.00129f),
				localAngles = new Vector3(354.9125f, 345.6655f, 3.33064f),
				localScale = new Vector3(0.50893f, 0.50893f, 0.50893f)
			}
		});
	}
}
