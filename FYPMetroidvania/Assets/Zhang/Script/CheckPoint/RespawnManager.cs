using UnityEngine;
using UnityEngine.SceneManagement;

public class RespawnManager : MonoBehaviour
{
    public static RespawnManager instance;
    [SerializeField] public string checkpointScene;
    [SerializeField] private Vector2 checkpointPos;

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
    void Start()
    {
        checkpointScene = SceneManager.GetActiveScene().name;
        checkpointPos = PlayerController.instance.transform.position;
    }

    void Update()
    {
        if(Input.GetKeyUp(KeyCode.R))
        {
            Respawn();
        }
    }

    public void SetCheckpoint(string sceneName, Vector2 pos)
    {
        checkpointScene = sceneName;
        checkpointPos = pos;
    }

    public void Respawn()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
        SceneManager.LoadScene(checkpointScene);

        //if (SceneManager.GetActiveScene().name != checkpointScene)
        //{
        //    SceneManager.sceneLoaded += OnSceneLoaded;
        //    SceneManager.LoadScene(checkpointScene);
        //}
        //else
        //{
        //    PlayerController.instance.transform.position = checkpointPos;
        //    SceneManager.LoadScene(checkpointScene);
        //}
    }
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == checkpointScene)
        {
            PlayerController.instance.transform.position = checkpointPos;
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }
    }
}
