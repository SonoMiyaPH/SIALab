// Ability.cs
// Base class every skill and ultimate inherits from. Not a MonoBehaviour -
// abilities are plain data+logic objects created in code (see AbilityFactory),
// not things you attach in the Inspector.

using System.Collections.Generic;

public abstract class Ability
{
    public string abilityName;
    public TargetType targetType;

    // Set by BattleManager just before Execute() when the player picked
    // a specific target (SingleEnemy/SingleAlly abilities). Null means
    // "no manual target was given" - abilities fall back to auto-picking.
    public Unit chosenTarget;

    // 'user' is the unit performing the ability.
    // 'allies' and 'enemies' are the FULL alive lists from the user's
    // point of view (allies = user's own side, enemies = the other side) -
    // it's up to each ability to pick from them based on its targetType.
    public abstract void Execute(Unit user, List<Unit> allies, List<Unit> enemies);

    protected Unit FirstAlive(List<Unit> units)
    {
        foreach (Unit u in units)
            if (u.IsAlive) return u;
        return null;
    }

    // Picks the lowest-HP% ally (excluding the user) - useful for heal/buff
    // targeting so the Priest doesn't waste heals on a full-HP unit.
    protected Unit LowestHpAlly(List<Unit> allies, Unit exclude)
    {
        Unit best = null;
        float bestPct = 999f;
        foreach (Unit u in allies)
        {
            if (!u.IsAlive || u == exclude) continue;
            float pct = (float)u.currentHP / u.maxHP;
            if (pct < bestPct)
            {
                bestPct = pct;
                best = u;
            }
        }
        return best != null ? best : exclude; // fall back to self if no other ally alive
    }
}