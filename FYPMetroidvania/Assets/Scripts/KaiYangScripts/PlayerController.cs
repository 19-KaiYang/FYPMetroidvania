using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    public static PlayerController instance;

    [Header("References")]
    public Transform spriteTransform;

    [Header("Movement")]
    public float moveSpeed = 5f;
    public float jumpForce = 8f;

    [Header("Dash")]
    public float dashSpeed = 15f;
    public float dashDuration = 0.2f;
    public float dashCooldown = 1f;

    [Header("Ground Check")]
    public Transform groundCheck;
    public float groundCheckRadius = 0.2f;
    public LayerMask groundLayer;

    public Rigidbody2D rb;
    private Animator animator;
    private SpriteRenderer spriteRenderer;
    public Vector2 moveInput;
    private Vector2 dashDirection;
    private bool isDashing;
    private float dashTimer;
    private float dashCooldownTimer;

    [HideInInspector] public bool externalVelocityOverride = false;

    // Jump control
    private bool jumpLocked = false;

    // Facing direction
    public bool facingRight { get; private set; } = true;

    // Conditions
    private bool hasAirDashed = false;
    public bool HasAirSwordDashed { get; private set; }   
    public bool HasAirUppercut { get; private set; }      
    public bool IsGrounded { get; private set; }

    private void Awake()
    {
        instance = this;
        rb = GetComponent<Rigidbody2D>();

        // Auto-find Animator & SpriteRenderer
        animator = GetComponentInChildren<Animator>();
        spriteRenderer = GetComponentInChildren<SpriteRenderer>();
    }

    private void Update()
    {
        // Dash timers
        if (isDashing)
        {
            dashTimer -= Time.deltaTime;
            if (dashTimer <= 0f)
            {
                isDashing = false;

                if (Mathf.Abs(rb.linearVelocity.y) > 0.1f)
                {
                    rb.linearVelocity = new Vector2(
                        rb.linearVelocity.x,
                        rb.linearVelocity.y * 0.3f
                    );
                }
            }
        }

        if (dashCooldownTimer > 0f)
            dashCooldownTimer -= Time.deltaTime;
    }

    private void FixedUpdate()
    {
        IsGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);

        if (IsGrounded)
        {
            jumpLocked = false;
            hasAirDashed = false;
            HasAirSwordDashed = false;   
            HasAirUppercut = false;      
        }

        // Movement
        if (!externalVelocityOverride)
        {
            if (!isDashing)
                rb.linearVelocity = new Vector2(moveInput.x * moveSpeed, rb.linearVelocity.y);
            else
                rb.linearVelocity = dashDirection * dashSpeed;
        }

        // Flip sprite
        if (moveInput.x > 0 && !facingRight)
            Flip();
        else if (moveInput.x < 0 && facingRight)
            Flip();
    }

    public void OnMove(InputValue value)
    {
        moveInput = value.Get<Vector2>();
    }

    public void OnJump()
    {
        if (IsGrounded && !jumpLocked)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
            jumpLocked = true;
        }
    }

    public void OnDash()
    {
        if (dashCooldownTimer <= 0f)
        {
            if (!IsGrounded && hasAirDashed)
                return;

            if (moveInput.sqrMagnitude > 0.1f)
                dashDirection = moveInput.normalized;
            else
                dashDirection = new Vector2(facingRight ? 1f : -1f, 0f);

            float speed = dashSpeed;

            isDashing = true;
            dashTimer = dashDuration;
            dashCooldownTimer = dashCooldown;

            rb.linearVelocity = dashDirection * speed;

            if (!IsGrounded)
                hasAirDashed = true;
        }
    }

    public void MarkAirSwordDash()
    {
        HasAirSwordDashed = true;
    }

    public void MarkAirUppercut()
    {
        HasAirUppercut = true;
    }

    public void Flip()
    {
        facingRight = !facingRight;

        Vector3 spriteScale = spriteTransform.localScale;
        spriteScale.x *= -1f;
        spriteTransform.localScale = spriteScale;
    }
}
