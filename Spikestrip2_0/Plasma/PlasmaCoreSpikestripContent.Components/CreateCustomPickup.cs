using PlasmaCoreSpikestripContent.Content.Interactibles;
using PlasmaCoreSpikestripContent.Core;
using RoR2;
using UnityEngine;

namespace PlasmaCoreSpikestripContent.Components;

public class CreateCustomPickup : MonoBehaviour
{
	public enum PickupToDrop
	{
		None,
		HiddenItem
	}

	public PickupToDrop pickupToDrop;

	public Transform dropPoint;

	public Vector3 dropForce = new Vector3(0f, 20f, 7f);

	public void Begin()
	{
		PickupIndex pickupIndex = PickupIndex.none;
		PickupToDrop pickupToDrop = this.pickupToDrop;
		if (pickupToDrop != PickupToDrop.None && pickupToDrop == PickupToDrop.HiddenItem)
		{
			pickupIndex = CloakedShrine.hiddenPickup.pickupIndex;
		}
		if (pickupIndex != PickupIndex.none)
		{
			PlasmaLog.Log("CreateCustomPickup - Creating pickup with index " + pickupIndex, allLogOnly: true);
			PickupDropletController.CreatePickupDroplet(pickupIndex, (dropPoint ? dropPoint : base.transform).position, base.transform.TransformVector(dropForce));
		}
		else
		{
			PlasmaLog.LogWarn("CreateCustomPickup - Does not have valid pickup!");
		}
	}
}
