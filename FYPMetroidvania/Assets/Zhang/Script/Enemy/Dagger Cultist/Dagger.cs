using UnityEngine;

public class Dagger : ProjectileBase
{
    private PlayerController player;
    private Vector3 playerPosition;

    protected override void Awake()
    {
        base.Awake();
        player = FindFirstObjectByType<PlayerController>();
        playerPosition = player.transform.position;
        Vector2 dir = (playerPosition - transform.position).normalized;
        rb.linearVelocity = dir * speed;

        Vector3 f = transform.localScale;
        if (playerPosition.x < transform.position.x) f.y = -1;
        else f.y = 1;
        transform.localScale = f;
    }

    protected override void Move()
    {
          
    }

    protected override void Update()
    {
        base.Update();

        if (rb != null)
        {
            Vector2 v = rb.linearVelocity;
            if (v.sqrMagnitude > 0.01f)
            {
                float angle = Mathf.Atan2(v.y, v.x) * Mathf.Rad2Deg;
                transform.rotation = Quaternion.Euler(0, 0, angle);
            }
        }
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        Health player = collision.GetComponent<Health>();
        if (player != null && player.isPlayer)
        {
            player.TakeDamage(damage);

            //Rigidbody2D rbEnemy = player.GetComponent<Rigidbody2D>();
            //if (rbEnemy != null)
            //{
            //    Vector2 knockDir = (player.transform.position - transform.position).normalized;
            //    ApplyKnockback(player, knockDir);
            //}

            Despawn();
        }
        if (!collision.CompareTag("Enemy") && !collision.CompareTag("Hurtbox"))
        {
            Despawn();
        }
    }
}
