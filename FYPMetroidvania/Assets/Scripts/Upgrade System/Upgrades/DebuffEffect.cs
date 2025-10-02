using UnityEngine;

[CreateAssetMenu(fileName = "Debuff", menuName = "Effects/Debuff")]
public class DebuffEffect : UpgradeEffect
{
    public Debuff Debuff;
    public int stacks = 1;
    public float duration = 5f;
    public override void DoEffect(ActionContext context)
    {
        if(context.target == null || Debuff == null) return;

        Debuff.ApplyDebuff(context.target, stacks, duration);
    }
}
