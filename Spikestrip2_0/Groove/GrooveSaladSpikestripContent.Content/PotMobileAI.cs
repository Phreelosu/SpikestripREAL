using RoR2;
using RoR2.CharacterAI;
using UnityEngine;

namespace GrooveSaladSpikestripContent.Content;

public class PotMobileAI
{
    //all of this is literally based on moff's potmobile enemy
    public static void CreateAI(GameObject masterPrefab)
	{
		AISkillDriver[] components = masterPrefab.GetComponents<AISkillDriver>();
		foreach (AISkillDriver obj in components)
		{
			Object.DestroyImmediate(obj);
		}
		AISkillDriver aISkillDriver = masterPrefab.AddComponent<AISkillDriver>();
		aISkillDriver.customName = "SuicideLowHealthOffNodeGraph";
		aISkillDriver.skillSlot = SkillSlot.Primary;
		aISkillDriver.moveTargetType = AISkillDriver.TargetType.CurrentEnemy;
		aISkillDriver.movementType = AISkillDriver.MovementType.ChaseMoveTarget;
		aISkillDriver.aimType = AISkillDriver.AimType.AtMoveTarget;
		aISkillDriver.activationRequiresTargetLoS = true;
		aISkillDriver.maxUserHealthFraction = 0.5f;
		aISkillDriver.requireSkillReady = true;
		aISkillDriver.selectionRequiresTargetLoS = true;
		aISkillDriver.moveInputScale = 0.5f;
		aISkillDriver.shouldSprint = false;
		aISkillDriver.buttonPressType = AISkillDriver.ButtonPressType.Hold;
		aISkillDriver.maxDistance = 10f;
		aISkillDriver.ignoreNodeGraph = true;
		AISkillDriver aISkillDriver2 = masterPrefab.AddComponent<AISkillDriver>();
		aISkillDriver2.customName = "BlastJump";
		aISkillDriver2.skillSlot = SkillSlot.Secondary;
		aISkillDriver2.moveTargetType = AISkillDriver.TargetType.CurrentEnemy;
		aISkillDriver2.movementType = AISkillDriver.MovementType.ChaseMoveTarget;
		aISkillDriver2.aimType = AISkillDriver.AimType.AtMoveTarget;
		aISkillDriver2.activationRequiresTargetLoS = true;
		aISkillDriver2.activationRequiresAimConfirmation = true;
		aISkillDriver2.activationRequiresAimTargetLoS = true;
		aISkillDriver2.maxUserHealthFraction = 0.75f;
		aISkillDriver2.requireSkillReady = true;
		aISkillDriver2.selectionRequiresTargetLoS = true;
		aISkillDriver2.moveInputScale = 1f;
		aISkillDriver2.shouldSprint = true;
		aISkillDriver2.buttonPressType = AISkillDriver.ButtonPressType.Hold;
		aISkillDriver2.maxDistance = 20f;
		AISkillDriver aISkillDriver3 = masterPrefab.AddComponent<AISkillDriver>();
		aISkillDriver3.customName = "ChaseIgnoreNodeGraph";
		aISkillDriver3.skillSlot = SkillSlot.None;
		aISkillDriver3.moveTargetType = AISkillDriver.TargetType.CurrentEnemy;
		aISkillDriver3.movementType = AISkillDriver.MovementType.ChaseMoveTarget;
		aISkillDriver3.aimType = AISkillDriver.AimType.AtMoveTarget;
		aISkillDriver3.shouldSprint = true;
		aISkillDriver3.maxDistance = 15f;
		aISkillDriver3.ignoreNodeGraph = true;
		AISkillDriver aISkillDriver4 = masterPrefab.AddComponent<AISkillDriver>();
		aISkillDriver4.customName = "SpeedUpMidRange";
		aISkillDriver4.skillSlot = SkillSlot.None;
		aISkillDriver4.moveTargetType = AISkillDriver.TargetType.CurrentEnemy;
		aISkillDriver4.movementType = AISkillDriver.MovementType.ChaseMoveTarget;
		aISkillDriver4.aimType = AISkillDriver.AimType.AtMoveTarget;
		aISkillDriver4.shouldSprint = true;
		aISkillDriver4.maxDistance = 35f;
		AISkillDriver aISkillDriver5 = masterPrefab.AddComponent<AISkillDriver>();
		aISkillDriver5.customName = "PathFromAfar";
		aISkillDriver5.skillSlot = SkillSlot.None;
		aISkillDriver5.moveTargetType = AISkillDriver.TargetType.CurrentEnemy;
		aISkillDriver5.movementType = AISkillDriver.MovementType.ChaseMoveTarget;
		aISkillDriver5.aimType = AISkillDriver.AimType.AtMoveTarget;
	}
}
