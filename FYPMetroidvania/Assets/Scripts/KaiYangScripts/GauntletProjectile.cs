using System.Collections.Generic;
using UnityEngine;

public class GauntletProjectile : MonoBehaviour
{
    [Header("Movement")]
    public float speed = 18f;

    private Rigidbody2D rb;
    private Transform owner;
    private float damage;

    [Header("Masks")]
    private LayerMask enemyMask;
    private LayerMask terrainMask;

    // States
    private bool isStuck = false;
    private bool isReturning = false;
    private Vector2 stuckPoint;
    private HashSet<Health> hitThisFlight = new HashSet<Health>();

    // Ranges
    private float minRange;
    private float maxRange;
    private Vector2 launchOrigin;

    // ======================== Init ========================
    public void Init(
        Transform owner,
        Vector2 dir,
        float damage,
        LayerMask enemyMask,
        LayerMask terrainMask,
        float minRange,
        float maxRange)
    {
        this.owner = owner;
        this.damage = damage;
        this.enemyMask = enemyMask;
        this.terrainMask = terrainMask;
        this.minRange = minRange;
        this.maxRange = maxRange;
        this.launchOrigin = owner.position;

        rb = GetComponent<Rigidbody2D>();
        if (!rb) rb = gameObject.AddComponent<Rigidbody2D>();

        rb.gravityScale = 0f;
        rb.freezeRotation = true;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;

        //  Always start moving forward
        rb.linearVelocity = dir.normalized * speed;
    }

    // ======================== Update ========================
    private void Update()
    {
        if (isReturning && owner != null)
        {
            Vector2 dir = ((Vector2)owner.position - (Vector2)transform.position).normalized;
            rb.linearVelocity = dir * speed;

            // Finish retract when close enough
            if (Vector2.Distance(transform.position, owner.position) < 0.35f)
            {
                Destroy(gameObject);
            }
        }
        else if (isStuck)
        {
            rb.linearVelocity = Vector2.zero;
            transform.position = stuckPoint;
        }
        else
        {
            float dist = Vector2.Distance(transform.position, launchOrigin);

            //  Auto retract if it travels too far
            if (maxRange > 0f && dist > maxRange)
            {
                Retract();
            }

            //  If terrain is too close (within minRange), stick immediately
            if (minRange > 0f && dist < minRange)
            {
                // just keep flying until it passes minRange
            }
        }
    }

    // ======================== Collision ========================
    private void OnTriggerEnter2D(Collider2D col)
    {
        int layerBit = 1 << col.gameObject.layer;

        // Damage enemies both outbound and inbound
        if ((enemyMask.value & layerBit) != 0)
        {
            var h = col.GetComponentInParent<Health>();
            if (h != null && !hitThisFlight.Contains(h))
            {
                hitThisFlight.Add(h);
                Vector2 knock = ((Vector2)col.transform.position - (Vector2)transform.position).normalized;
                h.TakeDamage(damage, knock);
            }
            return;
        }

        // Stick only to terrain on outbound (after passing minRange)
        if (!isReturning && (terrainMask.value & layerBit) != 0)
        {
            float distFromOrigin = Vector2.Distance(transform.position, launchOrigin);
            if (distFromOrigin >= minRange)
            {
                isStuck = true;
                stuckPoint = transform.position;
                rb.linearVelocity = Vector2.zero;
            }
        }
    }

    // ======================== Retract ========================
    public void Retract()
    {
        if (isStuck || !isReturning)   // stuck OR still outbound
        {
            isStuck = false;
            isReturning = true;
            hitThisFlight.Clear();    // allow new hits on the way back
        }
    }

    // ======================== State Queries ========================
    public bool IsStuck() => isStuck;
}
