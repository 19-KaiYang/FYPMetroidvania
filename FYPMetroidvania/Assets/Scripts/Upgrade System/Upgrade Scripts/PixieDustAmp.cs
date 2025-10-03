using UnityEngine;

[CreateAssetMenu(fileName = "Ferocious Fairy", menuName = "Upgrades/Misc/Pixie Dust Buff")]
public class PixieDustAmp : Upgrade
{
    public Debuff pixieDust;
    public int buffedmaxStacks;
    int originalmaxStacks;
    public override void OnApply(UpgradeManager upgradeManager)
    {
        if (pixieDust != null)
        {
            originalmaxStacks = pixieDust.maxStacks;
            pixieDust.maxStacks = buffedmaxStacks;
        }
    }

    public override void OnRemove(UpgradeManager upgradeManager)
    {
        if (pixieDust != null) pixieDust.maxStacks = originalmaxStacks;
    }

}
