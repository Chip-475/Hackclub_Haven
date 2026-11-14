using UnityEngine;
using static UnityEngine.Rendering.DebugUI;

[DisallowMultipleComponent]
public class Enemy : MonoBehaviour, IDamageable
{
    [SerializeField] EnemyData data;
    bool isDead;

    public float MaxHealth { get; private set; }
    public float Health { get; private set; }
    public float AttackDamage { get; private set; }
    public float Defense { get; private set; }
    public float Speed { get; private set; }

    Vector2? destination;
    bool canMove = true;

    float IDamageable.Health
    {
        get => Health;
        set => Health = value;
    }

    private void Awake()
    {
        if (data == null)
        {
            Debug.LogError($"Enemy data is not assigned on {name}.", this);
            enabled = false;
            return;
        }

        MaxHealth = Mathf.Max(0f, data.maxHp);
        Health = MaxHealth;
        AttackDamage = data.atk;
        Defense = data.def;
        Speed = data.spd;
    }
    private void Update()
    {
        if (destination != null)
            MoveTo(destination.Value);
    }

    public void MoveTo(Vector3 coords)
    {
        destination = coords;

        if (isDead || !canMove || GameManager.UniversalPathfinder.FindPath(transform.position, coords) == null)
        {
            destination = null;
            return;
        }

        transform.position = Vector3.MoveTowards(transform.position, coords, Speed * Time.deltaTime);
        if ((coords - transform.position).sqrMagnitude < 0.1f)
            destination = null;
    }

    public void TakeDamage(float damage)
    {
        if (isDead || damage <= 0f)
            return;

        Health -= damage;
        Health = Mathf.Clamp(Health, 0f, MaxHealth);

        if (Health <= 0f)
            Die();
    }
    public void Die()
    {
        if (isDead)
            return;

        isDead = true;
        Health = 0f;
        Destroy(gameObject);
    }
}
