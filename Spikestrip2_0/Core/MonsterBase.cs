using R2API;
using RoR2;
using RoR2.Navigation;
using UnityEngine;

namespace Spikestrip2_0.Core
{
  public abstract class MonsterBase<T> : ContentBase<T> where T : MonsterBase<T>
  {
    public CharacterSpawnCard CharacterSpawnCard;

    public DirectorCard DirectorCard;

    public GameObject MasterPrefab;

    public GameObject BodyPrefab;

    public override string ConfigSection => "Monsters";

    public override string ConfigName => MonsterName;

    public abstract string MonsterName { get; }

    public virtual bool UsesDirector => true;

    public override void Init()
    {
      SetHooks();
      AssignPrefab();
      if (BodyPrefab)
      {
        SpikestripUtil.RequireExpansion(BodyPrefab);
        ContentAddition.AddBody(BodyPrefab);
      }
      if (MasterPrefab)
      {
        ContentAddition.AddMaster(MasterPrefab);
      }
      if (UsesDirector)
      {
        CharacterSpawnCard = ScriptableObject.CreateInstance<CharacterSpawnCard>();
        ModifySpawnCard();
        DirectorCard = new DirectorCard();
        ModifyDirectorCardAndAddInteractable();
      }
    }

    public abstract void AssignPrefab();

    public abstract void SetHooks();

    public virtual void ModifySpawnCard()
    {
      CharacterSpawnCard.name = "csc" + MonsterName.Replace(" ", "");
      CharacterSpawnCard.prefab = MasterPrefab;
      CharacterSpawnCard.sendOverNetwork = true;
      CharacterSpawnCard.nodeGraphType = MapNodeGroup.GraphType.Ground;
      CharacterSpawnCard.requiredFlags = NodeFlags.None;
      CharacterSpawnCard.forbiddenFlags = NodeFlags.NoCharacterSpawn;
    }

    public virtual void ModifyDirectorCardAndAddInteractable()
    {
      DirectorCard.spawnCard = CharacterSpawnCard;
    }

    public void AddMonster(string categoryName, string stageName = null)
    {
      DirectorUtil.AddMonster(DirectorCard, categoryName, stageName);
    }
  }
}
