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

    [Header("Attack Points")]
    public Transform attackPointRight;
    public Transform attackPointLeft;
    public Transform attackPointUp;
    public Transform attackPointDown;
    public LayerMask enemyLayers;

    private Animator animator;
    private PlayerController controller;
    private Vector2 moveInput;
    private Transform currentAttackPoint;

    // Cached stats
    private float attackDamage;
    private float attackRange;
    private float attackCooldown;

    // Combat variables
    private int comboStep = 0;
    private float comboTimer = 0f;
    private float attackCooldownTimer = 0f;
    private bool isAttacking = false;

    // Weapon unlocks
    private HashSet<WeaponType> unlockedWeapons = new HashSet<WeaponType>();

    private void Awake()
    {
        animator = GetComponent<Animator>();
        controller = GetComponent<PlayerController>();

        // Start with no weapon
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

    // Unlock new weapon from pickups
    public void UnlockWeapon(WeaponType weapon)
    {
        if (!unlockedWeapons.Contains(weapon))
        {
            unlockedWeapons.Add(weapon);
            Debug.Log($"{weapon} unlocked!");

            // Auto-equip first weapon if currently unarmed
            if (currentWeapon == WeaponType.None)
            {
                SetWeapon(weapon);
            }
        }
    }

    // Switch weapons at runtime
    public void SetWeapon(WeaponType newWeapon)
    {
        if (newWeapon == WeaponType.None)
        {
            currentWeapon = WeaponType.None;
            ApplyWeaponStats(currentWeapon);
            Debug.Log("No weapon equipped.");
            return;
        }

        if (unlockedWeapons.Contains(newWeapon))
        {
            currentWeapon = newWeapon;
            ApplyWeaponStats(newWeapon);
            Debug.Log($"Equipped {newWeapon}");
        }
        else
        {
            Debug.Log($"{newWeapon} not unlocked yet!");
        }
    }

    private void ApplyWeaponStats(WeaponType type)
    {
        if (type == WeaponType.None)
        {
            attackDamage = 0f;
            attackRange = 0f;
            attackCooldown = 0.5f;
            Debug.Log("Currently unarmed.");
            return;
        }

        WeaponStats stats = weaponStatsList.Find(w => w.type == type);
        if (stats != null)
        {
            attackDamage = stats.damage;
            attackRange = stats.range;
            attackCooldown = stats.attackCooldown;
        }
        else
        {
            Debug.LogWarning("No stats found for weapon: " + type);
        }
    }

    public void OnAttack()
    {
        if (currentWeapon == WeaponType.None)
        {
            Debug.Log("Tried to attack, but no weapon equipped!");
            return;
        }

        if (attackCooldownTimer <= 0f)
        {
            PerformAttack();
        }
    }

    private void PerformAttack()
    {
        comboStep++;

        // Combo limits per weapon
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
            // Pick direction for melee
            if (moveInput.y > 0.5f) currentAttackPoint = attackPointUp;
            else if (moveInput.y < -0.5f) currentAttackPoint = attackPointDown;
            else if (controller.facingRight) currentAttackPoint = attackPointRight;
            else currentAttackPoint = attackPointLeft;

            StartCoroutine(AttackSequence(comboStep));
        }

        if (animator != null)
        {
            animator.SetTrigger("Attack" + comboStep);
            animator.SetInteger("ComboStep", comboStep);
        }

        Debug.Log($"Performing Combo Step {comboStep} with {currentWeapon} (Damage {attackDamage})");
    }

    private IEnumerator AttackSequence(int attackNumber)
    {
        isAttacking = true;

        float attackDuration = GetAttackDuration(attackNumber);

        yield return new WaitForSeconds(0.1f);
        DealDamage(attackNumber);

        yield return new WaitForSeconds(attackDuration - 0.1f);

        isAttacking = false;
    }

    private void DealDamage(int attackNumber)
    {
        if (currentAttackPoint == null) return;

        float damageMultiplier = GetDamageMultiplier(attackNumber);
        float totalDamage = attackDamage * damageMultiplier;

        Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(
            currentAttackPoint.position, attackRange, enemyLayers);

        foreach (Collider2D enemy in hitEnemies)
        {
            Health enemyHealth = enemy.GetComponent<Health>();
            if (enemyHealth != null)
                enemyHealth.TakeDamage(totalDamage);

            Rigidbody2D enemyRb = enemy.GetComponent<Rigidbody2D>();
            if (enemyRb != null)
            {
                Vector2 knockbackDirection = (enemy.transform.position - transform.position).normalized;
                float knockbackForce = GetKnockbackForce(attackNumber);
                enemyRb.AddForce(knockbackDirection * knockbackForce, ForceMode2D.Impulse);
            }
        }
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

                Debug.Log($"Projectile fired! Combo {comboStep} Damage = {projectileScript.damage}");
            }
        }
    }

    private float GetAttackDuration(int attackNumber)
    {
        switch (attackNumber)
        {
            case 1: return 0.3f;
            case 2: return 0.4f;
            case 3: return 0.6f;
            case 4: return 0.7f;
            default: return 0.3f;
        }
    }

    private float GetDamageMultiplier(int attackNumber)
    {
        switch (currentWeapon)
        {
            case WeaponType.Sword:
                switch (attackNumber)
                {
                    case 1: return 1f;
                    case 2: return 1.2f;
                    case 3: return 1.5f;
                    default: return 1f;
                }

            case WeaponType.Tome:
                switch (attackNumber)
                {
                    case 1: return 1f;
                    case 2: return 1.3f;
                    case 3: return 2f;
                    default: return 1f;
                }

            case WeaponType.Gauntlet:
                switch (attackNumber)
                {
                    case 1: return 1f;
                    case 2: return 1.1f;
                    case 3: return 1.2f;
                    default: return 1f;
                }

            default:
                return 1f;
        }
    }

    private float GetKnockbackForce(int attackNumber)
    {
        switch (attackNumber)
        {
            case 1: return 5f;
            case 2: return 7f;
            case 3: return 12f;
            case 4: return 15f;
            default: return 5f;
        }
    }

    private void ResetCombo()
    {
        comboStep = 0;
        comboTimer = 0f;
        if (animator != null)
            animator.SetInteger("ComboStep", 0);
    }

    private void OnDrawGizmosSelected()
    {
        if (attackPointRight != null)
            Gizmos.DrawWireSphere(attackPointRight.position, attackRange);
        if (attackPointLeft != null)
            Gizmos.DrawWireSphere(attackPointLeft.position, attackRange);
        if (attackPointUp != null)
            Gizmos.DrawWireSphere(attackPointUp.position, attackRange);
        if (attackPointDown != null)
            Gizmos.DrawWireSphere(attackPointDown.position, attackRange);
    }
}
