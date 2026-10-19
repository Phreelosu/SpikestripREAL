using R2API;
using RoR2;
using RoR2.Navigation;
using UnityEngine;
using Spikestrip2_0.Core;

namespace GrooveSaladSpikestripContent.Content;

public class ShrineAegis : InteractableBase<ShrineAegis>
{
	public class ShrineAegisVisuals : MonoBehaviour
	{
		private bool available;

		private PurchaseInteraction purchaseInteraction;

		public Material emissiveMat;

		public Material dullMat;

		public Renderer shrineRenderer;

		public GameObject flashEffect;

		public void Awake()
		{
			purchaseInteraction = GetComponent<PurchaseInteraction>();
		}

		public void FixedUpdate()
		{
			if (available != purchaseInteraction.available)
			{
				available = purchaseInteraction.available;
				flashEffect.gameObject.SetActive(value: true);
				if ((bool)shrineRenderer)
				{
					shrineRenderer.material = (available ? emissiveMat : dullMat);
				}
			}
		}
	}

	public static Material matShrineAegisSymbol;

	public static GameObject SnowyInteractablePrefab;

	public static InteractableSpawnCard SnowyInteractableSpawnCard;

	public static DirectorCard SnowyDirectorCard;

	public static GameObject SandyInteractablePrefab;

	public static InteractableSpawnCard SandyInteractableSpawnCard;

	public static DirectorCard SandyDirectorCard;

	public override string InteractableName => "Shrine of Tribulation";

	public override bool UsesDirector => false;

	public override void SetHooks()
	{
	}

	public override void AssignPrefab()
	{
		Material original = SpikestripUtil.AddressablesLoad<Material>("a5ae4f63a7fc9654b966c7fbde82d6a9");
		matShrineAegisSymbol = Object.Instantiate(original);
		matShrineAegisSymbol.SetTexture("_MainTex", SpikestripContent.GrooveBundle.LoadAsset<Texture>("texShrineAegisSymbol.png"));
		matShrineAegisSymbol.SetColor("_TintColor", SpikestripUtil.ColorRGB(155f, 20f, 20f));
		LanguageAPI.Add("SHRINE_AEGIS_NAME", InteractableName);
		LanguageAPI.Add("SHRINE_AEGIS_CONTEXT", "Offer to " + InteractableName);
		string categoryName = "Rare";
		CreateAegisShrinePrefab(SpikestripUtil.LegacyLoad<GameObject>("Prefabs/NetworkedObjects/Shrines/ShrineChance"), "AegisShrine", ref InteractablePrefab, ref InteractableSpawnCard, ref DirectorCard);
		AddInteractable(categoryName, "golemplains");
		AddInteractable(categoryName, "golemplains2");
		AddInteractable(categoryName, "blackbeach");
		AddInteractable(categoryName, "blackbeach2");
		AddInteractable(categoryName, "foggyswamp");
		AddInteractable(categoryName, "sulfurpools");
		AddInteractable(categoryName, "wispgraveyard");
		AddInteractable(categoryName, "dampcavesimple");
		AddInteractable(categoryName, "shipgraveyard");
		AddInteractable(categoryName, "rootjungle");
		AddInteractable(categoryName, "skymeadow");
		AddInteractable(categoryName, "artifactworld");
		CreateAegisShrinePrefab(SpikestripUtil.AddressablesLoad<GameObject>("a232aa071f626734d9e6349ef4f984ca"), "AegisShrineSnowy", ref SnowyInteractablePrefab, ref SnowyInteractableSpawnCard, ref SnowyDirectorCard);
		DirectorUtil.AddInteractable(SnowyDirectorCard, categoryName, "snowyforest");
		DirectorUtil.AddInteractable(SnowyDirectorCard, categoryName, "frozenwall");
		CreateAegisShrinePrefab(SpikestripUtil.AddressablesLoad<GameObject>("ccb461071fd3f6d4c9bc0bd96af47721"), "AegisShrineSandy", ref SandyInteractablePrefab, ref SandyInteractableSpawnCard, ref SandyDirectorCard);
		DirectorUtil.AddInteractable(SandyDirectorCard, categoryName, "goolake");
	}

	public void CreateAegisShrinePrefab(GameObject parentPrefab, string name, ref GameObject interactablePrefab, ref InteractableSpawnCard isc, ref DirectorCard directorCard)
	{
		interactablePrefab = PrefabAPI.InstantiateClone(parentPrefab, name, true);
		SpikestripUtil.RequireExpansion(interactablePrefab);
		PurchaseInteraction component = interactablePrefab.GetComponent<PurchaseInteraction>();
		component.displayNameToken = "SHRINE_AEGIS_NAME";
		component.contextToken = "SHRINE_AEGIS_CONTEXT";
		GenericDisplayNameProvider component2 = interactablePrefab.GetComponent<GenericDisplayNameProvider>();
		component2.displayToken = "SHRINE_AEGIS_NAME";
		ShrineChanceBehavior component3 = interactablePrefab.GetComponent<ShrineChanceBehavior>();
		component3.colorShrineRewardNormal = SpikestripUtil.ColorRGB(236f, 80f, 80f);
		component3.failureChance = 0.95f;
		component3.maxPurchaseCount = 1;
		BasicPickupDropTable basicPickupDropTable = ScriptableObject.CreateInstance<BasicPickupDropTable>();
		basicPickupDropTable.tier1Weight = 0f;
		basicPickupDropTable.tier2Weight = 0f;
		basicPickupDropTable.tier3Weight = 1f;
		component3.dropTable = basicPickupDropTable;
		MeshRenderer componentInChildren = interactablePrefab.GetComponentInChildren<MeshRenderer>();
		Material material = Object.Instantiate(componentInChildren.material);
		material.shader = SpikestripUtil.hopoothegames;
		material.EnableKeyword("FORCE_SPEC");
		material.color = SpikestripUtil.ColorRGB(252f, 200f, 194f);
		Material material2 = Object.Instantiate(material);
		material2.SetTexture("_EmTex", SpikestripContent.GrooveBundle.LoadAsset<Texture>("ShrineAegisEmissive.png"));
		material2.SetFloat("_EmPower", 5f);
		material2.SetColor("_EmColor", Color.red);
		componentInChildren.material = material2;
		MeshRenderer component4 = interactablePrefab.transform.Find("Symbol").GetComponent<MeshRenderer>();
		component4.material = matShrineAegisSymbol;
		GameObject gameObject = SpikestripUtil.LegacyLoad<GameObject>("Prefabs/CharacterBodies/GolemBody").GetComponent<ModelLocator>().modelTransform.GetComponent<ChildLocator>().FindChild("Eye").gameObject;
		GameObject gameObject2 = PrefabAPI.InstantiateClone(gameObject, "ShrineAegisEyeLight", false);
		gameObject2.transform.SetParent(interactablePrefab.transform);
		gameObject2.transform.localPosition = new Vector3(0f, 4.6f, 0.2f);
		Transform transform = gameObject2.transform.Find("FireParticles");
		Transform transform2 = gameObject2.transform.Find("Line");
		transform2.SetParent(interactablePrefab.transform);
		PurchaseAvailabilityIndicator purchaseAvailabilityIndicator = interactablePrefab.AddComponent<PurchaseAvailabilityIndicator>();
		purchaseAvailabilityIndicator.indicatorObject = gameObject2;
		purchaseAvailabilityIndicator.disabledIndicatorObject = transform2.gameObject;
		ShrineAegisVisuals shrineAegisVisuals = interactablePrefab.AddComponent<ShrineAegisVisuals>();
		shrineAegisVisuals.flashEffect = transform.gameObject;
		shrineAegisVisuals.shrineRenderer = componentInChildren;
		shrineAegisVisuals.emissiveMat = material2;
		shrineAegisVisuals.dullMat = material;
		isc = ScriptableObject.CreateInstance<InteractableSpawnCard>();
		isc.name = "isc" + name;
		isc.prefab = interactablePrefab;
		isc.sendOverNetwork = true;
		isc.nodeGraphType = MapNodeGroup.GraphType.Ground;
		isc.hullSize = HullClassification.Golem;
		isc.requiredFlags = NodeFlags.None;
		isc.forbiddenFlags = NodeFlags.NoShrineSpawn;
		isc.directorCreditCost = 40;
		isc.occupyPosition = true;
		isc.eliteRules = SpawnCard.EliteRules.Default;
		isc.orientToFloor = false;
		isc.slightlyRandomizeOrientation = false;
		isc.skipSpawnWhenSacrificeArtifactEnabled = true;
		directorCard = new DirectorCard();
		directorCard.spawnCard = isc;
		directorCard.selectionWeight = 2;
	}
}
