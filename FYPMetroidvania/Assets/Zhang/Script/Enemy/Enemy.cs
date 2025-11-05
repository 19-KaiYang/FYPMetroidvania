using System.Collections;
using UnityEngine;


public class Enemy : MonoBehaviour
{
    //[Header("-")]
    protected Rigidbody2D rb;
    protected Animator animator;
    public PlayerController player;
    protected StateMachine stateMachine;

    [Header("Layer")]
    [SerializeField] protected LayerMask playerLayer;
    [SerializeField] protected LayerMask obstacleLayer;
    [SerializeField] protected LayerMask groundleLayer;
    [SerializeField] protected LayerMask platformLayer;

    [Header("-")]
    //[SerializeField] public float maxHealth;
    //[SerializeField] public float currentHealth;
    //[SerializeField] public bool isDead = false;
    [SerializeField] public float moveSpeed;
    [SerializeField] public float attackDamage;

    [Header("-")]
    [SerializeField] public bool isFacingRight;
    [SerializeField] public bool isFacingLeft;
    [SerializeField] public Vector2 distanceToPlayer;
    [SerializeField] public Transform groundCheck;
    [SerializeField] private float groundCheckRadius;
    protected bool isOnPlatform;
    //[Space]
    //[SerializeField] private float groundCheckSize;
    //[SerializeField] private Vector2 groundCheckOffset;

    //cc
    public Health health;
    [Header("Damaged effect")]
    [SerializeField] protected GameObject damageParticle;
    [SerializeField] protected GameObject bloodParticle;
    [SerializeField] protected Transform damageParticlePos;
    [SerializeField] protected Transform bloodParticlePos;

    [Header("Knockdown State")]
    public float knockdownStep = 0;
    public bool getUp = false;

    protected virtual void OnEnable()
    {
        if (health == null)
            health = GetComponentInChildren<Health>();

        if (health != null)
            health.damageTaken += SpawnParticle;
            health.enemyDeath += DeathParticle;
    }

    protected virtual void OnDisable()
    {
        if (health != null)
            health.damageTaken -= SpawnParticle;
            health.enemyDeath -= DeathParticle;
    }
    protected virtual void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        //animator = GetComponent<Animator>();
        animator = GetComponentInChildren<Animator>();
        player = FindFirstObjectByType<PlayerController>();
        health = GetComponent<Health>();
    }

    protected virtual void Start()
    {
        
    }

    protected virtual void Update()
    {
        distanceToPlayer = transform.position - player.transform.position;
        RaycastHit2D platform = Physics2D.Raycast(groundCheck.position, Vector2.down, groundCheckRadius, platformLayer);
        if (platform)
        {
            isOnPlatform = true;
            if (rb.linearVelocityY < 0) rb.linearVelocityY = 0;
        }
        else isOnPlatform = false;
    }

    protected virtual void SpawnParticle(Health health)
    {
        if (damageParticle == null) return;

        Vector3 spawnPos = damageParticlePos.position;
        Quaternion rotation;

        float rotationR = Random.Range(0f, -70f);
        float rotationL = Random.Range(-130f, -180f);

        if (distanceToPlayer.x >= 0)
        {
            rotation = Quaternion.Euler(rotationR, 90f, -90f);
        }
        else
        {
            rotation = Quaternion.Euler(rotationL, 90f, -90f);
        }

        GameObject particle = Instantiate(damageParticle, spawnPos, rotation);

        Destroy(particle, 0.2f);
    }
    protected virtual void DeathParticle(GameObject _enemy)
    {
        Vector3 spawnPos = bloodParticlePos.position;
        if (bloodParticle == null) return;

        GameObject particle = Instantiate(bloodParticle, spawnPos, transform.rotation);

        Destroy(particle, 2.0f);
    }

    protected virtual void Flip()
    {
        isFacingRight = !isFacingRight;
        Vector3 f = transform.localScale;
        f.x = -f.x;
        transform.localScale = f;
    }

    protected virtual void FaceToPlayer()
    {
        if (player.transform.position.x < transform.position.x && isFacingRight)
        {
            Flip();
        }
        else if (player.transform.position.x > transform.position.x && !isFacingRight)
        {
            Flip();
        }
    }
    public virtual void Die()
    {

    }
}
