using System;
using GrooveSaladSpikestripContent;
using PlasmaCoreSpikestripContent.Content.Skills.States;
using PlasmaCoreSpikestripContent.Core;
using R2API;
using R2API.Utils;
using RoR2;
using RoR2.Projectile;
using RoR2.Skills;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;
using Spikestrip2_0.Core;

namespace PlasmaCoreSpikestripContent.Content.Skills;

public class MageChokeArea : SpikestripVoidAltSkillsBase<MageChokeArea>
{
	public static GameObject muzzleFlashEffect;

	public static SSSkillBaseSO scriptableObject;

	public static GameObject mainProjectile;

	public override string SkillName => "「Choke」";

	public override string SkillDescription => scriptableObject.Description;

	public override string SkillToken => scriptableObject.SkillToken;

	public override string PrerequisiteIdentifier => scriptableObject.PrerequisiteIdentifier;

	public override string CharacterName => scriptableObject.CharacterName;

	public override string AchievementName => scriptableObject.AchievementName;

	public override Sprite SkillIcon => scriptableObject.SkillIcon;

	public override GameObject OwnerBodyPrefab => SpikestripUtil.LegacyLoad<GameObject>(scriptableObject.OwnerBodyPath);

	public override SkillFamily[] SkillFamilies => new SkillFamily[1] { SpikestripUtil.LegacyLoad<SkillFamily>(scriptableObject.SkillFamilyPath) };

	public override Type[] EntityStateTypes => new Type[3]
	{
		typeof(Choke),
		typeof(ChokeFire),
		typeof(FireVoidBolts)
	};

	public override SkillDef GetSkillDef()
	{
		return scriptableObject.SkillDefinition;
	}

	public override void Init()
	{
		PlasmaCorePlugin.skillBaseScriptableObjects.TryGetValue(SkillName, out scriptableObject);
		SkillDef[] miscSkillDefs = scriptableObject.MiscSkillDefs;
		foreach (SkillDef item in miscSkillDefs)
		{
			ContentAddition.AddSkillDef(item);
		}
		GameObject[] effects = scriptableObject.Effects;
		foreach (GameObject effectPrefab in effects)
		{
			if ((bool)effectPrefab.GetComponent<RoR2.Orbs.OrbEffect>())
			{
				SpikestripUtil.DisableEffectPooling(effectPrefab);
			}
			ContentAddition.AddEffect(effectPrefab);
		}
		effects = scriptableObject.Projectiles;
		foreach (GameObject gameObject in effects)
		{
			ContentAddition.AddProjectile(gameObject);
			PlasmaUtils.RegisterNetworkPrefab(gameObject);
		}
		BuffDef[] buffs = scriptableObject.buffs;
		foreach (BuffDef item2 in buffs)
		{
			ContentAddition.AddBuffDef(item2);
		}
		base.Init();
		LanguageAPI.Add("MAGE_OVERRIDE_VOIDBOLTS_NAME", "「Void Bolts」");
		LanguageAPI.Add("MAGE_OVERRIDE_VOIDBOLTS_DESC", "Fire a bolt for <style=cIsVoid>220% damage</style> that <style=cIsVoid>marks</style> in a small area. Hold up to 4.</style>");
		LanguageAPI.Add("KEYWORD_CRUSH", "<style=cKeywordName>Crushing</style><style=cSub>Targets suffer <style=cIsVoid>25% more damage</style> for each character within 4m.</style>");
		muzzleFlashEffect = PrefabAPI.InstantiateClone(SpikestripUtil.LegacyLoad<GameObject>("Prefabs/Effects/MuzzleFlashes/MuzzleflashMageLightningLarge"), "MuzzleflashMageVoidLarge", false);
		ParticleSystemRenderer[] componentsInChildren = muzzleFlashEffect.GetComponentsInChildren<ParticleSystemRenderer>();
		componentsInChildren[0].material = UnityEngine.Object.Instantiate(componentsInChildren[0].material);
		componentsInChildren[0].material.SetTexture("_RemapTex", scriptableObject.MiscTextures[2]);
		Reflection.SetPropertyValue<Color>((object)componentsInChildren[1].GetComponent<ParticleSystem>().main.startColor, "color", (Color)new Color32(249, 23, byte.MaxValue, byte.MaxValue));
		componentsInChildren[2].material = UnityEngine.Object.Instantiate(componentsInChildren[2].material);
		componentsInChildren[2].material.SetTexture("_RemapTex", scriptableObject.MiscTextures[1]);
		componentsInChildren[3].material = UnityEngine.Object.Instantiate(componentsInChildren[3].material);
		componentsInChildren[3].material.SetTexture("_RemapTex", scriptableObject.MiscTextures[0]);
		componentsInChildren[3].material.SetFloat("_Boost", 1f);
		componentsInChildren[3].material.SetFloat("_AlphaBoost", 3f);
		muzzleFlashEffect.GetComponentInChildren<Light>().color = new Color32(249, 23, byte.MaxValue, byte.MaxValue);
		ContentAddition.AddEffect(muzzleFlashEffect);
		GameObject ghostPrefab = scriptableObject.Projectiles[0].GetComponent<ProjectileController>().ghostPrefab;
		ghostPrefab = scriptableObject.Effects[3];
		ghostPrefab.transform.GetChild(2).GetComponent<ParticleSystemRenderer>().material = SpikestripUtil.AddressablesLoad<Material>("cc20cbe750bfc654e871cd6df71758df");
		ghostPrefab.transform.GetChild(0).GetComponent<MeshRenderer>().materials = new Material[1] { SpikestripUtil.AddressablesLoad<Material>("1501cfe00570370439ba4b8b88565b12") };
		ghostPrefab.transform.GetComponentInChildren<PostProcessVolume>().profile = SpikestripUtil.AddressablesLoad<PostProcessProfile>("de37d74cae517ba4ca557bf528fd331f");
		ghostPrefab.transform.GetChild(5).GetComponent<ParticleSystemRenderer>().material = componentsInChildren[3].material;
		scriptableObject.Projectiles[1].GetComponent<ProjectileController>().ghostPrefab.transform.GetChild(1).GetComponent<ParticleSystemRenderer>().material = componentsInChildren[3].material;
		scriptableObject.Projectiles[2].GetComponent<ProjectileController>().ghostPrefab.transform.GetChild(1).GetComponent<ParticleSystemRenderer>().material = componentsInChildren[3].material;
		scriptableObject.Effects[2].transform.GetChild(0).GetChild(3).GetComponent<MeshRenderer>()
			.material = SpikestripUtil.AddressablesLoad<Material>("1501cfe00570370439ba4b8b88565b12");
		scriptableObject.Effects[4].transform.GetChild(1).GetComponent<ParticleSystemRenderer>().material = componentsInChildren[3].material;
	}
}
