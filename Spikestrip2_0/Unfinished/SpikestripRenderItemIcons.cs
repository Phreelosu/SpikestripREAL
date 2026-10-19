using R2API.Utils;
using RoR2;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceProviders;

namespace GrooveSaladSpikestripContent;

public class SpikestripRenderItemIcons : SpikestripContentBase<SpikestripRenderItemIcons>
{
	public static PickupDef selectedPickupDef;

	public override string ConfigName => "Render Item Icons";

	public override string ConfigSection => "Utils";

	public override bool IsEnabled => false;

	public override void Init()
	{
		base.Init();
		CommandHelper.AddToConsoleWhenReady();
	}

	[ConCommand(commandName = "render_pickup", flags = ConVarFlags.None, helpText = "Render Pickup Icon")]
	public static void CCRenderItem(ConCommandArgs args)
	{
		SpikestripContentBase.Log("render pickup!");
		string text = args.TryGetArgString(0);
		if (string.IsNullOrEmpty(text))
		{
			Debug.Log("Please input a valid pickup name");
			return;
		}
		foreach (PickupDef allPickup in PickupCatalog.allPickups)
		{
			string text2 = allPickup.internalName.Substring(allPickup.internalName.IndexOf(".") + 1).ToLower();
			if (text2.Contains(text.ToLower()) && !allPickup.internalName.StartsWith("Artifact"))
			{
				selectedPickupDef = allPickup;
				AsyncOperationHandle<SceneInstance> asyncOperationHandle = Addressables.LoadSceneAsync("722873b571c73734c8572658dbb8f0db");
				asyncOperationHandle.Completed += SpikestripRenderItemIcons_Completed;
				return;
			}
		}
		Debug.Log("Pickup with name " + text + " not found.");
	}

	private static void SpikestripRenderItemIcons_Completed(AsyncOperationHandle<SceneInstance> sceneInstance)
	{
		GameObject[] rootGameObjects = sceneInstance.Result.Scene.GetRootGameObjects();
		for (int i = 0; i < rootGameObjects.Length; i++)
		{
			if (rootGameObjects[i].name == "ITEM GOES HERE (can offset from here)")
			{
				GameObject gameObject = rootGameObjects[i];
				ItemDisplay[] componentsInChildren = gameObject.GetComponentsInChildren<ItemDisplay>();
				foreach (ItemDisplay itemDisplay in componentsInChildren)
				{
					itemDisplay.gameObject.SetActive(value: false);
				}
				GameObject gameObject2 = Object.Instantiate(selectedPickupDef.displayPrefab);
				gameObject2.transform.SetParent(gameObject.transform);
				gameObject2.transform.localPosition = Vector3.zero;
			}
		}
	}
}
