using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuUI : MonoBehaviour
{
    private static MainMenuUI instance;

    [SerializeField] private GameObject pausePanel;
    [SerializeField] private GameObject settingPanel;
    [SerializeField] private GameObject MainMenu;

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
    public void MainMenuButton()
    {
        var player = GameObject.FindWithTag("Player");
        if (player != null) Destroy(player);

        Time.timeScale = 1f;
        isPause = false;
        SceneManager.LoadScene("MainMenu");
    }
    public void StartGameBtn()
    {
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
