using PlasmaCoreSpikestripContent.Core;
using RoR2;
using RoR2.Projectile;
using UnityEngine;
using UnityEngine.Networking;

namespace PlasmaCoreSpikestripContent.Components;

public class WrathMeteorController : MonoBehaviour
{
	public static GameObject projectileToFire;

	public static float timerMax = 10f;

	public float timer = 10f;

	public static WrathMeteorController instance;

	public void Awake()
	{
		if ((bool)instance)
		{
			Object.DestroyImmediate(instance);
		}
		instance = this;
		projectileToFire = PlasmaCorePlugin.skyShardProjectile;
	}

	public void FixedUpdate()
	{
		if (!NetworkServer.active)
		{
			return;
		}
		timer -= Time.fixedDeltaTime;
		if (!(timer <= 0f))
		{
			return;
		}
		timer = timerMax;
		foreach (PlayerCharacterMasterController instance in PlayerCharacterMasterController.instances)
		{
			if ((bool)instance.master && (bool)instance.master.GetBody())
			{
				Vector3 footPosition = instance.master.GetBody().footPosition;
				footPosition = PlasmaUtils.GetNearestNode(PlasmaUtils.RandomTerrainPositionInCylinder(footPosition, 7f, 20f), 50f);
				FireProjectileInfo fireProjectileInfo = new FireProjectileInfo
				{
					crit = false,
					owner = base.gameObject,
					position = footPosition,
					projectilePrefab = projectileToFire,
					rotation = Quaternion.Euler(0f, Random.RandomRange(0f, 360f), 0f),
					damage = 150f * Run.instance.teamlessDamageCoefficient
				};
				ProjectileManager.instance.FireProjectile(fireProjectileInfo);
			}
		}
	}
}
