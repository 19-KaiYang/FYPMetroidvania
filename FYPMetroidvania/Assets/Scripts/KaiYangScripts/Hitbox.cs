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
    public bool isCritical = false;

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

    [Header("Knockback Settings")]
    public float X_Knockback; public float Y_Knockback;
    public bool facingRight;

    [Header("Crowd Control Settings")]
    public CrowdControlState CCType = CrowdControlState.Stunned;
    public float CCDuration = 0.5f;

    [Header("Special Settings")]
    public bool applyBloodMark = false;
    public bool isSkillHitbox = false;
    public bool isUltimateHitbox = false;

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
        isCritical = false;
        if (!isSkillHitbox && owner != null)
        {
            damage = owner.GetAttackDamage(owner.CurrentComboStep);
            facingRight = PlayerController.instance.facingRight;
        }
    }

    public void EnableCollider(float duration)
    {
        if (col == null) return;
        StartCoroutine(EnableTemporarily(duration));
    }

    public void ClearHitEnemies()
    {
         hitEnemies.Clear();
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

                OnHit?.Invoke(this, h);
                float directionalXknockback = facingRight ? X_Knockback : -X_Knockback;
                h.TakeDamage(damage, new Vector2(directionalXknockback, Y_Knockback), false, CCType, CCDuration);

                if (!h.isPlayer && applyBloodMark)
                {
                    h.ApplyBloodMark();
                }

                // hitstop 
                if (skills != null)
                {
                    if (applyHitstopToEnemy)
                        skills.StartCoroutine(skills.LocalHitstop(h.GetComponent<Rigidbody2D>(), hitstopDuration));

                    if (applyHitstopToPlayer)
                        skills.StartCoroutine(skills.LocalHitstop(skills.GetComponent<Rigidbody2D>(), hitstopDuration));
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