using System;
using PlasmaCoreSpikestripContent.Content.Elites;
using RoR2;
using RoR2.Projectile;
using UnityEngine;
using UnityEngine.Networking;

namespace PlasmaCoreSpikestripContent.Components;

public class RagingEliteProjectileSpawner : MonoBehaviour
{
	public static GameObject projectileSpawnerPrefab;

	public int waveCount = 5;

	public int projectilesPerWave = 5;

	public float delayBetweenWaves = 0.333f;

	public bool isCrit;

	public CharacterBody targetBody;

	public GameObject owner;

	public float ownerBodyDamage;

	private float stopwatch;

	private int spawnedWaves;

	public void FixedUpdate()
	{
		if (!NetworkServer.active)
		{
			UnityEngine.Object.Destroy(this);
			return;
		}
		stopwatch -= Time.fixedDeltaTime;
		if (stopwatch <= 0f)
		{
			stopwatch += delayBetweenWaves;
			for (int i = 0; i < projectilesPerWave; i++)
			{
				Vector3 position = RandomTerrainPositionInCylinder(targetBody ? targetBody.corePosition : base.transform.position, 10f, 40f, 10f);
				FireProjectileInfo fireProjectileInfo = new FireProjectileInfo
				{
					crit = isCrit,
					owner = base.gameObject,
					position = position,
					projectilePrefab = RagingElite.deathProjectile,
					rotation = Quaternion.Euler(0f, UnityEngine.Random.Range(0f, 360f), 0f),
					damage = 5f * ownerBodyDamage
				};
				ProjectileManager.instance.FireProjectile(fireProjectileInfo);
			}
			spawnedWaves++;
			if (spawnedWaves >= waveCount)
			{
				UnityEngine.Object.Destroy(this);
			}
		}
	}

	public static Vector3 RandomTerrainPositionInCylinder(Vector3 position, float minRadius, float maxRadius, float topOfCylinderHeight)
	{
		float num = UnityEngine.Random.Range(minRadius, maxRadius);
		float f = UnityEngine.Random.Range(0f, (float)Math.PI * 2f);
		Vector3 vector = new Vector3(position.x + Mathf.Sin(f) * num, position.y + topOfCylinderHeight, position.z + Mathf.Cos(f) * num);
		Ray ray = new Ray(vector, Vector3.down);
		RaycastHit hitInfo = default(RaycastHit);
		if (Physics.Raycast(ray, out hitInfo, topOfCylinderHeight * 3f, LayerIndex.world.mask, QueryTriggerInteraction.Ignore))
		{
			return hitInfo.point;
		}
		return vector - Vector3.down * topOfCylinderHeight * 3f;
	}
}
