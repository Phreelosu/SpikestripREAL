using On.RoR2.Projectile;
using RoR2;
using RoR2.Projectile;
using UnityEngine;
using Spikestrip2_0.Core;

namespace GrooveSaladSpikestripContent.Content;

public class Spread : ArtifactBase<Spread>
{
	public enum SplitType
	{
		None,
		Normal,
		Radius
	}

	public static float speedMultiplier = 0.5f;

	public static float coefficientMultiplier = 0.4f;

	public static float sizeMultiplier = 0.6f;

	public static int shardCount = 5;

	public bool CopyAetherium;

	public override string Name => "Spread";

	public override string Description => "Projectiles are split into weaker fragments.";

	public override Sprite EnabledIcon => SpikestripContent.GrooveBundle.LoadAsset<Sprite>("texArtifactSpreadEnabled.png");

	public override Sprite DisabledIcon => SpikestripContent.GrooveBundle.LoadAsset<Sprite>("texArtifactSpreadDisabled.png");

	public override void Init()
	{
		base.Init();
	}

	public override void SetHooks()
	{
		On.RoR2.Projectile.ProjectileManager.FireProjectile_FireProjectileInfo += ProjectileManager_FireProjectile_FireProjectileInfo;
		On.RoR2.Projectile.ProjectileManager.InitializeProjectile += ProjectileManager_InitializeProjectile;
		On.RoR2.Projectile.ProjectileGhostController.Awake += ProjectileGhostController_Awake;
	}

	private void ProjectileManager_FireProjectile_FireProjectileInfo(On.RoR2.Projectile.ProjectileManager.orig_FireProjectile_FireProjectileInfo orig, RoR2.Projectile.ProjectileManager self, FireProjectileInfo fireProjectileInfo)
	{
		if (ArtifactActive() && !CopyAetherium)
		{
			SplitType splitType = CanProjectileSplit(fireProjectileInfo);
			if (splitType == SplitType.None)
			{
				return;
			}
			Vector3 aimDirection = fireProjectileInfo.rotation * Vector3.forward;
			Vector3 position = fireProjectileInfo.position;
			Vector3 normalized = Vector3.ProjectOnPlane(Random.onUnitSphere, Vector3.up).normalized;
			float num = 360f / (float)shardCount;
			for (int i = 0; i < shardCount; i++)
			{
				if (splitType == SplitType.Normal)
				{
					fireProjectileInfo.rotation = Util.QuaternionSafeLookRotation(Util.ApplySpread(aimDirection, 0f, 15f, 1f, 0.5f));
				}
				if (splitType == SplitType.Radius)
				{
					Vector3 direction = Quaternion.AngleAxis(num * (float)i, Vector3.up) * normalized;
					fireProjectileInfo.position = new Ray(position, direction).GetPoint(5f);
				}
				try
				{
					CopyAetherium = true;
					RoR2.Projectile.ProjectileManager.instance.FireProjectile(fireProjectileInfo);
				}
				finally
				{
					CopyAetherium = false;
				}
			}
		}
		else
		{
			orig(self, fireProjectileInfo);
		}
	}

	private void ProjectileManager_InitializeProjectile(On.RoR2.Projectile.ProjectileManager.orig_InitializeProjectile orig, RoR2.Projectile.ProjectileController projectileController, FireProjectileInfo fireProjectileInfo)
	{
		orig(projectileController, fireProjectileInfo);
		SplitType splitType = CanProjectileSplit(fireProjectileInfo);
		GameObject gameObject = projectileController.gameObject;
		if (ArtifactActive() && (bool)gameObject && splitType != SplitType.None)
		{
			Transform transform = gameObject.transform;
			if ((bool)transform)
			{
				transform.localScale = new Vector3(transform.localScale.x * sizeMultiplier, transform.localScale.y * sizeMultiplier, transform.localScale.z * sizeMultiplier);
			}
			RoR2.Projectile.ProjectileDamage component = gameObject.GetComponent<RoR2.Projectile.ProjectileDamage>();
			if ((bool)component)
			{
				component.damage *= coefficientMultiplier;
				component.force *= coefficientMultiplier;
			}
			projectileController.procCoefficient *= coefficientMultiplier;
			RoR2.Projectile.ProjectileSimple component2 = gameObject.GetComponent<RoR2.Projectile.ProjectileSimple>();
			if ((bool)component2)
			{
				component2.desiredForwardSpeed *= speedMultiplier;
			}
			RoR2.Projectile.BoomerangProjectile component3 = gameObject.GetComponent<RoR2.Projectile.BoomerangProjectile>();
			if ((bool)component3)
			{
				component3.travelSpeed *= speedMultiplier;
			}
			RoR2.Projectile.MissileController component4 = gameObject.GetComponent<RoR2.Projectile.MissileController>();
			if ((bool)component4)
			{
				component4.acceleration *= speedMultiplier;
				component4.maxVelocity *= speedMultiplier;
				component4.rollVelocity *= speedMultiplier;
			}
			RoR2.Projectile.ProjectileExplosion[] components = gameObject.GetComponents<RoR2.Projectile.ProjectileExplosion>();
			foreach (RoR2.Projectile.ProjectileExplosion projectileExplosion in components)
			{
				projectileExplosion.blastRadius *= sizeMultiplier;
			}
		}
	}

	private void ProjectileGhostController_Awake(On.RoR2.Projectile.ProjectileGhostController.orig_Awake orig, RoR2.Projectile.ProjectileGhostController self)
	{
		if (ArtifactActive())
		{
			ParticleSystem[] componentsInChildren = self.GetComponentsInChildren<ParticleSystem>();
			for (int i = 0; i < componentsInChildren.Length; i++)
			{
				Vector3 localScale = componentsInChildren[i].gameObject.transform.localScale;
				componentsInChildren[i].gameObject.transform.localScale = new Vector3(localScale.x * sizeMultiplier, localScale.y * sizeMultiplier, localScale.z * sizeMultiplier);
				ParticleSystem.MainModule main = componentsInChildren[i].main;
				main.scalingMode = ParticleSystemScalingMode.Local;
			}
		}
		orig(self);
		if (ArtifactActive() && (bool)self.transform)
		{
			self.transform.localScale = new Vector3(self.transform.localScale.x * sizeMultiplier, self.transform.localScale.y * sizeMultiplier, self.transform.localScale.z * sizeMultiplier);
		}
	}

	public override void UnsetHooks()
	{
		On.RoR2.Projectile.ProjectileManager.FireProjectile_FireProjectileInfo -= ProjectileManager_FireProjectile_FireProjectileInfo;
		On.RoR2.Projectile.ProjectileManager.InitializeProjectile -= ProjectileManager_InitializeProjectile;
		On.RoR2.Projectile.ProjectileGhostController.Awake -= ProjectileGhostController_Awake;
	}

	private SplitType CanProjectileSplit(FireProjectileInfo fireProjectileInfo)
	{
		if ((bool)fireProjectileInfo.projectilePrefab)
		{
			RoR2.Projectile.ProjectileSimple component = fireProjectileInfo.projectilePrefab.GetComponent<RoR2.Projectile.ProjectileSimple>();
			RoR2.Projectile.BoomerangProjectile component2 = fireProjectileInfo.projectilePrefab.GetComponent<RoR2.Projectile.BoomerangProjectile>();
			bool flag = fireProjectileInfo.useSpeedOverride && fireProjectileInfo.speedOverride != 0f;
			bool flag2 = (bool)component && !fireProjectileInfo.useSpeedOverride && component.desiredForwardSpeed != 0f;
			bool flag3 = (bool)component2 && component2.travelSpeed != 0f;
			if (flag || flag2 || flag3)
			{
				return SplitType.Normal;
			}
			return SplitType.Radius;
		}
		return SplitType.None;
	}
}
