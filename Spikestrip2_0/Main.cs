using BepInEx;
using System;
using System.IO;
using System.Reflection;
using UnityEngine;

namespace Spikestrip2_0
{
  [BepInDependency(R2API.R2API.PluginGUID)]
  // Every R2API module the mod uses, so BepInEx always loads them first
  [BepInDependency(R2API.ArtifactCodeAPI.PluginGUID)]
  [BepInDependency(R2API.ColorsAPI.PluginGUID)]
  [BepInDependency(R2API.Utils.CommandHelper.PluginGUID)]
  [BepInDependency(R2API.ContentManagement.R2APIContentManager.PluginGUID)]
  [BepInDependency(R2API.DeployableAPI.PluginGUID)]
  [BepInDependency(R2API.DirectorAPI.PluginGUID)]
  [BepInDependency(R2API.DotAPI.PluginGUID)]
  [BepInDependency(R2API.EliteAPI.PluginGUID)]
  [BepInDependency(R2API.ItemAPI.PluginGUID)]
  [BepInDependency(R2API.LanguageAPI.PluginGUID)]
  [BepInDependency(R2API.Networking.NetworkingAPI.PluginGUID)]
  [BepInDependency(R2API.OrbAPI.PluginGUID)]
  [BepInDependency(R2API.PrefabAPI.PluginGUID)]
  [BepInDependency(R2API.RecalculateStatsAPI.PluginGUID)]
  [BepInDependency(R2API.SoundAPI.PluginGUID)]
  [BepInDependency(R2API.TempVisualEffectAPI.PluginGUID)]
  [BepInDependency(R2API.UnlockableAPI.PluginGUID)]
  [BepInDependency("com.themysticsword.mysticsitems", BepInDependency.DependencyFlags.SoftDependency)]
  [BepInDependency(Core.LookingGlassCompat.GUID, BepInDependency.DependencyFlags.SoftDependency)]
  [BepInPlugin(PluginGUID, PluginName, PluginVersion)]
  public class Main : BaseUnityPlugin
  {
    public const string PluginGUID = "com.groovesalad.GrooveSaladSpikestripContent";
    public const string PluginName = "Spikestrip2_0";
    public const string PluginVersion = "1.2.4";

    internal static Main Instance { get; private set; }

    public void Awake()
    {
      Instance = this;
      Log.Init(Logger);
      AppDomain.CurrentDomain.AssemblyResolve += ResolveMergedAssembly;
      LoadSoundBank();

      Core.SpikestripContent.Init();

      bool lookingGlassCompat = Core.SpikestripContent.Config.Bind("Mod Compatibility: LookingGlass", "Enable Compatibility Patches?", true,
        "Add item stats to LookingGlass (if installed)?").Value;
      if (lookingGlassCompat && Core.LookingGlassCompat.IsLookingGlassInstalled)
      {
        Core.LookingGlassCompat.Init();
      }
    }

    private static readonly string[] mergedAssemblyNames = { "PlasmaCoreSpikestripContent", "GrooveSaladSpikestripContent", "NoopSpikestripContent", "GrooveUnsharedUtils" };

    private static Assembly ResolveMergedAssembly(object sender, ResolveEventArgs args)
    {
      string name = new AssemblyName(args.Name).Name;
      return Array.IndexOf(mergedAssemblyNames, name) >= 0 ? Assembly.GetExecutingAssembly() : null;
    }

    // Bundles and the soundbank ship as files next to the dll. LoadFromFile streams from disk, where embedded
    // resources had to be copied into managed memory and kept there for LoadFromMemory.
    private static string AssetPath(string name) => Path.Combine(Path.GetDirectoryName(Instance.Info.Location), name);

    private static void LoadSoundBank()
    {
      string path = AssetPath("Spikestrip2.bnk");
      if (!File.Exists(path))
      {
        Log.Warning($"Missing soundbank {path}, Aragonite elites will spawn silently");
        return;
      }
      R2API.SoundAPI.SoundBanks.Add(path);
    }

    public static AssetBundle LoadBundle(string name)
    {
      string path = AssetPath(name);
      if (!File.Exists(path))
      {
        Log.Error($"Missing asset bundle {path}");
        return null;
      }
      return AssetBundle.LoadFromFile(path);
    }
  }

  // this legit only exists so other shit dont break on thunderstore
  [BepInPlugin("com.plasmacore.PlasmaCoreSpikestripContent", "PlasmaCoreSpikestripContent", Main.PluginVersion)]
  public class PlasmaCompatPlugin : BaseUnityPlugin
  {
  }
}
