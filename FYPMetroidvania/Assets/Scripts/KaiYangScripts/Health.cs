using NUnit.Framework;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
    public float flashDuration = 0.1f;
    public AudioClip hitSound;
    public AudioClip deathSound;

    [Header("Arc Knockdown")]
    public float arcKnockdownGravity = 15f;
    public bool isInArcKnockdown = false;

    [Header("Default Knockback")]
    public float knockbackForce = 5f;
    public float stunPushbackForce = 5f;
    public float groundCheckValue = 1f;

    [Header("Crowd Control Durations")]
    [Tooltip("Default stun duration when grounded")]
    public float defaultStunDuration = 1.0f;
    [Tooltip("Default knockdown duration when airborne")]
    public float defaultKnockdownDuration = 2.0f;
    [Tooltip("Recovery time after landing from knockdown")]
    public float knockdownRecoveryTime = 1.0f;

    [Header("Combat States")]
    public CrowdControlState currentCCState = CrowdControlState.None;

    private float ccTimer = 0f;

    public float invincibilityDuration = 0.3f; // player only

    private bool invincible = false;

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

    private AudioSource audioSource;
    private Rigidbody2D rb;

    public float CurrentHealth => currentHealth;


    private void Awake()
    {
        currentHealth = maxHealth;
        rb = GetComponent<Rigidbody2D>();
        audioSource = GetComponent<AudioSource>();
        if (spriteRenderer == null)
            spriteRenderer = GetComponent<SpriteRenderer>();
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

                    Debug.Log($"{gameObject.name} - TouchingGround: {touchingGround}, StoppedFalling: {stoppedFalling}, Y velocity: {(rb ? rb.linearVelocity.y : 0f)}");
                }


                // if in air knockdown, don't count down timer 
                if (isInArcKnockdown)
                {
                    if (landed)
                    {
                        ccTimer = knockdownRecoveryTime;
                        isInArcKnockdown = false;
                        Debug.Log($"{gameObject.name} landed from air knockdown, starting recovery timer ({knockdownRecoveryTime}s)");
                    }
                    else
                    {
                        Debug.Log($"{gameObject.name} still in air knockdown");
                    }
                   
                    return;
                }

                // Regular knockdown timer countdown (only when grounded)
                if (ccTimer > 0f)
                {
                    ccTimer -= Time.deltaTime;
                    Debug.Log($"{gameObject.name} knockdown recovery timer: {ccTimer:F2}s remaining");
                    if (ccTimer <= 0f)
                    {
                        currentCCState = CrowdControlState.None;
                        Debug.Log($"{gameObject.name} recovered from knockdown - can move again!");
                    }
                }
            }
            else
            {
                // For stun and other CC states, count down normally
                if (ccTimer > 0f)
                {
                    ccTimer -= Time.deltaTime;
                    if (ccTimer <= 0f)
                    {
                        currentCCState = CrowdControlState.None;
                        Debug.Log($"{gameObject.name} recovered from CC state");
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
        }

        HandleArcKnockdown();
    }

    public void TakeDamage(float amount, Vector2? hitDirection = null, bool useRawForce = false, CrowdControlState forceCC = CrowdControlState.None, float forceCCDuration = 0f)
    {
        if (isPlayer && invincible) return;

        currentHealth -= amount;

        // Visual feedback
        if (spriteRenderer != null)
            StartCoroutine(FlashRed());

        bool shouldPreserveVelocity = (forceCC == CrowdControlState.Knockdown && useRawForce);

        if (rb != null && hitDirection.HasValue)
        {
            if (useRawForce)
                rb.AddForce(hitDirection.Value, ForceMode2D.Impulse);
            else
                rb.AddForce(hitDirection.Value.normalized * knockbackForce, ForceMode2D.Impulse);
        }

        if (!isPlayer)
        {
            if (forceCC != CrowdControlState.None)
            {
                // Skill specified exact CC type
                if (forceCC == CrowdControlState.Stunned)
                    ApplyStun(forceCCDuration > 0 ? forceCCDuration : defaultStunDuration);
                else if (forceCC == CrowdControlState.Knockdown)
                    ApplyKnockdown(forceCCDuration > 0 ? forceCCDuration : defaultKnockdownDuration, true, hitDirection, shouldPreserveVelocity);
            }
            else
            {
                // Default behavior based on grounded state
                RaycastHit2D hit = Physics2D.Raycast(transform.position, Vector2.down, groundCheckValue, LayerMask.GetMask("Ground"));
                bool grounded = hit.collider != null;

                if (grounded)
                    ApplyStun(defaultStunDuration, hitDirection);
                else
                    ApplyKnockdown(defaultKnockdownDuration, true, hitDirection);
            }
        }

        // Death handling
        if (currentHealth <= 0)
        {
            currentHealth = 0;
            if (isPlayer)
                StartCoroutine(RespawnPlayer());
            else
                Die();
        }

        Debug.Log($"{gameObject.name} took {amount} damage! Remaining HP: {currentHealth}/{maxHealth}");
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

        if (!isPlayer) enemyDeath?.Invoke(this.gameObject);

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

        if (destroyOnDeath)
            Destroy(gameObject);
        else
            gameObject.SetActive(false);
    }




    private IEnumerator RespawnPlayer()
    {
        // Disable player controls
        var controller = GetComponent<PlayerController>();
        if (controller != null) controller.enabled = false;

        yield return new WaitForSeconds(1f);

        // Move player to last checkpoint
        if (CheckpointManager.Instance != null)
            transform.position = CheckpointManager.Instance.GetSpawnPoint();
        else
            transform.position = Vector3.zero;

        // Reset stats
        currentHealth = maxHealth;
        var energy = GetComponent<EnergySystem>();
        if (energy != null) energy.ResetEnergy();

        // Re-enable controls
        if (controller != null) controller.enabled = true;


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
        Color originalColor = spriteRenderer.color;
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

    public void ApplyStun(float duration, Vector2? hitDirection = null)
    {
        currentCCState = CrowdControlState.Stunned;
        ccTimer = duration;

        // Apply pushback for stunned enemies
        if (!isPlayer && rb != null && hitDirection.HasValue)
        {
            rb.linearVelocity = Vector2.zero;

            Vector2 pushDirection = new Vector2(hitDirection.Value.x, 0f).normalized;
            rb.AddForce(pushDirection * stunPushbackForce, ForceMode2D.Impulse);
        }

        Debug.Log($"{gameObject.name} stunned for {duration} sec");
    }
    public void ApplyStun(Vector2? hitDirection = null)
    {
        ApplyStun(defaultStunDuration, hitDirection);
    }

    public void ApplyKnockdown(float duration, bool isAirborne = false, Vector2? hitDirection = null, bool preserveVelocity = false)
    {
        currentCCState = CrowdControlState.Knockdown;
        ccTimer = duration;

        if (isPlayer)
        {
            var pc = GetComponent<PlayerController>();
            if (pc != null)
            {
                pc.SetVelocity(Vector2.zero);
                float xDir = pc.facingRight ? -1f : 1f;
                Vector2 arcKnockback = new Vector2(xDir * 10f, 8f);
                pc.SetVelocity(arcKnockback);
                isInArcKnockdown = true;
                pc.externalVelocityOverride = true;
            }
            StartCoroutine(PlayerKnockdownInvincibility(duration));
        }
        else
        {
            if (rb != null && !preserveVelocity)
            {
                rb.linearVelocity = Vector2.zero;

                float xDir = -1f;
                if (hitDirection.HasValue)
                {
                    // Use the actual hit direction from the attack
                    xDir = hitDirection.Value.x > 0 ? 1f : -1f;
                }
                else if (PlayerController.instance != null)
                {
                    Vector2 playerPos = PlayerController.instance.transform.position;
                    Vector2 enemyPos = transform.position;
                    xDir = (enemyPos.x > playerPos.x) ? 1f : -1f;
                }

                Vector2 arcKnockback = new Vector2(xDir * 8f, 6f);
                rb.AddForce(arcKnockback, ForceMode2D.Impulse);
                isInArcKnockdown = true;
            }
            else if (preserveVelocity)
            {
                isInArcKnockdown = true;
                Debug.Log($"{gameObject.name} knockdown with preserved velocity (sweep attack)");
            }

            if (isAirborne)
                Debug.Log($"{gameObject.name} launched into air knockdown (can be juggled)");
        }

        Debug.Log($"{gameObject.name} knockdown for {duration} sec");
    }

    public void ApplyKnockdown(bool isAirborne = false, Vector2? hitDirection = null)
    {
        ApplyKnockdown(defaultKnockdownDuration, isAirborne, hitDirection, false);
    }

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
                pc.SetVelocity(currentVel);

                if (pc.IsGrounded && isInArcKnockdown)
                {
                    pc.externalVelocityOverride = false;
                }
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