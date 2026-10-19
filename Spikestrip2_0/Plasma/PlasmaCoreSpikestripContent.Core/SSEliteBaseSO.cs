using RoR2;
using UnityEngine;
using Spikestrip2_0.Core;

namespace PlasmaCoreSpikestripContent.Core;

[CreateAssetMenu(fileName = "ssebBlank", menuName = "PlasmaCore/SpikeStrip/EliteBase", order = 7)]
public class SSEliteBaseSO : ScriptableObject
{
	public enum EliteTierType
	{
		TierOne = 1,
		TierTwo = 3,
		Honor = 2,
		Lunar = 4
	}

	public string Name;

	public Color eliteColor = Color.white;

	public Color affixColor = Color.white;

	public EliteTierType tier = EliteTierType.TierOne;

	public Sprite buffSprite;

	public Sprite EquipmentIcon;

	public string EquipmentName;

	public string AffixDescriptionMainWord;

	public Texture2D EliteRampTexture;

	public GameObject EquipmentDisplayModel;

	public GameObject[] MiscGameObjects = new GameObject[0];

	public BuffDef[] MiscBuffs = new BuffDef[0];

	public Material OverlayMaterial;
}
