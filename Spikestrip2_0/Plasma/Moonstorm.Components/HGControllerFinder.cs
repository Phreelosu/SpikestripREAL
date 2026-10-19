using UnityEngine;

namespace Moonstorm.Components;

public class HGControllerFinder : MonoBehaviour
{
	public Renderer Renderer;

	public Material material;

	public void OnEnable()
	{
		if ((bool)Renderer && (bool)material)
		{
			Renderer.material = material;
			Renderer.sharedMaterials[0] = material;
			MaterialControllerComponents.MaterialController materialController = null;
			switch (material.shader.name)
			{
			case "Hopoo Games/Deferred/Standard":
				materialController = base.gameObject.AddComponent<MaterialControllerComponents.HGStandardController>();
				break;
			case "Hopoo Games/Deferred/Snow Topped":
				materialController = base.gameObject.AddComponent<MaterialControllerComponents.HGSnowToppedController>();
				break;
			case "Hopoo Games/FX/Cloud Remap":
				materialController = base.gameObject.AddComponent<MaterialControllerComponents.HGCloudRemapController>();
				break;
			case "Hopoo Games/FX/Cloud Intersection Remap":
				materialController = base.gameObject.AddComponent<MaterialControllerComponents.HGIntersectionController>();
				break;
			case "Hopoo Games/FX/Solid Parallax":
				materialController = base.gameObject.AddComponent<MaterialControllerComponents.HGSolidParallaxController>();
				break;
			case "Hopoo Games/Deferred/Wavy Cloth":
				materialController = base.gameObject.AddComponent<MaterialControllerComponents.HGWavyClothController>();
				break;
			case "Hopoo Games/FX/Opaque Cloud Remap":
				materialController = base.gameObject.AddComponent<MaterialControllerComponents.HGOpaqueCloudRemap>();
				break;
			case "Hopoo Games/FX/Distant Water":
				materialController = base.gameObject.AddComponent<MaterialControllerComponents.HGDistantWaterController>();
				break;
			}
			if ((bool)materialController)
			{
				materialController.material = material;
				materialController.renderer = Renderer;
				materialController.MaterialName = material.name;
				Object.Destroy(this);
			}
			else
			{
				base.enabled = false;
			}
		}
		else
		{
			base.enabled = false;
		}
	}
}
