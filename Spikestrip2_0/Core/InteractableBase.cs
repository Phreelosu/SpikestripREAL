using R2API;
using RoR2;
using RoR2.Navigation;
using UnityEngine;

namespace Spikestrip2_0.Core
{
  public abstract class InteractableBase<T> : ContentBase<T> where T : InteractableBase<T>
  {
    public GameObject InteractablePrefab;

    public InteractableSpawnCard InteractableSpawnCard;

    public DirectorCard DirectorCard;

    public override string ConfigSection => "Interactables";

    public override string ConfigName => InteractableName;

    public abstract string InteractableName { get; }

    public virtual bool UsesDirector => true;

    public override void Init()
    {
      SetHooks();
      AssignPrefab();
      if (InteractablePrefab)
      {
        SpikestripUtil.RequireExpansion(InteractablePrefab);
        PrefabAPI.RegisterNetworkPrefab(InteractablePrefab);
      }
      if (UsesDirector)
      {
        InteractableSpawnCard = ScriptableObject.CreateInstance<InteractableSpawnCard>();
        ModifySpawnCard();
        DirectorCard = new DirectorCard();
        ModifyDirectorCardAndAddInteractable();
      }
    }

    public abstract void AssignPrefab();

    public abstract void SetHooks();

    public virtual void ModifySpawnCard()
    {
      InteractableSpawnCard.name = "isc" + InteractableName.Replace(" ", "");
      InteractableSpawnCard.prefab = InteractablePrefab;
      InteractableSpawnCard.sendOverNetwork = true;
      InteractableSpawnCard.nodeGraphType = MapNodeGroup.GraphType.Ground;
    }

    public virtual void ModifyDirectorCardAndAddInteractable()
    {
      DirectorCard.spawnCard = InteractableSpawnCard;
    }

    public void AddInteractable(string categoryName, string stageName = null)
    {
      DirectorUtil.AddInteractable(DirectorCard, categoryName, stageName);
    }
  }
}
