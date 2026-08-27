// Abilities.cs
// One concrete class per skill/ultimate from the proposal doc, plus a
// factory that hands the right ones to each character.

using System.Collections.Generic;
using UnityEngine;

// ---------- PALADIN ----------

public class PaladinDefenseUp : Ability
{
    public PaladinDefenseUp() { abilityName = "Fortify"; targetType = TargetType.Self; }
    public override void Execute(Unit user, List<Unit> allies, List<Unit> enemies)
    {
        user.defenseBuffAmount += 10;
        Debug.Log($"{user.unitName} uses Fortify (+10 defense)");
    }
}

public class PaladinUltimate : Ability
{
    public PaladinUltimate() { abilityName = "Radiant Smite (Ultimate)"; targetType = TargetType.AllEnemies; }
    public override void Execute(Unit user, List<Unit> allies, List<Unit> enemies)
    {
        Debug.Log($"{user.unitName} unleashes Radiant Smite on all enemies!");
        foreach (Unit e in enemies)
            if (e.IsAlive) e.TakeDamage(user.attack);
    }
}

// ---------- PRIEST ----------

public class PriestHeal : Ability
{
    public PriestHeal() { abilityName = "Heal"; targetType = TargetType.SingleAlly; }
    public override void Execute(Unit user, List<Unit> allies, List<Unit> enemies)
    {
        Unit target = (chosenTarget != null && chosenTarget.IsAlive) ? chosenTarget : LowestHpAlly(allies, user);
        Debug.Log($"{user.unitName} heals {target.unitName}");
        target.Heal(20);
    }
}

public class PriestDefenseBuffAlly : Ability
{
    public PriestDefenseBuffAlly() { abilityName = "Blessing"; targetType = TargetType.SingleAlly; }
    public override void Execute(Unit user, List<Unit> allies, List<Unit> enemies)
    {
        Unit target = (chosenTarget != null && chosenTarget.IsAlive) ? chosenTarget : LowestHpAlly(allies, user);
        target.defenseBuffAmount += 8;
        Debug.Log($"{user.unitName} casts Blessing on {target.unitName} (+8 defense)");
    }
}

public class PriestUltimate : Ability
{
    public PriestUltimate() { abilityName = "Purify (Ultimate)"; targetType = TargetType.AllAllies; }
    public override void Execute(Unit user, List<Unit> allies, List<Unit> enemies)
    {
        Debug.Log($"{user.unitName} uses Purify - removing status effects from all allies!");
        foreach (Unit a in allies)
        {
            if (!a.IsAlive) continue;
            a.isStunned = false;
            a.isTaunting = false;
            a.bleedTurnsRemaining = 0;
        }
    }
}

// ---------- HEAVY KNIGHT ----------

public class HeavyKnightTaunt : Ability
{
    // Self-targeted: forces enemies to prioritize attacking the Heavy Knight.
    // See BattleManager's enemy-targeting logic.
    public HeavyKnightTaunt() { abilityName = "Taunt"; targetType = TargetType.Self; }
    public override void Execute(Unit user, List<Unit> allies, List<Unit> enemies)
    {
        user.isTaunting = true;
        Debug.Log($"{user.unitName} taunts - enemies must target it now");
    }
}

public class HeavyKnightUltimate : Ability
{
    public HeavyKnightUltimate() { abilityName = "Second Wind (Ultimate)"; targetType = TargetType.Self; }
    public override void Execute(Unit user, List<Unit> allies, List<Unit> enemies)
    {
        Debug.Log($"{user.unitName} uses Second Wind!");
        user.Heal(40);
    }
}

// ---------- ARCHER ----------

public class ArcherBleed : Ability
{
    public ArcherBleed() { abilityName = "Bleeding Shot"; targetType = TargetType.SingleEnemy; }
    public override void Execute(Unit user, List<Unit> allies, List<Unit> enemies)
    {
        Unit target = (chosenTarget != null && chosenTarget.IsAlive) ? chosenTarget : FirstAlive(enemies);
        if (target == null) return;
        target.TakeDamage(user.attack / 2);
        target.bleedDamagePerTurn = 5;
        target.bleedTurnsRemaining = 3;
        Debug.Log($"{user.unitName} hits {target.unitName} with Bleeding Shot (bleed x3 turns)");
    }
}

public class ArcherUltimate : Ability
{
    public ArcherUltimate() { abilityName = "Rain of Arrows (Ultimate)"; targetType = TargetType.AllEnemies; }
    public override void Execute(Unit user, List<Unit> allies, List<Unit> enemies)
    {
        Debug.Log($"{user.unitName} unleashes Rain of Arrows!");
        foreach (Unit e in enemies)
            if (e.IsAlive) e.TakeDamage(user.attack);
    }
}

// ---------- MAGE ----------

public class MageStun : Ability
{
    public MageStun() { abilityName = "Arcane Stun"; targetType = TargetType.SingleEnemy; }
    public override void Execute(Unit user, List<Unit> allies, List<Unit> enemies)
    {
        Unit target = (chosenTarget != null && chosenTarget.IsAlive) ? chosenTarget : FirstAlive(enemies);
        if (target == null) return;
        target.isStunned = true;
        Debug.Log($"{user.unitName} stuns {target.unitName}");
    }
}

public class MageUltimate : Ability
{
    public MageUltimate() { abilityName = "Meteor (Ultimate)"; targetType = TargetType.AllEnemies; }
    public override void Execute(Unit user, List<Unit> allies, List<Unit> enemies)
    {
        Debug.Log($"{user.unitName} casts Meteor on all enemies!");
        foreach (Unit e in enemies)
            if (e.IsAlive) e.TakeDamage(user.attack);
    }
}

// ---------- FACTORY ----------
// Call AbilityFactory.AssignAbilities(unit) once (e.g. in Unit.Awake())
// to give a unit the skills/ultimate matching its characterClass.

public static class AbilityFactory
{
    public static List<Ability> GetSkills(CharacterClass c)
    {
        switch (c)
        {
            case CharacterClass.Paladin: return new List<Ability> { new PaladinDefenseUp() };
            case CharacterClass.Priest: return new List<Ability> { new PriestHeal(), new PriestDefenseBuffAlly() };
            case CharacterClass.HeavyKnight: return new List<Ability> { new HeavyKnightTaunt() };
            case CharacterClass.Archer: return new List<Ability> { new ArcherBleed() };
            case CharacterClass.Mage: return new List<Ability> { new MageStun() };
            default: return new List<Ability>();
        }
    }

    public static Ability GetUltimate(CharacterClass c)
    {
        switch (c)
        {
            case CharacterClass.Paladin: return new PaladinUltimate();
            case CharacterClass.Priest: return new PriestUltimate();
            case CharacterClass.HeavyKnight: return new HeavyKnightUltimate();
            case CharacterClass.Archer: return new ArcherUltimate();
            case CharacterClass.Mage: return new MageUltimate();
            default: return null;
        }
    }
}