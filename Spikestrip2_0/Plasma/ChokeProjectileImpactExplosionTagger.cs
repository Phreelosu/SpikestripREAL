using PlasmaCoreSpikestripContent.Components;
using PlasmaCoreSpikestripContent.Content.Skills;
using RoR2;
using RoR2.Projectile;
using UnityEngine;

public class ChokeProjectileImpactExplosionTagger : MonoBehaviour, IProjectileImpactBehavior
{
	public float radius = 6f;

	public void OnProjectileImpact(ProjectileImpactInfo impactInfo)
	{
		ProjectileController component = GetComponent<ProjectileController>();
		ChokeMainZone chokeMainZone = null;
		foreach (ChokeMainZone instance in ChokeMainZone.instances)
		{
			if ((bool)instance.attackerBody && instance.attackerBody.gameObject == component.owner)
			{
				chokeMainZone = instance;
				break;
			}
		}
		if (!chokeMainZone)
		{
			return;
		}
		Collider[] array = Physics.OverlapSphere(base.gameObject.transform.position, radius, LayerIndex.entityPrecise.mask);
		for (int i = 0; i < array.Length; i++)
		{
			HurtBox component2 = array[i].GetComponent<HurtBox>();
			if (!component2)
			{
				continue;
			}
			HealthComponent healthComponent = component2.healthComponent;
			if ((bool)healthComponent)
			{
				if ((bool)component.owner && healthComponent.gameObject == component.owner)
				{
					chokeMainZone.forceTargets.Add(healthComponent);
					healthComponent.body.AddTimedBuff(MageChokeArea.scriptableObject.buffs[1], 10f);
				}
				else if (FriendlyFireManager.ShouldSplashHitProceed(healthComponent, component.teamFilter.teamIndex))
				{
					chokeMainZone.forceTargets.Add(healthComponent);
					healthComponent.body.AddTimedBuff(MageChokeArea.scriptableObject.buffs[1], 10f);
				}
			}
		}
	}
}
