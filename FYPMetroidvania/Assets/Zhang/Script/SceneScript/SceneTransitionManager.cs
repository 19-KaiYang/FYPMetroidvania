using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SceneTransitionManager : MonoBehaviour
{
    public static SceneTransitionManager instance;

    [SerializeField] public string lastSceneName;
    [SerializeField] public string currentSceneName;
    [SerializeField] public bool isTrasition = false;

    [Header("fade")]
    [SerializeField] private float fadeTime;
    [SerializeField] private float fadeOutTime;
    [SerializeField] private Image fadeImage;

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
    public enum FadeDirection
    {
        IN, OUT
    }
    public IEnumerator FadeAndLoadScene(FadeDirection _fadeDir, string _sceneName)
    {
        //fadeImage.enabled = true;
        yield return Fade(_fadeDir);
        SceneManager.LoadScene(_sceneName);
    }
    public IEnumerator Fade(FadeDirection _fadeDir)
    {
        float _startAlpha = _fadeDir == FadeDirection.OUT ? 1 : 0;
        float _endAlpha = _fadeDir == FadeDirection.OUT ? 0 : 1;

        if (_fadeDir == FadeDirection.OUT)
        {
            yield return new WaitForSeconds(fadeOutTime);
            while (_startAlpha >= _endAlpha)
            {
                SetColorImage(ref _startAlpha, _fadeDir);
                yield return null;
            }
            //fadeImage.enabled = false;
        }
        else
        {
            //fadeImage.enabled = true;
            while (_startAlpha <= _endAlpha)
            {
                SetColorImage(ref _startAlpha, _fadeDir);
                yield return null;
            }
        }
    }
    void SetColorImage(ref float _alpha, FadeDirection _fadeDir)
    {
        fadeImage.color = new Color(fadeImage.color.r,
                                    fadeImage.color.g,
                                    fadeImage.color.b, _alpha);

        _alpha += Time.deltaTime * (1 / fadeTime) * (_fadeDir == FadeDirection.OUT ? -1 : 1);
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
