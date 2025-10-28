using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using System;

public class UpgradeSelectionUI : MonoBehaviour
{
    public static event Action OnUpgradeChosen;

    [Header("UI References")]
    public Button[] buttons;

    public TextMeshProUGUI[] nameLabels;

    public TextMeshProUGUI[] descriptionLabels;

    public Image[] icons;

    [Header("Upgrades Pool")]
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
        AudioManager.PlaySFX(SFXTYPE.UPGRADE_POPUP);
        ShowRandomUpgrades();
    }

    private void ShowRandomUpgrades()
    {
        List<Upgrade> options = new List<Upgrade>(upgradePool);

        // Remove upgrades already chosen
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

        // Populate UI slots
        for (int i = 0; i < buttons.Length; i++)
        {
            if (options.Count == 0)
            {
                nameLabels[i].text = "No More Upgrades";
                descriptionLabels[i].text = "";
                icons[i].enabled = false;

                buttons[i].onClick.RemoveAllListeners();
                buttons[i].interactable = false;
                continue;
            }

            // Pick random upgrade
            int index = UnityEngine.Random.Range(0, options.Count);
            Upgrade upgrade = options[index];
            options.RemoveAt(index);

            // Assign UI visuals
            nameLabels[i].text = !string.IsNullOrEmpty(upgrade.displayName) ? upgrade.displayName : upgrade.name;
            descriptionLabels[i].text = upgrade.description;
            if (icons != null && i < icons.Length && icons[i] != null)
            {
                icons[i].sprite = upgrade.icon;
                icons[i].enabled = (upgrade.icon != null);
            }

            // Assign button listener
            buttons[i].onClick.RemoveAllListeners();
            buttons[i].onClick.AddListener(() => SelectUpgrade(upgrade));
            buttons[i].interactable = true;
        }
    }

    private void SelectUpgrade(Upgrade upgrade)
    {
        // Assign to correct category
        if (upgrade is PixieWingsUpgrade || upgrade is FleetOfFootUpgrade)
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
                 upgrade.name.Contains("Rising Precision") ||
                 upgrade.name.Contains("Ferocious Fairy") ||
                 upgrade.name.Contains("Pixie Pellet") ||
                 upgrade.name.Contains("Sharp Winds"))
        {
            upgradeManager.MiscUpgrades.Add(upgrade);
            upgrade.OnApply(upgradeManager);
        }
        else
        {
            upgradeManager.AttackUpgrade = upgrade;
        }

        Time.timeScale = 1f;
        OnUpgradeChosen?.Invoke();
        Destroy(gameObject);
    }
}
