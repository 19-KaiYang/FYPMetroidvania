using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuUI : MonoBehaviour
{
    private static MainMenuUI instance;

    [SerializeField] private GameObject pausePanel;
    [SerializeField] private GameObject settingPanel;
    [SerializeField] private GameObject MainMenu;
    [SerializeField] private GameObject continueButton; // Add this to enable/disable the continue button

    private bool isPause = false;
    private bool otherPanel = false;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else Destroy(gameObject);
    }

    private void Start()
    {
        // Only update button state if we're in the main menu
        if (SceneManager.GetActiveScene().name == "MainMenu")
        {
            UpdateContinueButtonState();
        }
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Update continue button state when main menu loads
        if (scene.name == "MainMenu")
        {
            MainMenu.SetActive(true);
            pausePanel.SetActive(false);
            settingPanel.SetActive(false);
            UpdateContinueButtonState();
        }
        else
        {
            // Hide main menu UI when in gameplay scenes
            MainMenu.SetActive(false);
            pausePanel.SetActive(false);
            settingPanel.SetActive(false);
        }
    }

    private void UpdateContinueButtonState()
    {
        // Enable/disable the continue button based on whether save data exists
        if (continueButton != null)
        {
            continueButton.SetActive(RoomSaveManager.HasSaveData());
        }
    }

    private void Update()
    {
        if (Input.GetKeyUp(KeyCode.Escape))
        {
            string currentScene = SceneManager.GetActiveScene().name;

            if (currentScene != "MainMenu")
            {
                if (isPause) Resume();
                else PauseGame();
            }
        }
    }

    public void PauseGame()
    {
        pausePanel.SetActive(true);
        Time.timeScale = 0f;
        isPause = true;
    }

    public void Resume()
    {
        if (!pausePanel.activeInHierarchy) return;

        pausePanel.SetActive(false);
        Time.timeScale = 1f;
        isPause = false;
    }

    public void ContinueButton()
    {
        pausePanel.SetActive(false);
        Time.timeScale = 1f;
        isPause = false;
    }

    // NEW: Continue button for main menu (loads last saved room)
    public void ContinueGameButton()
    {
        string lastRoom = RoomSaveManager.GetLastSavedRoom();
        Debug.Log($"Loading saved room: {lastRoom}");

        // Hide main menu UI before loading
        MainMenu.SetActive(false);

        SceneManager.LoadScene(lastRoom);
    }

    // NEW: Start new game button (clears save and starts from beginning)
    public void NewGameButton()
    {
        RoomSaveManager.ClearSaveData();

        // Hide main menu UI before loading
        MainMenu.SetActive(false);

        SceneManager.LoadScene("Goblin Camp"); // Your starting room
    }

    public void MainMenuButton()
    {
        var player = GameObject.FindWithTag("Player");
        if (player != null) Destroy(player);

        Time.timeScale = 1f;
        isPause = false;
        SceneManager.LoadScene("MainMenu");

        // Update continue button when returning to main menu
        UpdateContinueButtonState();
    }

    public void StartGameBtn()
    {
        // Hide main menu UI before loading
        MainMenu.SetActive(false);

        // This can now call NewGameButton or you can keep it as is
        SceneManager.LoadScene("Goblin Camp");
    }

    public void Setting_BackButton()
    {
        settingPanel.SetActive(false);

        string currentScene = SceneManager.GetActiveScene().name;

        if (currentScene != "MainMenu") pausePanel.SetActive(true);
        else MainMenu.SetActive(true);
    }

    public void EndGame()
    {
        Application.Quit();
    }
}