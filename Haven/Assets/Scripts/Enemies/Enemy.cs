using Unity.VisualScripting;
using UnityEngine;
using static UnityEngine.Rendering.DebugUI;
using static UnityEngine.UI.Image;

[DisallowMultipleComponent]
public class Enemy : MonoBehaviour, IDamageable
{
    [SerializeField] EnemyData data;

    [Header("Stats")]
    [SerializeField] float maxHealth;
    [SerializeField] float health;
    [SerializeField] protected float attackDamage;
    [SerializeField] float defense;
    [SerializeField] protected float speed;
    [Space]
    [SerializeField] protected float attackRange;
    [SerializeField] float detectionRange;
    [SerializeField] protected float preferredDistanceFromPlayer;

    [Header("Runtime")]
    [SerializeField] protected bool isDead;
    [SerializeField] protected bool isPlayerInDetectionRange;
    [SerializeField] protected bool isDetectingPlayer;
    [SerializeField] protected bool canMove = true;

    [Header("Animation")]
    public Animator animator;
    public EnemyAnimations Animations;
    public EnemyAnimations.AnimationState state = EnemyAnimations.AnimationState.Idle;

    Vector2? destination;

    protected GameObject Player => PlayerManager.Instance != null ? PlayerManager.Instance.playerObj : null;

    public float DistanceFromPlayer => Player != null
        ? Vector2.Distance(Player.transform.position, transform.position)
        : float.PositiveInfinity;

    public float MaxHealth => maxHealth;
    public float Health => health;
    public float Defense => defense;

    protected virtual void Awake()
    {
        if (data == null)
        {
            Debug.LogError($"Enemy data is not assigned on {name}.", this);
            enabled = false;
            return;
        }

        maxHealth = Mathf.Max(0f, data.maxHp);
        health = maxHealth;
        attackDamage = data.atk;
        defense = data.def;
        speed = data.spd;

        attackRange = data.attackRange;
        detectionRange = data.detectionRange;
        preferredDistanceFromPlayer = data.preferredDistanceFromPlayer;

        if (animator == null)
            animator = GetComponentInChildren<Animator>();
        Animations = new(this);
        gameObject.AddComponent<EnemyPresentation>();
    }
    protected virtual void Update()
    {
        isPlayerInDetectionRange = !isDead && DistanceFromPlayer <= detectionRange;
        isDetectingPlayer = isPlayerInDetectionRange && Detect();
    }

    protected virtual void LateUpdate()
    {
        // Apply the state after the enemy's AI has finished updating it.
        Animations?.Tick();
    }

    public bool Detect()
    {
        if (isDead || Player == null || DistanceFromPlayer > detectionRange)
            return false;

        Vector3 origin = transform.position;
        Vector3 playerPos = Player.transform.position;

        int ignoreMask = ~(1 << LayerMask.NameToLayer("Enemy"));
        RaycastHit2D hit = Physics2D.Raycast(
            origin,
            playerPos - origin,
            detectionRange,
            ignoreMask
        );
        bool detected = hit.collider != null &&
            (hit.transform == Player.transform || hit.transform.IsChildOf(Player.transform));

        Debug.DrawRay(
            origin,
            (playerPos - origin).normalized * detectionRange,
            detected ? Color.green : Color.red
        );

        return detected;
    }

    public void MoveTo(Vector3 coords)
    {
        destination = coords;

        if (isDead || !canMove || GameManager.UniversalPathfinder.FindPath(transform.position, coords) == null)
        {
            destination = null;
            return;
        }

        transform.position = Vector3.MoveTowards(transform.position, coords, speed * Time.deltaTime);
        if ((coords - transform.position).sqrMagnitude < 0.1f)
            destination = null;
    }

    public void TakeDamage(float damage)
    {
        if (isDead || damage <= 0f)
            return;

        health -= damage;
        health = Mathf.Clamp(health, 0f, maxHealth);

        if (health <= 0f)
            Die();
    }
    public void Die()
    {
        if (isDead)
            return;

        isDead = true;
        health = 0f;
        Destroy(gameObject);
    }
}
