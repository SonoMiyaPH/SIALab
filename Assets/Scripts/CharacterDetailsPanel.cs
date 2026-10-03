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

	// Maps the underlying CharacterClass enum (which stays as Paladin,
	// Priest, HeavyKnight, Archer, Mage under the hood) to the Filipino
	// historical figure name that should show up in the UI. Update this
	// whenever you swap in a different figure (e.g. Priest -> Trinidad
	// Tecson) without needing to touch the enum or anything else.
	private static string GetDisplayName(CharacterClass characterClass)
	{
		switch (characterClass)
		{
			case CharacterClass.Paladin: return "Lapu-Lapu";
			case CharacterClass.Priest: return "Babaylan"; // swap to "Trinidad Tecson" if you go with her
			case CharacterClass.HeavyKnight: return "Andres Bonifacio";
			case CharacterClass.Archer: return "Heneral Luna";
			case CharacterClass.Mage: return "Hermano Pule"; // swap to "Felipe Salvador" if you go with him
			default: return characterClass.ToString();
		}
	}

	public void Show(CharacterSelectButton button)
	{
		gameObject.SetActive(true);

		if (portraitImage != null) portraitImage.sprite = button.portrait;
		if (nameText != null) nameText.text = GetDisplayName(button.characterClass);

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