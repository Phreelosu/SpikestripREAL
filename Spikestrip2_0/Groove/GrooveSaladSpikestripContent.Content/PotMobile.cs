using System.Collections.Generic;
using R2API;
using RoR2;
using RoR2.Navigation;
using UnityEngine;
using UnityEngine.Networking;
using Spikestrip2_0.Core;

namespace GrooveSaladSpikestripContent.Content;

public class PotMobile : MonsterBase<PotMobile>
{
	//all of this is literally based on moff's potmobile enemy
	public class EnableJetsFromSpeed : MonoBehaviour
	{
		public float threshold = 5f;

		public Transform targetTransform;

		public Rigidbody rb;

		public void Start()
		{
			rb = GetComponent<Rigidbody>();
		}

		public void FixedUpdate()
		{
			if ((bool)rb && rb.velocity.magnitude >= threshold && (bool)targetTransform)
			{
				if (!targetTransform.gameObject.activeSelf)
				{
					targetTransform.gameObject.SetActive(value: true);
				}
			}
			else if ((bool)targetTransform && targetTransform.gameObject.activeSelf)
			{
				targetTransform.gameObject.SetActive(value: false);
			}
		}
	}

	public class InstantiatePotMobileDamageTrail : MonoBehaviour
	{
		public PotMobileDamageTrail trailInstance;

		public Vector3 positionOffset = new Vector3(0f, -1f, 0f);

		private void Start()
		{
			trailInstance = Object.Instantiate(GooTrailPrefab, base.transform.position + positionOffset, Quaternion.identity).GetComponent<PotMobileDamageTrail>();
			CharacterBody component = GetComponent<CharacterBody>();
			trailInstance.damagePerSecond = 3f * component.damage;
			trailInstance.owner = base.gameObject;
		}

		private void FixedUpdate()
		{
			if ((bool)trailInstance)
			{
				trailInstance.transform.position = base.transform.position + positionOffset;
			}
		}

		private void OnDestroy()
		{
			if ((bool)trailInstance)
			{
				trailInstance.active = false;
			}
			trailInstance = null;
		}
	}

	public class PotMobileDamageTrail : MonoBehaviour
	{
		[Tooltip("How often to drop a new point onto the trail.")]
		public float pointUpdateInterval = 0.2f;

		[Tooltip("How often the damage trail should deal damage.")]
		public float damageUpdateInterval = 0.2f;

		[Tooltip("How large the radius, or width, of the damage detection should be.")]
		public float radius = 0.5f;

		[Tooltip("How large the height of the damage detection should be.")]
		public float height = 0.5f;

		[Tooltip("How long a point on the trail should last.")]
		public float pointLifetime = 3f;

		[Tooltip("The line renderer to use for display.")]
		public LineRenderer lineRenderer;

		public bool active = true;

		[Tooltip("Prefab to use per segment.")]
		public GameObject segmentPrefab;

		public bool destroyTrailSegments;

		public float damagePerSecond;

		public GameObject owner;

		public CharacterMotor motor;

		public List<DamageTrail.TrailPoint> pointsList;

		private float localTime;

		private float nextTrailPointUpdate;

		private float nextTrailDamageUpdate;

		private Vector3 lastGroundedPosition;

		private TeamIndex attackerTeamIndex = TeamIndex.Neutral;

		private void Awake()
		{
			pointsList = new List<DamageTrail.TrailPoint>();
		}

		private void Start()
		{
			motor = owner.GetComponent<CharacterMotor>();
			localTime = 0f;
		}

		private void FixedUpdate()
		{
			RaycastHit hitInfo;
			if ((bool)(Object)(object)motor && motor.isGrounded)
			{
				lastGroundedPosition = base.transform.position;
			}
			else if (Physics.Raycast(base.transform.position, Vector3.down, out hitInfo, 500f, LayerIndex.world.mask))
			{
				lastGroundedPosition = hitInfo.point;
			}
			localTime += Time.fixedDeltaTime;
			if (localTime >= nextTrailPointUpdate)
			{
				nextTrailPointUpdate += pointUpdateInterval;
				UpdateTrail(active);
			}
			if (localTime >= nextTrailDamageUpdate)
			{
				nextTrailDamageUpdate += damageUpdateInterval;
				DoDamage();
			}
			if (pointsList.Count > 0)
			{
				DamageTrail.TrailPoint value = pointsList[pointsList.Count - 1];
				value.position = lastGroundedPosition;
				if (active)
				{
					value.localEndTime = localTime + pointLifetime;
				}
				pointsList[pointsList.Count - 1] = value;
				if ((bool)value.segmentTransform)
				{
					value.segmentTransform.position = lastGroundedPosition;
				}
				if ((bool)lineRenderer)
				{
					lineRenderer.SetPosition(pointsList.Count - 1, value.position);
				}
				Vector3 position = lastGroundedPosition;
				for (int num = pointsList.Count - 1; num >= 0; num--)
				{
					Transform segmentTransform = pointsList[num].segmentTransform;
					segmentTransform.LookAt(position, Vector3.up);
					Vector3 vector = pointsList[num].position - position;
					segmentTransform.position = position + vector * 0.5f;
					float num2 = Mathf.Clamp01(Mathf.InverseLerp(pointsList[num].localStartTime, pointsList[num].localEndTime, localTime));
					Vector3 localScale = new Vector3(radius * (1f - num2), radius * (1f - num2), vector.magnitude);
					segmentTransform.localScale = localScale;
					position = pointsList[num].position;
				}
			}
			else if (!active)
			{
				Object.Destroy(base.gameObject);
			}
		}

		private void UpdateTrail(bool addPoint)
		{
			while (pointsList.Count > 0 && pointsList[0].localEndTime <= localTime)
			{
				RemovePoint(0);
			}
			if (addPoint)
			{
				AddPoint();
			}
			if ((bool)lineRenderer)
			{
				UpdateLineRenderer(lineRenderer);
			}
		}

		private void DoDamage()
		{
			if (!NetworkServer.active || pointsList.Count == 0)
			{
				return;
			}
			Vector3 vector = pointsList[pointsList.Count - 1].position;
			HashSet<GameObject> hashSet = new HashSet<GameObject>();
			if ((bool)owner)
			{
				hashSet.Add(owner);
				attackerTeamIndex = TeamComponent.GetObjectTeam(owner);
			}
			float damage = damagePerSecond * damageUpdateInterval;
			DamageInfo damageInfo = new DamageInfo();
			damageInfo.attacker = owner;
			damageInfo.inflictor = base.gameObject;
			damageInfo.crit = false;
			damageInfo.damage = damage;
			damageInfo.damageColorIndex = DamageColorIndex.Default;
			damageInfo.damageType = DamageType.ClayGoo;
			damageInfo.force = Vector3.zero;
			damageInfo.procCoefficient = 0f;
			for (int num = pointsList.Count - 2; num >= 0; num--)
			{
				Vector3 position = pointsList[num].position;
				Vector3 forward = position - vector;
				Vector3 halfExtents = new Vector3(radius, height, forward.magnitude);
				Vector3 center = Vector3.Lerp(position, vector, 0.5f);
				Quaternion orientation = Util.QuaternionSafeLookRotation(forward);
				Collider[] array = Physics.OverlapBox(center, halfExtents, orientation, LayerIndex.entityPrecise.mask, QueryTriggerInteraction.UseGlobal);
				for (int i = 0; i < array.Length; i++)
				{
					HurtBox component = array[i].GetComponent<HurtBox>();
					if (!component)
					{
						continue;
					}
					HealthComponent healthComponent = component.healthComponent;
					if (!healthComponent)
					{
						continue;
					}
					GameObject item = healthComponent.gameObject;
					if (!hashSet.Contains(item))
					{
						hashSet.Add(item);
						if (FriendlyFireManager.ShouldSplashHitProceed(healthComponent, attackerTeamIndex))
						{
							damageInfo.position = array[i].transform.position;
							healthComponent.TakeDamage(damageInfo);
						}
					}
				}
				vector = position;
			}
		}

		private void UpdateLineRenderer(LineRenderer lineRenderer)
		{
			lineRenderer.positionCount = pointsList.Count;
			for (int i = 0; i < pointsList.Count; i++)
			{
				lineRenderer.SetPosition(i, pointsList[i].position);
			}
		}

		private void AddPoint()
		{
			DamageTrail.TrailPoint item = new DamageTrail.TrailPoint
			{
				position = lastGroundedPosition,
				localStartTime = localTime,
				localEndTime = localTime + pointLifetime
			};
			if ((bool)segmentPrefab)
			{
				item.segmentTransform = Object.Instantiate(segmentPrefab, base.transform).transform;
			}
			pointsList.Add(item);
		}

		private void RemovePoint(int pointIndex)
		{
			if (destroyTrailSegments && (bool)pointsList[pointIndex].segmentTransform)
			{
				Object.Destroy(pointsList[pointIndex].segmentTransform.gameObject);
			}
			pointsList.RemoveAt(pointIndex);
		}

		private void OnDrawGizmos()
		{
			Vector3 vector = pointsList[pointsList.Count - 1].position;
			for (int num = pointsList.Count - 2; num >= 0; num--)
			{
				Vector3 position = pointsList[num].position;
				Vector3 forward = position - vector;
				Vector3 s = new Vector3(radius, 0.5f, forward.magnitude);
				Vector3 pos = Vector3.Lerp(position, vector, 0.5f);
				Quaternion q = Util.QuaternionSafeLookRotation(forward);
				Gizmos.matrix = Matrix4x4.TRS(pos, q, s);
				Gizmos.color = Color.blue;
				Gizmos.DrawWireCube(Vector3.zero, Vector3.one);
				Gizmos.matrix = Matrix4x4.identity;
				vector = position;
			}
		}
	}

	public static Material matGooTrail = SpikestripUtil.AddressablesLoad<Material>("2080ca5a40357224fb5c6b35d1e5b0e7");

	public static GameObject GooTrailSegment;

	public static GameObject GooTrailPrefab;

	public override string MonsterName => "Lively Pot";

	public override void ModifySpawnCard()
	{
		base.ModifySpawnCard();
		CharacterSpawnCard.directorCreditCost = 15;
		CharacterSpawnCard.eliteRules = SpawnCard.EliteRules.Default;
		CharacterSpawnCard.forbiddenAsBoss = false;
		CharacterSpawnCard.hullSize = HullClassification.Human;
		CharacterSpawnCard.nodeGraphType = MapNodeGroup.GraphType.Ground;
		CharacterSpawnCard.noElites = false;
		CharacterSpawnCard.occupyPosition = false;
	}

	public override void ModifyDirectorCardAndAddInteractable()
	{
		base.ModifyDirectorCardAndAddInteractable();
		DirectorCard.selectionWeight = 1;
		DirectorCard.preventOverhead = false;
		DirectorCard.spawnDistance = DirectorCore.MonsterSpawnDistance.Standard;
		AddMonster("Basic Monsters", "goolake");
		AddMonster("Basic Monsters", "wispgraveyard");
		AddMonster("Basic Monsters", "arena");
		AddMonster("Basic Monsters", "ancientloft");
		AddMonster("Basic Monsters", CommonStages.Simulacrum("goolake"));
		AddMonster("Basic Monsters", CommonStages.Simulacrum("ancientloft"));
		// GrooveUnsharedUtils' 1.1.0 changes: Dry Basin spawns, smaller body, quieter loop, weaker trail.
		AddMonster("Basic Monsters", "drybasin");
		RoR2.SfxLocator sfxLocator = BodyPrefab.GetComponent<RoR2.SfxLocator>();
		sfxLocator.aliveLoopStart = "Play_clayboss_walk_loop";
		sfxLocator.aliveLoopStop = "Stop_clayboss_walk_loop";
		BodyPrefab.GetComponent<RoR2.ModelLocator>().modelTransform.localScale = Vector3.one * 0.55f;
		GooTrailPrefab.GetComponent<PotMobileDamageTrail>().damagePerSecond = 1f;
	}

	public override void SetHooks()
	{
	}

	public override void AssignPrefab()
	{
		GooTrailSegment = PrefabAPI.InstantiateClone(LegacyResourcesAPI.Load<GameObject>("prefabs/FireTrail").GetComponent<DamageTrail>().segmentPrefab, "GooTrailSegment", false);
		ParticleSystemRenderer component = GooTrailSegment.GetComponent<ParticleSystemRenderer>();
		component.material = Object.Instantiate(matGooTrail);
		component.material.SetVector("_CutoffScroll", Vector4.zero);
		component.minParticleSize = 0f;
		component.maxParticleSize = 3f;
		ParticleSystem component2 = GooTrailSegment.GetComponent<ParticleSystem>();
		ParticleSystem.MainModule main = component2.main;
		main.simulationSpeed /= 3f;
		GooTrailSegment.GetComponent<DestroyOnTimer>().duration *= 3f;
		GooTrailPrefab = SpikestripUtil.EmptyPrefab("PotMobileGooTrail");
		PotMobileDamageTrail potMobileDamageTrail = GooTrailPrefab.AddComponent<PotMobileDamageTrail>();
		potMobileDamageTrail.damagePerSecond = 1.5f;
		potMobileDamageTrail.damageUpdateInterval = 0.2f;
		potMobileDamageTrail.destroyTrailSegments = false;
		potMobileDamageTrail.height = 0.5f;
		potMobileDamageTrail.lineRenderer = null;
		potMobileDamageTrail.pointLifetime = 9f;
		potMobileDamageTrail.pointUpdateInterval = 0.5f;
		potMobileDamageTrail.radius = 2f;
		potMobileDamageTrail.segmentPrefab = GooTrailSegment;
		GameObject gameObject = LegacyResourcesAPI.Load<GameObject>("prefabs/characterbodies/MageBody").GetComponent<ModelLocator>().modelTransform.Find("MageArmature/ROOT/base/stomach/chest/JetsOn").gameObject;
		GameObject gameObject2 = PrefabAPI.InstantiateClone(gameObject, "PotMobileJetsOn", false);
		GameObject gameObject3 = PrefabAPI.InstantiateClone(LegacyResourcesAPI.Load<GameObject>("prefabs/temporaryvisualeffects/CrippleEffect").transform.Find("Visual/Drips").gameObject, "PotMobileDrip", false);
		GameObject potMobileModel = LegacyResourcesAPI.Load<GameObject>("prefabs/characterbodies/PotMobile2Body").transform.Find("Model Base/mdlPotMobile").gameObject;
		GameObject gameObject4 = PrefabAPI.InstantiateClone(potMobileModel, "mdlPotMobileMonster", false);
		SpikestripUtil.BakeDefaultSkin(potMobileModel, gameObject4);
		UnityEngine.Object.DestroyImmediate(gameObject4.transform.Find("FrontWheel.L").gameObject);
		UnityEngine.Object.DestroyImmediate(gameObject4.transform.Find("FrontWheel.R").gameObject);
		UnityEngine.Object.DestroyImmediate(gameObject4.transform.Find("BackWheel.L").gameObject);
		UnityEngine.Object.DestroyImmediate(gameObject4.transform.Find("BackWheel.R").gameObject);
		UnityEngine.Object.DestroyImmediate(gameObject4.transform.Find("Collider").gameObject);
		Animator animator = gameObject4.AddComponent<Animator>();
		DestroyOnUnseen destroyOnUnseen = gameObject4.AddComponent<DestroyOnUnseen>();
		Transform transform = gameObject4.transform.Find("PotMobileBaseMesh");
		transform.Rotate(Vector3.back, 180f);
		transform.localPosition = new Vector3(0f, 0.3f, 0f);
		Transform transform2 = gameObject4.transform.Find("Hurtbox");
		transform2.localPosition = new Vector3(0f, 1.1f, 0f);
		CharacterModel component3 = gameObject4.GetComponent<CharacterModel>();
		component3.baseRendererInfos = SpikestripUtil.GenerateRendererInfos(gameObject4);
		component3.itemDisplayRuleSet = PotMobileIDRS.GetIDRS();
		ChildLocator component4 = gameObject4.GetComponent<ChildLocator>();
		component4.transformPairs = new ChildLocator.NameTransformPair[1]
		{
			new ChildLocator.NameTransformPair
			{
				name = "Base",
				transform = transform
			}
		};
		gameObject2.transform.SetParent(transform);
		Transform transform3 = gameObject2.transform;
		transform3.localPosition = new Vector3(0f, -3.1f, 0.6f);
		transform3.localRotation = Util.QuaternionSafeLookRotation(Vector3.down);
		transform3.localScale = new Vector3(0.2f, 0.2f, 2f);
		Transform transform4 = transform3.Find("JetsL");
		transform4.localRotation = Util.QuaternionSafeLookRotation(Vector3.forward);
		transform4.localPosition = new Vector3(6f, 0f, 0f);
		Transform transform5 = transform3.Find("JetsR");
		transform5.localRotation = Util.QuaternionSafeLookRotation(Vector3.forward);
		transform5.localPosition = new Vector3(-6f, 0f, 0f);
		gameObject4.transform.localScale = Vector3.one * 0.4f;
		gameObject3.transform.SetParent(gameObject4.transform);
		gameObject3.transform.localPosition = Vector3.zero;
		ParticleSystemRenderer component5 = gameObject3.GetComponent<ParticleSystemRenderer>();
		component5.mesh = gameObject4.GetComponentInChildren<MeshFilter>().mesh;
		component5.material = matGooTrail;
		BodyPrefab = PrefabAPI.InstantiateClone(LegacyResourcesAPI.Load<GameObject>("prefabs/characterbodies/LemurianBody"), "PotMobileMonsterBody", true);
		BodyPrefab.transform.Find("CameraPivot").localPosition = Vector3.zero;
		InstantiatePotMobileDamageTrail instantiatePotMobileDamageTrail = BodyPrefab.AddComponent<InstantiatePotMobileDamageTrail>();
		HurtBox component6 = transform2.GetComponent<HurtBox>();
		component6.healthComponent = BodyPrefab.GetComponent<HealthComponent>();
		component6.isSniperTarget = true;
		component6.isBullseye = true;
		CharacterBody component7 = BodyPrefab.GetComponent<CharacterBody>();
		component7.baseNameToken = "POTMOBILEMONSTER_BODY_NAME";
		LanguageAPI.Add(component7.baseNameToken, MonsterName);
		component7.bodyFlags = CharacterBody.BodyFlags.ImmuneToGoo;
		component7.baseAcceleration = 10f;
		component7.baseMoveSpeed = 18f;
		component7.baseMaxHealth = 120f;
		component7.levelMaxHealth = 36f;
		component7.portraitIcon = SpikestripContent.GrooveBundle.LoadAsset<Texture>("PotMobileBody.png");
		component3.body = component7;
		TemporaryOverlay temporaryOverlay = gameObject4.AddComponent<TemporaryOverlay>();
		temporaryOverlay.originalMaterial = LegacyResourcesAPI.Load<Material>("materials/matClayGooDebuff");
		temporaryOverlay.animateShaderAlpha = true;
		temporaryOverlay.alphaCurve = AnimationCurve.EaseInOut(0f, 0.7f, 1f, 0.1f);
		temporaryOverlay.duration = 4f;
		temporaryOverlay.destroyComponentOnEnd = false;
		temporaryOverlay.inspectorCharacterModel = component3;
		CapsuleCollider component8 = BodyPrefab.GetComponent<CapsuleCollider>();
		component8.height = 1f;
		component8.radius = 1f;
		SfxLocator component9 = BodyPrefab.GetComponent<SfxLocator>();
		component9.aliveLoopStart = "Play_clayboss_R_mulch_loop";
		component9.aliveLoopStop = "Stop_clayboss_R_mulch_loop";
		component9.barkSound = "Play_clayBruiser_attack2_shoot";
		ModelLocator component10 = BodyPrefab.GetComponent<ModelLocator>();
		Transform modelTransform = component10.modelTransform;
		UnityEngine.Object.DestroyImmediate(modelTransform.gameObject);
		gameObject4.transform.SetParent(component10.modelBaseTransform);
		component10.modelTransform = gameObject4.transform;
		EnableJetsFromSpeed enableJetsFromSpeed = BodyPrefab.AddComponent<EnableJetsFromSpeed>();
		enableJetsFromSpeed.targetTransform = gameObject2.transform;
		PotMobileSkills.RegisterSkills(BodyPrefab);
		UnlockableDef unlockableDef = ScriptableObject.CreateInstance<UnlockableDef>();
		unlockableDef.cachedName = "Logs.PotMobileMonsterBody.0";
		unlockableDef.displayModelPrefab = SpikestripUtil.nullModel;
		unlockableDef.nameToken = "UNLOCKABLE_LOG_POTMOBILEMONSTER";
		unlockableDef.hidden = false;
		ContentAddition.AddUnlockableDef(unlockableDef);
		LanguageAPI.Add(unlockableDef.nameToken, "Monster Log: " + MonsterName);
		DeathRewards component11 = BodyPrefab.GetComponent<DeathRewards>();
		component11.logUnlockableDef = unlockableDef;
		GameObject gameObject5 = new GameObject("HitBox");
		gameObject5.transform.SetParent(transform2);
		gameObject5.transform.localPosition = Vector3.zero;
		gameObject5.transform.localScale = Vector3.one * 4f;
		HitBox hitBox = gameObject5.AddComponent<HitBox>();
		HitBoxGroup hitBoxGroup = gameObject4.AddComponent<HitBoxGroup>();
		hitBoxGroup.groupName = "Contact";
		hitBoxGroup.hitBoxes = new HitBox[1] { hitBox };
		ContactDamage contactDamage = BodyPrefab.AddComponent<ContactDamage>();
		contactDamage.damageInterval = 0.25f;
		contactDamage.damagePerSecondCoefficient = 3f;
		contactDamage.damageType = DamageType.Generic;
		contactDamage.doBlastAttackInstead = false;
		contactDamage.pushForcePerSecond = 4000f;
		contactDamage.hitBoxGroup = hitBoxGroup;
		SetStateOnHurt component12 = BodyPrefab.GetComponent<SetStateOnHurt>();
		MasterPrefab = PrefabAPI.InstantiateClone(LegacyResourcesAPI.Load<GameObject>("prefabs/charactermasters/LemurianMaster"), "PotMobileMonsterMaster", true);
		CharacterMaster component13 = MasterPrefab.GetComponent<CharacterMaster>();
		component13.bodyPrefab = BodyPrefab;
		PotMobileAI.CreateAI(MasterPrefab);
	}
}
