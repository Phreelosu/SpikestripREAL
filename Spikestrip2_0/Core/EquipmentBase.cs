using R2API;
using RoR2;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Networking;

namespace Spikestrip2_0.Core;

public abstract class EquipmentBase<T> : ContentBase<T>, ItemBehaviourUpdater.IItem where T : EquipmentBase<T>
{
	public class EquipmentTargetBase : CharacterBody.ItemBehavior
	{
		public enum TargetOption
		{
			Enemy,
			Ally
		}

		public EquipmentSlot equipmentSlot;

		public BullseyeSearch targetFinder = new BullseyeSearch();

		public virtual float MaxDistance { get; } = 0f;

		public virtual float MaxAngleFilter { get; } = 10f;

		public virtual BullseyeSearch.SortMode SortMode { get; } = BullseyeSearch.SortMode.Angle;

		public virtual TargetOption TargetType { get; } = TargetOption.Enemy;

		public virtual GameObject GetTargetIndicatorPrefab(EquipmentSlot.UserTargetInfo info)
		{
			return LegacyResourcesAPI.Load<GameObject>("Prefabs/LightningIndicator");
		}

		public void Awake()
		{
			OnAwake();
		}

		public virtual void OnAwake()
		{
		}

		public void OnDestroy()
		{
			OnOnDestroy();
		}

		public virtual void OnOnDestroy()
		{
		}

		public void Update()
		{
			OnUpdate();
		}

		public virtual void OnUpdate()
		{
		}

		public void ConfigureTargetFinderBase()
		{
			targetFinder.teamMaskFilter = TeamMask.allButNeutral;
			targetFinder.teamMaskFilter.RemoveTeam(equipmentSlot.teamComponent.teamIndex);
			targetFinder.sortMode = SortMode;
			targetFinder.filterByLoS = true;
			float extraRaycastDistance;
			Ray ray = CameraRigController.ModifyAimRayIfApplicable(equipmentSlot.GetAimRay(), base.gameObject, out extraRaycastDistance);
			targetFinder.searchOrigin = ray.origin;
			targetFinder.searchDirection = ray.direction;
			targetFinder.maxAngleFilter = MaxAngleFilter;
			targetFinder.maxDistanceFilter = MaxDistance;
			targetFinder.viewer = equipmentSlot.characterBody;
		}

		public void ConfigureTargetFinderForEnemies()
		{
			ConfigureTargetFinderBase();
			targetFinder.teamMaskFilter = TeamMask.GetUnprotectedTeams(equipmentSlot.teamComponent.teamIndex);
			targetFinder.RefreshCandidates();
			targetFinder.FilterOutGameObject(base.gameObject);
		}

		public void ConfigureTargetFinderForFriendlies()
		{
			ConfigureTargetFinderBase();
			targetFinder.teamMaskFilter = TeamMask.none;
			targetFinder.teamMaskFilter.AddTeam(equipmentSlot.teamComponent.teamIndex);
			targetFinder.RefreshCandidates();
			targetFinder.FilterOutGameObject(base.gameObject);
		}
	}

	public class EquipmentSingleTargetBehaviour : EquipmentTargetBase
	{
		public Indicator targetIndicator;

		public EquipmentSlot.UserTargetInfo currentTarget;

		public override void OnAwake()
		{
			base.OnAwake();
			targetIndicator = new Indicator(base.gameObject, null);
			targetIndicator.active = false;
		}

		public override void OnUpdate()
		{
			base.OnUpdate();
			if ((bool)equipmentSlot)
			{
				UpdateTargets();
			}
		}

		public override void OnOnDestroy()
		{
			base.OnOnDestroy();
			targetIndicator.active = false;
		}

		public void UpdateTargets()
		{
			if (equipmentSlot.stock > 0)
			{
				switch (TargetType)
				{
				case TargetOption.Enemy:
					ConfigureTargetFinderForEnemies();
					break;
				case TargetOption.Ally:
					ConfigureTargetFinderForFriendlies();
					break;
				}
				currentTarget = new EquipmentSlot.UserTargetInfo(targetFinder.GetResults().FirstOrDefault());
			}
			else
			{
				InvalidateCurrentTarget();
			}
			bool flag = currentTarget.transformToIndicateAt;
			if (flag)
			{
				targetIndicator.visualizerPrefab = GetTargetIndicatorPrefab(currentTarget);
			}
			targetIndicator.active = flag;
			targetIndicator.targetTransform = (flag ? currentTarget.transformToIndicateAt : null);
		}

		public void InvalidateCurrentTarget()
		{
			currentTarget = default(EquipmentSlot.UserTargetInfo);
		}
	}

	public class EquipmentMultiTargetBehaviour : EquipmentTargetBase
	{
		public List<Indicator> indicators = new List<Indicator>();

		public List<EquipmentSlot.UserTargetInfo> currentTargets = new List<EquipmentSlot.UserTargetInfo>();

		public float updateStopwatch;

		public virtual float UpdateInterval { get; } = 0.05f;

		public override void OnAwake()
		{
			base.OnAwake();
		}

		public override void OnUpdate()
		{
			base.OnUpdate();
			if ((bool)equipmentSlot)
			{
				updateStopwatch += Time.fixedDeltaTime;
				if (updateStopwatch >= UpdateInterval)
				{
					updateStopwatch -= UpdateInterval;
					UpdateTargets();
				}
			}
		}

		public override void OnOnDestroy()
		{
			base.OnOnDestroy();
			for (int num = indicators.Count - 1; num >= 0; num--)
			{
				indicators[num].active = false;
			}
		}

		public Indicator NewIndicator()
		{
			Indicator indicator = new Indicator(base.gameObject, null);
			indicator.active = false;
			indicators.Add(indicator);
			return indicator;
		}

		public void UpdateTargets()
		{
			InvalidateCurrentTargets();
			if (equipmentSlot.stock > 0)
			{
				switch (TargetType)
				{
				case TargetOption.Enemy:
					ConfigureTargetFinderForEnemies();
					break;
				case TargetOption.Ally:
					ConfigureTargetFinderForFriendlies();
					break;
				}
				List<HurtBox> list = targetFinder.GetResults().Reverse().ToList();
				for (int i = 0; i < list.Count; i++)
				{
					EquipmentSlot.UserTargetInfo userTargetInfo = new EquipmentSlot.UserTargetInfo(list[i]);
					currentTargets.Add(userTargetInfo);
					Indicator indicator = ((i < indicators.Count) ? indicators[i] : NewIndicator());
					indicator.visualizerPrefab = GetTargetIndicatorPrefab(userTargetInfo);
					indicator.targetTransform = userTargetInfo.transformToIndicateAt;
					indicator.active = true;
				}
			}
			for (int num = indicators.Count - 1; num >= currentTargets.Count; num--)
			{
				indicators[num].active = false;
				indicators.RemoveAt(num);
			}
		}

		public void InvalidateCurrentTargets()
		{
			for (int num = currentTargets.Count - 1; num >= 0; num--)
			{
				currentTargets.Clear();
			}
		}
	}



	public EquipmentDef EquipmentDef;

	public GameObject EquipmentPickupModel;

	public GameObject EquipmentDisplayModel;

	public ItemDisplayRuleDict idrs = new ItemDisplayRuleDict(Array.Empty<ItemDisplayRule>());

	public override string ConfigName => EquipmentName;

	public override string ConfigSection => "Equipment";

	public abstract string EquipmentName { get; }

	public abstract string EquipmentToken { get; }

	public abstract string EquipmentPickup { get; }

	public abstract string EquipmentDescription { get; }

	public abstract string EquipmentLore { get; }

	public abstract Sprite EquipmentIcon { get; }

	public virtual bool AppearsInSinglePlayer => true;

	public virtual bool AppearsInMultiPlayer => true;

	public virtual bool CanDrop => true;

	public virtual float Cooldown => 60f;

	public virtual bool EnigmaCompatible => true;

	public virtual bool CanBeRandomlyTriggered => true;

	public virtual bool IsBoss => false;

	public virtual bool IsLunar => false;

	public virtual BuffDef PassiveBuff => null;

	public virtual bool CanActivate => true;

	public virtual float DropOnDeathChance => 0f;

	public virtual Type EquipmentBehaviour => null;

	public virtual bool ServerOnlyEquipmentBehaviour => false;

	public abstract void PopulateItemDisplayRules(ItemDisplayRuleDict itemDisplayRuleDict);

	public abstract void AssignEquipmentValues();

	public virtual bool OnActivationServer(EquipmentSlot equipmentSlot)
	{
		return false;
	}

	public override void Init()
	{
		AssignEquipmentValues();
		PopulateItemDisplayRules(idrs);
		EquipmentDef = ScriptableObject.CreateInstance<EquipmentDef>();
		EquipmentDef.name = "EQUIPMENT_" + EquipmentToken;
		EquipmentDef.nameToken = "EQUIPMENT_" + EquipmentToken + "_NAME";
		EquipmentDef.pickupToken = "EQUIPMENT_" + EquipmentToken + "_PICKUP";
		EquipmentDef.descriptionToken = "EQUIPMENT_" + EquipmentToken + "_DESCRIPTION";
		EquipmentDef.loreToken = "EQUIPMENT_" + EquipmentToken + "_LORE";
		EquipmentDef.pickupModelPrefab = EquipmentPickupModel;
		EquipmentDef.pickupIconSprite = EquipmentIcon;
		EquipmentDef.appearsInSinglePlayer = AppearsInSinglePlayer;
		EquipmentDef.appearsInMultiPlayer = AppearsInMultiPlayer;
		EquipmentDef.canDrop = CanDrop;
		EquipmentDef.cooldown = Cooldown;
		EquipmentDef.enigmaCompatible = EnigmaCompatible;
		EquipmentDef.passiveBuffDef = PassiveBuff;
		EquipmentDef.isBoss = IsBoss;
		EquipmentDef.isLunar = IsLunar;
		EquipmentDef.canBeRandomlyTriggered = CanBeRandomlyTriggered;
		EquipmentDef.dropOnDeathChance = DropOnDeathChance;
		EquipmentDef.requiredExpansion = RequiredExpansionHolder;
		EquipmentDef.colorIndex = IsLunar ? ColorCatalog.ColorIndex.LunarItem : IsBoss ? ColorCatalog.ColorIndex.BossItem : ColorCatalog.ColorIndex.Equipment;
		ItemAPI.Add(new CustomEquipment(EquipmentDef, idrs));

		LanguageAPI.Add(EquipmentDef.nameToken, EquipmentName);
		LanguageAPI.Add(EquipmentDef.pickupToken, EquipmentPickup);
		LanguageAPI.Add(EquipmentDef.descriptionToken, EquipmentDescription);
		if (!string.IsNullOrEmpty(EquipmentLore))
		{
			LanguageAPI.Add(EquipmentDef.loreToken, EquipmentLore);
		}

		if (CanActivate)
		{
			On.RoR2.EquipmentSlot.PerformEquipmentAction += PerformEquipmentAction;
		}
		if (EquipmentBehaviour != null)
		{
			ItemBehaviourUpdater.Register(this);
		}
	}

	private bool PerformEquipmentAction(On.RoR2.EquipmentSlot.orig_PerformEquipmentAction orig, EquipmentSlot self, EquipmentDef equipmentDef)
	{
		if (NetworkServer.active && equipmentDef == EquipmentDef)
		{
			return OnActivationServer(self);
		}
		return orig(self, equipmentDef);
	}

	void ItemBehaviourUpdater.IItem.UpdateBehaviour(CharacterBody body)
	{
		if (NetworkServer.active || !ServerOnlyEquipmentBehaviour)
		{
			AddEquipBehaviour(body);
		}
	}

	public void AddEquipBehaviour(CharacterBody characterBody)
	{
		bool holding = characterBody.inventory.currentEquipmentIndex == EquipmentDef.equipmentIndex;
		CharacterBody.ItemBehavior itemBehavior = (CharacterBody.ItemBehavior)characterBody.GetComponent(EquipmentBehaviour);
		if (holding)
		{
			if (!itemBehavior)
			{
				itemBehavior = (CharacterBody.ItemBehavior)characterBody.gameObject.AddComponent(EquipmentBehaviour);
				if (itemBehavior is EquipmentTargetBase targetBehaviour)
				{
					targetBehaviour.equipmentSlot = characterBody.GetComponent<EquipmentSlot>();
				}
				itemBehavior.body = characterBody;
				itemBehavior.enabled = true;
			}
		}
		else if (itemBehavior)
		{
			UnityEngine.Object.Destroy(itemBehavior);
		}
	}
}
