using UnityEngine;

[CreateAssetMenu(fileName = "PixieDust", menuName = "Effects/PixieDust")]
public class PixieDustEffect : UpgradeEffect
{
    public Debuff Debuff;
    public int stacks = 1;
    public float duration = 5f;
    public int maxStacks = 10;
    public Upgrade maxStacksUpgrade;
    public override void DoEffect(ActionContext context)
    {
        if (context.target == null || Debuff == null) return;

        Debuff.ApplyDebuff(context.target, stacks, duration);
        if(context.upgradeManager.MiscUpgrades.Find(m => maxStacksUpgrade) != null)
        {
            Debuff.maxStacks = maxStacks * 2;
        }
        else Debuff.maxStacks = maxStacks;
    }
}
