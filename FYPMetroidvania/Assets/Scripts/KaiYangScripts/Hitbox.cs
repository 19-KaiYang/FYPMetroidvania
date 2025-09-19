using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Hitbox : MonoBehaviour
{
    private CombatSystem owner;
    private Skills skills;


    [Header("Hitstop Settings")]
    public float hitstopDuration = 0.08f;
    public bool applyHitstopToEnemy = true;
    public bool applyHitstopToPlayer = true;


    private Collider2D col;
    private HashSet<Health> hitEnemies = new HashSet<Health>();

    private void Awake()
    {
        owner = GetComponentInParent<CombatSystem>();
        col = GetComponent<Collider2D>();
        skills = Object.FindFirstObjectByType<Skills>();




    }

    private void OnEnable()
    {
        hitEnemies.Clear(); 
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

                float totalDamage = owner.GetAttackDamage() * owner.GetDamageMultiplier(owner.CurrentComboStep);

                // Direction for knockback
                Vector2 dir = (other.transform.position - owner.transform.position).normalized;

                // Apply damage + knockback
                h.TakeDamage(totalDamage, dir);

                // hit stop activates
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

    //private IEnumerator ApplyHitstop(Rigidbody2D rb, Animator anim)
    //{
    //    if (rb != null) rb.simulated = false;
    //    if (anim != null) anim.speed = 0;

    //    yield return new WaitForSecondsRealtime(hitstopDuration);

    //    if (rb != null) rb.simulated = true;
    //    if (anim != null) anim.speed = 1;
    //}

    //Helper
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
