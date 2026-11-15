using UnityEngine;
using UnityEngine.InputSystem;
public class PlayerManager : MonoBehaviour, IDamageable
{
    public static PlayerManager Instance { get; private set; }

    public static PlayerInputs Inputs;
    public GameObject playerObj;
    public PlayerData playerData;
    [Space]
    public Transform groundChecker;
    public LayerMask groundLayer;

    public SpriteRenderer spriteRenderer;
    public Animator animator;
    public Collider2D hitCollider;

    public PlayerMovement Movement;
    public PlayerCombat Combat;
    public PlayerAnimations Animations;

    public PlayerAnimations.AnimationState state = PlayerAnimations.AnimationState.Idle;

    public float Health { get; private set; }
    public float Defense => playerData.def;
    public bool IsRespawning { get; private set; }
    float invulnerableUntil;

    public void TakeDamage(float damage)
    {
        if(IsRespawning || Time.time < invulnerableUntil || damage <= 0) return;
        Health = Mathf.Max(0, Health-damage);
        invulnerableUntil = Time.time + .7f;
        AudioManager.Instance?.Impact();
        if(Health<=0) Die();
    }

    [ContextMenu("die")]
    public void Die()
    {
        if(IsRespawning || GameplayDirector.Instance==null) return;
        IsRespawning=true; Inputs.Disable(); StopAllCoroutines();
        Combat.CancelAttack(); Movement.ResetActions();
        playerObj.GetComponent<Rigidbody2D>().linearVelocity=Vector2.zero;
        GameplayDirector.Instance.Bounceback(this);
    }
    public void RestoreAfterDeath(Vector3 position)
    {
        playerObj.transform.position=position;
        playerObj.GetComponent<Rigidbody2D>().linearVelocity=Vector2.zero;
        Health=playerData.maxHp; state=PlayerAnimations.AnimationState.Idle;
        animator.Play("Idle",0,0); Physics2D.SyncTransforms();
    }
    public void ResumeAfterDeath() { IsRespawning=false; invulnerableUntil=Time.time+1.5f; Inputs.Enable(); }
    void OnDestroy() { Inputs?.Dispose(); if(Instance==this) Instance=null; }

    private void Awake()
    {
        // Init
        Instance = this;

        Inputs = new PlayerInputs();
        Inputs.Enable();
        playerObj = GameObject.FindGameObjectWithTag("Player");

        Health = playerData.maxHp;
        Movement = new(this);
        Combat = new(this);
        Animations = new(this);

        Inputs.Combat.Attack.performed += ctx => StartCoroutine(Combat.PlayerAttack());
    }
    private void Update()
    {
        if(IsRespawning) return;
        Movement.Tick();
        Combat.Tick();
        Animations.Tick();
    }
}
