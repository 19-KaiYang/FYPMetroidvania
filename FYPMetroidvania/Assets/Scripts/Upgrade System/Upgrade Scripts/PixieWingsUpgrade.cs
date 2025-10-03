using JetBrains.Annotations;
using UnityEngine;

[CreateAssetMenu(fileName = "Pixie Wings",menuName = "Upgrades/Mobility/Pixie Wings", order = 0)]
public class PixieWingsUpgrade : Upgrade
{
    public int airJumps = 2;
    public float floatGravity = -5f;
    public override void OnApply(UpgradeManager upgradeManager)
    {
        PlayerController.instance.airJumpCount = 2;
        PlayerController.instance.canFloat = true;
        PlayerController.instance.floatGravity = floatGravity;
    }

    public override void OnRemove(UpgradeManager upgradeManager)
    {
        PlayerController.instance.airJumpCount = 0;
        PlayerController.instance.canFloat = false;
    }
}
