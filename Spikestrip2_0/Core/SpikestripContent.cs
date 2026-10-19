using BepInEx;
using BepInEx.Configuration;
using R2API;
using RoR2;
using RoR2.ExpansionManagement;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace Spikestrip2_0.Core
{
  public static class SpikestripContent
  {
    public static AssetBundle GrooveBundle;
    public static AssetBundle NoopBundle;
    public static AssetBundle UnsharedBundle;

    public static ConfigFile Config;

    public static ExpansionDef ExpansionDef;

    private static readonly HashSet<Type> enabledContent = new HashSet<Type>();

    public static bool IsEnabled<T>() where T : ContentBase => enabledContent.Contains(typeof(T));

    public static void Init()
    {
      Config = new ConfigFile(System.IO.Path.Combine(Paths.ConfigPath, "Spikestrip 2.0.cfg"), true);
      GrooveBundle = Main.LoadEmbeddedBundle("groovesaladspikestripcontentassets");
      NoopBundle = Main.LoadEmbeddedBundle("Noopspikestripcontentpack.bundle");
      UnsharedBundle = Main.LoadEmbeddedBundle("grooveunsharedutilsassets");
      BuffBehaviours.Init();
      CreateExpansionDef();
      PlasmaCoreSpikestripContent.Core.PlasmaCorePlugin.Init();
      LoadContent();
    }

    private static void CreateExpansionDef()
    {
      ExpansionDef dlc1 = LegacyResourcesAPI.Load<ExpansionDef>("ExpansionDefs/DLC1");
      ExpansionDef = ScriptableObject.CreateInstance<ExpansionDef>();
      ExpansionDef.name = "SPIKESTRIPCONTENT_EXPANSION";
      ExpansionDef.nameToken = "SPIKESTRIPCONTENT_EXPANSION_NAME";
      ExpansionDef.descriptionToken = "SPIKESTRIPCONTENT_EXPANSION_DESCRIPTION";
      ExpansionDef.iconSprite = GrooveBundle.LoadAsset<Sprite>("texSpikestripExpansionIcon.png");
      ExpansionDef.disabledIconSprite = dlc1.disabledIconSprite;
      ExpansionDef.requiredEntitlement = dlc1.requiredEntitlement;
      LanguageAPI.Add(ExpansionDef.nameToken, "Spikestrip 2.0");
      LanguageAPI.Add(ExpansionDef.descriptionToken, "Adds content from the 'Spikestrip 2.0' mod to the game.");
      ContentAddition.AddExpansionDef(ExpansionDef);
    }

    private static void LoadContent()
    {
      List<ContentBase> pending = Assembly.GetExecutingAssembly().GetTypes()
        .Where(type => !type.IsAbstract && type.IsSubclassOf(typeof(ContentBase)))
        .Select(type => (ContentBase)Activator.CreateInstance(type))
        .ToList();

      bool progressed = true;
      while (pending.Count > 0 && progressed)
      {
        progressed = false;
        for (int i = pending.Count - 1; i >= 0; i--)
        {
          ContentBase content = pending[i];
          if (content.DependsOn != null && pending.Any(other => other.GetType() == content.DependsOn))
          {
            continue;
          }
          pending.RemoveAt(i);
          progressed = true;
          bool enabled = content.DependsOn != null
            ? enabledContent.Contains(content.DependsOn)
            : Config.Bind(SafeForConfig(content.ConfigSection), "Enable " + SafeForConfig(content.ConfigName), true, "Enable/Disable : " + SafeForConfig(content.ConfigName)).Value;
          if (enabled)
          {
            Log.Info($"Initializing {content.ConfigSection} module: {content.ConfigName}");
            try
            {
              content.Init();
              enabledContent.Add(content.GetType());
            }
            catch (Exception e)
            {
              Log.Error($"Failed to initialize {content.ConfigSection} module {content.ConfigName}: {e}");
            }
          }
        }
      }
      foreach (ContentBase content in pending)
      {
        Log.Error($"{content.GetType().Name} depends on {content.DependsOn?.Name}, which never loaded");
      }
    }

    private static readonly char[] invalidConfigChars = { '=', '\n', '\t', '\\', '"', '\'', '[', ']' };

    private static string SafeForConfig(string original) => new string(original.Where(c => !invalidConfigChars.Contains(c)).ToArray());
  }
}
