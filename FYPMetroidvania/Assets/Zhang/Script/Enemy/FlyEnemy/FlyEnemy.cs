using UnityEngine;

public class FlyEnemy : Enemy
{
    [Header("Detaect")]
    [SerializeField] private LayerMask playerLayer;
    [SerializeField] private LayerMask obstacleLayer;

    [SerializeField] private float playerDetectDistance = 5f;
    [SerializeField] private float playerEscapeDistance = 10f;

    [SerializeField] private bool playerDetected;


    protected override void Awake()
    {
        base.Awake();
    }

    void Start()
    {
        
    }


    void Update()
    {
        DetectPlayer();
    }

    private void DetectPlayer()
    {
        Collider2D pDetected = Physics2D.OverlapCircle(transform.position, playerDetectDistance, playerLayer);

        Collider2D pEscaped = Physics2D.OverlapCircle(transform.position, playerEscapeDistance, playerLayer);

        Vector2 dir = (player.transform.position - transform.position).normalized;
        float distance = Vector2.Distance(transform.position, player.transform.position);
        RaycastHit2D ray = Physics2D.Raycast(transform.position, dir, distance, obstacleLayer);

        if (pDetected != null)
        {
            if (ray.collider == null)
            {
                playerDetected = true;
            }
            else
            {
                playerDetected = false;
            }
        }
        else if(pEscaped == null)
        {
            playerDetected = false;
        }
        
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, playerDetectDistance);

        if (player != null && playerDetected)
        {
            Gizmos.DrawLine(transform.position, player.transform.position);
        }

        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, playerEscapeDistance);
    }
}
