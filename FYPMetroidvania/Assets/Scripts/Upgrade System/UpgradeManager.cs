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
        health = GetComponent<Health>();

        if (!RoomSaveManager.HasSaveData())
        {
            if (MobilityUpgrade != null) MobilityUpgrade.OnApply(this);
            foreach (Upgrade upgrade in MiscUpgrades)
            {
                upgrade.OnApply(this);
            }
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            if (MobilityUpgrade != null)
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

        Skills.skillStart += OnSkillStart;
        Skills.skillHit += OnSkillHit;
        Skills.skillEnd += OnSkillEnd;

        Skills.OnUltimateStart += OnUltimateStart;
        Skills.OnUltimateHit += OnUltimateHit;
        Skills.OnUltimateEnd += OnUltimateEnd;

    }

    #region Processing Events
    void OnBasicAttackStart(Hitbox hitbox)
    {
        ActionContext context = new ActionContext(this, player, health, combatSystem, hb: hitbox);

        if (AttackUpgrade != null)
            AttackUpgrade.TryEffects(Trigger.OnStart, context);

        foreach (Upgrade misc in MiscUpgrades)
        {
            misc.TryEffects(Trigger.OnStart, context);      
            misc.TryEffects(Trigger.OnAttackStart, context); 
        }
    }

    void OnBasicAttackEnd()
    {
        ActionContext context = new ActionContext(this, player, health, combatSystem);

        if (AttackUpgrade != null)
            AttackUpgrade.TryEffects(Trigger.OnEnd, context);

        // Trigger for misc upgrades
        foreach (Upgrade misc in MiscUpgrades)
        {
            misc.TryEffects(Trigger.OnEnd, context);
            misc.TryEffects(Trigger.OnAttackEnd, context);
        }
    }

    void OnBasicAttackHit(Hitbox hitbox, Health hit)
    {
        if (hitbox.gameObject == skillManager.swordDashHitbox ||
            hitbox.gameObject == skillManager.swordUppercutHitbox ||
            hitbox.gameObject == skillManager.gauntletShockwaveHitbox)
        {
            return;
        }

        Debug.Log("Hit!");

        ActionContext context = new ActionContext(this, player, health, combatSystem, hb: hitbox, enemy: hit);

        if (AttackUpgrade != null)
            AttackUpgrade.TryEffects(Trigger.OnAttackHit, context);

        foreach (Upgrade misc in MiscUpgrades)
            misc.TryEffects(Trigger.OnAttackHit, context);
    }


    // Skills
    void OnSkillStart(Hitbox hitbox)
    {
        ActionContext context = new ActionContext(this, player, health, combatSystem, skillManager, hitbox);

        if (SkillUpgrade != null)
            SkillUpgrade.TryEffects(Trigger.OnStart, context);

        foreach (Upgrade misc in MiscUpgrades)
            misc.TryEffects(Trigger.OnSkillStart, context);
    }

    void OnSkillEnd()
    {
        ActionContext ctx = new ActionContext(this, player, health, combatSystem, skillManager);

        if (SkillUpgrade != null)
            SkillUpgrade.TryEffects(Trigger.OnEnd, ctx);

        foreach (Upgrade misc in MiscUpgrades)
            misc.TryEffects(Trigger.OnSkillEnd, ctx);
    }

    void OnSkillHit(Hitbox hitbox, Health enemy)
    {
        ActionContext ctx = new ActionContext(this, player, health, cs: null, skills: skillManager, hb: hitbox, enemy: enemy);

        if (SkillUpgrade != null)
            SkillUpgrade.TryEffects(Trigger.OnSkillHit, ctx);

        foreach (Upgrade misc in MiscUpgrades)
            misc.TryEffects(Trigger.OnSkillHit, ctx);
    }


    // Ultimate
    void OnUltimateStart(Hitbox hitbox)
    {
        ActionContext context = new ActionContext(this, player, health, combatSystem, hb: hitbox);

        if (SpiritUpgrade != null)
            SpiritUpgrade.TryEffects(Trigger.OnStart, context);

        foreach (Upgrade misc in MiscUpgrades)
            misc.TryEffects(Trigger.OnSkillStart, context);
    }

    void OnUltimateHit(Hitbox hitbox, Health enemy)
    {
        ActionContext context = new ActionContext(this, player, health, combatSystem, hb: hitbox, enemy: enemy);

        if (SpiritUpgrade != null)
            SpiritUpgrade.TryEffects(Trigger.OnHit, context);

        // Trigger for misc upgrades
        foreach (Upgrade misc in MiscUpgrades)
            misc.TryEffects(Trigger.OnSkillHit, context);
    }

    void OnUltimateEnd()
    {
        ActionContext context = new ActionContext(this, player, health, combatSystem);

        if (SpiritUpgrade != null)
            SpiritUpgrade.TryEffects(Trigger.OnEnd, context);

        // Trigger for misc upgrades
        foreach (Upgrade misc in MiscUpgrades)
            misc.TryEffects(Trigger.OnSkillEnd, context);
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

        Skills.skillStart -= OnSkillStart;
        Skills.skillHit -= OnSkillHit;
        Skills.skillEnd -= OnSkillEnd;

        Skills.OnUltimateStart -= OnUltimateStart;
        Skills.OnUltimateHit -= OnUltimateHit;
        Skills.OnUltimateEnd -= OnUltimateEnd;
    }
}