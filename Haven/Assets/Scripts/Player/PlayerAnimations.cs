using UnityEngine;
using System;

[Serializable]
public class PlayerAnimations
{
    public enum AnimationState
    {
        Idle,
        StartRunning,
        Running,
        Attack
    }

    Animator animator;
    PlayerManager manager;

    public PlayerAnimations(PlayerManager manager)
    {
        this.manager = manager;
        animator = manager.animator;
    }

    public void Tick()
    {
        if (manager.Combat.IsAttacking)
            manager.state = AnimationState.Attack;
        else if (!manager.Movement.IsMoving)
            manager.state = AnimationState.Idle;
        else if (manager.state == AnimationState.StartRunning)
        {
            // The controller advances from the startup clip to the running loop.
            if (animator.GetCurrentAnimatorStateInfo(0).IsName("Running") ||
                (animator.IsInTransition(0) && animator.GetNextAnimatorStateInfo(0).IsName("Running")))
                manager.state = AnimationState.Running;
        }
        else if (manager.state != AnimationState.Running)
            manager.state = AnimationState.StartRunning;

        string animationName = manager.state == AnimationState.StartRunning
            ? "Start Running"
            : manager.state.ToString();
        ChangeAnimations(animationName);
    }

    public void ChangeAnimations(string anim)
    {
        if (!animator.GetCurrentAnimatorStateInfo(0).IsName(anim) &&
            !(animator.IsInTransition(0) && animator.GetNextAnimatorStateInfo(0).IsName(anim)))
            animator.CrossFade(anim, 0);
    }
}
