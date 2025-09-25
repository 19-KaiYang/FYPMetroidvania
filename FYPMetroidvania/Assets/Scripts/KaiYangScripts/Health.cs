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

    [Header("Knockback")]
    public float knockbackForce = 5f;

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
        if (isPlayer && Input.GetKeyDown(KeyCode.K))
        {
            TestDamage();
        }

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


    public void TakeDamage(float amount, Vector2? hitDirection = null, bool useRawForce = false)
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

        bool grounded = false;
        if (isPlayer)
        {
            grounded = PlayerController.instance != null && PlayerController.instance.IsGrounded;
        }
        else
        {
            // Simple enemy grounded check using velocity
            grounded = Mathf.Abs(rb.linearVelocity.y) < 0.1f;

        }

     
        if (!isPlayer)
        {
            if (grounded)
                ApplyStun(1.0f);
            else
                ApplyKnockdown(2.0f, true);
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

        // Re-link camera follow if needed
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

    public void ApplyStun(float duration)
    {
        currentCCState = CrowdControlState.Stunned;
        ccTimer = duration;
        Debug.Log($"{gameObject.name} stunned for {duration} sec");
    }

    public void ApplyKnockdown(float duration, bool isAirborne = false)
    {
        currentCCState = CrowdControlState.Knockdown;
        ccTimer = duration;

        if (isPlayer)
        {
            var pc = GetComponent<PlayerController>();
            if (pc != null)
            {
                // Clear old velocity
                pc.SetVelocity(Vector2.zero);

                // Determine knockback direction
                float xDir = pc.facingRight ? -1f : 1f;

                // Arc knockback with higher horizontal speed, moderate vertical
                Vector2 arcKnockback = new Vector2(xDir * 10f, 8f); // More horizontal, less vertical
                pc.SetVelocity(arcKnockback);

                // Enable arc knockdown mode
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

                // Determine knockback direction away from player
                float xDir = -1f; // default left
                if (PlayerController.instance != null)
                {
                    Vector2 playerPos = PlayerController.instance.transform.position;
                    Vector2 enemyPos = transform.position;
                    xDir = (enemyPos.x > playerPos.x) ? 1f : -1f; // knock away from player
                }

                // Arc knockback for enemies
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
                // Apply custom gravity for smoother arc
                Vector2 currentVel = pc.GetVelocity();
                currentVel.y -= arcKnockdownGravity * Time.deltaTime;
                pc.SetVelocity(currentVel);

                // End arc knockdown when landed
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

    public void TestDamage()
    {
        TakeDamage(10f,Vector2.left);

        if (isPlayer)
        {
            bool grounded = PlayerController.instance != null && PlayerController.instance.IsGrounded;
            if (grounded)
                ApplyStun(1.0f);
            else
                ApplyKnockdown(2.0f, true);
        }
        else
        {
            bool grounded = Mathf.Abs(rb.linearVelocity.y) < 0.1f;
            if (grounded)
                ApplyStun(1.0f);
            else
                ApplyKnockdown(2.0f, true);
        }
    }


}
