using UnityEngine;

[CreateAssetMenu(fileName = "Skill Critical", menuName = "Effects/Skill Critical")]
public class SkillCritEffect : UpgradeEffect
{
    public float critChance = 0.2f;
    public float critDmgMultiplier = 1.5f;
    public Upgrade bleedCritBuff;
    public Debuff bleedDebuff;

    public override void DoEffect(ActionContext context)
    {
        if (context.hitbox == null || context.skillSystem == null || context.target == null) return;

        float finalCrit = critChance;

        if (context.upgradeManager.MiscUpgrades.Find(m => bleedCritBuff) != null)
        {
            foreach (DebuffInstance debuffInstance in context.target.debuffs)
            {
                if (debuffInstance.debuff == bleedDebuff)
                {
                    finalCrit *= 2;
                    break;
                }
            }
        }

        float rng = Random.Range(0f, 1f);
        if (rng <= finalCrit)
        {
            context.hitbox.damage *= critDmgMultiplier;
            Debug.Log("Skill CRIT landed on " + context.target.name);
            context.hitbox.isCritical = true;
        }
    }
}
