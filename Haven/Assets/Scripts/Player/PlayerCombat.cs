using System.Collections.Generic;
using UnityEngine;

public class PlayerCombat
{
    public enum AttackStage
    {
        Ready,
        Windup,
        Active,
        Winddown,
        Cooldown
    }

    float attack;
    float defense;

    float attackCooldown = 0.5f;
    float cooldownTimer;

    Collider2D attackHitbox;
    readonly List<Collider2D> overlapResults = new();
    readonly HashSet<GameObject> enemiesInHitbox = new();

    public AttackStage CurrentStage { get; private set; } = AttackStage.Ready;
    public bool CanAttack => CurrentStage == AttackStage.Ready;
    public bool IsAttacking { get; private set; }
    public IReadOnlyCollection<GameObject> EnemiesInHitbox => enemiesInHitbox;

    public PlayerCombat(PlayerManager manager)
    {
        attack = manager.playerData.atk;
        defense = manager.playerData.def;
        attackHitbox = manager.attackHitbox;

        if (attackHitbox != null)
            attackHitbox.enabled = false;
    }

    public void Tick()
    {
        if (CurrentStage == AttackStage.Active)
            RefreshEnemiesInHitbox();
        else
            enemiesInHitbox.Clear();

        if (CurrentStage != AttackStage.Cooldown)
            return;

        cooldownTimer -= Time.deltaTime;
        if (cooldownTimer <= 0f)
            CurrentStage = AttackStage.Ready;
    }

    public bool TryStartAttack()
    {
        if (!CanAttack)
            return false;

        CurrentStage = AttackStage.Windup;
        IsAttacking = true;
        return true;
    }

    // Call from an animation event on the first active attack frame.
    public void BeginActiveFrames()
    {
        if (CurrentStage != AttackStage.Windup)
            return;

        CurrentStage = AttackStage.Active;
        if (attackHitbox != null)
            attackHitbox.enabled = true;
    }

    // Call from an animation event on the last active attack frame.
    public void BeginWinddown()
    {
        if (CurrentStage != AttackStage.Active)
            return;

        if (attackHitbox != null)
            attackHitbox.enabled = false;

        CurrentStage = AttackStage.Winddown;
    }

    // Call from an animation event when the winddown animation ends.
    public void BeginCooldown()
    {
        if (CurrentStage != AttackStage.Winddown)
            return;

        IsAttacking = false;
        cooldownTimer = attackCooldown;
        CurrentStage = AttackStage.Cooldown;
    }

    public void Interrupt()
    {
        if (attackHitbox != null)
            attackHitbox.enabled = false;

        enemiesInHitbox.Clear();
        overlapResults.Clear();
        IsAttacking = false;
        cooldownTimer = 0f;
        CurrentStage = AttackStage.Ready;
    }

    void RefreshEnemiesInHitbox()
    {
        enemiesInHitbox.Clear();
        overlapResults.Clear();

        if (attackHitbox == null || !attackHitbox.enabled)
            return;

        ContactFilter2D filter = new ContactFilter2D
        {
            useLayerMask = false,
            useDepth = false,
            useNormalAngle = false,
            useTriggers = true
        };

        attackHitbox.Overlap(filter, overlapResults);

        foreach (Collider2D other in overlapResults)
        {
            if (other == null)
                continue;

            for (Transform candidate = other.transform; candidate != null; candidate = candidate.parent)
            {
                if (!candidate.CompareTag("Enemy"))
                    continue;

                enemiesInHitbox.Add(candidate.gameObject);
                break;
            }
        }
    }
}
