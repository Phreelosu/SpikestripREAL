using RoR2;
using RoR2.EntitlementManagement;
using RoR2.ExpansionManagement;
using RoR2.UI;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace GrooveSaladSpikestripContent;

public class LockSkillFromExpansion : MonoBehaviour
{
	public ExpansionDef requiredExpansion;

	private bool isLocked = false;

	private HGButton hgButton;

	private TooltipProvider tooltipProvider;

	private Image image;

	private bool cachedDisableGamepadClick;

	private bool cachedDisablePointerClick;

	private string cachedHoverToken;

	private Sprite cachedIcon;

	private string cachedOverrideTitleText;

	private string cachedOverrideBodyText;

	private Color cachedTitleColor;

	public void Start()
	{
		hgButton = GetComponent<HGButton>();
		tooltipProvider = GetComponent<TooltipProvider>();
		image = (hgButton ? ((Image)hgButton.targetGraphic) : null);
		if ((bool)hgButton)
		{
			cachedDisableGamepadClick = hgButton.disableGamepadClick;
			cachedDisablePointerClick = hgButton.disablePointerClick;
			cachedHoverToken = hgButton.hoverToken;
		}
		if ((bool)tooltipProvider)
		{
			cachedOverrideTitleText = tooltipProvider.overrideTitleText;
			cachedOverrideBodyText = tooltipProvider.overrideBodyText;
			cachedTitleColor = tooltipProvider.titleColor;
		}
		if ((bool)image)
		{
			cachedIcon = image.sprite;
		}
	}

	public void Update()
	{
		if ((bool)requiredExpansion)
		{
			bool flag = (PreGameController.instance ? PreGameController.instance.readOnlyRuleBook : (Run.instance ? Run.instance.ruleBook : null))?.IsChoiceActive(requiredExpansion.enabledChoice) ?? false;
			LocalUser localUser = ((MPEventSystem)EventSystem.current).localUser;
			bool flag2 = !requiredExpansion.requiredEntitlement || EntitlementManager.localUserEntitlementTracker.UserHasEntitlement(localUser, requiredExpansion.requiredEntitlement);
			bool flag3 = flag && flag2;
			if (isLocked == flag3)
			{
				isLocked = !flag3;
				RebuildButton();
			}
		}
	}

	public void RebuildButton()
	{
		string text = Language.GetString("UNIDENTIFIED");
		string text2 = GenerateLockedTitleText();
		string stringFormatted = Language.GetStringFormatted("LOGBOOK_HOVER_DESCRIPTION_FORMAT", text, text2, ColorUtility.ToHtmlStringRGBA(new Color(0.5f, 0.5f, 0.5f, 0.2f)));
		if ((bool)hgButton)
		{
			hgButton.disableGamepadClick = isLocked || cachedDisableGamepadClick;
			hgButton.disablePointerClick = isLocked || cachedDisablePointerClick;
			hgButton.hoverToken = (isLocked ? stringFormatted : cachedHoverToken);
		}
		if ((bool)tooltipProvider)
		{
			tooltipProvider.overrideTitleText = (isLocked ? text : cachedOverrideTitleText);
			tooltipProvider.overrideBodyText = (isLocked ? text2 : cachedOverrideBodyText);
			tooltipProvider.titleColor = (isLocked ? Color.gray : cachedTitleColor);
		}
		if ((bool)image)
		{
			image.sprite = (isLocked ? LoadoutPanelController.lockedIcon : cachedIcon);
		}
	}

	public string GenerateLockedTitleText()
	{
		if ((bool)requiredExpansion.requiredEntitlement)
		{
			LocalUser localUser = ((MPEventSystem)EventSystem.current).localUser;
			if (!EntitlementManager.localUserEntitlementTracker.UserHasEntitlement(localUser, requiredExpansion.requiredEntitlement))
			{
				return Language.GetStringFormatted("EXPANSION_PURCHASED_REQUIRED_FORMAT", Language.GetString(requiredExpansion.requiredEntitlement.nameToken));
			}
		}
		return Language.GetStringFormatted("EXPANSION_ENABLED_REQUIRED_FORMAT", Language.GetString(requiredExpansion.nameToken));
	}
}
