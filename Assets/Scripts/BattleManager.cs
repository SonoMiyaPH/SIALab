// BattleManager.cs
// This is the CORE GAME LOOP, now driven by real player input instead of
// auto-behavior:
// PlayerTurn (wait for a chosen action + target for each alive unit) ->
// EnemyTurn (still automatic - real enemy AI comes in Part 7) ->
// check win/lose -> repeat.
//
// How the player-turn flow works:
//   1. For the acting unit, BuildActionList() figures out what it can do
//      (Attack, its skills, its Ultimate if charged) and fires
//      OnActionMenuNeeded so BattleUIManager can show buttons.
//   2. UI calls SubmitAction(index) when the player picks one.
//   3. If that action needs a single target, BeginTargeting() fires
//      OnTargetPromptNeeded and waits for a click on a unit (handled by
//      UnitClickTarget, which calls SubmitTarget()).
//   4. ResolveAction() actually applies the attack/ability, then the
//      coroutine moves on to the next unit.

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BattleManager : MonoBehaviour
{
    [Header("Setup - drag your Unit GameObjects here")]
    public List<Unit> playerUnits = new List<Unit>();
    public List<Unit> enemyUnits = new List<Unit>();

    [Header("Pacing")]
    public float delayBetweenActions = 0.6f; // seconds, just so results are readable

    public BattleState CurrentState { get; private set; } = BattleState.Start;

    // ---------- Events the UI listens to ----------
    public event Action<Unit, List<string>> OnActionMenuNeeded;
    public event Action OnActionMenuHide;
    public event Action<List<Unit>> OnTargetPromptNeeded;
    public event Action OnTargetPromptHide;

    // Fired when a unit's turn begins/ends - used by TurnIndicator to show
    // a "it's your turn" halo under whichever unit (player or enemy) is
    // currently acting.
    public event Action<Unit> OnTurnStarted;
    public event Action<Unit> OnTurnEnded;

    // ---------- State the UI reads to validate target clicks ----------
    public bool waitingForTarget { get; private set; }
    public List<Unit> pendingValidTargets { get; private set; } = new List<Unit>();

    bool waitingForAction;
    Unit actingUnit;
    List<Ability> pendingActions;      // index-aligned with the labels sent to the UI; null entry = basic attack
    Ability pendingChosenAbility;      // the ability awaiting a target (null while waiting on a basic attack)
    bool pendingIsAttack;

    int roundNumber = 0;

    void Start()
    {
        StartCoroutine(BattleLoop());
    }

    IEnumerator BattleLoop()
    {
        CurrentState = BattleState.Start;
        Debug.Log("=== Battle Start ===");
        yield return new WaitForSeconds(delayBetweenActions);

        while (true)
        {
            roundNumber++;

            // --- PLAYER TURN ---
            CurrentState = BattleState.PlayerTurn;
            Debug.Log($"--- Player Turn (round {roundNumber}) ---");
            foreach (Unit attacker in playerUnits)
            {
                if (!attacker.IsAlive) continue;
                OnTurnStarted?.Invoke(attacker);

                if (attacker.ProcessStatusAtTurnStart())
                {
                    yield return new WaitForSeconds(delayBetweenActions);
                    OnTurnEnded?.Invoke(attacker);
                    if (CheckBattleEnd()) yield break;
                    continue;
                }

                yield return StartCoroutine(DoPlayerAction(attacker));

                OnTurnEnded?.Invoke(attacker);
                yield return new WaitForSeconds(delayBetweenActions);
                if (CheckBattleEnd()) yield break;
            }

            if (CheckBattleEnd()) yield break;

            // --- ENEMY TURN (still automatic - see Part 7 for real AI) ---
            CurrentState = BattleState.EnemyTurn;
            Debug.Log("--- Enemy Turn ---");
            foreach (Unit attacker in enemyUnits)
            {
                if (!attacker.IsAlive) continue;
                OnTurnStarted?.Invoke(attacker);

                if (attacker.ProcessStatusAtTurnStart())
                {
                    yield return new WaitForSeconds(delayBetweenActions);
                    OnTurnEnded?.Invoke(attacker);
                    if (CheckBattleEnd()) yield break;
                    continue;
                }

                Unit target = ChooseEnemyTarget(attacker);
                if (target == null) break;

                Debug.Log($"{attacker.unitName} attacks {target.unitName}");
                attacker.PlayAttack();
                target.TakeDamage(attacker.attack);

                OnTurnEnded?.Invoke(attacker);
                yield return new WaitForSeconds(delayBetweenActions);
                if (CheckBattleEnd()) yield break;
            }

            if (CheckBattleEnd()) yield break;
        }
    }

    // ---------- Player action flow ----------

    IEnumerator DoPlayerAction(Unit attacker)
    {
        actingUnit = attacker;
        pendingActions = BuildActionList(attacker);

        List<string> labels = new List<string>();
        foreach (Ability a in pendingActions)
            labels.Add(a == null ? "Attack" : a.abilityName);

        waitingForAction = true;
        OnActionMenuNeeded?.Invoke(attacker, labels);

        yield return new WaitUntil(() => !waitingForAction);
    }

    List<Ability> BuildActionList(Unit unit)
    {
        List<Ability> list = new List<Ability> { null }; // null = basic attack
        if (unit.skills != null) list.AddRange(unit.skills);
        if (unit.CanUseUltimate) list.Add(unit.ultimate);
        return list;
    }

    // Called by BattleUIManager when the player clicks an action button.
    public void SubmitAction(int index)
    {
        if (!waitingForAction || pendingActions == null || index < 0 || index >= pendingActions.Count) return;

        Ability chosen = pendingActions[index];
        OnActionMenuHide?.Invoke();

        if (chosen == null)
        {
            pendingIsAttack = true;
            pendingChosenAbility = null;
            BeginTargeting(AliveList(enemyUnits));
            return;
        }

        pendingIsAttack = false;
        pendingChosenAbility = chosen;

        switch (chosen.targetType)
        {
            case TargetType.SingleEnemy:
                BeginTargeting(AliveList(enemyUnits));
                break;
            case TargetType.SingleAlly:
                BeginTargeting(AliveList(playerUnits));
                break;
            default: // Self, AllEnemies, AllAllies - no manual target needed
                ResolveAction(null);
                break;
        }
    }

    void BeginTargeting(List<Unit> targets)
    {
        if (targets.Count == 0) { ResolveAction(null); return; }
        if (targets.Count == 1) { ResolveAction(targets[0]); return; } // only one option, no need to ask

        pendingValidTargets = targets;
        waitingForTarget = true;
        OnTargetPromptNeeded?.Invoke(targets);
    }

    // Called by UnitClickTarget when the player clicks a valid target unit.
    public void SubmitTarget(Unit target)
    {
        if (!waitingForTarget || !pendingValidTargets.Contains(target)) return;

        waitingForTarget = false;
        OnTargetPromptHide?.Invoke();
        ResolveAction(target);
    }

    void ResolveAction(Unit target)
    {
        if (pendingIsAttack)
        {
            if (target != null)
            {
                Debug.Log($"{actingUnit.unitName} attacks {target.unitName}");
                actingUnit.PlayAttack();
                target.TakeDamage(actingUnit.attack);
            }
            actingUnit.GainUltimateCharge();
        }
        else if (pendingChosenAbility != null)
        {
            bool wasUltimate = pendingChosenAbility == actingUnit.ultimate;
            actingUnit.PlayAttack(); // reuses the same Attack trigger for skills/ultimates for now;
                                      // see the note below if you want separate cast animations later
            pendingChosenAbility.chosenTarget = target;
            pendingChosenAbility.Execute(actingUnit, playerUnits, enemyUnits);
            pendingChosenAbility.chosenTarget = null;

            if (wasUltimate) actingUnit.ultimateCharge = 0;
            else actingUnit.GainUltimateCharge();
        }

        waitingForAction = false;
    }

    List<Unit> AliveList(List<Unit> source)
    {
        List<Unit> result = new List<Unit>();
        foreach (Unit u in source)
            if (u.IsAlive) result.Add(u);
        return result;
    }

    // Uses this enemy's EnemyAI component if it has one; otherwise falls
    // back to the simple "taunt, else first alive" rule.
    Unit ChooseEnemyTarget(Unit attacker)
    {
        EnemyAI ai = attacker.GetComponent<EnemyAI>();
        if (ai != null) return ai.ChooseTarget(playerUnits);
        return ChooseTarget(playerUnits);
    }

    // Picks a target for enemies with no EnemyAI component: prefers an
    // alive taunting unit (Heavy Knight), otherwise the first alive unit.
    Unit ChooseTarget(List<Unit> units)
    {
        foreach (Unit u in units)
            if (u.IsAlive && u.isTaunting) return u;

        foreach (Unit u in units)
            if (u.IsAlive) return u;

        return null;
    }

    // Returns true if the battle is over (and sets Win/Lose state).
    bool CheckBattleEnd()
    {
        bool allEnemiesDead = enemyUnits.TrueForAll(u => !u.IsAlive);
        bool allPlayersDead = playerUnits.TrueForAll(u => !u.IsAlive);

        if (allEnemiesDead)
        {
            CurrentState = BattleState.Win;
            Debug.Log("=== VICTORY ===");
            GameProgress.ClearLevel(CurrentLevel.World, CurrentLevel.Level);
            return true;
        }
        if (allPlayersDead)
        {
            CurrentState = BattleState.Lose;
            Debug.Log("=== DEFEAT ===");
            return true;
        }
        return false;
    }
}