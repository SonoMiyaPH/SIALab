// CharacterSelectButton.cs
// Attach to each of the 5 character buttons. Set 'characterClass' in
// the Inspector. Hook the button's OnClick() to OnButtonClicked().

using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class CharacterSelectButton : MonoBehaviour
{
    public CharacterClass characterClass;
    public CharacterSelectManager manager;

    [Header("Optional visuals")]
    public GameObject lockedOverlay;   // e.g. a padlock icon
    public GameObject selectedOverlay; // e.g. a highlight border/checkmark

    Button button;

    void Start()
    {
        button = GetComponent<Button>();
        if (manager == null) manager = FindObjectOfType<CharacterSelectManager>();
        manager.RegisterButton(this);
    }

    public void OnButtonClicked()
    {
        manager.OnCharacterClicked(characterClass);
    }

    public void Refresh(bool unlocked, bool isSelected)
    {
        button.interactable = unlocked;
        if (lockedOverlay != null) lockedOverlay.SetActive(!unlocked);
        if (selectedOverlay != null) selectedOverlay.SetActive(isSelected);
    }
}
