using R2API.Networking;
using R2API.Networking.Interfaces;
using R2API.Utils;
using RoR2;
using UnityEngine;
using UnityEngine.Networking;

namespace PlasmaCoreSpikestripContent.Networking;

public class IFSClientTargetRequestNetworkMessage : INetMessage, ISerializableObject
{
	public NetworkIdentity targeterInstance;

	public GameObject bodyGameObject;

	public void Deserialize(NetworkReader reader)
	{
		bodyGameObject = reader.ReadGameObject();
		targeterInstance = reader.ReadNetworkIdentity();
	}

	public void OnReceived()
	{
		if (NetworkServer.active)
		{
			if ((bool)bodyGameObject)
			{
				NetMessageExtensions.Send((INetMessage)(object)new IFSTargetConfirmRequestNetworkMessage
				{
					targeterInstance = targeterInstance,
					isValid = (Reflection.GetFieldValue<float>((object)bodyGameObject.GetComponent<CharacterBody>(), "outOfCombatStopwatch") <= 0.75f)
				}, (NetworkDestination)1);
			}
			else
			{
				NetMessageExtensions.Send((INetMessage)(object)new IFSTargetConfirmRequestNetworkMessage
				{
					targeterInstance = targeterInstance,
					isValid = false
				}, (NetworkDestination)1);
			}
		}
	}

	public void Serialize(NetworkWriter writer)
	{
		writer.Write(bodyGameObject);
		writer.Write(targeterInstance);
	}
}
