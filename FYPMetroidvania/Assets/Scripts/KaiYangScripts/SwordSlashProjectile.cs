using UnityEngine;

public class SwordSlashProjectile : ProjectileBase
{
    public float maxDistance = 10f;
    public float bloodCost;

    public CrowdControlState crowdControl = CrowdControlState.Stunned;
    public float ccDuration = 1.0f;
    public float X_Knockback; public float Y_Knockback;

    private Vector3 startPos;
    private Health playerHealth;
    private Hitbox hitbox;
    private bool hasInvokedStart = false;

    private void OnEnable()
    {
        startPos = transform.position;
        playerHealth = PlayerController.instance.GetComponent<Health>();
        hitbox = GetComponent<Hitbox>();
        hasInvokedStart = false;
    }

    private void Start()
    {
        if (hitbox != null && !hasInvokedStart)
        {
            hasInvokedStart = true;
            Skills.InvokeSkillStart(hitbox);
            
        }
    }

    protected override void Move()
    {
        if (Vector3.Distance(startPos, transform.position) >= maxDistance)
            Despawn();
    }

    public void Init(Vector2 dir)
    {
        if (!rb) rb = GetComponent<Rigidbody2D>();
        rb.linearVelocity = dir.normalized * speed;
        if(dir.x < 0)
        {
            transform.localScale = new Vector3(-Mathf.Abs(transform.localScale.x), transform.localScale.y, transform.localScale.z);
        }

        if (hitbox != null && !hasInvokedStart)
        {
            hasInvokedStart = true;
            Skills.InvokeSkillStart(hitbox);
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        Health enemy = collision.GetComponent<Health>();
        if (enemy != null && !enemy.isPlayer)
        {
            if (hitbox != null)
            {
                Skills.InvokeSkillHit(hitbox, enemy);
            }

            float directionalXknockback = rb.linearVelocity.x > 0 ? X_Knockback : -X_Knockback;
            // Damage without knockback (CC handles it)
            enemy.TakeDamage(damage, new Vector2(directionalXknockback, Y_Knockback), false, crowdControl, ccDuration);
            enemy.ApplyBloodMark();

            // Blood cost
            if (playerHealth != null && bloodCost > 0f)
            {
                float safeCost = Mathf.Min(bloodCost, playerHealth.CurrentHealth - 1f);
                if (safeCost > 0f)
                    playerHealth.TakeDamage(safeCost);
            }

            Despawn();
        }
    }

    public override void Despawn()
    {
        Skills.InvokeSkillEnd();
        base.Despawn();
    }
}