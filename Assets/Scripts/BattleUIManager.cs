// BattleUIManager.cs
// Attach to an empty GameObject in your battle scene (e.g. "BattleUIManager").
// Listens to BattleManager's events and builds the action-button panel,
// and shows/hides a simple "choose a target" prompt. Clicking a unit
// to target it is handled separately by UnitClickTarget.

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class BattleUIManager : MonoBehaviour
{
    [Header("References")]
    public BattleManager battleManager;

    [Header("Action buttons")]
    public GameObject actionPanel;          // parent panel to show/hide
    public Transform actionButtonContainer; // where buttons get spawned (e.g. a Vertical Layout Group)
    public Button actionButtonPrefab;       // a Button prefab with a TMP_Text child

    [Header("Target prompt")]
    public TMP_Text targetPromptText;       // e.g. "Choose a target..." - shown while waiting for a click

    readonly List<Button> spawnedButtons = new List<Button>();

    void OnEnable()
    {
        if (battleManager == null) return;
        battleManager.OnActionMenuNeeded += ShowActionMenu;
        battleManager.OnActionMenuHide += HideActionMenu;
        battleManager.OnTargetPromptNeeded += ShowTargetPrompt;
        battleManager.OnTargetPromptHide += HideTargetPrompt;
    }

    void OnDisable()
    {
        if (battleManager == null) return;
        battleManager.OnActionMenuNeeded -= ShowActionMenu;
        battleManager.OnActionMenuHide -= HideActionMenu;
        battleManager.OnTargetPromptNeeded -= ShowTargetPrompt;
        battleManager.OnTargetPromptHide -= HideTargetPrompt;
    }

    void ShowActionMenu(Unit unit, List<string> labels)
    {
        ClearButtons();
        actionPanel.SetActive(true);

        for (int i = 0; i < labels.Count; i++)
        {
            int index = i; // capture for the lambda below
            Button button = Instantiate(actionButtonPrefab, actionButtonContainer);
            TMP_Text label = button.GetComponentInChildren<TMP_Text>();
            if (label != null) label.text = labels[i];

            button.onClick.AddListener(() => battleManager.SubmitAction(index));
            spawnedButtons.Add(button);
        }
    }

    void HideActionMenu()
    {
        ClearButtons();
        actionPanel.SetActive(false);
    }

    void ShowTargetPrompt(List<Unit> validTargets)
    {
        if (targetPromptText == null) return;
        targetPromptText.gameObject.SetActive(true);
        targetPromptText.text = "Choose a target...";
    }

    void HideTargetPrompt()
    {
        if (targetPromptText == null) return;
        targetPromptText.gameObject.SetActive(false);
    }

    void ClearButtons()
    {
        foreach (Button b in spawnedButtons)
            Destroy(b.gameObject);
        spawnedButtons.Clear();
    }
}
