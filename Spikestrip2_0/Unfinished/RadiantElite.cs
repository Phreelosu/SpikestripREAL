using System;
using System.Collections.Generic;
using System.Linq;
using On.RoR2;
using R2API;
using RoR2;
using RoR2.Projectile;
using UnityEngine;
using UnityEngine.Networking;

namespace GrooveSaladSpikestripContent.Content;

public class RadiantElite : SpikestripEliteBase<RadiantElite>
{
	public class RadiantBeamController : MonoBehaviour
	{
		private LineRenderer lineRenderer;

		private Rigidbody rb;

		private Ray ray;

		public Transform sunTransform;

		public RoR2.CharacterBody body;

		private Transform target;

		public void Start()
		{
			lineRenderer = GetComponent<LineRenderer>();
			lineRenderer.enabled = false;
			rb = GetComponent<Rigidbody>();
			ray = default(Ray);
		}

		public void FixedUpdate()
		{
			UpdateBeam();
		}

		public void UpdateBeam()
		{
			if (!target)
			{
				if ((bool)rb)
				{
					rb.velocity = Vector3.zero;
				}
			}
			else
			{
				if (!sunTransform || !body)
				{
					return;
				}
				Vector3 position = sunTransform.position;
				if ((bool)lineRenderer)
				{
					lineRenderer.SetPosition(0, position);
				}
				if (!rb || !body)
				{
					return;
				}
				Vector3 vector = target.position - rb.position;
				rb.AddForce(vector * 0.2f, ForceMode.VelocityChange);
				Vector3 direction = rb.position - position;
				ray.origin = position;
				ray.direction = direction;
				Vector3 point = ray.GetPoint(200f);
				if (Physics.SphereCast(ray, 1f, out var hitInfo, 200f, (int)RoR2.LayerIndex.entityPrecise.mask | (int)RoR2.LayerIndex.world.mask))
				{
					point = hitInfo.point;
					RoR2.HurtBox hurtBox = (hitInfo.collider ? hitInfo.collider.GetComponent<RoR2.HurtBox>() : null);
					if ((bool)hurtBox && NetworkServer.active)
					{
						RoR2.HealthComponent healthComponent = hurtBox.healthComponent;
						if ((bool)healthComponent)
						{
							BeamImmunity component = healthComponent.GetComponent<BeamImmunity>();
							if ((!component || ((bool)component && component.attacker != this)) && RoR2.FriendlyFireManager.ShouldDirectHitProceed(healthComponent, body.teamComponent.teamIndex))
							{
								RoR2.DamageInfo damageInfo = new RoR2.DamageInfo();
								damageInfo.attacker = body.gameObject;
								damageInfo.inflictor = body.gameObject;
								damageInfo.damage = body.damage * 1f;
								damageInfo.crit = body.RollCrit();
								damageInfo.force = Vector3.zero;
								damageInfo.procCoefficient = 1f;
								damageInfo.damageType = DamageType.Generic;
								damageInfo.damageColorIndex = DamageColorIndex.Default;
								damageInfo.position = point;
								healthComponent.TakeDamage(damageInfo);
								RoR2.GlobalEventManager.instance.OnHitEnemy(damageInfo, healthComponent.gameObject);
								RoR2.GlobalEventManager.instance.OnHitAll(damageInfo, healthComponent.gameObject);
								component = healthComponent.gameObject.AddComponent<BeamImmunity>();
								component.timer = 0.5f;
								component.attacker = this;
							}
						}
					}
				}
				if ((bool)lineRenderer)
				{
					lineRenderer.SetPosition(1, point);
				}
			}
		}

		public void SetNewTarget(Transform newTarget)
		{
			target = newTarget;
			lineRenderer.enabled = newTarget;
		}
	}

	public class BeamImmunity : MonoBehaviour
	{
		public float timer;

		public RadiantBeamController attacker;

		public void FixedUpdate()
		{
			timer -= Time.fixedDeltaTime;
			if (timer <= 0f)
			{
				UnityEngine.Object.Destroy(this);
			}
		}
	}

	public class RadiantAffixBuffBehaviour : RoR2.CharacterBody.ItemBehavior
	{
		public static float missileFireInterval = 0.25f;

		private float missileTimer;

		public List<float> missileQueueServer = new List<float>();

		public static int beamCount = 3;

		private GameObject radiantOrbInstance;

		private GameObject radiantBeamInstance;

		private List<GameObject> beamObjects = new List<GameObject>();

		private RoR2.BullseyeSearch search = new RoR2.BullseyeSearch();

		private float targetUpdateStopwatch;

		public static float targetUpdateInterval = 0.2f;

		public Vector3 orbPosition => body.corePosition + Vector3.up * (body.bestFitRadius + 10f);

		public void Start()
		{
			radiantOrbInstance = UnityEngine.Object.Instantiate(radiantOrb, orbPosition, Quaternion.identity);
			radiantOrbInstance.SetActive(value: true);
			radiantBeamInstance = UnityEngine.Object.Instantiate(radiantBeam, body.corePosition, RoR2.Util.QuaternionSafeLookRotation(new Vector3(0f, 0f, 1f)));
			radiantBeamInstance.transform.RotateAround(base.transform.position, Vector3.forward, 180f);
			radiantBeamInstance.SetActive(value: true);
			for (int i = 0; i < beamCount; i++)
			{
				GameObject gameObject = UnityEngine.Object.Instantiate(radiantBeamControllerPrefab, body.corePosition + UnityEngine.Random.insideUnitSphere * 10f, Quaternion.identity);
				RadiantBeamController component = gameObject.GetComponent<RadiantBeamController>();
				component.sunTransform = radiantOrbInstance.transform;
				component.body = body;
				beamObjects.Add(gameObject);
			}
		}

		public void OnDestroy()
		{
			if ((bool)radiantOrbInstance)
			{
				UnityEngine.Object.Destroy(radiantOrbInstance);
			}
			if ((bool)radiantBeamInstance)
			{
				UnityEngine.Object.Destroy(radiantBeamInstance);
			}
			for (int i = 0; i < beamObjects.Count; i++)
			{
				if ((bool)beamObjects[i])
				{
					UnityEngine.Object.Destroy(beamObjects[i]);
				}
			}
		}

		public void FixedUpdate()
		{
			if ((bool)radiantOrbInstance)
			{
				radiantOrbInstance.transform.position = orbPosition;
			}
			if ((bool)radiantBeamInstance)
			{
				radiantBeamInstance.transform.position = body.corePosition;
			}
			if (NetworkServer.active)
			{
				UpdateMissilesServer();
			}
			targetUpdateStopwatch += Time.fixedDeltaTime;
			if (targetUpdateStopwatch >= targetUpdateInterval)
			{
				targetUpdateStopwatch -= targetUpdateInterval;
				Ray aimRay = new Ray(orbPosition, body.inputBank ? body.inputBank.aimDirection : body.transform.forward);
				UpdateTarget(aimRay);
			}
		}

		private void UpdateTarget(Ray aimRay)
		{
			search.teamMaskFilter = RoR2.TeamMask.GetUnprotectedTeams(body.teamComponent.teamIndex);
			search.filterByLoS = true;
			search.searchOrigin = aimRay.origin;
			search.searchDirection = aimRay.direction;
			search.sortMode = RoR2.BullseyeSearch.SortMode.Angle;
			search.maxDistanceFilter = 100f;
			search.maxAngleFilter = 180f;
			search.RefreshCandidates();
			search.FilterOutGameObject(base.gameObject);
			RoR2.HurtBox hurtBox = search.GetResults().FirstOrDefault();
			Transform newTarget = (hurtBox ? hurtBox.transform : null);
			for (int i = 0; i < beamCount; i++)
			{
				beamObjects[i].GetComponent<RadiantBeamController>().SetNewTarget(newTarget);
			}
		}

		public void UpdateMissilesServer()
		{
			if (missileTimer <= 0f)
			{
				if (missileQueueServer.Count != 0)
				{
					Vector3 onUnitSphere = UnityEngine.Random.onUnitSphere;
					FireProjectileInfo fireProjectileInfo = new FireProjectileInfo
					{
						projectilePrefab = missileProjectilePrefab,
						position = orbPosition,
						rotation = RoR2.Util.QuaternionSafeLookRotation(onUnitSphere),
						owner = base.gameObject,
						damage = missileQueueServer[0],
						crit = body.RollCrit(),
						force = 200f
					};
					ProjectileManager.instance.FireProjectile(fireProjectileInfo);
					missileQueueServer.RemoveAt(0);
				}
			}
			else
			{
				missileTimer -= Time.fixedDeltaTime;
			}
		}
	}

	public static Material matTracerBright = LegacyResourcesAPI.Load<GameObject>("prefabs/effects/tracers/TracerCaptainDefenseMatrix").transform.Find("TracerHead/Flash").gameObject.GetComponent<ParticleSystemRenderer>().material;

	public static Material goldenMaterial;

	public static Material radiantParticleReplacement;

	public static GameObject missileProjectilePrefab;

	public static GameObject missileProjectileGhost;

	public static GameObject spawnMissileEffect;

	public static GameObject radiantOrb;

	public static GameObject radiantBeam;

	public static GameObject radiantBeamControllerPrefab;

	public override string EliteName => "Radiant";

	public override Color EliteColor => Color.white;

	public override EliteTierDefinition MainEliteTierDefinition => SpikestripEliteBase<RadiantElite>.tierTwoEliteDefault;

	public override Sprite AffixBuffSprite => Base.GroovyAssetBundle.LoadAsset<Sprite>("texBuffAffixGravity.png");

	public override Sprite EquipmentIcon => Base.GroovyAssetBundle.LoadAsset<Sprite>("texAffixGravityIcon.png");

	public override string EquipmentName => "Their Guidance";

	public override string AffixDescriptionMainWord => "brilliance";

	public override Texture2D EliteRampTexture => Base.GroovyAssetBundle.LoadAsset<Texture2D>("texRampEliteRadiant.png");

	public override Type AffixBuffBehaviour => typeof(RadiantAffixBuffBehaviour);

	public override bool ServerOnlyAffixBuffBehaviour => false;

	public override bool IsEnabled => false;

	public override void Init()
	{
		base.Init();
		On.RoR2.CharacterModel.UpdateLights += CharacterModel_UpdateLights;
		goldenMaterial = UnityEngine.Object.Instantiate(LegacyResourcesAPI.Load<Material>("materials/matOnFire"));
		goldenMaterial.mainTexture = null;
		goldenMaterial.DisableKeyword("USE_CLOUDS");
		goldenMaterial.DisableKeyword("USE_UV1");
		SpikestripVisuals.RegisterOverlayMaterial(goldenMaterial, (RoR2.CharacterModel model) => model.body.HasBuff(AffixBuff));
		radiantParticleReplacement = UnityEngine.Object.Instantiate(LegacyResourcesAPI.Load<Material>("materials/matEliteLunarParticleReplacement"));
		radiantParticleReplacement.SetTexture("_RemapTex", goldenMaterial.GetTexture("_RemapTex"));
		radiantOrb = PrefabAPI.InstantiateClone(LegacyResourcesAPI.Load<GameObject>("prefabs/projectileghosts/GrandparentGravSphereGhost"), "RadiantOrb", false);
		SpikestripContentBase.DestroyImmediate(radiantOrb.GetComponent<ProjectileGhostController>());
		SpikestripContentBase.DestroyImmediate(radiantOrb.transform.Find("Particles/Goo, Drip").gameObject);
		SpikestripContentBase.DestroyImmediate(radiantOrb.transform.Find("Particles/GlowParticles, Fast").gameObject);
		radiantOrb.transform.Find("Particles/GlowParticles, Fast (1)").localScale = 0.05f * Vector3.one;
		radiantOrb.transform.Find("Particles/SoftGlow, Backdrop").localScale = 0.8f * Vector3.one;
		ParticleSystemRenderer[] componentsInChildren = radiantOrb.GetComponentsInChildren<ParticleSystemRenderer>();
		foreach (ParticleSystemRenderer particleSystemRenderer in componentsInChildren)
		{
			if (particleSystemRenderer.gameObject.name != "SoftGlow, Backdrop" && particleSystemRenderer.gameObject.name != "Sphere")
			{
				particleSystemRenderer.material = radiantParticleReplacement;
			}
		}
		Color color = SpikestripContentBase.ColorRGB(255f, 221f, 73f);
		ParticleSystemRenderer component = radiantOrb.transform.Find("Particles/Sphere").GetComponent<ParticleSystemRenderer>();
		Material material = UnityEngine.Object.Instantiate(component.material);
		material.SetTexture("_RemapTex", LegacyResourcesAPI.Load<Texture>("textures/texWhite"));
		material.SetColor("_TintColor", color);
		component.material = material;
		Light componentInChildren = radiantOrb.GetComponentInChildren<Light>();
		componentInChildren.color = color;
		RoR2.FlickerLight componentInChildren2 = radiantOrb.GetComponentInChildren<RoR2.FlickerLight>();
		componentInChildren2.sinWaves[0].period = 1f;
		componentInChildren2.sinWaves[0].frequency = 1f;
		componentInChildren2.sinWaves[1].period = 2f;
		componentInChildren2.sinWaves[1].frequency = 0.5f;
		Color value = SpikestripContentBase.ColorRGB(255f, 203f, 73f);
		GameObject gameObject = LegacyResourcesAPI.Load<GameObject>("prefabs/characterbodies/TimeCrystalBody").transform.Find("ModelBase/Mesh/Beam").gameObject;
		radiantBeam = PrefabAPI.InstantiateClone(gameObject, "RadiantBeam", false);
		ParticleSystemRenderer component2 = radiantBeam.GetComponent<ParticleSystemRenderer>();
		Material material2 = UnityEngine.Object.Instantiate(matTracerBright);
		material2.SetColor("_TintColor", value);
		material2.mainTexture = null;
		component2.material = material2;
		radiantBeamControllerPrefab = PrefabAPI.InstantiateClone(LegacyResourcesAPI.Load<GameObject>("prefabs/BleedEffect"), "RadiantBeamControllerPrefab", false);
		SpikestripContentBase.DestroyImmediate(radiantBeamControllerPrefab.transform.Find("Burst").gameObject);
		SpikestripContentBase.DestroyImmediate(radiantBeamControllerPrefab.GetComponent<RoR2.LoopSound>());
		SpikestripContentBase.DestroyImmediate((UnityEngine.Object)(object)radiantBeamControllerPrefab.GetComponent<AkEvent>());
		SpikestripContentBase.DestroyImmediate((UnityEngine.Object)(object)radiantBeamControllerPrefab.GetComponent<AkGameObj>());
		Rigidbody component3 = radiantBeamControllerPrefab.GetComponent<Rigidbody>();
		component3.isKinematic = false;
		component3.mass = 600f;
		LineRenderer lineRenderer = radiantBeamControllerPrefab.AddComponent<LineRenderer>();
		lineRenderer.material = material2;
		lineRenderer.startColor = Color.white;
		lineRenderer.startWidth = 1f;
		lineRenderer.endColor = Color.white;
		lineRenderer.endWidth = 1f;
		lineRenderer.positionCount = 2;
		RadiantBeamController radiantBeamController = radiantBeamControllerPrefab.AddComponent<RadiantBeamController>();
		missileProjectileGhost = PrefabAPI.InstantiateClone(LegacyResourcesAPI.Load<GameObject>("prefabs/projectileghosts/MissileGhost"), "RadiantMissileGhost", false);
		TrailRenderer componentInChildren3 = missileProjectileGhost.GetComponentInChildren<TrailRenderer>();
		componentInChildren3.material = material2;
		componentInChildren3.startColor = Color.white;
		componentInChildren3.startWidth = 1f;
		componentInChildren3.endColor = Color.white;
		componentInChildren3.endWidth = 0f;
		missileProjectilePrefab = PrefabAPI.InstantiateClone(LegacyResourcesAPI.Load<GameObject>("prefabs/projectiles/MissileProjectile"), "RadiantMissile", true);
		ProjectileController component4 = missileProjectilePrefab.GetComponent<ProjectileController>();
		component4.ghostPrefab = missileProjectileGhost;
		component4.procCoefficient = 0f;
		SpikestripContentBase.projectilePrefabContent.Add(missileProjectilePrefab);
		PrefabAPI.RegisterNetworkPrefab(missileProjectilePrefab);
	}

	private void CharacterModel_UpdateLights(On.RoR2.CharacterModel.orig_UpdateLights orig, RoR2.CharacterModel self)
	{
		if (self.myEliteIndex == MainEliteDef.eliteIndex)
		{
			self.lightColorOverride = SpikestripContentBase.ColorRGB(255f, 221f, 73f);
			self.particleMaterialOverride = radiantParticleReplacement;
		}
		orig(self);
	}

	public override void OnHitEnemyServer(RoR2.DamageInfo damageInfo, GameObject victim)
	{
		GameObject attacker = damageInfo.attacker;
		RoR2.CharacterBody characterBody = (attacker ? attacker.GetComponent<RoR2.CharacterBody>() : null);
		if (!attacker || !characterBody)
		{
			return;
		}
		RadiantAffixBuffBehaviour component = attacker.GetComponent<RadiantAffixBuffBehaviour>();
		if ((bool)component && characterBody.HasBuff(AffixBuff))
		{
			for (int i = 0; i < 3; i++)
			{
				component.missileQueueServer.Add(RoR2.Util.OnHitProcDamage(damageInfo.damage, characterBody.damage, 0.2f));
			}
		}
	}

	public override void AssignEquipmentValues()
	{
		EquipmentPickupModel = CreateAffixModel(Color.yellow);
	}

	public override void PopulateItemDisplayRules(ItemDisplayRuleDict itemDisplayRuleDict)
	{
	}
}
