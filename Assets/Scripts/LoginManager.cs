using System.Collections;
using System.Text.RegularExpressions;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// Handles the Login / Register screen and talks to your PHP backend.
/// Put in Assets/Scripts. The Login scene builder wires all references for you.
/// </summary>
public class LoginManager : MonoBehaviour
{
	[Header("Server (PHP endpoints)")]
	public string loginUrl = "http://localhost/DestinyUnchained/api/login.php";
	public string registerUrl = "http://localhost/DestinyUnchained/api/register.php";

	[Header("Flow")]
	public string nextScene = "MainMenu";
	[Tooltip("If the server can't be reached, enter as a guest. For development only - turn OFF before shipping.")]
	public bool allowOfflineBypass = false;

	[Header("UI references (auto-filled by the scene builder)")]
	public Button loginTabButton;
	public Button registerTabButton;
	public Image loginTabImage;
	public Image registerTabImage;
	public TMP_Text loginTabLabel;
	public TMP_Text registerTabLabel;

	public GameObject usernameGroup;
	public GameObject confirmGroup;
	public TMP_InputField usernameInput;
	public TMP_InputField emailInput;
	public TMP_InputField passwordInput;
	public TMP_InputField confirmInput;

	public Button togglePasswordButton;
	public TMP_Text togglePasswordLabel;

	public Button submitButton;
	public TMP_Text submitLabel;
	public TMP_Text statusText;

	[Header("Colors")]
	public Color tabActive = new Color32(0x3F, 0xB4, 0xC3, 255);
	public Color tabIdle = new Color32(0x0A, 0x1D, 0x26, 255);
	public Color tabTextActive = new Color32(0x04, 0x14, 0x1B, 255);
	public Color tabTextIdle = new Color32(0xF2, 0xF6, 0xF8, 255);
	public Color infoColor = new Color32(0x7F, 0xA3, 0xAD, 255);
	public Color errorColor = new Color32(0xFF, 0x7A, 0x7A, 255);
	public Color successColor = new Color32(0x7A, 0xE6, 0xB0, 255);

	bool isRegister;
	bool busy;

	void Awake()
	{
		loginTabButton.onClick.AddListener(() => SetMode(false));
		registerTabButton.onClick.AddListener(() => SetMode(true));
		submitButton.onClick.AddListener(Submit);
		togglePasswordButton.onClick.AddListener(TogglePassword);

		SetMode(false);
		emailInput.text = PlayerPrefs.GetString("last_email", "");
	}

	void Update()
	{
#if ENABLE_LEGACY_INPUT_MANAGER
		if (!busy && (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter)))
			Submit();
#endif
	}

	// ---------- UI state ----------

	void SetMode(bool register)
	{
		isRegister = register;

		usernameGroup.SetActive(register);
		confirmGroup.SetActive(register);
		confirmInput.text = "";

		loginTabImage.color = register ? tabIdle : tabActive;
		registerTabImage.color = register ? tabActive : tabIdle;
		loginTabLabel.color = register ? tabTextIdle : tabTextActive;
		registerTabLabel.color = register ? tabTextActive : tabTextIdle;

		submitLabel.text = register ? "FORGE YOUR ACCOUNT" : "ENTER THE REALM";
		Say("", infoColor);
	}

	void TogglePassword()
	{
		bool reveal = passwordInput.contentType == TMP_InputField.ContentType.Password;
		var type = reveal ? TMP_InputField.ContentType.Standard : TMP_InputField.ContentType.Password;

		passwordInput.contentType = type;
		confirmInput.contentType = type;
		passwordInput.ForceLabelUpdate();
		confirmInput.ForceLabelUpdate();
		togglePasswordLabel.text = reveal ? "HIDE" : "SHOW";
	}

	void SetBusy(bool value)
	{
		busy = value;
		submitButton.interactable = !value;
		loginTabButton.interactable = !value;
		registerTabButton.interactable = !value;
	}

	void Say(string message, Color color)
	{
		if (statusText == null) return;
		statusText.text = message;
		statusText.color = color;
	}

	// ---------- Validation + request ----------

	void Submit()
	{
		if (busy) return;

		string email = emailInput.text.Trim();
		string password = passwordInput.text;

		if (!Regex.IsMatch(email, @"^[^@\s]+@[^@\s]+\.[^@\s]+$"))
		{
			Say("Enter a valid email address.", errorColor);
			return;
		}

		var form = new WWWForm();
		form.AddField("email", email);
		form.AddField("password", password);

		if (isRegister)
		{
			string username = usernameInput.text.Trim();
			if (username.Length < 3)
			{
				Say("Username must be at least 3 characters.", errorColor);
				return;
			}
			if (password.Length < 6)
			{
				Say("Password must be at least 6 characters.", errorColor);
				return;
			}
			if (password != confirmInput.text)
			{
				Say("Passwords do not match.", errorColor);
				return;
			}
			form.AddField("username", username);
		}
		else if (password.Length == 0)
		{
			Say("Enter your password.", errorColor);
			return;
		}

		StartCoroutine(SendAuth(isRegister ? registerUrl : loginUrl, form, isRegister, email));
	}

	IEnumerator SendAuth(string url, WWWForm form, bool registering, string email)
	{
		SetBusy(true);
		Say(registering ? "Forging your account..." : "Opening the gates...", infoColor);

		using (UnityWebRequest req = UnityWebRequest.Post(url, form))
		{
			req.timeout = 10;
			yield return req.SendWebRequest();

			if (req.result == UnityWebRequest.Result.ConnectionError)
			{
				if (allowOfflineBypass)
				{
					Debug.LogWarning("[Login] Server unreachable - entering as guest (offline bypass).");
					EnterRealm(new AuthResponse { user_id = 0, username = "Guest" }, email);
					yield break;
				}

				Say("Cannot reach the server. Is XAMPP / Apache running?", errorColor);
				SetBusy(false);
				yield break;
			}

			AuthResponse res = null;
			try { res = JsonUtility.FromJson<AuthResponse>(req.downloadHandler.text); }
			catch { /* handled below */ }

			if (res == null)
			{
				Debug.LogError("[Login] Unexpected server response:\n" + req.downloadHandler.text);
				Say("Unexpected server response. Check the Console.", errorColor);
				SetBusy(false);
				yield break;
			}

			if (!res.success)
			{
				Say(string.IsNullOrEmpty(res.message) ? "Something went wrong." : res.message, errorColor);
				SetBusy(false);
				yield break;
			}

			if (registering)
			{
				SetBusy(false);
				SetMode(false);
				passwordInput.text = "";
				Say(string.IsNullOrEmpty(res.message) ? "Account created. You may now log in." : res.message, successColor);
				yield break;
			}

			EnterRealm(res, email);
		}
	}

	void EnterRealm(AuthResponse res, string email)
	{
		PlayerSession.UserId = res.user_id;
		PlayerSession.Username = string.IsNullOrEmpty(res.username) ? email : res.username;
		PlayerSession.Email = email;
		PlayerPrefs.SetString("last_email", email);

		Say("Welcome, " + PlayerSession.Username + ".", successColor);
		SceneManager.LoadScene(nextScene);
	}
}

/// <summary>Shape of the JSON your PHP endpoints should return.</summary>
[System.Serializable]
public class AuthResponse
{
	public bool success;
	public string message;
	public int user_id;
	public string username;
}

/// <summary>Who is logged in. Read from any other scene, e.g. PlayerSession.Username.</summary>
public static class PlayerSession
{
	public static int UserId;
	public static string Username;
	public static string Email;
	public static bool IsLoggedIn => !string.IsNullOrEmpty(Username);

	public static void Clear()
	{
		UserId = 0;
		Username = null;
		Email = null;
	}
}