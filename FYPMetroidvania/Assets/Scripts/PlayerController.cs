using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
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

    private Rigidbody2D rb;
    private Animator animator;
    private Vector2 moveInput;
    private Vector2 dashDirection;
    private bool isGrounded;
    private bool isDashing;
    private float dashTimer;
    private float dashCooldownTimer;

    // Jump control
    private bool jumpLocked = false;

    // Facing direction
    public bool facingRight { get; private set; } = true;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
    }

    private void Update()
    {
        // Dash timers
        if (isDashing)
        {
            dashTimer -= Time.deltaTime;
            if (dashTimer <= 0f)
                isDashing = false;
        }

        if (dashCooldownTimer > 0f)
            dashCooldownTimer -= Time.deltaTime;
    }

    private void FixedUpdate()
    {
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);

        if (isGrounded)
            jumpLocked = false;

        // Movement
        if (!isDashing)
            rb.linearVelocity = new Vector2(moveInput.x * moveSpeed, rb.linearVelocity.y);
        else
            rb.linearVelocity = dashDirection * dashSpeed;

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
        if (isGrounded && !jumpLocked)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
            jumpLocked = true;
        }
    }

    public void OnDash()
    {
        if (dashCooldownTimer <= 0f)
        {
            if (moveInput.sqrMagnitude > 0.1f)
                dashDirection = moveInput.normalized;
            else
                dashDirection = new Vector2(facingRight ? 1f : -1f, 0f);

            isDashing = true;
            dashTimer = dashDuration;
            dashCooldownTimer = dashCooldown;
        }
    }

    private void Flip()
    {
        facingRight = !facingRight;

        Vector3 spriteScale = spriteTransform.localScale;
        spriteScale.x *= -1f;
        spriteTransform.localScale = spriteScale;
    }
}
