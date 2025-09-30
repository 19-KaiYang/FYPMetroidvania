using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using NUnit.Framework;
using System.Collections.Generic;

public class SceneTransitionManager : MonoBehaviour
{
    public static SceneTransitionManager instance;

    [SerializeField] public string lastSceneName;
    [SerializeField] public string currentSceneName;
    [SerializeField] public bool isTrasition = false;

    [Header("fade")]
    [SerializeField] private float fadeTime;
    [SerializeField] private Image fadeImage;

    [Header("Room Handling")]
    public List<string> rooms;
    [SerializeField] ProgressionData progressionData;
    public int roomIndex = 0;
    public static System.Action<string> roomLoaded;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
            SceneManager.sceneLoaded += OnSceneLoaded;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        currentSceneName = SceneManager.GetActiveScene().name;
        roomIndex = 0;
        ShuffleRooms();
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
        fadeImage.enabled = true;
        //string random = GetRandomRoom();
        //if (random != null) _sceneName = random;
        if (roomIndex < rooms.Count)
        {
            _sceneName = rooms[roomIndex];
            roomIndex++;
        }
        if (_sceneName != null)
        {
            yield return Fade(_fadeDir);
            SceneManager.LoadScene(_sceneName);
        }
    }
    public IEnumerator Fade(FadeDirection _fadeDir)
    {
        float startAlpha = _fadeDir == FadeDirection.OUT ? 1f : 0f;
        float endAlpha = _fadeDir == FadeDirection.OUT ? 0f : 1f;

        float t = 0f;
        while (t < fadeTime)
        {
            t += Time.deltaTime;

            float alpha = Mathf.Lerp(startAlpha, endAlpha, t / fadeTime);

            fadeImage.color = new Color(fadeImage.color.r,
                                        fadeImage.color.g,
                                        fadeImage.color.b,
                                        alpha);

            yield return null;
        }
        fadeImage.color = new Color(fadeImage.color.r,
                                fadeImage.color.g,
                                fadeImage.color.b,
                                endAlpha);
        }
        void SetColorImage(ref float _alpha, FadeDirection _fadeDir)
        {
            fadeImage.color = new Color(fadeImage.color.r,
                                        fadeImage.color.g,
                                        fadeImage.color.b, _alpha);

            _alpha += Time.deltaTime * (1 / fadeTime) * (_fadeDir == FadeDirection.OUT ? -1 : 1);
        }

    public IEnumerator MoveToNewScene(Vector2 exitDir, float _jumpForce, float delay, bool _dir)
    {
        PlayerController player = PlayerController.instance;
        Rigidbody2D rb = player.rb;

        if (exitDir.y > 0)
        {
            player.SetVelocity(_jumpForce * exitDir);
        }

        if (exitDir.x != 0)
        {
            if (_dir)
            {
                exitDir.x = PlayerController.instance.spriteTransform.localScale.x > 0 ? 1 : -1;
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

    private string GetRandomRoom()
    {

        if(progressionData == null) return null;
        if (progressionData.rooms.Count < 2) return null;

        //string sceneName = "";
        while (true)
        {
            string scene = progressionData.rooms[Random.Range(0, progressionData.rooms.Count)];
            if(scene != SceneManager.GetActiveScene().name)
            {
                return scene;
            }
        }
    }

    void ShuffleRooms()
    {
        rooms = new List<string>(progressionData.rooms);
        for (int i = 0; i < rooms.Count; i++)
        {
            string temp = rooms[i];
            if (temp == currentSceneName)
            {
                rooms.Remove(temp);
                continue;
            }
            int randomIndex = Random.Range(i, rooms.Count);
            rooms[i] = rooms[randomIndex];
            rooms[randomIndex] = temp;
        }
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
        roomLoaded.Invoke(currentSceneName);
    }
    #endregion
}
