using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Attach to any UI Button that should match the "Destiny Unchained"
/// menu style: off-white rounded panel, bold dark text, subtle
/// hover/press/disabled feedback.
///
/// Setup:
///  1. Add this component to your Button GameObject (the one that
///     already has a Button + Image component).
///  2. Assign the Label field to the child TMP text (e.g. "Play").
///  3. On Awake this overwrites the Button's Color Tint colors and
///     transition settings automatically. You can still tweak the
///     hex fields below in the Inspector per-button if needed.
/// </summary>
[RequireComponent(typeof(Button))]
[RequireComponent(typeof(Image))]
public class StyledMenuButton : MonoBehaviour
{
    [Header("Text label (optional but recommended)")]
    public TextMeshProUGUI Label;

    [Header("Panel colors")]
    public Color NormalColor      = HexColor("#EDEBE6");
    public Color HighlightedColor = HexColor("#FFFFFF"); // brighter on hover
    public Color PressedColor     = HexColor("#C9C6BF"); // darker on click
    public Color DisabledColor    = HexColor("#8A8A8A", 0.5f);

    [Header("Text colors")]
    public Color TextNormalColor      = HexColor("#1F1F1F");
    public Color TextHighlightedColor = HexColor("#000000");
    public Color TextPressedColor     = HexColor("#000000");
    public Color TextDisabledColor    = HexColor("#4A4A4A", 0.6f);

    [Header("Feel")]
    [Range(0.01f, 0.3f)] public float FadeDuration = 0.08f;
    [Range(1f, 1.15f)]   public float HoverScale = 1.03f;

    private Button _button;
    private Vector3 _baseScale;

    private void Awake()
    {
        _button = GetComponent<Button>();
        _baseScale = transform.localScale;

        _button.transition = Selectable.Transition.ColorTint;
        var colors = _button.colors;
        colors.normalColor      = NormalColor;
        colors.highlightedColor = HighlightedColor;
        colors.pressedColor     = PressedColor;
        colors.selectedColor    = HighlightedColor;
        colors.disabledColor    = DisabledColor;
        colors.fadeDuration     = FadeDuration;
        colors.colorMultiplier  = 1f;
        _button.colors = colors;

        _button.onClick.AddListener(() => { }); // hook point for SFX etc.
    }

    private void Update()
    {
        if (Label == null) return;

        Label.color = !_button.interactable ? TextDisabledColor
                     : EventSystemPressed()   ? TextPressedColor
                     : EventSystemHovered()   ? TextHighlightedColor
                     : TextNormalColor;

        Vector3 target = EventSystemHovered() && _button.interactable
            ? _baseScale * HoverScale
            : _baseScale;
        transform.localScale = Vector3.Lerp(transform.localScale, target, Time.unscaledDeltaTime / FadeDuration);
    }

    private bool EventSystemHovered()
    {
        var es = UnityEngine.EventSystems.EventSystem.current;
        return es != null && es.currentSelectedGameObject == gameObject
               || IsPointerOver();
    }

    private bool IsPointerOver()
    {
        var es = UnityEngine.EventSystems.EventSystem.current;
        if (es == null) return false;
        var ped = new UnityEngine.EventSystems.PointerEventData(es) { position = Input.mousePosition };
        var results = new System.Collections.Generic.List<UnityEngine.EventSystems.RaycastResult>();
        es.RaycastAll(ped, results);
        foreach (var r in results) if (r.gameObject == gameObject) return true;
        return false;
    }

    private bool EventSystemPressed() => Input.GetMouseButton(0) && EventSystemHovered();

    private static Color HexColor(string hex, float alpha = 1f)
    {
        ColorUtility.TryParseHtmlString(hex, out var c);
        c.a = alpha;
        return c;
    }
}
