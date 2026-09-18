// UnitAnimator.cs
// Attach to the same GameObject as Unit and Animator. Works identically
// for player units and enemies - it just listens to Unit's events and
// fires the matching Animator trigger. Idle needs no code at all: it's
// simply the Animator Controller's default state, and every other state
// transitions back to it automatically (see the Animator setup notes).
//
// Expects your Animator Controller to have these Trigger parameters:
//   "Attack" - plays when this unit performs a basic attack, skill, or ultimate
//   "Hurt"   - plays when this unit takes damage (and survives)
//   "Die"    - plays when this unit's HP reaches 0

using UnityEngine;

[RequireComponent(typeof(Unit))]
[RequireComponent(typeof(Animator))]
public class UnitAnimator : MonoBehaviour
{
    static readonly int AttackTrigger = Animator.StringToHash("Attack");
    static readonly int HurtTrigger = Animator.StringToHash("Hurt");
    static readonly int DieTrigger = Animator.StringToHash("Die");

    Animator animator;
    Unit unit;

    void Awake()
    {
        animator = GetComponent<Animator>();
        unit = GetComponent<Unit>();
    }

    void OnEnable()
    {
        unit.OnAttackTriggered += HandleAttack;
        unit.OnDamaged += HandleDamaged;
    }

    void OnDisable()
    {
        unit.OnAttackTriggered -= HandleAttack;
        unit.OnDamaged -= HandleDamaged;
    }

    void HandleAttack()
    {
        animator.SetTrigger(AttackTrigger);
    }

    void HandleDamaged()
    {
        animator.SetTrigger(unit.IsAlive ? HurtTrigger : DieTrigger);
    }
}
