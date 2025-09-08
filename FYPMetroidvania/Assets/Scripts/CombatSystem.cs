using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public enum WeaponType
{
    None,
    Sword,
    Tome,
    Gauntlet
}

[System.Serializable]
public class WeaponStats
{
    public WeaponType type;
    public float damage;
    public float range;
    public float attackCooldown;

    [Header("Ranged Only")]
    public List<GameObject> projectilePrefabs;
    public float projectileSpeed = 10f;
    public float projectileLifetime = 3f;
}

public class CombatSystem : MonoBehaviour
{
    [Header("Weapon Settings")]
    public WeaponType currentWeapon;
    public List<WeaponStats> weaponStatsList = new List<WeaponStats>();

    [Header("Attack Points (for projectiles)")]
    public Transform attackPointRight;
    public Transform attackPointLeft;

    [Header("Melee Hitboxes")]
    public List<GameObject> swordHitboxes;
    public List<GameObject> gauntletHitboxes;

    private List<GameObject> activeHitboxes;

    private Animator animator;
    private PlayerController controller;

    // Cached stats
    private float attackDamage;
    private float attackRange;
    private float attackCooldown;

    // Combat variables
    private int comboStep = 0;
    private float comboTimer = 0f;
    private float attackCooldownTimer = 0f;

    // Weapon unlocks
    private HashSet<WeaponType> unlockedWeapons = new HashSet<WeaponType>();

    public int CurrentComboStep => comboStep;

    private void Awake()
    {
        animator = GetComponent<Animator>();
        controller = GetComponent<PlayerController>();

        currentWeapon = WeaponType.None;
        ApplyWeaponStats(currentWeapon);
    }

    private void Update()
    {
        if (attackCooldownTimer > 0f)
            attackCooldownTimer -= Time.deltaTime;

        if (comboStep > 0)
        {
            comboTimer -= Time.deltaTime;
            if (comboTimer <= 0f)
                ResetCombo();
        }

        if (Keyboard.current.digit1Key.wasPressedThisFrame) SetWeapon(WeaponType.Sword);
        if (Keyboard.current.digit2Key.wasPressedThisFrame) SetWeapon(WeaponType.Tome);
        if (Keyboard.current.digit3Key.wasPressedThisFrame) SetWeapon(WeaponType.Gauntlet);
    }

    public void UnlockWeapon(WeaponType weapon)
    {
        if (!unlockedWeapons.Contains(weapon))
        {
            unlockedWeapons.Add(weapon);
            Debug.Log($"{weapon} unlocked!");

            if (currentWeapon == WeaponType.None)
                SetWeapon(weapon);
        }
    }

    public void SetWeapon(WeaponType newWeapon)
    {
        currentWeapon = newWeapon;
        ApplyWeaponStats(newWeapon);

        if (newWeapon == WeaponType.Sword)
            activeHitboxes = swordHitboxes;
        else if (newWeapon == WeaponType.Gauntlet)
            activeHitboxes = gauntletHitboxes;
        else
            activeHitboxes = null;

        Debug.Log($"Equipped {newWeapon}");
    }

    private void ApplyWeaponStats(WeaponType type)
    {
        if (type == WeaponType.None)
        {
            attackDamage = 0f;
            attackRange = 0f;
            attackCooldown = 0.5f;
            return;
        }

        WeaponStats stats = weaponStatsList.Find(w => w.type == type);
        if (stats != null)
        {
            attackDamage = stats.damage;
            attackRange = stats.range;
            attackCooldown = stats.attackCooldown;
        }
    }

    public void OnAttack()
    {
        if (currentWeapon == WeaponType.None) return;
        if (attackCooldownTimer <= 0f) PerformAttack();
    }

    private void PerformAttack()
    {
        comboStep++;

        switch (currentWeapon)
        {
            case WeaponType.Sword:
            case WeaponType.Tome:
                if (comboStep > 3) comboStep = 1;
                break;
            case WeaponType.Gauntlet:
                if (comboStep > 4) comboStep = 1;
                break;
        }

        comboTimer = 1f;
        attackCooldownTimer = attackCooldown;

        if (currentWeapon == WeaponType.Tome)
        {
            ShootProjectile();
        }
        else
        {
            // Single trigger, animator checks ComboStep
            animator.SetTrigger("DoAttack");
            animator.SetInteger("ComboStep", comboStep);
        }

        Debug.Log($"Performing Combo Step {comboStep} with {currentWeapon}");
    }

    private void ShootProjectile()
    {
        WeaponStats stats = weaponStatsList.Find(w => w.type == currentWeapon);
        if (stats == null || stats.projectilePrefabs.Count == 0) return;

        Transform spawnPoint = controller.facingRight ? attackPointRight : attackPointLeft;
        Vector2 direction = controller.facingRight ? Vector2.right : Vector2.left;

        foreach (GameObject prefab in stats.projectilePrefabs)
        {
            GameObject proj = Instantiate(prefab, spawnPoint.position, Quaternion.identity);

            Rigidbody2D rb = proj.GetComponent<Rigidbody2D>();
            if (rb != null)
                rb.linearVelocity = direction * stats.projectileSpeed;

            Projectile projectileScript = proj.GetComponent<Projectile>();
            if (projectileScript != null)
            {
                float damageMultiplier = GetDamageMultiplier(comboStep);
                projectileScript.damage = attackDamage * damageMultiplier;
                projectileScript.lifeTime = stats.projectileLifetime;
            }
        }
    }

    public void EnableHitbox(int index)
    {
        if (activeHitboxes != null && index >= 0 && index < activeHitboxes.Count)
            activeHitboxes[index].SetActive(true);
    }

    public void DisableHitbox(int index)
    {
        if (activeHitboxes != null && index >= 0 && index < activeHitboxes.Count)
            activeHitboxes[index].SetActive(false);
    }

    public float GetDamageMultiplier(int attackNumber)
    {
        switch (currentWeapon)
        {
            case WeaponType.Sword:
                return attackNumber switch { 1 => 1f, 2 => 1.2f, 3 => 1.5f, _ => 1f };
            case WeaponType.Tome:
                return attackNumber switch { 1 => 1f, 2 => 1.3f, 3 => 2f, _ => 1f };
            case WeaponType.Gauntlet:
                return attackNumber switch { 1 => 1f, 2 => 1.1f, 3 => 1.2f, 4 => 1.5f, _ => 1f };
            default:
                return 1f;
        }
    }

    public float GetKnockbackForce(int attackNumber)
    {
        return attackNumber switch
        {
            1 => 5f,
            2 => 7f,
            3 => 12f,
            4 => 15f,
            _ => 5f
        };
    }

    private void ResetCombo()
    {
        comboStep = 0;
        comboTimer = 0f;
        if (animator != null)
            animator.SetInteger("ComboStep", 0);
    }

    public float GetAttackDamage()
    {
        return attackDamage;
    }

}
