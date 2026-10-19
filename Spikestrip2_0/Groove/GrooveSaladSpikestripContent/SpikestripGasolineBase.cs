using System;
using System.Collections.Generic;
using GrooveSaladSpikestripContent.Content;
using On.RoR2.UI.LogBook;
using R2API;
using RoR2;
using RoR2.ExpansionManagement;
using UnityEngine;
using Spikestrip2_0.Core;

namespace GrooveSaladSpikestripContent;

public abstract class SpikestripGasolineBase<T> : ItemBase<T> where T : SpikestripGasolineBase<T>
{
	public static GameObject igniteExplosionVFX = LegacyResourcesAPI.Load<GameObject>("Prefabs/Effects/ImpactEffects/IgniteExplosionVFX");

	private static readonly SphereSearch gasolineSphereSearch = new SphereSearch();

	private static readonly List<HurtBox> gasolineHurtBoxBuffer = new List<HurtBox>();

	public GameObject GasolineEffectPrefab;

	public virtual float RadiusBase { get; } = 12f;

	public virtual float RadiusStack { get; } = 4f;

	public virtual float ExplosionDamageBase { get; } = 1.5f;

	public virtual float ExplosionDamageStack { get; } = 0f;

	public virtual float ProcChanceBase { get; } = 100f;

	public virtual float ProcChanceStack { get; } = 0f;

	public virtual DamageType DamageType { get; } = DamageType.Generic;

	public virtual float ProcCoefficient { get; } = 0f;

	public virtual BlastAttack.FalloffModel FalloffModel { get; } = BlastAttack.FalloffModel.None;

	public virtual bool InvertTeamFilter { get; } = false;

	public override string ItemToken => "IGNITEONKILL";

	public override Type DependsOn => typeof(Arson);

	public abstract void AssignEffectPrefab();

	public virtual void OnProcServer(HurtBox hurtBox, Vector3 procOrigin, int stack)
	{
	}

	public override void PopulateItemDisplayRules(ItemDisplayRuleDict itemDisplayRuleDict)
	{
	}

	public bool ShouldProc(int stack, CharacterMaster master)
	{
		float num = SpikestripUtil.StackScaling(ProcChanceBase, ProcChanceStack, stack);
		if (num >= 100f)
		{
			return true;
		}
		if (num <= 0f)
		{
			return false;
		}
		return Util.CheckRoll(num, master);
	}

	public override void Init()
	{
		base.Init();
		AssignEffectPrefab();
		Arson.gasolineItemDefs.Add(ItemDef);
		GlobalEventManager.onCharacterDeathGlobal += ProcItem;
		LogBookController.CanSelectItemEntry += LogBookController_CanSelectItemEntry;
	}

	private bool LogBookController_CanSelectItemEntry(LogBookController.orig_CanSelectItemEntry orig, ItemDef itemDef, Dictionary<ExpansionDef, bool> expansionAvailability)
	{
		return !(itemDef == ItemDef) && orig(itemDef, expansionAvailability);
	}

	private void ProcItem(DamageReport damageReport)
	{
		CharacterBody attackerBody = damageReport.attackerBody;
		CharacterBody victimBody = damageReport.victimBody;
		CharacterMaster attackerMaster = damageReport.attackerMaster;
		TeamIndex attackerTeamIndex = damageReport.attackerTeamIndex;
		if (!attackerBody || !victimBody || !attackerMaster || !attackerMaster.inventory)
		{
			return;
		}
		int itemCount = attackerMaster.inventory.GetItemCountEffective(ItemDef);
		if (itemCount > 0 && ShouldProc(itemCount, attackerMaster))
		{
			float num = SpikestripUtil.StackScaling(RadiusBase, RadiusStack, itemCount) + victimBody.radius;
			float num2 = SpikestripUtil.StackScaling(ExplosionDamageBase, ExplosionDamageStack, itemCount) * attackerBody.damage;
			TeamMask mask = TeamMask.GetUnprotectedTeams(attackerTeamIndex);
			if (InvertTeamFilter)
			{
				mask = TeamMask.none;
				mask.AddTeam(attackerTeamIndex);
			}
			Vector3 corePosition = victimBody.corePosition;
			gasolineSphereSearch.origin = corePosition;
			gasolineSphereSearch.mask = LayerIndex.entityPrecise.mask;
			gasolineSphereSearch.radius = num;
			gasolineSphereSearch.RefreshCandidates();
			gasolineSphereSearch.FilterCandidatesByHurtBoxTeam(mask);
			gasolineSphereSearch.FilterCandidatesByDistinctHurtBoxEntities();
			gasolineSphereSearch.OrderCandidatesByDistance();
			gasolineSphereSearch.GetHurtBoxes(gasolineHurtBoxBuffer);
			gasolineSphereSearch.ClearCandidates();
			for (int i = 0; i < gasolineHurtBoxBuffer.Count; i++)
			{
				HurtBox hurtBox = gasolineHurtBoxBuffer[i];
				OnProcServer(hurtBox, corePosition, itemCount);
			}
			gasolineHurtBoxBuffer.Clear();
			if (num2 > 0f)
			{
				BlastAttack blastAttack = new BlastAttack();
				blastAttack.radius = num;
				blastAttack.baseDamage = num2;
				blastAttack.procCoefficient = ProcCoefficient;
				blastAttack.crit = Util.CheckRoll(damageReport.attackerBody.crit, damageReport.attackerMaster);
				blastAttack.damageColorIndex = DamageColorIndex.Item;
				blastAttack.attackerFiltering = AttackerFiltering.Default;
				blastAttack.falloffModel = FalloffModel;
				blastAttack.attacker = damageReport.attacker;
				blastAttack.teamIndex = attackerTeamIndex;
				blastAttack.position = corePosition;
				blastAttack.damageType = DamageType;
				blastAttack.Fire();
			}
			if ((bool)GasolineEffectPrefab)
			{
				EffectManager.SpawnEffect(GasolineEffectPrefab, new EffectData
				{
					origin = corePosition,
					scale = num,
					rotation = Util.QuaternionSafeLookRotation(damageReport.damageInfo.force)
				}, transmit: true);
			}
		}
	}

	public GameObject CreateGasolineModel(Color color)
	{
		GameObject gameObject = PrefabAPI.InstantiateClone(LegacyResourcesAPI.Load<GameObject>("prefabs/pickupmodels/PickupGasoline"), "Pickup" + ItemName, false);
		MeshRenderer componentInChildren = gameObject.GetComponentInChildren<MeshRenderer>();
		Material material = UnityEngine.Object.Instantiate(componentInChildren.material);
		material.color = color;
		material.mainTexture = SpikestripContent.GrooveBundle.LoadAsset<Texture>("texGasolineUncoloredDiffuse");
		componentInChildren.material = material;
		return gameObject;
	}
}
