using System;
using On.RoR2;
using R2API;
using R2API.Networking;
using R2API.Networking.Interfaces;
using RoR2;
using RoR2.UI;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;
using Spikestrip2_0.Core;

namespace GrooveSaladSpikestripContent.Content;

public class Calendar : ItemBase<Calendar>
{
	public class SyncedCalendarBehaviour : EnhancedItemBehaviour
	{
		public DayOfWeek networkedCurrentDay;

		public override void OnStackChanged(bool increase)
		{
			if (NetworkServer.active && increase)
			{
				SetDayOfWeek(DateTime.Now.DayOfWeek);
			}
		}

		private void FixedUpdate()
		{
			if (NetworkServer.active)
			{
				DayOfWeek dayOfWeek = DateTime.Now.DayOfWeek;
				if (networkedCurrentDay != dayOfWeek)
				{
					SetDayOfWeek(dayOfWeek);
				}
			}
		}

		private void SetDayOfWeek(DayOfWeek dayOfWeek)
		{
			if (NetworkServer.active)
			{
				networkedCurrentDay = dayOfWeek;
				NetworkIdentity component = GetComponent<NetworkIdentity>();
				if ((bool)component)
				{
					NetMessageExtensions.Send((INetMessage)(object)new SyncDayOfWeek(dayOfWeek, component.netId), (NetworkDestination)1);
				}
				body.statsDirty = true;
				if (body.localStartTime.timeSince > 1f)
				{
					RoR2.EffectManager.SpawnEffect(effectPrefab, new RoR2.EffectData
					{
						origin = body.corePosition,
						scale = 1f,
						genericUInt = RoR2.Util.IntToUintPlusOne((int)dayOfWeek)
					}, transmit: true);
				}
			}
		}
	}

	public class SyncDayOfWeek : INetMessage, ISerializableObject
	{
		private DayOfWeek NewDayOfWeek;

		private NetworkInstanceId GameObjectID;

		public SyncDayOfWeek()
		{
		}

		public SyncDayOfWeek(DayOfWeek newDayOfWeek, NetworkInstanceId gameObjectID)
		{
			NewDayOfWeek = newDayOfWeek;
			GameObjectID = gameObjectID;
		}

		public void Serialize(NetworkWriter writer)
		{
			writer.Write((int)NewDayOfWeek);
			writer.Write(GameObjectID);
		}

		public void Deserialize(NetworkReader reader)
		{
			NewDayOfWeek = (DayOfWeek)reader.ReadInt32();
			GameObjectID = reader.ReadNetworkId();
		}

		public void OnReceived()
		{
			if (NetworkServer.active)
			{
				return;
			}
			GameObject gameObject = RoR2.Util.FindNetworkObject(GameObjectID);
			if (!gameObject)
			{
				return;
			}
			SyncedCalendarBehaviour component = gameObject.GetComponent<SyncedCalendarBehaviour>();
			if ((bool)component)
			{
				component.networkedCurrentDay = NewDayOfWeek;
				if ((bool)component.body)
				{
					component.body.statsDirty = true;
				}
			}
		}
	}

	public class CalendarEffectController : MonoBehaviour
	{
		private void Start()
		{
			string text = "ITEM_CALENDAR_BONUS_" + ((DayOfWeek)RoR2.Util.UintToIntMinusOne(GetComponent<RoR2.EffectComponent>().effectData.genericUInt)/*cast due to .constrained prefix*/).ToString().ToUpper();
			TextMeshPro componentInChildren = GetComponentInChildren<TextMeshPro>();
			componentInChildren.text = text;
			LanguageTextMeshController componentInChildren2 = GetComponentInChildren<LanguageTextMeshController>();
			componentInChildren2.token = text;
		}
	}

	public class ClockHandController : MonoBehaviour
	{
		public enum TimeType
		{
			Seconds,
			Minutes,
			Hours
		}

		public TimeType timeType;

		public AnimationCurve secondsCurve = AnimationCurve.EaseInOut(0f, 0f, 0.2f, 1f);

		private void Update()
		{
			float num = 0f;
			switch (timeType)
			{
			case TimeType.Seconds:
			{
				double num2 = DateTime.Now.TimeOfDay.TotalSeconds % 60.0;
				double num3 = num2 % 1.0;
				float num4 = secondsCurve.Evaluate((float)num3);
				num2 += (double)num4;
				num2 -= num3;
				num = (float)(num2 / 60.0);
				break;
			}
			case TimeType.Minutes:
				num = (float)(DateTime.Now.TimeOfDay.TotalMinutes % 60.0 / 60.0);
				break;
			case TimeType.Hours:
				num = (float)(DateTime.Now.TimeOfDay.TotalHours % 12.0 / 12.0);
				break;
			}
			Transform transform = base.transform;
			Vector3 localEulerAngles = transform.localEulerAngles;
			localEulerAngles.z = 360f * num;
			transform.localEulerAngles = localEulerAngles;
		}
	}

	public static float bonusPower = 0.13f;

	public static GameObject effectPrefab;

	public override string ItemToken => "CALENDAR";

	public override string ItemName => "Groove's Alarm";

	public override string ItemPickup => "Gain a bonus according to the day of the week.";

	public override string ItemDescription => "Gain a <style=cIsUtility>13%</style> <style=cStack>(+13% per stack)</style> bonus according to the <style=cIsUtility>day of the week</style>. \n\n<style=cIsUtility>Jump power</style> on Monday.\n<style=cIsDamage>Inflicted DoT duration</style> on Tuesday.\n<style=cIsUtility>Movement speed</style> on Wednesday.\n<style=cIsHealing>Armor</style> on Thursday.\n<style=cIsDamage>Damage</style> on Friday.\n<style=cIsHealing>Regeneration</style> on Saturday.\n<style=cIsDamage>Attack speed</style> on Sunday.";

	public override string ItemLore => "<style=cMono>BEEP-BEEP-BEEP! BEEP-BEEP-BE-</style>(Click)\n\nUuuuueeeeeeaaaaaaaaaaagh.\n\nWhat day is it?\n\nTuesday?\n\nMaybe I'll sleep a while longer..\n\n<style=cMono>[End of transcription]</style>";

	public override ItemTier Tier => ItemTier.Tier1;

	public override ItemTag[] ItemTags => new ItemTag[3]
	{
		ItemTag.Damage,
		ItemTag.Healing,
		ItemTag.Utility
	};

	public override Sprite ItemIconSprite => SpikestripContent.GrooveBundle.LoadAsset<Sprite>("texAlarmIcon.png");

	public override Type ItemBehaviour => typeof(SyncedCalendarBehaviour);

	public override void Init()
	{
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Expected O, but got Unknown
		base.Init();
		NetworkingAPI.RegisterMessageType<SyncDayOfWeek>();
		RecalculateStatsAPI.GetStatCoefficients += new RecalculateStatsAPI.StatHookEventHandler(RecalculateStatsAPI_GetStatCoefficients);
		On.RoR2.CharacterBody.RecalculateStats += CharacterBody_RecalculateStats;
		On.RoR2.DotController.InflictDot_refInflictDotInfo += DotController_InflictDot_refInflictDotInfo;
		effectPrefab = PrefabAPI.InstantiateClone(LegacyResourcesAPI.Load<GameObject>("Prefabs/Effects/BearProc"), "CalendarEffect", false);
		UnityEngine.Object.Destroy(effectPrefab.transform.Find("Fluff").gameObject);
		effectPrefab.AddComponent<CalendarEffectController>();
		effectPrefab.GetComponent<RoR2.EffectComponent>().soundName = "";
		effectPrefab.GetComponent<RoR2.VFXAttributes>().vfxPriority = RoR2.VFXAttributes.VFXPriority.Always;
		TextMeshPro componentInChildren = effectPrefab.GetComponentInChildren<TextMeshPro>();
		componentInChildren.color = Color.white;
		ContentAddition.AddEffect(effectPrefab);
		LanguageAPI.Add("ITEM_CALENDAR_BONUS_MONDAY", "<style=cIsUtility>+Jump Power</style>");
		LanguageAPI.Add("ITEM_CALENDAR_BONUS_TUESDAY", "<style=cIsDamage>+Inflicted DoT Duration</style>");
		LanguageAPI.Add("ITEM_CALENDAR_BONUS_WEDNESDAY", "<style=cIsUtility>+Movement Speed</style>");
		LanguageAPI.Add("ITEM_CALENDAR_BONUS_THURSDAY", "<style=cIsHealing>+Armor</style>");
		LanguageAPI.Add("ITEM_CALENDAR_BONUS_FRIDAY", "<style=cIsDamage>+Damage</style>");
		LanguageAPI.Add("ITEM_CALENDAR_BONUS_SATURDAY", "<style=cIsHealing>+Regeneration</style>");
		LanguageAPI.Add("ITEM_CALENDAR_BONUS_SUNDAY", "<style=cIsDamage>+Attack Speed</style>");
	}

	private void DotController_InflictDot_refInflictDotInfo(On.RoR2.DotController.orig_InflictDot_refInflictDotInfo orig, ref InflictDotInfo inflictDotInfo)
	{
		GameObject attackerObject = inflictDotInfo.attackerObject;
		if ((bool)attackerObject)
		{
			RoR2.CharacterBody component = attackerObject.GetComponent<RoR2.CharacterBody>();
			if ((bool)component && (bool)component.inventory)
			{
				int itemCount = component.inventory.GetItemCountEffective(ItemDef);
				if (itemCount != 0)
				{
					SyncedCalendarBehaviour component2 = attackerObject.GetComponent<SyncedCalendarBehaviour>();
					if ((bool)component2 && component2.networkedCurrentDay == DayOfWeek.Tuesday)
					{
						float num = 1f + bonusPower * (float)itemCount;
						inflictDotInfo.duration *= num;
					}
				}
			}
		}
		orig(ref inflictDotInfo);
	}

	private void CharacterBody_RecalculateStats(On.RoR2.CharacterBody.orig_RecalculateStats orig, RoR2.CharacterBody self)
	{
		orig(self);
		if (!self.inventory)
		{
			return;
		}
		int itemCount = self.inventory.GetItemCountEffective(ItemDef);
		if (itemCount != 0)
		{
			SyncedCalendarBehaviour component = self.GetComponent<SyncedCalendarBehaviour>();
			if ((bool)component && component.networkedCurrentDay == DayOfWeek.Thursday)
			{
				float num = 1f + bonusPower * (float)itemCount;
				self.armor *= num;
			}
		}
	}

	private void RecalculateStatsAPI_GetStatCoefficients(RoR2.CharacterBody sender, RecalculateStatsAPI.StatHookEventArgs args)
	{
		if (!sender || !sender.inventory)
		{
			return;
		}
		int itemCount = sender.inventory.GetItemCountEffective(ItemDef);
		if (itemCount == 0)
		{
			return;
		}
		SyncedCalendarBehaviour component = sender.GetComponent<SyncedCalendarBehaviour>();
		if ((bool)component)
		{
			float num = bonusPower * (float)itemCount;
			switch (component.networkedCurrentDay)
			{
			case DayOfWeek.Monday:
				args.jumpPowerMultAdd += num;
				break;
			case DayOfWeek.Wednesday:
				args.moveSpeedMultAdd += num;
				break;
			case DayOfWeek.Friday:
				args.damageMultAdd += num;
				break;
			case DayOfWeek.Saturday:
				args.regenMultAdd += num;
				break;
			case DayOfWeek.Sunday:
				args.attackSpeedMultAdd += num;
				break;
			case DayOfWeek.Tuesday:
			case DayOfWeek.Thursday:
				break;
			}
		}
	}

	public override void AssignItemModels()
	{
		ItemPickupModel = SpikestripUtil.UpdateGameObjectShader(SpikestripContent.GrooveBundle.LoadAsset<GameObject>("AlarmPickup.prefab"));
		MeshRenderer component = ItemPickupModel.transform.Find("Glass").gameObject.GetComponent<MeshRenderer>();
		component.SetSharedMaterials(new Material[2]
		{
			SpikestripUtil.wispGlassDistortionMaterial,
			SpikestripUtil.infusionGlassMaterial
		}, 2);
		ClockHandController clockHandController = ItemPickupModel.transform.Find("Seconds").gameObject.AddComponent<ClockHandController>();
		clockHandController.timeType = ClockHandController.TimeType.Seconds;
		ClockHandController clockHandController2 = ItemPickupModel.transform.Find("Minutes").gameObject.AddComponent<ClockHandController>();
		clockHandController2.timeType = ClockHandController.TimeType.Minutes;
		ClockHandController clockHandController3 = ItemPickupModel.transform.Find("Hours").gameObject.AddComponent<ClockHandController>();
		clockHandController3.timeType = ClockHandController.TimeType.Hours;
		ItemDisplayModel = ItemPickupModel;
		RoR2.ItemDisplay itemDisplay = ItemDisplayModel.AddComponent<RoR2.ItemDisplay>();
		itemDisplay.rendererInfos = SpikestripUtil.GenerateRendererInfos(ItemDisplayModel);
	}

	public override void PopulateItemDisplayRules(ItemDisplayRuleDict itemDisplayRuleDict)
	{
		itemDisplayRuleDict.Add("mdlCommandoDualies", new RoR2.ItemDisplayRule[1]
		{
			new RoR2.ItemDisplayRule
			{
				ruleType = ItemDisplayRuleType.ParentedPrefab,
				followerPrefab = ItemDisplayModel,
				childName = "GunR",
				localPos = new Vector3(-0.19374f, 0.00299f, -0.00102f),
				localAngles = new Vector3(1.11829f, 90.64056f, 181.4436f),
				localScale = new Vector3(0.02033f, 0.02033f, 0.02033f)
			}
		});
		itemDisplayRuleDict.Add("mdlHuntress", new RoR2.ItemDisplayRule[1]
		{
			new RoR2.ItemDisplayRule
			{
				ruleType = ItemDisplayRuleType.ParentedPrefab,
				followerPrefab = ItemDisplayModel,
				childName = "Arrow",
				localPos = new Vector3(0.21127f, 0.00309f, -0.03743f),
				localAngles = new Vector3(0.76068f, 269.3823f, 88.70631f),
				localScale = new Vector3(0.0143f, 0.0143f, 0.0143f)
			}
		});
		itemDisplayRuleDict.Add("mdlToolbot", new RoR2.ItemDisplayRule[1]
		{
			new RoR2.ItemDisplayRule
			{
				ruleType = ItemDisplayRuleType.ParentedPrefab,
				followerPrefab = ItemDisplayModel,
				childName = "Chest",
				localPos = new Vector3(1.90149f, 2.99568f, 1.31896f),
				localAngles = new Vector3(0f, 0f, 0f),
				localScale = new Vector3(0.27846f, 0.27846f, 0.27846f)
			}
		});
		itemDisplayRuleDict.Add("mdlEngi", new RoR2.ItemDisplayRule[1]
		{
			new RoR2.ItemDisplayRule
			{
				ruleType = ItemDisplayRuleType.ParentedPrefab,
				followerPrefab = ItemDisplayModel,
				childName = "CannonHeadR",
				localPos = new Vector3(0.08081f, 0.30545f, 0.25563f),
				localAngles = new Vector3(274.6127f, 264.9436f, 274.9784f),
				localScale = new Vector3(0.03275f, 0.03275f, 0.03275f)
			}
		});
		itemDisplayRuleDict.Add("mdlMage", new RoR2.ItemDisplayRule[1]
		{
			new RoR2.ItemDisplayRule
			{
				ruleType = ItemDisplayRuleType.ParentedPrefab,
				followerPrefab = ItemDisplayModel,
				childName = "Chest",
				localPos = new Vector3(0.11493f, 0.33967f, 0.01739f),
				localAngles = new Vector3(7.59129f, 2.13761f, 348.3012f),
				localScale = new Vector3(0.01641f, 0.01641f, 0.01641f)
			}
		});
		itemDisplayRuleDict.Add("mdlMerc", new RoR2.ItemDisplayRule[1]
		{
			new RoR2.ItemDisplayRule
			{
				ruleType = ItemDisplayRuleType.ParentedPrefab,
				followerPrefab = ItemDisplayModel,
				childName = "ClavicleR",
				localPos = new Vector3(0.00053f, 0.18806f, -0.14215f),
				localAngles = new Vector3(0.80853f, 274.6135f, 89.40514f),
				localScale = new Vector3(0.0253f, 0.0253f, 0.0253f)
			}
		});
		itemDisplayRuleDict.Add("mdlTreebot", new RoR2.ItemDisplayRule[1]
		{
			new RoR2.ItemDisplayRule
			{
				ruleType = ItemDisplayRuleType.ParentedPrefab,
				followerPrefab = ItemDisplayModel,
				childName = "PlatformBase",
				localPos = new Vector3(0.70652f, 1.56366f, -0.52051f),
				localAngles = new Vector3(-1E-05f, 131.5801f, 2E-05f),
				localScale = new Vector3(0.07656f, 0.07656f, 0.07656f)
			}
		});
		itemDisplayRuleDict.Add("mdlLoader", new RoR2.ItemDisplayRule[1]
		{
			new RoR2.ItemDisplayRule
			{
				ruleType = ItemDisplayRuleType.ParentedPrefab,
				followerPrefab = ItemDisplayModel,
				childName = "MechUpperArmR",
				localPos = new Vector3(-0.00438f, -0.05017f, -0.07129f),
				localAngles = new Vector3(37.08526f, 0.00695f, 177.9394f),
				localScale = new Vector3(0.02315f, 0.02315f, 0.02315f)
			}
		});
		itemDisplayRuleDict.Add("mdlCroco", new RoR2.ItemDisplayRule[1]
		{
			new RoR2.ItemDisplayRule
			{
				ruleType = ItemDisplayRuleType.ParentedPrefab,
				followerPrefab = ItemDisplayModel,
				childName = "SpineChest2",
				localPos = new Vector3(-2.37585f, (float)Math.PI * -205f / 344f, -1.24966f),
				localAngles = new Vector3(346.6363f, 315.0724f, 84.67845f),
				localScale = new Vector3(0.20077f, 0.20077f, 0.20077f)
			}
		});
		itemDisplayRuleDict.Add("mdlCaptain", new RoR2.ItemDisplayRule[1]
		{
			new RoR2.ItemDisplayRule
			{
				ruleType = ItemDisplayRuleType.ParentedPrefab,
				followerPrefab = ItemDisplayModel,
				childName = "ClavicleR",
				localPos = new Vector3(-0.03276f, 0.1388f, -0.24589f),
				localAngles = new Vector3(13.64877f, 258.9686f, 88.14836f),
				localScale = new Vector3(0.0271f, 0.0271f, 0.0271f)
			}
		});
		itemDisplayRuleDict.Add("mdlBandit2", new RoR2.ItemDisplayRule[1]
		{
			new RoR2.ItemDisplayRule
			{
				ruleType = ItemDisplayRuleType.ParentedPrefab,
				followerPrefab = ItemDisplayModel,
				childName = "Hat",
				localPos = new Vector3(0.14966f, 0.07662f, 0.06794f),
				localAngles = new Vector3(343.4655f, 353.6933f, 6.92408f),
				localScale = new Vector3(0.02319f, 0.02319f, 0.02319f)
			}
		});
		itemDisplayRuleDict.Add("mdlRailGunner", new RoR2.ItemDisplayRule[1]
		{
			new RoR2.ItemDisplayRule
			{
				ruleType = ItemDisplayRuleType.ParentedPrefab,
				followerPrefab = ItemDisplayModel,
				childName = "TopRail",
				localPos = new Vector3(-0.13891f, 0.30612f, -0.16246f),
				localAngles = new Vector3(90f, 270f, 0f),
				localScale = new Vector3(0.02319f, 0.02319f, 0.02319f)
			}
		});
		itemDisplayRuleDict.Add("mdlVoidSurvivor", new RoR2.ItemDisplayRule[1]
		{
			new RoR2.ItemDisplayRule
			{
				ruleType = ItemDisplayRuleType.ParentedPrefab,
				followerPrefab = ItemDisplayModel,
				childName = "ShoulderR",
				localPos = new Vector3(0.05028f, 0.51898f, 0.07522f),
				localAngles = new Vector3(348.9274f, 145.0942f, 49.15798f),
				localScale = new Vector3(0.02463f, 0.02463f, 0.02463f)
			}
		});
		itemDisplayRuleDict.Add("mdlScav", new RoR2.ItemDisplayRule[1]
		{
			new RoR2.ItemDisplayRule
			{
				ruleType = ItemDisplayRuleType.ParentedPrefab,
				followerPrefab = ItemDisplayModel,
				childName = "Weapon",
				localPos = new Vector3(-1.58815f, 9.54177f, 4.09468f),
				localAngles = new Vector3(79.82661f, 64.87062f, 84.7156f),
				localScale = new Vector3(0.43131f, 0.43131f, 0.43131f)
			}
		});
		itemDisplayRuleDict.Add("mdlEngiTurret", new RoR2.ItemDisplayRule[1]
		{
			new RoR2.ItemDisplayRule
			{
				ruleType = ItemDisplayRuleType.ParentedPrefab,
				followerPrefab = ItemDisplayModel,
				childName = "Head",
				localPos = new Vector3(0.68415f, 0.96303f, -0.14096f),
				localAngles = new Vector3(0.40546f, 358.4813f, 0.88653f),
				localScale = new Vector3(0.12785f, 0.12785f, 0.12785f)
			}
		});
	}
}
