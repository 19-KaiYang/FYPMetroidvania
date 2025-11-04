using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UpgradeDescriptionUI : MonoBehaviour
{
    private static UpgradeDescriptionUI instance;

    [Header("UI References")]
    [SerializeField] private GameObject panelBackground;
    [SerializeField] private GameObject upgradePanel;    
    [SerializeField] private GameObject skillPanel;      
    [SerializeField] private Transform contentParent;     
    [SerializeField] private GameObject upgradeEntryPrefab;

    [Header("Toggle Button")]
    [SerializeField] private Button toggleButton;         
    [SerializeField] private TextMeshProUGUI buttonText;  

    private UpgradeManager upgradeManager;
    private bool isOpen = false;
    private bool showingUpgrade = true; 

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else Destroy(gameObject);

        if (toggleButton)
            toggleButton.onClick.AddListener(TogglePanels);

    }

    void Start()
    {
        upgradeManager = FindAnyObjectByType<UpgradeManager>();

        // Hide everything at start
        if (panelBackground) panelBackground.SetActive(false);
        if (upgradePanel) upgradePanel.SetActive(false);
        if (skillPanel) skillPanel.SetActive(false);
        if(toggleButton) toggleButton.gameObject.SetActive(false);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.M))
            ToggleMenu();
    }

    // ---------- MENU OPEN/CLOSE ----------
    void ToggleMenu()
    {
        isOpen = !isOpen;

        if (panelBackground) panelBackground.SetActive(isOpen);
        if (toggleButton) toggleButton.gameObject.SetActive(isOpen); 

        if (isOpen)
        {
            Time.timeScale = 0f;
            showingUpgrade = true;
            UpdateButtonText();
            upgradePanel.SetActive(true);
            skillPanel.SetActive(false);
            RefreshUI();
        }
        else
        {
            Time.timeScale = 1f;
            panelBackground.SetActive(false);
            upgradePanel.SetActive(false);
            skillPanel.SetActive(false);
            ClearUI();
        }
    }

    // ---------- PANEL SWITCH ----------
    void TogglePanels()
    {
        showingUpgrade = !showingUpgrade;
        UpdateButtonText();

        if (showingUpgrade)
        {
            // Show upgrade panel
            skillPanel.SetActive(false);
            upgradePanel.SetActive(true);
            RefreshUI();
        }
        else
        {
            // Show skill panel
            upgradePanel.SetActive(false);
            ClearUI();
            skillPanel.SetActive(true);
        }
    }

    // ---------- UPDATE BUTTON LABEL ----------
    void UpdateButtonText()
    {
        if (buttonText)
            buttonText.text = showingUpgrade ? "Skill" : "Upgrade";
    }

    // ---------- UPGRADE LIST CREATION ----------
    void RefreshUI()
    {
        ClearUI();
        if (!upgradeManager || !contentParent || !upgradeEntryPrefab) return;

        List<Upgrade> allUpgrades = new List<Upgrade>();
        if (upgradeManager.AttackUpgrade)   allUpgrades.Add(upgradeManager.AttackUpgrade);
        if (upgradeManager.SkillUpgrade)    allUpgrades.Add(upgradeManager.SkillUpgrade);
        if (upgradeManager.SpiritUpgrade)   allUpgrades.Add(upgradeManager.SpiritUpgrade);
        if (upgradeManager.MobilityUpgrade) allUpgrades.Add(upgradeManager.MobilityUpgrade);
        allUpgrades.AddRange(upgradeManager.MiscUpgrades);

        foreach (var upgrade in allUpgrades)
        {
            GameObject entry = Instantiate(upgradeEntryPrefab, contentParent);
            var icon = entry.transform.Find("LeftGroup/Icon")?.GetComponent<Image>();
            var nameText = entry.transform.Find("LeftGroup/Name")?.GetComponent<TextMeshProUGUI>();
            var descText = entry.transform.Find("Description")?.GetComponent<TextMeshProUGUI>();

            if (upgrade.icon != null && icon != null)
                icon.sprite = upgrade.icon;
            if (nameText != null)
                nameText.text = upgrade.displayName;
            if (descText != null)
                descText.text = upgrade.description;
        }
    }

    void ClearUI()
    {
        if (!contentParent) return;
        for (int i = contentParent.childCount - 1; i >= 0; i--)
            Destroy(contentParent.GetChild(i).gameObject);
    }
}
