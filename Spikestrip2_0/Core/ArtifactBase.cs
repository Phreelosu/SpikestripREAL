using R2API;
using RoR2;
using UnityEngine;

namespace Spikestrip2_0.Core
{
  public abstract class ArtifactBase<T> : ContentBase<T> where T : ArtifactBase<T>
  {
    public ArtifactDef artifactDef;

    public override string ConfigSection => "Artifacts";

    public override string ConfigName => GetFullName();

    public abstract string Name { get; }

    public abstract string Description { get; }

    public abstract Sprite EnabledIcon { get; }

    public abstract Sprite DisabledIcon { get; }

    public virtual string OverrideFullName => null;

    public override void Init()
    {
      artifactDef = ScriptableObject.CreateInstance<ArtifactDef>();
      artifactDef.cachedName = Name;
      artifactDef.nameToken = "SPIKESTRIPARTIFACT_" + Name.ToUpper() + "_NAME";
      artifactDef.descriptionToken = "SPIKESTRIPARTIFACT_" + Name.ToUpper() + "_DESCRIPTION";
      artifactDef.smallIconSelectedSprite = EnabledIcon;
      artifactDef.smallIconDeselectedSprite = DisabledIcon;
      artifactDef.requiredExpansion = RequiredExpansionHolder;
      ContentAddition.AddArtifactDef(artifactDef);
      LanguageAPI.Add(artifactDef.nameToken, GetFullName());
      LanguageAPI.Add(artifactDef.descriptionToken, Description);
      RunArtifactManager.onArtifactEnabledGlobal += OnArtifactEnabled;
      RunArtifactManager.onArtifactDisabledGlobal += OnArtifactDisabled;
    }

    private void OnArtifactEnabled(RunArtifactManager runArtifactManager, ArtifactDef enabledArtifact)
    {
      if (enabledArtifact == artifactDef)
      {
        SetHooks();
      }
    }

    private void OnArtifactDisabled(RunArtifactManager runArtifactManager, ArtifactDef disabledArtifact)
    {
      if (disabledArtifact == artifactDef)
      {
        UnsetHooks();
      }
    }

    public abstract void SetHooks();

    public abstract void UnsetHooks();

    public bool ArtifactActive()
    {
      return RunArtifactManager.instance && RunArtifactManager.instance.IsArtifactEnabled(artifactDef);
    }

    public string GetFullName()
    {
      return string.IsNullOrEmpty(OverrideFullName) ? "Artifact of " + Name : OverrideFullName;
    }
  }
}
