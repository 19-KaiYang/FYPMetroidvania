using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting.Antlr3.Runtime.Misc;
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
    public float attackCooldown;

    [Header("Ranged Only")]
    public List<GameObject> projectilePrefabs;
    public float projectileSpeed = 10f;
    public float projectileLifetime = 3f;
}

public class CombatSystem : MonoBehaviour
{

    [Header("General Attack Settings")]
    public float baseAttackDamage = 10f;

    [Header("Weapon Settings")]
    public WeaponType currentWeapon;
    public List<WeaponStats> weaponStatsList = new List<WeaponStats>();

    [Header("Attack Points (for projectiles)")]
    public Transform attackPointRight;
    public Transform attackPointLeft;

    [Header("Directional Hitboxes")]
    public GameObject swordUpHitbox;
    public GameObject gauntletUpHitbox;

    public GameObject swordDownHitbox;
    public GameObject gauntletDownHitbox;


    [Header("Melee Combo Hitboxes")]
    public List<GameObject> swordHitboxes;
    public List<GameObject> gauntletHitboxes;

    private List<GameObject> activeHitboxes;

    private Animator animator;
    private SpriteRenderer spriteRenderer;
    private PlayerController controller;

    // Cached stats
    private float attackDamage;
    private float attackCooldown;

    // Combat variables
    private int comboStep = 0;
    private float comboTimer = 0f;
    private float attackCooldownTimer = 0f;

    // Weapon unlocks
    private HashSet<WeaponType> unlockedWeapons = new HashSet<WeaponType>();

    //Skills 
    private Skills skills;

    public int CurrentComboStep => comboStep;

    private void Start()
    {
      
        foreach (var hb in swordHitboxes)
            if (hb) hb.SetActive(false);

       
        foreach (var hb in gauntletHitboxes)
            if (hb) hb.SetActive(false);

      
    }


    private void Awake()
    {
        // Auto-find Animator & SpriteRenderer 
        animator = GetComponentInChildren<Animator>();
        spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        skills = GetComponentInChildren<Skills>();

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

        if (Keyboard.current.digit1Key.wasPressedThisFrame && unlockedWeapons.Contains(WeaponType.Sword))
            SetWeapon(WeaponType.Sword);

        if (Keyboard.current.digit2Key.wasPressedThisFrame && unlockedWeapons.Contains(WeaponType.Tome))
            SetWeapon(WeaponType.Tome);

        if (Keyboard.current.digit3Key.wasPressedThisFrame && unlockedWeapons.Contains(WeaponType.Gauntlet))
            SetWeapon(WeaponType.Gauntlet);


    }

    #region Skills Usage
    public void OnSkill1(InputValue value)
    {
        if (skills == null) return;

        //Add skills here (Skills 1)
        switch (currentWeapon)
        {
            case WeaponType.Sword:
                skills.TryUseSwordDash();
                break;

            case WeaponType.Gauntlet:
                skills.TryUseGauntletShockwave();
                break;

            default:
                
                break;
        }
    }

    public void OnSkill2(InputValue value)
    {
        if (skills == null) return;

        //Add skills here (Skills 2)
        switch (currentWeapon)
        {
            case WeaponType.Sword:
                skills.TryUseSwordUppercut();
                break;
            case WeaponType.Gauntlet:
                skills.TryUseGauntletLaunch();
                break;


            default:

                break;
        }
    }
    public void OnSkill3(InputValue value)
    {
        if (skills == null) return;

        //Add skills here (Skills 3)
        switch (currentWeapon)
        {
            case WeaponType.Sword:
                skills.TryUseSwordCrimsonWave();
                break;
            case WeaponType.Gauntlet:
                skills.TryUseGauntletChargeShot();
                break;

            default:

                break;
        }
    }
    #endregion

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

        if (animator != null)
            animator.SetInteger("Weapon", (int)newWeapon);

        Debug.Log($"Equipped {newWeapon}");
    }

    public IEnumerable<WeaponType> GetUnlockedWeapons()
    {
        return unlockedWeapons;
    }


    private void ApplyWeaponStats(WeaponType type)
    {
        if (type == WeaponType.None)
        {
            attackDamage = 0f;
            attackCooldown = 0.5f;
            return;
        }

        WeaponStats stats = weaponStatsList.Find(w => w.type == type);
        if (stats != null)
        {
            attackDamage = baseAttackDamage;      
            attackCooldown = stats.attackCooldown;
        }
    }


    public void OnAttack()
    {
        if (skills != null && skills.IsUsingSkill) return;
        if (currentWeapon == WeaponType.None) return;
        if (attackCooldownTimer > 0f) return;


        bool up = Keyboard.current.wKey.isPressed;
        bool down = Keyboard.current.sKey.isPressed;

        if (up)
        {
            PerformUpAttack();
        }
        else if (down)
        {
            PerformDownAttack();
        }
        else
        {
            PerformAttack();
        }
    }

    private void PerformUpAttack()
    {
        comboStep = 0;
        attackCooldownTimer = attackCooldown;

        Debug.Log($"Performed UP attack with {currentWeapon}");

        if (currentWeapon == WeaponType.Sword && swordUpHitbox != null)
        {
            StartCoroutine(ToggleHitbox(swordUpHitbox, 0.2f));
        }
        else if (currentWeapon == WeaponType.Gauntlet && gauntletUpHitbox != null)
        {
            StartCoroutine(ToggleHitbox(gauntletUpHitbox, 0.2f));
        }
    }

    private IEnumerator ToggleHitbox(GameObject hitbox, float duration)
    {
        hitbox.SetActive(true);
        yield return new WaitForSeconds(duration);
        hitbox.SetActive(false);
    }



    private void PerformDownAttack()
    {
        comboStep = 0;
        attackCooldownTimer = attackCooldown;

        Debug.Log($"Performed DOWN attack with {currentWeapon}");

        if (currentWeapon == WeaponType.Sword && swordDownHitbox != null)
        {
            StartCoroutine(ToggleHitbox(swordDownHitbox, 0.2f));
        }
        else if (currentWeapon == WeaponType.Gauntlet && gauntletDownHitbox != null)
        {
            StartCoroutine(ToggleHitbox(gauntletDownHitbox, 0.2f));
        }
    }



    private void PerformAttack()
    {
        
        if (currentWeapon == WeaponType.Gauntlet && skills != null)
        {
            if (skills.GauntletDeployed)  
            {
                skills.RetractGauntlet();
                attackCooldownTimer = attackCooldown;
                return;
            }
        }

        // Normal flow
        comboStep++;
        if (comboStep > 3) comboStep = 1;

        comboTimer = 1f;
        attackCooldownTimer = attackCooldown;

        if (currentWeapon == WeaponType.Tome)
        {
            ShootProjectile();
        }
        else
        {
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

    // === HITBOX HELPERS ===
    public void EnableHitbox(int index)
    {
        Debug.Log($"[EnableHitbox] index {index}");
        if (activeHitboxes != null && index >= 0 && index < activeHitboxes.Count)
            activeHitboxes[index].SetActive(true);
    }


    public void DisableHitbox(int index)
    {
        if (activeHitboxes != null && index >= 0 && index < activeHitboxes.Count)
            activeHitboxes[index].SetActive(false);
    }

    public void DisableSwordUpHitbox()
    {
        if (swordUpHitbox != null)
        {
            var hb = swordUpHitbox.GetComponent<Hitbox>();
            if (hb != null) swordUpHitbox.GetComponent<Collider2D>().enabled = false;
        }
    }

    public void DisableGauntletUpHitbox()
    {
        if (gauntletUpHitbox != null)
        {
            var hb = gauntletUpHitbox.GetComponent<Hitbox>();
            if (hb != null) gauntletUpHitbox.GetComponent<Collider2D>().enabled = false;
        }
    }
    public void DisableSwordDownHitbox()
    {
        if (swordDownHitbox != null)
        {
            var hb = swordDownHitbox.GetComponent<Hitbox>();
            if (hb != null) swordDownHitbox.GetComponent<Collider2D>().enabled = false;
        }
    }

    public void DisableGauntletDownHitbox()
    {
        if (gauntletDownHitbox != null)
        {
            var hb = gauntletDownHitbox.GetComponent<Hitbox>();
            if (hb != null) gauntletDownHitbox.GetComponent<Collider2D>().enabled = false;
        }
    }



    // === DAMAGE MULTIPLIER ===
    public float GetDamageMultiplier(int attackNumber)
    {
        return attackNumber switch
        {
            1 => 1f,
            2 => 1.2f,
            3 => 1.5f,
            _ => 1f
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
        return baseAttackDamage + UpgradeManager.instance.GetGeneralDamageBonus();
    }

}
