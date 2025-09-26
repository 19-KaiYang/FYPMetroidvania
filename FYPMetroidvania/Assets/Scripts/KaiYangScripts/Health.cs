using System.Collections;
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
    private float currentHealth;

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

        // === CC TIMERS ===
        if (currentCCState != CrowdControlState.None)
        {
            ccTimer -= Time.deltaTime;

            if (currentCCState == CrowdControlState.Knockdown)
            {
                bool landed = false;

                if (isPlayer)
                {
                    landed = PlayerController.instance != null && PlayerController.instance.IsGrounded;
                }
                else
                {
                    RaycastHit2D hit = Physics2D.Raycast(transform.position, Vector2.down, 0.1f, LayerMask.GetMask("Ground"));
                    landed = hit.collider != null;
                }

                if (landed)
                {
                    currentCCState = CrowdControlState.None;
                    Debug.Log($"{gameObject.name} landed from knockdown");
                }
            }

            // Timer expiration fallback
            if (ccTimer <= 0f)
            {
                currentCCState = CrowdControlState.None;
                Debug.Log($"{gameObject.name} recovered from CC state");
            }
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

        // Knockback
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
                    ApplyStun(forceCCDuration > 0 ? forceCCDuration : 1.0f);
                else if (forceCC == CrowdControlState.Knockdown)
                    ApplyKnockdown(forceCCDuration > 0 ? forceCCDuration : 2.0f, true, hitDirection);
            }
            else
            {
                // Default behavior based on grounded state
                RaycastHit2D hit = Physics2D.Raycast(transform.position, Vector2.down, groundCheckValue, LayerMask.GetMask("Ground"));
                bool grounded = hit.collider != null;

                if (grounded)
                    ApplyStun(1.0f, hitDirection);
                else
                    ApplyKnockdown(2.0f, true, hitDirection);
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

    public void ApplyKnockdown(float duration, bool isAirborne = false, Vector2? hitDirection = null)
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
            if (rb != null)
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
                    // Fallback: calculate from positions
                    Vector2 playerPos = PlayerController.instance.transform.position;
                    Vector2 enemyPos = transform.position;
                    xDir = (enemyPos.x > playerPos.x) ? 1f : -1f;
                }

                Vector2 arcKnockback = new Vector2(xDir * 8f, 6f);
                rb.AddForce(arcKnockback, ForceMode2D.Impulse);
                isInArcKnockdown = true;
            }

            if (isAirborne)
                Debug.Log($"{gameObject.name} launched into air knockdown (can be juggled)");
        }

        Debug.Log($"{gameObject.name} knockdown for {duration} sec");
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


                if (pc.IsGrounded)
                {
                    isInArcKnockdown = false;
                    pc.externalVelocityOverride = false;
                    currentCCState = CrowdControlState.None;
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

                // Check if landed
                RaycastHit2D hit = Physics2D.Raycast(transform.position, Vector2.down, 0.1f, LayerMask.GetMask("Ground"));
                if (hit.collider != null)
                {
                    isInArcKnockdown = false;
                    currentCCState = CrowdControlState.None;
                }
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
    // ApplyKnockdown(duration, isAirborne, hitDirection)
    //   - Puts target into Knockdown state for <duration> seconds
    //   - If isAirborne = true then enemy can be juggled in the air
    //   - Example: targetHealth.ApplyKnockdown(2.0f, true, knockDir);
    //
    // ApplySkillCC(health, knockDir, groundedCC, airborneCC, ccDuration)
    //   - Unified helper to apply Stun/Knockdown depending on target state
    //   - health = target's Health component
    //   - knockDir = knockback direction
    //   - groundedCC = what to apply if enemy is on the ground (Stunned / None)
    //   - airborneCC = what to apply if enemy is in the air (Knockdown / None)
    //   - ccDuration = how long the effect lasts
    //   - Example: ApplySkillCC(h, knockDir, CrowdControlState.Stunned, CrowdControlState.Knockdown, 2.0f);
    //
    // ==========================


}
