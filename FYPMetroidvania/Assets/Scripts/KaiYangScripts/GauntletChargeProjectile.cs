using UnityEngine;

public class GauntletChargeProjectile : MonoBehaviour
{
    private Rigidbody2D rb;
    private float damage;
    private float knockback;

    [Header("Visuals")]
    public SpriteRenderer spriteRenderer;
    public Sprite weakSprite;
    public Sprite midSprite;
    public Sprite strongSprite;

    [Header("Projectile Settings")]
    public float baseSpeed = 10f;
    public float lifeTime = 3f;

    public void Init(Vector2 dir, float dmg, float kb, float chargeRatio)
    {
        rb = GetComponent<Rigidbody2D>();
        damage = dmg;
        knockback = kb;

        // Projectile speed scales slightly with charge
        rb.linearVelocity = dir * baseSpeed * Mathf.Lerp(1f, 1.5f, chargeRatio);

        // Choose sprite
        if (spriteRenderer != null)
        {
            if (chargeRatio < 0.33f && weakSprite != null)
                spriteRenderer.sprite = weakSprite;
            else if (chargeRatio < 0.66f && midSprite != null)
                spriteRenderer.sprite = midSprite;
            else if (strongSprite != null)
                spriteRenderer.sprite = strongSprite;
        }

        Destroy(gameObject, lifeTime);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        Health target = other.GetComponent<Health>();
        if (target != null && !target.isPlayer)
        {
            target.TakeDamage(damage);

            Rigidbody2D trb = target.GetComponent<Rigidbody2D>();
            if (trb != null)
            {
                Vector2 dir = (trb.transform.position - transform.position).normalized;
                trb.AddForce(dir * knockback, ForceMode2D.Impulse);
            }

            Destroy(gameObject);
        }
    }
}
