using UnityEngine;

[CreateAssetMenu(fileName = "Fleet of Foot", menuName = "Upgrades/Mobility/Fleet of Foot", order = 1)]
public class FleetOfFootUpgrade : Upgrade
{
    public override void OnApply(UpgradeManager upgradeManager)
    {
        PlayerController.instance.dashSpeed *= 1.25f;
        PlayerController.instance.dashCount = 2;
        PlayerController.instance.dashesRemaining = 2;

        Debug.Log("[UPGRADE] Fleet of Foot applied. dashCount=2, dashesRemaining=2, dashSpeed=" + PlayerController.instance.dashSpeed);
    }

    public override void OnRemove(UpgradeManager upgradeManager)
    {
        PlayerController.instance.dashSpeed /= 1.25f;
        PlayerController.instance.dashCount = 1;
        PlayerController.instance.dashesRemaining = 1;

        Debug.Log("[UPGRADE] Fleet of Foot removed. dashCount=1, dashesRemaining=1, dashSpeed=" + PlayerController.instance.dashSpeed);
    }


}
