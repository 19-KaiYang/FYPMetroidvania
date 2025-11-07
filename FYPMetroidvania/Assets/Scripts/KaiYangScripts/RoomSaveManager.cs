using UnityEngine;
using UnityEngine.SceneManagement;

public class RoomSaveManager : MonoBehaviour
{
    private const string LAST_ROOM_KEY = "LastRoom";
    private const string HAS_SAVE_KEY = "HasSave";
    private const string UPGRADE_SAVE_KEY = "UpgradeSave";
    private const string ROOM_INDEX_KEY = "RoomIndex";

    [Header("Required Player UI Prefabs (Optional)")]
    [SerializeField] private GameObject finalUpdatedCanvasPrefab;
    [SerializeField] private GameObject upgradeDescriptionUIPrefab;
    [SerializeField] private GameObject sceneTransitionManagerPrefab;
    [SerializeField] private GameObject audioManagerPrefab;

    [Header("All Available Upgrades")]
    [SerializeField] private Upgrade[] allUpgrades;

    private static bool isChangingScene = false;

    private void Awake()
    {
        // Ensure player UI and managers exist when loading into any room
        EnsurePlayerUIExists();
        EnsureSceneTransitionManagerExists();
        EnsureAudioManagerExists();

        // Subscribe to scene loaded event
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // When a new scene loads, reset the flag
        isChangingScene = false;
    }

    private void Start()
    {
        Debug.Log($"=== RoomSaveManager Start - Scene: {SceneManager.GetActiveScene().name} ===");

        // Save the current room when entering it
        string currentScene = SceneManager.GetActiveScene().name;

        // Only save if it's a gameplay room (not MainMenu)
        if (currentScene != "MainMenu")
        {
            SaveCurrentRoom(currentScene);

            // Load upgrades if continuing - load immediately
            if (HasSaveData())
            {
                Debug.Log("Save data found, loading upgrades...");
                LoadUpgrades();
            }
            else
            {
                Debug.Log("No save data found, starting fresh");
            }
        }

        isChangingScene = false;
    }

    private void EnsurePlayerUIExists()
    {
        // Check if FinalUpdatedCanvas exists, if not, instantiate it
        if (GameObject.Find("FinalUpdatedCanvas") == null && finalUpdatedCanvasPrefab != null)
        {
            GameObject canvas = Instantiate(finalUpdatedCanvasPrefab);
            canvas.name = "FinalUpdatedCanvas";
            DontDestroyOnLoad(canvas);
            Debug.Log("Created missing FinalUpdatedCanvas");
        }

        // Check if UpgradeDescriptionUI exists, if not, instantiate it
        if (GameObject.Find("UpgradeDescriptionUI") == null && upgradeDescriptionUIPrefab != null)
        {
            GameObject ui = Instantiate(upgradeDescriptionUIPrefab);
            ui.name = "UpgradeDescriptionUI";
            DontDestroyOnLoad(ui);
            Debug.Log("Created missing UpgradeDescriptionUI");
        }
    }

    private void EnsureSceneTransitionManagerExists()
    {
        // Check if SceneTransitionManager exists, if not, instantiate it
        if (SceneTransitionManager.instance == null && sceneTransitionManagerPrefab != null)
        {
            GameObject manager = Instantiate(sceneTransitionManagerPrefab);
            manager.name = "SceneTransitionManager";
            DontDestroyOnLoad(manager);
            Debug.Log("Created missing SceneTransitionManager");

            // Load the saved room index if continuing
            if (HasSaveData())
            {
                int savedRoomIndex = PlayerPrefs.GetInt(ROOM_INDEX_KEY, 0);
                SceneTransitionManager.instance.roomIndex = savedRoomIndex;
                Debug.Log($"Loaded room index into SceneTransitionManager: {savedRoomIndex}");
            }
        }
    }

    private void EnsureAudioManagerExists()
    {
        // Check if AudioManager exists, if not, instantiate it
        if (AudioManager.instance == null && audioManagerPrefab != null)
        {
            GameObject manager = Instantiate(audioManagerPrefab);
            manager.name = "AudioManager";
            DontDestroyOnLoad(manager);
            Debug.Log("Created missing AudioManager");
        }
    }

    private void LoadUpgrades()
    {
        Debug.Log("=== STARTING UPGRADE LOAD ===");

        var player = GameObject.FindWithTag("Player");
        if (player == null)
        {
            Debug.LogWarning("Player not found in scene, cannot load upgrades");
            return;
        }
        Debug.Log($"Found player: {player.name}");

        var upgradeManager = player.GetComponent<UpgradeManager>();
        if (upgradeManager == null)
        {
            Debug.LogWarning("UpgradeManager not found on player");
            return;
        }
        Debug.Log("Found UpgradeManager component");

        string saveJson = PlayerPrefs.GetString(UPGRADE_SAVE_KEY, "");
        if (string.IsNullOrEmpty(saveJson))
        {
            Debug.Log("No upgrade save data found");
            return;
        }
        Debug.Log($"Loading upgrade data: {saveJson}");

        UpgradeSaveData saveData = JsonUtility.FromJson<UpgradeSaveData>(saveJson);

        // Load AttackUpgrade
        if (!string.IsNullOrEmpty(saveData.attackUpgradeName))
        {
            Debug.Log($"Attempting to load AttackUpgrade: {saveData.attackUpgradeName}");
            Upgrade upgrade = FindUpgradeByName(saveData.attackUpgradeName);
            if (upgrade != null)
            {
                upgradeManager.AttackUpgrade = upgrade;
                upgrade.OnApply(upgradeManager);
                Debug.Log($" Successfully loaded and applied AttackUpgrade: {saveData.attackUpgradeName}");
            }
            else
            {
                Debug.LogError($" Failed to find AttackUpgrade: {saveData.attackUpgradeName}");
            }
        }

        // Load SkillUpgrade
        if (!string.IsNullOrEmpty(saveData.skillUpgradeName))
        {
            Debug.Log($"Attempting to load SkillUpgrade: {saveData.skillUpgradeName}");
            Upgrade upgrade = FindUpgradeByName(saveData.skillUpgradeName);
            if (upgrade != null)
            {
                upgradeManager.SkillUpgrade = upgrade;
                upgrade.OnApply(upgradeManager);
                Debug.Log($" Successfully loaded and applied SkillUpgrade: {saveData.skillUpgradeName}");
            }
            else
            {
                Debug.LogError($" Failed to find SkillUpgrade: {saveData.skillUpgradeName}");
            }
        }

        // Load SpiritUpgrade
        if (!string.IsNullOrEmpty(saveData.spiritUpgradeName))
        {
            Debug.Log($"Attempting to load SpiritUpgrade: {saveData.spiritUpgradeName}");
            Upgrade upgrade = FindUpgradeByName(saveData.spiritUpgradeName);
            if (upgrade != null)
            {
                upgradeManager.SpiritUpgrade = upgrade;
                upgrade.OnApply(upgradeManager);
                Debug.Log($" Successfully loaded and applied SpiritUpgrade: {saveData.spiritUpgradeName}");
            }
            else
            {
                Debug.LogError($" Failed to find SpiritUpgrade: {saveData.spiritUpgradeName}");
            }
        }

        // Load MobilityUpgrade
        if (!string.IsNullOrEmpty(saveData.mobilityUpgradeName))
        {
            Debug.Log($"Attempting to load MobilityUpgrade: {saveData.mobilityUpgradeName}");
            Upgrade upgrade = FindUpgradeByName(saveData.mobilityUpgradeName);
            if (upgrade != null)
            {
                upgradeManager.MobilityUpgrade = upgrade;
                upgrade.OnApply(upgradeManager);
                Debug.Log($" Successfully loaded and applied MobilityUpgrade: {saveData.mobilityUpgradeName}");
            }
            else
            {
                Debug.LogError($" Failed to find MobilityUpgrade: {saveData.mobilityUpgradeName}");
            }
        }

        // Load MiscUpgrades
        Debug.Log($"Loading {saveData.miscUpgradeNames.Count} MiscUpgrades...");
        upgradeManager.MiscUpgrades.Clear();
        foreach (string upgradeName in saveData.miscUpgradeNames)
        {
            Debug.Log($"Attempting to load MiscUpgrade: {upgradeName}");
            Upgrade upgrade = FindUpgradeByName(upgradeName);
            if (upgrade != null)
            {
                upgradeManager.MiscUpgrades.Add(upgrade);
                upgrade.OnApply(upgradeManager);
                Debug.Log($" Successfully loaded and applied MiscUpgrade: {upgradeName}");
            }
            else
            {
                Debug.LogError($" Failed to find MiscUpgrade: {upgradeName}");
            }
        }

        // Load Player Stats
        Debug.Log("=== Loading Player Stats ===");

        var health = player.GetComponent<Health>();
        if (health != null)
        {
            health.maxHealth = saveData.maxHealth;
            health.currentHealth = saveData.currentHealth;
            Debug.Log($"Loaded Health: {saveData.currentHealth}/{saveData.maxHealth}");
        }

        var energy = player.GetComponent<EnergySystem>();
        if (energy != null)
        {
            energy.maxEnergy = saveData.maxEnergy;
            energy.SetCurrentEnergy(saveData.currentEnergy);
            Debug.Log($"Loaded Energy: {saveData.currentEnergy}/{saveData.maxEnergy}");
        }

        var spirit = player.GetComponent<SpiritGauge>();
        if (spirit != null)
        {
            spirit.maxSpirit = saveData.maxSpirit;
            spirit.SetCurrentSpirit(saveData.currentSpirit);
            Debug.Log($"Loaded Spirit: {saveData.currentSpirit}/{saveData.maxSpirit}");
        }

        Debug.Log("=== UPGRADE LOAD COMPLETE ===");
    }

    private Upgrade FindUpgradeByName(string upgradeName)
    {
        foreach (Upgrade upgrade in allUpgrades)
        {
            if (upgrade != null && upgrade.name == upgradeName)
            {
                return upgrade;
            }
        }
        Debug.LogWarning($"Upgrade not found: {upgradeName}");
        return null;
    }

    public static void SaveCurrentRoom(string roomName)
    {
        PlayerPrefs.SetString(LAST_ROOM_KEY, roomName);
        PlayerPrefs.SetInt(HAS_SAVE_KEY, 1);

        // Save the current room index
        if (SceneTransitionManager.instance != null)
        {
            PlayerPrefs.SetInt(ROOM_INDEX_KEY, SceneTransitionManager.instance.roomIndex);
            Debug.Log($"Saved room index: {SceneTransitionManager.instance.roomIndex}");
        }

        PlayerPrefs.Save();
        Debug.Log($"Saved room: {roomName}");
    }

    // Call this method BEFORE loading a new scene
    public static void PrepareForSceneChange()
    {
        if (!isChangingScene)
        {
            isChangingScene = true;
            Debug.Log("Preparing for scene change, saving upgrades...");
            SaveUpgrades();
        }
    }

    public static void SaveUpgrades()
    {
        Debug.Log("=== STARTING UPGRADE SAVE ===");

        var player = GameObject.FindWithTag("Player");
        if (player == null)
        {
            Debug.LogWarning("Cannot save upgrades: Player not found");
            return;
        }
        Debug.Log($"Found player: {player.name}");

        var upgradeManager = player.GetComponent<UpgradeManager>();
        if (upgradeManager == null)
        {
            Debug.LogWarning("Cannot save upgrades: UpgradeManager not found");
            return;
        }
        Debug.Log("Found UpgradeManager component");

        UpgradeSaveData saveData = new UpgradeSaveData();

        // Save each upgrade by name
        if (upgradeManager.AttackUpgrade != null)
        {
            saveData.attackUpgradeName = upgradeManager.AttackUpgrade.name;
            Debug.Log($"Saving AttackUpgrade: {saveData.attackUpgradeName}");
        }
        else
        {
            Debug.Log("AttackUpgrade is null, not saving");
        }

        if (upgradeManager.SkillUpgrade != null)
        {
            saveData.skillUpgradeName = upgradeManager.SkillUpgrade.name;
            Debug.Log($"Saving SkillUpgrade: {saveData.skillUpgradeName}");
        }
        else
        {
            Debug.Log("SkillUpgrade is null, not saving");
        }

        if (upgradeManager.SpiritUpgrade != null)
        {
            saveData.spiritUpgradeName = upgradeManager.SpiritUpgrade.name;
            Debug.Log($"Saving SpiritUpgrade: {saveData.spiritUpgradeName}");
        }
        else
        {
            Debug.Log("SpiritUpgrade is null, not saving");
        }

        if (upgradeManager.MobilityUpgrade != null)
        {
            saveData.mobilityUpgradeName = upgradeManager.MobilityUpgrade.name;
            Debug.Log($"Saving MobilityUpgrade: {saveData.mobilityUpgradeName}");
        }
        else
        {
            Debug.Log("MobilityUpgrade is null, not saving");
        }

        // Save misc upgrades
        Debug.Log($"Found {upgradeManager.MiscUpgrades.Count} MiscUpgrades");
        foreach (Upgrade misc in upgradeManager.MiscUpgrades)
        {
            if (misc != null)
            {
                saveData.miscUpgradeNames.Add(misc.name);
                Debug.Log($"Saving MiscUpgrade: {misc.name}");
            }
        }

        // Save Player Stats
        Debug.Log("=== Saving Player Stats ===");

        var health = player.GetComponent<Health>();
        if (health != null)
        {
            saveData.maxHealth = health.maxHealth;
            saveData.currentHealth = health.currentHealth;
            Debug.Log($"Saved Health: {saveData.currentHealth}/{saveData.maxHealth}");
        }

        var energy = player.GetComponent<EnergySystem>();
        if (energy != null)
        {
            saveData.maxEnergy = energy.GetMaxEnergy();
            saveData.currentEnergy = energy.GetCurrentEnergy();
            Debug.Log($"Saved Energy: {saveData.currentEnergy}/{saveData.maxEnergy}");
        }

        var spirit = player.GetComponent<SpiritGauge>();
        if (spirit != null)
        {
            saveData.maxSpirit = spirit.GetMaxSpirit();
            saveData.currentSpirit = spirit.GetCurrentSpirit();
            Debug.Log($"Saved Spirit: {saveData.currentSpirit}/{saveData.maxSpirit}");
        }

        string json = JsonUtility.ToJson(saveData);
        PlayerPrefs.SetString(UPGRADE_SAVE_KEY, json);
        PlayerPrefs.Save();
        Debug.Log($"=== UPGRADE SAVE COMPLETE ===");
        Debug.Log($"Saved JSON: {json}");
    }

    public static string GetLastSavedRoom()
    {
        if (HasSaveData())
        {
            return PlayerPrefs.GetString(LAST_ROOM_KEY, "Goblin Camp");
        }
        return "Goblin Camp"; // Default starting room
    }

    public static bool HasSaveData()
    {
        return PlayerPrefs.GetInt(HAS_SAVE_KEY, 0) == 1;
    }

    public static void ClearSaveData()
    {
        PlayerPrefs.DeleteKey(LAST_ROOM_KEY);
        PlayerPrefs.DeleteKey(HAS_SAVE_KEY);
        PlayerPrefs.DeleteKey(UPGRADE_SAVE_KEY);
        PlayerPrefs.DeleteKey(ROOM_INDEX_KEY);
        PlayerPrefs.Save();
        Debug.Log("Save data cleared");
    }
}