using System;
using System.Linq;
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
        // Not DirectorAPI.Helpers.AddNewInteractable: it creates the category on stages that don't have it. A hidden
        // realm with no "Chests" then got a category holding only the 5-credit Lunar Vault, and the director spent its
        // whole budget on vaults. Like Groove's original director helper, only add to categories a stage already has.
        string category = holder.GetCategoryName();
        DirectorAPI.InteractableActions += (pool, stage) =>
        {
          if (!pool)
          {
            return;
          }
          DirectorAPI.Helpers.ForEachPoolEntryInDccsPool(pool, entry =>
          {
            if (entry.dccs && entry.dccs.categories.Any(c => string.Equals(c.name, category, StringComparison.OrdinalIgnoreCase)))
            {
              entry.dccs.AddCard(holder);
            }
          });
        };
      }
      else
      {
        DirectorAPI.Helpers.AddNewInteractableToStage(holder, DirectorAPI.ParseInternalStageName(stageName), stageName);
      }
    }
  }
}
