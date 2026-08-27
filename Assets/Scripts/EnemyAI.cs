// EnemyAI.cs
// Attach to each enemy Unit GameObject. Set 'behavior' in the Inspector
// per-enemy so different enemies in the same fight (or across different
// levels) can feel different, even though they all just do basic attacks
// for now - a stronger enemy later on could check behavior to also decide
// WHICH attack to use, not just who to target.

using System.Collections.Generic;
using UnityEngine;

public class EnemyAI : MonoBehaviour
{
    public EnemyBehavior behavior = EnemyBehavior.AttackFirst;

    // Returns who this enemy should attack this turn, or null if no
    // valid target exists (shouldn't normally happen - BattleManager
    // already checks for battle end before calling this).
    public Unit ChooseTarget(List<Unit> playerUnits)
    {
        // Taunt always overrides behavior - a Heavy Knight demanding
        // attention gets attacked regardless of AI type.
        foreach (Unit u in playerUnits)
            if (u.IsAlive && u.isTaunting) return u;

        List<Unit> alive = new List<Unit>();
        foreach (Unit u in playerUnits)
            if (u.IsAlive) alive.Add(u);

        if (alive.Count == 0) return null;

        switch (behavior)
        {
            case EnemyBehavior.Random:
                return alive[Random.Range(0, alive.Count)];

            case EnemyBehavior.FocusLowestHP:
                Unit lowest = alive[0];
                float lowestPct = (float)lowest.currentHP / lowest.maxHP;
                foreach (Unit u in alive)
                {
                    float pct = (float)u.currentHP / u.maxHP;
                    if (pct < lowestPct)
                    {
                        lowest = u;
                        lowestPct = pct;
                    }
                }
                return lowest;

            case EnemyBehavior.AttackFirst:
            default:
                return alive[0];
        }
    }
}
