using RoR2;
using UnityEngine;

namespace Spikestrip2_0.Core;

public static class CommonDisplayReferences
{
	public static EquipmentDef AffixRed = LegacyResourcesAPI.Load<EquipmentDef>("EquipmentDefs/AffixRed");

	public static GameObject AffixRedHornDisplay = SpikestripUtil.AddressablesLoad<GameObject>("880352d2f519466409abd39061d75b79");

	public static EquipmentDef AffixWhite = LegacyResourcesAPI.Load<EquipmentDef>("EquipmentDefs/AffixWhite");

	public static GameObject AffixWhiteCrownDisplay = SpikestripUtil.AddressablesLoad<GameObject>("767a76f115c24384498eb6bed7decc4d");

	public static EquipmentDef AffixBlue = LegacyResourcesAPI.Load<EquipmentDef>("EquipmentDefs/AffixBlue");

	public static GameObject AffixBlueHornDisplay = SpikestripUtil.AddressablesLoad<GameObject>("01c99be16781ea64cb7c24f1ff1354d4");

	public static EquipmentDef AffixPoison = LegacyResourcesAPI.Load<EquipmentDef>("EquipmentDefs/AffixPoison");

	public static GameObject AffixPoisonCrownDisplay = SpikestripUtil.AddressablesLoad<GameObject>("5071e940404ac11438977d61b4f37e5c");

	public static EquipmentDef AffixHaunted = LegacyResourcesAPI.Load<EquipmentDef>("EquipmentDefs/AffixHaunted");

	public static GameObject AffixHauntedCrownDisplay = SpikestripUtil.AddressablesLoad<GameObject>("f75c662c5bc0d744d8919219eeba482d");

	public static EquipmentDef AffixEarth = SpikestripUtil.AddressablesLoad<EquipmentDef>("463f8f6917dd8af40860b023a520f051");

	public static GameObject AffixEarthAntlersDisplay = SpikestripUtil.AddressablesLoad<GameObject>("f76a390c2ffbf9645862f81533eafc69");

	public static EquipmentDef AffixVoid = SpikestripUtil.AddressablesLoad<EquipmentDef>("118960de11f213c45806f7f0d2aef4d6");

	public static GameObject AffixVoidDisplay = SpikestripUtil.AddressablesLoad<GameObject>("a5888055b2e8dfe479af511631631c0a");
}
