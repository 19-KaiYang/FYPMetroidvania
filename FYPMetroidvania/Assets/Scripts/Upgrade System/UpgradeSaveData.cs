using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class UpgradeSaveData
{
    public string attackUpgradeName;
    public string skillUpgradeName;
    public string spiritUpgradeName;
    public string mobilityUpgradeName;
    public List<string> miscUpgradeNames = new List<string>();

    public UpgradeSaveData()
    {
        attackUpgradeName = "";
        skillUpgradeName = "";
        spiritUpgradeName = "";
        mobilityUpgradeName = "";
        miscUpgradeNames = new List<string>();
    }
}