using System;
using GrooveSaladSpikestripContent;
using PlasmaCoreSpikestripContent.Components;
using PlasmaCoreSpikestripContent.Core;
using R2API;
using RoR2;
using RoR2.Projectile;
using UnityEngine;
using UnityEngine.Networking;
using Spikestrip2_0.Core;

namespace PlasmaCoreSpikestripContent.Content.Elites;

public class RagingElite : EliteBase<RagingElite>
{
	public class RageAffixBuffBehaviourServer : CharacterBody.ItemBehavior, IOnKilledServerReceiver, IOnTakeDamageServerReceiver
	{
		public float currentRadius;

		private GameObject affixRageWard;

		public void Start()
		{
			Util.PlaySound("Play_AragoniteElite_Spawn", base.gameObject);
			ref IOnTakeDamageServerReceiver[] onTakeDamageReceivers = ref body.healthComponent.onTakeDamageReceivers;
			IOnTakeDamageServerReceiver value = this;
			SpikestripUtil.AppendArrayIfMissing(ref onTakeDamageReceivers, in value);
		}

		private void OnEnable()
		{
			CharacterBody component = GetComponent<CharacterBody>();
			if (NetworkServer.active)
			{
				affixRageWard = UnityEngine.Object.Instantiate(scriptableObject.MiscGameObjects[0]);
				affixRageWard.GetComponent<TeamFilter>().teamIndex = GetComponent<TeamComponent>().teamIndex;
				affixRageWard.GetComponent<BuffWard>().Networkradius = component.radius;
				affixRageWard.GetComponent<NetworkedBodyAttachment>().AttachToGameObjectAndSpawn(base.gameObject);
				NetworkServer.Spawn(affixRageWard);
			}
		}

		private void FixedUpdate()
		{
			if (NetworkServer.active)
			{
				if (currentRadius > 0f)
				{
					currentRadius = Mathf.Min(Mathf.Max(currentRadius - Time.fixedDeltaTime * 5f, 0f), 75f);
				}
				if ((bool)affixRageWard)
				{
					affixRageWard.GetComponent<BuffWard>().Networkradius = currentRadius;
				}
			}
		}

		private void OnDisable()
		{
			if ((bool)affixRageWard)
			{
				UnityEngine.Object.Destroy(affixRageWard);
			}
		}

		public void OnKilledServer(DamageReport damageReport)
		{
			if ((bool)affixRageWard)
			{
				affixRageWard.GetComponent<BuffWard>().Networkradius = 0f;
			}
			if ((bool)body)
			{
				body.AddBuff(scriptableObject.MiscBuffs[1]);
				RagingEliteProjectileSpawner component = UnityEngine.Object.Instantiate(scriptableObject.MiscGameObjects[3], damageReport.attackerBody ? damageReport.attackerBody.corePosition : body.corePosition, Quaternion.identity).GetComponent<RagingEliteProjectileSpawner>();
				component.isCrit = body.RollCrit();
				component.targetBody = damageReport.attackerBody;
				component.owner = base.gameObject;
				component.ownerBodyDamage = body.damage;
			}
		}

		public void OnTakeDamageServer(DamageReport damageReport)
		{
			if (!(damageReport.damageInfo.procCoefficient > 0f) || !damageReport.damageInfo.attacker)
			{
				return;
			}
			CharacterBody component = damageReport.damageInfo.attacker.GetComponent<CharacterBody>();
			CharacterBody component2 = damageReport.victim.GetComponent<CharacterBody>();
			if ((bool)component && (bool)component2 && component2.HasBuff(RagingElite.instance.AffixBuff) && !component2.HasBuff(scriptableObject.MiscBuffs[1]))
			{
				Vector3 position = component.footPosition;
				if ((bool)component.characterMotor && component.characterMotor.isGrounded && (bool)component && Physics.Raycast(new Ray(component.footPosition, Vector3.down), out var hitInfo, 20f))
				{
					position = hitInfo.point;
				}
				FireProjectileInfo fireProjectileInfo = new FireProjectileInfo
				{
					crit = component2.RollCrit(),
					owner = component2.gameObject,
					position = position,
					projectilePrefab = deathProjectile,
					rotation = Quaternion.Euler(0f, UnityEngine.Random.Range(0f, 360f), 0f),
					damage = 5f * component2.damage
				};
				ProjectileManager.instance.FireProjectile(fireProjectileInfo);
				component2.AddTimedBuff(scriptableObject.MiscBuffs[1], 1.5f);
			}
			if (damageReport.damageDealt >= 0.1f * damageReport.victim.fullCombinedHealth)
			{
				currentRadius += 40f;
			}
			else
			{
				currentRadius += 10f;
			}
		}

		public void OnDestroy()
		{
			ref IOnTakeDamageServerReceiver[] onTakeDamageReceivers = ref body.healthComponent.onTakeDamageReceivers;
			IOnTakeDamageServerReceiver value = this;
			SpikestripUtil.RemoveFromArray(ref onTakeDamageReceivers, in value);
		}
	}

	public static SSEliteBaseSO scriptableObject;

	public static GameObject deathProjectile;

	public override string EliteName => "Aragonite";

	public override Color EliteColor => scriptableObject.eliteColor;

	public override EliteTierDefinition MainEliteTierDefinition => scriptableObject.tier switch
	{
		SSEliteBaseSO.EliteTierType.TierTwo => EliteBase<RagingElite>.tierTwoEliteDefault, 
		SSEliteBaseSO.EliteTierType.Honor => EliteBase<RagingElite>.honorEliteDefault, 
		_ => EliteBase<RagingElite>.tierOneEliteDefault, 
	};

	public override EliteTierDefinition[] ExtraEliteTierDefitions
	{
		get
		{
			if (scriptableObject.tier == SSEliteBaseSO.EliteTierType.TierOne)
			{
				return new EliteTierDefinition[1] { EliteBase<RagingElite>.honorEliteDefault };
			}
			return new EliteTierDefinition[0];
		}
	}

	public override Sprite AffixBuffSprite => scriptableObject.buffSprite;

	public override Sprite EquipmentIcon => scriptableObject.EquipmentIcon;

	public override string EquipmentName => scriptableObject.EquipmentName;

	public override string AffixDescriptionMainWord => scriptableObject.AffixDescriptionMainWord;

	public override Texture2D EliteRampTexture => scriptableObject.EliteRampTexture;

	public override Type AffixBuffBehaviour => typeof(RageAffixBuffBehaviourServer);

	public override bool ServerOnlyAffixBuffBehaviour => false;

	public override void Init()
	{
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Expected O, but got Unknown
		PlasmaCorePlugin.eliteBaseScriptableObjects.TryGetValue(EliteName, out scriptableObject);
		RecalculateStatsAPI.GetStatCoefficients += new RecalculateStatsAPI.StatHookEventHandler(RecalculateStatsAPI_GetStatCoefficients);
		PlasmaUtils.RegisterNetworkPrefab(scriptableObject.MiscGameObjects[0]);
		ContentAddition.AddBuffDef(scriptableObject.MiscBuffs[0]);
		scriptableObject.MiscBuffs[0].iconSprite = SpikestripUtil.AddressablesLoad<BuffDef>("aabb5fce0f91514429bfa91cb2f790da").iconSprite;
		ContentAddition.AddBuffDef(scriptableObject.MiscBuffs[1]);
		deathProjectile = scriptableObject.MiscGameObjects[1];
		PlasmaUtils.RegisterNetworkPrefab(deathProjectile);
		ContentAddition.AddProjectile(deathProjectile);
		ContentAddition.AddEffect(scriptableObject.MiscGameObjects[2]);
		OverlayMaterials.Register(scriptableObject.OverlayMaterial, (CharacterModel self) => self.body.HasBuff(scriptableObject.MiscBuffs[0]) || self.body.HasBuff(AffixBuff));
		base.Init();
		EquipmentDef.dropOnDeathChance = 0.00025f;
	}

	public override void AssignEquipmentValues()
	{
		EquipmentPickupModel = CreateAffixModel(scriptableObject.affixColor);
		EquipmentDisplayModel = scriptableObject.EquipmentDisplayModel;
	}

	public override void PopulateItemDisplayRules(ItemDisplayRuleDict itemDisplayRuleDict)
	{
		RegisterDisplayParent(LegacyResourcesAPI.Load<EquipmentDef>("EquipmentDefs/AffixHaunted"), EquipmentDisplayModel);
	}

	private void RecalculateStatsAPI_GetStatCoefficients(CharacterBody sender, RecalculateStatsAPI.StatHookEventArgs args)
	{
		if (sender.HasBuff(AffixBuff))
		{
			if (sender.HasBuff(scriptableObject.MiscBuffs[0]))
			{
				sender.RemoveBuff(scriptableObject.MiscBuffs[0]);
			}
			args.attackSpeedMultAdd += 0.5f;
			args.moveSpeedMultAdd += 0.5f;
			args.cooldownMultAdd -= 0.5f;
		}
		else if (sender.HasBuff(scriptableObject.MiscBuffs[0]))
		{
			args.attackSpeedMultAdd += 0.5f;
			args.moveSpeedMultAdd += 0.5f;
			args.cooldownMultAdd -= 0.5f;
		}
	}
}
