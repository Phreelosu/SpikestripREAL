using EntityStates;
using RoR2;
using RoR2.Navigation;
using UnityEngine;

namespace PlasmaCoreSpikestripContent.Content.Monsters.States;

public class Burrow : BaseState
{
	public static float duration = 1f;

	public static float distance = 30f;

	public static GameObject effect;

	private Transform modelTransform;

	private CharacterModel characterModel;

	private float stopwatch;

	private Vector3 blinkDestination = Vector3.zero;

	private Vector3 blinkStart = Vector3.zero;

	public override void OnEnter()
	{
		base.OnEnter();
		Util.PlaySound("Play_minorConstruct_spawn", base.gameObject);
		PlayAnimation("Body", "Burrow");
		if ((bool)base.characterMotor)
		{
			base.characterMotor.enabled = false;
		}
		if (base.isAuthority)
		{
			Vector3 vector = base.inputBank.moveVector * distance;
			blinkDestination = base.transform.position;
			blinkStart = base.transform.position;
			NodeGraph groundNodes = SceneInfo.instance.groundNodes;
			NodeGraph.NodeIndex nodeIndex = groundNodes.FindClosestNode(base.transform.position + vector, base.characterBody.hullClassification);
			groundNodes.GetNodePosition(nodeIndex, out blinkDestination);
			vector = blinkDestination - blinkStart;
			blinkDestination += base.transform.position - base.characterBody.footPosition;
			CreateBlinkEffect(Util.GetCorePosition(base.gameObject), vector);
		}
	}

	private void CreateBlinkEffect(Vector3 origin, Vector3 direction)
	{
		EffectData effectData = new EffectData();
		effectData.rotation = Util.QuaternionSafeLookRotation(direction);
		effectData.origin = origin + Vector3.up * 2f;
		EffectManager.SpawnEffect(effect, effectData, transmit: true);
	}

	private void SetPosition(Vector3 newPosition)
	{
		if ((bool)base.characterMotor)
		{
			base.characterMotor.Motor.SetPositionAndRotation(newPosition, Quaternion.identity);
			return;
		}
		base.transform.position = newPosition;
		base.transform.rotation = Quaternion.identity;
	}

	public override void FixedUpdate()
	{
		base.FixedUpdate();
		stopwatch += Time.fixedDeltaTime;
		if ((bool)base.characterMotor && (bool)base.characterDirection)
		{
			base.characterMotor.velocity = Vector3.zero;
		}
		if (stopwatch >= duration)
		{
			SetPosition(blinkDestination);
			PlayAnimation("Body", "Unburrow");
			if (base.isAuthority)
			{
				outer.SetNextStateToMain();
			}
		}
	}

	public override void OnExit()
	{
		Util.PlaySound("Play_minorConstruct_spawn", base.gameObject);
		if ((bool)base.characterDirection)
		{
			base.characterDirection.forward = base.inputBank.moveVector;
		}
		if ((bool)base.characterMotor)
		{
			base.characterMotor.enabled = true;
		}
		base.OnExit();
		Vector3 direction = blinkStart - blinkDestination;
		CreateBlinkEffect(Util.GetCorePosition(base.gameObject), direction);
	}
}
