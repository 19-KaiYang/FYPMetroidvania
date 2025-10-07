using UnityEngine;

[CreateAssetMenu(fileName = "DebuffDamage", menuName = "Effects/Debuff Damage")]
public class DebuffDamageEffect : UpgradeEffect
{
    public float damageMultiplier = 1.15f;

    public override void DoEffect(ActionContext ctx)
    {
        if (ctx.target == null) return;

        Health h = ctx.target.GetComponent<Health>();
        if (h == null) return;

        // check if the target has any debuffs
        if (h.debuffs != null && h.debuffs.Count > 0)
        {
            ctx.damage *= damageMultiplier;  
            Debug.Log($"[DebuffDamageEffect] Bonus applied! New damage = {ctx.damage}");
        }
    }
}
