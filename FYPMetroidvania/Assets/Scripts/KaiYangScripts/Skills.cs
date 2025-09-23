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
    private Health health;
    private OverheatSystem overheat;

 

    // global skill gate
    private bool usingSkill = false;
    public bool IsUsingSkill => usingSkill;

    // ===================== LUNGING STRIKE =====================
    [Header("Lunging Strike")]
    public float dashSpeed = 22f;
    public float dashDuration = 0.18f;
    public float dashFlatDamage = 0f;
    public float swordDashHealthCost = 5f;
    public Vector2 dashBoxSize = new Vector2(1.4f, 1.0f);
    public Vector2 dashBoxOffset = new Vector2(0.7f, 0f);

    [Header("Lunging Strike Cooldown")]
    public float swordDashCooldown = 2f;
    private float swordDashCooldownTimer = 0f;


    [Header("Lunging Strike Cost")]
    public float swordDashCost = 20f;
    

    // ===================== ASCENDING SLASH =====================
    [Header("Ascending Slash")]
    public float uppercutUpSpeed = 12f;
    public float uppercutForwardSpeed = 4f;
    public float uppercutDuration = 0.35f;
    public float uppercutFlatDamage = 10f;
    public float swordUppercutHealthCost = 8f;
    public Vector2 uppercutBoxSize = new Vector2(1.2f, 2.0f);
    public Vector2 uppercutBoxOffset = new Vector2(0.6f, 1f);

    [Header("Ascending Slash Cost")]
    public float swordUppercutCooldown = 3f;
    private float swordUppercutCooldownTimer = 0f;
    public float swordUppercutCost = 20f;

    // ===================== CRIMSON WAVE =====================

    [Header("Crimson Wave")]
    public Transform projectileSpawnPoint; 
    public float swordSlashBloodCost = 5f;
    public float swordSlashEnergyCost;

    //COST EDIT IN SWORDPROJECTILE.CS

    // ===================== GAUNTLET SHOCKWAVE =====================
    [Header("Gauntlet Shockwave (Ground)")]
    public float shockwaveRadius = 2.5f;
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

    [Header("Gauntlet Shockwave Cost")]
    public float gauntletShockwaveCost = 30f;

    // ===================== Rocket Hand =====================


    private GauntletProjectile activeGauntlet;

    [Header("Rocket Hand")]
    public LayerMask terrainMask;
    public float gauntletLaunchDamage = 12f;
    public float gauntletLaunchSpeed = 18f;
    public float gauntletMinRange = 1.5f;
    public float gauntletMaxFlightRange = 8f;
    public float gauntletMaxLeashRange = 15f;
    public float gauntletSkillEnergyCost;

    public bool GauntletDeployed => activeGauntlet != null;
    public bool HasActiveGauntlet() => activeGauntlet != null;



    // ===================== GAUNTLET CHARGE SHOT =====================

    [Header("Gauntlet Charge Shot")]
    public GameObject gauntletChargeProjectilePrefab;
    public Transform gauntletChargeSpawnPoint;

    public float gauntletChargeMaxTime = 2f;  
    public float gauntletChargeMinDamage = 10f;
    public float gauntletChargeMaxDamage = 40f;
    public float gauntletChargeMinKnockback = 5f;
    public float gauntletChargeMaxKnockback = 15f;
    public float gauntletChargeEnergyCost = 20f;

    private float currentChargeTime;
    private bool isCharging;
    private Coroutine chargeRoutine;

    [Header("Gauntlet Charge Shot Particles")]
    public ParticleSystem sharedChargeParticles;
    private int lastStage = 0;

    //cache modules
    private ParticleSystem.MainModule chargeMain;
    private ParticleSystem.EmissionModule chargeEmission;

    [Header("Gauntlet Charge Stages")]
    public ChargeStageSettings[] chargeStages = new ChargeStageSettings[3];

    public bool IsChargeLocked { get; private set; }



    [System.Serializable]
    public class ChargeStageSettings
    {
        public Color startColor = Color.white;
        public float startSize = 0.2f;
        public float startSpeed = -2f;
        public float rateOverTime = 20f;
    }



    public bool IsChargeButtonHeld { get; set; }

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        combat = GetComponent<CombatSystem>();
        controller = GetComponent<PlayerController>();
        energy = GetComponent<EnergySystem>();
        health = GetComponent<Health>();
        overheat = GetComponent<OverheatSystem>();

        if (sharedChargeParticles != null)
        {
            chargeMain = sharedChargeParticles.main;
            chargeEmission = sharedChargeParticles.emission;
        }
    }

    private void Update()
    {
        if (swordDashCooldownTimer > 0f) swordDashCooldownTimer -= Time.deltaTime;
        if (gauntletShockCooldownTimer > 0f) gauntletShockCooldownTimer -= Time.deltaTime;
        if (swordUppercutCooldownTimer > 0f) swordUppercutCooldownTimer -= Time.deltaTime;

     
    }

    #region CombatSystem API
    public void TryUseSwordDash()
    {
        if (usingSkill) return;
        if (swordDashCooldownTimer > 0f) return;

    
        if (!PlayerController.instance.IsGrounded && PlayerController.instance.HasAirSwordDashed)
            return;

        float cost = swordDashCost;
        if (cost < 0) cost = 0;

        if (energy != null && !energy.TrySpend(cost)) return;




        StartCoroutine(Skill_SwordDash());

        if (!PlayerController.instance.IsGrounded)
            PlayerController.instance.MarkAirSwordDash();
    }

    public void TryUseSwordUppercut()
    {
        if (usingSkill) return;
        if (swordUppercutCooldownTimer > 0f) return;

        if (!PlayerController.instance.IsGrounded && PlayerController.instance.HasAirUppercut)
            return;

        float cost = swordUppercutCost;
        if (cost < 0) cost = 0;

        if (energy != null && !energy.TrySpend(cost)) return;



        StartCoroutine(Skill_SwordUppercut());

        if (!PlayerController.instance.IsGrounded)
            PlayerController.instance.MarkAirUppercut();
    }

    public void TryUseSwordCrimsonWave()
    {
        float cost = swordSlashEnergyCost;
        if (cost < 0) cost = 0;

        if (energy != null && !energy.TrySpend(cost))
            return;

        Vector2 dir = controller.facingRight ? Vector2.right : Vector2.left;
        Vector3 spawnPos = transform.position + (Vector3)(dir * 0.7f);

        SwordSlashProjectile proj = ProjectileManager.instance.SpawnSwordSlash(spawnPos, Quaternion.identity);
        if (proj != null)
        {
            // assign only what’s needed
            proj.bloodCost = swordSlashBloodCost;

            // initialize movement
            proj.Init(dir);
        }
    }




    public void TryUseGauntletShockwave()
    {
        if (usingSkill) return;
        if (overheat != null && overheat.IsOverheated) return;
        if (GauntletDeployed) { RetractGauntlet(); return; }
        if (gauntletShockCooldownTimer > 0f) return;

        float cost = gauntletShockwaveCost;
        if (cost < 0) cost = 0;

        if (energy != null && !energy.TrySpend(cost)) return;

        overheat.AddHeat(overheat.heatPerSkill);

        StartCoroutine(Skill_GauntletShockwave());
    }



    public void TryUseGauntletLaunch()
    {
        if (usingSkill) return;
        if (overheat != null && overheat.IsOverheated) return;
        if (GauntletDeployed)
        {
            RetractGauntlet();
            return;
        }
        if (activeGauntlet != null) return;

        float cost = gauntletSkillEnergyCost;
        if (energy != null && !energy.TrySpend(cost)) return;

        overheat.AddHeat(overheat.heatPerSkill);

        StartCoroutine(Skill_GauntletLaunch());
    }

    //public void TryUseGauntletChargeShot()
    //{
    //    if (!isCharging)
    //    {
    //        chargeRoutine = StartCoroutine(Skill_GauntletChargeShot());
    //    }
    //    else
    //    {
    //        if (chargeRoutine != null) StopCoroutine(chargeRoutine);
    //        StartCoroutine(FireGauntletChargeShot());
    //    }
    //}
    // Replace these methods in Skills.cs:

    public void StartGauntletChargeShot()
    {
        if (overheat != null && overheat.IsOverheated) return;
        if (!CanUseGauntletChargeShot())
        {
            if (sharedChargeParticles != null)
                sharedChargeParticles.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            return;
        }

        isCharging = true;
        IsChargeLocked = true;   
        chargeRoutine = StartCoroutine(Skill_GauntletChargeShot());
    }

    public void ReleaseGauntletChargeShot()
    {
        if (!isCharging) return;

        if (chargeRoutine != null)
        {
            StopCoroutine(chargeRoutine);
            chargeRoutine = null;
        }

        FireGauntletChargeShot();

        IsChargeLocked = false;  
    }


    private bool CanUseGauntletChargeShot()
    {
        if (isCharging) return false; 
        if (energy == null) return false;
        if (!energy.HasEnough(gauntletChargeEnergyCost)) return false; 

        return true;
    }



    private IEnumerator ChargeLoop()
    {
        while (isCharging && IsChargeButtonHeld && currentChargeTime < gauntletChargeMaxTime)
        {
            currentChargeTime += Time.deltaTime;
            float ratio = currentChargeTime / gauntletChargeMaxTime;

            int stage = 0;
            if (ratio >= 0.66f) stage = 3;
            else if (ratio >= 0.33f) stage = 2;
            else stage = 1;

            if (stage != lastStage)
            {
                PlayChargeStage(stage);
                lastStage = stage;
            }

            yield return null;
        }

        // Only auto-fire if we reached max charge AND still charging
        if (isCharging && currentChargeTime >= gauntletChargeMaxTime)
        {
            chargeRoutine = null; // Clear the reference
            FireGauntletChargeShot();
        }
    }






    #endregion

    #region Sword Skills
    private IEnumerator Skill_SwordDash()
    {
        usingSkill = true;
        swordDashCooldownTimer = swordDashCooldown;

        if (controller) controller.externalVelocityOverride = true;

        // temporarily disable collisions between player and enemies
        int playerLayer = gameObject.layer;
        int enemyLayer = SingleLayerIndex(enemyMask);
        bool collisionToggled = false;
        if (enemyLayer >= 0)
        {
            Physics2D.IgnoreLayerCollision(playerLayer, enemyLayer, true);
            collisionToggled = true;
        }

        HashSet<Health> hit = new HashSet<Health>();
        Vector2 dir = controller.facingRight ? Vector2.right : Vector2.left;

        float t = 0f;
        while (t < dashDuration)
        {
            t += Time.deltaTime;

            controller.SetVelocity(Vector2.zero);

            // movement step
            Vector2 moveStep = dir * dashSpeed * Time.deltaTime;

            // check terrain walls (not enemies)
            RaycastHit2D wallHit = Physics2D.BoxCast(
                transform.position,
                controller.colliderSize,
                0f,
                dir,
                moveStep.magnitude,
                controller.groundLayer
            );

            if (wallHit.collider != null)
            {
                // stop at wall
                float dist = wallHit.distance - 0.01f;
                transform.Translate(dir * dist);
                break;
            }
            else
            {
                transform.Translate(moveStep);
            }

            // enemy hit detection
            Vector2 center = (Vector2)transform.position +
                             new Vector2(dashBoxOffset.x * (controller.facingRight ? 1f : -1f),
                                         dashBoxOffset.y);

            var cols = Physics2D.OverlapBoxAll(center, dashBoxSize, 0f, enemyMask);
            foreach (var c in cols)
            {
                var h = c.GetComponentInParent<Health>();
                if (h != null && !hit.Contains(h))
                {
                    hit.Add(h);
                    float dmg = dashFlatDamage;
                    Vector2 knockDir = (h.transform.position - transform.position).normalized;
                    h.TakeDamage(dmg, knockDir);

                    if (!h.isPlayer)
                    {
                        h.ApplyBloodMark();
                        if (health != null && swordDashHealthCost > 0f)
                        {
                            float safeCost = Mathf.Min(swordDashHealthCost, health.CurrentHealth - 1f);
                            if (safeCost > 0f)
                                health.TakeDamage(safeCost);
                        }
                    }

                    if (hitstop > 0f)
                    {
                        // Enemy hitstop
                        StartCoroutine(LocalHitstop(h.GetComponent<Rigidbody2D>(), hitstop));
                        // Player hitstop
                        StartCoroutine(LocalHitstop(rb, hitstop));
                    }

                }
            }

            yield return null;
        }

        // restore collisions
        if (collisionToggled)
            Physics2D.IgnoreLayerCollision(playerLayer, enemyLayer, false);

        if (controller) controller.externalVelocityOverride = false;
        usingSkill = false;
    }



    private IEnumerator Skill_SwordUppercut()
    {
        usingSkill = true;
        swordUppercutCooldownTimer = swordUppercutCooldown;

        if (controller) controller.externalVelocityOverride = true;

        // temporarily disable collisions with enemies
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

        float forward = controller.facingRight ? uppercutForwardSpeed : -uppercutForwardSpeed;

        // --- Phase 1: short forward dash (little upward) ---
        controller.SetVelocity(new Vector2(forward, uppercutUpSpeed * 0.25f));
        float dashPhase = 0.12f; // tweak: how long forward dash lasts
        while (elapsed < dashPhase)
        {
            elapsed += Time.deltaTime;
            DoUppercutHitDetection(hit);
            yield return null;
        }

        // --- Phase 2: rising slash (forward + strong upward) ---
        controller.SetVelocity(new Vector2(forward * 0.7f, uppercutUpSpeed));
        while (elapsed < uppercutDuration)
        {
            elapsed += Time.deltaTime;
            DoUppercutHitDetection(hit);
            yield return null;
        }

        // restore collisions
        if (collisionToggled)
            Physics2D.IgnoreLayerCollision(playerLayer, enemyLayer, false);

        if (controller) controller.externalVelocityOverride = false;
        usingSkill = false;
    }
    private void DoUppercutHitDetection(HashSet<Health> hit)
    {
        Vector2 offset = new Vector2(
            controller.facingRight ? uppercutBoxOffset.x : -uppercutBoxOffset.x,
            uppercutBoxOffset.y
        );
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

                if (!h.isPlayer)
                {
                    h.ApplyBloodMark();
                    if (health != null && swordUppercutHealthCost > 0f)
                    {
                        float safeCost = Mathf.Min(swordUppercutHealthCost, health.CurrentHealth - 1f);
                        if (safeCost > 0f)
                            health.TakeDamage(safeCost);
                    }
                }

                if (hitstop > 0f)
                {
                    StartCoroutine(LocalHitstop(h.GetComponent<Rigidbody2D>(), hitstop));
                    StartCoroutine(LocalHitstop(rb, hitstop));
                }
            }
        }
    }



    #endregion

    #region Gauntlet Skills
    private IEnumerator Skill_GauntletShockwave()
    {
        usingSkill = true;
        gauntletShockCooldownTimer = gauntletShockCooldown;

        // air to plunge
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

            while (true)
            {
                Vector2 moveStep = Vector2.down * plungeSpeed * Time.deltaTime;

                //stop going thru ground
                RaycastHit2D hitGround = Physics2D.BoxCast(
                    transform.position,
                    controller.colliderSize,
                    0f,
                    Vector2.down,
                    moveStep.magnitude,
                    controller.groundLayer
                );

                if (hitGround.collider != null)
                {
                    //hit ground
                    float dist = hitGround.distance - 0.01f;
                    transform.Translate(Vector2.down * dist);
                    break; 
                }
                else
                {
                    transform.Translate(moveStep);
                }

                // enemy hit detection mid-plunge
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
                            StartCoroutine(LocalHitstop(rb, hitstop * 0.75f));
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

    private IEnumerator Skill_GauntletLaunch()
    {
        usingSkill = true;

        Vector2 dir = controller.facingRight ? Vector2.right : Vector2.left;

        activeGauntlet = ProjectileManager.instance.SpawnGauntlet(transform.position,Quaternion.identity);
        activeGauntlet.speed = gauntletLaunchSpeed;
        activeGauntlet.Init(transform, dir, activeGauntlet.damage, enemyMask, terrainMask,gauntletMaxFlightRange, gauntletMaxLeashRange);


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

    private IEnumerator Skill_GauntletChargeShot()
    {
        isCharging = true;
        currentChargeTime = 0f;
        lastStage = 0;

        while (currentChargeTime < gauntletChargeMaxTime && IsChargeButtonHeld)
        {
            currentChargeTime += Time.deltaTime;
            float ratio = currentChargeTime / gauntletChargeMaxTime;

            int stage = 0;
            if (ratio >= 0.66f) stage = 3;
            else if (ratio >= 0.33f) stage = 2;
            else stage = 1;

            if (stage != lastStage)
            {
                PlayChargeStage(stage);
                lastStage = stage;
            }

            yield return null;
        }


        FireGauntletChargeShot();
        yield break;
    }

    private void PlayChargeStage(int stage)
    {
        if (sharedChargeParticles == null) return;
        if (stage <= 0 || stage > chargeStages.Length) return;

        var settings = chargeStages[stage - 1]; 
        chargeMain.startColor = settings.startColor;
        chargeMain.startSize = settings.startSize;
        chargeMain.startSpeed = settings.startSpeed;
        chargeEmission.rateOverTime = settings.rateOverTime;

        if (!sharedChargeParticles.isPlaying)
            sharedChargeParticles.Play();
    }





    private void FireGauntletChargeShot()
    {
        if (!isCharging) return;

        isCharging = false;

        if (sharedChargeParticles != null)
            sharedChargeParticles.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);

        if (energy != null && !energy.TrySpend(gauntletChargeEnergyCost))
        {
            currentChargeTime = 0f;
            return;
        }

        float ratio = Mathf.Clamp01(currentChargeTime / gauntletChargeMaxTime);
        float damage = Mathf.Lerp(gauntletChargeMinDamage, gauntletChargeMaxDamage, ratio);
        float knockback = Mathf.Lerp(gauntletChargeMinKnockback, gauntletChargeMaxKnockback, ratio);

        Vector2 dir = controller.facingRight ? Vector2.right : Vector2.left;

        GauntletChargeProjectile chargeProj = ProjectileManager.instance.SpawnGauntletCharge(
            gauntletChargeSpawnPoint.position, Quaternion.identity
        );
        if (chargeProj != null)
            chargeProj.Init(dir, damage, knockback, ratio);

        if (overheat != null)
        {
            if (ratio < 0.33f)          // Stage 1
                overheat.AddHeat(overheat.heatPerSkill * 0.5f); 
            else if (ratio < 0.66f)     // Stage 2
                overheat.AddHeat(overheat.heatPerSkill * 1.0f);  
            else                        // Stage 3
                overheat.AddHeat(overheat.heatPerSkill * 1.5f);  
        }

        currentChargeTime = 0f;
        IsChargeLocked = false;

        Debug.Log($"Fired charge shot at {ratio * 100f}% charge");
    }



    #endregion

    #region Helpers
    private bool IsGrounded()
    {
        if (controller == null) return false;
        if (controller.groundCheck == null) return false;
        return Physics2D.OverlapCircle(controller.groundCheck.position, controller.groundCheckRadius, controller.groundLayer);
    }

    public void ClearGauntlet()
    {
        activeGauntlet = null;
    }

    public IEnumerator LocalHitstop(Rigidbody2D targetRb, float duration)
    {
        PlayerController pc = targetRb ? targetRb.GetComponent<PlayerController>() : null;

        Vector2 savedVel = Vector2.zero;
        if (pc != null) savedVel = pc.GetVelocity();

        // freeze
        if (pc != null) pc.SetHitstop(true);
        if (targetRb) targetRb.simulated = false;

        yield return new WaitForSecondsRealtime(duration);

        // unfreeze
        if (pc != null)
        {
            pc.SetHitstop(false);
            pc.SetVelocity(savedVel);
        }
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
    #endregion

    #region Gizmos
    private void OnDrawGizmosSelected()
    {
        // --- Sword Dash Box ---
        Gizmos.color = Color.red;
        Vector2 dashCenter = (Vector2)transform.position +
                             new Vector2(dashBoxOffset.x * (controller != null && controller.facingRight ? 1f : -1f),
                                         dashBoxOffset.y);
        Gizmos.DrawWireCube(dashCenter, dashBoxSize); 

        // --- Sword Uppercut Box ---
        Gizmos.color = Color.blue;
        if (controller != null)
        {
            Vector2 upOffset = new Vector2(controller.facingRight ? uppercutBoxOffset.x : -uppercutBoxOffset.x,
                                           uppercutBoxOffset.y);
            Vector2 upCenter = (Vector2)transform.position + upOffset;
            Gizmos.DrawWireCube(upCenter, uppercutBoxSize);
        }

        // --- Gauntlet Shockwave Radius ---
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, shockwaveRadius);
    }
    #endregion
}
