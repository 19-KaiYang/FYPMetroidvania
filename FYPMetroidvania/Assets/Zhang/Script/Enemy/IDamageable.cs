using UnityEngine;

public interface IDamageable
{
    void TakeDamage(float _damage, Vector2 _dir);
    void Die();
}
