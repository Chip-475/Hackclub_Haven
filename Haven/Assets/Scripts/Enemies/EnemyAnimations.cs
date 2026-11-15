using System;
using UnityEngine;

[Serializable]
public class EnemyAnimations
{
    public enum AnimationState
    {
        Idle,
        Running,
        Attack
    }

    Animator animator;
    Enemy enemy;

    public EnemyAnimations(Enemy enemy)
    {
        this.enemy = enemy;
        animator = enemy.animator;
    }

    public void Tick() => ChangeAnimations(enemy.state.ToString());

    public void ChangeAnimations(string anim)
    {
        if (animator == null || animator.runtimeAnimatorController == null)
            return;

        if (animator.HasState(0, Animator.StringToHash(anim)) &&
            !animator.GetCurrentAnimatorStateInfo(0).IsName(anim) &&
            !(animator.IsInTransition(0) && animator.GetNextAnimatorStateInfo(0).IsName(anim)))
            animator.CrossFade(anim, 0f);
    }
}
