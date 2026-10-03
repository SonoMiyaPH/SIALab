using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// Builds a dark-teal themed Login/Register scene (v3: compact layout, gradient buttons,
/// glowing background, focus borders, optional Cinzel font).
/// Needs: LoginManager.cs and InputFocusBorder.cs in Assets/Scripts.
/// Put this file in Assets/Editor, then use Tools > Destiny Unchained > Create Login Scene.
/// </summary>
public static class LoginSceneBuilder
{
    const string ScenePath = "Assets/Scenes/Login.unity";
    const string SpriteFolder = "Assets/Sprites/UI";
    const string GameTitle = "CHAIN BREAKER";
    const string GameSubtitle = "DESTINY UNCHAINED";

    // Corner size on screen = 64 / multiplier
    const float PanelMult = 2.0f;    // ~32px
    const float ControlMult = 4.0f;  // ~16px

    // Palette (matched to the web login)
    static readonly Color BgDark      = Hex("#050B14");
    static readonly Color GlowColor   = new Color(0.10f, 0.40f, 0.46f, 0.55f);
    static readonly Color PanelBg     = Hex("#08151F");
    static readonly Color Border      = Hex("#1B4A55");
    static readonly Color Teal        = Hex("#3FB4C3");
    static readonly Color TealBright  = Hex("#4CC3D0");
    static readonly Color TealDim     = Hex("#1F7F8C");
    static readonly Color TabIdle     = Hex("#0A1D26");
    static readonly Color LightField  = Hex("#EEF1FF");
    static readonly Color DarkField   = Hex("#0A1A2E");
    static readonly Color ButtonBg    = Hex("#1A3F4E");
    static readonly Color ButtonHover = Hex("#25596B");
    static readonly Color ButtonPress = Hex("#122E3A");
    static readonly Color TextLight   = Hex("#F2F6F8");
    static readonly Color TextMuted   = Hex("#7FA3AD");
    static readonly Color TextDark    = Hex("#0B1622");
    static readonly Color HintDark    = Hex("#8A97A6");

    enum Kind { Fill, Ring, FillGradient, Glow, LineFade }

    static Sprite fillSprite, ringSprite, gradSprite, glowSprite, lineSprite;
    static TMP_FontAsset font;

    class Field
    {
        public GameObject root;
        public TMP_InputField input;
        public Button toggle;
        public TextMeshProUGUI toggleLabel;
    }

    [MenuItem("Tools/Destiny Unchained/Create Login Scene")]
    public static void Build()
    {
        if (AssetDatabase.LoadAssetAtPath<SceneAsset>(ScenePath) != null &&
            !EditorUtility.DisplayDialog("Login scene exists",
                "Assets/Scenes/Login.unity already exists. Replace it?", "Replace", "Cancel"))
            return;

        if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;

        EnsureFolder("Assets/Scenes");
        EnsureSprites();
        font = FindFont();

        var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

        // ----- Camera -----
        var camGO = new GameObject("Main Camera", typeof(Camera), typeof(AudioListener));
        camGO.tag = "MainCamera";
        var cam = camGO.GetComponent<Camera>();
        cam.clearFlags = CameraClearFlags.SolidColor;
        cam.backgroundColor = BgDark;
        cam.orthographic = true;

        // ----- EventSystem -----
        new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));

        // ----- Canvas (scales by HEIGHT so the panel always fits vertically) -----
        var canvasGO = new GameObject("Canvas", typeof(RectTransform), typeof(Canvas),
                                      typeof(CanvasScaler), typeof(GraphicRaycaster));
        var canvas = canvasGO.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        var scaler = canvasGO.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        scaler.matchWidthOrHeight = 1f;

        // ----- Background: solid color (drop your own artwork into its Source Image) + soft glow -----
        var bg = NewUI("Background", canvasGO.transform);
        StretchFull(bg.GetComponent<RectTransform>());
        var bgImg = bg.AddComponent<Image>();
        bgImg.color = BgDark;
        bgImg.raycastTarget = false;

        var glow = NewUI("BackgroundGlow", canvasGO.transform);
        StretchFull(glow.GetComponent<RectTransform>());
        var glowImg = glow.AddComponent<Image>();
        glowImg.sprite = glowSprite;
        glowImg.color = GlowColor;
        glowImg.raycastTarget = false;

        // ----- Panel -----
        var panel = NewUI("LoginPanel", canvasGO.transform);
        var panelRT = panel.GetComponent<RectTransform>();
        panelRT.anchorMin = panelRT.anchorMax = panelRT.pivot = new Vector2(0.5f, 0.5f);
        panelRT.anchoredPosition = Vector2.zero;
        panelRT.sizeDelta = new Vector2(600, 0);

        var panelImg = panel.AddComponent<Image>();
        panelImg.sprite = fillSprite;
        panelImg.type = Image.Type.Sliced;
        panelImg.pixelsPerUnitMultiplier = PanelMult;
        panelImg.color = PanelBg;
        AddBorder(panel, Border, PanelMult);

        var vlg = panel.AddComponent<VerticalLayoutGroup>();
        vlg.padding = new RectOffset(44, 44, 38, 40);
        vlg.spacing = 12;
        vlg.childAlignment = TextAnchor.UpperCenter;
        vlg.childControlWidth = true;
        vlg.childControlHeight = true;
        vlg.childForceExpandWidth = true;
        vlg.childForceExpandHeight = false;

        var fitter = panel.AddComponent<ContentSizeFitter>();
        fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
        fitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;

        // ----- Heading -----
        var sub = CreateText(panel.transform, "Subtitle", GameSubtitle, 20, Teal,
                             TextAlignmentOptions.Left, FontStyles.Normal, 10);
        sub.gameObject.AddComponent<LayoutElement>().preferredHeight = 24;

        var title = CreateText(panel.transform, "Title", GameTitle, 58, TextLight,
                               TextAlignmentOptions.Left, FontStyles.Bold, 2);
        title.gameObject.AddComponent<LayoutElement>().preferredHeight = 68;

        var line = NewUI("Divider", panel.transform);
        var lineImg = line.AddComponent<Image>();
        lineImg.sprite = lineSprite;
        lineImg.color = Teal;
        lineImg.raycastTarget = false;
        line.AddComponent<LayoutElement>().preferredHeight = 2;

        // ----- Tabs -----
        var tabs = NewUI("Tabs", panel.transform);
        var hlg = tabs.AddComponent<HorizontalLayoutGroup>();
        hlg.spacing = 14;
        hlg.childControlWidth = true;
        hlg.childControlHeight = true;
        hlg.childForceExpandWidth = true;
        hlg.childForceExpandHeight = true;
        tabs.AddComponent<LayoutElement>().preferredHeight = 60;

        Image loginTabImg, registerTabImg;
        TextMeshProUGUI loginTabTxt, registerTabTxt;
        var loginTab = CreateButton(tabs.transform, "LoginTab", "LOGIN", TealBright, TextDark, 22, 3,
                                    out loginTabImg, out loginTabTxt);
        var registerTab = CreateButton(tabs.transform, "RegisterTab", "REGISTER", TabIdle, TextLight, 22, 3,
                                       out registerTabImg, out registerTabTxt);
        loginTabImg.sprite = gradSprite;
        registerTabImg.sprite = gradSprite;
        AddBorder(loginTab.gameObject, Border, ControlMult);
        AddBorder(registerTab.gameObject, Border, ControlMult);

        // ----- Fields -----
        var username = CreateField(panel.transform, "USERNAME", "Choose your name", true,
                                   TMP_InputField.ContentType.Standard, false);
        var email = CreateField(panel.transform, "EMAIL", "you@example.com", true,
                                TMP_InputField.ContentType.EmailAddress, false);
        var password = CreateField(panel.transform, "PASSWORD", "Your password", false,
                                   TMP_InputField.ContentType.Password, true);
        var confirm = CreateField(panel.transform, "CONFIRM PASSWORD", "Repeat your password", false,
                                  TMP_InputField.ContentType.Password, false);

        // ----- Status text -----
        var status = CreateText(panel.transform, "Status", "", 20, TextMuted,
                                TextAlignmentOptions.Center, FontStyles.Normal, 0, false);
        status.enableWordWrapping = true;
        status.gameObject.AddComponent<LayoutElement>().minHeight = 26;

        // ----- Submit -----
        Image submitImg;
        TextMeshProUGUI submitTxt;
        var submit = CreateButton(panel.transform, "SubmitButton", "ENTER THE REALM", Color.white, TextLight, 24, 3,
                                  out submitImg, out submitTxt);
        submitImg.sprite = gradSprite;
        SetTint(submit, ButtonBg, ButtonHover, ButtonPress);
        AddBorder(submit.gameObject, TealDim, ControlMult);
        submit.gameObject.AddComponent<LayoutElement>().preferredHeight = 64;

        // ----- Manager + wiring -----
        var mgrGO = new GameObject("LoginManager");
        var mgr = mgrGO.AddComponent<LoginManager>();
        mgr.loginTabButton = loginTab;
        mgr.registerTabButton = registerTab;
        mgr.loginTabImage = loginTabImg;
        mgr.registerTabImage = registerTabImg;
        mgr.loginTabLabel = loginTabTxt;
        mgr.registerTabLabel = registerTabTxt;
        mgr.usernameGroup = username.root;
        mgr.confirmGroup = confirm.root;
        mgr.usernameInput = username.input;
        mgr.emailInput = email.input;
        mgr.passwordInput = password.input;
        mgr.confirmInput = confirm.input;
        mgr.togglePasswordButton = password.toggle;
        mgr.togglePasswordLabel = password.toggleLabel;
        mgr.submitButton = submit;
        mgr.submitLabel = submitTxt;
        mgr.statusText = status;
        mgr.tabActive = TealBright;   // gradient sprite darkens it toward the corner
        mgr.tabIdle = TabIdle;

        // Register-only fields start hidden
        username.root.SetActive(false);
        confirm.root.SetActive(false);

        // ----- Save + build settings -----
        EditorSceneManager.SaveScene(scene, ScenePath);
        AddToBuildSettingsFirst(ScenePath);
        Selection.activeGameObject = mgrGO;

        Debug.Log("Login scene created at " + ScenePath + (font != null ? " (using font: " + font.name + ")" : " (default font - add Cinzel for the fantasy look)") +
                  ". Select the LoginManager object to set your PHP URLs.");
    }

    // ---------- Builders ----------

    static Field CreateField(Transform parent, string label, string placeholder, bool light,
                             TMP_InputField.ContentType type, bool withToggle)
    {
        var f = new Field();
        f.root = NewUI(label + " Field", parent);

        var v = f.root.AddComponent<VerticalLayoutGroup>();
        v.spacing = 6;
        v.childControlWidth = true;
        v.childControlHeight = true;
        v.childForceExpandWidth = true;
        v.childForceExpandHeight = false;

        var lbl = CreateText(f.root.transform, "Label", label, 18, TextMuted,
                             TextAlignmentOptions.Left, FontStyles.Normal, 4);
        lbl.gameObject.AddComponent<LayoutElement>().preferredHeight = 22;

        // Input background
        var inputGO = NewUI("Input", f.root.transform);
        var img = inputGO.AddComponent<Image>();
        img.sprite = fillSprite;
        img.type = Image.Type.Sliced;
        img.pixelsPerUnitMultiplier = ControlMult;
        img.color = light ? LightField : DarkField;
        Color idleBorder = light ? new Color(Teal.r, Teal.g, Teal.b, 0f) : TealDim;
        var borderImg = AddBorder(inputGO, idleBorder, ControlMult);
        inputGO.AddComponent<LayoutElement>().preferredHeight = 60;

        // Text area (children first, then the TMP_InputField component)
        var viewport = NewUI("Text Area", inputGO.transform);
        var vpRT = viewport.GetComponent<RectTransform>();
        Stretch(vpRT, 20, 6, withToggle ? 96 : 20, 6);
        viewport.AddComponent<RectMask2D>();

        var ph = CreateText(viewport.transform, "Placeholder", placeholder, 24,
                            light ? HintDark : TextMuted, TextAlignmentOptions.Left, FontStyles.Italic, 0, false);
        StretchFull(ph.rectTransform);
        var tx = CreateText(viewport.transform, "Text", "", 24,
                            light ? TextDark : TextLight, TextAlignmentOptions.Left, FontStyles.Normal, 0, false);
        StretchFull(tx.rectTransform);

        var input = inputGO.AddComponent<TMP_InputField>();
        input.textViewport = vpRT;
        input.textComponent = tx;
        input.placeholder = ph;
        input.targetGraphic = img;
        input.lineType = TMP_InputField.LineType.SingleLine;
        input.contentType = type;
        input.customCaretColor = true;
        input.caretColor = light ? TextDark : Teal;
        input.selectionColor = new Color(Teal.r, Teal.g, Teal.b, 0.35f);
        f.input = input;

        var focus = inputGO.AddComponent<InputFocusBorder>();
        focus.border = borderImg;
        focus.idleColor = idleBorder;
        focus.focusColor = Teal;

        // Show / hide password toggle
        if (withToggle)
        {
            Image tImg;
            TextMeshProUGUI tTxt;
            var tb = CreateButton(inputGO.transform, "ToggleVisibility", "SHOW", Color.clear, Teal, 16, 2,
                                  out tImg, out tTxt);
            var rt = tb.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(1, 0);
            rt.anchorMax = new Vector2(1, 1);
            rt.pivot = new Vector2(1, 0.5f);
            rt.sizeDelta = new Vector2(84, -10);
            rt.anchoredPosition = new Vector2(-8, 0);
            f.toggle = tb;
            f.toggleLabel = tTxt;
        }

        return f;
    }

    static Button CreateButton(Transform parent, string name, string label, Color bg, Color textColor,
                               float size, float spacing, out Image image, out TextMeshProUGUI text)
    {
        var go = NewUI(name, parent);
        image = go.AddComponent<Image>();
        image.sprite = fillSprite;
        image.type = Image.Type.Sliced;
        image.pixelsPerUnitMultiplier = ControlMult;
        image.color = bg;

        var b = go.AddComponent<Button>();
        b.targetGraphic = image;
        SetTint(b, Color.white, new Color(0.88f, 0.88f, 0.88f, 1f), new Color(0.7f, 0.7f, 0.7f, 1f));

        text = CreateText(go.transform, "Label", label, size, textColor,
                          TextAlignmentOptions.Center, FontStyles.Bold, spacing);
        StretchFull(text.rectTransform);
        return b;
    }

    /// <summary>fancy = use the Cinzel font if available (titles, labels, buttons). Typed text stays readable.</summary>
    static TextMeshProUGUI CreateText(Transform parent, string name, string content, float size, Color color,
                                      TextAlignmentOptions align, FontStyles style, float spacing, bool fancy = true)
    {
        var go = NewUI(name, parent);
        var t = go.AddComponent<TextMeshProUGUI>();
        t.text = content;
        t.fontSize = size;
        t.color = color;
        t.alignment = align;
        t.characterSpacing = spacing;
        t.enableWordWrapping = false;
        t.raycastTarget = false;

        if (fancy && font != null)
        {
            t.font = font;
            t.fontStyle = FontStyles.Normal;   // the Cinzel asset is already bold-ish; avoid double bold
        }
        else
        {
            t.fontStyle = style;
        }
        return t;
    }

    // ---------- Helpers ----------

    static GameObject NewUI(string name, Transform parent)
    {
        var go = new GameObject(name, typeof(RectTransform));
        go.transform.SetParent(parent, false);
        return go;
    }

    static void Stretch(RectTransform rt, float left, float top, float right, float bottom)
    {
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = new Vector2(left, bottom);
        rt.offsetMax = new Vector2(-right, -top);
    }

    static void StretchFull(RectTransform rt) { Stretch(rt, 0, 0, 0, 0); }

    /// <summary>Thin crisp border drawn as a separate ring sprite on top of the fill.</summary>
    static Image AddBorder(GameObject target, Color color, float mult)
    {
        var go = NewUI("Border", target.transform);
        StretchFull(go.GetComponent<RectTransform>());
        var img = go.AddComponent<Image>();
        img.sprite = ringSprite;
        img.type = Image.Type.Sliced;
        img.pixelsPerUnitMultiplier = mult;
        img.color = color;
        img.raycastTarget = false;
        go.AddComponent<LayoutElement>().ignoreLayout = true;
        return img;
    }

    /// <summary>Hover / press colors. 'normal' is multiplied with the Image color.</summary>
    static void SetTint(Button b, Color normal, Color hover, Color pressed)
    {
        var c = b.colors;
        c.normalColor = normal;
        c.highlightedColor = hover;
        c.selectedColor = normal;   // don't stay highlighted after a click
        c.pressedColor = pressed;
        c.disabledColor = new Color(normal.r, normal.g, normal.b, 0.5f);
        c.colorMultiplier = 1f;
        c.fadeDuration = 0.12f;
        b.colors = c;
    }

    static Color Hex(string hex)
    {
        ColorUtility.TryParseHtmlString(hex, out var c);
        return c;
    }

    // ---------- Generated sprites (anti-aliased) ----------

    static void EnsureFolder(string path)
    {
        if (AssetDatabase.IsValidFolder(path)) return;
        string parent = Path.GetDirectoryName(path).Replace('\\', '/');
        EnsureFolder(parent);
        AssetDatabase.CreateFolder(parent, Path.GetFileName(path));
    }

    static void EnsureSprites()
    {
        EnsureFolder(SpriteFolder);
        fillSprite = MakeSprite("UI_Rounded.png", Kind.Fill);
        ringSprite = MakeSprite("UI_RoundedRing.png", Kind.Ring);
        gradSprite = MakeSprite("UI_RoundedGradient.png", Kind.FillGradient);
        glowSprite = MakeSprite("UI_Glow.png", Kind.Glow);
        lineSprite = MakeSprite("UI_LineFade.png", Kind.LineFade);
    }

    static Sprite MakeSprite(string fileName, Kind kind)
    {
        string path = SpriteFolder + "/" + fileName;
        int w = 256, h = 256;
        if (kind == Kind.LineFade) { w = 256; h = 4; }
        const float radius = 64f;
        const float thickness = 6f;

        var tex = new Texture2D(w, h, TextureFormat.RGBA32, false);
        var pixels = new Color32[w * h];

        for (int y = 0; y < h; y++)
        {
            for (int x = 0; x < w; x++)
            {
                byte rgb = 255;
                float a = 1f;

                if (kind == Kind.Fill || kind == Kind.Ring || kind == Kind.FillGradient)
                {
                    float half = w * 0.5f;
                    float px = x + 0.5f - half;
                    float py = y + 0.5f - half;
                    float qx = Mathf.Abs(px) - (half - radius);
                    float qy = Mathf.Abs(py) - (half - radius);
                    float outside = new Vector2(Mathf.Max(qx, 0f), Mathf.Max(qy, 0f)).magnitude;
                    float d = outside + Mathf.Min(Mathf.Max(qx, qy), 0f) - radius;
                    float outer = Mathf.Clamp01(0.5f - d);
                    a = outer;

                    if (kind == Kind.Ring)
                    {
                        float inner = Mathf.Clamp01(0.5f - (d + thickness));
                        a = Mathf.Clamp01(outer - inner);
                    }
                    else if (kind == Kind.FillGradient)
                    {
                        // light at top-left, ~25% darker at bottom-right
                        float t = 0.5f * (x / (float)(w - 1)) + 0.5f * (1f - y / (float)(h - 1));
                        rgb = (byte)Mathf.RoundToInt(255f - 62f * t);
                    }
                }
                else if (kind == Kind.Glow)
                {
                    float nx = (x + 0.5f - w * 0.5f) / (w * 0.5f);
                    float ny = (y + 0.5f - h * 0.5f) / (h * 0.5f);
                    float dist = Mathf.Sqrt(nx * nx + ny * ny);
                    a = Mathf.Clamp01(1f - dist);
                    a = a * a;
                }
                else if (kind == Kind.LineFade)
                {
                    a = 1f - x / (float)(w - 1);
                }

                pixels[y * w + x] = new Color32(rgb, rgb, rgb, (byte)Mathf.RoundToInt(Mathf.Clamp01(a) * 255f));
            }
        }

        tex.SetPixels32(pixels);
        File.WriteAllBytes(path, tex.EncodeToPNG());
        Object.DestroyImmediate(tex);

        AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);
        var importer = (TextureImporter)AssetImporter.GetAtPath(path);
        importer.textureType = TextureImporterType.Sprite;
        importer.spriteImportMode = SpriteImportMode.Single;
        if (kind == Kind.Fill || kind == Kind.Ring || kind == Kind.FillGradient)
            importer.spriteBorder = new Vector4(radius, radius, radius, radius);
        importer.alphaIsTransparency = true;
        importer.mipmapEnabled = true;
        importer.wrapMode = TextureWrapMode.Clamp;
        importer.filterMode = FilterMode.Trilinear;
        importer.textureCompression = TextureImporterCompression.Uncompressed;
        importer.SaveAndReimport();

        return AssetDatabase.LoadAssetAtPath<Sprite>(path);
    }

    /// <summary>Uses a fantasy-serif TMP font asset if one exists in the project (Cinzel preferred).</summary>
    static TMP_FontAsset FindFont()
    {
        foreach (var name in new[] { "Cinzel", "Cormorant", "Marcellus", "Trajan" })
        {
            foreach (var guid in AssetDatabase.FindAssets(name + " t:TMP_FontAsset"))
            {
                var asset = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(AssetDatabase.GUIDToAssetPath(guid));
                if (asset != null) return asset;
            }
        }
        return null; // falls back to TMP's default font
    }

    static void AddToBuildSettingsFirst(string path)
    {
        var list = new List<EditorBuildSettingsScene>(EditorBuildSettings.scenes);
        list.RemoveAll(s => s.path == path);
        list.Insert(0, new EditorBuildSettingsScene(path, true));
        EditorBuildSettings.scenes = list.ToArray();
    }
}
