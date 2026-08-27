// TurnIndicator.cs
// Attach to the same GameObject as Unit - works for both player units
// and enemies. Assign a child GameObject (e.g. a halo/circle sprite
// positioned below the unit's feet) to indicatorVisual. It shows itself
// automatically when it's this unit's turn, and hides when the turn ends.
// Make sure indicatorVisual starts INACTIVE in the scene/prefab.

using UnityEngine;

[RequireComponent(typeof(Unit))]
public class TurnIndicator : MonoBehaviour
{
    [Tooltip("Child GameObject with the halo/circle sprite. Should start inactive.")]
    public GameObject indicatorVisual;

    Unit unit;
    BattleManager battleManager;

    void Awake()
    {
        unit = GetComponent<Unit>();
        battleManager = FindObjectOfType<BattleManager>();

        if (indicatorVisual != null) indicatorVisual.SetActive(false);
    }

    void OnEnable()
    {
        if (battleManager == null) return;
        battleManager.OnTurnStarted += HandleTurnStarted;
        battleManager.OnTurnEnded += HandleTurnEnded;
    }

    void OnDisable()
    {
        if (battleManager == null) return;
        battleManager.OnTurnStarted -= HandleTurnStarted;
        battleManager.OnTurnEnded -= HandleTurnEnded;
    }

    void HandleTurnStarted(Unit u)
    {
        if (u == unit && indicatorVisual != null) indicatorVisual.SetActive(true);
    }

    void HandleTurnEnded(Unit u)
    {
        if (u == unit && indicatorVisual != null) indicatorVisual.SetActive(false);
    }
}
