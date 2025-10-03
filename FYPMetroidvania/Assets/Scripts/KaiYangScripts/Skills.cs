using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Unity.Collections.AllocatorManager;

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
    private GauntletCannon activeCannon;


    // global skill gate
    private bool usingSkill = false;
    public bool IsUsingSkill => usingSkill;

    private bool usingUltimate = false;
    public bool IsUsingUltimate => usingUltimate;


    // SKILL EVENTS - for ian
    public static event System.Action<Hitbox> skillStart;
    public static event System.Action skillEnd;
    public static event System.Action<Hitbox, Health> skillHit;

    // ULTIMATE EVENTS - for ian
    public static event System.Action<Hitbox> OnUltimateStart;
    public static event System.Action OnUltimateEnd;
    public static event System.Action<Hitbox, Health> OnUltimateHit;

    #region Skills Variables

    [Header("Lunging Strike Knockback")]
    public float swordDashStunKnockbackMultiplier = 1f;
    public float swordDashKnockdownKnockbackMultiplier = 1f;

    [Header("Ascending Slash Knockback")]
    public float swordUppercutStunKnockbackMultiplier = 1f;
    public float swordUppercutKnockdownKnockbackMultiplier = 1f;

    [Header("Crimson Wave Knockback")]
    public float crimsonWaveStunKnockbackMultiplier = 1f;
    public float crimsonWaveKnockdownKnockbackMultiplier = 1f;


    [Header("Gauntlet Shockwave Knockback")]
    public float gauntletShockwaveStunKnockbackMultiplier = 1f;
    public float gauntletShockwaveKnockdownKnockbackMultiplier = 1f;

    [Header("Gauntlet Charge Shot Knockback")]
    public float gauntletChargeStunKnockbackMultiplier = 1f;
    public float gauntletChargeKnockdownKnockbackMultiplier = 1f;


    // ===================== Skills Prefab =====================

    [Header("Skill Hitboxes")]
    public GameObject swordDashHitbox;
    public GameObject swordUppercutHitbox;
    public GameObject gauntletShockwaveHitbox;


    // ===================== LUNGING STRIKE =====================
    [Header("Lunging Strike")]
    public float dashSpeed = 22f;
    public float dashDuration = 0.18f;
    public float dashFlatDamage = 0f;
    public float swordDashHealthCost = 5f;

    public CrowdControlState swordDashGroundedCC = CrowdControlState.Stunned;
    public CrowdControlState swordDashAirborneCC = CrowdControlState.Knockdown;
    public float swordDashCCDuration = 1.5f;

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

    public CrowdControlState swordUppercutGroundedCC = CrowdControlState.Knockdown; 
    public CrowdControlState swordUppercutAirborneCC = CrowdControlState.Knockdown;
    public float swordUppercutCCDuration = 2.0f;

    [Header("Ascending Slash Cost")]
    public float swordUppercutCooldown = 3f;
    private float swordUppercutCooldownTimer = 0f;
    public float swordUppercutCost = 20f;

    // ===================== CRIMSON WAVE =====================



    [Header("Crimson Wave")]
    public Transform projectileSpawnPoint;
    public float swordSlashBloodCost = 5f;
    public float swordSlashEnergyCost;
    public CrowdControlState crimsonWaveGroundedCC = CrowdControlState.Stunned;
    public CrowdControlState crimsonWaveAirborneCC = CrowdControlState.Knockdown;
    public float crimsonWaveCCDuration = 1.0f;

    //COST EDIT IN SWORDPROJECTILE.CS

    // ===================== GAUNTLET SHOCKWAVE =====================
    [Header("Gauntlet Shockwave")]
    public float shockwaveRadius = 2.5f;
    public float shockwaveFlatDamage = 0f;
    public float shockwaveKnockForce = 12f;
    public float shockwaveUpwardBoost = 6f;

    public CrowdControlState gauntletShockwaveGroundedCC = CrowdControlState.Knockdown;
    public CrowdControlState gauntletShockwaveAirborneCC = CrowdControlState.Knockdown;
    public float gauntletShockwaveCCDuration = 2.5f;

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

        public CrowdControlState groundedCC = CrowdControlState.Stunned;
        public CrowdControlState airborneCC = CrowdControlState.Knockdown;
        public float ccDuration = 1.0f;
    }

    #endregion

    #region Ultimate Variables
    [Header("Sword Ultimate")]
    public GameObject spiritSlashPrefab;
    public float spiritSlashInterval = 0.2f; 
    public float spiritSlashRadius = 6f;
    public float spiritSlashHealthCost = 10f;

    private SpiritGauge spirit;


    [Header("Gauntlet Ultimate")]
    public GameObject gauntletCannonPrefab;
    public float gauntletBeamDamage = 10f;
    public float gauntletBeamTickRate = 0.5f;
    public Vector2 gauntletBeamSize = new Vector2(6f, 2f);
    public float gauntletBeamChargeTime = 2f;
    public float gauntletBeamDuration = 3f;


    [Header("Spirit Gain")]
    public float spiritGainPerHit = 5f;   
    public float spiritGainPerSkill = 10f; 

    #endregion


    public bool IsChargeButtonHeld { get; set; }

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        combat = GetComponent<CombatSystem>();
        controller = GetComponent<PlayerController>();
        energy = GetComponent<EnergySystem>();
        health = GetComponent<Health>();
        overheat = GetComponent<OverheatSystem>();
        spirit = GetComponent<SpiritGauge>();


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
            proj.bloodCost = swordSlashBloodCost;
            proj.groundedCC = crimsonWaveGroundedCC;
            proj.airborneCC = crimsonWaveAirborneCC;
            proj.ccDuration = crimsonWaveCCDuration;
            proj.stunKnockbackMultiplier = crimsonWaveStunKnockbackMultiplier;
            proj.knockdownKnockbackMultiplier = crimsonWaveKnockdownKnockbackMultiplier;
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

        if (isCharging && currentChargeTime >= gauntletChargeMaxTime)
        {
            chargeRoutine = null; 
            FireGauntletChargeShot();
        }
    }

    // Ultimates

    public void TryUseSwordUltimate()
    {
        if (usingUltimate) return;
        if (spirit == null || spirit.IsEmpty) return;

        StartCoroutine(Skill_SwordUltimate());
    }


    private IEnumerator Skill_SwordUltimate()
    {
        usingUltimate = true;
        spirit.StartDrain();

        // Find first enemy in range
        Collider2D[] enemies = Physics2D.OverlapCircleAll(transform.position, spiritSlashRadius, enemyMask);
        Transform firstTarget = enemies.Length > 0 ? enemies[Random.Range(0, enemies.Length)].transform : null;

        if (firstTarget == null)
        {
            Debug.Log("No enemies in range for ultimate!");
            spirit.StopDrain();
            usingUltimate = false;
            yield return null;
        }

        GameObject slash = Instantiate(spiritSlashPrefab, transform.position, Quaternion.identity);
        SpiritSlash slashComp = slash.GetComponent<SpiritSlash>();

        if (slashComp != null)
        {
            slashComp.Init(transform, firstTarget, enemyMask);
        }

        // Wait until spirit is fully drained
        while (!spirit.IsEmpty)
        {
            yield return null;
        }

        if (slash != null)
        {
            Destroy(slash);
        }

        spirit.StopDrain();
        usingUltimate = false;
    }
 

    public void TryUseGauntletUltimate()
    {
        if (spirit == null || spirit.IsEmpty) return;

        if (activeCannon != null)
        {
            activeCannon.ManualOverrideFire();
            return;
        }

        if (usingUltimate) return;

        usingUltimate = true;

        var cannon = Instantiate(gauntletCannonPrefab, PlayerController.instance.transform.position, Quaternion.identity);
        activeCannon = cannon.GetComponent<GauntletCannon>();
        activeCannon.Init(
            PlayerController.instance.facingRight,
            spirit,
            enemyMask,
            gauntletBeamChargeTime,
            gauntletBeamDamage,
            gauntletBeamTickRate,
            gauntletBeamSize,
            gauntletBeamDuration
        );

        activeCannon.OnFinished += () =>
        {
            activeCannon = null;
            usingUltimate = false;
        };
    }



    #endregion

    #region Sword Skills
    private IEnumerator Skill_SwordDash()
    {
        usingSkill = true;
        swordDashCooldownTimer = swordDashCooldown;

        if (controller) controller.externalVelocityOverride = true;

        int playerLayer = gameObject.layer;
        int enemyLayer = SingleLayerIndex(enemyMask);
        bool collisionToggled = false;
        if (enemyLayer >= 0)
        {
            Physics2D.IgnoreLayerCollision(playerLayer, enemyLayer, true);
            collisionToggled = true;
        }

        Vector2 dir = controller.facingRight ? Vector2.right : Vector2.left;
        float t = 0f;

        // --- Hook into hitbox for Sword Dash specific logic ---
        void OnDashHit(Hitbox hb, Health h)
        {
            if (h == null || h.isPlayer) return;

            Vector2 knockDir = (h.transform.position - transform.position).normalized;

            h.TakeDamage(dashFlatDamage, knockDir, false, CrowdControlState.None, 0f, true, false, 0f);



            // Apply Sword Dash CC
            ApplySkillCC(h, knockDir, swordDashGroundedCC, swordDashAirborneCC, swordDashCCDuration,
              swordDashStunKnockbackMultiplier, swordDashKnockdownKnockbackMultiplier);


            // Spirit + BloodMark + HealthCost (only Sword)
            h.ApplyBloodMark();
            GainSpirit(spiritGainPerHit);

            if (health != null && swordDashHealthCost > 0f)
            {
                float safeCost = Mathf.Min(swordDashHealthCost, health.CurrentHealth - 1f);
                if (safeCost > 0f) health.TakeDamage(safeCost);
            }

            // Local hitstop
            if (hitstop > 0f)
            {
                StartCoroutine(LocalHitstop(h.GetComponent<Rigidbody2D>(), hitstop));
                StartCoroutine(LocalHitstop(rb, hitstop));
            }
        }

        Hitbox.OnHit += OnDashHit;
        //SKILL START, SKILL HIT, SKILL END
        Coroutine hitboxRoutine = StartCoroutine(ActivateSkillHitbox(swordDashHitbox, dashDuration));

        // --- Dash loop ---
        while (t < dashDuration)
        {
            t += Time.deltaTime;
            controller.SetVelocity(Vector2.zero);

            Vector2 moveStep = dir * dashSpeed * Time.deltaTime;

            // Stop if wall ahead
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
                float dist = wallHit.distance - 0.01f;
                transform.Translate(dir * dist);
                break;
            }
            else
            {
                transform.Translate(moveStep);
            }

            yield return null;
        }

        // --- Cleanup ---
        if (hitboxRoutine != null)
            yield return hitboxRoutine; 

        Hitbox.OnHit -= OnDashHit;

        if (collisionToggled)
            Physics2D.IgnoreLayerCollision(playerLayer, enemyLayer, false);

        if (controller)
        {
            controller.externalVelocityOverride = false;
            controller.SetHitstop(false);
        }

        usingSkill = false;
    }



    private IEnumerator Skill_SwordUppercut()
    {
        usingSkill = true;
        swordUppercutCooldownTimer = swordUppercutCooldown;

        if (controller) controller.externalVelocityOverride = true;

        // Disable collisions with enemies temporarily
        int playerLayer = gameObject.layer;
        int enemyLayer = SingleLayerIndex(enemyMask);
        bool collisionToggled = false;
        if (enemyLayer >= 0)
        {
            Physics2D.IgnoreLayerCollision(playerLayer, enemyLayer, true);
            collisionToggled = true;
        }

        float elapsed = 0f;
        float forward = controller.facingRight ? uppercutForwardSpeed : -uppercutForwardSpeed;

        // --- Hook into hitbox for Uppercut specific logic ---
        void OnUppercutHit(Hitbox hb, Health h)
        {
            if (h == null || h.isPlayer) return;

            Vector2 knockDir = (h.transform.position - transform.position).normalized;

            h.TakeDamage(uppercutFlatDamage, knockDir, false, CrowdControlState.None, 0f, true, false, 0f);

            // Apply Uppercut CC
            ApplySkillCC(h, knockDir, swordUppercutGroundedCC, swordUppercutAirborneCC, swordUppercutCCDuration,
              swordUppercutStunKnockbackMultiplier, swordUppercutKnockdownKnockbackMultiplier);

            // Spirit + BloodMark + HealthCost
            h.ApplyBloodMark();
            GainSpirit(spiritGainPerHit);

            if (health != null && swordUppercutHealthCost > 0f)
            {
                float safeCost = Mathf.Min(swordUppercutHealthCost, health.CurrentHealth - 1f);
                if (safeCost > 0f) health.TakeDamage(safeCost);
            }

            // Local hitstop
            if (hitstop > 0f)
            {
                StartCoroutine(LocalHitstop(h.GetComponent<Rigidbody2D>(), hitstop));
                StartCoroutine(LocalHitstop(rb, hitstop));
            }
        }

        Hitbox.OnHit += OnUppercutHit;

        // --- Phase 1: short forward dash ---
        controller.SetVelocity(new Vector2(forward, uppercutUpSpeed * 0.25f));
        float dashPhase = 0.12f;

        // Activate hitbox during dash phase
        Coroutine hitboxRoutine = StartCoroutine(ActivateSkillHitbox(swordUppercutHitbox, uppercutDuration));

        while (elapsed < dashPhase)
        {
            elapsed += Time.deltaTime;
            yield return null;
        }

        // --- Phase 2: rising slash (forward + strong upward) ---
        controller.SetVelocity(new Vector2(forward * 0.7f, uppercutUpSpeed));
        while (elapsed < uppercutDuration)
        {
            elapsed += Time.deltaTime;
            yield return null;
        }

        // Wait for hitbox to finish
        if (hitboxRoutine != null)
            yield return hitboxRoutine;

        // Cleanup
        Hitbox.OnHit -= OnUppercutHit;

        // Restore collisions
        if (collisionToggled)
            Physics2D.IgnoreLayerCollision(playerLayer, enemyLayer, false);

        if (controller) controller.externalVelocityOverride = false;
        usingSkill = false;
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

            HashSet<Health> plungeHits = new HashSet<Health>();

            // Plunge downward
            while (true)
            {
                Vector2 moveStep = Vector2.down * plungeSpeed * Time.deltaTime;

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
                    float dist = hitGround.distance - 0.01f;
                    transform.Translate(Vector2.down * dist);
                    break;
                }
                else
                {
                    transform.Translate(moveStep);
                }

                // Mid-plunge hit detection
                Vector2 center = transform.position;
                Collider2D[] cols = Physics2D.OverlapCircleAll(center, 0.6f, enemyMask);
                foreach (var c in cols)
                {
                    var h = c.GetComponentInParent<Health>();
                    if (h != null && !plungeHits.Contains(h))
                    {
                        plungeHits.Add(h);
                        float dmg = shockwaveFlatDamage;
                        GainSpirit(spiritGainPerHit);
                        Vector2 knockDir = (h.transform.position - transform.position).normalized;
                        h.TakeDamage(dmg, knockDir.normalized, false, CrowdControlState.None, 0f);
                        ApplySkillCC(h, knockDir.normalized, gauntletShockwaveGroundedCC, gauntletShockwaveAirborneCC, gauntletShockwaveCCDuration);

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
        }

        // Now use hitbox for the actual shockwave impact
        yield return StartCoroutine(DoShockwaveWithHitbox());

        usingSkill = false;
    }

    private IEnumerator DoShockwaveWithHitbox()
    {
        // Hook into hitbox for Shockwave specific logic
        void OnShockwaveHit(Hitbox hb, Health h)
        {
            if (h == null || h.isPlayer) return;

            Vector2 knockDir = (h.transform.position - transform.position).normalized;

            // Apply Shockwave CC with separate multipliers
            ApplySkillCC(h, knockDir, gauntletShockwaveGroundedCC, gauntletShockwaveAirborneCC,
                gauntletShockwaveCCDuration,
                gauntletShockwaveStunKnockbackMultiplier,
                gauntletShockwaveKnockdownKnockbackMultiplier);


            // Apply Shockwave CC
            ApplySkillCC(h, knockDir, gauntletShockwaveGroundedCC, gauntletShockwaveAirborneCC, gauntletShockwaveCCDuration);

            // Spirit gain
            GainSpirit(spiritGainPerHit);

            // Local hitstop
            if (hitstop > 0f)
            {
                StartCoroutine(LocalHitstop(h.GetComponent<Rigidbody2D>(), hitstop));
                StartCoroutine(LocalHitstop(rb, hitstop * 0.75f));
            }
        }

        Hitbox.OnHit += OnShockwaveHit;

        // Activate the shockwave hitbox briefly (like 0.1-0.2 seconds)
        float shockwaveDuration = 0.15f;
        Coroutine hitboxRoutine = StartCoroutine(ActivateSkillHitbox(gauntletShockwaveHitbox, shockwaveDuration));

        yield return hitboxRoutine;

        Hitbox.OnHit -= OnShockwaveHit;
    }

    private IEnumerator Skill_GauntletLaunch()
    {
        usingSkill = true;

        Vector2 dir = Get8DirectionalAim();

        activeGauntlet = ProjectileManager.instance.SpawnGauntlet(transform.position,Quaternion.identity);
        activeGauntlet.speed = gauntletLaunchSpeed;
        activeGauntlet.Init(transform, dir, activeGauntlet.damage, enemyMask, terrainMask,gauntletMaxFlightRange, gauntletMaxLeashRange);


        usingSkill = false;
        yield return null;
    }

    private Vector2 Get8DirectionalAim()
    {
        Vector2 input = controller.moveInput;


        if (input.sqrMagnitude < 0.1f)
        {
            return controller.facingRight ? Vector2.right : Vector2.left;
        }

        // Normalize to get clean 8 directions
        float x = Mathf.Round(input.x);
        float y = Mathf.Round(input.y);

        Vector2 direction = new Vector2(x, y);

        if (direction.sqrMagnitude > 1.1f)
        {
            direction.Normalize();
        }

        return direction;
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
        {
            int stageIndex = Mathf.Clamp(lastStage - 1, 0, chargeStages.Length - 1);
            var stageSettings = chargeStages[stageIndex];

            chargeProj.Init(dir, damage, knockback, ratio,
                   stageSettings.groundedCC,
                   stageSettings.airborneCC,
                   stageSettings.ccDuration,
                   gauntletChargeStunKnockbackMultiplier,      
                   gauntletChargeKnockdownKnockbackMultiplier); 
        }


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
    private int SingleLayerIndex(LayerMask mask)
    {
        int v = mask.value;
        if (v == 0) return -1;
        if ((v & (v - 1)) != 0) return -1;
        int index = 0;
        while ((v >>= 1) != 0) index++;
        return index;
    }

    public void GainSpirit(float amount)
    {
        if (spirit != null && amount > 0f)
            spirit.Refill(amount);
    }
    // SKILL START END AND HIT
    private IEnumerator ActivateSkillHitbox(GameObject hitbox, float duration)
    {
        Hitbox hb = hitbox.GetComponent<Hitbox>();
        if (hb == null) yield break;

        
        System.Action<Hitbox, Health> onHit = (h, enemy) =>
        {
            skillHit?.Invoke(h, enemy);  
        };
        Hitbox.OnHit += onHit;

        skillStart?.Invoke(hb);

        // Activate
        hitbox.SetActive(true);
        yield return new WaitForSeconds(duration);

        // Deactivate
        hitbox.SetActive(false);

        // Fire end event
        skillEnd?.Invoke();

        // Unsubscribe
        Hitbox.OnHit -= onHit;
    }



    public void ApplySkillCC(Health target, Vector2 knockDir,
   CrowdControlState groundedCC, CrowdControlState airborneCC,
   float ccDuration = 0f, float stunKnockbackMultiplier = 1f, float knockdownKnockbackMultiplier = 1f)
    {
        if (!target) return;

        RaycastHit2D hit = Physics2D.Raycast(target.transform.position, Vector2.down, target.groundCheckValue, LayerMask.GetMask("Ground"));
        bool grounded = hit.collider != null;

        if (grounded)
        {
            if (groundedCC == CrowdControlState.Stunned)
                target.ApplyStun(ccDuration > 0 ? ccDuration : target.defaultStunDuration, knockDir, stunKnockbackMultiplier);
            else if (groundedCC == CrowdControlState.Knockdown)
                target.ApplyKnockdown(ccDuration > 0 ? ccDuration : target.defaultKnockdownDuration, false, knockDir, false, knockdownKnockbackMultiplier);
        }
        else
        {
            if (airborneCC == CrowdControlState.Stunned)
                target.ApplyStun(ccDuration > 0 ? ccDuration : target.defaultStunDuration, knockDir, stunKnockbackMultiplier);
            else if (airborneCC == CrowdControlState.Knockdown)
                target.ApplyKnockdown(ccDuration > 0 ? ccDuration : target.defaultKnockdownDuration, true, knockDir, false, knockdownKnockbackMultiplier);
        }
    }



    //INVOKES ARE HERE
    #region InvokeSkillsStartHitEnd
    public static void InvokeSkillStart(Hitbox hitbox)
    {
        skillStart?.Invoke(hitbox);
    }

    public static void InvokeSkillHit(Hitbox hitbox, Health enemy)
    {
        skillHit?.Invoke(hitbox, enemy);
    }

    public static void InvokeSkillEnd()
    {
        skillEnd?.Invoke();
    }

    public static void InvokeUltimateStart(Hitbox hitbox)
    {
        OnUltimateStart?.Invoke(hitbox);
    }

    public static void InvokeUltimateHit(Hitbox hitbox, Health enemy)
    {
        OnUltimateHit?.Invoke(hitbox, enemy);
    }

    public static void InvokeUltimateEnd()
    {
        OnUltimateEnd?.Invoke();
    }
    #endregion

    #endregion

    #region Gizmos
    private void OnDrawGizmosSelected()
    {

    

        // --- Gauntlet Shockwave Radius ---
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, shockwaveRadius);
    }
    #endregion
}
