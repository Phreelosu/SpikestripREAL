using R2API;
using RoR2;

namespace Spikestrip2_0.Core
{
  public static class DirectorUtil
  {
    public static void AddMonster(DirectorCard card, string categoryName, string stageName)
    {
      DirectorAPI.DirectorCardHolder holder = new DirectorAPI.DirectorCardHolder { Card = card };
      holder.MonsterCategory = categoryName switch
      {
        "Basic Monsters" => DirectorAPI.MonsterCategory.BasicMonsters,
        "Minibosses" => DirectorAPI.MonsterCategory.Minibosses,
        "Champions" => DirectorAPI.MonsterCategory.Champions,
        "Special" => DirectorAPI.MonsterCategory.Special,
        _ => DirectorAPI.MonsterCategory.Custom
      };
      if (holder.MonsterCategory == DirectorAPI.MonsterCategory.Custom)
      {
        holder.CustomMonsterCategory = categoryName;
      }
      if (string.IsNullOrEmpty(stageName))
      {
        DirectorAPI.Helpers.AddNewMonster(holder, false);
      }
      else
      {
        DirectorAPI.Helpers.AddNewMonsterToStage(holder, false, DirectorAPI.ParseInternalStageName(stageName), stageName);
      }
    }

    public static void AddInteractable(DirectorCard card, string categoryName, string stageName)
    {
      DirectorAPI.DirectorCardHolder holder = new DirectorAPI.DirectorCardHolder { Card = card };
      holder.InteractableCategory = categoryName switch
      {
        "Chests" => DirectorAPI.InteractableCategory.Chests,
        "Barrels" => DirectorAPI.InteractableCategory.Barrels,
        "Shrines" => DirectorAPI.InteractableCategory.Shrines,
        "Drones" => DirectorAPI.InteractableCategory.Drones,
        "Misc" => DirectorAPI.InteractableCategory.Misc,
        "Rare" => DirectorAPI.InteractableCategory.Rare,
        "Duplicator" => DirectorAPI.InteractableCategory.Duplicator,
        _ => DirectorAPI.InteractableCategory.Custom
      };
      if (holder.InteractableCategory == DirectorAPI.InteractableCategory.Custom)
      {
        holder.CustomInteractableCategory = categoryName;
      }
      if (string.IsNullOrEmpty(stageName))
      {
        DirectorAPI.Helpers.AddNewInteractable(holder);
      }
      else
      {
        DirectorAPI.Helpers.AddNewInteractableToStage(holder, DirectorAPI.ParseInternalStageName(stageName), stageName);
      }
    }
  }
}
