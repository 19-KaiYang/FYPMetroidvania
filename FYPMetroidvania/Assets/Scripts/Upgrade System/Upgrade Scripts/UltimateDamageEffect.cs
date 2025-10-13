using UnityEngine;

[CreateAssetMenu(fileName = "Ultimate Damage Amp", menuName = "Effects/Ultimate Damage Amp")]
public class UltimateDamageEffect : UpgradeEffect
{
    public float damageMult = 1.2f;

    public override void DoEffect(ActionContext context)
    {
        if (context.hitbox != null)
        {
            context.hitbox.damage *= damageMult;
            Debug.Log("Ultimate damage boosted x" + damageMult);
        }
    }
}