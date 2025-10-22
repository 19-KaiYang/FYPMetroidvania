using UnityEngine;

public class SwordSlashProjectile : ProjectileBase
{
    public float maxDistance = 10f;
    public float bloodCost;

    public CrowdControlState groundedCC = CrowdControlState.Stunned;
    public CrowdControlState airborneCC = CrowdControlState.Knockdown;
    public float ccDuration = 1.0f;
    public float stunKnockbackMultiplier = 1f;
    public float knockdownKnockbackMultiplier = 1f;

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

            Vector2 knockDir = (enemy.transform.position - transform.position).normalized;

            // Damage without knockback (CC handles it)
            enemy.TakeDamage(damage, knockDir, false, CrowdControlState.None, 0f, true, false, 0f);
            enemy.ApplyBloodMark();

            // Apply CC with separate multipliers
            var skills = PlayerController.instance.GetComponent<Skills>();
            if (skills != null)
            {
                skills.ApplySkillCC(enemy, knockDir, groundedCC, airborneCC, ccDuration,
                                   stunKnockbackMultiplier, knockdownKnockbackMultiplier);
            }

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