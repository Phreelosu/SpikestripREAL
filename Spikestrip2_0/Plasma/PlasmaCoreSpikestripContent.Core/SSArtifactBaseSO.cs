using R2API.ScriptableObjects;
using RoR2;
using UnityEngine;
using Spikestrip2_0.Core;

namespace PlasmaCoreSpikestripContent.Core;

[CreateAssetMenu(fileName = "ssabBlank", menuName = "PlasmaCore/SpikeStrip/ArtifactBase", order = 1)]
public class SSArtifactBaseSO : ScriptableObject
{
	public string Name = "";

	public string OverrideFullName = "";

	public string Description = "";

	public Sprite EnabledIcon;

	public Sprite DisabledIcon;

	public ArtifactCode artifactCode;

	public GameObject pickupModel;

	public UnlockableDef unlockableDef;

	public BuffDef[] associatedBuffs;
}
