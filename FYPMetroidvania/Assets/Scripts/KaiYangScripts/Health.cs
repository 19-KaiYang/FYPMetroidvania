using System.Collections;
using UnityEngine;

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

    [Header("Knockback")]
    public float knockbackForce = 5f;

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

    public void TakeDamage(float amount, Vector2? hitDirection = null)
    {
      


        currentHealth -= amount;

        // Visual feedback
        if (spriteRenderer != null)
            StartCoroutine(FlashRed());

        // Knockback
        if (rb != null && hitDirection.HasValue)
            rb.AddForce(hitDirection.Value.normalized * knockbackForce, ForceMode2D.Impulse);

        if (currentHealth <= 0)
        {
            currentHealth = 0;

            if (isPlayer)
            {
                StartCoroutine(RespawnPlayer()); 
            }
            else
            {
                Die(); 
            }
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





}
