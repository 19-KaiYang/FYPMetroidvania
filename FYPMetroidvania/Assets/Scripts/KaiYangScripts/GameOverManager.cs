using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using UnityEngine.UI;

public class GameOverManager : MonoBehaviour
{
    public static GameOverManager Instance { get; private set; }

    private static string causeOfDeath = "Unknown";
    private static string lastSceneName;
    private static string enemyType = "Unknown";

    [Header("UI References")]
    public TextMeshProUGUI deathText;
    public Button mainMenuButton;
    public Animator canvasAnimator;

    [Header("Scene Names")]
    public string mainMenuSceneName = "MainMenu";

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {

        DisablePlayerUI();

        if (deathText != null)
        {
            deathText.text = $"You died to: {causeOfDeath}";
        }
        if (mainMenuButton != null)
        {
            mainMenuButton.onClick.AddListener(OnMainMenuClicked);
        }

        GameObject player = GameObject.FindWithTag("Player");
        if (player != null)
        {
            Destroy(player);
        }
    }

    public static void SetDeathInfo(string cause)
    {
        causeOfDeath = cause;
        lastSceneName = SceneManager.GetActiveScene().name;
    }

    public static void LoadGameOverScene()
    {
        SceneManager.LoadScene("GameOver");
    }

    private void OnRetryClicked()
    {
        Time.timeScale = 1f;
        if (!string.IsNullOrEmpty(lastSceneName))
        {
            SceneManager.LoadScene(lastSceneName);
        }
    }

    private void OnMainMenuClicked()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(mainMenuSceneName);
    }

    public static string GetFriendlyEnemyName(GameObject enemy)
    {
        if (enemy == null) return "Unknown";

        if (enemy.GetComponent<MeleeEnemy>() != null) return " Claw Knight";
        if (enemy.GetComponent<Spearman>() != null) return " Spear Knight";
        if (enemy.GetComponent<DaggerCultist>() != null) return " Truck Cultist";
        if (enemy.GetComponent<FlyEnemy>() != null) return " Bird Knight";
        if (enemy.GetComponent<TruckBoss>() != null) return " Truck Boss";

        return enemy.name.Replace("(Clone)", "").Trim();
    }

    void DisablePlayerUI()
    {
        GameObject canvas = GameObject.Find("FinalUpdatedCanvas");
        if (canvas != null)
        {
            canvas.SetActive(false);
        }

        GameObject UpgradeDescription = GameObject.Find("UpgradeDescriptionUI");
        if (UpgradeDescription != null)
        {
            UpgradeDescription.SetActive(false);
        }
    }
}