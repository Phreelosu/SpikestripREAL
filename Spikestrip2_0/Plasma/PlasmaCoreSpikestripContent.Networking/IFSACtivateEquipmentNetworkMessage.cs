using PlasmaCoreSpikestripContent.Content.Equipment;
using R2API.Networking.Interfaces;
using RoR2;
using UnityEngine.Networking;

namespace PlasmaCoreSpikestripContent.Networking;

public class IFSACtivateEquipmentNetworkMessage : INetMessage, ISerializableObject
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
		if (NetworkServer.active && targeterInstance != null && (bool)targeterInstance.GetComponent<ImpulseFrostShield.ImpulseFrostTargeter>())
		{
			ImpulseFrostShield.ImpulseFrostTargeter component = targeterInstance.GetComponent<ImpulseFrostShield.ImpulseFrostTargeter>();
			component.targetIsValid = isValid;
			component.forceTargetIsValid = isValid;
			targeterInstance.GetComponent<EquipmentSlot>().ExecuteIfReady();
		}
	}

	public void Serialize(NetworkWriter writer)
	{
		writer.Write(isValid);
		writer.Write(targeterInstance);
	}
}
