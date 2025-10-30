using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UpgradeDescriptionUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject panel;
    [SerializeField] private Transform contentParent;
    [SerializeField] private GameObject upgradeEntryPrefab;
    [SerializeField] private GameObject Background;
    [SerializeField] private GameObject UpgradeParent;


    private UpgradeManager upgradeManager;
    private bool isOpen = false;

    void Start()
    {
        upgradeManager = FindAnyObjectByType<UpgradeManager>();
        panel.SetActive(false);
        if (Background != null)
            Background.SetActive(false);

        if (UpgradeParent != null)
            UpgradeParent.SetActive(false);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.M))
        {
            ToggleMenu();
        }
    }

    void ToggleMenu()
    {
        isOpen = !isOpen;
        panel.SetActive(isOpen);
        Background.SetActive(true);
        UpgradeParent.SetActive(true);

        if (isOpen)
        {
            Time.timeScale = 0f;
            RefreshUI();
        }
        else
        {
            Time.timeScale = 1f;
            ClearUI();
            Background.SetActive(false);
            UpgradeParent.SetActive(false);
        }
    }

    void RefreshUI()
    {
        ClearUI();

        List<Upgrade> allUpgrades = new List<Upgrade>();
        if (upgradeManager.AttackUpgrade) allUpgrades.Add(upgradeManager.AttackUpgrade);
        if (upgradeManager.SkillUpgrade) allUpgrades.Add(upgradeManager.SkillUpgrade);
        if (upgradeManager.SpiritUpgrade) allUpgrades.Add(upgradeManager.SpiritUpgrade);
        if (upgradeManager.MobilityUpgrade) allUpgrades.Add(upgradeManager.MobilityUpgrade);
        allUpgrades.AddRange(upgradeManager.MiscUpgrades);

        foreach (var upgrade in allUpgrades)
        {
            GameObject entry = Instantiate(upgradeEntryPrefab, contentParent);
            var icon = entry.transform.Find("LeftGroup/Icon").GetComponent<Image>();
            var nameText = entry.transform.Find("LeftGroup/Name").GetComponent<TextMeshProUGUI>();
            var descText = entry.transform.Find("Description").GetComponent<TextMeshProUGUI>();


            if (upgrade.icon != null)
                icon.sprite = upgrade.icon;
            nameText.text = upgrade.displayName;
            descText.text = upgrade.description;
        }
    }

    void ClearUI()
    {
        foreach (Transform child in contentParent)
        {
            Destroy(child.gameObject);
        }
    }
}
