// Unit.cs
// Attach this to every character GameObject in the battle scene
// (both your 3 chosen player units and the enemies).
// For now, stats are set directly in the Inspector. Later we'll move
// this to a ScriptableObject "CharacterData" asset so you can reuse
// the same base stats across levels without retyping them.

using System;
using System.Collections.Generic;
using UnityEngine;

public class Unit : MonoBehaviour
{
    [Header("Identity")]
    public string unitName = "Unit";
    public CharacterClass characterClass;
    public bool isPlayerUnit = true;

    [Header("Stats")]
    public int maxHP = 100;
    public int currentHP;
    public int attack = 10;
    public int defense = 5;

    [Header("Status")]
    public bool isTaunting = false;      // set true by Heavy Knight's taunt skill
    public bool isStunned = false;       // set true by Mage's stun skill
    public int defenseBuffAmount = 0;    // extra defense from Paladin/Priest buffs
    public int bleedDamagePerTurn = 0;   // set by Archer's Bleeding Shot
    public int bleedTurnsRemaining = 0;

    [Header("Abilities (auto-assigned from characterClass)")]
    public List<Ability> skills;
    public Ability ultimate;
    public int ultimateCharge = 0;
    public const int ULTIMATE_CHARGE_MAX = 3;

    // Fired whenever currentHP changes (damage, heal, bleed tick), so a
    // UnitHealthBar can update its slider without BattleManager needing
    // to know anything about UI.
    public event Action<int, int> OnHealthChanged; // (currentHP, maxHP)

    void Awake()
    {
        currentHP = maxHP;
        skills = AbilityFactory.GetSkills(characterClass);
        ultimate = AbilityFactory.GetUltimate(characterClass);
    }

    public bool IsAlive => currentHP > 0;

    public int EffectiveDefense => defense + defenseBuffAmount;

    // Basic damage formula: attacker's attack minus this unit's effective
    // defense, minimum 1 damage so fights always progress.
    public void TakeDamage(int rawAttack)
    {
        int dmg = Mathf.Max(1, rawAttack - EffectiveDefense);
        currentHP = Mathf.Max(0, currentHP - dmg);
        Debug.Log($"{unitName} took {dmg} damage ({currentHP}/{maxHP} HP left)");
        OnHealthChanged?.Invoke(currentHP, maxHP);
    }

    public void Heal(int amount)
    {
        currentHP = Mathf.Min(maxHP, currentHP + amount);
        Debug.Log($"{unitName} healed for {amount} ({currentHP}/{maxHP} HP)");
        OnHealthChanged?.Invoke(currentHP, maxHP);
    }

    public bool CanUseUltimate => ultimate != null && ultimateCharge >= ULTIMATE_CHARGE_MAX;

    public void GainUltimateCharge()
    {
        ultimateCharge = Mathf.Min(ULTIMATE_CHARGE_MAX, ultimateCharge + 1);
    }

    // Call at the start of this unit's turn, before it acts.
    // Returns true if the unit's turn should be skipped entirely (stunned).
    public bool ProcessStatusAtTurnStart()
    {
        if (bleedTurnsRemaining > 0)
        {
            currentHP = Mathf.Max(0, currentHP - bleedDamagePerTurn);
            bleedTurnsRemaining--;
            Debug.Log($"{unitName} bleeds for {bleedDamagePerTurn} ({currentHP}/{maxHP} HP left)");
            OnHealthChanged?.Invoke(currentHP, maxHP);
        }

        if (isStunned)
        {
            Debug.Log($"{unitName} is stunned and skips its turn");
            isStunned = false; // stun lasts exactly 1 turn
            return true;
        }

        return false;
    }
}