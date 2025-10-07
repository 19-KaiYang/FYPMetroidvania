using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using System;

public class UpgradeSelectionUI : MonoBehaviour
{
    public static event Action OnUpgradeChosen; 

    public Button[] buttons;
    public TextMeshProUGUI[] labels;
    public List<Upgrade> upgradePool;

    private UpgradeManager upgradeManager;

    void Awake()
    {
        upgradeManager = FindAnyObjectByType<UpgradeManager>();
        gameObject.SetActive(false);
    }

    public void ShowMenu()
    {
        Time.timeScale = 0f;
        gameObject.SetActive(true);
        ShowRandomUpgrades();
    }

    private void ShowRandomUpgrades()
    {
        List<Upgrade> options = new List<Upgrade>(upgradePool);

        if (upgradeManager.AttackUpgrade != null)
            options.Remove(upgradeManager.AttackUpgrade);
        if (upgradeManager.SkillUpgrade != null)
            options.Remove(upgradeManager.SkillUpgrade);
        if (upgradeManager.SpiritUpgrade != null)
            options.Remove(upgradeManager.SpiritUpgrade);
        if (upgradeManager.MobilityUpgrade != null)
            options.Remove(upgradeManager.MobilityUpgrade);
        foreach (var misc in upgradeManager.MiscUpgrades)
            options.Remove(misc);

        for (int i = 0; i < buttons.Length; i++)
        {
            if (options.Count == 0)
            {
                labels[i].text = "No More Upgrades";
                buttons[i].onClick.RemoveAllListeners();
                buttons[i].interactable = false;
                continue;
            }

            int index = UnityEngine.Random.Range(0, options.Count);
            Upgrade upgrade = options[index];
            options.RemoveAt(index);

            labels[i].text = upgrade.name;

            buttons[i].onClick.RemoveAllListeners();
            buttons[i].onClick.AddListener(() => SelectUpgrade(upgrade));
            buttons[i].interactable = true;
        }
    }

    private void SelectUpgrade(Upgrade upgrade)
    {
        if (upgrade is PixieWingsUpgrade)
        {
            upgradeManager.MobilityUpgrade = upgrade;
            upgrade.OnApply(upgradeManager);
        }
        else if (upgrade.name.Contains("Skill"))
        {
            upgradeManager.SkillUpgrade = upgrade;
        }
        else if (upgrade.name.Contains("Spirit"))
        {
            upgradeManager.SpiritUpgrade = upgrade;
        }
        else if (upgrade.name.Contains("Dust Devil") ||
                 upgrade.name.Contains("Malicious Magic") ||
                 upgrade.name.Contains("Exploit Wounds") ||
                 upgrade.name.Contains("Rising Precision"))
        {
            upgradeManager.MiscUpgrades.Add(upgrade);
            upgrade.OnApply(upgradeManager);
        }
        else
        {
            upgradeManager.AttackUpgrade = upgrade;
        }

        // Resume
        Time.timeScale = 1f;

        // Fire event so SceneTransition can continue
        OnUpgradeChosen?.Invoke();

        Destroy(gameObject);
    }
}
