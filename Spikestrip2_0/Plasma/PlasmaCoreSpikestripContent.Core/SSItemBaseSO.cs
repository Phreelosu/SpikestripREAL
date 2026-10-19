using System;
using RoR2;
using UnityEngine;
using Spikestrip2_0.Core;

namespace PlasmaCoreSpikestripContent.Core;

[CreateAssetMenu(fileName = "ssibBlank", menuName = "PlasmaCore/SpikeStrip/ItemBase", order = 0)]
public class SSItemBaseSO : ScriptableObject
{
	public string ItemName = "";

	public Sprite Icon;

	public ItemTier ItemTier = ItemTier.NoTier;

	public string ItemToken = "";

	[TextArea(0, 5)]
	public string ItemPickup = "";

	[TextArea(0, 5)]
	public string ItemDescription = "";

	public GameObject pickupObject;

	public GameObject displayObject;

	public GameObject[] extraDisplayObjects;

	public BuffDef[] buffs;

	public ItemTag[] tags = Array.Empty<ItemTag>();
}
