using GrooveSaladSpikestripContent.Content;
using LookingGlass.ItemStatsNameSpace;
using NoopSpikestrip;
using PlasmaCoreSpikestripContent.Content.Equipment;
using PlasmaCoreSpikestripContent.Content.Items;
using PlasmaCoreSpikestripContent.Misc;
using RoR2;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using StatType = LookingGlass.ItemStatsNameSpace.ItemStatsDef.ValueType;
using Units = LookingGlass.ItemStatsNameSpace.ItemStatsDef.MeasurementUnits;

namespace Spikestrip2_0.Core
{
  /// <summary>
  /// Adds the extra stat lines LookingGlass shows under item descriptions. Every method touching LookingGlass
  /// types is NoInlining so nothing from LookingGlass loads unless it's installed.
  /// </summary>
  internal static class LookingGlassCompat
  {
    public const string GUID = "droppod.lookingglass";
    public static bool IsLookingGlassInstalled => BepInEx.Bootstrap.Chainloader.PluginInfos.ContainsKey(GUID);

    [MethodImpl(MethodImplOptions.NoInlining)]
    public static void Init()
    {
      ItemCatalog.availability.CallWhenAvailable(RegisterItemStats);
      EquipmentCatalog.availability.CallWhenAvailable(RegisterEquipmentStats);
    }

    private struct Line
    {
      public string description;
      public StatType valueType;
      public Units units;

      public Line(string description, StatType valueType, Units units)
      {
        this.description = description;
        this.valueType = valueType;
        this.units = units;
      }
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static ItemStatsDef Create(Line[] lines)
    {
      var def = new ItemStatsDef();
      foreach (var line in lines)
      {
        def.descriptions.Add(line.description);
        def.valueTypes.Add(line.valueType);
        def.measurementUnits.Add(line.units);
      }
      return def;
    }

    /// <summary>Stats that only depend on the stack count.</summary>
    [MethodImpl(MethodImplOptions.NoInlining)]
    private static void AddItem(ItemDef item, Func<int, List<float>> values, params Line[] lines)
    {
      var def = Create(lines);
      def.calculateValuesFlat = values;
      Register(item, def);
    }

    /// <summary>Stats with a chance that LookingGlass should adjust for luck.</summary>
    [MethodImpl(MethodImplOptions.NoInlining)]
    private static void AddChanceItem(ItemDef item, Func<float, int, List<float>> values, params Line[] lines)
    {
      var def = Create(lines);
      def.hasChance = true;
      def.calculateValuesNew = (luck, stackCount, procChance) => values(luck, stackCount);
      Register(item, def);
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static void AddEquipment(EquipmentDef equipmentDef, Func<List<float>> values, params Line[] lines)
    {
      if (!equipmentDef || equipmentDef.equipmentIndex == EquipmentIndex.None)
      {
        return;
      }
      var def = Create(lines);
      def.calculateValuesFlat = count => values();
      ItemDefinitions.allEquipmentDefinitions[(int)equipmentDef.equipmentIndex] = def;
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static void Register(ItemDef item, ItemStatsDef def)
    {
      // Disabled items never create an ItemDef.
      if (!item || item.itemIndex == ItemIndex.None)
      {
        return;
      }
      ItemDefinitions.allItemDefinitions[(int)item.itemIndex] = def;
    }

    private static float Stack(float initial, float additional, int count) => SpikestripUtil.StackScaling(initial, additional, count);

    /// <summary>Same rerolling Util.CheckRoll does for luck (LookingGlass's own helper is internal). chance is 0..1.</summary>
    private static float ChanceWithLuck(float chance, float luck)
    {
      chance = Math.Min(chance, 1f);
      int rolls = (int)Math.Ceiling(luck);
      if (rolls > 0)
      {
        return 1f - (float)Math.Pow(1f - chance, rolls + 1);
      }
      if (rolls < 0)
      {
        return (float)Math.Pow(chance, -rolls + 1);
      }
      return chance;
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static void RegisterItemStats()
    {
      try
      {
        RegisterGrooveItems();
        RegisterGasolines();
        RegisterNoopItems();
        RegisterPlasmaItems();
      }
      catch (Exception e)
      {
        Log.Error("Failed to register LookingGlass item stats: " + e);
      }
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static void RegisterGrooveItems()
    {
      AddItem(Bracelet.instance?.ItemDef, count => new List<float> { 0.1f * count },
        new Line("Crit Damage per Hit: ", StatType.Damage, Units.Percentage));

      // Each stack is 10 BoostDamage/BoostHp (+10% each) on the lemurian.
      AddItem(BlueLemurian.instance?.ItemDef, count => new List<float>
        {
          (int)(count * BlueLemurian.damageBonusPerStack + BlueLemurian.damageBonusBase),
          (int)(count * BlueLemurian.healthBonusPerStack + BlueLemurian.healthBonusBase)
        },
        new Line("Lemurian Bonus Damage: ", StatType.Damage, Units.Percentage),
        new Line("Lemurian Bonus Health: ", StatType.Healing, Units.Percentage));

      AddItem(Calendar.instance?.ItemDef, count => new List<float> { Calendar.bonusPower * count },
        new Line("Daily Bonus: ", StatType.Utility, Units.Percentage));

      AddItem(DotProcs.instance?.ItemDef, count => new List<float> { 0.5f * count },
        new Line("DoT Proc Coefficient: ", StatType.Damage, Units.Percentage));

      AddItem(HealOnSkillUse.instance?.ItemDef, count => new List<float> { 4f, 0.01f * count },
        new Line("Flat Healing: ", StatType.Healing, Units.FlatHealing),
        new Line("Healing: ", StatType.Healing, Units.PercentHealth));

      AddItem(InfinityCodex.instance?.ItemDef, count => new List<float> { 0.5f * count },
        new Line("Logged Enemy Damage: ", StatType.Damage, Units.Percentage));

      AddItem(PainBox.instance?.ItemDef, count => new List<float> { Stack(40f, 10f, count) },
        new Line("Armor: ", StatType.Armor, Units.Number));

      // A spike pile hits up to maximumHitCount times.
      AddItem(PermaSpike.instance?.ItemDef, count => new List<float>
        {
          PermaSpike.maximumHitCount * Stack(PermaSpike.PermaSpikeBehaviourServer.damageCoef, PermaSpike.PermaSpikeBehaviourServer.stackingDamageCoef, count)
        },
        new Line("Max Damage per Pile: ", StatType.Damage, Units.Percentage));

      AddItem(Arson.instance?.ItemDef, count => new List<float> { Stack(2f, 1f, count) },
        new Line("Gasolines per Stage: ", StatType.Utility, Units.Number));
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static void RegisterGasolines()
    {
      // The blast radius also gets the victim's radius added, which can't be shown here.
      GasolineFreeze freeze = GasolineFreeze.instance;
      if (freeze != null)
      {
        AddChanceItem(freeze.ItemDef, (luck, count) => new List<float>
          {
            ChanceWithLuck(Stack(freeze.ProcChanceBase, freeze.ProcChanceStack, count) * 0.01f, luck),
            Stack(freeze.RadiusBase, freeze.RadiusStack, count),
            Stack(1.5f, 0.5f, count)
          },
          new Line("Freeze Chance: ", StatType.Utility, Units.Percentage),
          new Line("Radius: ", StatType.Utility, Units.Meters),
          new Line("Freeze Duration: ", StatType.Utility, Units.Seconds));
      }

      GasolinePressure pressure = GasolinePressure.instance;
      if (pressure != null)
      {
        AddChanceItem(pressure.ItemDef, (luck, count) => new List<float>
          {
            ChanceWithLuck(Stack(pressure.ProcChanceBase, pressure.ProcChanceStack, count) * 0.01f, luck),
            Stack(pressure.RadiusBase, pressure.RadiusStack, count)
          },
          new Line("Pull Chance: ", StatType.Utility, Units.Percentage),
          new Line("Radius: ", StatType.Utility, Units.Meters));
      }

      // CrocoRegen regenerates 10% max health per second for 0.5s (+0.25s per stack).
      GasolineHeal heal = GasolineHeal.instance;
      if (heal != null)
      {
        AddItem(heal.ItemDef, count => new List<float>
          {
            Stack(heal.RadiusBase, heal.RadiusStack, count),
            0.1f * Stack(0.5f, 0.25f, count)
          },
          new Line("Radius: ", StatType.Utility, Units.Meters),
          new Line("Ally Healing: ", StatType.Healing, Units.PercentHealth));
      }

      GasolineGup gup = GasolineGup.instance;
      if (gup != null)
      {
        AddItem(gup.ItemDef, count => new List<float>
          {
            Stack(gup.RadiusBase, gup.RadiusStack, count),
            Stack(3f, 1.5f, count)
          },
          new Line("Radius: ", StatType.Utility, Units.Meters),
          new Line("Slow Duration: ", StatType.Utility, Units.Seconds));
      }
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static void RegisterNoopItems()
    {
      AddItem(Wool.instance?.ItemDef, count => new List<float> { 5f * count },
        new Line("Armor per Enemy: ", StatType.Armor, Units.Number));

      // Each buff stack is +5% bleed chance.
      AddItem(BloodyMask.instance?.ItemDef, count => new List<float>
        {
          SpikestripUtil.StackScaling(5, 3, count),
          0.05f * SpikestripUtil.StackScaling(5, 3, count)
        },
        new Line("Max Buff Stacks: ", StatType.Utility, Units.Number),
        new Line("Max Bleed Chance: ", StatType.Damage, Units.Percentage));

      // Matches the RecalculateStats IL hook and ReferenceBehavior.damageThreshold.
      AddItem(ObligiatoryReference.instance?.ItemDef, count => new List<float>
        {
          0.4f * Mathf.Pow(0.9f, count - 1),
          75f * Mathf.Pow(0.85f, count - 1)
        },
        new Line("Health to Shield: ", StatType.Health, Units.Percentage),
        new Line("Damage per Recharge: ", StatType.Damage, Units.Percentage));

      // The cartridge breaks after 10 prevented debuffs no matter the stack count.
      AddChanceItem(Voiditem.instance?.ItemDef, (luck, count) => new List<float> { 10f, ChanceWithLuck(0.1f, luck) },
        new Line("Debuffs Prevented: ", StatType.Void, Units.Number),
        new Line("Blessing Chance: ", StatType.Void, Units.Percentage));

      AddItem(BLESSING.instance?.ItemDef, count => new List<float> { 0.03f * count },
        new Line("All Stats: ", StatType.Void, Units.Percentage));
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static void RegisterPlasmaItems()
    {
      // The first hit calls down a 1500% burn and marks the enemy; hits on marked enemies burn for 50% per stack.
      AddItem(SunSword.instance?.ItemDef, count => new List<float> { SunSwordOrb.sunSwordDamageMult, 0.5f * count, 7f },
        new Line("Sun Strike Damage: ", StatType.Damage, Units.Percentage),
        new Line("Marked Burn Damage: ", StatType.Damage, Units.Percentage),
        new Line("Cooldown: ", StatType.Utility, Units.Seconds));
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static void RegisterEquipmentStats()
    {
      try
      {
        AddEquipment(DoubleItemsEquip.instance?.EquipmentDef, () => new List<float> { DoubleItemsEquip.buffDuration },
          new Line("Duration: ", StatType.Utility, Units.Seconds));

        AddEquipment(MultiTargetEquip.instance?.EquipmentDef, () => new List<float> { MultiTargetEquip.damageCoef },
          new Line("Damage per Target: ", StatType.Damage, Units.Percentage));

        AddEquipment(ImpulseFrostShield.instance?.EquipmentDef, () => new List<float> { 4f, 0.1f },
          new Line("Freeze Duration: ", StatType.Utility, Units.Seconds),
          new Line("Damage: ", StatType.Damage, Units.PercentHealth));
      }
      catch (Exception e)
      {
        Log.Error("Failed to register LookingGlass equipment stats: " + e);
      }
    }
  }
}
