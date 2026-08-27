// MainMenuManager.cs
// Attach this to an empty GameObject in your MainMenu scene (e.g. "MainMenuManager").
// Hook the Play and Quit buttons' OnClick() events to these public methods
// in the Inspector (see setup steps).

using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuManager : MonoBehaviour
{
    [Tooltip("Must match the exact scene name as it appears in Build Settings.")]
    public string battleSceneName = "BattleTest";

    public void OnPlayPressed()
    {
        SceneManager.LoadScene(battleSceneName);
    }

    public void OnQuitPressed()
    {
        Debug.Log("Quit pressed");
        Application.Quit();

#if UNITY_EDITOR
        // Application.Quit() does nothing in the Editor, so this lets you
        // test the button while playing inside Unity.
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }
}
