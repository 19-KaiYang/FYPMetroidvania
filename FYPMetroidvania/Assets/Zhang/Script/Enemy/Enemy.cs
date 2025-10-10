using System;
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

    [Header("-")]
    [SerializeField] public float maxHealth;
    [SerializeField] public float currentHealth;
    [SerializeField] public bool isDead = false;
    [SerializeField] public float moveSpeed;
    [SerializeField] public float attackDamage;

    [Header("-")]
    [SerializeField] public bool isFacingRight;
    [SerializeField] public bool isFacingLeft;
    [SerializeField] public Vector2 distanceToPlayer;
    //[Space]
    //[SerializeField] private float groundCheckSize;
    //[SerializeField] private Vector2 groundCheckOffset;
    [Header("CC")]
    protected Health health;


    protected virtual void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        animator = GetComponentInChildren<Animator>();
        player = FindFirstObjectByType<PlayerController>();
        health = GetComponent<Health>();
    }

    void Start()
    {
        //health.damageTaken += SpawnParticle;
        currentHealth = maxHealth;
    }

    protected virtual void Update()
    {
        distanceToPlayer = transform.position - player.transform.position;
    }

    public void SpawnParticle(GameObject other)
    {
        
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

    public virtual void TakeDamage(float _damage, Vector2 _dir)
    {
        currentHealth -= _damage;

        if(currentHealth <= 0)
        {
            Die();
        }
    }

    public virtual void Die()
    {

    }
}
