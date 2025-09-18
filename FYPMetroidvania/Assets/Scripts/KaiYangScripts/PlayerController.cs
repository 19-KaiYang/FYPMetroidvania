using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    public static PlayerController instance;
    private Skills skills;

    [Header("References")]
    public Transform spriteTransform;

    [Header("Movement")]
    public float moveSpeed = 5f;
    public float jumpForce = 8f;
    public float gravity = -20f;
    public float jumpBufferTime = 0.15f;
    private float jumpBufferCounter;

    [Header("Wall Coyote Time")]
    public float wallCoyoteTime = 0.15f;
    private float wallCoyoteCounter;


    [Header("Dash")]
    public float dashSpeed = 15f;
    public float dashDuration = 0.2f;
    public float dashCooldown = 1f;

    [Header("Wall Jump")]
    public float wallCheckDistance = 0.3f;
    public float wallJumpForce = 8f;
    public Vector2 wallJumpDirection = new Vector2(1, 1);
    private bool hasWallJumped;

    [Header("Ground Check")]
    public Transform groundCheck;
    public float groundCheckRadius = 0.2f;
    public LayerMask groundLayer;

    [Header("Collider")]
    public Vector2 colliderSize = new Vector2(0.5f, 1f);

    public Rigidbody2D rb;
    private Animator animator;
    private SpriteRenderer spriteRenderer;

    public Vector2 moveInput;
    private Vector2 dashDirection;
    private Vector2 velocity;

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

    public Vector2 CurrentVelocity => velocity;

    private void Awake()
    {
        instance = this;
        rb = GetComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Kinematic; 
        rb.simulated = true;                     

        skills = GetComponentInChildren<Skills>();
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
                if (Mathf.Abs(velocity.y) > 0.1f)
                    velocity.y *= 0.3f;
            }
        }

        if (dashCooldownTimer > 0f)
            dashCooldownTimer -= Time.deltaTime;

        // Ground check
        IsGrounded = Physics2D.Raycast(groundCheck.position,Vector2.down,groundCheckRadius,groundLayer);


        if (IsGrounded)
        {
            jumpLocked = false;
            hasAirDashed = false;
            HasAirSwordDashed = false;
            HasAirUppercut = false;
            hasWallJumped = false;

            if (velocity.y < 0) velocity.y = -1f;

          
            if (jumpBufferCounter > 0f && !jumpLocked)
            {
                velocity.y = jumpForce;
                jumpLocked = true;
                jumpBufferCounter = 0f;
            }
        }

        // Movement logic
        if (!externalVelocityOverride)
        {
            if (skills != null && skills.IsChargeLocked)
                velocity.x = 0f;
            else if (!isDashing)
                velocity.x = moveInput.x * moveSpeed;
            else
                velocity = dashDirection * dashSpeed;
        }

      

        // Flip sprite
        if (moveInput.x > 0 && !facingRight)
            Flip();
        else if (moveInput.x < 0 && facingRight)
            Flip();

        // Apply movement manually
        Move(velocity * Time.deltaTime);

        // Handle wall coyote timer
        if (IsTouchingWall() && !IsGrounded)
        {
            wallCoyoteCounter = wallCoyoteTime;
        }
        else
        {
            if (wallCoyoteCounter > 0)
                wallCoyoteCounter -= Time.deltaTime;
            else
                hasWallJumped = false; 
        }


    }

    private void FixedUpdate()
    {
        // Apply gravity
        if (!isDashing)
            velocity.y += gravity * Time.fixedDeltaTime;

        // Move the character
        Move(velocity * Time.fixedDeltaTime);

        // Tick down jump buffer
        if (jumpBufferCounter > 0)
            jumpBufferCounter -= Time.fixedDeltaTime;
    }


    private void Move(Vector2 moveAmount)
    {
        // Horizontal
        if (moveAmount.x != 0)
        {
            Vector2 dir = new Vector2(Mathf.Sign(moveAmount.x), 0);
            RaycastHit2D hit = Physics2D.BoxCast(transform.position, colliderSize, 0f, dir, Mathf.Abs(moveAmount.x), groundLayer);
            if (hit.collider != null)
            {
                float dist = hit.distance - 0.01f;
                transform.Translate(dir * dist);
                velocity.x = 0;
            }
            else
                transform.Translate(Vector2.right * moveAmount.x);
        }

        // Vertical
        if (moveAmount.y != 0)
        {
            Vector2 dir = new Vector2(0, Mathf.Sign(moveAmount.y));
            RaycastHit2D hit = Physics2D.BoxCast(transform.position, colliderSize, 0f, dir, Mathf.Abs(moveAmount.y), groundLayer);
            if (hit.collider != null)
            {
                float dist = hit.distance - 0.01f;
                transform.Translate(dir * dist);
                velocity.y = 0;
            }
            else
                transform.Translate(Vector2.up * moveAmount.y);
        }
    }

    public void OnMove(InputValue value) => moveInput = value.Get<Vector2>();

    public void OnJump()
    {
        if (skills != null && skills.IsChargeLocked) return;


        jumpBufferCounter = jumpBufferTime;

        if (IsGrounded && !jumpLocked)
        {
            velocity.y = jumpForce;
            jumpLocked = true;
        }
        else if (!IsGrounded && !hasWallJumped && IsTouchingWall())
        {
            hasWallJumped = true;

            bool wallOnRight = IsWallOnRight();
            float dir = wallOnRight ? -1f : 1f;

            velocity = new Vector2(
                dir * wallJumpDirection.x * wallJumpForce,
                wallJumpDirection.y * wallJumpForce
            );

            StartCoroutine(WallJumpBuffer());
        }




    }

    private IEnumerator WallJumpBuffer()
    {
        externalVelocityOverride = true;
        yield return new WaitForSeconds(0.2f);
        externalVelocityOverride = false;
    }


    public void OnDash()
    {
        if (skills != null && skills.IsChargeLocked) return;
        if (dashCooldownTimer > 0f) return;
        if (!IsGrounded && hasAirDashed) return;

        if (moveInput.sqrMagnitude > 0.1f)
            dashDirection = moveInput.normalized;
        else
            dashDirection = new Vector2(facingRight ? 1f : -1f, 0f);

        isDashing = true;
        dashTimer = dashDuration;
        dashCooldownTimer = dashCooldown;
        velocity = dashDirection * dashSpeed;

        if (!IsGrounded)
            hasAirDashed = true;
    }

    public void MarkAirSwordDash() => HasAirSwordDashed = true;
    public void MarkAirUppercut() => HasAirUppercut = true;

    public void Flip()
    {
        facingRight = !facingRight;
        Vector3 spriteScale = spriteTransform.localScale;
        spriteScale.x *= -1f;
        spriteTransform.localScale = spriteScale;
    }

    private bool IsTouchingWall()
    {
        float offsetX = colliderSize.x / 2f + 0.05f;
        Vector2 originRight = (Vector2)transform.position + Vector2.right * offsetX;
        Vector2 originLeft = (Vector2)transform.position + Vector2.left * offsetX;

        return Physics2D.Raycast(originRight, Vector2.right, wallCheckDistance, groundLayer) ||
               Physics2D.Raycast(originLeft, Vector2.left, wallCheckDistance, groundLayer);
    }

    private bool IsWallOnRight()
    {
        float offsetX = colliderSize.x / 2f + 0.05f;
        Vector2 originRight = (Vector2)transform.position + Vector2.right * offsetX;
        return Physics2D.Raycast(originRight, Vector2.right, wallCheckDistance, groundLayer);
    }

    public void SetVelocity(Vector2 newVel)
    {
        velocity = newVel;
    }


    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireCube(transform.position, colliderSize);
    }
}
