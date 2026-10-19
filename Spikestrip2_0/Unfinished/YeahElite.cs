using System.Collections.Generic;
using System.Linq;
using On.RoR2;
using R2API;
using RoR2;
using UnityEngine;
using UnityEngine.Networking;

namespace GrooveSaladSpikestripContent.Content;

public class YeahElite : SpikestripEliteBase<YeahElite>
{
	[RequireComponent(typeof(RoR2.TeamFilter))]
	public class YeahElitePulseController : MonoBehaviour
	{
		private RoR2.TeamFilter teamFilter;

		private float pulseTimer;

		public float interval;

		public float buffDuration;

		public float radius;

		public BuffIndex buffIndex;

		private void Awake()
		{
			teamFilter = GetComponent<RoR2.TeamFilter>();
		}

		private void FixedUpdate()
		{
			if (NetworkServer.active)
			{
				pulseTimer += Time.fixedDeltaTime;
				if (pulseTimer >= interval)
				{
					pulseTimer -= interval;
					GameObject gameObject = Object.Instantiate(yeahPulsePrefab, base.transform.position, base.transform.rotation);
					RoR2.PulseController component = gameObject.GetComponent<RoR2.PulseController>();
					component.performSearch += PerformSearch;
					component.onPulseHit += OnPulseHit;
					component.StartPulseServer();
					NetworkServer.Spawn(gameObject);
				}
			}
		}

		public void PerformSearch(RoR2.PulseController pulseController, Vector3 origin, float radius, List<RoR2.PulseController.PulseSearchResult> dest)
		{
			foreach (RoR2.TeamComponent teamMember in RoR2.TeamComponent.GetTeamMembers(teamFilter.teamIndex))
			{
				if ((teamMember.transform.position - origin).sqrMagnitude <= radius * radius)
				{
					RoR2.CharacterBody component = teamMember.GetComponent<RoR2.CharacterBody>();
					if ((bool)component)
					{
						dest.Add(new RoR2.PulseController.PulseSearchResult
						{
							hitObject = component,
							hitPos = teamMember.transform.position
						});
					}
				}
			}
		}

		public void OnPulseHit(RoR2.PulseController pulseController, RoR2.PulseController.PulseHit hitInfo)
		{
			RoR2.CharacterBody characterBody = (RoR2.CharacterBody)hitInfo.hitObject;
			if ((bool)characterBody)
			{
				characterBody.AddTimedBuff(buffIndex, buffDuration);
			}
		}
	}

	public static BuffIndex[] availableBuffIndices;

	public static GameObject yeahDeathWardPrefab;

	public static GameObject yeahPulsePrefab;

	public override string EliteName => "Yeah";

	public override Color EliteColor => SpikestripContentBase.ColorRGB(255f, 255f, 255f);

	public override EliteTierDefinition MainEliteTierDefinition => SpikestripEliteBase<YeahElite>.tierOneEliteDefault;

	public override EliteTierDefinition[] ExtraEliteTierDefitions => new EliteTierDefinition[1] { SpikestripEliteBase<YeahElite>.honorEliteDefault };

	public override Sprite AffixBuffSprite => Base.GroovyAssetBundle.LoadAsset<Sprite>("texBuffAffixGravity.png");

	public override Sprite EquipmentIcon => Base.GroovyAssetBundle.LoadAsset<Sprite>("texAffixGravityIcon.png");

	public override string EquipmentName => "Yeahhhhhhhhh";

	public override string AffixDescriptionMainWord => "yeah";

	public override Texture2D EliteRampTexture => Base.GroovyAssetBundle.LoadAsset<Texture2D>("texRampEliteYeah.png");

	public override bool HookOnHitEnemy => false;

	public override bool IsEnabled => false;

	public bool BuffFilter(RoR2.BuffDef buffDef)
	{
		return buffDef.isDebuff && !buffDef.isCooldown && !buffDef.canStack && !buffDef.isHidden;
	}

	public override void Init()
	{
		base.Init();
		On.RoR2.DotController.InitDotCatalog += DotController_InitDotCatalog;
		RoR2.GlobalEventManager.onCharacterDeathGlobal += GlobalEventManager_onCharacterDeathGlobal;
		yeahDeathWardPrefab = PrefabAPI.InstantiateClone(SpikestripContentBase.LegacyLoad<GameObject>("Prefabs/NetworkedObjects/WarbannerWard"), "YeahEliteDeathWard", true);
		SpikestripContentBase.DestroyImmediate(yeahDeathWardPrefab.GetComponent<RoR2.BuffWard>());
		YeahElitePulseController yeahElitePulseController = yeahDeathWardPrefab.AddComponent<YeahElitePulseController>();
		yeahElitePulseController.radius = 15f;
		yeahElitePulseController.interval = 1f;
		yeahElitePulseController.buffDuration = 2f;
		SpikestripContentBase.networkedObjectContent.Add(yeahDeathWardPrefab);
		yeahPulsePrefab = PrefabAPI.InstantiateClone(SpikestripContentBase.LegacyLoad<GameObject>("Prefabs/NetworkedObjects/MoonBatteryDesignPulse"), "YeahElitePulse", true);
		SpikestripContentBase.networkedObjectContent.Add(yeahPulsePrefab);
	}

	private void GlobalEventManager_onCharacterDeathGlobal(RoR2.DamageReport damageReport)
	{
		RoR2.CharacterBody characterBody = (damageReport.victim ? damageReport.victim.GetComponent<RoR2.CharacterBody>() : null);
		if ((bool)characterBody && characterBody.HasBuff(AffixBuff))
		{
			GameObject gameObject = Object.Instantiate(yeahDeathWardPrefab, characterBody.corePosition, Quaternion.identity);
			YeahElitePulseController component = gameObject.GetComponent<YeahElitePulseController>();
			component.buffIndex = availableBuffIndices[Random.Range(0, availableBuffIndices.Length)];
			RoR2.TeamFilter component2 = gameObject.GetComponent<RoR2.TeamFilter>();
			component2.teamIndex = damageReport.victimTeamIndex;
			NetworkServer.Spawn(gameObject);
		}
	}

	private void DotController_InitDotCatalog(On.RoR2.DotController.orig_InitDotCatalog orig)
	{
		orig();
		List<BuffIndex> associatedWithDebuffs = new List<BuffIndex>();
		for (int i = 0; i < RoR2.DotController.dotDefs.Length; i++)
		{
			RoR2.BuffDef associatedBuff = RoR2.DotController.dotDefs[i].associatedBuff;
			if ((bool)associatedBuff)
			{
				associatedWithDebuffs.Add(associatedBuff.buffIndex);
			}
		}
		availableBuffIndices = (from buffDef in RoR2.BuffCatalog.buffDefs
			where !buffDef.isDebuff && !buffDef.isCooldown && !buffDef.isHidden && !buffDef.eliteDef && !associatedWithDebuffs.Contains(buffDef.buffIndex)
			select buffDef.buffIndex).ToArray();
	}

	public override void AssignEquipmentValues()
	{
		EquipmentPickupModel = CreateAffixModel(SpikestripContentBase.ColorRGB(178f, 98f, 200f));
		EquipmentDisplayModel = SpikestripContentBase.nullModel;
	}

	public override void PopulateItemDisplayRules(ItemDisplayRuleDict itemDisplayRuleDict)
	{
	}
}
