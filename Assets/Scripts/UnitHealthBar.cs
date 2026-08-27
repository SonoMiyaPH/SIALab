// UnitHealthBar.cs
// Attach to the same GameObject as Unit. Assign an Image (Image Type =
// Filled, Fill Method = Horizontal) to hpFillImage - it updates itself
// automatically whenever the unit takes damage, heals, or bleeds. No
// Slider, no Fill Rect wiring, no anchors to fight with - just one
// fillAmount value from 0 (empty) to 1 (full).

using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Unit))]
public class UnitHealthBar : MonoBehaviour
{
    public Image hpFillImage;

    Unit unit;

    void Awake()
    {
        unit = GetComponent<Unit>();
        unit.OnHealthChanged += HandleHealthChanged;
    }

    void Start()
    {
        // Initialize the bar to the unit's starting HP.
        HandleHealthChanged(unit.currentHP, unit.maxHP);
    }

    void HandleHealthChanged(int current, int max)
    {
        if (hpFillImage == null) return;
        hpFillImage.fillAmount = max > 0 ? (float)current / max : 0f;
    }

    void OnDestroy()
    {
        if (unit != null) unit.OnHealthChanged -= HandleHealthChanged;
    }
}