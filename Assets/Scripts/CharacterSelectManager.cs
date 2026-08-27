// CharacterSelectManager.cs
// Attach to an empty GameObject in a new CharacterSelect scene. Create
// one button per character class (5 total), each with a
// CharacterSelectButton component (below) pointing at this manager.

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class CharacterSelectManager : MonoBehaviour
{
    public const int PARTY_SIZE = 3;

    [Tooltip("Scene to load once the player confirms their party of 3.")]
    public string nextSceneName = "LevelSelect";

    List<CharacterClass> selected = new List<CharacterClass>();
    List<CharacterSelectButton> allButtons = new List<CharacterSelectButton>();

    public void RegisterButton(CharacterSelectButton button)
    {
        allButtons.Add(button);
        button.Refresh(GameProgress.IsCharacterUnlocked(button.characterClass), false);
    }

    // Called by a CharacterSelectButton when clicked.
    public void OnCharacterClicked(CharacterClass c)
    {
        if (!GameProgress.IsCharacterUnlocked(c)) return;

        if (selected.Contains(c))
        {
            selected.Remove(c);
        }
        else
        {
            if (selected.Count >= PARTY_SIZE) return; // already have 3, ignore
            selected.Add(c);
        }

        RefreshAllButtons();
    }

    void RefreshAllButtons()
    {
        foreach (CharacterSelectButton b in allButtons)
            b.Refresh(GameProgress.IsCharacterUnlocked(b.characterClass), selected.Contains(b.characterClass));
    }

    // Early on you may only have 1-2 characters unlocked - you should
    // still be able to fight with a partial team, not be blocked until
    // you have exactly 3.
    public bool CanConfirm => selected.Count >= 1 && selected.Count <= PARTY_SIZE;

    // Hook this up to a "Confirm Team" button's OnClick().
    public void OnConfirmPressed()
    {
        if (!CanConfirm)
        {
            Debug.Log("Pick at least 1 character before confirming.");
            return;
        }

        PartySelection.ChosenParty = new List<CharacterClass>(selected);
        SceneManager.LoadScene(nextSceneName);
    }
}