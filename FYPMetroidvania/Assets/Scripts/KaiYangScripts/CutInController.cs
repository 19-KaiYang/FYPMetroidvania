using UnityEngine;

public class CutInController : MonoBehaviour
{
    private Animator animator;
    private SpiritSlash pendingSlash;
    private Transform pendingPlayer;
    private Transform pendingTarget;
    private LayerMask pendingEnemyMask;

    void Start()
    {
        animator = GetComponent<Animator>();
    }

    private void Awake()
    {
        DontDestroyOnLoad(gameObject);
    }

    // Call this INSTEAD of calling Init directly
    public void PlayCutInThenInit(SpiritSlash slash, Transform player, Transform target, LayerMask enemyMask)
    {
        // Store the parameters
        pendingSlash = slash;
        pendingPlayer = player;
        pendingTarget = target;
        pendingEnemyMask = enemyMask;

        // Play the cut-in animation
        animator.SetTrigger("PlayCutIn"); // Adjust trigger name if needed
    }

    // This method will be called by the Animation Event
    public void OnCutInComplete()
    {
        if (pendingSlash != null)
        {
            // NOW call Init - this starts the spirit slash effects
            pendingSlash.Init(pendingPlayer, pendingTarget, pendingEnemyMask);

            // Clear references
            pendingSlash = null;
            pendingPlayer = null;
            pendingTarget = null;
        }
    }
}