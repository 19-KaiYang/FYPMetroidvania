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
        if (upgrade is PixieWingsUpgrade)
        {
            upgradeManager.MobilityUpgrade = upgrade;
            Debug.Log("Selected Mobility Upgrade: " + upgrade.name);
        }
        else
        {
            upgradeManager.AttackUpgrade = upgrade;
            Debug.Log("Selected Attack Upgrade: " + upgrade.name);
        }

        upgrade.OnApply(upgradeManager);

        // Hide the UI after picking
        gameObject.SetActive(false);
    }
}
