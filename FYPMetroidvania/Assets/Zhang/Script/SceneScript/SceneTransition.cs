using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneTransition : MonoBehaviour
{
    [Header("Transition Settings")] 
    [SerializeField] private string nextSceneName;
    [SerializeField] private Transform startPoint;
    [SerializeField] private Vector2 exitDirection;
    [SerializeField] private float exitTime;
    [SerializeField] private float jumpForce;
    [SerializeField] private bool needPress = false;
    [SerializeField] private bool isTriggered = false;
    [SerializeField] private bool dir = true;

    [Header("Upgrade Menu")]
    [SerializeField] private GameObject upgradeMenuPrefab;

    private void Start()
    {
        if (nextSceneName == SceneTransitionManager.instance.lastSceneName && SceneTransitionManager.instance.isTrasition == true)
        {
            SceneTransitionManager.instance.isTrasition = false;

            PlayerController.instance.transform.position = startPoint.position;

            //exitDirection = PlayerController.instance.spriteTransform.localScale;

            StartCoroutine(SceneTransitionManager.instance.MoveToNewScene(exitDirection, jumpForce, exitTime, dir));
            StartCoroutine(SceneTransitionManager.instance.Fade(SceneTransitionManager.FadeDirection.OUT));
        }
    }

    private void OnEnable()
    {
        SceneTransitionManager.roomLoaded += SetPlayerSpawnPos;
    }
    private void OnDisable()
    {
        SceneTransitionManager.roomLoaded -= SetPlayerSpawnPos;
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

            //SceneManager.LoadScene(nextSceneName);
            StartCoroutine(SceneTransitionManager.instance.FadeAndLoadScene(SceneTransitionManager.FadeDirection.IN, nextSceneName));
        }
    }
    void SetPlayerSpawnPos(string sceneName)
    {
        SceneTransitionManager.instance.isTrasition = false;

        PlayerController.instance.transform.position = startPoint.position;

        StartCoroutine(SceneTransitionManager.instance.MoveToNewScene(exitDirection, jumpForce, exitTime, dir));
        StartCoroutine(SceneTransitionManager.instance.Fade(SceneTransitionManager.FadeDirection.OUT));
    }

    private void HandleUpgradeChosen()
    {
        UpgradeSelectionUI.OnUpgradeChosen -= HandleUpgradeChosen;

        SceneTransitionManager.instance.isTrasition = true;
        SceneTransitionManager.instance.lastSceneName = SceneManager.GetActiveScene().name;

        StartCoroutine(SceneTransitionManager.instance.FadeAndLoadScene(SceneTransitionManager.FadeDirection.IN, nextSceneName));
    }

    private void OnTriggerEnter2D(Collider2D _other)
    {
        if (!needPress)
        {
            if (_other.CompareTag("Player"))
            {
                //SceneTransitionManager.instance.isTrasition = true;

                //SceneTransitionManager.instance.lastSceneName = SceneManager.GetActiveScene().name;

                ////SceneManager.LoadScene(nextSceneName);
                //StartCoroutine(SceneTransitionManager.instance.FadeAndLoadScene(SceneTransitionManager.FadeDirection.IN, nextSceneName));

                if (!needPress && _other.CompareTag("Player"))
                {
                    GameObject menuObj = Instantiate(upgradeMenuPrefab);
                    UpgradeSelectionUI ui = menuObj.GetComponent<UpgradeSelectionUI>();
                    ui.ShowMenu();

                    UpgradeSelectionUI.OnUpgradeChosen += HandleUpgradeChosen;
                }
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
