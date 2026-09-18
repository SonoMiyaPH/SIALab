// CharacterDetailsPanel.cs
// Attach to a panel in your CharacterSelect scene. Shows whichever
// character was last hovered/clicked - portrait, name, base stats, and
// ability names (skills + ultimate). Starts hidden; CharacterSelectManager
// shows it via ShowDetails().

using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CharacterDetailsPanel : MonoBehaviour
{
    public Image portraitImage;
    public TMP_Text nameText;
    public TMP_Text statsText;
    public TMP_Text abilitiesText;

    void Awake()
    {
        gameObject.SetActive(false);
    }

    public void Show(CharacterSelectButton button)
    {
        gameObject.SetActive(true);

        if (portraitImage != null) portraitImage.sprite = button.portrait;
        if (nameText != null) nameText.text = button.characterClass.ToString();

        if (statsText != null)
        {
            if (button.statsPrefab != null)
            {
                Unit s = button.statsPrefab;
                statsText.text = $"HP: {s.maxHP}\nATK: {s.attack}\nDEF: {s.defense}";
            }
            else
            {
                statsText.text = "";
            }
        }

        if (abilitiesText != null)
        {
            // AbilityFactory is static, so we get the ability list for this
            // class directly - no need to instantiate the character's prefab.
            var skills = AbilityFactory.GetSkills(button.characterClass);
            var ultimate = AbilityFactory.GetUltimate(button.characterClass);

            string text = "";
            foreach (var skill in skills)
                text += $"{skill.abilityName}\n";
            if (ultimate != null)
                text += $"{ultimate.abilityName}";

            abilitiesText.text = text;
        }
    }
}
