using UnityEngine;

public class Enemy : MonoBehaviour,IDamageable
{
    //[Header("-")]
    private Rigidbody rb;
    private Animator animator;
    private PlayerController player;

    [Header("-")]
    [SerializeField] public float maxHealth;
    [SerializeField] public float currentHealth;
    [SerializeField] public bool isDead = false;
    [SerializeField] public float moveSpeed;

    [Header("-")]
    [SerializeField] public bool isFacingRight;
    [SerializeField] public bool isFacingLeft;
    [SerializeField] public Vector2 distanceToPlayer;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        animator = GetComponent<Animator>();
        player = FindFirstObjectByType<PlayerController>();
    }

    void Start()
    {
        currentHealth = maxHealth;
    }


    void Update()
    {   
        
    }

    public virtual void TakeDamage(float _damage)
    {

    }

    public void Die()
    {

    }
}
