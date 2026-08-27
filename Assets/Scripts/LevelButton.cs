// LevelButton.cs
// Attach to each level button in your level-select screen (you'll have
// up to 30 of these, 5 per world). Set 'world' and 'level' in the
// Inspector for each button, then hook the button's OnClick() to
// this script's OnLevelButtonPressed().

using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(Button))]
public class LevelButton : MonoBehaviour
{
    [Header("Which level this button represents")]
    public int world = 1;
    public int level = 1;

    [Header("Scene to load when pressed")]
    public string battleSceneName = "BattleTest";

    [Header("Optional visuals")]
    public GameObject lockedOverlay;   // e.g. a padlock icon, shown when locked
    public GameObject clearedOverlay;  // e.g. a checkmark/star, shown when cleared

    Button button;

    void Start()
    {
        button = GetComponent<Button>();
        Refresh();
    }

    public void Refresh()
    {
        bool unlocked = GameProgress.IsLevelUnlocked(world, level);
        bool cleared = GameProgress.IsLevelCleared(world, level);

        button.interactable = unlocked;

        if (lockedOverlay != null) lockedOverlay.SetActive(!unlocked);
        if (clearedOverlay != null) clearedOverlay.SetActive(cleared);
    }

    public void OnLevelButtonPressed()
    {
        if (!GameProgress.IsLevelUnlocked(world, level)) return;

        // Store which level was chosen so the battle scene / victory
        // screen knows what to mark as cleared afterward.
        CurrentLevel.World = world;
        CurrentLevel.Level = level;

        SceneManager.LoadScene(battleSceneName);
    }
}

// Tiny static holder so the battle scene knows which level is being
// played without needing to pass data through the scene load.
public static class CurrentLevel
{
    public static int World = 1;
    public static int Level = 1;
}