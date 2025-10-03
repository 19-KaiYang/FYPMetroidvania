using UnityEngine;

[CreateAssetMenu(fileName = "Critical", menuName = "Effects/Critical")]
public class CritEffect : UpgradeEffect
{
    public float critChance = 0.2f;
    public float critDmgMultiplier = 1.5f;
    public Upgrade bleedCritBuff;
    public Debuff bleedDebuff;
    public override void DoEffect(ActionContext context)
    {
        if (context.hitbox.isCritical || context.target == null) return;

        float finalcrit = critChance;

        if(context.upgradeManager.MiscUpgrades.Find(m => bleedCritBuff) != null)
        {
            foreach (DebuffInstance debuffInstance in context.target.debuffs)
            {
                if (debuffInstance.debuff == bleedDebuff)
                {
                    finalcrit *= 2;
                    break;
                }
            }
        }

        float rng = Random.Range(0f, 1f);
        if(rng <= finalcrit)
        {
            if (context.hitbox != null && context.skillSystem == null) {
                context.hitbox.damage *= critDmgMultiplier;
                Debug.Log("Crit landed on " + context.target.name);
            }
        }
    }

}
