using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UpgradeSelectionUI : MonoBehaviour
{
    [Header("UI References")]
    public TextMeshProUGUI[] optionLabels;   // Assign the 3 "New Text" TMP texts
    public Button[] optionButtons;           // Assign the 3 "Select" buttons

    [Header("Upgrade Pool")]
    public Upgrade[] allUpgrades;            // Drag all your Upgrade assets here

    private UpgradeManager upgradeManager;
    private List<Upgrade> chosenUpgrades = new List<Upgrade>();

    void Start()
    {
        upgradeManager = PlayerController.instance.GetComponent<UpgradeManager>();
        ShowRandomUpgrades();
    }

    void ShowRandomUpgrades()
    {
        chosenUpgrades.Clear();

        // Pick 3 random upgrades
        List<Upgrade> pool = new List<Upgrade>(allUpgrades);
        for (int i = 0; i < 3 && pool.Count > 0; i++)
        {
            int idx = Random.Range(0, pool.Count);
            Upgrade picked = pool[idx];
            chosenUpgrades.Add(picked);
            pool.RemoveAt(idx);

            optionLabels[i].text = picked.name; // Show upgrade name

            int capturedIndex = i; // capture for closure
            optionButtons[i].onClick.RemoveAllListeners();
            optionButtons[i].onClick.AddListener(() =>
            {
                SelectUpgrade(chosenUpgrades[capturedIndex]);
            });
        }
    }

    void SelectUpgrade(Upgrade upgrade)
    {
        // Decide where this upgrade belongs
        if (upgrade is PixieWingsUpgrade)
        {
            // Mobility (only one)
            upgradeManager.MobilityUpgrade = upgrade;
            Debug.Log("Selected Mobility Upgrade: " + upgrade.name);
        }
        else if (upgrade is PixieDustAmp
                 || upgrade.name.Contains("Dust Devil")
                 || upgrade.name.Contains("Malicious Magic")
                 || upgrade.name.Contains("Exploit Wounds")
                 || upgrade.name.Contains("Rising Precision"))
        {
            // Misc (multiple allowed)
            upgradeManager.MiscUpgrades.Add(upgrade);
            Debug.Log("Selected Misc Upgrade: " + upgrade.name);
        }
        else if (upgrade.name.Contains("Skill"))
        {
            // Skills (only one)
            upgradeManager.SkillUpgrade = upgrade;
            Debug.Log("Selected Skill Upgrade: " + upgrade.name);
        }
        else if (upgrade.name.Contains("Spirit"))
        {
            // Spirit Attacks (only one)
            upgradeManager.SpiritUpgrade = upgrade;
            Debug.Log("Selected Spirit Upgrade: " + upgrade.name);
        }
        else
        {
            // Default to Attack
            upgradeManager.AttackUpgrade = upgrade;
            Debug.Log("Selected Attack Upgrade: " + upgrade.name);
        }

        // Apply it
        upgrade.OnApply(upgradeManager);

        // Hide the UI
        gameObject.SetActive(false);
    }


}
