using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using EntityStates;
using HG;
using On.RoR2;
using On.RoR2.CharacterAI;
using R2API;
using R2API.Networking;
using R2API.Networking.Interfaces;
using RoR2;
using RoR2.CharacterAI;
using RoR2.Skills;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Networking;
using Spikestrip2_0.Core;

namespace GrooveSaladSpikestripContent.Content;

public class HuntressVoidPortal : SpikestripVoidAltSkillsBase<HuntressVoidPortal>
{
	public class SyncVoidOverlay : INetMessage, ISerializableObject
	{
		private NetworkInstanceId GameObjectID;

		public SyncVoidOverlay()
		{
		}

		public SyncVoidOverlay(NetworkInstanceId gameObjectID)
		{
			GameObjectID = gameObjectID;
		}

		public void Serialize(NetworkWriter writer)
		{
			writer.Write(GameObjectID);
		}

		public void Deserialize(NetworkReader reader)
		{
			GameObjectID = reader.ReadNetworkId();
		}

		public void OnReceived()
		{
			if (NetworkServer.active)
			{
				return;
			}
			GameObject gameObject = RoR2.Util.FindNetworkObject(GameObjectID);
			if ((bool)gameObject)
			{
				RoR2.CharacterBody component = gameObject.GetComponent<RoR2.CharacterBody>();
				if ((bool)component)
				{
					AddVoidOverlay(component);
				}
			}
		}
	}

	public class ApplyVoidDoT : MonoBehaviour, IOnDamageDealtServerReceiver
	{
		public void OnDamageDealtServer(RoR2.DamageReport damageReport)
		{
			RoR2.DamageInfo damageInfo = damageReport.damageInfo;
			if (damageInfo.procCoefficient > 0f && (bool)damageReport.victimBody)
			{
				InflictDotInfo inflictDotInfo = new InflictDotInfo
				{
					attackerObject = damageReport.attacker,
					dotIndex = voidPortalDotIndex,
					damageMultiplier = 1f,
					duration = float.PositiveInfinity,
					victimObject = damageReport.victimBody.gameObject
				};
				RoR2.DotController.InflictDot(ref inflictDotInfo);
				RemoveVoidDotServer removeVoidDotServer = damageReport.victimBody.GetComponent<RemoveVoidDotServer>();
				if (!removeVoidDotServer)
				{
					removeVoidDotServer = damageReport.victimBody.gameObject.AddComponent<RemoveVoidDotServer>();
					CreateVoidBlinkEffect(damageReport.victimBody);
				}
				if (!removeVoidDotServer.contributors.Contains(damageReport.attackerBody))
				{
					removeVoidDotServer.contributors.Add(damageReport.attackerBody);
				}
			}
		}
	}

	public class RemoveVoidDotServer : MonoBehaviour
	{
		public static List<RemoveVoidDotServer> instancesList = new List<RemoveVoidDotServer>();

		public List<RoR2.CharacterBody> contributors = new List<RoR2.CharacterBody>();

		public void RemoveContributorServer(RoR2.CharacterBody contributor)
		{
			if (!NetworkServer.active)
			{
				return;
			}
			contributors.Remove(contributor);
			if (contributors.Count != 0)
			{
				return;
			}
			if (RoR2.DotController.dotControllerLocator.TryGetValue(base.gameObject.GetInstanceID(), out var value))
			{
				for (int num = value.dotStackList.Count - 1; num >= 0; num--)
				{
					if (value.dotStackList[num].dotIndex == voidPortalDotIndex)
					{
						value.RemoveDotStackAtServer(num);
					}
				}
			}
			RoR2.CharacterBody component = GetComponent<RoR2.CharacterBody>();
			if ((bool)component)
			{
				CreateVoidBlinkEffect(component);
			}
			UnityEngine.Object.Destroy(this);
		}

		private void OnEnable()
		{
			instancesList.Add(this);
		}

		private void OnDisable()
		{
			instancesList.Remove(this);
		}
	}

	public class InVoidPortal : BaseState
	{
		public static float tickPeriodSeconds = 0.25f;

		public static float healthFractionPerSecond = 0.02f;

		public static float healthFractionRampCoefficientPerSecond = 0.1f;

		private ApplyVoidDoT applyVoidDoT;

		private Vector3 entryPosition;

		private RoR2.CameraRigController lastRecordedCameraRigController;

		private float damageTimer;

		public override void OnEnter()
		{
			base.OnEnter();
			entryPosition = (base.characterBody ? base.characterBody.corePosition : base.transform.position);
			RoR2.Indicator indicator = GetComponent<RoR2.HuntressTracker>().indicator;
			indicator.visualizerPrefab = voidHuntressTrackingIndicator;
			applyVoidDoT = base.gameObject.AddComponent<ApplyVoidDoT>();
			if (NetworkServer.active)
			{
				base.characterBody.AddBuff(RoR2.RoR2Content.Buffs.VoidFogMild);
			}
			AddVelocity();
			if ((bool)base.skillLocator && (bool)base.skillLocator.utility)
			{
				base.skillLocator.utility.SetSkillOverride(this, cancelSkillDef, RoR2.GenericSkill.SkillOverridePriority.Contextual);
			}
			CreateVoidBlinkEffect(base.characterBody);
			On.RoR2.MusicController.UpdateTeleporterParameters += MusicController_UpdateTeleporterParameters;
			On.RoR2.CharacterModel.UpdateForCamera += CharacterModel_UpdateForCamera;
			On.RoR2.CharacterModel.UpdateOverlays += CharacterModel_UpdateOverlays;
			On.RoR2.CharacterAI.BaseAI.Target.GetBullseyePosition += Target_GetBullseyePosition;
		}

		private void MusicController_UpdateTeleporterParameters(On.RoR2.MusicController.orig_UpdateTeleporterParameters orig, RoR2.MusicController self, RoR2.TeleporterInteraction teleporter, Transform cameraTransform, RoR2.CharacterBody targetBody)
		{
			orig(self, teleporter, cameraTransform, targetBody);
			if (targetBody == base.characterBody)
			{
				self.rtpcTeleporterPlayerStatus.value = Mathf.Min(self.rtpcTeleporterPlayerStatus.value, 0f);
			}
		}

		private void CharacterModel_UpdateForCamera(On.RoR2.CharacterModel.orig_UpdateForCamera orig, RoR2.CharacterModel self, RoR2.CameraRigController cameraRigController)
		{
			lastRecordedCameraRigController = cameraRigController;
			orig(self, cameraRigController);
		}

		private void CharacterModel_UpdateOverlays(On.RoR2.CharacterModel.orig_UpdateOverlays orig, RoR2.CharacterModel self)
		{
			if ((bool)lastRecordedCameraRigController && (bool)lastRecordedCameraRigController.targetBody && (bool)self.body && ((lastRecordedCameraRigController.targetBody == base.characterBody && !self.body.HasBuff(voidPortalBuff) && self.body != base.characterBody) || (lastRecordedCameraRigController.targetBody != base.characterBody && self.body == base.characterBody)))
			{
				self.visibility = VisibilityLevel.Invisible;
			}
			orig(self);
		}

		private bool Target_GetBullseyePosition(On.RoR2.CharacterAI.BaseAI.Target.orig_GetBullseyePosition orig, RoR2.CharacterAI.BaseAI.Target self, out Vector3 position)
		{
			if (self.characterBody == base.characterBody && (bool)self.owner.body && !self.owner.body.HasBuff(voidPortalBuff))
			{
				position = entryPosition;
				return false;
			}
			return orig(self, out position);
		}

		private void AddVelocity()
		{
			if (base.isAuthority && (bool)base.characterDirection && (bool)(UnityEngine.Object)(object)base.characterMotor)
			{
				base.characterMotor.velocity += base.characterDirection.forward * 10f + Vector3.up * 5f;
			}
		}

		public override void FixedUpdate()
		{
			base.FixedUpdate();
			if (!NetworkServer.active)
			{
				return;
			}
			damageTimer += Time.fixedDeltaTime;
			if (damageTimer > tickPeriodSeconds)
			{
				damageTimer -= tickPeriodSeconds;
				float num = healthFractionPerSecond * (1f + base.fixedAge * healthFractionRampCoefficientPerSecond) * tickPeriodSeconds * base.healthComponent.fullCombinedHealth;
				if (num > 0f)
				{
					base.healthComponent.TakeDamage(new RoR2.DamageInfo
					{
						damage = num,
						position = base.characterBody.corePosition,
						damageType = (DamageType.BypassArmor | DamageType.BypassBlock),
						damageColorIndex = DamageColorIndex.Void
					});
				}
			}
		}

		public override void OnExit()
		{
			RoR2.Indicator indicator = GetComponent<RoR2.HuntressTracker>().indicator;
			indicator.visualizerPrefab = huntressTrackingIndicator;
			if ((bool)applyVoidDoT)
			{
				EntityState.Destroy(applyVoidDoT);
			}
			if (NetworkServer.active)
			{
				base.characterBody.RemoveBuff(RoR2.RoR2Content.Buffs.VoidFogMild);
				for (int i = 0; i < RemoveVoidDotServer.instancesList.Count; i++)
				{
					RemoveVoidDotServer removeVoidDotServer = RemoveVoidDotServer.instancesList[i];
					if (removeVoidDotServer.contributors.Contains(base.characterBody))
					{
						removeVoidDotServer.RemoveContributorServer(base.characterBody);
					}
				}
			}
			AddVelocity();
			if ((bool)base.skillLocator && (bool)base.skillLocator.utility)
			{
				base.skillLocator.utility.UnsetSkillOverride(this, cancelSkillDef, RoR2.GenericSkill.SkillOverridePriority.Contextual);
			}
			CreateVoidBlinkEffect(base.characterBody);
			On.RoR2.MusicController.UpdateTeleporterParameters -= MusicController_UpdateTeleporterParameters;
			On.RoR2.CharacterModel.UpdateForCamera -= CharacterModel_UpdateForCamera;
			On.RoR2.CharacterModel.UpdateOverlays -= CharacterModel_UpdateOverlays;
			On.RoR2.CharacterAI.BaseAI.Target.GetBullseyePosition -= Target_GetBullseyePosition;
			base.OnExit();
		}

		public override InterruptPriority GetMinimumInterruptPriority()
		{
			return InterruptPriority.Any;
		}
	}

	[Serializable]
	[CompilerGenerated]
	private sealed class _003C_003Ec
	{
		public static readonly _003C_003Ec _003C_003E9 = new _003C_003Ec();

		public static DotAPI.CustomDotBehaviour _003C_003E9__28_0;

		public static Func<RoR2.CharacterBody, float> _003C_003E9__28_1;

		public static Func<RoR2.CharacterBody, bool> _003C_003E9__28_2;

		internal void _003CInit_003Eb__28_0(RoR2.DotController self, RoR2.DotController.DotStack dotStack)
		{
			dotStack.damageType |= DamageType.BypassArmor | DamageType.BypassBlock;
			int i = 0;
			for (int count = self.dotStackList.Count; i < count; i++)
			{
				if (self.dotStackList[i].dotIndex == dotStack.dotIndex)
				{
					self.dotStackList[i].timer = Mathf.Max(self.dotStackList[i].timer, dotStack.timer);
				}
			}
		}

		internal float _003CInit_003Eb__28_1(RoR2.CharacterBody body)
		{
			return body.bestFitRadius;
		}

		internal bool _003CInit_003Eb__28_2(RoR2.CharacterBody body)
		{
			return body.HasBuff(voidPortalBuff);
		}
	}

	public static GameObject huntressTrackingIndicator = SpikestripUtil.LegacyLoad<GameObject>("Prefabs/HuntressTrackingIndicator");

	public static GameObject voidHuntressTrackingIndicator;

	public static GameObject voidBlinkEffect;

	public static RoR2.BuffDef voidPortalBuff;

	public static RoR2.DotController.DotIndex voidPortalDotIndex;

	public static SkillDef cancelSkillDef;

	public static Material matVoidWardCrabOverlay = SpikestripUtil.AddressablesLoad<Material>("0752a526857b5994d9891256da5f7307");

	public override string SkillName => "「Network」";

	public override string SkillDescription => $"Enter the Void, <style=cIsVoid>draining your health</style> over time. Attacks <style=cIsVoid>integrate</style> enemies, inflicting <style=cIsVoid>{SpikestripUtil.Percent(0.5f)}% damage per second</style>.";

	public override string SkillToken => "VOIDPORTAL";

	public override string PrerequisiteIdentifier => null;

	public override string CharacterName => "Huntress";

	public override string AchievementName => "Soundless Depths";

	public override Sprite SkillIcon => SpikestripContent.GrooveBundle.LoadAsset<Sprite>("texPortalIcon.png");

	public override GameObject OwnerBodyPrefab => SpikestripUtil.LegacyLoad<GameObject>("Prefabs/CharacterBodies/HuntressBody");

	public override SkillFamily[] SkillFamilies => new SkillFamily[1] { SpikestripUtil.LegacyLoad<SkillFamily>("SkillDefs/HuntressBody/HuntressBodyUtilityFamily") };

	public override Type[] EntityStateTypes => new Type[1] { typeof(InVoidPortal) };

	public override SkillDef GetSkillDef()
	{
		SkillDef skillDef = ScriptableObject.CreateInstance<SkillDef>();
		skillDef.activationState = new SerializableEntityStateType(typeof(InVoidPortal));
		skillDef.activationStateMachineName = "SpikestripVoid";
		skillDef.baseMaxStock = 1;
		skillDef.baseRechargeInterval = 7f;
		skillDef.cancelSprintingOnActivation = false;
		skillDef.interruptPriority = InterruptPriority.Any;
		skillDef.isCombatSkill = false;
		skillDef.beginSkillCooldownOnSkillEnd = true;
		skillDef.mustKeyPress = true;
		skillDef.resetCooldownTimerOnUse = true;
		skillDef.fullRestockOnAssign = false;
		return skillDef;
	}

	public override void Init()
	{
		//IL_00aa: Unknown result type (might be due to invalid IL or missing references)
		//IL_00af: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b5: Expected O, but got Unknown
		base.Init();
		NetworkingAPI.RegisterMessageType<SyncVoidOverlay>();
		voidPortalBuff = ScriptableObject.CreateInstance<RoR2.BuffDef>();
		voidPortalBuff.buffColor = SpikestripUtil.ColorRGB(203f, 121f, 213f);
		voidPortalBuff.canStack = true;
		voidPortalBuff.isDebuff = false;
		voidPortalBuff.isCooldown = false;
		voidPortalBuff.iconSprite = SpikestripContent.GrooveBundle.LoadAsset<Sprite>("texVoidPortalDOTIcon.png");
		ContentAddition.AddBuffDef(voidPortalBuff);
		RoR2.BuffDef buffDef = voidPortalBuff;
		object obj = _003C_003Ec._003C_003E9__28_0;
		if (obj == null)
		{
			DotAPI.CustomDotBehaviour val = delegate(RoR2.DotController self, RoR2.DotController.DotStack dotStack)
			{
				dotStack.damageType |= DamageType.BypassArmor | DamageType.BypassBlock;
				int i = 0;
				for (int count = self.dotStackList.Count; i < count; i++)
				{
					if (self.dotStackList[i].dotIndex == dotStack.dotIndex)
					{
						self.dotStackList[i].timer = Mathf.Max(self.dotStackList[i].timer, dotStack.timer);
					}
				}
			};
			_003C_003Ec._003C_003E9__28_0 = val;
			obj = (object)val;
		}
		voidPortalDotIndex = DotAPI.RegisterDotDef(0.5f, 0.25f, DamageColorIndex.Void, buffDef, (DotAPI.CustomDotBehaviour)obj, (DotAPI.CustomDotVisual)null);
		TempVisualEffectAPI.AddTemporaryVisualEffect(SpikestripUtil.LegacyLoad<GameObject>("Prefabs/TemporaryVisualEffects/VoidFogMildEffect"), (RoR2.CharacterBody body) => body.bestFitRadius, (RoR2.CharacterBody body) => body.HasBuff(voidPortalBuff));
		cancelSkillDef = ScriptableObject.CreateInstance<SkillDef>();
		cancelSkillDef.activationState = new SerializableEntityStateType(typeof(Idle));
		cancelSkillDef.activationStateMachineName = "SpikestripVoid";
		cancelSkillDef.baseMaxStock = 0;
		cancelSkillDef.baseRechargeInterval = 0f;
		cancelSkillDef.cancelSprintingOnActivation = false;
		cancelSkillDef.dontAllowPastMaxStocks = false;
		cancelSkillDef.fullRestockOnAssign = false;
		cancelSkillDef.icon = SpikestripContent.GrooveBundle.LoadAsset<Sprite>("texCancelPortalIcon.png");
		cancelSkillDef.interruptPriority = InterruptPriority.Any;
		cancelSkillDef.isCombatSkill = false;
		cancelSkillDef.mustKeyPress = true;
		cancelSkillDef.rechargeStock = 0;
		cancelSkillDef.requiredStock = 0;
		cancelSkillDef.resetCooldownTimerOnUse = false;
		cancelSkillDef.stockToConsume = 0;
		cancelSkillDef.skillName = "SPIKESTRIPSKILL_CANCELPORTAL";
		cancelSkillDef.skillNameToken = "SPIKESTRIPSKILL_SPIKESTRIPSKILL_CANCELPORTAL_NAME";
		cancelSkillDef.skillDescriptionToken = "SPIKESTRIPSKILL_CANCELPORTAL_DESCRIPTION";
		ContentAddition.AddSkillDef(cancelSkillDef);
		LanguageAPI.Add(cancelSkillDef.skillNameToken, SkillName);
		LanguageAPI.Add(cancelSkillDef.skillDescriptionToken, "<style=cIsHealing>Withdraw from the Void</style>, freeing all integrated enemies.");
		RoR2.EntityStateMachine value = OwnerBodyPrefab.AddComponent<RoR2.EntityStateMachine>();
		value.customName = "SpikestripVoid";
		value.initialStateType = new SerializableEntityStateType(typeof(Idle));
		value.mainStateType = new SerializableEntityStateType(typeof(Idle));
		RoR2.NetworkStateMachine component = OwnerBodyPrefab.GetComponent<RoR2.NetworkStateMachine>();
		ArrayUtils.ArrayAppend(ref component.stateMachines, in value);
		voidHuntressTrackingIndicator = PrefabAPI.InstantiateClone(huntressTrackingIndicator, "VoidHuntressTrackingIndicator", false);
		Sprite sprite = Addressables.LoadAssetAsync<Sprite>("cc00df736a97b3740b95a64c819c32f0").WaitForCompletion();
		SpriteRenderer[] componentsInChildren = voidHuntressTrackingIndicator.GetComponentsInChildren<SpriteRenderer>();
		foreach (SpriteRenderer spriteRenderer in componentsInChildren)
		{
			switch (spriteRenderer.gameObject.name)
			{
			case "Nib":
				spriteRenderer.sprite = sprite;
				spriteRenderer.color = SpikestripUtil.ColorRGB(238f, 26f, 121f, 0.286f);
				break;
			case "Core Pip":
				spriteRenderer.color = SpikestripUtil.ColorRGB(238f, 26f, 146f, 0.286f);
				spriteRenderer.transform.SetAsLastSibling();
				break;
			case "Core, Dark":
				spriteRenderer.color = SpikestripUtil.ColorRGB(154f, 0f, 137f, 0.9f);
				break;
			}
		}
		GameObject gameObject = SpikestripUtil.AddressablesLoad<GameObject>("8d071456afc8e504e90e1bf15da2c4c1");
		voidBlinkEffect = PrefabAPI.InstantiateClone(gameObject, "VoidPortalBlinkEffect", false);
		RoR2.EffectComponent component2 = voidBlinkEffect.GetComponent<RoR2.EffectComponent>();
		component2.applyScale = true;
		component2.soundName = "Play_voidDevastator_m2_primary_explo";
		ContentAddition.AddEffect(voidBlinkEffect);
	}

	public static void CreateVoidBlinkEffect(RoR2.CharacterBody body)
	{
		if (NetworkServer.active)
		{
			RoR2.EffectData effectData = new RoR2.EffectData();
			effectData.rotation = RoR2.Util.QuaternionSafeLookRotation(body.characterDirection ? body.characterDirection.animatorForward : body.transform.forward);
			effectData.origin = body.corePosition;
			effectData.scale = body.bestFitRadius;
			RoR2.EffectManager.SpawnEffect(voidBlinkEffect, effectData, transmit: true);
			AddVoidOverlay(body);
			NetworkIdentity networkIdentity = body.networkIdentity;
			if ((bool)networkIdentity)
			{
				NetMessageExtensions.Send((INetMessage)(object)new SyncVoidOverlay(networkIdentity.netId), (NetworkDestination)1);
			}
		}
	}

	private static void AddVoidOverlay(RoR2.CharacterBody body)
	{
		Transform transform = (body.modelLocator ? body.modelLocator.modelTransform : null);
		RoR2.CharacterModel characterModel = (transform ? transform.GetComponent<RoR2.CharacterModel>() : null);
		if ((bool)characterModel)
		{
			RoR2.TemporaryOverlay temporaryOverlay = characterModel.gameObject.AddComponent<RoR2.TemporaryOverlay>();
			temporaryOverlay.duration = 1f;
			temporaryOverlay.animateShaderAlpha = true;
			temporaryOverlay.alphaCurve = AnimationCurve.EaseInOut(0f, 1f, 1f, 0f);
			temporaryOverlay.destroyComponentOnEnd = true;
			temporaryOverlay.originalMaterial = matVoidWardCrabOverlay;
			temporaryOverlay.AddToCharacerModel(characterModel);
		}
	}
}
