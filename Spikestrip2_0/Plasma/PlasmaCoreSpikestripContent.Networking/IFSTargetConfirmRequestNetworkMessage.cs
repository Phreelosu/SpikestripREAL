using PlasmaCoreSpikestripContent.Content.Equipment;
using R2API.Networking.Interfaces;
using UnityEngine.Networking;

namespace PlasmaCoreSpikestripContent.Networking;

public class IFSTargetConfirmRequestNetworkMessage : INetMessage, ISerializableObject
{
	public NetworkIdentity targeterInstance;

	public bool isValid;

	public void Deserialize(NetworkReader reader)
	{
		isValid = reader.ReadBoolean();
		targeterInstance = reader.ReadNetworkIdentity();
	}

	public void OnReceived()
	{
		if (!NetworkServer.active && targeterInstance != null && (bool)targeterInstance.GetComponent<ImpulseFrostShield.ImpulseFrostTargeter>())
		{
			targeterInstance.GetComponent<ImpulseFrostShield.ImpulseFrostTargeter>().targetIsValid = isValid;
		}
	}

	public void Serialize(NetworkWriter writer)
	{
		writer.Write(isValid);
		writer.Write(targeterInstance);
	}
}
