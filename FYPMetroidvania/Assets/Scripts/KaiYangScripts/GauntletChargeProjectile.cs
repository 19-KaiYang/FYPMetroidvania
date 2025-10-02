using UnityEngine;

public class GauntletChargeProjectile : ProjectileBase
{
    private float chargeRatio;
    private bool hasExploded = false;

    [Header("Visuals")]
    public SpriteRenderer spriteRenderer;
    public Sprite weakSprite;
    public Sprite midSprite;
    public Sprite strongSprite;

    [Header("Projectile Settings")]
    public float lifeTime = 3f;

    [Header("Explosion Settings")]
    public float minExplosionRadius = 1f;
    public float midExplosionRadius = 2f;
    public float maxExplosionRadius = 3f;

    private CrowdControlState groundedCC;
    private CrowdControlState airborneCC;
    private float ccDuration;

    public LayerMask enemyMask;

    private Hitbox hitbox;
    private bool hasInvokedStart = false;

    private void OnEnable()
    {
        hitbox = GetComponent<Hitbox>();
        hasInvokedStart = false;
    }

    public void Init(Vector2 dir, float dmg, float kb, float ratio,
                   CrowdControlState groundedCC, CrowdControlState airborneCC, float ccDuration)
    {
        hasExploded = false;

        damage = dmg;
        knockback = kb;
        chargeRatio = ratio;

        this.groundedCC = groundedCC;
        this.airborneCC = airborneCC;
        this.ccDuration = ccDuration;

        if (rb)
            rb.linearVelocity = dir * speed * Mathf.Lerp(1f, 1.5f, ratio);

        if (spriteRenderer != null)
        {
            if (ratio < 0.33f && weakSprite != null)
                spriteRenderer.sprite = weakSprite;
            else if (ratio < 0.66f && midSprite != null)
                spriteRenderer.sprite = midSprite;
            else if (strongSprite != null)
                spriteRenderer.sprite = strongSprite;
        }

        // Fire skill start event
        if (hitbox != null && !hasInvokedStart)
        {
            hasInvokedStart = true;
            Skills.InvokeSkillStart(hitbox);
        }

        CancelInvoke();
        Invoke(nameof(Explode), lifeTime);
    }

    protected override void Move()
    {
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (hasExploded) return; // Prevent multiple explosions

        Health target = other.GetComponent<Health>();
        if (target != null && !target.isPlayer)
        {
            Explode();
        }
    }

    private void Explode()
    {
        if (hasExploded) return;
        hasExploded = true;

        CancelInvoke();

        float radius = (chargeRatio < 0.33f) ? minExplosionRadius :
                       (chargeRatio < 0.66f) ? midExplosionRadius : maxExplosionRadius;

        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, radius, enemyMask);
        foreach (var hit in hits)
        {
            Health h = hit.GetComponent<Health>();
            if (h != null && !h.isPlayer)
            {
                // Fire skill hit event
                if (hitbox != null)
                {
                    Skills.InvokeSkillHit(hitbox, h);
                }

                h.TakeDamage(damage);

                Vector2 knockDir = (h.transform.position - transform.position).normalized;

                // Determine what CC will be applied
                Skills skills = PlayerController.instance.GetComponent<Skills>();
                if (skills != null)
                {
                    Rigidbody2D targetRb = h.GetComponent<Rigidbody2D>();
                    bool isAirborne = targetRb != null && Mathf.Abs(targetRb.linearVelocity.y) > 0.5f;
                    CrowdControlState ccToApply = isAirborne ? airborneCC : groundedCC;

                    // Apply CC only (stun will handle horizontal pushback)
                    skills.ApplySkillCC(h, knockDir, groundedCC, airborneCC, ccDuration);
                    skills.GainSpirit(skills.spiritGainPerHit);
                }
                else
                {
                    ApplyKnockback(h, knockDir);
                }
            }
        }

        Despawn();
    }
    private void OnDisable()
    {
        CancelInvoke();
        hasExploded = false;

        // Reset velocity
        if (rb != null)
            rb.linearVelocity = Vector2.zero;
    }

    public override void Despawn()
    {
        // Fire skill end event
        Skills.InvokeSkillEnd();

        // Clean up before returning to pool
        CancelInvoke();
        hasExploded = false;

        if (rb != null)
            rb.linearVelocity = Vector2.zero;

        base.Despawn();
    }
}