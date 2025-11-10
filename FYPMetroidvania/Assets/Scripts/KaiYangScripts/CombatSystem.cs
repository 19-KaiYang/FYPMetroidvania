using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
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
[Serializable]
public class AttackVFX
{
    public string VFX_name;
    public string animationName;
    public Vector3 position;
    public float angle;
    public Vector3 scale;
    public float sfxVolume = 0.5f;
}

public class CombatSystem : MonoBehaviour
{
    private InputAction _skill3ChargeAction;
    public OverheatSystem overheat;

    public bool isAttacking;

    [Header("Particle Effects")]
    public Animator particleEffectAnimator;
    public GameObject particleEffectsObject;

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

    [Header("Melee VFX")]
    public List<AttackVFX> attackVFXList;

    private List<GameObject> activeHitboxes;

    private Animator animator;
    private SpriteRenderer spriteRenderer;
    private PlayerController controller;

    // Cached stats
    private float attackDamage;
    private float attackCooldown;

    // Combat variables
    public int comboStep = 0;
    private float comboTimer = 0f;
    private float attackCooldownTimer = 0f;
    public bool canTransition = true;
    public bool isBuffered;

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
            Physics2D.IgnoreLayerCollision(playerLayer, enemyLayer, true);

        UnlockWeapon(WeaponType.Sword);
        ApplyWeaponStats(currentWeapon);
    }

    private void Update()
    {
        int playerLayer = gameObject.layer;
        int enemyLayer = LayerMask.NameToLayer("EnemyLayer");
        Physics2D.IgnoreLayerCollision(playerLayer, enemyLayer, true);

        if (attackCooldownTimer > 0f)
            attackCooldownTimer -= Time.deltaTime;

        if (isBuffered && !isAttacking)
        {
            if (comboStep > 3)
            {
                //comboStep = 0;
                comboTimer = 1f;
            }
            else
            {
                comboTimer = 1f;
            }
            attackCooldownTimer = attackCooldown;

            animator.SetBool("IsAttacking", true);
            animator.SetTrigger("DoAttack");

            Debug.Log($"Performing Combo Step {comboStep} with {currentWeapon}");
            canTransition = false;
            isAttacking = true;
            controller.externalVelocityOverride = true;
            if(controller.IsGrounded) controller.SetVelocity(Vector2.zero);
            //isBuffered = false;
        }

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
                case WeaponType.Gauntlet:
                    skills.TryUseGauntletUltimate();
                    break;

            }
        }
    }

    #endregion

    #region Weapon System
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
    #endregion
    public void OnAttack()
    {
        var health = GetComponent<Health>();
        if (health != null && health.currentCCState != CrowdControlState.None)
            return;

        if (skills != null && skills.IsChargeLocked) return;
        if (skills != null && skills.IsUsingSkill && !skills.IsUsingUltimate) return;
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
        if (isAttacking || !controller.IsGrounded) return;

        comboStep = 0;
        attackCooldownTimer = attackCooldown;

        isAttacking = true;
        controller.externalVelocityOverride = true;
        if (controller.IsGrounded) controller.SetVelocity(Vector2.zero);
        if (currentWeapon == WeaponType.Sword && swordUpHitbox != null)
        {
            //StartCoroutine(ToggleHitbox(swordUpHitbox, 0.2f));
            animator.SetBool("IsAttacking", true);
            animator.SetTrigger("UpAttack");
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
        if (isAttacking) return;

        comboStep = 0;
        attackCooldownTimer = attackCooldown;

        
        if (currentWeapon == WeaponType.Sword)
        {
            if (controller.IsGrounded && swordDownSweepHitbox != null)
            {
                controller.externalVelocityOverride = true;
                if (controller.IsGrounded) controller.SetVelocity(Vector2.zero);
                StartCoroutine(ToggleHitbox(swordDownSweepHitbox, 0.2f));
            }
            else if (swordDownHitbox != null)
            {
                isAttacking = true;
                controller.externalVelocityOverride = true;
                animator.SetBool("IsAttacking", true);
                animator.SetTrigger("Air DownAttack");
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
        if (!controller.IsGrounded && !isAttacking) {
            isAttacking = true;
            controller.externalVelocityOverride = true;
            animator.SetBool("IsAttacking", true);
            animator.SetTrigger("Air Attack");
            return;
        }

        if (!isBuffered)
        {
            // Normal flow
            comboStep++;
            animator.SetInteger("ComboStep", comboStep);
            isBuffered = true;
            //if (comboStep > 3) ResetCombo();
        }
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
        //isAttacking = false;
    }
    public void DisableAllHitboxes()
    {
        foreach(var hitbox in activeHitboxes)
            hitbox.SetActive(false);
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

    //Particle Effect Animation Events
    // Call these from Animation Events
    public void PlayEffect1()
    {
        if (particleEffectAnimator != null)
        {
            particleEffectAnimator.SetTrigger("Effect1");
            Debug.Log("Playing Effect1");
        }
        AudioManager.PlaySFX(SFXTYPE.SWORD_SWING, 0.5f);
    }
    public void PlayEffect2()
    {
        if (particleEffectAnimator != null)
        {
            particleEffectAnimator.SetTrigger("Effect2");
            Debug.Log("Playing Effect2");
        }
        AudioManager.PlaySFX(SFXTYPE.SWORD_SWING, 0.5f);
    }
    public void PlayEffect3()
    {
        if (particleEffectAnimator != null)
        {
            particleEffectAnimator.SetTrigger("Effect3");
            Debug.Log("Playing Effect3");
        }
        AudioManager.PlaySFX(SFXTYPE.SWORD_SWING, 1.0f);
    }
    public void PlayVFX(string Name)
    {
        if (particleEffectsObject == null) return;
        AttackVFX vfx = attackVFXList.FirstOrDefault(i => i.VFX_name == Name);
        if (vfx != null)
        {
            particleEffectsObject.SetActive(true);
            particleEffectsObject.transform.localPosition = vfx.position;
            particleEffectsObject.transform.localEulerAngles = new Vector3(0f, 0f, vfx.angle);
            particleEffectsObject.transform.localScale = vfx.scale;
            particleEffectAnimator.Play(vfx.animationName);

            AudioManager.PlaySFX(SFXTYPE.SWORD_SWING, vfx.sfxVolume);
        }
    }
    public void HideVFX()
    {
        if (particleEffectsObject != null)
        {
            var sr = particleEffectsObject.GetComponent<SpriteRenderer>();
            if (sr != null) sr.sprite = null;

            particleEffectsObject.SetActive(false);
        }
        //animator.SetBool("isAttacking", false);
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


        //controller.externalVelocityOverride = false;
        //controller.SetHitstop(false);
        canTransition = true;
        isBuffered = false;
    }

    public void SetCanTransition(int comboEnd)
    {
        Debug.Log("attack can be cancelled");
        isAttacking = false;
        controller.externalVelocityOverride = false;
        if (comboEnd == 1) ResetCombo();
    }
    public void SetCanBuffer()
    {
        isBuffered = false;
        comboStep = animator.GetInteger("ComboStep");
        animator.SetInteger("ComboStep", 0);
    }
    public void SetEndAnimation()
    {
        Debug.Log("End of attack animation");
        //isAttacking = false;
        animator.SetBool("IsAttacking", false);
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
