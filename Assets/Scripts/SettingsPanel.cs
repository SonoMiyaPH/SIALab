// SettingsPanel.cs
// Attach to your SettingsPanel GameObject in the Canvas. Handles master
// volume, graphics quality, and fullscreen toggle, and remembers the
// player's choices between sessions via PlayerPrefs.
//
// Setup:
// 1. Add a Slider (volume), a TMP_Dropdown (quality), and a Toggle
//    (fullscreen) as children of this panel.
// 2. Drag each into the matching field below in the Inspector.
// 3. Wire each control's "On Value Changed" event to the matching
//    method here (OnVolumeChanged, OnQualityChanged, OnFullscreenToggled).
// 4. Optionally wire a Close button's OnClick() to OnClosePressed(),
//    or just use MainMenuManager's OnCloseSettingsPressed() instead -
//    either works, just don't wire both to avoid double-handling.

using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SettingsPanel : MonoBehaviour
{
	[Header("UI References")]
	public Slider volumeSlider;
	public TMP_Dropdown qualityDropdown;
	public Toggle fullscreenToggle;

	private const string VolumeKey = "settings_volume";
	private const string QualityKey = "settings_quality";
	private const string FullscreenKey = "settings_fullscreen";

	void OnEnable()
	{
		LoadSettings();
	}

	private void LoadSettings()
	{
		float savedVolume = PlayerPrefs.GetFloat(VolumeKey, 1f);
		int savedQuality = PlayerPrefs.GetInt(QualityKey, QualitySettings.GetQualityLevel());
		bool savedFullscreen = PlayerPrefs.GetInt(FullscreenKey, Screen.fullScreen ? 1 : 0) == 1;

		// Apply to the game itself.
		AudioListener.volume = savedVolume;
		QualitySettings.SetQualityLevel(savedQuality);
		Screen.fullScreen = savedFullscreen;

		// Reflect current values in the UI without re-triggering the
		// OnValueChanged callbacks (which would just re-save the same values).
		if (volumeSlider != null) volumeSlider.SetValueWithoutNotify(savedVolume);
		if (qualityDropdown != null) qualityDropdown.SetValueWithoutNotify(savedQuality);
		if (fullscreenToggle != null) fullscreenToggle.SetIsOnWithoutNotify(savedFullscreen);
	}

	// Wire to the volume Slider's On Value Changed (float).
	public void OnVolumeChanged(float value)
	{
		AudioListener.volume = value;
		PlayerPrefs.SetFloat(VolumeKey, value);
	}

	// Wire to the quality TMP_Dropdown's On Value Changed (int32).
	// The dropdown's option list should match your project's Quality
	// Settings levels (Edit > Project Settings > Quality) in the same order.
	public void OnQualityChanged(int index)
	{
		QualitySettings.SetQualityLevel(index);
		PlayerPrefs.SetInt(QualityKey, index);
	}

	// Wire to the fullscreen Toggle's On Value Changed (bool).
	public void OnFullscreenToggled(bool isOn)
	{
		Screen.fullScreen = isOn;
		PlayerPrefs.SetInt(FullscreenKey, isOn ? 1 : 0);
	}

	// Optional: wire a Close button here instead of through MainMenuManager
	// if you'd rather keep this panel fully self-contained.
	public void OnClosePressed()
	{
		gameObject.SetActive(false);
	}
}