using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class SpiritSlash : MonoBehaviour
{
    public float speed = 15f;
    public float damage = 15f;
    public float bounceRange = 6f;
    public float overshootDistance = 1f;
    public float hitDelay = 0.3f;
    public float hitCooldown = 0.5f;

    private Transform player;
    private Transform currentTarget;
    private LayerMask enemyMask;
    private SpiritGauge spirit;

    private bool waiting = false;
    private bool isDelaying = false;
    private float lastHitTime = 0f;
    private Vector2 lastMovementDirection;

    // Track enemies we've damaged (by Health component id), not by collider Transform
    private HashSet<int> hitEnemyIds = new HashSet<int>();

    public void Init(Transform playerTransform, Transform target, LayerMask enemyMask)
    {
        player = playerTransform;
        currentTarget = target;
        this.enemyMask = enemyMask;

        if (player != null)
        {
            spirit = player.GetComponent<SpiritGauge>();
        }
    }

    private void Update()
    {
        if (spirit == null || spirit.IsEmpty)
        {
            Destroy(gameObject);
            return;
        }

        if (isDelaying) return;

        if (currentTarget == null)
        {
            if (!waiting) StartCoroutine(WaitForEnemy());
            return;
        }

        // Move toward current target
        Vector2 dir = (currentTarget.position - transform.position).normalized;
        lastMovementDirection = dir;
        transform.position += (Vector3)dir * speed * Time.deltaTime;

        // Single hit check (AFTER moving)
        if (Vector2.Distance(transform.position, currentTarget.position) < 0.5f &&
            Time.time - lastHitTime >= hitCooldown)
        {
            HitTarget(currentTarget);
            lastHitTime = Time.time;
        }
    }

    private void HitTarget(Transform target)
    {
        // Always resolve to root enemy via Health
        var h = target ? target.GetComponentInParent<Health>() : null;
        if (h != null)
        {
            int id = h.GetInstanceID();

            if (!hitEnemyIds.Contains(id))
            {
                h.TakeDamage(damage, (h.transform.position - player.position).normalized);
                hitEnemyIds.Add(id);
                Debug.Log($"[SpiritSlash] DEALT damage once to {h.name} (id {id})");
            }
            else
            {
                Debug.Log($"[SpiritSlash] Skipped duplicate on {h.name} (id {id})");
            }
        }
        else
        {
            Debug.Log($"[SpiritSlash] No Health found on/above {target?.name}");
        }

        // Overshoot a bit past the enemy
        Vector3 overshootPosition = (h ? h.transform.position : target.position) +
                                    (Vector3)(lastMovementDirection * overshootDistance);
        transform.position = overshootPosition;

        // Clear current target so we won’t re-hit this frame
        currentTarget = null;

        // Short pause before selecting the next enemy
        StartCoroutine(DelayBeforeNextTarget());
    }

    private IEnumerator DelayBeforeNextTarget()
    {
        isDelaying = true;
        yield return new WaitForSeconds(hitDelay);
        isDelaying = false;

        FindNextTarget();
    }

    private void FindNextTarget()
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(player.position, bounceRange, enemyMask);

        // Deduplicate by root enemy (Health.transform)
        var available = new List<Transform>();
        var unhit = new List<Transform>();
        var seenRoots = new HashSet<Transform>();

        foreach (var col in hits)
        {
            var h = col.GetComponentInParent<Health>();
            if (h == null) continue;

            Transform root = h.transform;
            if (!seenRoots.Add(root)) continue; // skip duplicate colliders of same enemy

            available.Add(root);
            if (!hitEnemyIds.Contains(h.GetInstanceID()))
                unhit.Add(root);
        }

        Transform best = null;

        // Prefer enemies we haven't damaged yet
        if (unhit.Count > 0)
        {
            float closest = float.MaxValue;
            foreach (var t in unhit)
            {
                float d = Vector2.Distance(transform.position, t.position);
                if (d < closest)
                {
                    closest = d;
                    best = t;
                }
            }
        }
        else if (available.Count > 0)
        {
            // All have been hit — reset the set and start a new cycle
            hitEnemyIds.Clear();

            float closest = float.MaxValue;
            foreach (var t in available)
            {
                float d = Vector2.Distance(transform.position, t.position);
                if (d < closest)
                {
                    closest = d;
                    best = t;
                }
            }
        }

        if (best != null)
        {
            currentTarget = best;
            waiting = false;
        }
        else
        {
            currentTarget = null;
            hitEnemyIds.Clear(); // clean slate if no enemies
        }
    }

    private IEnumerator WaitForEnemy()
    {
        waiting = true;
        while (currentTarget == null && spirit != null && !spirit.IsEmpty)
        {
            FindNextTarget();
            yield return new WaitForSeconds(0.2f);
        }
        waiting = false;
    }
}
