using UnityEngine;

public class UpgradeManager : MonoBehaviour
{
    public static UpgradeManager instance;

    [Header("General Upgrades")]
    public int generalDamageLevel = 0;
    public int generalDamagePerUpgrade = 10;

    // ======================
    // Sword Skills
    // ======================
    [Header("Sword Dash Upgrades")]
    public int swordDashDamageLevel = 0;
    public int swordDashDamagePerUpgrade = 10;

    public int swordDashEnergyLevel = 0;
    public int swordDashEnergyReductionPerUpgrade = 3;

    [Header("Sword Uppercut Upgrades")]
    public int swordUppercutDamageLevel = 0;
    public int swordUppercutDamagePerUpgrade = 15;

    public int swordUppercutEnergyLevel = 0;
    public int swordUppercutEnergyReductionPerUpgrade = 5;

    // ======================
    // Gauntlet Skills
    // ======================
    [Header("Gauntlet Shockwave Upgrades")]
    public int gauntletShockwaveDamageLevel = 0;
    public int gauntletShockwaveDamagePerUpgrade = 20;

    public int gauntletShockwaveEnergyLevel = 0;
    public int gauntletShockwaveEnergyReductionPerUpgrade = 5;

    [Header("Gauntlet Launch Upgrades")]
    public int gauntletLaunchDamageLevel = 0;
    public int gauntletLaunchDamagePerUpgrade = 25;

    public int gauntletLaunchEnergyLevel = 0;
    public int gauntletLaunchEnergyReductionPerUpgrade = 10;

    private void Awake()
    {
        if (instance == null) instance = this;
        else Destroy(gameObject);
    }

    // ======================
    // General
    // ======================
    public float GetGeneralDamageBonus()
        => generalDamageLevel * generalDamagePerUpgrade;

    // ======================
    // Sword Dash
    // ======================
    public float GetSwordDashBonus()
        => swordDashDamageLevel * swordDashDamagePerUpgrade;

    public float GetSwordDashEnergyReduction()
        => swordDashEnergyLevel * swordDashEnergyReductionPerUpgrade;

    // ======================
    // Sword Uppercut
    // ======================
    public float GetSwordUppercutBonus()
        => swordUppercutDamageLevel * swordUppercutDamagePerUpgrade;

    public float GetSwordUppercutEnergyReduction()
        => swordUppercutEnergyLevel * swordUppercutEnergyReductionPerUpgrade;

    // ======================
    // Gauntlet Shockwave
    // ======================
    public float GetGauntletShockwaveBonus()
        => gauntletShockwaveDamageLevel * gauntletShockwaveDamagePerUpgrade;

    public float GetGauntletShockwaveEnergyReduction()
        => gauntletShockwaveEnergyLevel * gauntletShockwaveEnergyReductionPerUpgrade;

    // ======================
    // Gauntlet Launch
    // ======================
    public float GetGauntletLaunchBonus()
        => gauntletLaunchDamageLevel * gauntletLaunchDamagePerUpgrade;

    public float GetGauntletLaunchEnergyReduction()
        => gauntletLaunchEnergyLevel * gauntletLaunchEnergyReductionPerUpgrade;
}
