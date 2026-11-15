using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using UnityEngine;

[Serializable]
public class PlayerCombat
{
    float attack;
    float defense;

    float attackWindup = 0.1f;
    float attackDuration = 0.5f;
    float attackCooldown = 0.5f;
    bool onCd;

    Collider2D hitCollider;
    readonly HashSet<Collider2D> enemiesInHitbox = new();

    public bool CanAttack { get; private set; }
    public bool IsAttacking { get; private set; }

    public PlayerCombat(PlayerManager manager)
    {
        attack = manager.playerData.atk;
        defense = manager.playerData.def;
        hitCollider = manager.hitCollider;

        if (hitCollider != null)
            hitCollider.enabled = false;
    }

    public void Tick()
    {
        if (hitCollider.enabled)
            TryHit();
        else enemiesInHitbox.Clear();
    }

    public IEnumerator PlayerAttack()
    {
        if (IsAttacking || onCd)
            yield break;

        IsAttacking = true;

        // Attack phase.
        yield return new WaitForSeconds(attackWindup);
        hitCollider.enabled = true;
        yield return new WaitForSeconds(attackDuration);
        hitCollider.enabled = false;
        IsAttacking = false;

        // Cooldown begins after the complete attack animation.
        onCd = true;
        yield return new WaitForSeconds(attackCooldown);
        onCd = false;
    }

    public void CancelAttack()
    {
        IsAttacking=false; onCd=false;
        if(hitCollider!=null) hitCollider.enabled=false;
        enemiesInHitbox.Clear();
    }

    void TryHit()
    {
        if (!hitCollider.enabled)
            return;

        ContactFilter2D filter = new()
        {
            useLayerMask = false,
            useDepth = false,
            useNormalAngle = false,
            useTriggers = true
        };
        List<Collider2D> results = new();

        hitCollider.Overlap(filter, results);

        foreach (Collider2D collider in results)
        {
            if (collider == null || !collider.CompareTag("Enemy") || !collider.gameObject.TryGetComponent(out IDamageable damageable) || enemiesInHitbox.Contains(collider))
                continue;

            enemiesInHitbox.Add(collider);
            float damage = attack - damageable.Defense;
            damageable.TakeDamage(damage);
        }
    }
}
