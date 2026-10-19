using UnityEngine;
using UnityEngine.AddressableAssets;

namespace PlasmaCoreSpikestripContent.Components;

public class GameObjectInstantiater : MonoBehaviour
{
	public enum GameObjectType
	{
		None,
		DiabloExplosion,
		DiabloGhostPoint
	}

	public GameObjectType toInstantiate;

	private void Start()
	{
		switch (toInstantiate)
		{
		case GameObjectType.DiabloExplosion:
			Object.Instantiate(Addressables.LoadAssetAsync<GameObject>("c5477c8a6fc239c45a7268052343737f").WaitForCompletion().transform.GetChild(0).gameObject, base.transform);
			break;
		case GameObjectType.DiabloGhostPoint:
			Object.Instantiate(Addressables.LoadAssetAsync<GameObject>("e478bf9d9eb031246b7262dd8171c63e").WaitForCompletion().transform.GetChild(4).GetChild(0).GetChild(0)
				.gameObject, base.transform);
				break;
			case GameObjectType.None:
				break;
			}
		}
	}
