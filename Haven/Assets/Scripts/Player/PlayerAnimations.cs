using System.Diagnostics;
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

    public PlayerAnimations(PlayerManager manager) => animator = manager.animator;

    public void Tick() => ChangeAnimations(PlayerManager.Instance.state.ToString());

    public void ChangeAnimations(string anim)
    {
        if (!animator.GetCurrentAnimatorStateInfo(0).IsName(anim))
            animator.CrossFade(anim, 0);
    }
}
