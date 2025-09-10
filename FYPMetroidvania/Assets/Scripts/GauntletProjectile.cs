using System.Collections.Generic;
using UnityEngine;

public class GauntletProjectile : MonoBehaviour
{
    [Header("Movement")]
    public float speed = 18f;

    [Header("Masks")]
    [SerializeField] private LayerMask enemyMask;
    [SerializeField] private LayerMask terrainMask;

    private Rigidbody2D rb;
    private Transform owner;
    private float damage;
    private bool isStuck = false;
    private bool isReturning = false;
    private Vector2 stuckPoint;
    private HashSet<Health> hitThisFlight = new HashSet<Health>();

    public void Init(Transform owner, Vector2 dir, float damage, LayerMask enemyMask, LayerMask terrainMask)
    {
        this.owner = owner;
        this.damage = damage;
        this.enemyMask = enemyMask;
        this.terrainMask = terrainMask;

        rb = GetComponent<Rigidbody2D>();
        if (!rb) rb = gameObject.AddComponent<Rigidbody2D>();
        rb.gravityScale = 0f;
        rb.freezeRotation = true;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        rb.linearVelocity = dir.normalized * speed;
    }

    private void Update()
    {
        if (isReturning && owner != null)
        {
            Vector2 dir = ((Vector2)owner.position - (Vector2)transform.position).normalized;
            rb.linearVelocity = dir * speed;

            if (Vector2.Distance(transform.position, owner.position) < 0.35f)
            {
                Destroy(gameObject);
            }
        }
        else if (isStuck)
        {
            // Pin to the stuck point
            rb.linearVelocity = Vector2.zero;
            transform.position = stuckPoint;
        }
    }

    private void OnTriggerEnter2D(Collider2D col)
    {
        int layerBit = 1 << col.gameObject.layer;

        // Damage enemies on both outbound and return paths
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

        // Stick only to terrain on the outbound flight
        if (!isReturning && (terrainMask.value & layerBit) != 0)
        {
            isStuck = true;
            stuckPoint = transform.position;
            rb.linearVelocity = Vector2.zero;
        }
    }

    public void Retract()
    {
        if (isStuck || !isReturning)   // if stuck OR still outbound
        {
            isStuck = false;
            isReturning = true;
            hitThisFlight.Clear();    // allow new hits on the way back
        }
    }

    public bool IsStuck() => isStuck;
}
