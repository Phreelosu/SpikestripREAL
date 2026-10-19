using RoR2;
using UnityEngine;

namespace PlasmaCoreSpikestripContent.Core;

[CreateAssetMenu(fileName = "ssebBlank", menuName = "PlasmaCore/SpikeStrip/EquipBase", order = 1)]
public class SSEquipBaseSO : ScriptableObject
{
	public string EquipName = "";

	public Sprite Icon;

	public string EquipToken = "";

	[TextArea(0, 5)]
	public string EquipPickup = "";

	[TextArea(0, 5)]
	public string EquipDescription = "";

	public float Cooldown = 45f;

	public GameObject pickupObject;

	public GameObject displayObject;

	public GameObject[] extraDisplayObjects;

	public BuffDef associatedBuff;

	public BuffDef[] buffs;
}
