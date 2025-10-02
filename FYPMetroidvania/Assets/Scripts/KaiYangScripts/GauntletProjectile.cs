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
    private Transform grabbedEnemy = null;
    private Vector2 grabbedEnemyOffset;

    private Hitbox hitbox;
    private bool hasInvokedStart = false;

    private void OnEnable()
    {
        hitbox = GetComponent<Hitbox>();
        hasInvokedStart = false;
    }

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

        // Fire skill start event
        if (hitbox != null && !hasInvokedStart)
        {
            hasInvokedStart = true;
            Skills.InvokeSkillStart(hitbox);
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
                grabbedEnemy.position = transform.position + (Vector3)grabbedEnemyOffset;
            }

            if (Vector2.Distance(transform.position, owner.position) < 0.35f)
            {
                // Release enemy at player position
                if (grabbedEnemy != null)
                {
                    grabbedEnemy.SetParent(null);

                    var enemyRb = grabbedEnemy.GetComponent<Rigidbody2D>();
                    if (enemyRb != null)
                    {
                        enemyRb.simulated = true;
                        enemyRb.linearVelocity = Vector2.zero;
                    }

                    grabbedEnemy = null;
                }

                isReturning = false;
                hitThisFlight.Clear();
                if (lineRenderer) lineRenderer.enabled = false;

                var skills = owner.GetComponent<Skills>();
                if (skills) skills.ClearGauntlet();

                Despawn();
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

                // Fire skill hit event
                if (hitbox != null)
                {
                    Skills.InvokeSkillHit(hitbox, h);
                }

                h.TakeDamage(damage);

                // Attach enemy to gauntlet
                grabbedEnemy = h.transform;
                grabbedEnemyOffset = (Vector2)(grabbedEnemy.position - transform.position);
                grabbedEnemy.SetParent(transform);

                Retract();
            }
        }
        else if ((terrainMask.value & layerBit) != 0)
        {
            Retract();
        }
    }

    public void Retract()
    {
        if (!isReturning)
        {
            isReturning = true;
            rb.gravityScale = 0f;

            if (grabbedEnemy != null)
            {
                var enemyRb = grabbedEnemy.GetComponent<Rigidbody2D>();
                if (enemyRb != null)
                {
                    enemyRb.simulated = false;
                }
            }
        }
    }

    public override void Despawn()
    {
        // Fire skill end event
        Skills.InvokeSkillEnd();

        base.Despawn();
    }
}