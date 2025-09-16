using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneTransition : MonoBehaviour
{
    [Header("Transition Settings")] 
    [SerializeField] private string nextSceneName;
    [SerializeField] private Transform startPoint;
    [SerializeField] private Vector2 exitDirection;
    [SerializeField] private float exitTime;
    [SerializeField] private bool needPress = false;
    [SerializeField] private bool isTriggered = false;
    [SerializeField] private bool dir = true;

    private void Start()
    {
        if (nextSceneName == SceneTransitionManager.instance.lastSceneName && SceneTransitionManager.instance.isTrasition == true)
        {
            SceneTransitionManager.instance.isTrasition = false;

            PlayerController.instance.transform.position = startPoint.position;

            //exitDirection = PlayerController.instance.spriteTransform.localScale;

            StartCoroutine(SceneTransitionManager.instance.MoveToNewScene(exitDirection, exitTime, dir));
        }
    }

    private void Update()
    {
        TransitionScene();
    }

    void TransitionScene()
    {
        if (needPress && isTriggered && Input.GetKey(KeyCode.UpArrow))
        {
            SceneTransitionManager.instance.isTrasition = true;

            SceneTransitionManager.instance.lastSceneName = SceneManager.GetActiveScene().name;

            SceneManager.LoadScene(nextSceneName);

            //StartCoroutine(FadeSceneTransitionManager.Instance.sceneFade.FadeAndLoadScene(SceneFade.FadeDirection.IN, nextSceneName));
        }
    }

    private void OnTriggerEnter2D(Collider2D _other)
    {
        if (!needPress)
        {
            if (_other.CompareTag("Player"))
            {
                SceneTransitionManager.instance.isTrasition = true;

                SceneTransitionManager.instance.lastSceneName = SceneManager.GetActiveScene().name;

                // PlayerController.Instance.cutScene = true;

                SceneManager.LoadScene(nextSceneName);

                //StartCoroutine(FadeSceneTransitionManager.Instance.sceneFade.FadeAndLoadScene(SceneFade.FadeDirection.IN, nextSceneName));
            }
        }
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            isTriggered = true;
        }
    }
    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            isTriggered = false;
        }
    }
}
