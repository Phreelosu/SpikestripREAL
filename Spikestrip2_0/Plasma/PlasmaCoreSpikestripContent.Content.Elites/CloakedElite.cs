using System;
using GrooveSaladSpikestripContent;
using On.RoR2;
using PlasmaCoreSpikestripContent.Core;
using R2API;
using RoR2;
using UnityEngine;
using UnityEngine.Networking;
using Spikestrip2_0.Core;

namespace PlasmaCoreSpikestripContent.Content.Elites;

public class CloakedElite : EliteBase<CloakedElite>
{
	public class CloakedAffixBuffBehaviour : RoR2.CharacterBody.ItemBehavior, IOnTakeDamageServerReceiver
	{
		public void Start()
		{
			body.AddTimedBuff(RoR2.RoR2Content.Buffs.Cloak, 30f);
			body.AddBuff(scriptableObject.MiscBuffs[0]);
			body.AddBuff(scriptableObject.MiscBuffs[0]);
			ref IOnTakeDamageServerReceiver[] onTakeDamageReceivers = ref body.healthComponent.onTakeDamageReceivers;
			IOnTakeDamageServerReceiver value = this;
			SpikestripUtil.AppendArrayIfMissing(ref onTakeDamageReceivers, in value);
		}

		public void FixedUpdate()
		{
			if (NetworkServer.active && body.outOfCombatStopwatch >= 5f && !body.HasBuff(RoR2.RoR2Content.Buffs.Cloak) && body.healthComponent.timeSinceLastHit >= 10f)
			{
				for (int i = 0; i < 2; i++)
				{
					if (body.HasBuff(scriptableObject.MiscBuffs[0]))
					{
						body.RemoveBuff(scriptableObject.MiscBuffs[0]);
					}
				}
				body.AddTimedBuff(RoR2.RoR2Content.Buffs.Cloak, 15f);
				body.AddBuff(scriptableObject.MiscBuffs[0]);
				body.AddBuff(scriptableObject.MiscBuffs[0]);
				RoR2.EffectManager.SpawnEffect(SmokebombEffect, new RoR2.EffectData
				{
					_origin = body.transform.position,
					rotation = body.transform.rotation,
					scale = body.bestFitRadius * 0.4f
				}, transmit: true);
			}
			else if (!body.HasBuff(scriptableObject.MiscBuffs[0]) && body.HasBuff(RoR2.RoR2Content.Buffs.Cloak))
			{
				body.RemoveOldestTimedBuff(RoR2.RoR2Content.Buffs.Cloak);
			}
		}

		public void OnDestroy()
		{
			ref IOnTakeDamageServerReceiver[] onTakeDamageReceivers = ref body.healthComponent.onTakeDamageReceivers;
			IOnTakeDamageServerReceiver value = this;
			SpikestripUtil.RemoveFromArray(ref onTakeDamageReceivers, in value);
		}

		public void OnTakeDamageServer(RoR2.DamageReport damageReport)
		{
			if (body.HasBuff(scriptableObject.MiscBuffs[0]))
			{
				body.RemoveBuff(scriptableObject.MiscBuffs[0]);
				RoR2.EffectManager.SpawnEffect(SmokebombEffect, new RoR2.EffectData
				{
					_origin = damageReport.damageInfo.position,
					rotation = damageReport.victimBody.transform.rotation,
					scale = body.bestFitRadius * 0.3f
				}, transmit: true);
			}
		}
	}

	public static SSEliteBaseSO scriptableObject;

	public static GameObject CloakedProjectile;

	public static GameObject CloakedTracer;

	public static GameObject SmokebombEffect;

	public override string EliteName => "Veiled";

	public override Color EliteColor => scriptableObject.eliteColor;

	public override EliteTierDefinition MainEliteTierDefinition => scriptableObject.tier switch
	{
		SSEliteBaseSO.EliteTierType.TierTwo => EliteBase<CloakedElite>.tierTwoEliteDefault, 
		SSEliteBaseSO.EliteTierType.Honor => EliteBase<CloakedElite>.honorEliteDefault, 
		_ => EliteBase<CloakedElite>.tierOneEliteDefault, 
	};

	public override EliteTierDefinition[] ExtraEliteTierDefitions
	{
		get
		{
			if (scriptableObject.tier == SSEliteBaseSO.EliteTierType.TierOne)
			{
				return new EliteTierDefinition[1] { EliteBase<CloakedElite>.honorEliteDefault };
			}
			return null;
		}
	}

	public override Sprite AffixBuffSprite => scriptableObject.buffSprite;

	public override Sprite EquipmentIcon => scriptableObject.EquipmentIcon;

	public override string EquipmentName => scriptableObject.EquipmentName;

	public override string AffixDescriptionMainWord => scriptableObject.AffixDescriptionMainWord;

	public override Texture2D EliteRampTexture => scriptableObject.EliteRampTexture;

	public override Type AffixBuffBehaviour => typeof(CloakedAffixBuffBehaviour);

	public override bool ServerOnlyAffixBuffBehaviour => false;

	public override void Init()
	{
		PlasmaCorePlugin.eliteBaseScriptableObjects.TryGetValue(EliteName, out scriptableObject);
		CloakedTracer = PrefabAPI.InstantiateClone(SpikestripUtil.AddressablesLoad<GameObject>("3f67c04a8c335bb4a81a0fe80f4b074e"), "CloakedTracer", false);
		ContentAddition.AddEffect(CloakedTracer);
		CloakedTracer.GetComponent<LineRenderer>().material = SpikestripUtil.AddressablesLoad<Material>("cc20cbe750bfc654e871cd6df71758df");
		CloakedTracer.GetComponentInChildren<ParticleSystemRenderer>().material = SpikestripUtil.AddressablesLoad<Material>("6c979f58a0f01b14f981c739264f8280");
		CloakedTracer.GetComponentInChildren<ParticleSystem>().startSize *= 0.5f;
		CloakedProjectile = scriptableObject.MiscGameObjects[0];
		SmokebombEffect = SpikestripUtil.AddressablesLoad<GameObject>("533b5c10469c1314ab21bea8fac7462e");
		ContentAddition.AddBuffDef(scriptableObject.MiscBuffs[0]);
		base.Init();
		EquipmentDef.dropOnDeathChance = 0.00025f;
	}

	private void GlobalEventManager_OnHitEnemy(On.RoR2.GlobalEventManager.orig_OnHitEnemy orig, RoR2.GlobalEventManager self, RoR2.DamageInfo damageInfo, GameObject victim)
	{
		orig(self, damageInfo, victim);
		if (damageInfo.procCoefficient > 0f && (bool)damageInfo.attacker)
		{
			RoR2.CharacterBody component = damageInfo.attacker.GetComponent<RoR2.CharacterBody>();
			if ((bool)component && !component.HasBuff(RoR2.RoR2Content.Buffs.Cloak) && component.HasBuff(AffixBuff))
			{
				component.AddTimedBuff(RoR2.RoR2Content.Buffs.Cloak, 5f * damageInfo.procCoefficient);
			}
		}
	}

	public override void AssignEquipmentValues()
	{
		EquipmentPickupModel = CreateAffixModel(scriptableObject.affixColor);
		EquipmentDisplayModel = scriptableObject.EquipmentDisplayModel;
	}

	public override void PopulateItemDisplayRules(ItemDisplayRuleDict itemDisplayRuleDict)
	{
		RegisterDisplayParent(LegacyResourcesAPI.Load<RoR2.EquipmentDef>("EquipmentDefs/AffixHaunted"), EquipmentDisplayModel);
	}
}
