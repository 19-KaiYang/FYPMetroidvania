using UnityEngine;

[CreateAssetMenu(menuName = "Upgrades/Simple Upgrade", order = 0)]
public class SimpleUpgrade : Upgrade
{
    public override void OnApply(UpgradeManager upgradeManager)
    {
        Debug.Log($"{name} applied.");
    }

    public override void OnRemove(UpgradeManager upgradeManager)
    {
        Debug.Log($"{name} removed.");
    }
}


