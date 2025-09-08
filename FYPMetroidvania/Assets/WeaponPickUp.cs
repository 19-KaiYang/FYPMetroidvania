using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponPickUp : MonoBehaviour
{
    public WeaponType weaponType;

    private void OnTriggerEnter2D(Collider2D other)
    {
        CombatSystem combat = other.GetComponent<CombatSystem>();
        if (combat != null)
        {
            combat.UnlockWeapon(weaponType);
            Debug.Log($"{weaponType} picked up!");
            Destroy(gameObject); 
        }
    }
}
