using System.Collections;
using UnityEngine;

public class HealingSpot : MonoBehaviour
{
    [Header("Healing Settings")]
    public float healPerSecond = 5f;   
    public float tickInterval = 0.5f;  

    private void OnTriggerEnter2D(Collider2D other)
    {
        Health h = other.GetComponent<Health>();
        if (h != null && h.IsAlive())
        {
            
            StartCoroutine(HealOverTime(h));
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
       
        StopAllCoroutines();
    }

    private IEnumerator HealOverTime(Health target)
    {
        while (target != null && target.IsAlive() &&
               target.GetComponent<Collider2D>().IsTouching(GetComponent<Collider2D>()))
        {
            target.Heal(healPerSecond * tickInterval);
            yield return new WaitForSeconds(tickInterval);
        }
    }
}
