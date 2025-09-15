using UnityEngine;

public class UpgradeManager : MonoBehaviour
{
    public static UpgradeManager instance;

    [Header("General Upgrades")]
    public int generalDamageLevel = 0;
    public int generalDamagePerUpgrade = 10;

    [Header("Sword Upgrades")]
    public int swordDashDamageLevel = 0;
    public int swordDashDamagePerUpgrade = 10;

    public int swordUppercutDamageLevel = 0;
    public int swordUppercutDamagePerUpgrade = 15;

    public int swordUppercutEnergyLevel = 0;
    public int swordUppercutEnergyReductionPerUpgrade = 5;

    [Header("Gauntlet Upgrades")]
    public int gauntletShockwaveDamageLevel = 0;
    public int gauntletShockwaveDamagePerUpgrade = 20;

    public int gauntletLaunchDamageLevel = 0;
    public int gauntletLaunchDamagePerUpgrade = 25;

    public int gauntletLaunchEnergyLevel = 0;
    public int gauntletLaunchEnergyReductionPerUpgrade = 10;

    private void Awake()
    {
        if (instance == null) instance = this;
        else Destroy(gameObject);
    }

    // === General ===
    public float GetGeneralDamageBonus()
        => generalDamageLevel * generalDamagePerUpgrade;

    // === Sword Dash ===
    public float GetSwordDashBonus()
        => swordDashDamageLevel * swordDashDamagePerUpgrade;

    // === Sword Uppercut ===
    public float GetSwordUppercutBonus()
        => swordUppercutDamageLevel * swordUppercutDamagePerUpgrade;

    public float GetSwordUppercutEnergyReduction()
        => swordUppercutEnergyLevel * swordUppercutEnergyReductionPerUpgrade;

    // === Gauntlet Shockwave ===
    public float GetGauntletShockwaveBonus()
        => gauntletShockwaveDamageLevel * gauntletShockwaveDamagePerUpgrade;

    // === Gauntlet Launch ===
    public float GetGauntletLaunchBonus()
        => gauntletLaunchDamageLevel * gauntletLaunchDamagePerUpgrade;

    public float GetGauntletLaunchEnergyReduction()
        => gauntletLaunchEnergyLevel * gauntletLaunchEnergyReductionPerUpgrade;
}
