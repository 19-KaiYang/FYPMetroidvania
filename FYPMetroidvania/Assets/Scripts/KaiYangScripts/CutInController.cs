using UnityEngine;
using UnityEngine.SceneManagement;

public class CutInController : MonoBehaviour
{
    private static CutInController instance;
    private Animator animator;
    private SpiritSlash pendingSlash;
    private Transform pendingPlayer;
    private Transform pendingTarget;
    private LayerMask pendingEnemyMask;
    private float pendingHealAmount;

    void Start()
    {
        animator = GetComponent<Animator>();
    }

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else Destroy(gameObject);
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDestroy()
    {

        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == "MainMenu")
        {
            Destroy(gameObject);
        }
    }

    public void PlayCutInThenInit(SpiritSlash slash, Transform player, Transform target, LayerMask enemyMask, float healamount)
    {
        pendingSlash = slash;
        pendingPlayer = player;
        pendingTarget = target;
        pendingEnemyMask = enemyMask;
        pendingHealAmount = healamount;

        animator.SetTrigger("PlayCutIn");
    }

    public void OnCutInComplete()
    {
        if (pendingSlash != null)
        {
            pendingSlash.Init(pendingPlayer, pendingTarget, pendingEnemyMask,pendingHealAmount);
            pendingSlash = null;
            pendingPlayer = null;
            pendingTarget = null;
        }
    }
}