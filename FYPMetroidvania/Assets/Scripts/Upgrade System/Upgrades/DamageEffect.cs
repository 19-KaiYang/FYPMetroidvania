using UnityEngine;

[CreateAssetMenu(fileName = "Damage Amp", menuName = "Effects/Damage Amp")]
public class DamageEffect : UpgradeEffect
{
    public float damageMult = 1.2f;
    public override void DoEffect(ActionContext context)
    {
        if(context.hitbox != null && context.skillSystem == null)
        {
            context.hitbox.damage *= damageMult;
        }
    }
}
