using System;
using On.RoR2;
using On.RoR2.PostProcessing;
using On.RoR2.UI;
using R2API;
using RoR2;
using RoR2.PostProcessing;
using RoR2.UI;
using UnityEngine;
using Spikestrip2_0.Core;

namespace GrooveSaladSpikestripContent.Content;

public class PainBox : ItemBase<PainBox>
{
	public class PainBoxBehaviourServer : RoR2.CharacterBody.ItemBehavior
	{
		private void OnDisable()
		{
			RoR2.HealthComponent healthComponent = (body ? body.healthComponent : null);
			if ((bool)healthComponent)
			{
				healthComponent.Networkhealth = Mathf.Max(healthComponent.health, healthComponent.fullHealth);
				healthComponent.Networkshield = Mathf.Max(healthComponent.shield, healthComponent.fullShield);
			}
		}
	}

	public Sprite painBoxHealthbarSprite = SpikestripContent.GrooveBundle.LoadAsset<Sprite>("texPainBoxHealthbar.png");

	public override string ItemToken => "PAINBOX";

	public override string ItemName => "Box of Agony";

	public override string ItemPickup => "Reduce incoming damage... <color=#FF7F7F>BUT your health is hidden from view.</color>\n";

	public override string ItemDescription => "<style=cIsHealing>Increase armor</style> by <style=cIsHealing>40</style> <style=cStack>(+10 per stack)</style>. Your <style=cIsHealing>health</style> is <style=cIsUtility>hidden from view</style>.";

	public override string ItemLore => "I must not fear.\n\nFear is the mind-killer. Fear is the little-death that brings total obliteration.\n\nI will face my fear. I will permit it to pass over me and through me. And when it has gone past I will turn the inner eye to see its path. Where the fear has gone there will be nothing.\n\nOnly I will remain.";

	public override ItemTier Tier => ItemTier.Lunar;

	public override ItemTag[] ItemTags => new ItemTag[1] { ItemTag.Utility };

	public override Sprite ItemIconSprite => SpikestripContent.GrooveBundle.LoadAsset<Sprite>("texPainBoxIcon.png");

	public override Type ItemBehaviour => typeof(PainBoxBehaviourServer);

	public override bool ServerOnlyBehaviour => true;

	public override void Init()
	{
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Expected O, but got Unknown
		base.Init();
		// ScreenDamage moved from OnRenderImage to Update in newer game versions.
		On.RoR2.PostProcessing.ScreenDamage.Update += ScreenDamage_Update;
		On.RoR2.MusicController.RecalculateHealth += MusicController_RecalculateHealth;
		On.RoR2.HealthComponent.GetHealthBarValues += HealthComponent_GetHealthBarValues;
		On.RoR2.UI.HealthBar.ApplyBars += HealthBar_ApplyBars;
		On.RoR2.CharacterBody.RecalculateStats += CharacterBody_RecalculateStats;
	}

	private void CharacterBody_RecalculateStats(On.RoR2.CharacterBody.orig_RecalculateStats orig, RoR2.CharacterBody self)
	{
		orig(self);
		if ((bool)self.inventory)
		{
			int itemCount = self.inventory.GetItemCountEffective(ItemDef);
			if (itemCount > 0)
			{
				self.armor += SpikestripUtil.StackScaling(40f, 10f, itemCount);
			}
		}
	}

	private void ScreenDamage_Update(On.RoR2.PostProcessing.ScreenDamage.orig_Update orig, RoR2.PostProcessing.ScreenDamage self)
	{
		float health = 0f;
		RoR2.HealthComponent healthComponent = null;
		RoR2.CharacterBody characterBody = (self.cameraRigController ? self.cameraRigController.targetBody : null);
		if ((bool)characterBody && (bool)characterBody.inventory && characterBody.inventory.GetItemCountEffective(ItemDef) > 0)
		{
			healthComponent = characterBody.healthComponent;
			if ((bool)healthComponent)
			{
				health = healthComponent.health;
				healthComponent.health = healthComponent.fullHealth;
			}
		}
		orig(self);
		if (healthComponent != null)
		{
			healthComponent.health = health;
		}
	}

	private void MusicController_RecalculateHealth(On.RoR2.MusicController.orig_RecalculateHealth orig, RoR2.MusicController self, GameObject playerObject)
	{
		orig(self, playerObject);
		RoR2.CharacterBody characterBody = (self.target ? self.target.GetComponent<RoR2.CharacterBody>() : null);
		if ((bool)characterBody && (bool)characterBody.inventory && characterBody.inventory.GetItemCountEffective(ItemDef) > 0)
		{
			self.rtpcPlayerHealthValue.value = 100f;
		}
	}

	private RoR2.HealthComponent.HealthBarValues HealthComponent_GetHealthBarValues(On.RoR2.HealthComponent.orig_GetHealthBarValues orig, RoR2.HealthComponent self)
	{
		float health = self.health;
		float shield = self.shield;
		RoR2.Inventory inventory = (self.body ? self.body.inventory : null);
		if ((bool)inventory && inventory.GetItemCountEffective(ItemDef) > 0)
		{
			self.health = self.fullCombinedHealth;
			self.shield = 0f;
		}
		RoR2.HealthComponent.HealthBarValues result = orig(self);
		self.health = health;
		self.shield = shield;
		return result;
	}

	private void HealthBar_ApplyBars(On.RoR2.UI.HealthBar.orig_ApplyBars orig, RoR2.UI.HealthBar self)
	{
		RoR2.CharacterBody characterBody = (self.source ? self.source.body : null);
		if ((bool)characterBody && (bool)characterBody.inventory && characterBody.inventory.GetItemCountEffective(ItemDef) > 0)
		{
			if ((bool)self.currentHealthText)
			{
				self.currentHealthText.text = "???";
			}
			self.barInfoCollection.shieldBarInfo.enabled = false;
			ref RoR2.UI.HealthBar.BarInfo trailingOverHealthbarInfo = ref self.barInfoCollection.trailingOverHealthbarInfo;
			trailingOverHealthbarInfo.enabled = true;
			trailingOverHealthbarInfo.color = SpikestripUtil.ColorRGB(47f, 132f, 126f);
			trailingOverHealthbarInfo.sprite = painBoxHealthbarSprite;
			self.barInfoCollection.lowHealthOverBarInfo.sprite = painBoxHealthbarSprite;
		}
		orig(self);
	}

	public override void AssignItemModels()
	{
		ItemPickupModel = SpikestripContent.GrooveBundle.LoadAsset<GameObject>("PainBox.prefab");
		Material original = SpikestripUtil.AddressablesLoad<Material>("f79c2b0bd0a1fff4aa5bd546ba199545");
		Material material = UnityEngine.Object.Instantiate(original);
		material.color = SpikestripUtil.ColorRGB(31f, 57f, 47f);
		material.EnableKeyword("DITHER");
		Transform transform = ItemPickupModel.transform.Find("mdlPainBox");
		transform.GetComponent<MeshRenderer>().material = material;
		Transform transform2 = ItemPickupModel.transform.Find("shadow");
		transform2.SetParent(transform);
		transform.localRotation = RoR2.Util.QuaternionSafeLookRotation(Vector3.forward);
		Shader shader = SpikestripUtil.LegacyLoad<Shader>("Shaders/Deferred/HGWavyCloth");
		MeshRenderer component = transform2.GetComponent<MeshRenderer>();
		Material material2 = component.material;
		material2.shader = shader;
		material2.color = SpikestripUtil.ColorRGB(78f, 123f, 108f);
		material2.EnableKeyword("DITHER");
		GameObject gameObject = SpikestripUtil.AddressablesLoad<GameObject>("845aa38562b22b8489056e900ab1fe52");
		ItemDisplayModel = PrefabAPI.InstantiateClone(gameObject, "DisplayPainBox", false);
		UnityEngine.Object.DestroyImmediate(ItemDisplayModel.transform.Find("mdlDaggerDisplay/DaggerHeldMesh").gameObject);
		SkinnedMeshRenderer component2 = ItemDisplayModel.transform.Find("mdlDaggerDisplay/DaggerArmMesh").GetComponent<SkinnedMeshRenderer>();
		Material material3 = UnityEngine.Object.Instantiate(component2.material);
		material3.mainTexture = null;
		material3.SetColor("_TintColor", SpikestripUtil.ColorRGB(51f, 130f, 147f));
		component2.material = material3;
		GameObject gameObject2 = PrefabAPI.InstantiateClone(transform.gameObject, "mdlDisplayPainBox", false);
		UnityEngine.Object.DestroyImmediate(gameObject2.transform.Find("shadow").gameObject);
		gameObject2.transform.SetParent(ItemDisplayModel.transform.Find("mdlDaggerDisplay/DaggerArmature/upper_arm.r/lower_arm.r/hand.r/dagger"));
		gameObject2.transform.localPosition = Vector3.zero;
		gameObject2.transform.localScale = Vector3.one * 0.045f;
		gameObject2.transform.Rotate(Vector3.up, 150f, Space.Self);
		gameObject2.transform.Rotate(Vector3.forward, 50f, Space.Self);
		RoR2.ItemDisplay component3 = ItemDisplayModel.GetComponent<RoR2.ItemDisplay>();
		component3.rendererInfos = SpikestripUtil.GenerateRendererInfos(ItemDisplayModel, allRendererTypes: true);
	}

	public override void PopulateItemDisplayRules(ItemDisplayRuleDict itemDisplayRuleDict)
	{
		itemDisplayRuleDict.Add("mdlCommandoDualies", new RoR2.ItemDisplayRule[1]
		{
			new RoR2.ItemDisplayRule
			{
				ruleType = ItemDisplayRuleType.ParentedPrefab,
				followerPrefab = ItemDisplayModel,
				childName = "Chest",
				localPos = new Vector3(-0.09085f, 0.45307f, -0.09882f),
				localAngles = new Vector3(312.8834f, 328.0389f, 1.46064f),
				localScale = new Vector3(-1f, 1f, 1f)
			}
		});
		itemDisplayRuleDict.Add("mdlHuntress", new RoR2.ItemDisplayRule[1]
		{
			new RoR2.ItemDisplayRule
			{
				ruleType = ItemDisplayRuleType.ParentedPrefab,
				followerPrefab = ItemDisplayModel,
				childName = "Chest",
				localPos = new Vector3(-0.04833f, 0.32582f, -0.13447f),
				localAngles = new Vector3(295.0777f, 262.5839f, 24.96688f),
				localScale = new Vector3(-0.7f, 0.7f, 0.7f)
			}
		});
		itemDisplayRuleDict.Add("mdlToolbot", new RoR2.ItemDisplayRule[1]
		{
			new RoR2.ItemDisplayRule
			{
				ruleType = ItemDisplayRuleType.ParentedPrefab,
				followerPrefab = ItemDisplayModel,
				childName = "UpperArmL",
				localPos = new Vector3(1.02579f, -0.87221f, 0.03931f),
				localAngles = new Vector3(41.09282f, 201.7994f, 227.9452f),
				localScale = new Vector3(-6f, 6f, 6f)
			}
		});
		itemDisplayRuleDict.Add("mdlEngi", new RoR2.ItemDisplayRule[1]
		{
			new RoR2.ItemDisplayRule
			{
				ruleType = ItemDisplayRuleType.ParentedPrefab,
				followerPrefab = ItemDisplayModel,
				childName = "Chest",
				localPos = new Vector3(-0.43914f, 0.73593f, -0.04122f),
				localAngles = new Vector3(21.71623f, 356.9213f, 19.60459f),
				localScale = new Vector3(-1f, 1f, 1f)
			}
		});
		itemDisplayRuleDict.Add("mdlMage", new RoR2.ItemDisplayRule[1]
		{
			new RoR2.ItemDisplayRule
			{
				ruleType = ItemDisplayRuleType.ParentedPrefab,
				followerPrefab = ItemDisplayModel,
				childName = "Chest",
				localPos = new Vector3(-0.13001f, 0.28353f, 0.02077f),
				localAngles = new Vector3(338.4518f, 352.4123f, 357.4706f),
				localScale = new Vector3(-0.5f, 0.5f, 0.5f)
			}
		});
		itemDisplayRuleDict.Add("mdlMerc", new RoR2.ItemDisplayRule[1]
		{
			new RoR2.ItemDisplayRule
			{
				ruleType = ItemDisplayRuleType.ParentedPrefab,
				followerPrefab = ItemDisplayModel,
				childName = "Chest",
				localPos = new Vector3(-0.15166f, 0.22584f, -0.00187f),
				localAngles = new Vector3(338.1407f, 350.9364f, 0.25935f),
				localScale = new Vector3(-1f, 1f, 1f)
			}
		});
		itemDisplayRuleDict.Add("mdlTreebot", new RoR2.ItemDisplayRule[1]
		{
			new RoR2.ItemDisplayRule
			{
				ruleType = ItemDisplayRuleType.ParentedPrefab,
				followerPrefab = ItemDisplayModel,
				childName = "PlatformBase",
				localPos = new Vector3(-0.92173f, 1.16228f, -0.28341f),
				localAngles = new Vector3(13.88799f, 330.2061f, 13.8456f),
				localScale = new Vector3(-1f, 1f, 1f)
			}
		});
		itemDisplayRuleDict.Add("mdlLoader", new RoR2.ItemDisplayRule[1]
		{
			new RoR2.ItemDisplayRule
			{
				ruleType = ItemDisplayRuleType.ParentedPrefab,
				followerPrefab = ItemDisplayModel,
				childName = "Chest",
				localPos = new Vector3(-0.22704f, 0.25721f, -0.07928f),
				localAngles = new Vector3(357.9956f, 352.6357f, 358.499f),
				localScale = new Vector3(-1f, 1f, 1f)
			}
		});
		itemDisplayRuleDict.Add("mdlCroco", new RoR2.ItemDisplayRule[1]
		{
			new RoR2.ItemDisplayRule
			{
				ruleType = ItemDisplayRuleType.ParentedPrefab,
				followerPrefab = ItemDisplayModel,
				childName = "Chest",
				localPos = new Vector3(2.9446f, 1.87029f, 1.41043f),
				localAngles = new Vector3(310.6325f, 186.554f, 355.9327f),
				localScale = new Vector3(-5f, 5f, 5f)
			}
		});
		itemDisplayRuleDict.Add("mdlCaptain", new RoR2.ItemDisplayRule[1]
		{
			new RoR2.ItemDisplayRule
			{
				ruleType = ItemDisplayRuleType.ParentedPrefab,
				followerPrefab = ItemDisplayModel,
				childName = "Chest",
				localPos = new Vector3(-0.26834f, 0.31055f, 0.05537f),
				localAngles = new Vector3(309.3522f, 25.49826f, 357.8659f),
				localScale = new Vector3(-1f, 1f, 1f)
			}
		});
		itemDisplayRuleDict.Add("mdlBandit2", new RoR2.ItemDisplayRule[1]
		{
			new RoR2.ItemDisplayRule
			{
				ruleType = ItemDisplayRuleType.ParentedPrefab,
				followerPrefab = ItemDisplayModel,
				childName = "ClavicleL",
				localPos = new Vector3(-0.047f, 0.02766f, -0.00076f),
				localAngles = new Vector3(355.8817f, 136.7875f, 249.4395f),
				localScale = new Vector3(-1f, 1f, 1f)
			}
		});
		itemDisplayRuleDict.Add("mdlRailGunner", new RoR2.ItemDisplayRule[1]
		{
			new RoR2.ItemDisplayRule
			{
				ruleType = ItemDisplayRuleType.ParentedPrefab,
				followerPrefab = ItemDisplayModel,
				childName = "Chest",
				localPos = new Vector3(-0.29022f, 0.36264f, -0.20977f),
				localAngles = new Vector3(343.8759f, 308.0452f, 30.74546f),
				localScale = new Vector3(-0.86029f, 0.86029f, 0.86029f)
			}
		});
		itemDisplayRuleDict.Add("mdlVoidSurvivor", new RoR2.ItemDisplayRule[1]
		{
			new RoR2.ItemDisplayRule
			{
				ruleType = ItemDisplayRuleType.ParentedPrefab,
				followerPrefab = ItemDisplayModel,
				childName = "Chest",
				localPos = new Vector3(-0.20697f, 0.13112f, -0.25777f),
				localAngles = new Vector3(289.6892f, 304.1104f, 32.67938f),
				localScale = new Vector3(-1f, 1f, 1f)
			}
		});
		itemDisplayRuleDict.Add("mdlScav", new RoR2.ItemDisplayRule[1]
		{
			new RoR2.ItemDisplayRule
			{
				ruleType = ItemDisplayRuleType.ParentedPrefab,
				followerPrefab = ItemDisplayModel,
				childName = "Weapon",
				localPos = new Vector3(3.89388f, 16.68199f, (float)Math.PI * 121f / 123f),
				localAngles = new Vector3(317.2897f, 199.9094f, 306.977f),
				localScale = new Vector3(-5f, 5f, 5f)
			}
		});
		itemDisplayRuleDict.Add("mdlEngiTurret", new RoR2.ItemDisplayRule[1]
		{
			new RoR2.ItemDisplayRule
			{
				ruleType = ItemDisplayRuleType.ParentedPrefab,
				followerPrefab = ItemDisplayModel,
				childName = "Head",
				localPos = new Vector3(-0.62321f, 0.07351f, 0.16657f),
				localAngles = new Vector3(348.5144f, 330.5276f, 18.23479f),
				localScale = new Vector3(-1.91032f, 1.91032f, 1.91032f)
			}
		});
	}
}
