using NUnit.Framework;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public enum CrowdControlState
{
    None,
    Stunned,
    Knockdown
}
public class Health : MonoBehaviour
{
    [Header("Health Settings")]
    public float maxHealth = 100f;
    public bool destroyOnDeath = true;
    public bool isPlayer = false;
    public float currentHealth;

    [Header("Feedback")]
    public SpriteRenderer spriteRenderer;
    public Color originalColor;
    public float flashDuration = 0.1f;
    public AudioClip hitSound;
    public AudioClip deathSound;
    public Slider healthBar;
    public Color defaultDamageColor;

    [Header("Arc Knockdown")]
    public float arcKnockdownGravity = 15f;
    public bool isInArcKnockdown = false;

    [Header("Default Knockback")]
    public float knockbackForce = 5f;
    public float stunPushbackForce = 5f;
    public float groundCheckValue = 1f;
    public float knockbackMult = 1f;

    [Header("Crowd Control Durations")]
    [Tooltip("Default stun duration when grounded")]
    public float defaultStunDuration = 1.0f;
    [Tooltip("Default knockdown duration when airborne")]
    public float defaultKnockdownDuration = 2.0f;
    [Tooltip("Recovery time after landing from knockdown")]
    public float knockdownRecoveryTime = 1.0f;

    [Header("Combat States")]
    public CrowdControlState currentCCState = CrowdControlState.None;
    public bool knockdownLanded = false;
    public bool stunImmune = false;
    public bool knockdownImmune = false;

    public float ccTimer = 0f;

    public float invincibilityDuration = 0.3f; // player only
    private Animator animator;

    public bool invincible = false;

    //Enemy Use
    [Header("Blood Mark")]
    public bool isBloodMarked = false;
    public float bloodMarkHealAmount = 10f;
    public GameObject bloodMarkIcon;

    [Header("Debuffs")]
    public List<DebuffInstance> debuffs = new();
    public List<GameObject> debuffVFXs = new();

    // Events
    public Action<Health> damageTaken;
    public System.Action<GameObject> enemyDeath;
    public Action<Health, float, Color> updateUI;

    private AudioSource audioSource;
    private Rigidbody2D rb;

    public float CurrentHealth => currentHealth;


    private void Awake()
    {
        currentHealth = maxHealth;
        rb = GetComponent<Rigidbody2D>();
        audioSource = GetComponent<AudioSource>();
        animator = GetComponentInChildren<Animator>();
        if (spriteRenderer == null)
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
        }
        originalColor = spriteRenderer.color;
    }

    private void Update()
    {
        UpdateDebuffs();

        // === CC TIMERS ===
        if (currentCCState != CrowdControlState.None)
        {
            if (currentCCState == CrowdControlState.Knockdown)
            {
                bool landed = false;

                if (isPlayer)
                {
                    landed = PlayerController.instance != null && PlayerController.instance.IsGrounded;
                }
                else
                {
                    float rayDist = groundCheckValue > 0 ? groundCheckValue : 0.5f;
                    LayerMask groundMask = LayerMask.GetMask("Ground") | LayerMask.GetMask("Platform");
                    RaycastHit2D hit = Physics2D.Raycast(transform.position, Vector2.down, rayDist, groundMask);

                    bool touchingGround = hit.collider != null;
                    bool stoppedFalling = rb != null && Mathf.Abs(rb.linearVelocity.y) < 0.1f;

                    landed = touchingGround && stoppedFalling;
                }
                // Regular knockdown timer countdown (only when grounded)

                if (ccTimer > 0f && !(isPlayer && isInArcKnockdown) && landed)
                {
                    if (landed) invincible = true;
                    ccTimer -= Time.deltaTime;
                    if (ccTimer <= 0f)
                    {
                        currentCCState = CrowdControlState.None;
                        invincible = false;
                        // Clear external velocity override for player
                        if (isPlayer)
                        {
                            var pc = GetComponent<PlayerController>();
                            if (pc != null)
                            {
                                pc.externalVelocityOverride = false;
                                pc.isInKnockback = false;
                            }

                            // Also reset any stuck skill states
                            var skills = GetComponent<Skills>();
                            if (skills != null)
                            {
                                skills.ResetState();
                            }
                        }

                        Debug.Log($"{gameObject.name} recovered from knockdown - can move again!");
                    }
                }
            }
            else
            {
                // For stun and other CC states, count down normally
                if (ccTimer >= 0f)
                {
                    ccTimer -= Time.deltaTime;
                    if (ccTimer <= 0f)
                    {
                        currentCCState = CrowdControlState.None;

                        // IMPORTANT: Clear external velocity override for player
                        if (isPlayer)
                        {
                            var pc = GetComponent<PlayerController>();
                            if (pc != null)
                            {
                                pc.externalVelocityOverride = false;
                                pc.isInKnockback = false;
                            }

                            // Also reset any stuck skill states
                            var skills = GetComponent<Skills>();
                            if (skills != null)
                            {
                                skills.ResetState();
                            }
                        }

                        Debug.Log($"{gameObject.name} recovered from CC state");
                    }
                }
            }
        }
        else
        {
            // NOT IN CC STATE - make sure player isn't stuck
            if (isPlayer)
            {
                var pc = GetComponent<PlayerController>();
                var skills = GetComponent<Skills>();

                // Safety check: if not in CC and not using skill, clear override
                if (pc != null && skills != null)
                {
                    if (!skills.IsUsingSkill && pc.externalVelocityOverride && !pc.isInKnockback)
                    {
                        Debug.LogWarning("Detected stuck state - clearing externalVelocityOverride");
                        pc.externalVelocityOverride = false;
                    }
                }
            }
        }

        if (isPlayer) // only test on the player
        {
            if (Input.GetKeyDown(KeyCode.Alpha1)) // press 1 to damage self
                TakeDamage(10f);

            if (Input.GetKeyDown(KeyCode.Alpha2)) // press 2 to stun self
                ApplyStun(defaultStunDuration);

            if (Input.GetKeyDown(KeyCode.Alpha3)) // press 3 to knockdown self
                ApplyKnockdown(defaultKnockdownDuration);

            if (Input.GetKeyDown(KeyCode.Alpha4)) // press 4 to air knockdown self
                ApplyKnockdown(defaultKnockdownDuration, true);

            if (Input.GetKeyDown(KeyCode.Alpha5))
                TakeDamage(0f, new Vector2(-10f, 5f), false, CrowdControlState.Stunned, 0.75f);
        }

        HandleArcKnockdown();
    }

    public void TakeDamage(float amount, Vector2? hitDirection = null, bool useRawForce = false,
     CrowdControlState forceCC = CrowdControlState.None, float forceCCDuration = 0f,
     bool triggerEffects = true, bool isDebuff = false, float knockbackMultiplier = 1f, Color? damageNumberColor = null)
    {
        if (isPlayer && invincible) return;

        currentHealth -= amount;
        updateUI?.Invoke(this, amount, damageNumberColor.HasValue ? damageNumberColor.Value : Color.white);
        if (triggerEffects) damageTaken?.Invoke(this);

        if (spriteRenderer != null && gameObject.activeInHierarchy)
            StartCoroutine(FlashRed());

        bool shouldPreserveVelocity = (forceCC == CrowdControlState.Knockdown && useRawForce);

        Debug.Log($"{gameObject.name} took {amount} damage. Current HP = {currentHealth}");

        // Apply knockback 
        if (rb != null && hitDirection.HasValue)
        {
            if (isPlayer)
            {
                PlayerController pc = GetComponent<PlayerController>();
                pc.ResetState();
                pc.externalVelocityOverride = false;
                pc.SetKnockback(hitDirection.Value, forceCCDuration);
                AudioManager.PlaySFX(SFXTYPE.PLAYER_HURT,0.3f);
            }
            else
            {
                rb.linearVelocity = Vector2.zero;
                Vector2 finalForce = hitDirection.Value * knockbackMult;
                if (useRawForce)
                    rb.AddForce(hitDirection.Value, ForceMode2D.Impulse);
                else
                    rb.AddForce(finalForce, ForceMode2D.Impulse);
            }
        }
        RaycastHit2D hit = Physics2D.Raycast(transform.position, Vector2.down, groundCheckValue, LayerMask.GetMask("Ground"));
        bool airborne = hit.collider == null;
        // Apply CC effects 
        if (!isDebuff)
        {
            if (forceCC != CrowdControlState.None)
            {
                if (forceCC == CrowdControlState.Stunned && !stunImmune)
                {
                    if (!airborne)
                        ApplyStun(forceCCDuration, hitDirection);
                    else ApplyKnockdown(forceCCDuration, airborne, hitDirection, shouldPreserveVelocity);
                }
                else if (forceCC == CrowdControlState.Knockdown)
                {
                    if (!knockdownImmune)
                    {
                        ApplyKnockdown(forceCCDuration, airborne, hitDirection, shouldPreserveVelocity);
                    }
                    else
                        ApplyStun(forceCCDuration, hitDirection);
                }
            }
        }

        // Death handling
        if (currentHealth <= 0)
        {
            //currentHealth = 0;
            if (isPlayer)
                StartCoroutine(RespawnPlayer());
            else
                Die();
        }
    }

    private void UpdateDebuffs()
    {
        if(debuffs.Count == 0) return;
        for (int i = 0; i < debuffs.Count; i++)
        {
            debuffs[i].UpdateTime(this, Time.time);
            if (debuffs[i].duration <= 0)
            {
                RemoveDebuff(debuffs[i]);
            }
        }

    }
    public void RemoveDebuff(DebuffInstance debuffInstance)
    {
        int index = debuffs.IndexOf(debuffInstance);
        debuffInstance.OnRemove(this);
        debuffs.Remove(debuffInstance);
        GameObject vfxObject = debuffVFXs[index];
        if (vfxObject != null)
        {
            debuffVFXs.RemoveAt(index);
            Destroy(vfxObject);
        }
    }

    public void Heal(float healAmount)
    {
        currentHealth += healAmount;
        currentHealth = Mathf.Min(currentHealth, maxHealth);
    }

    public float GetHealthPercentage() => currentHealth / maxHealth;
    public bool IsAlive() => currentHealth > 0f;

    private void Die()
    {
        Debug.Log($"{gameObject.name} has died!");
        enemyDeath?.Invoke(gameObject);

        if (animator != null)
            animator.SetTrigger("Die");

        for (int i = debuffs.Count - 1; i >= 0; i--)
            RemoveDebuff(debuffs[i]);

        if (!isPlayer && isBloodMarked)
        {
            var player = PlayerController.instance;
            if (player != null)
            {
                Health playerHealth = player.GetComponent<Health>();
                if (playerHealth != null)
                    playerHealth.Heal(bloodMarkHealAmount);
            }
        }

        if (bloodMarkIcon != null)
        {
            var sr = bloodMarkIcon.GetComponent<SpriteRenderer>();
            if (sr != null) sr.enabled = false;
            bloodMarkIcon.SetActive(false);
        }

        if (audioSource != null && deathSound != null)
            audioSource.PlayOneShot(deathSound);

        // wait for animation to finish
        if (destroyOnDeath)
            Destroy(gameObject, 1.5f);
    }


    private IEnumerator RespawnPlayer()
    {
        Debug.Log("=== PLAYER DEATH - Starting Respawn ===");

        // Disable player controls but keep controller for gravity simulation
        var controller = GetComponent<PlayerController>();
        bool wasGrounded = controller != null && controller.IsGrounded;

        if (controller != null)
        {
            controller.enabled = false;
        }

        // Trigger death animation
        if (animator != null)
        {
            Debug.Log("Triggering Die animation...");

            // Check if parameter exists
            bool hasDieParam = false;
            foreach (var param in animator.parameters)
            {
                if (param.name == "Die" && param.type == AnimatorControllerParameterType.Trigger)
                {
                    hasDieParam = true;
                    break;
                }
            }

            if (hasDieParam)
            {
                animator.SetTrigger("Die");

                float deathAnimTime = 1.5f; 
                float elapsedTime = 0f;

                if (!wasGrounded)
                {
                    Vector2 fallVelocity = Vector2.zero;
                    float deathGravity = -20f; 

                    while (elapsedTime < deathAnimTime)
                    {
                        fallVelocity.y += deathGravity * Time.deltaTime;

                        Vector2 moveAmount = fallVelocity * Time.deltaTime;
                        transform.position += (Vector3)moveAmount;
                        if (controller != null && controller.groundCheck != null)
                        {
                            RaycastHit2D groundHit = Physics2D.Raycast(
                                controller.groundCheck.position,
                                Vector2.down,
                                controller.groundCheckRadius,
                                controller.groundLayer | controller.platformLayer
                            );

                            if (groundHit.collider != null)
                            {
                                fallVelocity.y = 0f;
                            }
                        }

                        elapsedTime += Time.deltaTime;
                        yield return null;
                    }
                }
                else
                {
                    yield return new WaitForSeconds(deathAnimTime);
                }
            }
            else
            {
                yield return new WaitForSeconds(1f);
            }
        }
        else
        {
            yield return new WaitForSeconds(1f);
        }

        // Move player to last checkpoint
        if (CheckpointManager.Instance != null)
            transform.position = CheckpointManager.Instance.GetSpawnPoint();
        else
            transform.position = Vector3.zero;

        // Reset stats
        currentHealth = maxHealth;
        var energy = GetComponent<EnergySystem>();
        if (energy != null) energy.ResetEnergy();

        // Reset animator state
        if (animator != null)
        {
            animator.Rebind();
            animator.Update(0f);
        }

        // Reset physics
        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
        }

        // Re-enable controls
        if (controller != null)
        {
            controller.enabled = true;
            controller.SetVelocity(Vector2.zero);
        }

        Camera mainCam = Camera.main;
        if (mainCam != null)
        {
            var follow = mainCam.GetComponent<CameraFollow>();
            if (follow != null) follow.target = transform;
        }

        Debug.Log("Player respawned at checkpoint!");
    }

    private IEnumerator FlashRed()
    {
        spriteRenderer.color = Color.red;
        yield return new WaitForSeconds(flashDuration);
        spriteRenderer.color = originalColor;
    }

    public void ApplyBloodMark()
    {
        if (!isPlayer && !isBloodMarked)
        {
            isBloodMarked = true;

            if (bloodMarkIcon != null)
            {
                bloodMarkIcon.SetActive(true);

                var sr = bloodMarkIcon.GetComponent<SpriteRenderer>();
                if (sr != null) sr.enabled = true;
            }
        }
    }

    public void ApplyStun(float duration, Vector2? hitDirection = null, float knockbackMultiplier = 1f)
    {
        if(currentCCState == CrowdControlState.None) currentCCState = CrowdControlState.Stunned;
        ccTimer = duration;
    }

    //public void ApplyStun(Vector2? hitDirection = null)
    //{
    //    ApplyStun(defaultStunDuration, hitDirection, 1f);
    //}

    public void ApplyKnockdown(float duration, bool isAirborne = false,
     Vector2? hitDirection = null, bool preserveVelocity = false, float knockbackMultiplier = 1f)
    {
        currentCCState = CrowdControlState.Knockdown;
        ccTimer = duration;

        if (isPlayer)
        {
            var pc = GetComponent<PlayerController>();
            if (pc != null)
            {
                Debug.Log("arc knockdown");
                pc.SetVelocity(Vector2.zero);
                float xDir = pc.facingRight ? -1f : 1f;
                Vector2 arcKnockback = new Vector2(xDir * 10f, 8f) * knockbackMult;
                pc.SetVelocity(arcKnockback);
                isInArcKnockdown = true;
                pc.externalVelocityOverride = true;
            }
            StartCoroutine(PlayerKnockdownInvincibility(duration));
        }
        else
        {
            if (rb != null && !isAirborne)
            {
                // Override knockback if no upward knockback detected
                if (!hitDirection.HasValue)
                {
                    rb.linearVelocity = Vector2.zero;
                    float xDir = (transform.position.x > PlayerController.instance.transform.position.x) ? 1f : -1f;
                    Vector2 arcKnockback = new Vector2(xDir * 8f, 6f) * knockbackMult;
                    rb.AddForce(arcKnockback, ForceMode2D.Impulse);
                }
                else if(hitDirection.Value.y <= 0f)
                {
                    rb.linearVelocity = Vector2.zero;
                    float xDir = (transform.position.x > PlayerController.instance.transform.position.x) ? 1f : -1f;
                    Vector2 arcKnockback = new Vector2(xDir * 8f, 6f) * knockbackMult;
                    rb.AddForce(arcKnockback, ForceMode2D.Impulse);
                }
                    //float xDir = -1f;
                    //if (hitDirection.HasValue)
                    //    xDir = hitDirection.Value.x > 0 ? 1f : -1f;
                    //else if (PlayerController.instance != null)
                    //    xDir = (transform.position.x > PlayerController.instance.transform.position.x) ? 1f : -1f;

                    //// Scale arc knockdown push by multiplier
                    //Vector2 arcKnockback = new Vector2(xDir * 8f, 6f) * knockbackMultiplier;
                    //rb.AddForce(arcKnockback, ForceMode2D.Impulse);
                    isInArcKnockdown = true;
            }
            else
            {
                isInArcKnockdown = true;
                Debug.Log($"{gameObject.name} knockdown with preserved velocity (sweep attack)");
            }

            if (isAirborne)
                Debug.Log($"{gameObject.name} launched into air knockdown (can be juggled)");
        }

        Debug.Log($"{gameObject.name} knockdown for {duration} sec (KBx{knockbackMultiplier})");
    }

    //public void ApplyKnockdown(bool isAirborne = false, Vector2? hitDirection = null)
    //{
    //    ApplyKnockdown(defaultKnockdownDuration, isAirborne, hitDirection, false, 1f);
    //}

    private void HandleArcKnockdown()
    {
        if (!isInArcKnockdown) return;

        if (isPlayer)
        {
            var pc = PlayerController.instance;
            if (pc != null)
            {
                Vector2 currentVel = pc.GetVelocity();
                currentVel.y -= arcKnockdownGravity * Time.deltaTime;
                pc.SetKnockback(currentVel, pc.knockbackTimer);
            }
        }
        else
        {
            // Enemy arc knockdown
            if (rb != null)
            {
                // Apply custom gravity
                Vector2 currentVel = rb.linearVelocity;
                currentVel.y -= arcKnockdownGravity * Time.deltaTime;
                rb.linearVelocity = currentVel;

            }
        }
    }


    private IEnumerator PlayerKnockdownInvincibility(float duration)
    {
        invincible = true;
        yield return new WaitForSeconds(duration);
        invincible = false;


        if (isInArcKnockdown)
        {
            isInArcKnockdown = false;
            var pc = PlayerController.instance;
            if (pc != null)
                pc.externalVelocityOverride = false;
        }
    }

    // ==========================
    // HOW TO CALL CC FUNCTIONS
    // ==========================
    //
    // ApplyStun(duration, hitDirection)
    //   - Puts target into Stun state for <duration> seconds
    //   - Example: targetHealth.ApplyStun(1.5f, knockDir);
    //
    // ApplyStun(hitDirection) - NEW OVERLOAD
    //   - Uses defaultStunDuration from inspector
    //   - Example: targetHealth.ApplyStun(knockDir);
    //
    // ApplyKnockdown(duration, isAirborne, hitDirection)
    //   - Puts target into Knockdown state for <duration> seconds
    //   - If isAirborne = true then enemy can be juggled in the air
    //   - Example: targetHealth.ApplyKnockdown(2.0f, true, knockDir);
    //
    // ApplyKnockdown(isAirborne, hitDirection) - NEW OVERLOAD
    //   - Uses defaultKnockdownDuration from inspector
    //   - Example: targetHealth.ApplyKnockdown(true, knockDir);
    //
    // ApplySkillCC(health, knockDir, groundedCC, airborneCC, ccDuration)
    //   - Unified helper to apply Stun/Knockdown depending on target state
    //   - health = target's Health component
    //   - knockDir = knockback direction
    //   - groundedCC = what to apply if enemy is on the ground (Stunned / None)
    //   - airborneCC = what to apply if enemy is in the air (Knockdown / None)
    //   - ccDuration = how long the effect lasts (use 0 to use default durations)
    //   - Example: ApplySkillCC(h, knockDir, CrowdControlState.Stunned, CrowdControlState.Knockdown, 2.0f);
    //
    // ==========================


}