// Abilities.cs
// One concrete class per skill/ultimate from the proposal doc, plus a
// factory that hands the right ones to each character.
//
// NOTE ON RENAMING: the underlying CharacterClass enum values (Paladin,
// Archer, HeavyKnight, Priest, Mage) are left as-is here since that enum
// lives in another file I don't have visibility into. Only the flavor
// text (abilityName, class names, Debug.Log lines) has been reskinned to
// the Filipino historical figures. If you want the enum values themselves
// renamed (e.g. CharacterClass.LapuLapu instead of CharacterClass.Paladin),
// send over that enum file and I'll update it + every reference to match.

using System.Collections.Generic;
using UnityEngine;

// ---------- LAPU-LAPU (was Paladin) ----------
// Datu of Mactan, defeated Magellan's forces in 1521. A defensive,
// hard-to-kill frontline warrior fits his role as the one who held the line.

public class LapuLapuFortify : Ability
{
	public LapuLapuFortify() { abilityName = "Tameng ng Mactan"; targetType = TargetType.Self; } // "Shield of Mactan"
	public override void Execute(Unit user, List<Unit> allies, List<Unit> enemies)
	{
		user.defenseBuffAmount += 10;
		Debug.Log($"{user.unitName} uses Tameng ng Mactan (+10 defense)");
	}
}

public class LapuLapuUltimate : Ability
{
	public LapuLapuUltimate() { abilityName = "Digmaan sa Mactan (Ultimate)"; targetType = TargetType.AllEnemies; } // "Battle of Mactan"
	public override void Execute(Unit user, List<Unit> allies, List<Unit> enemies)
	{
		Debug.Log($"{user.unitName} calls down Digmaan sa Mactan on all enemies!");
		foreach (Unit e in enemies)
			if (e.IsAlive) e.TakeDamage(user.attack);
	}
}

// ---------- BABAYLAN (was Priest) ----------
// Pre-colonial spiritual healer/shaman - a natural fit for the support role.
// Swap in a named historical figure (e.g. Gabriela Silang) if you'd rather
// have a specific person instead of the archetype.

public class BabaylanLunas : Ability
{
	public BabaylanLunas() { abilityName = "Lunas"; targetType = TargetType.SingleAlly; } // "Cure/Remedy"
	public override void Execute(Unit user, List<Unit> allies, List<Unit> enemies)
	{
		Unit target = (chosenTarget != null && chosenTarget.IsAlive) ? chosenTarget : LowestHpAlly(allies, user);
		Debug.Log($"{user.unitName} casts Lunas on {target.unitName}");
		target.Heal(20);
	}
}

public class BabaylanBlessing : Ability
{
	public BabaylanBlessing() { abilityName = "Pagpapala ni Bathala"; targetType = TargetType.SingleAlly; } // "Blessing of Bathala"
	public override void Execute(Unit user, List<Unit> allies, List<Unit> enemies)
	{
		Unit target = (chosenTarget != null && chosenTarget.IsAlive) ? chosenTarget : LowestHpAlly(allies, user);
		target.defenseBuffAmount += 8;
		Debug.Log($"{user.unitName} calls Pagpapala ni Bathala on {target.unitName} (+8 defense)");
	}
}

public class BabaylanUltimate : Ability
{
	public BabaylanUltimate() { abilityName = "Paghilom ng Bayan (Ultimate)"; targetType = TargetType.AllAllies; } // "Healing of the Nation"
	public override void Execute(Unit user, List<Unit> allies, List<Unit> enemies)
	{
		Debug.Log($"{user.unitName} invokes Paghilom ng Bayan - cleansing all allies!");
		foreach (Unit a in allies)
		{
			if (!a.IsAlive) continue;
			a.isStunned = false;
			a.isTaunting = false;
			a.bleedTurnsRemaining = 0;
		}
	}
}

// ---------- ANDRES BONIFACIO (was Heavy Knight) ----------
// Founder of the Katipunan, rallied the masses with the Cry of Balintawak.
// A taunting frontline fighter fits his role as the one who drew the
// Spanish forces' attention and refused to back down.

public class BonifacioTaunt : Ability
{
	public BonifacioTaunt() { abilityName = "Sigaw ng Balintawak"; targetType = TargetType.Self; } // "Cry of Balintawak"
	public override void Execute(Unit user, List<Unit> allies, List<Unit> enemies)
	{
		user.isTaunting = true;
		Debug.Log($"{user.unitName} raises the Sigaw ng Balintawak - enemies must target it now");
	}
}

public class BonifacioUltimate : Ability
{
	public BonifacioUltimate() { abilityName = "Muling Pagbangon (Ultimate)"; targetType = TargetType.Self; } // "Rise Again"
	public override void Execute(Unit user, List<Unit> allies, List<Unit> enemies)
	{
		Debug.Log($"{user.unitName} refuses to fall - Muling Pagbangon!");
		user.Heal(40);
	}
}

// ---------- HENERAL LUNA (was Archer) ----------
// Antonio Luna, fiery and disciplined general known for aggressive tactics
// and artillery use. Reskinned here as a sharp, punishing striker.

public class HeneralLunaStrike : Ability
{
	public HeneralLunaStrike() { abilityName = "Poot ni Luna"; targetType = TargetType.SingleEnemy; } // "Luna's Wrath"
	public override void Execute(Unit user, List<Unit> allies, List<Unit> enemies)
	{
		Unit target = (chosenTarget != null && chosenTarget.IsAlive) ? chosenTarget : FirstAlive(enemies);
		if (target == null) return;
		target.TakeDamage(user.attack / 2);
		target.bleedDamagePerTurn = 5;
		target.bleedTurnsRemaining = 3;
		Debug.Log($"{user.unitName} strikes {target.unitName} with Poot ni Luna (bleed x3 turns)");
	}
}

public class HeneralLunaUltimate : Ability
{
	public HeneralLunaUltimate() { abilityName = "Kulog ng Kanyon (Ultimate)"; targetType = TargetType.AllEnemies; } // "Thunder of Cannons"
	public override void Execute(Unit user, List<Unit> allies, List<Unit> enemies)
	{
		Debug.Log($"{user.unitName} unleashes Kulog ng Kanyon on all enemies!");
		foreach (Unit e in enemies)
			if (e.IsAlive) e.TakeDamage(user.attack);
	}
}

// ---------- HERMANO PULE (was Mage) ----------
// Apolinario de la Cruz, founder of the Cofradia de San Jose, revered by
// followers as having mystical/divine power - fits the caster archetype.
// Swap for another figure if you'd prefer someone else here.

public class HermanoPuleBulong : Ability
{
	public HermanoPuleBulong() { abilityName = "Bulong ng Espiritu"; targetType = TargetType.SingleEnemy; } // "Whisper of the Spirit"
	public override void Execute(Unit user, List<Unit> allies, List<Unit> enemies)
	{
		Unit target = (chosenTarget != null && chosenTarget.IsAlive) ? chosenTarget : FirstAlive(enemies);
		if (target == null) return;
		target.isStunned = true;
		Debug.Log($"{user.unitName} stuns {target.unitName} with Bulong ng Espiritu");
	}
}

public class HermanoPuleUltimate : Ability
{
	public HermanoPuleUltimate() { abilityName = "Poot ng Bathala (Ultimate)"; targetType = TargetType.AllEnemies; } // "Wrath of Bathala"
	public override void Execute(Unit user, List<Unit> allies, List<Unit> enemies)
	{
		Debug.Log($"{user.unitName} calls down Poot ng Bathala on all enemies!");
		foreach (Unit e in enemies)
			if (e.IsAlive) e.TakeDamage(user.attack);
	}
}

// ---------- FACTORY ----------
// Call AbilityFactory.AssignAbilities(unit) once (e.g. in Unit.Awake())
// to give a unit the skills/ultimate matching its characterClass.
// (Enum values kept as the original Paladin/Priest/HeavyKnight/Archer/Mage
// so nothing else in your project breaks - only the classes returned here
// have been renamed/reskinned.)

public static class AbilityFactory
{
	public static List<Ability> GetSkills(CharacterClass c)
	{
		switch (c)
		{
			case CharacterClass.Paladin: return new List<Ability> { new LapuLapuFortify() };
			case CharacterClass.Priest: return new List<Ability> { new BabaylanLunas(), new BabaylanBlessing() };
			case CharacterClass.HeavyKnight: return new List<Ability> { new BonifacioTaunt() };
			case CharacterClass.Archer: return new List<Ability> { new HeneralLunaStrike() };
			case CharacterClass.Mage: return new List<Ability> { new HermanoPuleBulong() };
			default: return new List<Ability>();
		}
	}

	public static Ability GetUltimate(CharacterClass c)
	{
		switch (c)
		{
			case CharacterClass.Paladin: return new LapuLapuUltimate();
			case CharacterClass.Priest: return new BabaylanUltimate();
			case CharacterClass.HeavyKnight: return new BonifacioUltimate();
			case CharacterClass.Archer: return new HeneralLunaUltimate();
			case CharacterClass.Mage: return new HermanoPuleUltimate();
			default: return null;
		}
	}
}