using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using Unity.Cinemachine;

public class Hitbox : MonoBehaviour
{
    private CombatSystem owner;
    private Skills skills;

    [Header("Damage")]
    public float baseDamage;
    public float damage;
    public bool isCritical = false;

    [Header("Hitstop Settings")]
    public float hitstopDuration = 0.05f;
    public bool applyHitstop = false;

    [Header("Knockback Settings")]
    public float X_Knockback; public float Y_Knockback;
    public bool facingRight;

    [Header("Crowd Control Settings")]
    public CrowdControlState CCType = CrowdControlState.Stunned;
    public float CCDuration = 0.5f;

    [Header("FX")]
    public SFXTYPE sfx = SFXTYPE.NONE;

    [Header("Special Settings")]
    public bool applyBloodMark = false;
    public bool isSkillHitbox = false;
    public bool isUltimateHitbox = false;

    [Header("Screenshake")]
    public bool screenshake = false;
    public CinemachineImpulseSource impulseSource;
    public float screenshakeForce = 0.5f;

    private Collider2D col;
    private HashSet<Health> hitEnemies = new HashSet<Health>();

    // Events
    public static Action<Hitbox, Health> OnHit;
    public static Action<Hitbox, Health> OnUltHit;

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
        if (owner != null)
        {
            damage = baseDamage;
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
            if (h != null && !hitEnemies.Contains(h) && !h.invincible)
            {
                if(sfx != SFXTYPE.NONE) AudioManager.PlaySFX(sfx, 0.3f);
                hitEnemies.Add(h);

                if (isUltimateHitbox) OnUltHit?.Invoke(this, h);
                else OnHit?.Invoke(this, h);
                float directionalXknockback = PlayerController.instance.facingRight ? X_Knockback : -X_Knockback;
                h.TakeDamage(damage, new Vector2(directionalXknockback, Y_Knockback), false, CCType, CCDuration, isCritical: isCritical);
                if (screenshake && impulseSource != null && SettingData.instance.screenshake)
                {
                    impulseSource.GenerateImpulse(screenshakeForce);
                }
                if (applyHitstop)
                {
                    StartCoroutine(LocalHitstop(owner.GetComponent<Rigidbody2D>(), hitstopDuration));
                }
                
                //// hitstop 
                //if (skills != null)
                //{
                //    if (applyHitstopToEnemy)
                //        skills.StartCoroutine(skills.LocalHitstop(h.GetComponent<Rigidbody2D>(), hitstopDuration));

                //    if (applyHitstopToPlayer)
                //        skills.StartCoroutine(skills.LocalHitstop(skills.GetComponent<Rigidbody2D>(), hitstopDuration));
                //}
            }
        }
    }
    public IEnumerator LocalHitstop(Rigidbody2D targetRb, float duration)
    {
        PlayerController pc = targetRb ? targetRb.GetComponent<PlayerController>() : null;

        Vector2 savedVel = Vector2.zero;
        if (pc != null) savedVel = pc.GetVelocity();

        // freeze
        if (pc != null) pc.SetHitstop(true);
        //if (targetRb) targetRb.simulated = false;

        yield return new WaitForSecondsRealtime(duration);

        // unfreeze
        if (pc != null)
        {
            pc.SetHitstop(false);
            pc.SetVelocity(savedVel);
            pc.externalVelocityOverride = false;
        }

        if (targetRb) targetRb.simulated = true;
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