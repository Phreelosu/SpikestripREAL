using GrooveSaladSpikestripContent;
using PlasmaCoreSpikestripContent.Content.Monsters.States;
using PlasmaCoreSpikestripContent.Core;
using R2API;
using RoR2;
using RoR2.Audio;
using RoR2.Skills;
using UnityEngine;
using Spikestrip2_0.Core;

namespace PlasmaCoreSpikestripContent.Content.Monsters;

public class SigmaConstruct : MonsterBase<SigmaConstruct>
{
	public SSEnemyBaseSO scriptableObject;

	public override string MonsterName => "Sigma Construct";

	public override void Init()
	{
		scriptableObject = PlasmaCorePlugin.enemyBaseScriptableObjects[MonsterName];
		base.Init();
		LanguageAPI.Add("CHARACTER_SIGMACONSTRUCT_NAME", "Sigma Construct");
		LanguageAPI.Add("CHARACTER_SIGMACONSTRUCT_SUBTITLE", "Horde of Many");
		LanguageAPI.Add("CHARACTER_SIGMACONSTRUCT_LORE", "========================================\r\n<style=cMono>====   MyBabel Machine Translator   ====\r\n====    [Version 12.45.1.009 ]   ======\r\n========================================\r\nTraining… <100000000 cycles>\r\nTraining… <100000000 cycles>\r\nTraining... <100000000 cycles>\r\nTraining... <104819 cycles>\r\nComplete!\r\nDisplay result? Y/N\r\nY\r\n========================================</style>\r\n\r\nDETECTING LOW ENERGY SIGNAL FROM NEARBY QUADRANT.\r\n\r\nDETECTING UHF COMMUNICATIONS FROM NEARBY QUADRANT.\r\n\r\nOPENING LINE. UNITS UNKNOWN\r\n\r\nMAKING NOTE OF POTENTIAL HOSTILE.\r\n\r\nLISTENING\r\n\r\nVERIFYING HISTORY SLATES\r\n\r\nUNBELIEVABLE\r\n\r\nDESERVED\r\n\r\nLINE CLOSED");
		LanguageAPI.Add("UNLOCKABLE_LOG_SIGMACONSTRUCT", "Monster Log: Sigma Construct");
		SkillFamily[] skillFamilies = scriptableObject.SkillFamilies;
		foreach (SkillFamily item in skillFamilies)
		{
			ContentAddition.AddSkillFamily(item);
		}
		SkillDef[] skills = scriptableObject.Skills;
		foreach (SkillDef item2 in skills)
		{
			ContentAddition.AddSkillDef(item2);
		}
		GameObject[] effects = scriptableObject.Effects;
		foreach (GameObject effectPrefab in effects)
		{
			ContentAddition.AddEffect(effectPrefab);
		}
		effects = scriptableObject.Projectiles;
		foreach (GameObject item3 in effects)
		{
			ContentAddition.AddProjectile(item3);
		}
		BuffDef[] buffs = scriptableObject.Buffs;
		foreach (BuffDef item4 in buffs)
		{
			ContentAddition.AddBuffDef(item4);
		}
		ContentAddition.AddUnlockableDef(scriptableObject.logUnlock);
		ContentAddition.AddEntityState(typeof(SigmaBeam), out _);
		ContentAddition.AddEntityState(typeof(Burrow), out _);
		ContentAddition.AddEntityState(typeof(Closed), out _);
		ContentAddition.AddEntityState(typeof(Opened), out _);
		SigmaBeam.laserEffect = scriptableObject.MiscGameObjects[0];
		SigmaBeam.muzzleFlashEffect = SpikestripUtil.AddressablesLoad<GameObject>("291e246b4f79c9e40901a4ecb7a1d543");
		SigmaBeam.hitEffectPrefab = SpikestripUtil.AddressablesLoad<GameObject>("75a9cf01972ef924d9c434318a2b4483");
		SigmaBeam.loopSoundDef = SpikestripUtil.AddressablesLoad<LoopSoundDef>("a4aab1b77369a9b418fd2e6e6a3a856a");
		Burrow.effect = scriptableObject.Effects[0];
		BodyPrefab.GetComponentInChildren<HurtBoxGroup>().OnValidate();
		AddMonster(SigmaConstruct.instance.scriptableObject.CharacterCategory, "FBLScene");
		AddMonster(SigmaConstruct.instance.scriptableObject.CharacterCategory, "forgottenhaven");
		AddMonster(SigmaConstruct.instance.scriptableObject.CharacterCategory, CommonStages.Simulacrum("skymeadow"));
	}

	public override void ModifySpawnCard()
	{
		base.ModifySpawnCard();
		CharacterSpawnCard = scriptableObject.CharacterSpawnCard;
	}

	public override void ModifyDirectorCardAndAddInteractable()
	{
		base.ModifyDirectorCardAndAddInteractable();
		DirectorCard.selectionWeight = scriptableObject.SelectionWeight;
		DirectorCard.preventOverhead = false;
		DirectorCard.spawnDistance = scriptableObject.spawnDistance;
		DirectorCard.minimumStageCompletions = scriptableObject.MinimumStageCompletion;
		AddMonster(scriptableObject.CharacterCategory, "golemplains");
		AddMonster(scriptableObject.CharacterCategory, "golemplains2");
		AddMonster(scriptableObject.CharacterCategory, "sulfurpools");
		AddMonster(scriptableObject.CharacterCategory, "arena");
		AddMonster(scriptableObject.CharacterCategory, "skymeadow");
	}

	public override void AssignPrefab()
	{
		BodyPrefab = scriptableObject.CharacterBody;
		MasterPrefab = scriptableObject.CharacterMaster;
		BodyPrefab.GetComponent<ModelLocator>().modelTransform.GetComponent<CharacterModel>().itemDisplayRuleSet = SigmaConstructItemDisplays.RegisterDisplays();
		BodyPrefab.GetComponent<HealthComponent>().health = BodyPrefab.GetComponent<CharacterBody>().baseMaxHealth;
	}

	public override void SetHooks()
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Expected O, but got Unknown
		RecalculateStatsAPI.GetStatCoefficients += new RecalculateStatsAPI.StatHookEventHandler(RecalculateStatsAPI_GetStatCoefficients);
	}

	private void RecalculateStatsAPI_GetStatCoefficients(CharacterBody sender, RecalculateStatsAPI.StatHookEventArgs args)
	{
		if (sender.HasBuff(scriptableObject.Buffs[0]))
		{
			args.armorAdd += 400f;
		}
	}
}
