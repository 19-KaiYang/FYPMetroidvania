using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;
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
    public List<Upgrade> MiscUpgrades = new();

    private void Start()
    {
        player = GetComponent<PlayerController>();
        combatSystem = GetComponent<CombatSystem>();
        skillManager = GetComponent<Skills>();
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
    void OnSkillStart()
    {

    }
    void OnSkillEnd()
    {

    }
    void OnSkillHit()
    {

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

    private void OnDestroy()
    {
        CombatSystem.basicAttackStart -= OnBasicAttackStart;
        CombatSystem.basicAttackEnd -= OnBasicAttackEnd;
        Hitbox.OnHit -= OnBasicAttackHit;
    }
}
