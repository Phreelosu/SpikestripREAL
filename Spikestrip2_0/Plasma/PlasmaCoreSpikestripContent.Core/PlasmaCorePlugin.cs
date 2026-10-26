using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using BepInEx;
using EntityStates;
using GrooveSaladSpikestripContent;
using On.RoR2;
using On.RoR2.CharacterAI;
using PlasmaCoreSpikestripContent.Misc;
using PlasmaCoreSpikestripContent.Networking;
using R2API.Networking;
using R2API.Utils;
using RoR2;
using RoR2.CharacterAI;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using R2API;
using Spikestrip2_0.Core;

namespace PlasmaCoreSpikestripContent.Core;

public static class PlasmaCorePlugin
{
	public static AssetBundle bundle;

	public static GameObject skyShardProjectile;

	public static Dictionary<string, SSItemBaseSO> itemBaseScriptableObjects = new Dictionary<string, SSItemBaseSO>();

	public static Dictionary<string, SSEquipBaseSO> equipBaseScriptableObjects = new Dictionary<string, SSEquipBaseSO>();

	public static Dictionary<string, SSArtifactBaseSO> artifactBaseScriptableObjects = new Dictionary<string, SSArtifactBaseSO>();

	public static Dictionary<string, SSInteractibleBaseSO> interactibleBaseScriptableObjects = new Dictionary<string, SSInteractibleBaseSO>();

	public static Dictionary<string, SSSkillBaseSO> skillBaseScriptableObjects = new Dictionary<string, SSSkillBaseSO>();

	public static Dictionary<string, SSEnemyBaseSO> enemyBaseScriptableObjects = new Dictionary<string, SSEnemyBaseSO>();

	public static Dictionary<string, SSEliteBaseSO> eliteBaseScriptableObjects = new Dictionary<string, SSEliteBaseSO>();

	public static GameObject crosshairIFSActive;

	public static GameObject crosshairIFSInactive;

	public static GameObject freezeEffectIFS;

	public static void Init()
	{
		NetworkingAPI.RegisterMessageType<IFSClientTargetRequestNetworkMessage>();
		NetworkingAPI.RegisterMessageType<IFSTargetConfirmRequestNetworkMessage>();
		NetworkingAPI.RegisterMessageType<IFSACtivateEquipmentNetworkMessage>();
		NetworkingAPI.RegisterMessageType<ApplyWeaveDebuffNetworkMessage>();
		bundle = Spikestrip2_0.Main.LoadBundle("plasmacorespikestripcontentpack.bundle");
		freezeEffectIFS = bundle.LoadAsset<GameObject>("FreezeEffect");
		ContentAddition.AddEffect(freezeEffectIFS);
		crosshairIFSActive = bundle.LoadAsset<GameObject>("IFSIndicatorActive");
		crosshairIFSInactive = bundle.LoadAsset<GameObject>("IFSIndicatorInactive");
		skyShardProjectile = bundle.LoadAsset<GameObject>("SkyShardProjectile");
		PlasmaUtils.RegisterNetworkPrefab(skyShardProjectile);
		ContentAddition.AddProjectile(skyShardProjectile);
		Material material = Addressables.LoadAssetAsync<GameObject>("e478bf9d9eb031246b7262dd8171c63e").WaitForCompletion().transform.GetChild(4).GetChild(0).GetChild(2)
			.GetChild(0)
			.GetComponent<MeshRenderer>()
			.materials[0];
		bundle.LoadAsset<GameObject>("SkyShardImpactEffect").transform.GetChild(1).GetChild(1).GetComponent<MeshRenderer>()
			.material = material;
		bundle.LoadAsset<GameObject>("SkyShardWarningGhost").transform.GetChild(0).GetChild(0).GetChild(0)
			.GetChild(1)
			.GetComponent<MeshRenderer>()
			.material = material;
		ContentAddition.AddEffect(bundle.LoadAsset<GameObject>("SkyShardImpactEffect"));
		StubShaders(bundle.LoadAllAssets<Material>());
		PlasmaLog.Log("Getting all ItemBaseSOs", allLogOnly: true);
		SSItemBaseSO[] array = bundle.LoadAllAssets<SSItemBaseSO>();
		foreach (SSItemBaseSO sSItemBaseSO in array)
		{
			PlasmaLog.Log("Adding ItemBaseSO with name " + sSItemBaseSO.ItemName, allLogOnly: true);
			itemBaseScriptableObjects.Add(sSItemBaseSO.ItemName, sSItemBaseSO);
		}
		PlasmaLog.Log("Getting all EquipBaseSOs", allLogOnly: true);
		SSEquipBaseSO[] array2 = bundle.LoadAllAssets<SSEquipBaseSO>();
		foreach (SSEquipBaseSO sSEquipBaseSO in array2)
		{
			PlasmaLog.Log("Adding EquipBaseSO with name " + sSEquipBaseSO.EquipName, allLogOnly: true);
			equipBaseScriptableObjects.Add(sSEquipBaseSO.EquipName, sSEquipBaseSO);
		}
		PlasmaLog.Log("Getting all ArtifactBaseSOs", allLogOnly: true);
		SSArtifactBaseSO[] array3 = bundle.LoadAllAssets<SSArtifactBaseSO>();
		foreach (SSArtifactBaseSO sSArtifactBaseSO in array3)
		{
			PlasmaLog.Log("Adding ArtifactBaseSO with name " + sSArtifactBaseSO.Name, allLogOnly: true);
			artifactBaseScriptableObjects.Add(sSArtifactBaseSO.Name, sSArtifactBaseSO);
		}
		PlasmaLog.Log("Getting all InteractibleBaseSOs", allLogOnly: true);
		SSInteractibleBaseSO[] array4 = bundle.LoadAllAssets<SSInteractibleBaseSO>();
		foreach (SSInteractibleBaseSO sSInteractibleBaseSO in array4)
		{
			PlasmaLog.Log("Adding InteractibleBaseSO with name " + sSInteractibleBaseSO.InteractableName, allLogOnly: true);
			interactibleBaseScriptableObjects.Add(sSInteractibleBaseSO.InteractableName, sSInteractibleBaseSO);
		}
		PlasmaLog.Log("Getting all SkillBaseSOs", allLogOnly: true);
		SSSkillBaseSO[] array5 = bundle.LoadAllAssets<SSSkillBaseSO>();
		foreach (SSSkillBaseSO sSSkillBaseSO in array5)
		{
			PlasmaLog.Log("Adding SkillBaseSO with name " + sSSkillBaseSO.Name, allLogOnly: true);
			skillBaseScriptableObjects.Add(sSSkillBaseSO.Name, sSSkillBaseSO);
		}
		PlasmaLog.Log("Getting all EnemyBaseSOs", allLogOnly: true);
		SSEnemyBaseSO[] array6 = bundle.LoadAllAssets<SSEnemyBaseSO>();
		foreach (SSEnemyBaseSO sSEnemyBaseSO in array6)
		{
			PlasmaLog.Log("Adding EnemyBaseSO with name " + sSEnemyBaseSO.Name, allLogOnly: true);
			enemyBaseScriptableObjects.Add(sSEnemyBaseSO.Name, sSEnemyBaseSO);
		}
		PlasmaLog.Log("Getting all EnemyBaseSOs", allLogOnly: true);
		SSEliteBaseSO[] array7 = bundle.LoadAllAssets<SSEliteBaseSO>();
		foreach (SSEliteBaseSO sSEliteBaseSO in array7)
		{
			PlasmaLog.Log("Adding EnemyBaseSO with name " + sSEliteBaseSO.Name, allLogOnly: true);
			eliteBaseScriptableObjects.Add(sSEliteBaseSO.Name, sSEliteBaseSO);
		}
	}

	public static void StubShaders(Material[] materials)
	{
		AsyncOperationHandle<Shader> asyncOperationHandle = Addressables.LoadAssetAsync<Shader>("48dca5b99d113b8d11006bab44295342");
		AsyncOperationHandle<Shader> asyncOperationHandle2 = Addressables.LoadAssetAsync<Shader>("bbffe49749c91724d819563daf91445d");
		AsyncOperationHandle<Shader> asyncOperationHandle3 = Addressables.LoadAssetAsync<Shader>("a035a371a79a19c468ec4e6dc40911c5");
		AsyncOperationHandle<Shader> asyncOperationHandle4 = Addressables.LoadAssetAsync<Shader>("43a6c7a9084ef9743ab45ee8d5f3c4e9");
		AsyncOperationHandle<Shader> asyncOperationHandle5 = Addressables.LoadAssetAsync<Shader>("f6bd449dcf2a4496da3d2ad0c3881450");
		AsyncOperationHandle<Shader> asyncOperationHandle6 = Addressables.LoadAssetAsync<Shader>("302e1057ea9d0e74dab5a0de5cbf611c");
		AsyncOperationHandle<Shader> asyncOperationHandle7 = Addressables.LoadAssetAsync<Shader>("94b2ede73cf555f4f8549dc24b957446");
		AsyncOperationHandle<Shader> asyncOperationHandle8 = Addressables.LoadAssetAsync<Shader>("d48a4aa52cd665f45a89801d053c38de");
		AsyncOperationHandle<Shader> asyncOperationHandle9 = Addressables.LoadAssetAsync<Shader>("cd44d5076b47fbc4d8872b2a500b78f8");
		AsyncOperationHandle<Shader> asyncOperationHandle10 = Addressables.LoadAssetAsync<Shader>("ec2c273472427df41846b25c110155c2");
		Shader shader = asyncOperationHandle.WaitForCompletion();
		Shader shader2 = asyncOperationHandle2.WaitForCompletion();
		Shader shader3 = asyncOperationHandle3.WaitForCompletion();
		Shader shader4 = asyncOperationHandle4.WaitForCompletion();
		Shader shader5 = asyncOperationHandle5.WaitForCompletion();
		Shader shader6 = asyncOperationHandle6.WaitForCompletion();
		Shader shader7 = asyncOperationHandle7.WaitForCompletion();
		Shader shader8 = asyncOperationHandle8.WaitForCompletion();
		Shader shader9 = asyncOperationHandle9.WaitForCompletion();
		Shader shader10 = asyncOperationHandle10.WaitForCompletion();
		foreach (Material material in materials)
		{
			switch (material.shader.name)
			{
			case "StubbedShader/deferred/standard":
				material.shader = shader;
				break;
			case "StubbedShader/fx/hgcloudremap":
				material.shader = shader2;
				material.EnableKeyword("USE_CLOUDS");
				material.EnableKeyword("VERTEXCOLOR");
				material.EnableKeyword("_EMISSION");
				break;
			case "StubbedShader/fx/hgopaquecloudremap":
				material.shader = shader3;
				break;
			case "StubbedShader/fx/cloudintersectionremap":
				material.shader = shader4;
				material.EnableKeyword("CLOUDOFFSET");
				material.EnableKeyword("USE_CLOUDS");
				material.EnableKeyword("_EMISSION");
				break;
			case "StubbedShader/fx/hgdistortion":
				material.shader = shader5;
				break;
			case "StubbedShader/fx/solidparallax":
				material.shader = shader6;
				break;
			case "StubbedShader/fx/hgforwardplanet":
				material.shader = shader7;
				break;
			case "StubbedShader/environment/distantwater":
				material.shader = shader8;
				break;
			case "StubbedShader/deferred/hgtriplanarterrainblend":
				material.shader = shader9;
				break;
			case "StubbedShader/deferred/hgsnowtopped":
				material.shader = shader10;
				break;
			default:
				if (PlasmaLog.debugAll && (material.shader.name.ToLower().Contains("stub") || material.shader.name.ToLower().Contains("hopoo")))
				{
					PlasmaLog.LogWarn("[VF2Plugin]: Unable to find shader for stub " + material.shader.name, allLogOnly: true);
				}
				break;
			}
		}
	}
}
