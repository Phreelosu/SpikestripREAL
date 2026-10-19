using R2API;
using RoR2;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

namespace Spikestrip2_0.Core
{
  public abstract class ItemBase<T> : ContentBase<T>, ItemBehaviourUpdater.IItem where T : ItemBase<T>
  {
    public class EnhancedItemBehaviour : CharacterBody.ItemBehavior
    {
      public virtual void OnStackChanged(bool increase)
      {
      }
    }

    public ItemDef ItemDef;

    public GameObject ItemPickupModel;

    public GameObject ItemDisplayModel;

    public ItemDisplayRuleDict idrs = new ItemDisplayRuleDict(Array.Empty<ItemDisplayRule>());

    public override string ConfigName => ItemName;

    public override string ConfigSection => "Items";

    public abstract string ItemName { get; }

    public abstract string ItemToken { get; }

    public abstract string ItemPickup { get; }

    public abstract string ItemDescription { get; }

    public abstract string ItemLore { get; }

    public abstract ItemTier Tier { get; }

    public abstract Sprite ItemIconSprite { get; }

    public virtual ItemTierDef OverrideTierDef => null;

    public virtual bool CanRemove => true;

    public virtual bool Hidden => false;

    public virtual ItemDef[] ItemsToCorrupt => Array.Empty<ItemDef>();

    public virtual UnlockableDef UnlockableDef => null;

    public virtual ItemTag[] ItemTags => Array.Empty<ItemTag>();

    public virtual Type ItemBehaviour => null;

    public virtual bool ServerOnlyBehaviour => false;

    public abstract void PopulateItemDisplayRules(ItemDisplayRuleDict itemDisplayRuleDict);

    public abstract void AssignItemModels();

    public override void Init()
    {
      AssignItemModels();
      PopulateItemDisplayRules(idrs);
      ItemDef = ScriptableObject.CreateInstance<ItemDef>();
      ItemDef.name = "ITEM_" + ItemToken;
      ItemDef.nameToken = "ITEM_" + ItemToken + "_NAME";
      ItemDef.pickupToken = "ITEM_" + ItemToken + "_PICKUP";
      ItemDef.descriptionToken = "ITEM_" + ItemToken + "_DESCRIPTION";
      ItemDef.loreToken = "ITEM_" + ItemToken + "_LORE";
      ItemDef.pickupIconSprite = ItemIconSprite;
      ItemDef.pickupModelPrefab = ItemPickupModel;
      ItemDef.canRemove = CanRemove;
      ItemDef.hidden = Hidden;
      ItemDef.tags = ItemTags;
      if (OverrideTierDef)
      {
        ItemDef._itemTierDef = OverrideTierDef;
      }
      else
      {
        ItemDef.deprecatedTier = Tier;
      }
      ItemDef.unlockableDef = UnlockableDef;
      ItemDef.requiredExpansion = RequiredExpansionHolder;
      ItemAPI.Add(new CustomItem(ItemDef, idrs));

      LanguageAPI.Add(ItemDef.nameToken, ItemName);
      LanguageAPI.Add(ItemDef.pickupToken, ItemPickup);
      LanguageAPI.Add(ItemDef.descriptionToken, ItemDescription);
      if (!string.IsNullOrEmpty(ItemLore))
      {
        LanguageAPI.Add(ItemDef.loreToken, ItemLore);
      }

      if (ItemsToCorrupt.Length != 0)
      {
        ItemRelationshipProvider provider = ScriptableObject.CreateInstance<ItemRelationshipProvider>();
        provider.name = ItemDef.name + "_CORRUPTS";
        provider.relationshipType = LegacyResourcesAPI.Load<ItemRelationshipType>("ItemRelationships/ContagiousItem");
        provider.relationships = new ItemDef.Pair[ItemsToCorrupt.Length];
        for (int i = 0; i < ItemsToCorrupt.Length; i++)
        {
          provider.relationships[i] = new ItemDef.Pair { itemDef1 = ItemsToCorrupt[i], itemDef2 = ItemDef };
        }
        ContentAddition.AddItemRelationshipProvider(provider);
      }

      if (ItemBehaviour != null)
      {
        ItemBehaviourUpdater.Register(this);
      }
    }

    public void AddItemBehavior(CharacterBody characterBody)
    {
      int itemCount = characterBody.inventory.GetItemCountEffective(ItemDef);
      CharacterBody.ItemBehavior itemBehavior = (CharacterBody.ItemBehavior)characterBody.GetComponent(ItemBehaviour);
      if (itemCount > 0)
      {
        if (!itemBehavior)
        {
          itemBehavior = (CharacterBody.ItemBehavior)characterBody.gameObject.AddComponent(ItemBehaviour);
          itemBehavior.body = characterBody;
          itemBehavior.enabled = true;
        }
        int previous = itemBehavior.stack;
        itemBehavior.stack = itemCount;
        if (itemBehavior is EnhancedItemBehaviour enhanced && previous != itemCount)
        {
          enhanced.OnStackChanged(itemCount > previous);
        }
      }
      else if (itemBehavior)
      {
        itemBehavior.stack = 0;
        if (itemBehavior is EnhancedItemBehaviour enhanced)
        {
          enhanced.OnStackChanged(increase: false);
        }
        UnityEngine.Object.Destroy(itemBehavior);
      }
    }

    void ItemBehaviourUpdater.IItem.UpdateBehaviour(CharacterBody body)
    {
      if (NetworkServer.active || !ServerOnlyBehaviour)
      {
        AddItemBehavior(body);
      }
    }
  }

  public static class ItemBehaviourUpdater
  {
    public interface IItem
    {
      void UpdateBehaviour(CharacterBody body);
    }

    private static readonly List<IItem> items = new List<IItem>();

    internal static void Register(IItem item)
    {
      if (items.Count == 0)
      {
        On.RoR2.CharacterBody.OnInventoryChanged += CharacterBody_OnInventoryChanged;
      }
      items.Add(item);
    }

    private static void CharacterBody_OnInventoryChanged(On.RoR2.CharacterBody.orig_OnInventoryChanged orig, CharacterBody self)
    {
      orig(self);
      foreach (IItem item in items)
      {
        item.UpdateBehaviour(self);
      }
    }
  }
}
