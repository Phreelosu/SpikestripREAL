using System;
using System.Collections.Generic;
using EntityStates;
using GrooveSaladSpikestripContent;
using On.RoR2;
using PlasmaCoreSpikestripContent.Core;
using PlasmaCoreSpikestripContent.Networking;
using R2API;
using R2API.Networking;
using R2API.Networking.Interfaces;
using R2API.Utils;
using RoR2;
using UnityEngine;
using UnityEngine.Networking;
using Spikestrip2_0.Core;

namespace PlasmaCoreSpikestripContent.Content.Equipment;

public class ImpulseFrostShield : EquipmentBase<ImpulseFrostShield>
{
	public class ImpulseFrostTargeter : EquipmentSingleTargetBehaviour
	{
		public static Dictionary<int, ImpulseFrostTargeter> instances = new Dictionary<int, ImpulseFrostTargeter>();

		public bool targetIsValid;

		public bool forceTargetIsValid;

		public override float MaxDistance => 225f;

		public override float MaxAngleFilter => 10f;

		public override RoR2.BullseyeSearch.SortMode SortMode => RoR2.BullseyeSearch.SortMode.Angle;

		public override TargetOption TargetType => TargetOption.Enemy;

		public override void OnAwake()
		{
			base.OnAwake();
			instances.Add(GetInstanceID(), this);
		}

		public override GameObject GetTargetIndicatorPrefab(RoR2.EquipmentSlot.UserTargetInfo info)
		{
			if ((bool)info.hurtBox && (bool)info.hurtBox.healthComponent && (bool)info.hurtBox.healthComponent.body)
			{
				if (NetworkServer.active)
				{
					bool flag = Reflection.GetFieldValue<float>((object)info.hurtBox.healthComponent.body, "outOfCombatStopwatch") <= 0.75f;
					if (flag && !targetIsValid)
					{
						RoR2.Util.PlaySound("Play_engi_seekerMissile_lockOn", body.gameObject);
					}
					targetIsValid = flag;
				}
				else
				{
					NetMessageExtensions.Send((INetMessage)(object)new IFSClientTargetRequestNetworkMessage
					{
						targeterInstance = base.gameObject.GetComponent<NetworkIdentity>(),
						bodyGameObject = info.hurtBox.healthComponent.body.gameObject
					}, (NetworkDestination)2);
				}
			}
			if (targetIsValid || forceTargetIsValid)
			{
				return PlasmaCorePlugin.crosshairIFSActive;
			}
			return PlasmaCorePlugin.crosshairIFSInactive;
		}
	}

	public static SSEquipBaseSO scriptableObject;

	public override string EquipmentName => "Impulse Frost Shield";

	public override string EquipmentToken => scriptableObject.EquipToken;

	public override string EquipmentPickup => scriptableObject.EquipPickup;

	public override string EquipmentDescription => scriptableObject.EquipDescription;

	public override string EquipmentLore => "";

	public override Sprite EquipmentIcon => scriptableObject.Icon;

	public override Type EquipmentBehaviour => typeof(ImpulseFrostTargeter);

	public override float Cooldown => scriptableObject.Cooldown;

	public unsafe override void Init()
	{
		On.RoR2.EquipmentSlot.CallCmdExecuteIfReady += EquipmentSlot_CallCmdExecuteIfReady;
		PlasmaCorePlugin.equipBaseScriptableObjects.TryGetValue(EquipmentName, out scriptableObject);
		if ((bool)scriptableObject)
		{
			EquipmentPickupModel = scriptableObject.pickupObject;
			EquipmentDisplayModel = scriptableObject.displayObject;
			PlasmaLog.Log("Found associated ScriptableObject for EquipBase " + EquipmentName);
		}
		else
		{
			PlasmaLog.LogWarn("Missing associated ScriptableObject for EquipBase " + EquipmentName);
		}
		base.Init();
	}

	private void EquipmentSlot_CallCmdExecuteIfReady(On.RoR2.EquipmentSlot.orig_CallCmdExecuteIfReady orig, RoR2.EquipmentSlot self, RoR2.EquipmentSlot.NetUserTargetInfo netUserTargetInfo)
	{
		if (self.equipmentIndex == EquipmentDef.equipmentIndex)
		{
			ImpulseFrostTargeter component = self.GetComponent<ImpulseFrostTargeter>();
			NetMessageExtensions.Send((INetMessage)(object)new IFSACtivateEquipmentNetworkMessage
			{
				targeterInstance = self.gameObject.GetComponent<NetworkIdentity>(),
				isValid = component.targetIsValid
			}, (NetworkDestination)2);
		}
		else
		{
			orig(self, netUserTargetInfo);
		}
	}

	public override void AssignEquipmentValues()
	{
	}

	public override bool OnActivationServer(RoR2.EquipmentSlot equipmentSlot)
	{
		ImpulseFrostTargeter component = equipmentSlot.GetComponent<ImpulseFrostTargeter>();
		RoR2.HurtBox hurtBox = component.currentTarget.hurtBox;
		if ((bool)hurtBox && (bool)hurtBox.healthComponent)
		{
			RoR2.HealthComponent healthComponent = hurtBox.healthComponent;
			if ((bool)healthComponent && (bool)healthComponent.body)
			{
				float fieldValue = Reflection.GetFieldValue<float>((object)healthComponent.body, "outOfCombatStopwatch");
				if (component.targetIsValid || component.forceTargetIsValid)
				{
					component.forceTargetIsValid = false;
					RoR2.EntityStateMachine entityStateMachine = RoR2.EntityStateMachine.FindByCustomName(healthComponent.body.gameObject, "Body");
					RoR2.EntityStateMachine entityStateMachine2 = RoR2.EntityStateMachine.FindByCustomName(healthComponent.body.gameObject, "Weapon");
					if ((bool)entityStateMachine)
					{
						FrozenState frozenState = new FrozenState();
						frozenState.freezeDuration = 4f;
						entityStateMachine.SetInterruptState(frozenState, InterruptPriority.Taunt);
					}
					if ((bool)entityStateMachine2)
					{
						entityStateMachine2.SetNextStateToMain();
					}
					RoR2.Util.PlaySound("Play_mage_m2_shoot", equipmentSlot.characterBody.gameObject);
					RoR2.Util.PlaySound("Play_wCrit", healthComponent.gameObject);
					healthComponent.TakeDamage(new RoR2.DamageInfo
					{
						damage = healthComponent.fullCombinedHealth / 10f,
						crit = false,
						inflictor = equipmentSlot.characterBody.gameObject,
						attacker = equipmentSlot.characterBody.gameObject,
						force = Vector3.zero,
						position = healthComponent.transform.position,
						procCoefficient = 0f,
						damageType = DamageType.NonLethal,
						damageColorIndex = DamageColorIndex.Item
					});
					RoR2.EffectManager.SpawnEffect(PlasmaCorePlugin.freezeEffectIFS, new RoR2.EffectData
					{
						scale = healthComponent.body.bestFitRadius,
						origin = healthComponent.transform.position
					}, transmit: true);
					return true;
				}
				if (fieldValue <= 1.1f)
				{
					return false;
				}
				return true;
			}
		}
		return false;
	}

	public override void PopulateItemDisplayRules(ItemDisplayRuleDict itemDisplayRuleDict)
	{
		itemDisplayRuleDict.Add("mdlCommandoDualies", new RoR2.ItemDisplayRule[1]
		{
			new RoR2.ItemDisplayRule
			{
				ruleType = ItemDisplayRuleType.ParentedPrefab,
				followerPrefab = EquipmentDisplayModel,
				childName = "LowerArmL",
				localPos = new Vector3(0.00986f, 0.16259f, -0.05033f),
				localAngles = new Vector3(0.15186f, 359.9667f, 88.39442f),
				localScale = new Vector3(0.1036f, 0.1036f, 0.1036f)
			}
		});
		itemDisplayRuleDict.Add("mdlHuntress", new RoR2.ItemDisplayRule[1]
		{
			new RoR2.ItemDisplayRule
			{
				ruleType = ItemDisplayRuleType.ParentedPrefab,
				followerPrefab = EquipmentDisplayModel,
				childName = "LowerArmL",
				localPos = new Vector3(0.00125f, 0.14577f, -0.06114f),
				localAngles = new Vector3(356.1169f, 358.9397f, 105.2859f),
				localScale = new Vector3(0.1f, 0.1f, 0.1f)
			}
		});
		itemDisplayRuleDict.Add("mdlToolbot", new RoR2.ItemDisplayRule[1]
		{
			new RoR2.ItemDisplayRule
			{
				ruleType = ItemDisplayRuleType.ParentedPrefab,
				followerPrefab = EquipmentDisplayModel,
				childName = "LowerArmR",
				localPos = new Vector3(-0.15085f, 2.33217f, 0.72157f),
				localAngles = new Vector3(354.1447f, 180f, 90f),
				localScale = new Vector3(1f, 1f, 1f)
			}
		});
		itemDisplayRuleDict.Add("mdlEngi", new RoR2.ItemDisplayRule[1]
		{
			new RoR2.ItemDisplayRule
			{
				ruleType = ItemDisplayRuleType.ParentedPrefab,
				followerPrefab = EquipmentDisplayModel,
				childName = "LowerArmL",
				localPos = new Vector3(0.02823f, 0.13761f, -0.05102f),
				localAngles = new Vector3(356.7065f, 0f, 90f),
				localScale = new Vector3(0.1f, 0.1f, 0.1f)
			}
		});
		itemDisplayRuleDict.Add("mdlMage", new RoR2.ItemDisplayRule[1]
		{
			new RoR2.ItemDisplayRule
			{
				ruleType = ItemDisplayRuleType.ParentedPrefab,
				followerPrefab = EquipmentDisplayModel,
				childName = "UpperArmL",
				localPos = new Vector3(0.06414f, 0.18576f, 0.00454f),
				localAngles = new Vector3(355.1956f, 275.6926f, 90f),
				localScale = new Vector3(0.1f, 0.1f, 0.1f)
			}
		});
		itemDisplayRuleDict.Add("mdlMerc", new RoR2.ItemDisplayRule[1]
		{
			new RoR2.ItemDisplayRule
			{
				ruleType = ItemDisplayRuleType.ParentedPrefab,
				followerPrefab = EquipmentDisplayModel,
				childName = "LowerArmR",
				localPos = new Vector3(0.0061f, 0.16259f, -0.09576f),
				localAngles = new Vector3(356.4101f, 0f, 90f),
				localScale = new Vector3(0.1f, 0.1f, 0.1f)
			}
		});
		itemDisplayRuleDict.Add("mdlTreebot", new RoR2.ItemDisplayRule[1]
		{
			new RoR2.ItemDisplayRule
			{
				ruleType = ItemDisplayRuleType.ParentedPrefab,
				followerPrefab = EquipmentDisplayModel,
				childName = "FootFrontL",
				localPos = new Vector3(-0.00014f, 0.48531f, -0.2035f),
				localAngles = new Vector3(0f, 0f, 90f),
				localScale = new Vector3(0.25f, 0.25f, 0.25f)
			}
		});
		itemDisplayRuleDict.Add("mdlLoader", new RoR2.ItemDisplayRule[1]
		{
			new RoR2.ItemDisplayRule
			{
				ruleType = ItemDisplayRuleType.ParentedPrefab,
				followerPrefab = EquipmentDisplayModel,
				childName = "MechLowerArmL",
				localPos = new Vector3(0.02135f, 0.35605f, -0.14987f),
				localAngles = new Vector3(0f, 0f, 97.81449f),
				localScale = new Vector3(0.1f, 0.1f, 0.1f)
			}
		});
		itemDisplayRuleDict.Add("mdlCroco", new RoR2.ItemDisplayRule[1]
		{
			new RoR2.ItemDisplayRule
			{
				ruleType = ItemDisplayRuleType.ParentedPrefab,
				followerPrefab = EquipmentDisplayModel,
				childName = "LowerArmL",
				localPos = new Vector3(-1.15393f, 1.72311f, 0.13479f),
				localAngles = new Vector3(4.02749f, 107.6041f, 91.27666f),
				localScale = new Vector3(0.80125f, 0.80125f, 0.80125f)
			}
		});
		itemDisplayRuleDict.Add("mdlCaptain", new RoR2.ItemDisplayRule[1]
		{
			new RoR2.ItemDisplayRule
			{
				ruleType = ItemDisplayRuleType.ParentedPrefab,
				followerPrefab = EquipmentDisplayModel,
				childName = "LowerArmL",
				localPos = new Vector3(-0.02971f, 0.10757f, -0.06635f),
				localAngles = new Vector3(355.5442f, 37.41209f, 88.19579f),
				localScale = new Vector3(0.1f, 0.1f, 0.1f)
			}
		});
		itemDisplayRuleDict.Add("mdlBandit2", new RoR2.ItemDisplayRule[1]
		{
			new RoR2.ItemDisplayRule
			{
				ruleType = ItemDisplayRuleType.ParentedPrefab,
				followerPrefab = EquipmentDisplayModel,
				childName = "LowerArmL",
				localPos = new Vector3(-0.03f, 0.10113f, -0.04034f),
				localAngles = new Vector3(359.176f, 38.95728f, 85.88149f),
				localScale = new Vector3(0.1f, 0.1f, 0.1f)
			}
		});
		itemDisplayRuleDict.Add("mdlScav", new RoR2.ItemDisplayRule[1]
		{
			new RoR2.ItemDisplayRule
			{
				ruleType = ItemDisplayRuleType.ParentedPrefab,
				followerPrefab = EquipmentDisplayModel,
				childName = "LowerArmL",
				localPos = new Vector3(-0.25123f, 1.24f, 1.28747f),
				localAngles = new Vector3(355.3492f, 165.8023f, 90.29515f),
				localScale = new Vector3(1.75f, 1.75f, 1.75f)
			}
		});
		itemDisplayRuleDict.Add("mdlEngiTurret", new RoR2.ItemDisplayRule[1]
		{
			new RoR2.ItemDisplayRule
			{
				ruleType = ItemDisplayRuleType.ParentedPrefab,
				followerPrefab = EquipmentDisplayModel,
				childName = "Head",
				localPos = new Vector3(-1.02194f, 0.77255f, 0.2958f),
				localAngles = new Vector3(38.61912f, 112.9375f, 175.9272f),
				localScale = new Vector3(0.25f, 0.2f, 0.25f)
			}
		});
		itemDisplayRuleDict.Add("mdlRailGunner", new RoR2.ItemDisplayRule[1]
		{
			new RoR2.ItemDisplayRule
			{
				ruleType = ItemDisplayRuleType.ParentedPrefab,
				followerPrefab = EquipmentDisplayModel,
				childName = "LowerArmL",
				localPos = new Vector3(-0.00869f, 0.14618f, -0.06005f),
				localAngles = new Vector3(353.7924f, 348.4818f, 96.72074f),
				localScale = new Vector3(0.1f, 0.1f, 0.1f)
			}
		});
		itemDisplayRuleDict.Add("mdlVoidSurvivor", new RoR2.ItemDisplayRule[1]
		{
			new RoR2.ItemDisplayRule
			{
				ruleType = ItemDisplayRuleType.ParentedPrefab,
				followerPrefab = EquipmentDisplayModel,
				childName = "ForeArmL",
				localPos = new Vector3(0.13608f, 0.1567f, 0.01445f),
				localAngles = new Vector3(11.4892f, 270f, 90f),
				localScale = new Vector3(0.1f, 0.1f, 0.1f)
			}
		});
	}
}
