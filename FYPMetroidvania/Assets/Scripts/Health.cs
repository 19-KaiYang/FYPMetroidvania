using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Health : MonoBehaviour
{
    [Header("Health Settings")]
    public float maxHealth = 100f;
    public bool destroyOnDeath = true;
    private float currentHealth;

    [Header("Feedback")]
    public SpriteRenderer spriteRenderer;
    public float flashDuration = 0.1f;
    public AudioClip hitSound;
    public AudioClip deathSound;

    [Header("Knockback")]
    public float knockbackForce = 5f;

  

    private AudioSource audioSource;
    private Rigidbody2D rb;

    private void Awake()
    {
        currentHealth = maxHealth;
        rb = GetComponent<Rigidbody2D>();
        audioSource = GetComponent<AudioSource>();
        if (spriteRenderer == null)
            spriteRenderer = GetComponent<SpriteRenderer>();
    }

    public void TakeDamage(float damage, Vector2? hitDirection = null)
    {
        if (currentHealth <= 0f) return; // already dead

        currentHealth -= damage;
        currentHealth = Mathf.Max(currentHealth, 0f);

        // Visual feedback
        if (spriteRenderer != null)
            StartCoroutine(FlashRed());

        // Knockback
        if (rb != null && hitDirection.HasValue)
            rb.AddForce(hitDirection.Value.normalized * knockbackForce, ForceMode2D.Impulse);

        Debug.Log($"{gameObject.name} took {damage} damage. Health: {currentHealth}/{maxHealth}");

        if (currentHealth <= 0f)
            Die();
    }

    public void Heal(float healAmount)
    {
        currentHealth += healAmount;
        currentHealth = Mathf.Min(currentHealth, maxHealth);
        Debug.Log($"{gameObject.name} healed for {healAmount}. Health: {currentHealth}/{maxHealth}");
    }

    public float GetHealthPercentage() => currentHealth / maxHealth;
    public bool IsAlive() => currentHealth > 0f;

    private void Die()
    {
        Debug.Log($"{gameObject.name} has died!");

        if (audioSource != null && deathSound != null)
            audioSource.PlayOneShot(deathSound);

      

        if (destroyOnDeath)
            Destroy(gameObject);
        else
            gameObject.SetActive(false);
    }

    private IEnumerator FlashRed()
    {
        Color originalColor = spriteRenderer.color;
        spriteRenderer.color = Color.red;
        yield return new WaitForSeconds(flashDuration);
        spriteRenderer.color = originalColor;
    }
}