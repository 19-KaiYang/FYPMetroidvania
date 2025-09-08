using UnityEngine;

public class Hitbox : MonoBehaviour
{
    private CombatSystem owner;

    private void Awake()
    {
        owner = GetComponentInParent<CombatSystem>();
        gameObject.SetActive(false);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Hurtbox"))
        {
            Health h = other.GetComponentInParent<Health>();
            if (h != null)
            {
                float totalDamage = owner.GetAttackDamage() * owner.GetDamageMultiplier(owner.CurrentComboStep);
                h.TakeDamage(totalDamage);
            }
        }
    }
}
