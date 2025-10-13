using UnityEngine;

public class PixieBoltProjectile : ProjectileBase
{
    public float maxDistance = 7f;

    public Debuff pixieDustDebuff;   
    public float dustTime = 5f;      
    public int dustStacks = 1;       

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

            enemy.TakeDamage(damage, triggerEffects: false);

            if (pixieDustDebuff != null)
                pixieDustDebuff.ApplyDebuff(enemy, dustStacks, dustTime);

            Despawn();
        }
    }
}
