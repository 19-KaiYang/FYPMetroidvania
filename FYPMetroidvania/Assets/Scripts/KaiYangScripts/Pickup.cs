using UnityEngine;

public class Pickup : MonoBehaviour
{
    [Header("Pickup Settings")]
    public bool isHealthPickup = true;
    public float healAmount = 25f;
    public float spiritAmount = 30f;

    [Header("Visual")]
    public float floatSpeed = 2f;
    public float floatHeight = 0.2f;

    [Header("Particle Effects")]
    public GameObject healthParticlePrefab;
    public GameObject spiritParticlePrefab;

    private Vector3 startPos;
    private Transform player;

    private void Start()
    {
        startPos = transform.position;
        if (PlayerController.instance != null)
            player = PlayerController.instance.transform;
    }

    private void Update()
    {
        // Float animation
        float newY = startPos.y + Mathf.Sin(Time.time * floatSpeed) * floatHeight;
        transform.position = new Vector3(transform.position.x, newY, transform.position.z);

        // Pull towards player when close
        if (player != null && Vector2.Distance(transform.position, player.position) < 2f)
        {
            transform.position = Vector3.MoveTowards(transform.position, player.position, 5f * Time.deltaTime);
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            if (isHealthPickup)
            {
                Health health = collision.GetComponent<Health>();
                if (health != null)
                {
                    health.Heal(healAmount);
                    Debug.Log($"Healed {healAmount} HP");

                    if (healthParticlePrefab != null)
                    {
                        GameObject fx = Instantiate(healthParticlePrefab, collision.transform.position, Quaternion.identity);
                        Destroy(fx, 2f);
                    }
                }
            }
            else
            {
                SpiritGauge spirit = collision.GetComponent<SpiritGauge>();
                if (spirit != null)
                {
                    spirit.Refill(spiritAmount);
                    Debug.Log($"Restored {spiritAmount} Spirit");

                    if (spiritParticlePrefab != null)
                    {
                        GameObject fx = Instantiate(spiritParticlePrefab, collision.transform.position, Quaternion.identity);
                        Destroy(fx, 2f);
                    }
                }
            }

            Destroy(gameObject);
        }
    }
}