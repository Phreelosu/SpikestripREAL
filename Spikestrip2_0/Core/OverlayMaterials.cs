using RoR2;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Spikestrip2_0.Core
{
  public static class OverlayMaterials
  {
    private struct OverlayMaterial
    {
      public Material material;

      public Func<CharacterModel, bool> activeCondition;
    }

    private static readonly List<OverlayMaterial> overlays = new List<OverlayMaterial>();

    public static void Register(Material material, Func<CharacterModel, bool> activeCondition)
    {
      if (overlays.Count == 0)
      {
        On.RoR2.CharacterModel.UpdateOverlays += CharacterModel_UpdateOverlays;
      }
      overlays.Add(new OverlayMaterial { material = material, activeCondition = activeCondition });
    }

    private static void CharacterModel_UpdateOverlays(On.RoR2.CharacterModel.orig_UpdateOverlays orig, CharacterModel self)
    {
      orig(self);
      if (!self.body)
      {
        return;
      }
      foreach (OverlayMaterial overlay in overlays)
      {
        if (self.activeOverlayCount < CharacterModel.maxOverlays && overlay.activeCondition(self))
        {
          self.currentOverlays[self.activeOverlayCount++] = overlay.material;
        }
      }
    }
  }
}
