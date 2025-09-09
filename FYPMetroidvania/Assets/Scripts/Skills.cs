using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Skills : MonoBehaviour
{
    [Header("Enemy Detection")]
    public LayerMask enemyMask;
    public float hitstop = 0.06f;

    private Rigidbody2D rb;
    private CombatSystem combat;
    private PlayerController controller;

    private bool usingSkill = false;
    public bool IsUsingSkill => usingSkill;

    [Header("Sword Dash Settings")]
    public float dashSpeed = 22f;
    public float dashDuration = 0.18f;
    public float damageMultiplier = 1.2f;
    public Vector2 boxSize = new Vector2(1.4f, 1.0f);
    public Vector2 boxOffset = new Vector2(0.7f, 0f);

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        combat = GetComponent<CombatSystem>();
        controller = GetComponent<PlayerController>();
    }

    public void TryUseSwordDash()
    {
        if (!usingSkill)
            StartCoroutine(Skill_SwordDash());
    }

    private IEnumerator Skill_SwordDash()
    {
        usingSkill = true;

        // stop PlayerController from overriding our velocity
        if (controller) controller.externalVelocityOverride = true;

        // ignore collisions between Player and Enemy layers
        int playerLayer = gameObject.layer;
        int enemyLayer = SingleLayerIndex(enemyMask);
        bool collisionToggled = false;
        if (enemyLayer >= 0)
        {
            Physics2D.IgnoreLayerCollision(playerLayer, enemyLayer, true);
            collisionToggled = true;
        }

        HashSet<Health> hit = new HashSet<Health>();
        Vector2 dir = (controller != null && controller.facingRight) ? Vector2.right : Vector2.left;
        Vector2 originalVel = rb.linearVelocity;

        float t = 0f;
        while (t < dashDuration)
        {
            t += Time.deltaTime;
            rb.linearVelocity = dir * dashSpeed;

            Vector2 center = (Vector2)transform.position +
                             new Vector2(boxOffset.x * ((controller != null && controller.facingRight) ? 1f : -1f),
                                         boxOffset.y);

            var cols = Physics2D.OverlapBoxAll(center, boxSize, 0f, enemyMask);
            foreach (var c in cols)
            {
                var h = c.GetComponentInParent<Health>();
                if (h != null && !hit.Contains(h))
                {
                    hit.Add(h);

                    float dmg = combat.GetAttackDamage() * damageMultiplier;
                    Vector2 knockDir = (h.transform.position - transform.position).normalized;
                    h.TakeDamage(dmg, knockDir);

                    if (hitstop > 0f)
                    {
                        StartCoroutine(LocalHitstop(h.GetComponent<Rigidbody2D>(), hitstop));
                        StartCoroutine(LocalHitstop(rb, hitstop));
                    }
                }
            }
            yield return null;
        }

        rb.linearVelocity = originalVel;

        // restore collisions
        if (collisionToggled)
            Physics2D.IgnoreLayerCollision(playerLayer, enemyLayer, false);

        // allow PlayerController to control velocity \
        if (controller) controller.externalVelocityOverride = false;

        usingSkill = false;
    }

    private IEnumerator LocalHitstop(Rigidbody2D targetRb, float duration)
    {
        if (targetRb) targetRb.simulated = false;
        yield return new WaitForSecondsRealtime(duration);
        if (targetRb) targetRb.simulated = true;
    }

    private int SingleLayerIndex(LayerMask mask)
    {
        int v = mask.value;
        if (v == 0) return -1;
        if ((v & (v - 1)) != 0) return -1;
        int index = 0;
        while ((v >>= 1) != 0) index++;
        return index;
    }

    private void OnDrawGizmosSelected()
    {
        if (controller == null) controller = GetComponent<PlayerController>();
        bool facingRight = controller != null ? controller.facingRight : true;

        Vector2 center = (Vector2)transform.position +
                         new Vector2(boxOffset.x * (facingRight ? 1f : -1f), boxOffset.y);

        Gizmos.color = new Color(1f, 0.5f, 0f, 0.25f);
        Gizmos.DrawCube(center, boxSize);
    }
}
