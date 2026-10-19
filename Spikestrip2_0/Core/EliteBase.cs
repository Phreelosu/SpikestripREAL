using HG;
using R2API;
using RoR2;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

namespace Spikestrip2_0.Core
{
  public abstract class EliteBase<T> : EquipmentBase<T> where T : EliteBase<T>
  {
    public enum TierType
    {
      TierOne,
      Honor,
      TierTwo,
      Lunar
    }

    public struct EliteTierDefinition
    {
      public TierType tierType;

      public float healthBoostCoefficient;

      public float damageBoostCoefficient;
    }

    public struct ItemDisplayParent
    {
      public UnityEngine.Object parentAsset;

      public GameObject prefab;
    }

    public static EliteTierDefinition tierOneEliteDefault = new EliteTierDefinition
    {
      tierType = TierType.TierOne,
      healthBoostCoefficient = 4f,
      damageBoostCoefficient = 2f
    };

    public static EliteTierDefinition tierTwoEliteDefault = new EliteTierDefinition
    {
      tierType = TierType.TierTwo,
      healthBoostCoefficient = 18f,
      damageBoostCoefficient = 6f
    };

    public static EliteTierDefinition honorEliteDefault = new EliteTierDefinition
    {
      tierType = TierType.Honor,
      healthBoostCoefficient = 2.5f,
      damageBoostCoefficient = 1.5f
    };

    public BuffDef AffixBuff;

    public EliteDef MainEliteDef;

    public List<EliteDef> EliteDefs = new List<EliteDef>();

    public List<ItemDisplayParent> itemDisplayParents = new List<ItemDisplayParent>();

    public abstract string EliteName { get; }

    public abstract string AffixDescriptionMainWord { get; }

    public abstract Color EliteColor { get; }

    public abstract Sprite AffixBuffSprite { get; }

    public abstract EliteTierDefinition MainEliteTierDefinition { get; }

    public abstract Texture2D EliteRampTexture { get; }

    public virtual Type AffixBuffBehaviour => null;

    public virtual bool ServerOnlyAffixBuffBehaviour => true;

    public virtual bool HookOnHitEnemy => true;

    public virtual EliteTierDefinition[] ExtraEliteTierDefitions => Array.Empty<EliteTierDefinition>();

    public override string ConfigName => EliteName;

    public override string ConfigSection => "Elites";

    public override bool CanActivate => false;

    public override bool EnigmaCompatible => false;

    public override BuffDef PassiveBuff => AffixBuff;

    public override bool CanDrop => false;

    public override string EquipmentPickup => "Become an aspect of " + AffixDescriptionMainWord + ".";

    public override string EquipmentDescription => "Become an aspect of " + AffixDescriptionMainWord + ".";

    public override string EquipmentLore => null;

    public override string EquipmentToken => "AFFIX" + EliteName.ToUpper();

    public override bool CanBeRandomlyTriggered => false;

    public override float DropOnDeathChance => 0.00025f;

    public override void Init()
    {
      AffixBuff = ScriptableObject.CreateInstance<BuffDef>();
      base.Init();
      string modifierToken = "ELITE_MODIFIER_" + EliteName.ToUpper();
      LanguageAPI.Add(modifierToken, EliteName + " {0}");

      MainEliteDef = AddElite(MainEliteTierDefinition, modifierToken);
      foreach (EliteTierDefinition extraTier in ExtraEliteTierDefitions)
      {
        AddElite(extraTier, modifierToken);
      }

      AffixBuff.name = "bdElite" + EliteName.Replace(" ", "");
      AffixBuff.buffColor = EliteColor;
      AffixBuff.canStack = false;
      AffixBuff.eliteDef = MainEliteDef;
      AffixBuff.iconSprite = AffixBuffSprite;
      AffixBuff.isDebuff = false;
      ContentAddition.AddBuffDef(AffixBuff);
      if (AffixBuffBehaviour != null)
      {
        BuffBehaviours.Register(AffixBuff, AffixBuffBehaviour, ServerOnlyAffixBuffBehaviour);
      }
      if (itemDisplayParents.Count != 0)
      {
        On.RoR2.BodyCatalog.SetBodyPrefabs += CopyItemDisplayRules;
      }
      if (HookOnHitEnemy)
      {
        On.RoR2.GlobalEventManager.OnHitEnemy += OnHitEffect;
      }
    }

    private EliteDef AddElite(EliteTierDefinition tier, string modifierToken)
    {
      EliteDef eliteDef = ScriptableObject.CreateInstance<EliteDef>();
      eliteDef.name = "ed" + EliteName.Replace(" ", "") + tier.tierType;
      eliteDef.modifierToken = modifierToken;
      eliteDef.color = EliteColor;
      eliteDef.eliteEquipmentDef = EquipmentDef;
      eliteDef.healthBoostCoefficient = tier.healthBoostCoefficient;
      eliteDef.damageBoostCoefficient = tier.damageBoostCoefficient;
      EliteAPI.Add(new CustomElite(eliteDef, TierDefs(tier.tierType), EliteRampTexture));
      EliteDefs.Add(eliteDef);
      return eliteDef;
    }

    // Lazy is purposeful here
    private static IEnumerable<CombatDirector.EliteTierDef> TierDefs(TierType tierType)
    {
      VanillaEliteTier vanillaTier = tierType switch
      {
        TierType.TierOne => VanillaEliteTier.BaseTier1,
        TierType.Honor => VanillaEliteTier.BaseTier1Honor,
        TierType.TierTwo => VanillaEliteTier.Tier2,
        _ => VanillaEliteTier.Lunar
      };
      foreach (CombatDirector.EliteTierDef tierDef in EliteAPI.GetEliteTierEnumerable(vanillaTier))
      {
        if (tierDef != null)
        {
          yield return tierDef;
        }
      }
    }

    private void OnHitEffect(On.RoR2.GlobalEventManager.orig_OnHitEnemy orig, GlobalEventManager self, DamageInfo damageInfo, GameObject victim)
    {
      orig(self, damageInfo, victim);
      if (NetworkServer.active && damageInfo.procCoefficient > 0f && !damageInfo.rejected && damageInfo.attacker)
      {
        OnHitEnemyServer(damageInfo, victim);
      }
    }

    public virtual void OnHitEnemyServer(DamageInfo damageInfo, GameObject victim)
    {
    }

    private void CopyItemDisplayRules(On.RoR2.BodyCatalog.orig_SetBodyPrefabs orig, GameObject[] newBodyPrefabs)
    {
      foreach (GameObject bodyPrefab in newBodyPrefabs)
      {
        if (bodyPrefab)
        {
          CopyIDRFrom(bodyPrefab);
        }
      }
      orig(newBodyPrefabs);
    }

    public void CopyIDRFrom(GameObject bodyPrefab)
    {
      ModelLocator modelLocator = bodyPrefab.GetComponent<ModelLocator>();
      CharacterModel characterModel = (modelLocator && modelLocator.modelTransform) ? modelLocator.modelTransform.GetComponent<CharacterModel>() : null;
      ItemDisplayRuleSet itemDisplayRuleSet = characterModel ? characterModel.itemDisplayRuleSet : null;
      if (!itemDisplayRuleSet)
      {
        return;
      }
      List<ItemDisplayParent> remaining = new List<ItemDisplayParent>(itemDisplayParents);
      List<ItemDisplayRule> rules = new List<ItemDisplayRule>();
      for (int i = 0; i < itemDisplayRuleSet.keyAssetRuleGroups.Length && remaining.Count > 0; i++)
      {
        ItemDisplayRuleSet.KeyAssetRuleGroup group = itemDisplayRuleSet.keyAssetRuleGroups[i];
        for (int j = remaining.Count - 1; j >= 0; j--)
        {
          if (!group.keyAsset || group.keyAsset != remaining[j].parentAsset)
          {
            continue;
          }
          foreach (ItemDisplayRule rule in group.displayRuleGroup.rules)
          {
            rules.Add(new ItemDisplayRule
            {
              followerPrefab = remaining[j].prefab,
              localPos = rule.localPos,
              localAngles = rule.localAngles,
              localScale = rule.localScale,
              childName = rule.childName,
              limbMask = rule.limbMask,
              ruleType = rule.ruleType
            });
          }
          remaining.RemoveAt(j);
        }
      }
      if (rules.Count != 0)
      {
        DisplayRuleGroup displayRuleGroup = DisplayRuleGroup.empty;
        displayRuleGroup.rules = rules.ToArray();
        ArrayUtils.ArrayAppend(ref itemDisplayRuleSet.keyAssetRuleGroups, new ItemDisplayRuleSet.KeyAssetRuleGroup
        {
          keyAsset = EquipmentDef,
          displayRuleGroup = displayRuleGroup
        });
      }
    }

    public GameObject CreateAffixModel(Color color)
    {
      GameObject model = PrefabAPI.InstantiateClone(LegacyResourcesAPI.Load<GameObject>("prefabs/pickupmodels/PickupAffixRed"), "PickupAffix" + EliteName, false);
      Material material = UnityEngine.Object.Instantiate(model.GetComponentInChildren<MeshRenderer>().material);
      material.color = color;
      foreach (Renderer renderer in model.GetComponentsInChildren<Renderer>())
      {
        renderer.material = material;
      }
      return model;
    }

    public void RegisterDisplayParent(UnityEngine.Object parent, GameObject prefab = null)
    {
      itemDisplayParents.Add(new ItemDisplayParent
      {
        parentAsset = parent,
        prefab = prefab ? prefab : EquipmentDisplayModel
      });
    }

    public ItemDisplayRule[] AffixIDR(string childName, Vector3 position, Vector3 angles, Vector3 scale)
    {
      return new ItemDisplayRule[]
      {
        new ItemDisplayRule
        {
          ruleType = ItemDisplayRuleType.ParentedPrefab,
          followerPrefab = EquipmentDisplayModel,
          childName = childName,
          localPos = position,
          localAngles = angles,
          localScale = scale
        }
      };
    }
  }
}
