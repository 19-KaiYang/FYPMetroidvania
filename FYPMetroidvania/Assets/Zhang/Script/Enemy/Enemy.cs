using UnityEngine;

public class Enemy : MonoBehaviour, IDamageable
{
    //[Header("-")]
    protected Rigidbody2D rb;
    protected Animator animator;
    protected PlayerController player;
    private StateMachine stateMachine;

    

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
        stateMachine = GetComponent<StateMachine>();
    }

    void Start()
    {
        currentHealth = maxHealth;
    }


    void Update()
    {
        distanceToPlayer = transform.position - player.transform.position;
    }

    


    public virtual void TakeDamage(float _damage)
    {

    }

    public virtual void Die()
    {

    }
}
