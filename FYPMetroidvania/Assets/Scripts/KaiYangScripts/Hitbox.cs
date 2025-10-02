using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public class Hitbox : MonoBehaviour
{
    private CombatSystem owner;
    private Skills skills;

    [Header("Damage")]
    public float damage;
    public bool isSkillHitbox = false;

    [Header("Hitstop Settings")]
    public float hitstopDuration = 0.08f;
    public bool applyHitstopToEnemy = true;
    public bool applyHitstopToPlayer = true;

    [Header("Knockback Overrides")]
    public bool forceUpKnockback = false;
    public Vector2 customKnockback = Vector2.zero;

    [Header("Special Sweep Knockback")]
    public bool isSweepHitbox = false;
    public float sweepKnockbackForce = 12f;

    private Collider2D col;
    private HashSet<Health> hitEnemies = new HashSet<Health>();

    // Events
    public static Action<Hitbox, Health> OnHit;

    private void Awake()
    {
        owner = GetComponentInParent<CombatSystem>();
        col = GetComponent<Collider2D>();
        skills = UnityEngine.Object.FindFirstObjectByType<Skills>();
    }

    private void OnEnable()
    {
        hitEnemies.Clear();
        if (!isSkillHitbox && owner != null)
        {
            damage = owner.GetAttackDamage(owner.CurrentComboStep);
        }
    }

    public void EnableCollider(float duration)
    {
        if (col == null) return;
        StartCoroutine(EnableTemporarily(duration));
    }

    private IEnumerator EnableTemporarily(float duration)
    {
        hitEnemies.Clear();
        col.enabled = true;
        yield return new WaitForSeconds(duration);
        col.enabled = false;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Hurtbox"))
        {
            Health h = other.GetComponentInParent<Health>();
            if (h != null && !hitEnemies.Contains(h))
            {
                hitEnemies.Add(h);

                Vector2 dir;
                bool useRawForce = false;
                CrowdControlState forceCC = CrowdControlState.None;

                if (isSweepHitbox)
                {
                    dir = Vector2.up * sweepKnockbackForce;
                    useRawForce = true;
                    h.TakeDamage(damage, dir, useRawForce, CrowdControlState.Knockdown, 2.0f);
                }
                else if (forceUpKnockback)
                {
                    dir = customKnockback;
                    h.TakeDamage(damage, dir, useRawForce, CrowdControlState.None, 0f);
                }
                else
                {
                    dir = (other.transform.position - owner.transform.position).normalized;

                    // For skill hitboxes, pass Stunned with duration 0 to prevent default knockback
                    // The skill's event handler will apply the actual CC
                    if (isSkillHitbox)
                    {
                        h.TakeDamage(damage, dir, false, CrowdControlState.Stunned, 0f);
                    }
                    else
                    {
                        h.TakeDamage(damage, dir, false, CrowdControlState.None, 0f);
                    }
                }

                OnHit?.Invoke(this, h);

                if (!h.isPlayer)
                {
                    h.ApplyBloodMark();
                }

                // hitstop
                if (skills != null)
                {
                    if (applyHitstopToEnemy)
                        StartCoroutine(skills.LocalHitstop(h.GetComponent<Rigidbody2D>(), hitstopDuration));

                    if (applyHitstopToPlayer)
                        StartCoroutine(skills.LocalHitstop(skills.GetComponent<Rigidbody2D>(), hitstopDuration));
                }
            }
        }
    }

    private void OnDrawGizmos()
    {
        if (col == null) col = GetComponent<Collider2D>();
        if (col == null) return;

        Gizmos.color = Color.yellow;

        if (col is BoxCollider2D box)
            Gizmos.DrawWireCube(box.bounds.center, box.bounds.size);
        else if (col is CircleCollider2D circle)
            Gizmos.DrawWireSphere(circle.bounds.center, circle.radius * transform.lossyScale.x);
    }
}