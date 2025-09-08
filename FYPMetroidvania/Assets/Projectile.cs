using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Projectile : MonoBehaviour
{
    public float damage = 5f;
    public float lifeTime = 3f;

    private void Start()
    {
        Destroy(gameObject, lifeTime);
    }


    private void OnTriggerEnter2D(Collider2D other)
    {
        Health enemyHealth = other.GetComponent<Health>();
        if (enemyHealth != null)
        {
            enemyHealth.TakeDamage(damage);
            Destroy(gameObject);
        }
    }
}
