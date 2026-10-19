using RoR2;
using UnityEngine;

namespace PlasmaCoreSpikestripContent.Core;

[CreateAssetMenu(fileName = "ssibBlank", menuName = "PlasmaCore/SpikeStrip/InterableBase", order = 2)]
public class SSInteractibleBaseSO : ScriptableObject
{
	public string InteractableName = "";

	public bool UsesDirector = true;

	public InteractableSpawnCard InteractableSpawnCard;

	public string interactableCategory = "";

	public int SelectionWeight = 1;

	public DirectorCore.MonsterSpawnDistance spawnDistance;

	public int MinimumStageCompletion;
}
