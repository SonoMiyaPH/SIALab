// GameProgress.cs
// Static class = accessible from anywhere without needing a scene reference
// (e.g. GameProgress.ClearLevel(1,3);  from your victory screen).
//
// Levels are addressed as (world, level) e.g. world 2, level 1 = "2-1".
// Worlds are 1-6, levels within a world are 1-5.
//
// Progress is saved with PlayerPrefs, which is simple key/value storage
// that persists between play sessions - good enough for a prototype.
// (Later, if you want proper save slots/cloud saves, we'd swap this
// for JSON file saving - same public methods, so nothing else changes.)

using UnityEngine;

public static class GameProgress
{
    const int WORLD_COUNT = 6;
    const int LEVELS_PER_WORLD = 5;

    // ---------- LEVEL CLEARING ----------

    static string LevelKey(int world, int level) => $"Cleared_{world}-{level}";

    public static void ClearLevel(int world, int level)
    {
        PlayerPrefs.SetInt(LevelKey(world, level), 1);
        PlayerPrefs.Save();

        // Unlocking a character is tied to clearing level 1 of a world.
        // World 1 = Paladin, always unlocked by default (see IsCharacterUnlocked).
        if (level == 1)
        {
            CharacterClass? unlocked = CharacterForWorld(world);
            if (unlocked.HasValue)
                UnlockCharacter(unlocked.Value);
        }
    }

    public static bool IsLevelCleared(int world, int level)
    {
        return PlayerPrefs.GetInt(LevelKey(world, level), 0) == 1;
    }

    // A level is playable if it's the very first level (1-1), or the
    // level before it in the same world is cleared, or (for level 1 of
    // worlds 2+) the previous world's level 1 is cleared - adjust this
    // rule freely once you see how it feels to play.
    public static bool IsLevelUnlocked(int world, int level)
    {
        if (world == 1 && level == 1) return true;

        if (level > 1)
            return IsLevelCleared(world, level - 1);

        // level == 1 of world 2+: need previous world's level 1 cleared
        return IsLevelCleared(world - 1, 1);
    }

    // ---------- CHARACTER UNLOCKING ----------

    static string CharacterKey(CharacterClass c) => $"Unlocked_{c}";

    public static void UnlockCharacter(CharacterClass c)
    {
        PlayerPrefs.SetInt(CharacterKey(c), 1);
        PlayerPrefs.Save();
        Debug.Log($"Character unlocked: {c}");
    }

    public static bool IsCharacterUnlocked(CharacterClass c)
    {
        if (c == CharacterClass.Paladin) return true; // always available
        return PlayerPrefs.GetInt(CharacterKey(c), 0) == 1;
    }

    // Maps world number -> the character that unlocks after clearing
    // level 1 of that world, per your proposal doc.
    static CharacterClass? CharacterForWorld(int world)
    {
        switch (world)
        {
            case 2: return CharacterClass.Priest;
            case 3: return CharacterClass.HeavyKnight;
            case 4: return CharacterClass.Archer;
            case 5: return CharacterClass.Mage;
            default: return null; // world 1 and world 6 unlock no new character
        }
    }

    // ---------- UTILITY ----------

    // Wipes all saved progress. Handy for testing - call from a debug button.
    public static void ResetAllProgress()
    {
        for (int w = 1; w <= WORLD_COUNT; w++)
            for (int l = 1; l <= LEVELS_PER_WORLD; l++)
                PlayerPrefs.DeleteKey(LevelKey(w, l));

        foreach (CharacterClass c in System.Enum.GetValues(typeof(CharacterClass)))
            PlayerPrefs.DeleteKey(CharacterKey(c));

        PlayerPrefs.Save();
        Debug.Log("Progress reset.");
    }
}
