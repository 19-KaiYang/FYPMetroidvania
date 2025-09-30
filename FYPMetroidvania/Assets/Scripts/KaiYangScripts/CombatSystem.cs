using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting.Antlr3.Runtime.Misc;
using UnityEngine;
using UnityEngine.InputSystem;

public enum WeaponType
{
    None,
    Sword,
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
    private InputAction _skill3ChargeAction;

    public OverheatSystem overheat;

    [Header("General Attack Settings")]
    public float baseAttackDamage = 10f;
    public float OverheatMultiplier;

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

    public GameObject swordDownSweepHitbox;


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

    // Events
    public static Action<Hitbox> basicAttackStart;
    public static Action basicAttackEnd;

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
        overheat = GetComponent<OverheatSystem>();

        controller = GetComponent<PlayerController>();

        var pi = GetComponent<PlayerInput>();
        _skill3ChargeAction = pi.actions["Skill3Charge"];

        _skill3ChargeAction.started += OnSkill3ChargeStarted;
        _skill3ChargeAction.canceled += OnSkill3ChargeCanceled;

        int playerLayer = gameObject.layer;
        int enemyLayer = LayerMask.NameToLayer("EnemyLayer");
        if (enemyLayer >= 0)
            Physics2D.IgnoreLayerCollision(playerLayer, enemyLayer, false);

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

        if (skills != null && skills.IsChargeLocked)
            return; 

        if (Keyboard.current.digit1Key.wasPressedThisFrame && unlockedWeapons.Contains(WeaponType.Sword))
            SetWeapon(WeaponType.Sword);

        if (Keyboard.current.digit2Key.wasPressedThisFrame && unlockedWeapons.Contains(WeaponType.Gauntlet))
            SetWeapon(WeaponType.Gauntlet);


    }

    #region Skills Usage
    public void OnSkill1(InputValue value)
    {
        if (skills != null && skills.IsChargeLocked) return;
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
        if (skills != null && skills.IsChargeLocked) return;
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
        if (skills != null && skills.IsChargeLocked) return;
        if (skills == null) return;
        bool pressed = value.isPressed;

        //Add skills here (Skills 3)
        switch (currentWeapon)
        {
            case WeaponType.Sword:
                skills.TryUseSwordCrimsonWave();
                break;

            default:

                break;
        }
    }

    //strictly for hold gauntlet chargeshot
    private void OnSkill3ChargeStarted(InputAction.CallbackContext ctx)
    {
        if (currentWeapon == WeaponType.Gauntlet)
        {
            if (skills != null)
            {
                skills.IsChargeButtonHeld = true;
                skills.StartGauntletChargeShot(); 
            }

        }
    }


    private void OnSkill3ChargeCanceled(InputAction.CallbackContext ctx)
    {
        if (currentWeapon == WeaponType.Gauntlet)
        {
            skills.IsChargeButtonHeld = false;  
            skills.ReleaseGauntletChargeShot();
        }
    }


    private void OnDestroy()
    {
        if (_skill3ChargeAction != null)
        {
            _skill3ChargeAction.started -= OnSkill3ChargeStarted;
            _skill3ChargeAction.canceled -= OnSkill3ChargeCanceled;
        }
    }

    #endregion

    #region Ultimate Usage

    public void OnUltimate(InputValue value)
    {
        if (skills != null && !skills.IsChargeLocked)
        {
            switch (currentWeapon)
            {
                case WeaponType.Sword:
                    skills.TryUseSwordUltimate();
                    break;

            }
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
        var health = GetComponent<Health>();
        if (health != null && health.currentCCState != CrowdControlState.None)
            return;

        if (skills != null && skills.IsChargeLocked) return;
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

        if (currentWeapon == WeaponType.Sword)
        {
            if (controller.IsGrounded && swordDownSweepHitbox != null)
            {
                // Grounded sweep attack
                var hb = swordDownSweepHitbox.GetComponent<Hitbox>();
                if (hb != null)
                    hb.isSweepHitbox = true; 

                StartCoroutine(ToggleHitbox(swordDownSweepHitbox, 0.2f));
            }
            else if (swordDownHitbox != null)
            {
                // Air down attack
                StartCoroutine(ToggleHitbox(swordDownHitbox, 0.2f));
            }
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
                return;
            }
        }

        // Normal flow
        comboStep++;
        if (comboStep > 3) comboStep = 1;

        comboTimer = 1f;
        attackCooldownTimer = attackCooldown;

        animator.SetTrigger("DoAttack");
        animator.SetInteger("ComboStep", comboStep);

        controller.externalVelocityOverride = false;
        controller.SetHitstop(false); // clear any leftover hitstop

        Debug.Log($"Performing Combo Step {comboStep} with {currentWeapon}");
    }

    // === HITBOX HELPERS ===
    public void EnableHitbox(int index)
    {
        Debug.Log($"[EnableHitbox] index {index}");
        if (activeHitboxes != null && index >= 0 && index < activeHitboxes.Count)
        {
            activeHitboxes[index].SetActive(true);
            basicAttackStart?.Invoke(activeHitboxes[index].GetComponent<Hitbox>());
        }
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

        controller.externalVelocityOverride = false;
        controller.SetHitstop(false);
    }


    public float GetAttackDamage(int attackNumber)
    {
        float dmg = baseAttackDamage;
        dmg *= GetDamageMultiplier(attackNumber);

        // Debug each condition separately
        Debug.Log($"Current weapon: {currentWeapon}");
        Debug.Log($"Overheat reference null? {overheat == null}");
        if (overheat != null)
        {
            Debug.Log($"Is overheated? {overheat.IsOverheated}");
            Debug.Log($"Current heat: {overheat.CurrentHeat}");
        }

        if (currentWeapon == WeaponType.Gauntlet && overheat != null && overheat.IsOverheated)
        {
            Debug.Log("OVERHEAT MULTIPLIER APPLIED! Damage boosted from " + (dmg / 3f) + " to " + dmg);
            dmg *= OverheatMultiplier;
        }
        return dmg;
    }


}
