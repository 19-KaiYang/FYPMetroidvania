using UnityEngine;

[CreateAssetMenu(fileName = "Skill Damage Amp", menuName = "Effects/Skill Damage Amp")]
public class SkillDamageEffect : UpgradeEffect
{
    public float damageMult = 1.2f;

    public override void DoEffect(ActionContext context)
    {
        if (context.hitbox == null || context.skillSystem == null) return;

        context.hitbox.damage *= damageMult;
        Debug.Log("Skill damage boosted x" + damageMult);
    }
}
