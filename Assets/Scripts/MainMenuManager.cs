// MainMenuManager.cs
// Attach this to an empty GameObject in your MainMenu scene (e.g. "MainMenuManager").
// Hook the Play, Quit, Multiplayer, and Settings buttons' OnClick() events to
// these public methods in the Inspector (see setup steps).

using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuManager : MonoBehaviour
{
	[Tooltip("Must match the exact scene name as it appears in Build Settings.")]
	public string battleSceneName = "BattleTest";

	[Header("Settings")]
	[Tooltip("The Settings panel GameObject in your Canvas. Starts inactive.")]
	public GameObject settingsPanel;

	[Header("Multiplayer (locked)")]
	[Tooltip("Optional small popup/text shown briefly when the locked Multiplayer button is pressed. Leave empty if you just made the button non-interactable instead.")]
	public GameObject multiplayerLockedMessage;

	[Tooltip("How long the 'coming soon' message stays visible, in seconds.")]
	public float lockedMessageDuration = 2f;

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

	// Wire this to the Multiplayer button's OnClick() ONLY if you kept the
	// button's Interactable checkbox ON and want a "coming soon" popup
	// instead of just greying it out. If you unchecked Interactable, this
	// never fires and you don't need to hook it up at all.
	public void OnMultiplayerPressed()
	{
		Debug.Log("Multiplayer is not available yet.");

		if (multiplayerLockedMessage != null)
		{
			CancelInvoke(nameof(HideMultiplayerLockedMessage));
			multiplayerLockedMessage.SetActive(true);
			Invoke(nameof(HideMultiplayerLockedMessage), lockedMessageDuration);
		}
	}

	private void HideMultiplayerLockedMessage()
	{
		if (multiplayerLockedMessage != null)
			multiplayerLockedMessage.SetActive(false);
	}

	// Wire this to the Settings button's OnClick().
	public void OnSettingsPressed()
	{
		Debug.Log("Settings clicked! Panel: " + (settingsPanel != null ? settingsPanel.name : "NULL"));
		if (settingsPanel != null)
		{
			settingsPanel.SetActive(true);
			Debug.Log("Active now: " + settingsPanel.activeSelf + " / InHierarchy: " + settingsPanel.activeInHierarchy);
		}
	}

	// Wire this to the Settings panel's own Close/Back button OnClick().
	public void OnCloseSettingsPressed()
	{
		if (settingsPanel != null)
			settingsPanel.SetActive(false);
	}
}