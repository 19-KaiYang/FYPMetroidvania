using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class UpgradeSaveData
{
    // Upgrades
    public string attackUpgradeName;
    public string skillUpgradeName;
    public string spiritUpgradeName;
    public string mobilityUpgradeName;
    public List<string> miscUpgradeNames = new List<string>();

    // Player Stats
    public float currentHealth;
    public float maxHealth;
    public float currentEnergy;
    public float maxEnergy;
    public float currentSpirit;
    public float maxSpirit;

    public UpgradeSaveData()
    {
        attackUpgradeName = "";
        skillUpgradeName = "";
        spiritUpgradeName = "";
        mobilityUpgradeName = "";
        miscUpgradeNames = new List<string>();

        // Default stats (will be overwritten when saving)
        currentHealth = 100f;
        maxHealth = 100f;
        currentEnergy = 100f;
        maxEnergy = 100f;
        currentSpirit = 100f;
        maxSpirit = 100f;
    }
}