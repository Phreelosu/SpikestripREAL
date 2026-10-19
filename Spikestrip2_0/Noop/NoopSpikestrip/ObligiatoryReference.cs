using Mono.Cecil.Cil;
using MonoMod.Cil;
using System;
using GrooveSaladSpikestripContent;
using On.RoR2;
using On.RoR2.UI;
using R2API;
using R2API.Networking;
using R2API.Networking.Interfaces;
using RoR2;
using RoR2.UI;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using Spikestrip2_0.Core;

namespace NoopSpikestrip;

internal class ObligiatoryReference : ItemBase<ObligiatoryReference>
{
	public class ReferenceBehavior : RoR2.CharacterBody.ItemBehavior
	{
		public class SyncDamageCount : INetMessage, ISerializableObject
		{
			private float NewDamageCount;

			private NetworkInstanceId GameObjectID;

			public SyncDamageCount()
			{
			}

			public SyncDamageCount(float newDamageCount, NetworkInstanceId gameObjectID)
			{
				NewDamageCount = newDamageCount;
				GameObjectID = gameObjectID;
			}

			public void Serialize(NetworkWriter writer)
			{
				writer.Write(NewDamageCount);
				writer.Write(GameObjectID);
			}

			public void Deserialize(NetworkReader reader)
			{
				NewDamageCount = reader.ReadSingle();
				GameObjectID = reader.ReadNetworkId();
			}

			public void OnReceived()
			{
				if (NetworkServer.active)
				{
					return;
				}
				GameObject gameObject = RoR2.Util.FindNetworkObject(GameObjectID);
				if ((bool)gameObject)
				{
					ReferenceBehavior component = gameObject.GetComponent<ReferenceBehavior>();
					if ((bool)component)
					{
						component.damageCount = NewDamageCount;
					}
				}
			}
		}

		public static float rechargeFraction = 0.1f;

		public float damageCount;

		private Image image;

		public float damageThreshold => 75f * Mathf.Pow(0.85f, stack - 1);

		public float NetworkDamageCount
		{
			get
			{
				return damageCount;
			}
			set
			{
				damageCount = value;
				NetworkIdentity component = GetComponent<NetworkIdentity>();
				if ((bool)component)
				{
					NetMessageExtensions.Send((INetMessage)(object)new SyncDamageCount(value, component.netId), (NetworkDestination)1);
				}
			}
		}

		public void AddNetworkDamage(float damage)
		{
			if (NetworkServer.active && !(body.healthComponent.shield >= body.healthComponent.fullShield))
			{
				float num = damageCount + damage;
				if (num >= damageThreshold)
				{
					num -= damageThreshold;
					body.healthComponent.RechargeShield(body.healthComponent.fullCombinedHealth * rechargeFraction);
				}
				NetworkDamageCount = num;
			}
		}

		public void UpdateBar(RoR2.UI.HealthBar healthbar)
		{
			if (!healthbar.barAllocator.containerTransform.gameObject.scene.IsValid())
			{
				return;
			}
			if (damageCount <= 0f)
			{
				if ((bool)image)
				{
					if (Application.isPlaying)
					{
						UnityEngine.Object.Destroy(image.gameObject);
					}
					else
					{
						UnityEngine.Object.DestroyImmediate(image.gameObject);
					}
					image = null;
				}
			}
			else if (!image)
			{
				image = UnityEngine.Object.Instantiate(healthbar.barAllocator.elementPrefab, healthbar.barAllocator.containerTransform).GetComponent<Image>();
				GameObject gameObject = image.gameObject;
				gameObject.SetActive(value: true);
				healthbar.barAllocator.AllocateElements(0);
			}
		}

		public void ApplyBar(RoR2.UI.HealthBar healthBar)
		{
			if ((bool)image && (bool)healthBar.source)
			{
				image.type = healthBar.style.shieldBarStyle.imageType;
				image.sprite = healthBar.style.shieldBarStyle.sprite;
				RoR2.HealthComponent.HealthBarValues healthBarValues = healthBar.source.GetHealthBarValues();
				image.color = (healthBarValues.hasVoidShields ? SpikestripUtil.ColorRGB(127f, 88f, 140f) : SpikestripUtil.ColorRGB(88f, 113f, 140f));
				float num = healthBarValues.healthFraction + healthBarValues.shieldFraction;
				float num2 = damageCount / damageThreshold;
				float num3 = (1f - healthBarValues.curseFraction) / healthBar.source.fullCombinedHealth * healthBar.source.fullCombinedHealth;
				float xMax = Mathf.Min(num + num3 * rechargeFraction * num2, 1f - healthBarValues.curseFraction);
				SetRectPosition((RectTransform)image.transform, num, xMax, 1f);
			}
		}

		public static void SetRectPosition(RectTransform rectTransform, float xMin, float xMax, float sizeDelta)
		{
			rectTransform.anchorMin = new Vector2(xMin, 0f);
			rectTransform.anchorMax = new Vector2(xMax, 1f);
			rectTransform.anchoredPosition = Vector2.zero;
			rectTransform.sizeDelta = new Vector2(sizeDelta * 0.5f + 1f, sizeDelta + 1f);
		}

		public void OnDestroy()
		{
			if ((bool)image)
			{
				if (Application.isPlaying)
				{
					UnityEngine.Object.Destroy(image.gameObject);
				}
				else
				{
					UnityEngine.Object.DestroyImmediate(image.gameObject);
				}
			}
		}
	}

	public static float Shield = 0.4f;

	public override string ItemName => "Imperfect Construct";

	public override string ItemToken => "SHIELDREMOVAL";

	public override string ItemPickup => "Recharge shield by attacking enemies... <color=#FF7F7F>But replace a portion of your health with shield that cannot recharge naturally.</style>";

	public override string ItemDescription => "Gain a <style=cIsHealing>Shield</style> that replaces <style=cIsHealing>40%</style> <style=cStack>(-10% per stack)</style> of your <style=cIsHealing>Max Health</style>. <style=cIsHealing>Recharge 10% of your shield</style> after dealing a total of <style=cIsDamage>7500%</style> <style=cStack>(-15% per stack)</style> of base damage. <style=cIsUtility>Natural shield regeneration is disabled.</style> ";

	public override string ItemLore => "<style=cMono>\n========================================\n====   MyBabel Machine Translator   ====\n====     [Version 12.45.1.009 ]   ======\n========================================\nTraining… <100000000 cycles>\nTraining… <100000000 cycles>\nTraining… <100000000 cycles>\nUnknown Language Detected\nDisplay partial result? Y/N\nY\n================================</style> \nHollow. \n\n< Disease > spreads. \n\nIt's consuming me. \n\nMy Insides. \n\nHollow. \n\nI fear that I do not have enough time left. \n\nI need to be brave. \n\nI need to be a hero. \n\nThey said we (Drifters) would be heroes. \n\nYet... \n\n... \n\n... \n\n<style=cWorldEvent>The dog</style> leads me. \n\nShould I follow?  \n\n <style=cMono> END OF TRANSLATION </style>";

	public override ItemTag[] ItemTags => new ItemTag[2]
	{
		ItemTag.Utility,
		ItemTag.Damage
	};

	public override ItemTier Tier => ItemTier.Lunar;

	public override Type ItemBehaviour => typeof(ReferenceBehavior);

	public override bool ServerOnlyBehaviour => false;

	public override Sprite ItemIconSprite => SpikestripContent.NoopBundle.LoadAsset<Sprite>("ConstructIcon.png");

	public unsafe override void Init()
	{
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Expected O, but got Unknown
		base.Init();
		NetworkingAPI.RegisterMessageType<ReferenceBehavior.SyncDamageCount>();
		On.RoR2.GlobalEventManager.OnHitEnemy += GlobalEventManager_OnHitEnemy;
		IL.RoR2.CharacterBody.RecalculateStats += CharacterBody_RecalculateStats;
		On.RoR2.HealthComponent.ServerFixedUpdate += HealthComponent_ServerFixedUpdate;
		On.RoR2.UI.HealthBar.UpdateHealthbar += HealthBar_UpdateHealthbar;
	}

	private void HealthComponent_ServerFixedUpdate(On.RoR2.HealthComponent.orig_ServerFixedUpdate orig, RoR2.HealthComponent self, float deltaTime)
	{
		float? num = null;
		RoR2.Inventory inventory = self.body.inventory;
		if ((bool)inventory && inventory.GetItemCountEffective(ItemDef) > 0)
		{
			num = self.shield;
			self.shield = self.body.maxShield;
		}
		orig(self, deltaTime);
		if (num.HasValue)
		{
			self.shield = num.Value;
		}
	}

	private void GlobalEventManager_OnHitEnemy(On.RoR2.GlobalEventManager.orig_OnHitEnemy orig, RoR2.GlobalEventManager self, RoR2.DamageInfo damageInfo, GameObject victim)
	{
		orig(self, damageInfo, victim);
		if (!(damageInfo.procCoefficient <= 0f) && !damageInfo.rejected && NetworkServer.active && (bool)damageInfo.attacker)
		{
			ReferenceBehavior component = damageInfo.attacker.GetComponent<ReferenceBehavior>();
			RoR2.CharacterBody component2 = damageInfo.attacker.GetComponent<RoR2.CharacterBody>();
			if ((bool)component && (bool)component2)
			{
				component.AddNetworkDamage(damageInfo.damage / component2.damage);
			}
		}
	}

	private void CharacterBody_RecalculateStats(ILContext il)
	{
		ILCursor c = new ILCursor(il);
		if (c.TryGotoNext(MoveType.After, x => x.MatchLdarg(0), x => x.MatchLdsfld(typeof(RoR2.RoR2Content.Buffs).GetField("AffixBlue")), x => x.MatchCall<RoR2.CharacterBody>("HasBuff"))
			&& c.TryGotoNext(MoveType.After, x => x.MatchCall<RoR2.CharacterBody>("set_maxShield")))
		{
			c.Emit(OpCodes.Ldarg_0);
			c.EmitDelegate<Action<RoR2.CharacterBody>>(body =>
			{
				int count = body.inventory ? body.inventory.GetItemCountEffective(ItemDef) : 0;
				if (count > 0)
				{
					float converted = body.maxHealth * 0.4f * Mathf.Pow(0.9f, count - 1);
					body.maxHealth -= converted;
					body.maxShield += converted;
				}
			});
		}
		else
		{
			Spikestrip2_0.Log.Error("Imperfect Construct: RecalculateStats IL hook failed");
		}
	}

	private void HealthBar_UpdateHealthbar(On.RoR2.UI.HealthBar.orig_UpdateHealthbar orig, RoR2.UI.HealthBar self, float deltaTime)
	{
		ReferenceBehavior referenceBehavior = null;
		if ((bool)self.source)
		{
			referenceBehavior = self.source.GetComponent<ReferenceBehavior>();
			if ((bool)referenceBehavior)
			{
				referenceBehavior.UpdateBar(self);
			}
		}
		orig(self, deltaTime);
		if ((bool)referenceBehavior)
		{
			referenceBehavior.ApplyBar(self);
		}
	}

	public override void AssignItemModels()
	{
		GameObject originalModel = SpikestripContent.NoopBundle.LoadAsset<GameObject>("construct.prefab");
		ItemPickupModel = SpikestripUtil.UpdateGameObjectShader(originalModel);
		ItemPickupModel.transform.localScale = new Vector3(0.6f, 0.6f, 0.6f);
		ItemDisplayModel = SpikestripUtil.EmptyPrefab("DisplayDrone");
		RoR2.ItemDisplay itemDisplay = ItemDisplayModel.AddComponent<RoR2.ItemDisplay>();
		itemDisplay.rendererInfos = Array.Empty<RoR2.CharacterModel.RendererInfo>();
		RoR2.ItemFollower itemFollower = ItemDisplayModel.AddComponent<RoR2.ItemFollower>();
		itemFollower.targetObject = ItemDisplayModel;
		itemFollower.followerPrefab = ItemPickupModel;
		itemFollower.distanceDampTime = 0.3f;
		itemFollower.distanceMaxSpeed = 200f;
	}

	public override void PopulateItemDisplayRules(ItemDisplayRuleDict itemDisplayRuleDict)
	{
		itemDisplayRuleDict.Add("mdlCommandoDualies", new RoR2.ItemDisplayRule[1]
		{
			new RoR2.ItemDisplayRule
			{
				ruleType = ItemDisplayRuleType.ParentedPrefab,
				followerPrefab = ItemDisplayModel,
				childName = "Base",
				localPos = new Vector3(0.04961f, -0.38856f, -1.25984f),
				localAngles = new Vector3(270f, 0f, 0f),
				localScale = new Vector3(0.1f, 0.1f, 0.1f)
			}
		});
		itemDisplayRuleDict.Add("mdlHuntress", new RoR2.ItemDisplayRule[1]
		{
			new RoR2.ItemDisplayRule
			{
				ruleType = ItemDisplayRuleType.ParentedPrefab,
				followerPrefab = ItemDisplayModel,
				childName = "Base",
				localPos = new Vector3(0.04961f, -0.38856f, -1.25984f),
				localAngles = new Vector3(270f, 0f, 0f),
				localScale = new Vector3(0.1f, 0.1f, 0.1f)
			}
		});
		itemDisplayRuleDict.Add("mdlToolbot", new RoR2.ItemDisplayRule[1]
		{
			new RoR2.ItemDisplayRule
			{
				ruleType = ItemDisplayRuleType.ParentedPrefab,
				followerPrefab = ItemDisplayModel,
				childName = "Base",
				localPos = new Vector3(0.32543f, -1.17691f, 10.97546f),
				localAngles = new Vector3(280.542f, 269.3459f, 270.4126f),
				localScale = new Vector3(0.1f, 0.1f, 0.1f)
			}
		});
		itemDisplayRuleDict.Add("mdlEngi", new RoR2.ItemDisplayRule[1]
		{
			new RoR2.ItemDisplayRule
			{
				ruleType = ItemDisplayRuleType.ParentedPrefab,
				followerPrefab = ItemDisplayModel,
				childName = "Base",
				localPos = new Vector3(0.04961f, -0.38856f, -1.25984f),
				localAngles = new Vector3(270f, 0f, 0f),
				localScale = new Vector3(0.1f, 0.1f, 0.1f)
			}
		});
		itemDisplayRuleDict.Add("mdlMage", new RoR2.ItemDisplayRule[1]
		{
			new RoR2.ItemDisplayRule
			{
				ruleType = ItemDisplayRuleType.ParentedPrefab,
				followerPrefab = ItemDisplayModel,
				childName = "Base",
				localPos = new Vector3(0.04961f, -0.38856f, -1.25984f),
				localAngles = new Vector3(270f, 0f, 0f),
				localScale = new Vector3(0.1f, 0.1f, 0.1f)
			}
		});
		itemDisplayRuleDict.Add("mdlMerc", new RoR2.ItemDisplayRule[1]
		{
			new RoR2.ItemDisplayRule
			{
				ruleType = ItemDisplayRuleType.ParentedPrefab,
				followerPrefab = ItemDisplayModel,
				childName = "Base",
				localPos = new Vector3(0.04961f, -0.38856f, -1.25984f),
				localAngles = new Vector3(270f, 0f, 0f),
				localScale = new Vector3(0.1f, 0.1f, 0.1f)
			}
		});
		itemDisplayRuleDict.Add("mdlTreebot", new RoR2.ItemDisplayRule[1]
		{
			new RoR2.ItemDisplayRule
			{
				ruleType = ItemDisplayRuleType.ParentedPrefab,
				followerPrefab = ItemDisplayModel,
				childName = "Base",
				localPos = new Vector3(0.08086f, -1.0055f, -3.60588f),
				localAngles = new Vector3(270f, 0f, 0f),
				localScale = new Vector3(0.15f, 0.15f, 0.15f)
			}
		});
		itemDisplayRuleDict.Add("mdlLoader", new RoR2.ItemDisplayRule[1]
		{
			new RoR2.ItemDisplayRule
			{
				ruleType = ItemDisplayRuleType.ParentedPrefab,
				followerPrefab = ItemDisplayModel,
				childName = "Base",
				localPos = new Vector3(0.04961f, -0.38856f, -1.25984f),
				localAngles = new Vector3(270f, 0f, 0f),
				localScale = new Vector3(0.1f, 0.1f, 0.1f)
			}
		});
		itemDisplayRuleDict.Add("mdlCroco", new RoR2.ItemDisplayRule[1]
		{
			new RoR2.ItemDisplayRule
			{
				ruleType = ItemDisplayRuleType.ParentedPrefab,
				followerPrefab = ItemDisplayModel,
				childName = "Base",
				localPos = new Vector3(0.31207f, -0.3915f, 12.23377f),
				localAngles = new Vector3(88.46455f, 204.9254f, 204.9175f),
				localScale = new Vector3(0.1f, 0.1f, 0.1f)
			}
		});
		itemDisplayRuleDict.Add("mdlCaptain", new RoR2.ItemDisplayRule[1]
		{
			new RoR2.ItemDisplayRule
			{
				ruleType = ItemDisplayRuleType.ParentedPrefab,
				followerPrefab = ItemDisplayModel,
				childName = "Base",
				localPos = new Vector3(0.04961f, -0.48966f, -1.39771f),
				localAngles = new Vector3(270f, 0f, 0f),
				localScale = new Vector3(0.1f, 0.1f, 0.1f)
			}
		});
		itemDisplayRuleDict.Add("mdlBandit2", new RoR2.ItemDisplayRule[1]
		{
			new RoR2.ItemDisplayRule
			{
				ruleType = ItemDisplayRuleType.ParentedPrefab,
				followerPrefab = ItemDisplayModel,
				childName = "Base",
				localPos = new Vector3(0.40291f, 0.10195f, -1.26003f),
				localAngles = new Vector3(343.5631f, 269.9999f, 90.00006f),
				localScale = new Vector3(0.1f, 0.1f, 0.1f)
			}
		});
		itemDisplayRuleDict.Add("mdlRailGunner", new RoR2.ItemDisplayRule[1]
		{
			new RoR2.ItemDisplayRule
			{
				ruleType = ItemDisplayRuleType.ParentedPrefab,
				followerPrefab = ItemDisplayModel,
				childName = "Base",
				localPos = new Vector3(0.04961f, -0.38856f, -1.25984f),
				localAngles = new Vector3(270f, 0f, 0f),
				localScale = new Vector3(0.1f, 0.1f, 0.1f)
			}
		});
		itemDisplayRuleDict.Add("mdlVoidSurvivor", new RoR2.ItemDisplayRule[1]
		{
			new RoR2.ItemDisplayRule
			{
				ruleType = ItemDisplayRuleType.ParentedPrefab,
				followerPrefab = ItemDisplayModel,
				childName = "Base",
				localPos = new Vector3(-0.13028f, 0.7203f, 1.01341f),
				localAngles = new Vector3(60.33562f, 57.1818f, 50.21627f),
				localScale = new Vector3(0.1f, 0.1f, 0.1f)
			}
		});
		itemDisplayRuleDict.Add("mdlScav", new RoR2.ItemDisplayRule[1]
		{
			new RoR2.ItemDisplayRule
			{
				ruleType = ItemDisplayRuleType.ParentedPrefab,
				followerPrefab = ItemDisplayModel,
				childName = "Base",
				localPos = new Vector3(0.05534f, -6.03361f, 14.71212f),
				localAngles = new Vector3(87.22279f, 232.1215f, 230.3829f),
				localScale = new Vector3(0.3f, 0.3f, 0.3f)
			}
		});
		itemDisplayRuleDict.Add("mdlEngiTurret", new RoR2.ItemDisplayRule[1]
		{
			new RoR2.ItemDisplayRule
			{
				childName = "Base",
				localPos = new Vector3(1.60967f, 4.90143f, 1.03799f),
				localAngles = new Vector3(359.6393f, 12.06022f, 359.9229f),
				localScale = new Vector3(0.1f, 0.1f, 0.1f)
			}
		});
	}
}
