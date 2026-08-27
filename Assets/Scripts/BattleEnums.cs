// BattleEnums.cs
// Shared enums used across the battle system.

public enum BattleState
{
    Start,      // Battle is initializing
    PlayerTurn, // Waiting for the player to pick an action + target
    EnemyTurn,  // Enemy AI is acting
    Win,        // All enemies defeated
    Lose        // All player units defeated
}

public enum CharacterClass
{
    Paladin,
    Priest,
    HeavyKnight,
    Archer,
    Mage
}

// Who an ability's effect applies to. The BattleManager uses this to
// decide which list(s) of units to hand to Ability.Execute().
public enum TargetType
{
    SingleEnemy,
    SingleAlly,
    Self,
    AllEnemies,
    AllAllies
}

// Different targeting styles for enemy AI - lets you design levels
// where some enemies feel "smart" and others feel simple/random.
public enum EnemyBehavior
{
    Random,        // picks any alive player unit at random
    FocusLowestHP, // always goes after whichever player unit has the lowest HP%
    AttackFirst    // always attacks the first alive unit in the list (old default)
}