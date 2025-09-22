using UnityEngine;

public class SwordSlashProjectile : ProjectileBase
{
    public float maxDistance = 10f;
    public float bloodCost;
    public float knockbackForce = 8f;
    public float speed;

    private Vector3 startPos;
    private Health playerHealth;

    private void OnEnable()
    {
        startPos = transform.position;
        playerHealth = PlayerController.instance.GetComponent<Health>();
    }

    protected override void Move()
    {
        // SwordSlash doesn't self-move, just dies after distance
        if (Vector3.Distance(startPos, transform.position) >= maxDistance)
            gameObject.SetActive(false);
    }

    public void Init(Vector2 dir)
    {
        if (!rb) rb = GetComponent<Rigidbody2D>();
        rb.linearVelocity = dir.normalized * speed;
    }


    private void OnTriggerEnter2D(Collider2D collision)
    {
        Health enemy = collision.GetComponent<Health>();
        if (enemy != null && !enemy.isPlayer)
        {
            enemy.TakeDamage(damage);
            enemy.ApplyBloodMark();

            Rigidbody2D rbEnemy = enemy.GetComponent<Rigidbody2D>();
            if (rbEnemy != null)
            {
                Vector2 knockDir = (enemy.transform.position - transform.position).normalized;
                rbEnemy.AddForce(knockDir * knockbackForce, ForceMode2D.Impulse);
            }

            if (playerHealth != null && bloodCost > 0f)
            {
                float safeCost = Mathf.Min(bloodCost, playerHealth.CurrentHealth - 1f);
                if (safeCost > 0f)
                    playerHealth.TakeDamage(safeCost);
            }

            gameObject.SetActive(false);
        }
    }
}
