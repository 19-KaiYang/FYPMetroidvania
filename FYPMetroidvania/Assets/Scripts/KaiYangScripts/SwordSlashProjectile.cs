using UnityEngine;

public class SwordSlashProjectile : MonoBehaviour
{
    public float damage = 20f;
    public float maxDistance = 10f;
    public float bloodCost = 5f;

    private Vector3 startPos;
    private Health playerHealth;

    void Start()
    {
        startPos = transform.position;
        playerHealth = PlayerController.instance.GetComponent<Health>();
    }

    void Update()
    {
        if (Vector3.Distance(startPos, transform.position) >= maxDistance)
            Destroy(gameObject);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        Health enemy = collision.GetComponent<Health>();
        if (enemy != null && !enemy.isPlayer)
        {
            enemy.TakeDamage(damage);

            // apply BloodMark
            enemy.ApplyBloodMark();

            //  apply blood cost (only if hit connects)
            if (playerHealth != null && bloodCost > 0f)
            {
                float safeCost = Mathf.Min(bloodCost, playerHealth.CurrentHealth - 1f);
                if (safeCost > 0f)
                    playerHealth.TakeDamage(safeCost);
            }

            Destroy(gameObject); 
        }
    }
}
