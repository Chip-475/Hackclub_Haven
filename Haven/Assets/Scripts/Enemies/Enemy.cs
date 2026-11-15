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
    [SerializeField] float attackDamage;
    [SerializeField] float defense;
    [SerializeField] float speed;
    [Space]
    [SerializeField] float attackRange;
    [SerializeField] float detectionRange;
    [SerializeField] float preferredDistanceFromPlayer;

    [Header("Runtime")]
    [SerializeField] bool isDead;
    [SerializeField] bool isPlayerInDetectionRange;
    [SerializeField] bool isDetectingPlayer;
    [SerializeField] bool canMove = true;

    Vector2? destination;

    public float DistanceFromPlayer => (PlayerManager.Instance.playerObj.transform.position - transform.position).sqrMagnitude;

    public float Health => health;
    public float Defense => defense;

    private void Awake()
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
    }
    private void Update()
    {
        isDetectingPlayer = Detect();
    }

    public bool Detect()
    {
        Vector3 origin = transform.position;
        Vector3 playerPos = PlayerManager.Instance.playerObj.transform.position;

        int ignoreMask = ~(1 << LayerMask.NameToLayer("Enemy"));
        RaycastHit2D hit = Physics2D.Raycast(
            origin,
            playerPos - origin,
            detectionRange,
            ignoreMask
        );
        bool detected = hit.collider != null && hit.collider.gameObject.layer == LayerMask.NameToLayer("Player");

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
