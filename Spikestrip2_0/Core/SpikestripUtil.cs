using HG;
using R2API;
using RoR2;
using System.Linq;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Rendering;

namespace Spikestrip2_0.Core
{
  public static class SpikestripUtil
  {
    public static Shader hopoothegames = LegacyResourcesAPI.Load<Shader>("Shaders/Deferred/HGStandard");

    public static Material infusionGlassMaterial = LegacyResourcesAPI.Load<GameObject>("prefabs/pickupmodels/PickupInfusion").transform.Find("mdlInfusionPickup/InfusionPickupGlass").GetComponent<MeshRenderer>().material;

    public static Material wispGlassDistortionMaterial = LegacyResourcesAPI.Load<GameObject>("prefabs/pickupmodels/PickupWilloWisp").transform.Find("mdlGlassJar/GlassJar").GetComponent<MeshRenderer>().material;

    // matArtifactShellDistortion. ArtifactShellModel's prefab renderer has no materials anymore (the skin supplies them).
    public static Material artifactShellDistortionMaterial = AddressablesLoad<Material>("2680f10508f99124abe819f13d14a4ed");

    public static GameObject nullModel = LegacyResourcesAPI.Load<GameObject>("Prefabs/NullModel");

    public static GameObject UpdateGameObjectShader(GameObject originalModel, bool dither = true)
    {
      if (!originalModel)
      {
        return null;
      }
      foreach (Renderer renderer in originalModel.GetComponentsInChildren<Renderer>())
      {
        if (renderer)
        {
          renderer.material.shader = hopoothegames;
          if (dither)
          {
            renderer.material.EnableKeyword("DITHER");
          }
        }
      }
      return originalModel;
    }

    public static CharacterModel.RendererInfo[] GenerateRendererInfos(GameObject obj, bool allRendererTypes = false)
    {
      Renderer[] renderers = allRendererTypes ? obj.GetComponentsInChildren<Renderer>() : obj.GetComponentsInChildren<MeshRenderer>();
      CharacterModel.RendererInfo[] infos = new CharacterModel.RendererInfo[renderers.Length];
      for (int i = 0; i < renderers.Length; i++)
      {
        infos[i] = new CharacterModel.RendererInfo
        {
          defaultMaterial = renderers[i].material,
          renderer = renderers[i],
          defaultShadowCastingMode = ShadowCastingMode.On,
          ignoreOverlays = false
        };
      }
      return infos;
    }

    public static NetworkSoundEventDef nseFromString(string eventName)
    {
      NetworkSoundEventDef networkSoundEventDef = ScriptableObject.CreateInstance<NetworkSoundEventDef>();
      networkSoundEventDef.eventName = eventName;
      networkSoundEventDef.name = "nse" + eventName;
      ContentAddition.AddNetworkSoundEventDef(networkSoundEventDef);
      return networkSoundEventDef;
    }

    public static Color ColorRGB(float rUnscaled, float gUnscaled, float bUnscaled, float a = 1f)
    {
      return new Color(rUnscaled / 255f, gUnscaled / 255f, bUnscaled / 255f, a);
    }

    public static void AppendArrayIfMissing<T>(ref T[] array, in T value)
    {
      if (!array.Contains(value))
      {
        ArrayUtils.ArrayAppend(ref array, in value);
      }
    }

    public static void RemoveFromArray<T>(ref T[] array, in T value)
    {
      int index = System.Array.IndexOf(array, value);
      if (index >= 0)
      {
        ArrayUtils.ArrayRemoveAtAndResize(ref array, index);
      }
    }

    public static GameObject EmptyPrefab(string name, bool registerNetwork = false)
    {
      return PrefabAPI.InstantiateClone(new GameObject(name), name, registerNetwork);
    }

    // ProjectileController prefers ghostPrefabAddress over ghostPrefab when the address is valid, so cloned
    // vanilla projectiles keep the original ghost unless the address is cleared too. Pass null for no ghost.
    public static void SetProjectileGhost(RoR2.Projectile.ProjectileController projectileController, GameObject ghostPrefab)
    {
      projectileController.ghostPrefab = ghostPrefab;
      projectileController.ghostPrefabAddress = new AssetReferenceT<GameObject>("");
    }

    public static int StackScaling(int baseValue, int stackValue, int stack)
    {
      return stack > 0 ? baseValue + (stack - 1) * stackValue : 0;
    }

    public static float StackScaling(float baseValue, float stackValue, int stack)
    {
      return stack > 0 ? baseValue + (stack - 1) * stackValue : 0f;
    }

    public static int Percent(float coefficient)
    {
      return Mathf.RoundToInt(coefficient * 100f);
    }

    public static TAsset LegacyLoad<TAsset>(string path) where TAsset : Object
    {
      return LegacyResourcesAPI.Load<TAsset>(path);
    }

    public static TAsset AddressablesLoad<TAsset>(string key) where TAsset : Object
    {
      return Addressables.LoadAssetAsync<TAsset>(key).WaitForCompletion();
    }

    public static void DisableEffectPooling(GameObject effect)
    {
      VFXAttributes attributes = effect.GetComponent<VFXAttributes>();
      if (!attributes)
      {
        attributes = effect.AddComponent<VFXAttributes>();
      }
      attributes.DoNotPool = true;
    }

    public static void BakeDefaultSkin(GameObject originalModel, GameObject clonedModel)
    {
      ModelSkinController controller = originalModel.GetComponent<ModelSkinController>();
      SkinDef skin = controller && controller.skins != null && controller.skins.Length > 0 ? controller.skins[0] : null;
      SkinDefParams skinParams = null;
      if (skin)
      {
        skinParams = skin.skinDefParams;
        if (!skinParams && skin.skinDefParamsAddress != null && skin.skinDefParamsAddress.RuntimeKeyIsValid())
        {
          skinParams = skin.skinDefParamsAddress.LoadAssetAsync().WaitForCompletion();
        }
        if (!skinParams)
        {
          skinParams = SkinDefParams.FromSkinDef(skin);
        }
      }
      if (skinParams)
      {
        foreach (SkinDefParams.MeshReplacement replacement in skinParams.meshReplacements ?? System.Array.Empty<SkinDefParams.MeshReplacement>())
        {
          Renderer renderer = CloneCounterpart(originalModel, clonedModel, replacement.renderer) as Renderer;
          Mesh mesh = replacement.mesh;
          if (!mesh && replacement.meshAddress != null && replacement.meshAddress.RuntimeKeyIsValid())
          {
            mesh = replacement.meshAddress.LoadAssetAsync().WaitForCompletion();
          }
          if (!renderer || !mesh)
          {
            continue;
          }
          if (renderer is SkinnedMeshRenderer skinnedMeshRenderer)
          {
            skinnedMeshRenderer.sharedMesh = mesh;
          }
          else if (renderer.TryGetComponent(out MeshFilter meshFilter))
          {
            meshFilter.sharedMesh = mesh;
          }
        }
        foreach (SkinDefParams.GameObjectActivation activation in skinParams.gameObjectActivations ?? System.Array.Empty<SkinDefParams.GameObjectActivation>())
        {
          Transform target = !activation.spawnPrefabOnModelObject && activation.gameObject ? CloneCounterpart(originalModel, clonedModel, activation.gameObject.transform) as Transform : null;
          if (target)
          {
            target.gameObject.SetActive(activation.shouldActivate);
          }
        }
        foreach (CharacterModel.RendererInfo info in skinParams.rendererInfos ?? System.Array.Empty<CharacterModel.RendererInfo>())
        {
          Renderer renderer = CloneCounterpart(originalModel, clonedModel, info.renderer) as Renderer;
          Material material = info.defaultMaterial;
          if (!material && info.defaultMaterialAddress != null && info.defaultMaterialAddress.RuntimeKeyIsValid())
          {
            material = info.defaultMaterialAddress.LoadAssetAsync().WaitForCompletion();
          }
          if (renderer && material)
          {
            renderer.sharedMaterial = material;
          }
        }
      }
      // Model Animators ship without a controller and avatar too: the ModelSkinController loads them in Awake. It's
      // removed below, so bake them in as well, or the model T-poses.
      Animator clonedAnimator = clonedModel.GetComponent<Animator>();
      if (controller && clonedAnimator)
      {
        if (!clonedAnimator.runtimeAnimatorController)
        {
          clonedAnimator.runtimeAnimatorController = LoadSkinControllerAsset<RuntimeAnimatorController>(controller, "_animatorControllerAddress", "_animatorController");
        }
        if (!clonedAnimator.avatar)
        {
          clonedAnimator.avatar = LoadSkinControllerAsset<Avatar>(controller, "_avatarAddress", "_avatar");
        }
      }
      ModelSkinController clonedController = clonedModel.GetComponent<ModelSkinController>();
      if (clonedController)
      {
        Object.DestroyImmediate(clonedController);
      }
    }

    // Reads the ModelSkinController's (internal) address field without going through its animatorControllerRef /
    // avatarRef properties, which would start a load on the vanilla prefab. The handle is never released on purpose:
    // the baked prefab keeps referencing the asset.
    private static T LoadSkinControllerAsset<T>(ModelSkinController controller, string addressField, string directField) where T : Object
    {
      const System.Reflection.BindingFlags flags = System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic;
      if (typeof(ModelSkinController).GetField(addressField, flags)?.GetValue(controller) is AssetReferenceT<T> address && address.RuntimeKeyIsValid())
      {
        T asset = Addressables.LoadAssetAsync<T>(address.RuntimeKey).WaitForCompletion();
        if (asset)
        {
          return asset;
        }
      }
      return typeof(ModelSkinController).GetField(directField, flags)?.GetValue(controller) as T;
    }

    private static Component CloneCounterpart(GameObject originalModel, GameObject clonedModel, Component original)
    {
      if (!original || !original.transform.IsChildOf(originalModel.transform))
      {
        return null;
      }
      string path = PathFrom(originalModel.transform, original.transform);
      Transform target = path.Length == 0 ? clonedModel.transform : clonedModel.transform.Find(path);
      return target ? target.GetComponent(original.GetType()) : null;
    }

    private static string PathFrom(Transform root, Transform child)
    {
      string path = "";
      for (Transform current = child; current != root; current = current.parent)
      {
        path = path.Length == 0 ? current.name : current.name + "/" + path;
      }
      return path;
    }

    public static void FillInertSkillDef(RoR2.Skills.SkillDef skillDef)
    {
      if (string.IsNullOrWhiteSpace(skillDef.activationStateMachineName))
      {
        skillDef.activationStateMachineName = "SpikestripInert";
      }
      if (skillDef.activationState.stateType == null)
      {
        skillDef.activationState = new EntityStates.SerializableEntityStateType(typeof(EntityStates.Idle));
      }
    }

    public static void RequireExpansion(GameObject prefab)
    {
      if (!prefab)
      {
        return;
      }
      RoR2.ExpansionManagement.ExpansionRequirementComponent requirement = prefab.GetComponent<RoR2.ExpansionManagement.ExpansionRequirementComponent>();
      if (!requirement)
      {
        requirement = prefab.AddComponent<RoR2.ExpansionManagement.ExpansionRequirementComponent>();
      }
      requirement.requiredExpansion = SpikestripContent.ExpansionDef;
    }
  }
}
