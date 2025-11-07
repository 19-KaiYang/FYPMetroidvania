using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuUI : MonoBehaviour
{
    private static MainMenuUI instance;

    [SerializeField] private GameObject pausePanel;
    [SerializeField] private GameObject settingPanel;
    [SerializeField] private GameObject MainMenu;
    [SerializeField] private GameObject continueButton; 

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
    
        if (scene.name == "MainMenu")
        {
            MainMenu.SetActive(true);
            pausePanel.SetActive(false);
            settingPanel.SetActive(false);
            UpdateContinueButtonState();
        }
        else
        {
            
            MainMenu.SetActive(false);
            pausePanel.SetActive(false);
            settingPanel.SetActive(false);
        }
    }

    private void UpdateContinueButtonState()
    {
     
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
    public void ContinueGameButton()
    {
        string lastRoom = RoomSaveManager.GetLastSavedRoom();
        Debug.Log($"Loading saved room: {lastRoom}");

        // Hide main menu UI before loading
        MainMenu.SetActive(false);

        SceneManager.LoadScene(lastRoom);
    }
    public void NewGameButton()
    {
        RoomSaveManager.ClearSaveData();

        if (SceneTransitionManager.instance != null)
        {
            SceneTransitionManager.instance.roomIndex = 0;
            SceneTransitionManager.instance.currentSceneName = "GoblinCamp";
            SceneTransitionManager.instance.lastSceneName = "";
        }
        if (SceneTransitionManager.instance != null)
        {
            Destroy(SceneTransitionManager.instance.gameObject);
            SceneTransitionManager.instance = null;
        }

        // Hide UI and start game
        MainMenu.SetActive(false);
        Time.timeScale = 1f;

        Debug.Log("=== Starting New Game from Goblin Camp ===");
        SceneManager.LoadScene("Goblin Camp");
    }

    public void MainMenuButton()
    {
        var player = GameObject.FindWithTag("Player");
        if (player != null) Destroy(player);

        Time.timeScale = 1f;
        isPause = false;
        if (SceneTransitionManager.instance != null)
        {
            Destroy(SceneTransitionManager.instance.gameObject);
            SceneTransitionManager.instance = null;
        }


        SceneManager.LoadScene("MainMenu");
        Debug.Log("=== Returned to Main Menu ===");
    }

    public void StartGameBtn()
    {
        MainMenu.SetActive(false);
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