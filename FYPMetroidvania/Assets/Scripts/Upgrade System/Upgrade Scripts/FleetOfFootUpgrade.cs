using UnityEngine;

[CreateAssetMenu(fileName = "Fleet of Foot", menuName = "Upgrades/Mobility/Fleet of Foot", order = 1)]
public class FleetOfFootUpgrade : Upgrade
{
    public float dashSpeedMultiplier = 1.25f; 
    public int extraDashes = 1;               

    public override void OnApply(UpgradeManager upgradeManager)
    {
        PlayerController.instance.dashSpeed *= dashSpeedMultiplier;
        PlayerController.instance.dashCount += extraDashes;
    }

    public override void OnRemove(UpgradeManager upgradeManager)
    {
        PlayerController.instance.dashSpeed /= dashSpeedMultiplier;
        PlayerController.instance.dashCount -= extraDashes;
    }
}
