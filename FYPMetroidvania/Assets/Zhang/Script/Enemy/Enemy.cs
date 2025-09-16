using UnityEngine;

public class Enemy : MonoBehaviour, IDamageable
{
    //[Header("-")]
    protected Rigidbody2D rb;
    protected Animator animator;
    protected PlayerController player;
    protected StateMachine stateMachine;

    [Header("Detaect")]
    [SerializeField] protected LayerMask playerLayer;
    [SerializeField] protected LayerMask obstacleLayer;
    [SerializeField] protected LayerMask groundleLayer;

    [Header("-")]
    [SerializeField] public float maxHealth;
    [SerializeField] public float currentHealth;
    [SerializeField] public bool isDead = false;
    [SerializeField] public float moveSpeed;

    [Header("-")]
    [SerializeField] public bool isFacingRight;
    [SerializeField] public bool isFacingLeft;
    [SerializeField] public Vector2 distanceToPlayer;

    

    protected virtual void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        player = FindFirstObjectByType<PlayerController>();
    }

    void Start()
    {
        currentHealth = maxHealth;
    }


    protected virtual void Update()
    {
        distanceToPlayer = transform.position - player.transform.position;
    }

    protected virtual void Flip()
    {
        isFacingRight = !isFacingRight;
        Vector3 f = transform.localScale;
        f.x = -f.x;
        transform.localScale = f;
    }

    public virtual void TakeDamage(float _damage)
    {

    }

    public virtual void Die()
    {

    }
}
