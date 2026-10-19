using EntityStates;
using RoR2;

namespace PlasmaCoreSpikestripContent.Content.Monsters.States;

public class Opened : BaseState
{
	public override void OnEnter()
	{
		base.OnEnter();
		PlayAnimation("Body", "HiddenToRevealed");
		Util.PlaySound("Play_minorConstruct_attack_bodyOpen", base.gameObject);
	}

	public override void FixedUpdate()
	{
		base.FixedUpdate();
		if (base.isAuthority && base.characterBody.outOfCombat && base.characterBody.outOfDanger)
		{
			outer.SetNextState(new Closed());
		}
	}
}
