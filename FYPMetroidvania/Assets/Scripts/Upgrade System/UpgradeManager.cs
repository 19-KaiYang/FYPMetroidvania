using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem.XR;
using static UnityEngine.Rendering.DebugUI;

public class UpgradeManager : MonoBehaviour
{
    [Header("References")]
    private PlayerController player;
    private Health health;
    private CombatSystem combatSystem;
    private Skills skillManager;

    [Header("Upgrades")]
    public Upgrade AttackUpgrade;
    public Upgrade SkillUpgrade;
    public Upgrade SpiritUpgrade;
    public Upgrade MobilityUpgrade;
    public List<Upgrade> MiscUpgrades = new();

    private void Start()
    {
        player = GetComponent<PlayerController>();
        combatSystem = GetComponent<CombatSystem>();
        skillManager = GetComponent<Skills>();

        // Temp for testing
        if (MobilityUpgrade != null) MobilityUpgrade.OnApply(this);
        foreach(Upgrade upgrade in MiscUpgrades)
        {
            upgrade.OnApply(this);
        }
    }
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            if(MobilityUpgrade != null)
            {
                MobilityUpgrade.OnRemove(this);
                MobilityUpgrade = null;
            }
        }   
    }
    private void OnEnable()
    {
        CombatSystem.basicAttackStart += OnBasicAttackStart;
        CombatSystem.basicAttackEnd += OnBasicAttackEnd;
        Hitbox.OnHit += OnBasicAttackHit;
    }

    #region Processing Events
    // Basic Attack
    void OnBasicAttackStart(Hitbox hitbox)
    {
        if (AttackUpgrade == null) return;
        Debug.Log("Received attack upgrade event: " + hitbox.name);
        ActionContext context = new ActionContext(this, player, health, combatSystem, hb: hitbox);
        AttackUpgrade.TryEffects(Trigger.OnStart, context);
    }
    void OnBasicAttackEnd()
    {
        if (AttackUpgrade == null) return;

        ActionContext context = new ActionContext(this, player, health, combatSystem);
        AttackUpgrade.TryEffects(Trigger.OnEnd, context);
    }
    void OnBasicAttackHit(Hitbox hitbox, Health hit)
    {
        Debug.Log("Hit!");
        if (AttackUpgrade == null) return;

        ActionContext context = new ActionContext(this, player, health, combatSystem, hb: hitbox, enemy: hit);
        AttackUpgrade.TryEffects(Trigger.OnHit, context);
    }

    // Skills
    void OnSkillStart(Hitbox hitbox)
    {
        if (SkillUpgrade == null) return;
        ActionContext ctx = new ActionContext(this, player, health, combatSystem, skillManager, hitbox);
        SkillUpgrade.TryEffects(Trigger.OnStart, ctx);
    }
    void OnSkillEnd()
    {
        if (SkillUpgrade == null) return;
        ActionContext ctx = new ActionContext(this, player, health, combatSystem, skillManager);
        SkillUpgrade.TryEffects(Trigger.OnEnd, ctx);
    }
    void OnSkillHit(Hitbox hitbox, Health enemy)
    {
        if (SkillUpgrade == null) return;
        ActionContext ctx = new ActionContext(this, player, health, combatSystem, skillManager, hitbox, enemy);
        SkillUpgrade.TryEffects(Trigger.OnHit, ctx);
    }

    // Ultimate
    void OnUltStart()
    {

    }
    void OnUltEnd()
    {

    }
    void OnUltHit()
    {

    }

    // Misc

    #endregion

    public void SpawnEffectProjectile(GameObject projectile)
    {
        Vector2 dir = player.facingRight ? Vector2.right : Vector2.left;
        GameObject proj = Instantiate(projectile, transform.position + (Vector3)(dir * 0.7f), Quaternion.identity);
        ProjectileBase projectilebase = proj.GetComponent<ProjectileBase>();
        if (projectilebase != null)
        {
            projectilebase.direction = dir;
            projectile.GetComponent<SpriteRenderer>().flipX = player.facingRight ? false : true;
        }
    }
    private void OnDestroy()
    {
        CombatSystem.basicAttackStart -= OnBasicAttackStart;
        CombatSystem.basicAttackEnd -= OnBasicAttackEnd;
        Hitbox.OnHit -= OnBasicAttackHit;

        foreach(Upgrade upgrade in MiscUpgrades)
        {
            upgrade.OnRemove(this);
        }
    }
}
