using On.RoR2;
using On.RoR2.UI;
using R2API.Utils;
using RoR2;
using RoR2.UI;

namespace GrooveSaladSpikestripContent;

public class TrailerMode : SpikestripContentBase<TrailerMode>
{
	public static bool ShouldGenerateInteractables;

	public override string ConfigName => "Trailer Mode";

	public override string ConfigSection => "Utils";

	public override bool IsEnabled => false;

	public override void Init()
	{
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Expected O, but got Unknown
		base.Init();
		CommandHelper.AddToConsoleWhenReady();
		RoR2.SceneDirector.onPrePopulateSceneServer += SceneDirector_onPrePopulateSceneServer;
		// Every PushPickupNotification overload funnels into the UniquePickup one.
		On.RoR2.CharacterMasterNotificationQueue.PushPickupNotification_CharacterMaster_UniquePickup += CharacterMasterNotificationQueue_PushPickupNotification;
		RoR2.CharacterBody.onBodyStartGlobal += CharacterBody_onBodyStartGlobal;
		On.RoR2.UI.CombatHealthBarViewer.Awake += CombatHealthBarViewer_Awake;
	}

	[RoR2.ConCommand(commandName = "toggle_interactables", flags = ConVarFlags.ExecuteOnServer, helpText = "Toggle spawning of interactables on stages")]
	public static void CCToggleInteractables(RoR2.ConCommandArgs args)
	{
		ShouldGenerateInteractables = !ShouldGenerateInteractables;
		SpikestripContentBase.Log("(TRAILER MODE) Spawn interactables set to: " + ShouldGenerateInteractables);
	}

	private void SceneDirector_onPrePopulateSceneServer(RoR2.SceneDirector obj)
	{
		if (!ShouldGenerateInteractables)
		{
			SpikestripContentBase.Log(((object)obj)?.ToString() + ": placing no interactables! (TRAILER MODE)");
			obj.interactableCredit = 0;
		}
	}

	private void CharacterMasterNotificationQueue_PushPickupNotification(On.RoR2.CharacterMasterNotificationQueue.orig_PushPickupNotification_CharacterMaster_UniquePickup orig, RoR2.CharacterMaster characterMaster, RoR2.UniquePickup uniquePickup)
	{
	}

	private void CharacterBody_onBodyStartGlobal(RoR2.CharacterBody body)
	{
		RoR2.CharacterMaster master = body.master;
		if (!master)
		{
			return;
		}
		RoR2.PlayerCharacterMasterController component = master.GetComponent<RoR2.PlayerCharacterMasterController>();
		if ((bool)component)
		{
			RoR2.NetworkUser networkUser = component.networkUser;
			if ((bool)networkUser)
			{
				networkUser.userName = body.GetDisplayName();
			}
		}
	}

	private void CombatHealthBarViewer_Awake(On.RoR2.UI.CombatHealthBarViewer.orig_Awake orig, RoR2.UI.CombatHealthBarViewer self)
	{
		SpikestripContentBase.DestroyImmediate(self);
	}
}
