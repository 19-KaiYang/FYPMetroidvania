using System.Collections;
using UnityEngine;

public class SwordSlashProjectile : ProjectileBase
{
    public float maxDistance = 10f;
    public float bloodCost;

    private Vector3 startPos;
    private Health playerHealth;
    private Hitbox hitbox;
    private bool hasHit = false;

    private void Awake()
    {
        hitbox = GetComponent<Hitbox>();
    }

    private void OnEnable()
    {
        startPos = transform.position;
        hasHit = false;
        playerHealth = PlayerController.instance.GetComponent<Health>();

        // Subscribe to hitbox events
        if (hitbox != null)
        {
            Hitbox.OnHit += OnProjectileHit;
        }
    }

    private void OnDisable()
    {
        // Unsubscribe from hitbox events
        if (hitbox != null)
        {
            Hitbox.OnHit -= OnProjectileHit;
        }
    }

    protected override void Move()
    {
        if (hasHit) return; // Stop moving after hit

        if (Vector3.Distance(startPos, transform.position) >= maxDistance)
            Despawn();
    }

    public void Init(Vector2 dir)
    {
        if (!rb) rb = GetComponent<Rigidbody2D>();
        rb.linearVelocity = dir.normalized * speed;

        // Set the hitbox damage from this projectile's damage value
        if (hitbox != null)
        {
            hitbox.damage = this.damage;
        }
    }

    private void OnProjectileHit(Hitbox hb, Health enemy)
    {
        // Only respond to hits from this projectile's hitbox
        if (hb != hitbox) return;
        if (hasHit) return; // Prevent multiple hits

        if (!enemy.isPlayer)
        {
            hasHit = true;

            // Stop the projectile
            if (rb != null)
                rb.linearVelocity = Vector2.zero;

            enemy.ApplyBloodMark();

            // Apply health cost to player
            if (playerHealth != null && bloodCost > 0f)
            {
                float safeCost = Mathf.Min(bloodCost, playerHealth.CurrentHealth - 1f);
                if (safeCost > 0f)
                    playerHealth.TakeDamage(safeCost);
            }

            // Delay despawn to allow hitstop to complete
            StartCoroutine(DespawnAfterHitstop());
        }
    }

    private IEnumerator DespawnAfterHitstop()
    {
        // Wait for hitstop duration plus a small buffer
        float waitTime = (hitbox != null) ? hitbox.hitstopDuration + 0.05f : 0.1f;
        yield return new WaitForSeconds(waitTime);
        Despawn();
    }
}