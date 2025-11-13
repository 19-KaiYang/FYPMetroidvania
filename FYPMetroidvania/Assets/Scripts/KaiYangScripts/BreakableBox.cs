using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BreakableBox : MonoBehaviour
{
    [Header("What This Box Drops")]
    public List<GameObject> itemsInside = new List<GameObject>();

    [Header("Settings")]
    public float popForce = 3f;
    public float spreadRadius = 0.5f;

    [Header("Effects")]
    public GameObject breakParticles; 

    private void OnTriggerEnter2D(Collider2D collision)
    {
        Hitbox hitbox = collision.GetComponent<Hitbox>();
        if (hitbox != null)
        {
            AudioManager.PlaySFX(SFXTYPE.BARREL_BREAK, 0.5f);
            Break();
        }

        if (collision.CompareTag("PlayerProjectile"))
        {
            AudioManager.PlaySFX(SFXTYPE.BARREL_BREAK, 0.5f);
            Break();
        }
    }

    private void Break()
    {
        // Spawn particle effect
        if (breakParticles != null)
        {
            Instantiate(breakParticles, transform.position, Quaternion.identity);
        }

        foreach (GameObject item in itemsInside)
        {
            if (item != null)
            {
                Vector2 randomOffset = Random.insideUnitCircle * spreadRadius;
                Vector3 spawnPos = transform.position + new Vector3(randomOffset.x, 0.2f + randomOffset.y, 0f);

                GameObject drop = Instantiate(item, spawnPos, Quaternion.identity);
                Rigidbody2D rb = drop.GetComponent<Rigidbody2D>();
                if (rb != null)
                {
                    Vector2 popDirection = new Vector2(Random.Range(-0.5f, 0.5f), 1f).normalized;
                    rb.AddForce(popDirection * popForce, ForceMode2D.Impulse);
                    StartCoroutine(DisablePhysics(rb));
                }
            }
        }

        Destroy(gameObject);
    }

    private IEnumerator DisablePhysics(Rigidbody2D rb)
    {
        yield return new WaitForSeconds(0.5f);
        if (rb != null)
        {
            rb.bodyType = RigidbodyType2D.Kinematic;
            rb.linearVelocity = Vector2.zero;
        }
    }
}