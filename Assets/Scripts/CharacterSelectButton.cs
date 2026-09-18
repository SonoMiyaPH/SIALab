// CharacterSelectButton.cs
// Attach to each of the 5 character buttons. Set 'characterClass' in
// the Inspector. Hook the button's OnClick() to OnButtonClicked().
//
// Also shows this character's portrait + stats in the shared details
// panel on hover (mouse) AND on click (so it works for touch too),
// regardless of whether the character is unlocked - letting players
// preview a locked character before they unlock it.

using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

[RequireComponent(typeof(Button))]
public class CharacterSelectButton : MonoBehaviour, IPointerEnterHandler
{
    public CharacterClass characterClass;
    public CharacterSelectManager manager;

    [Header("Portrait + stats source")]
    public Sprite portrait;

    [Tooltip("Drag this character's prefab here (the same one used in PartySpawner). " +
             "It's only READ from for its base stats - never instantiated here.")]
    public Unit statsPrefab;

    [Header("Optional visuals")]
    public GameObject lockedOverlay;   // e.g. a padlock icon
    public GameObject selectedOverlay; // e.g. a highlight border/checkmark
    public Image buttonPortraitImage;  // optional: shows the portrait on the button itself, not just the details panel

    Button button;

    void Start()
    {
        button = GetComponent<Button>();
        if (manager == null) manager = FindObjectOfType<CharacterSelectManager>();
        manager.RegisterButton(this);

        if (buttonPortraitImage != null && portrait != null)
            buttonPortraitImage.sprite = portrait;
    }

    public void OnButtonClicked()
    {
        manager.OnCharacterClicked(characterClass);
        manager.ShowDetails(this);
    }

    // Works even while locked (interactable = false), since pointer-enter
    // isn't gated by Selectable.interactable - only the click behavior is.
    public void OnPointerEnter(PointerEventData eventData)
    {
        manager.ShowDetails(this);
    }

    public void Refresh(bool unlocked, bool isSelected)
    {
        button.interactable = unlocked;
        if (lockedOverlay != null) lockedOverlay.SetActive(!unlocked);
        if (selectedOverlay != null) selectedOverlay.SetActive(isSelected);
    }
}