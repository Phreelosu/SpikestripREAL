using System;
using RoR2;
using RoR2.Skills;
using UnityEngine;

namespace PlasmaCoreSpikestripContent.Core;

[CreateAssetMenu(fileName = "sssbBlank", menuName = "PlasmaCore/SpikeStrip/SkillBase", order = 5)]
public class SSSkillBaseSO : ScriptableObject
{
	public string Name = "";

	[TextArea(0, 5)]
	public string Description = "";

	public string SkillToken = "";

	public string PrerequisiteIdentifier = "";

	public string AchievementName = "";

	public Sprite SkillIcon;

	public string CharacterName = "";

	public string OwnerBodyPath = "";

	public string SkillFamilyPath = "";

	public Type[] EntityStateTypes = new Type[0];

	public SkillDef SkillDefinition;

	public GameObject[] Projectiles;

	public GameObject[] Effects;

	public GameObject[] MiscGameObjects;

	public Texture[] MiscTextures;

	public BuffDef[] buffs;

	public SkillDef[] MiscSkillDefs;
}
