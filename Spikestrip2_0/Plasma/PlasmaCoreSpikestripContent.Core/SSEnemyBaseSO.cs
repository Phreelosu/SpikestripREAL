using RoR2;
using RoR2.Skills;
using UnityEngine;

namespace PlasmaCoreSpikestripContent.Core;

[CreateAssetMenu(fileName = "ssebBlank", menuName = "PlasmaCore/SpikeStrip/EnemyBase", order = 6)]
public class SSEnemyBaseSO : ScriptableObject
{
	public GameObject CharacterMaster;

	public GameObject CharacterBody;

	public GameObject CharacterModel;

	public string Name;

	public UnlockableDef logUnlock;

	[Header("Skills and Projectiles")]
	public BuffDef[] Buffs;

	public SkillFamily[] SkillFamilies;

	public SkillDef[] Skills;

	public GameObject[] Effects;

	public GameObject[] Projectiles;

	public GameObject[] MiscGameObjects;

	[Header("Spawn Card")]
	public CharacterSpawnCard CharacterSpawnCard;

	public string CharacterCategory = "";

	public int SelectionWeight = 1;

	public DirectorCore.MonsterSpawnDistance spawnDistance;

	public int MinimumStageCompletion;
}
