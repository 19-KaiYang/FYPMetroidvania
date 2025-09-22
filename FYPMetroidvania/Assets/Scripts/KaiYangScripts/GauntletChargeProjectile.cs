using UnityEngine;

public class GauntletChargeProjectile : ProjectileBase
{
    private float knockback;
    private float chargeRatio;

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

    public LayerMask enemyMask;

    public void Init(Vector2 dir, float dmg, float kb, float ratio)
    {
        damage = dmg;
        knockback = kb;
        chargeRatio = ratio;
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

        Invoke(nameof(Explode), lifeTime);
    }

    protected override void Move()
    {
        
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        Health target = other.GetComponent<Health>();
        if (target != null && !target.isPlayer)
        {
            Explode();
        }
    }

    private void Explode()
    {
        float radius = (chargeRatio < 0.33f) ? minExplosionRadius :
                       (chargeRatio < 0.66f) ? midExplosionRadius : maxExplosionRadius;

        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, radius, enemyMask);
        foreach (var hit in hits)
        {
            Health h = hit.GetComponent<Health>();
            if (h != null && !h.isPlayer)
            {
                h.TakeDamage(damage);

                Rigidbody2D trb = h.GetComponent<Rigidbody2D>();
                if (trb != null)
                {
                    Vector2 dir = (trb.transform.position - transform.position).normalized;
                    trb.AddForce(dir * knockback, ForceMode2D.Impulse);
                }
            }
        }

        gameObject.SetActive(false);
    }
}
