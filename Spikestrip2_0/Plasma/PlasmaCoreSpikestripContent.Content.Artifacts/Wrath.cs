using GrooveSaladSpikestripContent;
using PlasmaCoreSpikestripContent.Components;
using PlasmaCoreSpikestripContent.Core;
using R2API;
using RoR2;
using UnityEngine;
using UnityEngine.AddressableAssets;
using Spikestrip2_0.Core;

namespace PlasmaCoreSpikestripContent.Content.Artifacts;

public class Wrath : ArtifactBase<Wrath>
{
	public static SSArtifactBaseSO scriptableObject;

	public override string Name => "Wrath";

	public override string Description => scriptableObject.Description;

	public override Sprite EnabledIcon => scriptableObject.EnabledIcon;

	public override Sprite DisabledIcon => scriptableObject.DisabledIcon;

	public override void Init()
	{
		PlasmaCorePlugin.artifactBaseScriptableObjects.TryGetValue(Name, out scriptableObject);
		if ((bool)scriptableObject)
		{
			PlasmaLog.Log("Found associated ScriptableObject for ArtifactBase " + Name);
			base.Init();
			artifactDef.pickupModelPrefab = scriptableObject.pickupModel;
			artifactDef.pickupModelPrefab.GetComponentInChildren<MeshRenderer>().material = Addressables.LoadAssetAsync<GameObject>("30a7682adf74fd843af4b8f5d5cdea3c").WaitForCompletion().GetComponentInChildren<MeshRenderer>()
				.material;
			artifactDef.unlockableDef = scriptableObject.unlockableDef;
			ArtifactCodeAPI.AddCode(artifactDef, scriptableObject.artifactCode);
		}
		else
		{
			PlasmaLog.LogWarn("Missing associated ScriptableObject for ArtifactBase " + Name);
			base.Init();
		}
	}

	public override void SetHooks()
	{
		SceneDirector.onPostPopulateSceneServer += SceneDirector_onPostPopulateSceneServer;
	}

	public override void UnsetHooks()
	{
		SceneDirector.onPostPopulateSceneServer -= SceneDirector_onPostPopulateSceneServer;
	}

	public void SceneDirector_onPostPopulateSceneServer(SceneDirector obj)
	{
		obj.gameObject.AddComponent<WrathMeteorController>();
	}
}
