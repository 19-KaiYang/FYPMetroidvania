using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

public class SceneTransitionManager : MonoBehaviour
{
    public static SceneTransitionManager instance { get; private set; }

    [SerializeField] public string lastSceneName;
    [SerializeField] public string currentSceneName;
    [SerializeField] public bool isTrasition = false;

    private void Awake()
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
        currentSceneName = SceneManager.GetActiveScene().name;
    }

    void Update()
    {
        
    }

    public IEnumerator MoveToNewScene(Vector2 exitDir, float delay, bool _dir)
    {
        PlayerController player = PlayerController.instance;
        Rigidbody2D rb = player.rb;

        if (exitDir.y > 0)
        {
            rb.linearVelocity = 10 * exitDir;
        }

        if (exitDir.x != 0)
        {
            if (_dir)
            {
                exitDir = PlayerController.instance.spriteTransform.localScale;
            }
            

            player.moveInput.x = exitDir.x > 0 ? 1 : -1;

            //rb.linearVelocity = new Vector2(exitDir.x * 15, rb.linearVelocity.y);
        }

        if ((exitDir.x > 0 && !player.facingRight) || (exitDir.x < 0 && player.facingRight))
        {
            player.Flip();
        }

        yield return new WaitForSeconds(delay);

        player.moveInput.x = 0;
    }


    #region get current scene
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
        currentSceneName = scene.name;
    }
    #endregion
}
