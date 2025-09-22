using System.Collections.Generic;
using UnityEngine;

public class GauntletProjectile : MonoBehaviour
{
    [Header("References")]
    public Rigidbody2D rb;
    public Collider2D col;
    public LineRenderer lineRenderer;

    [Header("Movement")]
    public float speed = 18f;

    [Header("Masks")]
    private LayerMask enemyMask;
    private LayerMask terrainMask;

    // States
    private bool isStuck = false;
    private bool isReturning = false;
    private bool isFallen = false;
    private Vector2 stuckPoint;
    private HashSet<Health> hitThisFlight = new HashSet<Health>();

    // Ranges
    private float minRange;
    private float maxFlightRange;
    private float maxLeashRange;
    private Vector2 launchOrigin;

    // Owner + damage
    private Transform owner;
    private float damage;

    // ======================== Init ========================
    public void Init(
        Transform owner,
        Vector2 dir,
        float damage,
        LayerMask enemyMask,
        LayerMask terrainMask,
        float minRange,
        float maxFlightRange,
        float maxLeashRange)
    {
        this.owner = owner;
        this.damage = damage;
        this.enemyMask = enemyMask;
        this.terrainMask = terrainMask;
        this.minRange = minRange;
        this.maxFlightRange = maxFlightRange;
        this.maxLeashRange = maxLeashRange;
        this.launchOrigin = owner.position;

        if (!rb) rb = GetComponent<Rigidbody2D>();
        if (!col) col = GetComponent<Collider2D>();
        if (!lineRenderer) lineRenderer = GetComponent<LineRenderer>();

        rb.gravityScale = 0f;
        rb.freezeRotation = true;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;

        rb.linearVelocity = dir.normalized * speed;

        isStuck = false;
        isFallen = false;
        isReturning = false;

        if (lineRenderer)
        {
            lineRenderer.enabled = true;
            lineRenderer.positionCount = 2;
        }
    }

    // ======================== Update ========================
    private void Update()
    {
        if (!owner) return;

        // Update tether line
        if (lineRenderer && lineRenderer.enabled)
        {
            lineRenderer.SetPosition(0, owner.position);
            lineRenderer.SetPosition(1, transform.position);
        }

        if (isReturning)
        {
            Vector2 dir = ((Vector2)owner.position - (Vector2)transform.position).normalized;
            rb.gravityScale = 0f;
            rb.linearVelocity = dir * speed;

            if (Vector2.Distance(transform.position, owner.position) < 0.35f)
                Destroy(gameObject);
        }
        else if (isStuck)
        {
            rb.linearVelocity = Vector2.zero;
            transform.position = stuckPoint;
        }
        else if (isFallen)
        {
            rb.linearVelocity = Vector2.zero;
            rb.gravityScale = 0f;
        }
        else
        {
            float dist = Vector2.Distance(transform.position, launchOrigin);

            // Enable gravity arc after max flight range
            if (maxFlightRange > 0f && dist > maxFlightRange)
                rb.gravityScale = 3f;
        }

        // Retract if beyond leash
        if (maxLeashRange > 0f && Vector2.Distance(transform.position, owner.position) > maxLeashRange)
            Retract();
    }

    // ======================== Collision ========================
    private void OnTriggerEnter2D(Collider2D col)
    {
        int layerBit = 1 << col.gameObject.layer;

        // Damage enemies
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

        // Stick to terrain if outbound & past minRange
        if (!isReturning && (terrainMask.value & layerBit) != 0)
        {
            float distFromOrigin = Vector2.Distance(transform.position, launchOrigin);
            if (distFromOrigin >= minRange)
            {
                isStuck = true;
                stuckPoint = transform.position;
                rb.gravityScale = 0f;
                rb.linearVelocity = Vector2.zero;
            }
        }

        // Fall if outbound and not stuck
        if (!isReturning && !isStuck && (terrainMask.value & layerBit) != 0)
        {
            isFallen = true;
            rb.linearVelocity = Vector2.zero;
            rb.gravityScale = 0f;
        }
    }

    // ======================== Retract ========================
    public void Retract()
    {
        if (isStuck || isFallen || !isReturning)
        {
            isStuck = false;
            isFallen = false;
            isReturning = true;
            rb.gravityScale = 0f;
            hitThisFlight.Clear();
        }
    }

    // ======================== State Queries ========================
    public bool IsStuck() => isStuck || isFallen;


}
