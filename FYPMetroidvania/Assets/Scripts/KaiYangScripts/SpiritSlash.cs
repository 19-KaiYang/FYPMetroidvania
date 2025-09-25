using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class SpiritSlash : MonoBehaviour
{
    public float speed = 15f;
    public float damage = 15f;
    public float bounceRange = 6f;
    public float overshootDistance = 1f; // How far to go past the enemy after hitting

    private Transform player;
    private Transform currentTarget;
    private LayerMask enemyMask;
    private SpiritGauge spirit;

    private bool waiting = false;
    private float lastHitTime = 0f;
    private float hitCooldown = 0.5f; // Minimum time between hits

    // Track which enemies we've hit to cycle through them
    private List<Transform> hitEnemies = new List<Transform>();

    public void Init(Transform playerTransform, Transform target, LayerMask enemyMask)
    {
        player = playerTransform;
        currentTarget = target;
        this.enemyMask = enemyMask;

        // Get spirit reference from player
        if (player != null)
        {
            spirit = player.GetComponent<SpiritGauge>();
        }
    }

    private void Update()
    {
        // Only destroy if spirit is null/empty (controlled by ultimate)
        if (spirit == null || spirit.IsEmpty)
        {
            Destroy(gameObject);
            return;
        }

        if (currentTarget == null)
        {
            if (!waiting) StartCoroutine(WaitForEnemy());
            return;
        }

        // Move toward target
        Vector2 dir = (currentTarget.position - transform.position).normalized;
        transform.position += (Vector3)dir * speed * Time.deltaTime;

        // Check if close enough to hit AND enough time has passed since last hit
        if (Vector2.Distance(transform.position, currentTarget.position) < 0.5f &&
            Time.time - lastHitTime >= hitCooldown)
        {
            HitTarget(currentTarget);
            lastHitTime = Time.time;
        }
    }

    private void HitTarget(Transform target)
    {
        var h = target.GetComponent<Health>();
        if (h != null)
        {
            h.TakeDamage(damage, (target.position - player.position).normalized);
        }

        // Add to hit list
        if (!hitEnemies.Contains(target))
        {
            hitEnemies.Add(target);
        }

        // Calculate overshoot position - go a bit beyond the target
        Vector2 directionToTarget = (target.position - transform.position).normalized;
        Vector3 overshootPosition = target.position + (Vector3)(directionToTarget * overshootDistance);

        // Move to overshoot position
        transform.position = overshootPosition;

        // Try to find next target for bouncing
        FindNextTarget();
    }

    private void FindNextTarget()
    {
        // Find all enemies around the player position
        Collider2D[] enemies = Physics2D.OverlapCircleAll(player.position, bounceRange, enemyMask);

        List<Transform> availableEnemies = new List<Transform>();
        List<Transform> unhitEnemies = new List<Transform>();

        // Separate enemies into hit and unhit categories
        foreach (var enemy in enemies)
        {
            availableEnemies.Add(enemy.transform);
            if (!hitEnemies.Contains(enemy.transform))
            {
                unhitEnemies.Add(enemy.transform);
            }
        }

        Transform bestTarget = null;

        // Prioritize enemies we haven't hit yet
        if (unhitEnemies.Count > 0)
        {
            // Find closest unhit enemy
            float closestDistance = float.MaxValue;
            foreach (var enemy in unhitEnemies)
            {
                float distance = Vector2.Distance(transform.position, enemy.position);
                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    bestTarget = enemy;
                }
            }
        }
        else if (availableEnemies.Count > 0)
        {
            // All enemies have been hit, reset the hit list and start over
            hitEnemies.Clear();

            // Find closest enemy
            float closestDistance = float.MaxValue;
            foreach (var enemy in availableEnemies)
            {
                float distance = Vector2.Distance(transform.position, enemy.position);
                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    bestTarget = enemy;
                }
            }
        }

        if (bestTarget != null)
        {
            currentTarget = bestTarget;
            waiting = false;
        }
        else
        {
            // No targets found at all
            currentTarget = null;
            hitEnemies.Clear(); // Reset hit list when no enemies available
        }
    }

    private IEnumerator WaitForEnemy()
    {
        waiting = true;

        while (currentTarget == null && spirit != null && !spirit.IsEmpty)
        {
            // Keep searching for enemies around player position
            FindNextTarget();
            yield return new WaitForSeconds(0.2f);
        }

        waiting = false;
    }
}