using System;
using EntityStates;
using R2API;
using RoR2;
using RoR2.Navigation;
using UnityEngine;
using UnityEngine.Networking;

namespace GrooveSaladSpikestripContent.Content;

public class NullifierCorpse : SpikestripInteractableBase<NullifierCorpse>
{
	public class VoidSingularityBehaviour : MonoBehaviour
	{
		private DirectorCard chosenDirectorCard = new DirectorCard
		{
			spawnCard = LegacyResourcesAPI.Load<SpawnCard>("spawncards/characterspawncards/cscNullifier"),
			selectionWeight = 1,
			spawnDistance = DirectorCore.MonsterSpawnDistance.Standard,
			preventOverhead = false
		};

		public int baseMonsterCredit = 100;

		private float monsterCredit => (float)baseMonsterCredit * Stage.instance.entryDifficultyCoefficient;

		public void Awake()
		{
			PurchaseInteraction component = GetComponent<PurchaseInteraction>();
			component.onPurchase.AddListener(OnUse);
		}

		public void OnUse(Interactor interactor)
		{
			if (NetworkServer.active)
			{
				PurchaseInteraction component = GetComponent<PurchaseInteraction>();
				component.SetAvailable(newAvailable: false);
				EntityStateMachine component2 = GetComponent<EntityStateMachine>();
				if ((bool)component2)
				{
					component2.SetNextState(new SingularityBombState());
				}
				CombatDirector component3 = GetComponent<CombatDirector>();
				component3.enabled = true;
				component3.monsterCredit += monsterCredit;
				component3.OverrideCurrentMonsterCard(chosenDirectorCard);
				component3.monsterSpawnTimer = 0f;
			}
		}
	}

	public class SingularityBombState : EntityState
	{
		public static float duration = 2f;

		public static float deathExplosionRadius = 15f;

		private GameObject deathPreExplosionInstance;

		private Transform modelTransform;

		public override void OnEnter()
		{
			base.OnEnter();
			modelTransform = base.transform.Find("mdlDeadNullifier");
			if (!modelTransform)
			{
			}
		}

		public override void FixedUpdate()
		{
			base.FixedUpdate();
			if (base.fixedAge >= duration)
			{
				EntityState.Destroy(deathPreExplosionInstance);
				if (NetworkServer.active)
				{
					FireVoidBombServer();
					EntityState.Destroy(base.gameObject);
				}
			}
		}

		private void FireVoidBombServer()
		{
			if (!modelTransform)
			{
				return;
			}
			Collider[] array = Physics.OverlapSphere(modelTransform.position, deathExplosionRadius, LayerIndex.entityPrecise.mask);
			CharacterBody[] array2 = new CharacterBody[array.Length];
			int count = 0;
			for (int i = 0; i < array.Length; i++)
			{
				CharacterBody characterBody = Util.HurtBoxColliderToBody(array[i]);
				if ((bool)characterBody && Array.IndexOf(array2, characterBody, 0, count) == -1)
				{
					array2[count++] = characterBody;
				}
			}
			CharacterBody[] array3 = array2;
			foreach (CharacterBody characterBody2 in array3)
			{
				if ((bool)characterBody2 && (bool)characterBody2.healthComponent)
				{
					characterBody2.healthComponent.Suicide(base.gameObject, base.gameObject, DamageType.VoidDeath);
				}
			}
		}
	}

	public override string InteractableName => "Void Reaver Corpse";

	public override bool IsEnabled => false;

	public override void ModifySpawnCard()
	{
		base.ModifySpawnCard();
		InteractableSpawnCard.hullSize = HullClassification.Golem;
		InteractableSpawnCard.requiredFlags = NodeFlags.None;
		InteractableSpawnCard.forbiddenFlags = NodeFlags.NoChestSpawn;
		InteractableSpawnCard.directorCreditCost = 2;
		InteractableSpawnCard.occupyPosition = true;
		InteractableSpawnCard.eliteRules = SpawnCard.EliteRules.Default;
		InteractableSpawnCard.orientToFloor = true;
		InteractableSpawnCard.slightlyRandomizeOrientation = false;
		InteractableSpawnCard.skipSpawnWhenSacrificeArtifactEnabled = false;
	}

	public override void ModifyDirectorCardAndAddInteractable()
	{
		base.ModifyDirectorCardAndAddInteractable();
		DirectorCard.selectionWeight = 5;
		DirectorCard.minimumStageCompletions = 7;
		AddInteractable("Barrels");
	}

	public override void SetHooks()
	{
	}

	public override void AssignPrefab()
	{
		SpikestripContentBase.entityStateTypeContent.Add(typeof(SingularityBombState));
		InteractablePrefab = PrefabAPI.InstantiateClone(LegacyResourcesAPI.Load<GameObject>("prefabs/networkedobjects/shrines/ShrineCombat"), "NullifierCorpse", true);
		UnityEngine.Object.Destroy(InteractablePrefab.transform.Find("Base").gameObject);
		UnityEngine.Object.Destroy(InteractablePrefab.transform.Find("Symbol").gameObject);
		UnityEngine.Object.Destroy(InteractablePrefab.GetComponent<DitherModel>());
		UnityEngine.Object.Destroy(InteractablePrefab.GetComponent<GenericDisplayNameProvider>());
		UnityEngine.Object.Destroy(InteractablePrefab.GetComponent<PurchaseAvailabilityIndicator>());
		UnityEngine.Object.Destroy(InteractablePrefab.GetComponent<ModelLocator>());
		UnityEngine.Object.Destroy(InteractablePrefab.GetComponent<ShrineCombatBehavior>());
		GameObject gameObject = LegacyResourcesAPI.Load<GameObject>("prefabs/stagedisplay/ArenaDioramaDisplay").transform.Find("Props/ArenaNullifierMolt").gameObject;
		GameObject gameObject2 = PrefabAPI.InstantiateClone(gameObject, "mdlDeadNullifier", false);
		gameObject2.transform.Find("NullifierMesh").SetAsFirstSibling();
		SkinnedMeshRenderer componentInChildren = gameObject2.GetComponentInChildren<SkinnedMeshRenderer>();
		EntityLocator entityLocator = gameObject2.AddComponent<EntityLocator>();
		entityLocator.entity = InteractablePrefab;
		CapsuleCollider capsuleCollider = gameObject2.AddComponent<CapsuleCollider>();
		capsuleCollider.isTrigger = true;
		capsuleCollider.direction = 1;
		capsuleCollider.center = Vector3.zero;
		capsuleCollider.radius = 5f;
		capsuleCollider.height = 1.5f;
		gameObject2.transform.SetParent(InteractablePrefab.transform);
		gameObject2.transform.localPosition = new Vector3(0f, 0f, 0f);
		InteractablePrefab.GetComponent<Highlight>().targetRenderer = componentInChildren;
		EntityStateMachine entityStateMachine = InteractablePrefab.AddComponent<EntityStateMachine>();
		entityStateMachine.customName = "Main";
		entityStateMachine.initialStateType = new SerializableEntityStateType(typeof(Idle));
		entityStateMachine.mainStateType = new SerializableEntityStateType(typeof(Idle));
		NetworkStateMachine networkStateMachine = InteractablePrefab.AddComponent<NetworkStateMachine>();
		networkStateMachine.stateMachines = new EntityStateMachine[1] { entityStateMachine };
		PingInfoProvider pingInfoProvider = InteractablePrefab.AddComponent<PingInfoProvider>();
		pingInfoProvider.pingIconOverride = LegacyResourcesAPI.Load<Sprite>("textures/miscicons/texAttackIcon");
		DitherModel ditherModel = InteractablePrefab.AddComponent<DitherModel>();
		ditherModel.fade = 0f;
		ditherModel.bounds = capsuleCollider;
		ditherModel.renderers = new Renderer[2]
		{
			gameObject2.transform.Find("NullifierArmorMesh").GetComponent<SkinnedMeshRenderer>(),
			gameObject2.transform.Find("NullifierMesh").GetComponent<SkinnedMeshRenderer>()
		};
		PurchaseInteraction component = InteractablePrefab.GetComponent<PurchaseInteraction>();
		component.displayNameToken = "NULLIFIERCORPSE_NAME";
		component.contextToken = "NULLIFIERCORPSE_CONTEXT";
		component.onPurchase = new PurchaseEvent();
		LanguageAPI.Add(component.displayNameToken, InteractableName);
		LanguageAPI.Add(component.contextToken, "Examine Void Reaver Corpse");
		VoidSingularityBehaviour voidSingularityBehaviour = InteractablePrefab.AddComponent<VoidSingularityBehaviour>();
	}
}
