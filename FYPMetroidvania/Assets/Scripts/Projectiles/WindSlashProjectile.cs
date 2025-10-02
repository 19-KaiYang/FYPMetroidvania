using UnityEngine;

public class WindSlashProjectile : ProjectileBase
{
    public float maxDistance = 7f;

    public Debuff bleedDebuff;
    public float bleedTime = 3f;

    private Vector3 startPos;

    private void OnEnable()
    {
        startPos = transform.position;
        if (!rb) rb = GetComponent<Rigidbody2D>();
    }

    protected override void Move()
    {
        rb.linearVelocity = direction.normalized * speed;
        
        if (Vector3.Distance(startPos, transform.position) >= maxDistance)
            Despawn();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        Health enemy = collision.GetComponent<Health>();
        if (enemy != null && !enemy.isPlayer)
        {
            enemy.TakeDamage(damage, forceCC: CrowdControlState.Stunned, forceCCDuration: 0.5f, triggerEffects: false);
            if (bleedDebuff != null)
                bleedDebuff.ApplyDebuff(enemy, 1, bleedTime);
            Despawn();
        }
    }
}
