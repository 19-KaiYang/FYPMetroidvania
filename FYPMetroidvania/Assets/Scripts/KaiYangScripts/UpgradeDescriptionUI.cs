using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class UpgradeDescriptionUI : MonoBehaviour
{
    private static UpgradeDescriptionUI instance;

    [Header("UI References")]
    [SerializeField] private GameObject panelBackground;
    [SerializeField] private GameObject scrollBackdrop;
    [SerializeField] private GameObject upgradePanel;
    [SerializeField] private GameObject skillPanel;
    [SerializeField] private Transform contentParent;
    [SerializeField] private GameObject upgradeEntryPrefab;

    [Header("Tab Buttons")]
    [SerializeField] private Button upgradeTabButton;
    [SerializeField] private Button skillTabButton;
    [SerializeField] private Color selectedTabColor = Color.white;
    [SerializeField] private Color unselectedTabColor = Color.gray;

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

        // Hook up both tab buttons
        if (upgradeTabButton)
            upgradeTabButton.onClick.AddListener(ShowUpgradeTab);
        if (skillTabButton)
            skillTabButton.onClick.AddListener(ShowSkillTab);

        // Subscribe to scene loaded to refresh UpgradeManager reference
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Find the new UpgradeManager when a scene loads
        FindUpgradeManager();

        if (scene.name == "MainMenu")
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        FindUpgradeManager();

        // Hide everything at start
        if (panelBackground) panelBackground.SetActive(false);
        if (scrollBackdrop) scrollBackdrop.SetActive(false);
        if (upgradePanel) upgradePanel.SetActive(false);
        if (skillPanel) skillPanel.SetActive(false);
        if (upgradeTabButton) upgradeTabButton.gameObject.SetActive(false);
        if (skillTabButton) skillTabButton.gameObject.SetActive(false);
    }

    // Helper method to find the current UpgradeManager
    void FindUpgradeManager()
    {
        upgradeManager = FindAnyObjectByType<UpgradeManager>();
        if (upgradeManager != null)
        {
            Debug.Log($"UpgradeDescriptionUI found UpgradeManager on: {upgradeManager.gameObject.name}");
        }
        else
        {
            Debug.LogWarning("UpgradeDescriptionUI could not find UpgradeManager");
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.M))
            ToggleMenu();
    }

    // ---------- MENU OPEN/CLOSE ----------
    void ToggleMenu()
    {
        // Refresh UpgradeManager reference in case it changed
        if (upgradeManager == null)
        {
            FindUpgradeManager();
        }

        isOpen = !isOpen;

        if (panelBackground) panelBackground.SetActive(isOpen);
        if (scrollBackdrop) scrollBackdrop.SetActive(isOpen);
        if (upgradeTabButton) upgradeTabButton.gameObject.SetActive(isOpen);
        if (skillTabButton) skillTabButton.gameObject.SetActive(isOpen);

        if (isOpen)
        {
            Time.timeScale = 0f;
            showingUpgrade = true;
            ShowUpgradeTab(); // Start with upgrade tab selected
        }
        else
        {
            Time.timeScale = 1f;
            panelBackground.SetActive(false);
            scrollBackdrop.SetActive(false);
            upgradePanel.SetActive(false);
            skillPanel.SetActive(false);
            ClearUI();

            // Hide tooltip when closing menu
            if (TooltipSystem.Instance != null)
                TooltipSystem.Instance.HideTooltip();
        }
    }

    // ---------- TAB SWITCHING ----------
    void ShowUpgradeTab()
    {
        showingUpgrade = true;

        // Switch panels
        skillPanel.SetActive(false);
        upgradePanel.SetActive(true);

        // Update tab button visuals
        UpdateTabVisuals();

        // Refresh upgrade list
        RefreshUI();
    }

    void ShowSkillTab()
    {
        showingUpgrade = false;

        // Switch panels
        upgradePanel.SetActive(false);
        ClearUI();
        skillPanel.SetActive(true);

        // Update tab button visuals
        UpdateTabVisuals();
    }

    // ---------- UPDATE TAB BUTTON VISUALS ----------
    void UpdateTabVisuals()
    {
        if (upgradeTabButton)
        {
            var colors = upgradeTabButton.colors;
            Color targetColor = showingUpgrade ? selectedTabColor : unselectedTabColor;
            colors.normalColor = targetColor;
            colors.highlightedColor = targetColor;
            colors.selectedColor = targetColor; 
            colors.pressedColor = targetColor;  
            upgradeTabButton.colors = colors;
            upgradeTabButton.targetGraphic.color = targetColor;
        }

        if (skillTabButton)
        {
            var colors = skillTabButton.colors;
            Color targetColor = showingUpgrade ? unselectedTabColor : selectedTabColor;
            colors.normalColor = targetColor;
            colors.highlightedColor = targetColor;
            colors.selectedColor = targetColor; // Add this line
            colors.pressedColor = targetColor;  // Optional: also set pressed color
            skillTabButton.colors = colors;
            skillTabButton.targetGraphic.color = targetColor;
        }
    }

    // ---------- UPGRADE LIST CREATION ----------
    void RefreshUI()
    {
        ClearUI();
        if (!upgradeManager || !contentParent || !upgradeEntryPrefab) return;

        List<Upgrade> allUpgrades = new List<Upgrade>();
        if (upgradeManager.AttackUpgrade) allUpgrades.Add(upgradeManager.AttackUpgrade);
        if (upgradeManager.SkillUpgrade) allUpgrades.Add(upgradeManager.SkillUpgrade);
        if (upgradeManager.SpiritUpgrade) allUpgrades.Add(upgradeManager.SpiritUpgrade);
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
            {
                descText.text = upgrade.description;

                // Add TooltipTrigger component to enable tooltips
                if (descText.GetComponent<TooltipTrigger>() == null)
                {
                    descText.gameObject.AddComponent<TooltipTrigger>();
                }
            }
        }
    }

    void ClearUI()
    {
        if (!contentParent) return;
        for (int i = contentParent.childCount - 1; i >= 0; i--)
            Destroy(contentParent.GetChild(i).gameObject);
    }
}