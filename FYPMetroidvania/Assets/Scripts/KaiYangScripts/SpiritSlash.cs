using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class SpiritSlash : MonoBehaviour
{
    public float speed = 15f;
    public float bounceRange = 6f;
    public float overshootDistance = 1f;
    public float hitDelay = 0.3f;
    public float hitCooldown = 0.5f;
    public float spiritSlashBloodCost = 10f;

    [Header("Hitbox")]
    public GameObject hitboxObject; // Assign in prefab

    private Transform player;
    private Transform currentTarget;
    private LayerMask enemyMask;
    private SpiritGauge spirit;
    private Hitbox hitbox;

    private bool waiting = false;
    private bool isDelaying = false;
    private float lastHitTime = 0f;
    private Vector2 lastMovementDirection;

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

        // Get the hitbox component
        if (hitboxObject != null)
        {
            hitbox = hitboxObject.GetComponent<Hitbox>();
        }

        // Subscribe to hit events
        Hitbox.OnHit += OnSpiritSlashHit;

        Skills.InvokeUltimateStart(hitbox);
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

        // Move toward target
        Vector2 dir = (currentTarget.position - transform.position).normalized;
        lastMovementDirection = dir;
        transform.position += (Vector3)dir * speed * Time.deltaTime;

        if (Vector2.Distance(transform.position, currentTarget.position) < 0.5f &&
            Time.time - lastHitTime >= hitCooldown)
        {
            ReachTarget(currentTarget);
            lastHitTime = Time.time;
        }
    }

    private void OnSpiritSlashHit(Hitbox hb, Health h)
    {
        // Only respond to our own hitbox
        if (hb != hitbox) return;
        if (h == null || h.isPlayer) return;

        int id = h.GetInstanceID();

        // Track hit enemies
        if (!hitEnemyIds.Contains(id))
        {
            hitEnemyIds.Add(id);

            // Invoke ultimate hit event
            Skills.InvokeUltimateHit(hitbox, h);

            // Apply blood mark
            h.ApplyBloodMark();

            // Apply health cost to player
            Health playerHealth = player.GetComponent<Health>();
            if (playerHealth != null && spiritSlashBloodCost > 0f)
            {
                float safeCost = Mathf.Min(spiritSlashBloodCost, playerHealth.CurrentHealth - 1f);
                if (safeCost > 0f)
                    playerHealth.TakeDamage(safeCost);
            }
        }
    }

    private void ReachTarget(Transform target)
    {
        // Enable hitbox briefly AT THE TARGET POSITION (not during travel)
        if (hitboxObject != null)
        {
            StartCoroutine(EnableHitboxAtTarget(target.position));
        }

        currentTarget = null;
        StartCoroutine(DelayBeforeNextTarget());
    }

    private IEnumerator EnableHitboxAtTarget(Vector3 targetPos)
    {
        if (hitbox == null) yield break;

        Collider2D col = hitboxObject.GetComponent<Collider2D>();
        if (col == null) yield break;

        transform.position = targetPos;

        // clear hit list and enable collider
        hitbox.ClearHitEnemies();
        col.enabled = true;

        yield return new WaitForFixedUpdate();

        // Disable immediately
        col.enabled = false;

        Vector3 overshootPosition = targetPos + (Vector3)(lastMovementDirection * overshootDistance);
        transform.position = overshootPosition;
    }


    private IEnumerator EnableHitboxTemporarily(float duration)
    {
        if (hitboxObject == null) yield break;

        hitboxObject.SetActive(true);

        if (hitbox != null)
            hitbox.ClearHitEnemies();

        yield return new WaitForSeconds(duration);

        hitboxObject.SetActive(false);
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
        var available = new List<Transform>();
        var unhit = new List<Transform>();
        var seenRoots = new HashSet<Transform>();

        foreach (var col in hits)
        {
            var h = col.GetComponentInParent<Health>();
            if (h == null) continue;

            Transform root = h.transform;
            if (!seenRoots.Add(root)) continue;

            available.Add(root);
            if (!hitEnemyIds.Contains(h.GetInstanceID()))
                unhit.Add(root);
        }

        Transform best = null;

        // Prioritize unhit enemies
        if (unhit.Count > 0)
        {
            float closest = float.MaxValue;
            foreach (var t in unhit)
            {
                float d = Vector2.Distance(transform.position, t.position);
                if (d < closest) { closest = d; best = t; }
            }
        }
        // Only clear and cycle if ALL enemies in range have been hit AND there's more than 1
        else if (available.Count > 1)
        {
            hitEnemyIds.Clear();

            float closest = float.MaxValue;
            foreach (var t in available)
            {
                float d = Vector2.Distance(transform.position, t.position);
                if (d < closest) { closest = d; best = t; }
            }
        }
        // Only 1 enemy left - wait for new enemies instead of re-hitting
        else if (available.Count == 1)
        {
            // Don't retarget the single enemy - wait for more
            best = null;
        }

        if (best != null) { currentTarget = best; waiting = false; }
        else { currentTarget = null; }
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

    private void OnDestroy()
    {
        // Unsubscribe from events
        Hitbox.OnHit -= OnSpiritSlashHit;
        Skills.InvokeUltimateEnd();
    }
}