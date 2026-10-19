using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

namespace PlasmaCoreSpikestripContent.Components;

public class TheCoolerRampFog : MonoBehaviour
{
	public PostProcessVolume target;

	public float intensity = 1f;

	public float power = 1f;

	public float fogZero;

	public float fogOne = 0.5f;

	public Color32 startColor = Color.white;

	public Color32 middleColor = Color.grey;

	public Color32 endColor = Color.black;

	public float skyboxPower = 0.1f;

	private void Start()
	{
		RampFog rampFog = ScriptableObject.CreateInstance<RampFog>();
		rampFog.enabled.value = true;
		rampFog.fogIntensity.value = intensity;
		rampFog.fogIntensity.overrideState = true;
		rampFog.fogPower.value = power;
		rampFog.fogPower.overrideState = true;
		rampFog.fogOne.value = fogOne;
		rampFog.fogOne.overrideState = true;
		rampFog.fogZero.value = fogZero;
		rampFog.fogZero.overrideState = true;
		rampFog.fogColorStart.value = startColor;
		rampFog.fogColorStart.overrideState = true;
		rampFog.fogColorMid.value = middleColor;
		rampFog.fogColorMid.overrideState = true;
		rampFog.fogColorEnd.value = endColor;
		rampFog.fogColorEnd.overrideState = true;
		rampFog.skyboxStrength.value = skyboxPower;
		rampFog.skyboxStrength.overrideState = true;
		target.profile.AddSettings(rampFog);
	}
}
