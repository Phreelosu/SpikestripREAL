using BepInEx;
using System;
using System.IO;
using System.Reflection;
using UnityEngine;

namespace Spikestrip2_0
{
  [BepInDependency(R2API.R2API.PluginGUID)]
  [BepInDependency("com.themysticsword.mysticsitems", BepInDependency.DependencyFlags.SoftDependency)]
  [BepInPlugin(PluginGUID, PluginName, PluginVersion)]
  public class Main : BaseUnityPlugin
  {
    public const string PluginGUID = "com.groovesalad.GrooveSaladSpikestripContent";
    public const string PluginName = "Spikestrip2_0";
    public const string PluginVersion = "1.2.0";

    internal static Main Instance { get; private set; }

    public void Awake()
    {
      Instance = this;
      Log.Init(Logger);
      AppDomain.CurrentDomain.AssemblyResolve += ResolveMergedAssembly;
      LoadSoundBank();

      Core.SpikestripContent.Init();
    }

    private static readonly string[] mergedAssemblyNames = { "PlasmaCoreSpikestripContent", "GrooveSaladSpikestripContent", "NoopSpikestripContent", "GrooveUnsharedUtils" };

    private static Assembly ResolveMergedAssembly(object sender, ResolveEventArgs args)
    {
      string name = new AssemblyName(args.Name).Name;
      return Array.IndexOf(mergedAssemblyNames, name) >= 0 ? Assembly.GetExecutingAssembly() : null;
    }

    private static void LoadSoundBank()
    {
      using Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("Spikestrip2_0.Spikestrip2.bnk");
      if (stream == null)
      {
        Log.Warning("Spikestrip2.bnk is not embedded yet, Aragonite elites will spawn silently");
        return;
      }
      byte[] bank = new byte[stream.Length];
      stream.Read(bank, 0, bank.Length);
      R2API.SoundAPI.SoundBanks.Add(bank);
    }

    public static AssetBundle LoadEmbeddedBundle(string name)
    {
      using Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("Spikestrip2_0." + name);
      if (stream == null)
      {
        Log.Error($"Missing embedded asset bundle {name}");
        return null;
      }
      using var memory = new MemoryStream();
      stream.CopyTo(memory);
      return AssetBundle.LoadFromMemory(memory.ToArray());
    }
  }

  // this legit only exists so other shit dont break on thunderstore
  [BepInPlugin("com.plasmacore.PlasmaCoreSpikestripContent", "PlasmaCoreSpikestripContent", Main.PluginVersion)]
  public class PlasmaCompatPlugin : BaseUnityPlugin
  {
  }
}
