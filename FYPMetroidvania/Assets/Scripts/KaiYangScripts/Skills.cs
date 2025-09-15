using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Skills : MonoBehaviour
{
    [Header("Enemy Detection")]

    public LayerMask enemyMask;

    public float hitstop = 0.06f;

    private Rigidbody2D rb;
    private CombatSystem combat;
    private PlayerController controller;
    private EnergySystem energy;

    [Header("Gauntlet Skill Requirements")]
    public GameObject gauntletPrefab;
    private GauntletProjectile activeGauntlet;

    [Header("Gauntlet Launch")]
    public LayerMask terrainMask;              
    public float gauntletLaunchDamage = 12f;   
    public float gauntletLaunchSpeed = 18f;
    public float gauntletMinRange = 1.5f;
    public float gauntletMaxFlightRange = 8f;   
    public float gauntletMaxLeashRange = 15f;   
    public float gauntletSkillEnergyCost;

    public bool GauntletDeployed => activeGauntlet != null;                
    public bool HasStuckGauntlet() => activeGauntlet != null && activeGauntlet.IsStuck();

    // global skill gate
    private bool usingSkill = false;
    public bool IsUsingSkill => usingSkill;

    // ===================== SWORD DASH =====================
    [Header("Sword Dash")]
    public float dashSpeed = 22f;
    public float dashDuration = 0.18f;
    //public float dashDamageMult = 1.2f;
    public float dashFlatDamage = 0f;
    public Vector2 dashBoxSize = new Vector2(1.4f, 1.0f);
    public Vector2 dashBoxOffset = new Vector2(0.7f, 0f);

    [Header("Sword Dash Cooldown")]
    public float swordDashCooldown = 2f;
    private float swordDashCooldownTimer = 0f;

    // ===================== SWORD UPPERCUT =====================
    [Header("Sword Uppercut")]

    public float uppercutUpSpeed = 12f;        
    public float uppercutForwardSpeed = 4f;    
    public float uppercutDuration = 0.35f;     
    public float uppercutFlatDamage = 10f;    
    public Vector2 uppercutBoxSize = new Vector2(1.2f, 2.0f);
    public Vector2 uppercutBoxOffset = new Vector2(0.6f, 1f); 

    [Header("Sword Uppercut Cooldown / Cost")]
    public float swordUppercutCooldown = 3f;
    private float swordUppercutCooldownTimer = 0f;
    public float swordUppercutCost = 20f;



    // ===================== GAUNTLET SHOCKWAVE =====================
    [Header("Gauntlet Shockwave (Ground)")]
    public float shockwaveRadius = 2.5f;          
    //public float shockwaveDamageMult = 1.4f;
    public float shockwaveFlatDamage = 0f;
    public float shockwaveKnockForce = 12f;       
    public float shockwaveUpwardBoost = 6f;       

    [Header("Gauntlet Shockwave (Air to Plunge)")]
    public float plungeSpeed = 28f;              
    public float maxPlungeTime = 0.8f;            
    public float preShockStopTime = 0.03f;        

    [Header("Gauntlet Shockwave Cooldown")]
    public float gauntletShockCooldown = 3f;
    private float gauntletShockCooldownTimer = 0f;

    [Header("Energy Usage")]
    public float swordDashCost = 20f;
    public float gauntletShockwaveCost = 30f;


   


    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        combat = GetComponent<CombatSystem>();
        controller = GetComponent<PlayerController>();
        energy = GetComponent<EnergySystem>();
    }

    private void Update()
    {
        if (swordDashCooldownTimer > 0f) swordDashCooldownTimer -= Time.deltaTime;
        if (gauntletShockCooldownTimer > 0f) gauntletShockCooldownTimer -= Time.deltaTime;
        if (swordUppercutCooldownTimer > 0f) swordUppercutCooldownTimer -= Time.deltaTime;


    }
    #region CombatSystem
    // ===================== PUBLIC API (called by CombatSystem) =====================
    public void TryUseSwordDash()
    {
        if (usingSkill) return;
        if (swordDashCooldownTimer > 0f) return;
        if (energy != null && !energy.TrySpend(swordDashCost)) return;
        StartCoroutine(Skill_SwordDash());
    }

    public void TryUseGauntletShockwave()
    {
        if (usingSkill) return;
        if (GauntletDeployed)
        {
            RetractGauntlet();
            return;
        }
        if (gauntletShockCooldownTimer > 0f) return;
        if (energy != null && !energy.TrySpend(gauntletShockwaveCost)) return;
        StartCoroutine(Skill_GauntletShockwave());
    }

    public void TryUseGauntletLaunch()
    {
        if (usingSkill) return;
        if (GauntletDeployed)
        {
            RetractGauntlet();
            return;
        }
        if (activeGauntlet != null) return;
        if (energy != null && !energy.TrySpend(gauntletSkillEnergyCost)) return;

        StartCoroutine(Skill_GauntletLaunch());
    }


    public void TryUseSwordUppercut()
    {
        if (usingSkill) return;
        if (swordUppercutCooldownTimer > 0f) return;

        if (energy != null && !energy.TrySpend(swordUppercutCost)) return;

        StartCoroutine(Skill_SwordUppercut());
    }


    #endregion
    #region Sword Skills
    // ===================== SWORD: DASH =====================
    private IEnumerator Skill_SwordDash()
    {
        usingSkill = true;
        swordDashCooldownTimer = swordDashCooldown;

        // prevent PlayerController from overwriting velocity
        if (controller) controller.externalVelocityOverride = true;

        // pass through enemies (ignore collisions during dash)
        int playerLayer = gameObject.layer;
        int enemyLayer = SingleLayerIndex(enemyMask);
        bool collisionToggled = false;
        if (enemyLayer >= 0)
        {
            Physics2D.IgnoreLayerCollision(playerLayer, enemyLayer, true);
            collisionToggled = true;
        }

        HashSet<Health> hit = new HashSet<Health>();
        Vector2 dir = (controller != null && controller.facingRight) ? Vector2.right : Vector2.left;
        Vector2 originalVel = rb.linearVelocity;

        float t = 0f;
        while (t < dashDuration)
        {
            t += Time.deltaTime;
            rb.linearVelocity = dir * dashSpeed;

            Vector2 center = (Vector2)transform.position +
                             new Vector2(dashBoxOffset.x * ((controller != null && controller.facingRight) ? 1f : -1f),
                                         dashBoxOffset.y);

            var cols = Physics2D.OverlapBoxAll(center, dashBoxSize, 0f, enemyMask);
            foreach (var c in cols)
            {
                var h = c.GetComponentInParent<Health>();
                if (h != null && !hit.Contains(h))
                {
                    hit.Add(h);
                    float dmg =  dashFlatDamage;
                    Vector2 knockDir = (h.transform.position - transform.position).normalized;
                    h.TakeDamage(dmg, knockDir);

                    if (hitstop > 0f)
                    {
                        StartCoroutine(LocalHitstop(h.GetComponent<Rigidbody2D>(), hitstop));
                        StartCoroutine(LocalHitstop(rb, hitstop));
                    }
                }
            }
            yield return null;
        }

        rb.linearVelocity = originalVel;

        if (collisionToggled) Physics2D.IgnoreLayerCollision(playerLayer, enemyLayer, false);
        if (controller) controller.externalVelocityOverride = false;

        usingSkill = false;
    }
    // ===================== SWORD: SWORD UPPERCUT =====================
    private IEnumerator Skill_SwordUppercut()
    {
        usingSkill = true;
        swordUppercutCooldownTimer = swordUppercutCooldown;

        if (controller) controller.externalVelocityOverride = true;

        //  ignore collisions with enemies while uppercutting
        int playerLayer = gameObject.layer;
        int enemyLayer = SingleLayerIndex(enemyMask);
        bool collisionToggled = false;
        if (enemyLayer >= 0)
        {
            Physics2D.IgnoreLayerCollision(playerLayer, enemyLayer, true);
            collisionToggled = true;
        }

        HashSet<Health> hit = new HashSet<Health>();
        float elapsed = 0f;

        // ---- Rising phase ----
        while (elapsed < uppercutDuration)
        {
            elapsed += Time.deltaTime;

            // Slight forward + upward force
            float forward = controller.facingRight ? uppercutForwardSpeed : -uppercutForwardSpeed;
            rb.linearVelocity = new Vector2(forward, uppercutUpSpeed);

            // Damage enemies in front/above
            Vector2 offset = new Vector2(controller.facingRight ? uppercutBoxOffset.x : -uppercutBoxOffset.x,
                                         uppercutBoxOffset.y);
            Vector2 center = (Vector2)transform.position + offset;
            var cols = Physics2D.OverlapBoxAll(center, uppercutBoxSize, 0f, enemyMask);

            foreach (var c in cols)
            {
                var h = c.GetComponentInParent<Health>();
                if (h != null && !hit.Contains(h))
                {
                    hit.Add(h);

                    float dmg = uppercutFlatDamage;
                    Vector2 knockDir = (h.transform.position - transform.position).normalized;
                    h.TakeDamage(dmg, knockDir);

                    if (hitstop > 0f)
                    {
                        StartCoroutine(LocalHitstop(h.GetComponent<Rigidbody2D>(), hitstop));
                        StartCoroutine(LocalHitstop(rb, hitstop));
                    }
                }
            }

            yield return null;
        }

        // Restore collisions
        if (collisionToggled)
            Physics2D.IgnoreLayerCollision(playerLayer, enemyLayer, false);

        if (controller) controller.externalVelocityOverride = false;
        usingSkill = false;
    }




    #endregion
    #region Gauntlet Skills
    // ===================== GAUNTLET: SHOCKWAVE =====================
    private IEnumerator Skill_GauntletShockwave()
    {
        usingSkill = true;
        gauntletShockCooldownTimer = gauntletShockCooldown;

        // If airborne: plunge pass-through
        if (!IsGrounded())
        {
            if (controller) controller.externalVelocityOverride = true;

            int playerLayer = gameObject.layer;
            int enemyLayer = SingleLayerIndex(enemyMask);
            bool collisionToggled = false;
            if (enemyLayer >= 0)
            {
                Physics2D.IgnoreLayerCollision(playerLayer, enemyLayer, true);
                collisionToggled = true;
            }

            HashSet<Health> hit = new HashSet<Health>();

            while (!IsGrounded())
            {
                rb.linearVelocity = new Vector2(rb.linearVelocity.x, -plungeSpeed);

                // damage enemies while falling
                Vector2 center = transform.position;
                Collider2D[] cols = Physics2D.OverlapCircleAll(center, 0.6f, enemyMask);
                foreach (var c in cols)
                {
                    var h = c.GetComponentInParent<Health>();
                    if (h != null && !hit.Contains(h))
                    {
                        hit.Add(h);
                        float dmg = shockwaveFlatDamage;
                        Vector2 knockDir = (h.transform.position - transform.position).normalized;
                        h.TakeDamage(dmg, knockDir);

                        if (hitstop > 0f)
                        {
                            StartCoroutine(LocalHitstop(h.GetComponent<Rigidbody2D>(), hitstop));
                            StartCoroutine(LocalHitstop(rb, hitstop));
                        }
                    }
                }

                yield return null;
            }

            if (controller) controller.externalVelocityOverride = false;

           
            if (collisionToggled)
                Physics2D.IgnoreLayerCollision(playerLayer, enemyLayer, false);

            DoShockwave();
        }
        else
        {
            DoShockwave();
        }

        usingSkill = false;
    }





    private void DoShockwave()
    {
        // Damage + radial knockback around player
        Collider2D[] cols = Physics2D.OverlapCircleAll(transform.position, shockwaveRadius, enemyMask);
        HashSet<Health> hit = new HashSet<Health>();   

        foreach (var c in cols)
        {
            var h = c.GetComponentInParent<Health>();
            if (h == null) continue;
            if (hit.Contains(h)) continue;             
            hit.Add(h);

            float dmg = shockwaveFlatDamage;

            Vector2 away = ((Vector2)h.transform.position - (Vector2)transform.position).normalized;
            Vector2 knock = away * shockwaveKnockForce + Vector2.up * shockwaveUpwardBoost;

            h.TakeDamage(dmg, knock.normalized);

            if (hitstop > 0f)
            {
                StartCoroutine(LocalHitstop(h.GetComponent<Rigidbody2D>(), hitstop));
                StartCoroutine(LocalHitstop(rb, hitstop * 0.75f));
            }
        }
    }
    // ===================== GAUNTLET: LAUNCH =====================
    private IEnumerator Skill_GauntletLaunch()
    {
        usingSkill = true;

        Vector2 dir = controller.facingRight ? Vector2.right : Vector2.left;
        GameObject g = Instantiate(gauntletPrefab, transform.position, Quaternion.identity);
        activeGauntlet = g.GetComponent<GauntletProjectile>();
        if (!activeGauntlet) activeGauntlet = g.AddComponent<GauntletProjectile>();

    
        activeGauntlet.speed = gauntletLaunchSpeed;
        activeGauntlet.Init(transform, dir, gauntletLaunchDamage, enemyMask, terrainMask, gauntletMinRange, gauntletMaxFlightRange,gauntletMaxLeashRange); 

        
        usingSkill = false;
        yield return null;
    }

    public void RetractGauntlet()
    {
        if (activeGauntlet != null)
        {
            activeGauntlet.Retract();
        }
    }
    #endregion
    #region Helpers

    // ===================== Helpers =====================
    private bool IsGrounded()
    {
        if (controller == null) return false;
        if (controller.groundCheck == null) return false;
        return Physics2D.OverlapCircle(controller.groundCheck.position, controller.groundCheckRadius, controller.groundLayer);
    }

    private IEnumerator LocalHitstop(Rigidbody2D targetRb, float duration)
    {
        if (targetRb) targetRb.simulated = false;
        yield return new WaitForSecondsRealtime(duration);
        if (targetRb) targetRb.simulated = true;
    }

    private int SingleLayerIndex(LayerMask mask)
    {
        int v = mask.value;
        if (v == 0) return -1;
        if ((v & (v - 1)) != 0) return -1;
        int index = 0;
        while ((v >>= 1) != 0) index++;
        return index;
    }

    private void OnDrawGizmosSelected()
    {
        // Sword dash gizmo
        bool facingRight = controller != null ? controller.facingRight : true;
        Vector2 dashCenter = (Vector2)transform.position +
                             new Vector2(dashBoxOffset.x * (facingRight ? 1f : -1f), dashBoxOffset.y);
        Gizmos.color = new Color(1f, 0.5f, 0f, 0.25f);
        Gizmos.DrawCube(dashCenter, dashBoxSize);

        // Shockwave radius gizmo
        Gizmos.color = new Color(0f, 0.6f, 1f, 0.15f);
        Gizmos.DrawSphere(transform.position, shockwaveRadius);

        // Sword Uppercut box gizmo
        Gizmos.color = new Color(0.9f, 0.6f, 0f, 0.25f);
        Vector2 offset = new Vector2(controller != null && controller.facingRight ? uppercutBoxOffset.x : -uppercutBoxOffset.x,
                                     uppercutBoxOffset.y);
        Vector2 center = (Vector2)transform.position + offset;
        Gizmos.DrawCube(center, uppercutBoxSize);
    }
    #endregion
}
