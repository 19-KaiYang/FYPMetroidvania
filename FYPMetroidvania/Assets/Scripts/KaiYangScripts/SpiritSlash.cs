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
    public GameObject hitboxObject;
    [Header("Knockback Settings")]
    public float knockbackMultiplier = 0f;

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

    // NEW: keep track of the Health of the target we're enabling the hitbox for
    private Health pendingTargetHealth = null;

    public void Init(Transform playerTransform, Transform target, LayerMask enemyMask)
    {
        player = playerTransform;
        currentTarget = target;
        this.enemyMask = enemyMask;

        if (player != null)
        {
            spirit = player.GetComponent<SpiritGauge>();
        }

        if (hitboxObject != null)
        {
            hitbox = hitboxObject.GetComponent<Hitbox>();
        }

        // event subscribe
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
        if (hb != hitbox) return;
        if (h == null || h.isPlayer) return;

        if (currentTarget != null)
        {
            Health currentTargetHealth = currentTarget.GetComponent<Health>();
            if (currentTargetHealth == null || h != currentTargetHealth)
                return; // ignore hits on other enemies while flying to the target
        }
        else if (pendingTargetHealth != null)
        {
            if (h != pendingTargetHealth)
                return; // ignore hits on other enemies while hitbox is briefly enabled at the target
        }
        else
        {
            // No valid target context -> ignore
            return;
        }

        int id = h.GetInstanceID();

        // Track hit enemies
        if (!hitEnemyIds.Contains(id))
        {
            hitEnemyIds.Add(id);

            Skills.InvokeUltimateHit(hitbox, h);

            Vector2 knockDir = (h.transform.position - transform.position).normalized;

           h.TakeDamage(spiritSlashBloodCost,knockDir,false,CrowdControlState.None, 0f, true,false,knockbackMultiplier);


            // Apply blood mark
            h.ApplyBloodMark();

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
        if (hitboxObject != null)
        {
            // store the target's Health so the hit handler knows which enemy is valid
            pendingTargetHealth = target.GetComponent<Health>();
            StartCoroutine(EnableHitboxAtTarget(target.position));
        }

        // Clear currentTarget right away (movement finished)
        currentTarget = null;
        StartCoroutine(DelayBeforeNextTarget());
    }

    private IEnumerator EnableHitboxAtTarget(Vector3 targetPos)
    {
        if (hitbox == null) yield break;

        Collider2D col = hitboxObject.GetComponent<Collider2D>();
        if (col == null) yield break;

        Vector3 offsetPos = targetPos - (Vector3)(lastMovementDirection * 0.3f);
        transform.position = offsetPos;

        hitbox.ClearHitEnemies();


        yield return null;

        col.enabled = true;
        yield return new WaitForFixedUpdate();
        col.enabled = false;


        Vector3 overshootPosition = targetPos + (Vector3)(lastMovementDirection * overshootDistance);
        transform.position = overshootPosition;

        pendingTargetHealth = null;
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

        // prior unhit enemies
        if (unhit.Count > 0)
        {
            float closest = float.MaxValue;
            foreach (var t in unhit)
            {
                float d = Vector2.Distance(transform.position, t.position);
                if (d < closest) { closest = d; best = t; }
            }
        }
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
        else if (available.Count == 1)
        {
           
            hitEnemyIds.Clear();  
            best = available[0];
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
