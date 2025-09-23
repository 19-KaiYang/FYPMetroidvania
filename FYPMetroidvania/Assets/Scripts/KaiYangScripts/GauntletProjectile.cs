using UnityEngine;
using System.Collections.Generic;

public class GauntletProjectile : ProjectileBase
{
    [Header("References")]
    public Collider2D col;
    public LineRenderer lineRenderer;

    [Header("Masks")]
    private LayerMask enemyMask;
    private LayerMask terrainMask;

    private bool isReturning = false;
    private HashSet<Health> hitThisFlight = new HashSet<Health>();

    private float maxFlightRange;
    private float maxLeashRange;
    private Vector2 launchOrigin;

    private Transform owner;
    private Transform grabbedEnemy = null; // track the grabbed enemy
    private Vector2 grabbedEnemyOffset; // offset from gauntlet to enemy

    public void Init(
        Transform owner,
        Vector2 dir,
        float dmg,
        LayerMask enemyMask,
        LayerMask terrainMask,
        float maxFlightRange,
        float maxLeashRange)
    {
        this.owner = owner;
        this.damage = dmg;
        this.enemyMask = enemyMask;
        this.terrainMask = terrainMask;
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

        isReturning = false;
        hitThisFlight.Clear();
        grabbedEnemy = null;
        grabbedEnemyOffset = Vector2.zero;

        if (lineRenderer)
        {
            lineRenderer.enabled = true;
            lineRenderer.positionCount = 2;
        }
    }

    protected override void Move()
    {
        if (!owner) return;

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

            // Update grabbed enemy position continuously during return
            if (grabbedEnemy != null)
            {
                // Keep enemy at the same offset from the gauntlet
                grabbedEnemy.position = transform.position + (Vector3)grabbedEnemyOffset;
            }

            if (Vector2.Distance(transform.position, owner.position) < 0.35f)
            {
                // Release enemy at player position
                if (grabbedEnemy != null)
                {
                    grabbedEnemy.SetParent(null);

                    // Re-enable enemy physics when releasing
                    var enemyRb = grabbedEnemy.GetComponent<Rigidbody2D>();
                    if (enemyRb != null)
                    {
                        enemyRb.simulated = true; // Re-enable physics
                        enemyRb.linearVelocity = Vector2.zero; // Stop the enemy
                    }

                    grabbedEnemy = null;
                }

                isReturning = false;
                hitThisFlight.Clear();
                if (lineRenderer) lineRenderer.enabled = false;

                // Notify skills the gauntlet is done
                var skills = owner.GetComponent<Skills>();
                if (skills) skills.ClearGauntlet();

                Despawn(); // return to pool
            }
        }
        else
        {
            // Auto retract if it travels too far
            float dist = Vector2.Distance(transform.position, launchOrigin);
            if (maxFlightRange > 0f && dist > maxFlightRange)
                Retract();

            if (maxLeashRange > 0f && Vector2.Distance(transform.position, owner.position) > maxLeashRange)
                Retract();
        }
    }

    private void OnTriggerEnter2D(Collider2D col)
    {
        int layerBit = 1 << col.gameObject.layer;

        if ((enemyMask.value & layerBit) != 0 && grabbedEnemy == null)
        {
            var h = col.GetComponentInParent<Health>();
            if (h != null && !hitThisFlight.Contains(h))
            {
                hitThisFlight.Add(h);
                h.TakeDamage(damage);

                // Attach enemy to gauntlet
                grabbedEnemy = h.transform;

                // Calculate and store the offset from gauntlet to enemy
                grabbedEnemyOffset = (Vector2)(grabbedEnemy.position - transform.position);

                // Parent the enemy to the gauntlet for easier management
                grabbedEnemy.SetParent(transform);

                // Retract after grabbing
                Retract();
            }
        }
        else if ((terrainMask.value & layerBit) != 0) 
        {
            // Immediately retract on terrain hit
            Retract();
        }
    }

    public void Retract()
    {
        if (!isReturning)
        {
            isReturning = true;
            rb.gravityScale = 0f;

            // Only disable enemy physics when we start retracting (if we have an enemy)
            if (grabbedEnemy != null)
            {
                var enemyRb = grabbedEnemy.GetComponent<Rigidbody2D>();
                if (enemyRb != null)
                {
                    enemyRb.simulated = false; // Disable physics during drag
                }
            }
        }
    }
}