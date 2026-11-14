public interface IDamageable
{
    public float Health { get; set; }

    void TakeDamage(float damage);
    void Die();
}
