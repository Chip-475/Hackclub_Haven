public interface IDamageable
{
    public float Health { get; }
    public float Defense { get; }

    void TakeDamage(float damage);
    void Die();
}
