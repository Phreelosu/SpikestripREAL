using System;
using GrooveSaladSpikestripContent;
using On.RoR2;
using PlasmaCoreSpikestripContent.Core;
using R2API;
using RoR2;
using RoR2.Skills;
using UnityEngine;
using Spikestrip2_0.Core;

namespace PlasmaCoreSpikestripContent.Content.Skills;

public class DeepRot : SpikestripVoidAltSkillsBase<DeepRot>
{
	public static GameObject muzzleFlashEffect;

	public static SSSkillBaseSO scriptableObject;

	public static Material deepRotMinor;

	public static Material deepRotMajor;

	public static RoR2.DotController.DotIndex deepRotDOT;

	public override string SkillName => "「Deeprot」";

	public override string SkillDescription => scriptableObject.Description;

	public override string SkillToken => scriptableObject.SkillToken;

	public override string PrerequisiteIdentifier => scriptableObject.PrerequisiteIdentifier;

	public override string CharacterName => scriptableObject.CharacterName;

	public override string AchievementName => scriptableObject.AchievementName;

	public override Sprite SkillIcon => scriptableObject.SkillIcon;

	public override GameObject OwnerBodyPrefab => SpikestripUtil.LegacyLoad<GameObject>(scriptableObject.OwnerBodyPath);

	public override SkillFamily[] SkillFamilies => new SkillFamily[1] { SpikestripUtil.LegacyLoad<SkillFamily>(scriptableObject.SkillFamilyPath) };

	public override Type[] EntityStateTypes => new Type[0];

	public override SkillDef GetSkillDef()
	{
		SpikestripUtil.FillInertSkillDef(scriptableObject.SkillDefinition);
		return scriptableObject.SkillDefinition;
	}

	public override void Init()
	{
		//IL_0119: Unknown result type (might be due to invalid IL or missing references)
		//IL_0123: Expected O, but got Unknown
		//IL_0228: Unknown result type (might be due to invalid IL or missing references)
		//IL_0232: Expected O, but got Unknown
		PlasmaCorePlugin.skillBaseScriptableObjects.TryGetValue(SkillName, out scriptableObject);
		deepRotDOT = DotAPI.RegisterDotDef(new RoR2.DotController.DotDef
		{
			interval = 0.333f,
			damageCoefficient = 0.6666f,
			damageColorIndex = DamageColorIndex.Void,
			associatedBuff = scriptableObject.buffs[0]
		}, (DotAPI.CustomDotBehaviour)null, (DotAPI.CustomDotVisual)null);
		LanguageAPI.Add("KEYWORD_SOULROT", "<style=cKeywordName>Soulrot</style><style=cSub>Deals <style=cIsVoid>fatal damage</style> equal to <style=cIsVoid>62.5%</style> of their maximum health over 25s.</style>");
		deepRotMinor = UnityEngine.Object.Instantiate(SpikestripUtil.LegacyLoad<Material>("Materials/matPoisoned"));
		deepRotMinor.SetTexture("_RemapTex", scriptableObject.MiscTextures[0]);
		deepRotMajor = UnityEngine.Object.Instantiate(SpikestripUtil.LegacyLoad<Material>("Materials/matPoisoned"));
		deepRotMajor.SetTexture("_RemapTex", scriptableObject.MiscTextures[1]);
		ContentAddition.AddBuffDef(scriptableObject.buffs[0]);
		ContentAddition.AddBuffDef(scriptableObject.buffs[1]);
		On.RoR2.CrocoDamageTypeController.GetDamageType += CrocoDamageTypeController_GetDamageType;
		On.RoR2.GlobalEventManager.OnHitEnemy += GlobalEventManager_OnHitEnemy;
		// Every AddDot overload funnels into the ProcChainMask one.
		On.RoR2.DotController.AddDot_GameObject_float_HurtBox_DotIndex_float_ProcChainMask_Nullable1_Nullable1_Nullable1 += DotController_AddDot;
		OverlayMaterials.Register(deepRotMajor, (RoR2.CharacterModel self) => self.body.HasBuff(scriptableObject.buffs[0]));
		OverlayMaterials.Register(deepRotMinor, (RoR2.CharacterModel self) => self.body.HasBuff(scriptableObject.buffs[1]));
		TempVisualEffectAPI.AddTemporaryVisualEffect(scriptableObject.MiscGameObjects[0], (RoR2.CharacterBody body) => body.bestFitRadius, (RoR2.CharacterBody body) => body.HasBuff(scriptableObject.buffs[0]), "Head");
		TempVisualEffectAPI.AddTemporaryVisualEffect(SpikestripUtil.LegacyLoad<GameObject>("Prefabs/TemporaryVisualEffects/VoidFogMildEffect"), (RoR2.CharacterBody body) => body.bestFitRadius, (RoR2.CharacterBody body) => body.HasBuff(scriptableObject.buffs[1]) && !body.HasBuff(scriptableObject.buffs[0]));
		base.Init();
		RecalculateStatsAPI.GetStatCoefficients += new RecalculateStatsAPI.StatHookEventHandler(RecalculateStatsAPI_GetStatCoefficients);
	}

	private void RecalculateStatsAPI_GetStatCoefficients(RoR2.CharacterBody sender, RecalculateStatsAPI.StatHookEventArgs args)
	{
		int buffCount = sender.GetBuffCount(scriptableObject.buffs[1]);
		if (buffCount > 0)
		{
			args.cooldownMultAdd += (float)buffCount * 0.3f;
			args.attackSpeedMultAdd -= (float)buffCount * 0.1f;
			args.moveSpeedReductionMultAdd += (float)buffCount * 0.1f;
		}
	}

	private void DotController_AddDot(On.RoR2.DotController.orig_AddDot_GameObject_float_HurtBox_DotIndex_float_ProcChainMask_Nullable1_Nullable1_Nullable1 orig, RoR2.DotController self, GameObject attackerObject, float duration, RoR2.HurtBox hitHurtBox, RoR2.DotController.DotIndex dotIndex, float damageMultiplier, RoR2.ProcChainMask procChainMask, uint? maxStacksFromAttacker, float? totalDamage, RoR2.DotController.DotIndex? preUpgradeDotIndex)
	{
		orig(self, attackerObject, duration, hitHurtBox, dotIndex, damageMultiplier, procChainMask, maxStacksFromAttacker, totalDamage, preUpgradeDotIndex);
		RoR2.DotController.DotStack dotStack = self.dotStackList[self.dotStackList.Count - 1];
		if (dotStack.dotIndex == deepRotDOT)
		{
			dotStack.damage = Mathf.Max(self.victimBody.healthComponent.fullCombinedHealth / 100f * 2.5f * dotStack.dotDef.interval, dotStack.damage);
		}
	}

	private void GlobalEventManager_OnHitEnemy(On.RoR2.GlobalEventManager.orig_OnHitEnemy orig, RoR2.GlobalEventManager self, RoR2.DamageInfo damageInfo, GameObject victim)
	{
		DamageType damageType = DamageType.PoisonOnHit | DamageType.BlightOnHit;
		if ((damageInfo.damageType & damageType) == damageType && damageInfo.procCoefficient > 0f && !damageInfo.rejected && (bool)damageInfo.attacker && (bool)victim.GetComponent<RoR2.CharacterBody>())
		{
			RoR2.CrocoDamageTypeController component = damageInfo.attacker.GetComponent<RoR2.CrocoDamageTypeController>();
			if ((bool)component && component.passiveSkillSlot.skillNameToken == "SPIKESTRIPSKILL_DEEPROT_NAME")
			{
				damageInfo.damageType &= ~damageType;
				RoR2.CharacterBody component2 = victim.GetComponent<RoR2.CharacterBody>();
				if (!component2.HasBuff(scriptableObject.buffs[0]))
				{
					component2.AddTimedBuff(scriptableObject.buffs[1], 12f);
					component2.RecalculateStats();
					if (component2.GetBuffCount(scriptableObject.buffs[1]) >= 5)
					{
						RoR2.Util.PlaySound("Play_acrid_m2_bite_hit", victim);
						RoR2.DotController.InflictDot(victim, damageInfo.attacker, deepRotDOT, 20f, 1f, null);
					}
				}
				else
				{
					component2.AddTimedBuff(scriptableObject.buffs[1], 12f);
					component2.RecalculateStats();
				}
			}
		}
		orig(self, damageInfo, victim);
	}

	private RoR2.DamageTypeCombo CrocoDamageTypeController_GetDamageType(On.RoR2.CrocoDamageTypeController.orig_GetDamageType orig, RoR2.CrocoDamageTypeController self)
	{
		if (self.passiveSkillSlot.skillDef == scriptableObject.SkillDefinition)
		{
			return DamageType.PoisonOnHit | DamageType.BlightOnHit;
		}
		return orig(self);
	}
}
