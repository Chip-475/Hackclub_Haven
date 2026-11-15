using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class Swordsman : Enemy
{
    public enum AnimationState { Idle, Running, Attack }

    [Header("Attack Timing")]
    [SerializeField, Min(0f)] float attackWindup = 0.2f;
    [SerializeField, Min(0f)] float attackDuration = 0.4f;
    [SerializeField, Min(0f)] float attackCooldown = 1f;

    [Header("Animation")]
    [SerializeField] Animator animator;
    [SerializeField] SpriteRenderer spriteRenderer;
    public AnimationState state = AnimationState.Idle;

    public bool IsAttacking { get; private set; }
    public bool CanAttack => !isDead && !IsAttacking && Time.time >= nextAttackTime;

    Rigidbody2D body;
    float horizontalMovement;
    float attackTimer;
    float nextAttackTime;
    bool hitAttempted;

    protected override void Awake()
    {
        base.Awake();
        body = GetComponent<Rigidbody2D>();
        if (animator == null)
            animator = GetComponentInChildren<Animator>();
        if (spriteRenderer == null)
            spriteRenderer = GetComponentInChildren<SpriteRenderer>();
    }

    protected override void Update()
    {
        base.Update();
        AI();
        UpdateAnimation();
    }

    void FixedUpdate()
    {
        // Preserve gravity and collisions while approaching on the ground.
        body.linearVelocityX = !isDead && canMove && !IsAttacking ? horizontalMovement : 0f;
    }

    public void AI()
    {
        horizontalMovement = 0f;
        if (isDead)
            return;

        if (IsAttacking)
        {
            attackTimer += Time.deltaTime;
            if (!hitAttempted && attackTimer >= attackWindup)
            {
                hitAttempted = true;
                TryHitPlayer();
            }

            if (attackTimer >= attackWindup + attackDuration)
            {
                IsAttacking = false;
                nextAttackTime = Time.time + attackCooldown;
                state = AnimationState.Idle;
            }
            return;
        }

        state = AnimationState.Idle;
        if (!isDetectingPlayer || Player == null)
            return;

        float offset = Player.transform.position.x - transform.position.x;
        if (spriteRenderer != null && !Mathf.Approximately(offset, 0f))
            spriteRenderer.flipX = offset < 0f;

        // Never stop outside attack reach, even if the preferred distance is larger.
        float stoppingDistance = Mathf.Clamp(preferredDistanceFromPlayer, 0f, Mathf.Max(0f, attackRange));
        if (canMove && Mathf.Abs(offset) > stoppingDistance)
        {
            horizontalMovement = Mathf.Sign(offset) * Mathf.Max(0f, speed);
            if (!Mathf.Approximately(horizontalMovement, 0f))
                state = AnimationState.Running;
        }
        else if (DistanceFromPlayer <= attackRange && CanAttack)
        {
            horizontalMovement = 0f;
            IsAttacking = true;
            attackTimer = 0f;
            hitAttempted = false;
            state = AnimationState.Attack;
        }
    }

    void TryHitPlayer()
    {
        // Recheck at impact: the player can dodge or move behind cover during windup.
        if (Player == null || DistanceFromPlayer > attackRange || !Detect())
            return;

        IDamageable target = Player.GetComponentInParent<IDamageable>();
        if (target == null)
            target = Player.GetComponentInChildren<IDamageable>();
        if (target != null)
            target.TakeDamage(Mathf.Max(0f, attackDamage - target.Defense));
    }

    void UpdateAnimation()
    {
        if (animator == null || animator.runtimeAnimatorController == null)
            return;

        string animationName = state.ToString();
        if (animator.HasState(0, Animator.StringToHash(animationName)) &&
            !animator.GetCurrentAnimatorStateInfo(0).IsName(animationName) &&
            !(animator.IsInTransition(0) && animator.GetNextAnimatorStateInfo(0).IsName(animationName)))
            animator.CrossFade(animationName, 0f);
    }

    void OnDisable()
    {
        horizontalMovement = 0f;
        IsAttacking = false;
        state = AnimationState.Idle;
        if (body != null)
            body.linearVelocityX = 0f;
    }
}
