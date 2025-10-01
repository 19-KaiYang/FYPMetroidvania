using UnityEngine;

[CreateAssetMenu(fileName = "Pixie Dust", menuName = "Effects/Pixie Dust")]
public class PixieDustEffect : UpgradeEffect
{
    public Debuff PixieDustDebuff;
    public int stacks = 1;
    public float duration = 5f;
    public override void DoEffect(ActionContext context)
    {
        if(context.target == null || PixieDustDebuff == null) return;

        PixieDustDebuff.ApplyDebuff(context.target, stacks, duration);
    }
}
