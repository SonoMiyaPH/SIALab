// UnitClickTarget.cs
// Attach to the same GameObject as Unit, and make sure the GameObject
// also has a Collider2D (e.g. Box Collider 2D matching its sprite) so
// OnMouseDown can detect clicks. Only does something while
// BattleManager is waiting for a target AND this unit is a valid one
// (BattleManager takes care of that check).

using UnityEngine;

[RequireComponent(typeof(Unit))]
public class UnitClickTarget : MonoBehaviour
{
    public BattleManager battleManager; // auto-found in Awake if left empty

    Unit unit;

    void Awake()
    {
        unit = GetComponent<Unit>();
        if (battleManager == null)
            battleManager = FindObjectOfType<BattleManager>();
    }

    void OnMouseDown()
    {
        if (battleManager == null) return;
        if (!battleManager.waitingForTarget) return;
        if (!battleManager.pendingValidTargets.Contains(unit)) return;

        battleManager.SubmitTarget(unit);
    }
}
