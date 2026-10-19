using System;
using System.Collections.Generic;
using System.Linq;
using HG;
using R2API;
using RoR2;
using UnityEngine;

namespace GrooveSaladSpikestripContent.Content;

public class DeathElite : SpikestripEliteBase<DeathElite>
{
	public class DeathAffixBuffBehaviourServer : CharacterBody.ItemBehavior, IOnIncomingDamageServerReceiver
	{
		public SphereSearch sphereSearch;

		public void Start()
		{
			if ((bool)body.healthComponent && !body.healthComponent.onIncomingDamageReceivers.Contains(this))
			{
				ref IOnIncomingDamageServerReceiver[] onIncomingDamageReceivers = ref body.healthComponent.onIncomingDamageReceivers;
				IOnIncomingDamageServerReceiver value = this;
				ArrayUtils.ArrayAppend(ref onIncomingDamageReceivers, in value);
				Debug.Log("Insert Self");
			}
			sphereSearch = new SphereSearch();
		}

		public void OnDestroy()
		{
			if (!body.healthComponent || !body.healthComponent.onIncomingDamageReceivers.Contains(this))
			{
				return;
			}
			for (int i = 0; i < body.healthComponent.onIncomingDamageReceivers.Length; i++)
			{
				if (body.healthComponent.onIncomingDamageReceivers[i] == this)
				{
					ArrayUtils.ArrayRemoveAtAndResize(ref body.healthComponent.onIncomingDamageReceivers, i);
					Debug.Log("Destroy Self");
				}
			}
		}

		public void OnIncomingDamageServer(DamageInfo damageInfo)
		{
			if (!damageInfo.rejected && SpreadDamageToAlly())
			{
				damageInfo.rejected = true;
				DamageNumberManager.instance.SpawnDamageNumber(damageInfo.damage, damageInfo.position, damageInfo.crit, TeamIndex.None, DamageColorIndex.Default);
			}
		}

		public bool SpreadDamageToAlly()
		{
			List<HurtBox> list = new List<HurtBox>();
			TeamMask mask = default(TeamMask);
			mask.AddTeam(body.teamComponent.teamIndex);
			sphereSearch.radius = 25f;
			sphereSearch.mask = LayerIndex.entityPrecise.mask;
			sphereSearch.origin = base.transform.position;
			sphereSearch.RefreshCandidates();
			sphereSearch.FilterCandidatesByHurtBoxTeam(mask);
			sphereSearch.OrderCandidatesByDistance();
			sphereSearch.FilterCandidatesByDistinctHurtBoxEntities();
			sphereSearch.GetHurtBoxes(list);
			sphereSearch.ClearCandidates();
			if (list.Count == 0)
			{
				return false;
			}
			for (int i = 0; i < list.Count; i++)
			{
				HurtBox hurtBox = list[i];
				HealthComponent healthComponent = (hurtBox ? hurtBox.healthComponent : null);
				if ((bool)healthComponent && (bool)healthComponent.body && !healthComponent.body.HasBuff(SpikestripContentBase<DeathElite>.instance.AffixBuff))
				{
					return true;
				}
			}
			return false;
		}
	}

	public override string EliteName => "Yeah";

	public override Color EliteColor => Color.blue;

	public override EliteTierDefinition MainEliteTierDefinition => SpikestripEliteBase<DeathElite>.tierTwoEliteDefault;

	public override Sprite AffixBuffSprite => Base.GroovyAssetBundle.LoadAsset<Sprite>("texBuffAffixGravity.png");

	public override Sprite EquipmentIcon => Base.GroovyAssetBundle.LoadAsset<Sprite>("texAffixGravityIcon.png");

	public override string EquipmentName => "Their Madness";

	public override string AffixDescriptionMainWord => "death";

	public override Texture2D EliteRampTexture => Base.GroovyAssetBundle.LoadAsset<Texture2D>("texRampEliteYeah.png");

	public override Type AffixBuffBehaviour => typeof(DeathAffixBuffBehaviourServer);

	public override bool ServerOnlyAffixBuffBehaviour => true;

	public override bool IsEnabled => false;

	public override void Init()
	{
		base.Init();
		ParticleSystemRenderer component = LegacyResourcesAPI.Load<GameObject>("prefabs/temporaryvisualeffects/DoppelgangerEffect").transform.Find("Particles/PulseEffect, Tentacles").GetComponent<ParticleSystemRenderer>();
		Material material = UnityEngine.Object.Instantiate(component.material);
		material.SetColor("_TintColor", Color.red);
		component.material = material;
	}

	public override void AssignEquipmentValues()
	{
		EquipmentPickupModel = CreateAffixModel(Color.blue);
	}

	public override void PopulateItemDisplayRules(ItemDisplayRuleDict itemDisplayRuleDict)
	{
	}
}
