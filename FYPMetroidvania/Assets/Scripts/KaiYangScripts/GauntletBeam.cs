using UnityEngine;
using System.Collections;

public class GauntletBeam : MonoBehaviour
{
    public Vector2 beamSize = new Vector2(20f, 5f);
    public float damage = 20f;          // damage per tick
    public float tickRate = 0.2f;       // how often damage is applied
    public float spiritDrainRate = 5f;  // Spirit consumed per second
    public LayerMask enemyMask;

    private bool facingRight;
    private SpiritGauge spirit;

    public void Init(bool facingRight, SpiritGauge spiritGauge)
    {
        this.facingRight = facingRight;
        this.spirit = spiritGauge;

        // Flip if needed
        if (!facingRight)
            transform.localScale = new Vector3(-transform.localScale.x, transform.localScale.y, 1);

        StartCoroutine(BeamRoutine());
    }

    private IEnumerator BeamRoutine()
    {
        float tickTimer = 0f;

        while (spirit != null && !spirit.IsEmpty)
        {
            // Drain Spirit over time
            //spirit.Deduct(spiritDrainRate * Time.deltaTime);

            // Tick damage every tickRate seconds
            tickTimer += Time.deltaTime;
            if (tickTimer >= tickRate)
            {
                tickTimer = 0f;
                DoDamage();
            }

            yield return null;
        }

        // Destroy beam when Spirit runs out
        Destroy(gameObject);
    }

    private void DoDamage()
    {
        Vector2 center = (Vector2)transform.position + new Vector2(
            facingRight ? beamSize.x / 2f : -beamSize.x / 2f, 0f
        );

        Collider2D[] hits = Physics2D.OverlapBoxAll(center, beamSize, 0f, enemyMask);
        foreach (var hit in hits)
        {
            Health h = hit.GetComponent<Health>();
            if (h != null)
            {
                Vector2 knockDir = (hit.transform.position - transform.position).normalized;
                h.TakeDamage(damage, knockDir);
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Vector2 center = (Vector2)transform.position + new Vector2(
            facingRight ? beamSize.x / 2f : -beamSize.x / 2f, 0f
        );
        Gizmos.DrawWireCube(center, beamSize);
    }
}
