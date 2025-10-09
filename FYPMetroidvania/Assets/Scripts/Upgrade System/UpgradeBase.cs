using NUnit.Framework;
using System;
using System.Collections.Generic;
using UnityEngine;
public enum Trigger
{
    NoTrigger,
    OnStart,
    OnHit,
    OnEnd,
    OnAttackStart,   
    OnAttackHit,       
    OnAttackEnd,       
    OnSkillStart,      
    OnSkillHit,       
    OnSkillEnd,
}
public abstract class Upgrade : ScriptableObject
{
    public List<UpgradeEffect> effects;
    public abstract void OnApply(UpgradeManager upgradeManager);
    public abstract void OnRemove(UpgradeManager upgradeManager);
    public void TryEffects(Trigger trigger, ActionContext context)
    {
        foreach (var effect in effects)
        {
            if (effect.trigger == trigger) effect.DoEffect(context);
        }
    }
}

// struct for passing in all relevant action data for the upgrade to process
public struct ActionContext
{
    public UpgradeManager upgradeManager;
    public PlayerController player;
    public Health playerHealth;
    public CombatSystem combatSystem;
    public Skills skillSystem;
    public Hitbox hitbox;
    public Health target;
    public float damage;

    public ActionContext(UpgradeManager um, PlayerController pc = null, Health health = null, CombatSystem cs = null, Skills skills = null, Hitbox hb = null, Health enemy = null, float dmg = 0)
    {
        upgradeManager = um;
        player = pc;
        playerHealth = health;
        combatSystem = cs;
        skillSystem = skills;
        hitbox = hb;
        target = enemy;
        damage = dmg;
    }
}

