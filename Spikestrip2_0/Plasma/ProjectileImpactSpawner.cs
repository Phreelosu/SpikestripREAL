using RoR2.Projectile;
using UnityEngine;
using UnityEngine.Networking;

[RequireComponent(typeof(ProjectileController))]
public class ProjectileImpactSpawner : MonoBehaviour, IProjectileImpactBehavior
{
	public GameObject prefab;

	public bool useRotation;

	public void OnProjectileImpact(ProjectileImpactInfo impactInfo)
	{
		if (NetworkServer.active)
		{
			Quaternion quaternion = Quaternion.identity;
			if (useRotation)
			{
				quaternion = base.transform.rotation;
			}
			ProjectileController component = GetComponent<ProjectileController>();
			ProjectileDamage component2 = GetComponent<ProjectileDamage>();
			if ((bool)component2 && (bool)component)
			{
				ProjectileManager.instance.FireProjectile(prefab, base.transform.position, quaternion, component.owner, component2.damage, component2.force, component2.crit, component2.damageColorIndex, (GameObject)null, -1f);
			}
		}
	}
}
