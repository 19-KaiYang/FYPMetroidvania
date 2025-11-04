using System;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class PlayerController : MonoBehaviour
{
    public static PlayerController instance;
    private CombatSystem combat;
    private Skills skills;

    [Header("References")]
    public Transform spriteTransform;

    [Header("Movement")]
    public float moveSpeed = 5f;
    public float jumpForce = 8f;
    public float gravity = -20f;
    public float jumpBufferTime = 0.15f;
    public int airJumpCount = 0;
    private int airJumpsDone;
    private float jumpBufferCounter;

    [Header("Footstep Settings")]
    public float footstepInterval = 0.4f;
    private float footstepTimer = 0f;
    private Vector3 lastPosition;
    private float moveDistanceSinceLastStep = 0f;

    [Header("Float")]
    public bool canFloat = false;
    private bool isFloating = false;
    public float floatGravity = -5f;

    [Header("Platform dropping")]
    public bool platformDropping;
    public float platformDropDuration = 0.5f;
    private float platformDropTimer = 0f;
    public float platformDropSpeed = 2f;

    [Header("Wall Coyote Time")]
    public float wallCoyoteTime = 0.15f;
    private float wallCoyoteCounter;

    [Header("Dash")]
    public float dashSpeed = 15f;
    public float dashDuration = 0.2f;
    public float dashCooldown = 1f;
    public int dashCount = 1;
    public int dashesRemaining;
    public TrailRenderer dashTrail;

    [Header("Wall Jump")]
    public float wallCheckDistance = 0.3f;
    public float wallJumpForce = 8f;
    public Vector2 wallJumpDirection = new Vector2(1, 1);
    private bool hasWallJumped;
    private float lastWallJumpDirection = 0f;

    [Header("Ground Check")]
    public Transform groundCheck;
    public float groundCheckRadius = 0.2f;
    public LayerMask groundLayer;
    public LayerMask platformLayer;
    private LayerMask GroundCheckLayer;

    [Header("Collider")]
    public Vector2 colliderSize = new Vector2(0.5f, 1f);

    [Header("Knockback")]
    public bool isInKnockback = false;
    public float knockbackTimer = 0f;
    public Vector2 knockbackVelocity;

    public Rigidbody2D rb;
    public Animator animator;
    private SpriteRenderer spriteRenderer;

    public Vector2 moveInput;
    private Vector2 dashDirection;
    private Vector2 velocity;

    public bool isDashing;
    private float dashTimer;
    private float dashCooldownTimer;

    private int currentKnockdownPhase = 0;
    public bool wasGroundedLastFrame = false;
    private float knockdownPhaseTimer = 0f;

    public bool externalVelocityOverride = false;

    // Jump control
    private bool jumpLocked = false;

    // Facing direction
    public bool facingRight { get; private set; } = true;
    public Action flipped;

    // Conditions
    private bool hasAirDashed = false;
    public bool HasAirSwordDashed { get; private set; }
    public bool HasAirUppercut { get; private set; }
    public bool IsGrounded { get; private set; }
    private bool IsOnPlatform;

    public bool isInHitstop { get; private set; }
    public Vector2 GetVelocity() => velocity;
    public Vector2 CurrentVelocity => velocity;

    public bool isInCutscene;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
            if(SceneManager.GetActiveScene().name == "Goblin Camp")
                isInCutscene = true;
        }
        else
        {
            Destroy(gameObject);
        }

        rb = GetComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Kinematic;
        rb.simulated = true;

        skills = GetComponentInChildren<Skills>();
        combat = GetComponentInChildren<CombatSystem>();
        if(animator == null) animator = GetComponentInChildren<Animator>();
        spriteRenderer = GetComponentInChildren<SpriteRenderer>();

        if (groundCheck != null)
        {
            groundCheck.position = new Vector2(transform.position.x, transform.position.y - (colliderSize.y / 2));
        }
        GroundCheckLayer = groundLayer | platformLayer;

        lastPosition = transform.position;
        // Initialize dash count
        dashesRemaining = dashCount;
        if (dashTrail != null) dashTrail.emitting = false;
    }

    private void Update()
    {
        if (isInHitstop) return;
        if (isInCutscene) return;

        var health = GetComponent<Health>();

        // GROUND CHECK
        RaycastHit2D ground = Physics2D.Raycast(groundCheck.position, Vector2.down, groundCheckRadius, GroundCheckLayer);
        if (ground.collider != null)
        {
            if (!IsGrounded)
            {
                AudioManager.PlaySFX(SFXTYPE.PLAYER_LAND, 0.5f);
                if (combat.isAttacking)
                {
                    externalVelocityOverride = false;
                    combat.DisableAllHitboxes();
                }
                combat.isAttacking = false;
                animator.SetBool("IsAttacking", false); 
            }
            IsGrounded = true;
            IsOnPlatform = ground.collider.CompareTag("Platform");
        }
        else
        {
            IsGrounded = false;
            IsOnPlatform = false;
        }
        //if (currentKnockdownPhase >= 3)
        //{
        //    currentKnockdownPhase = 0;
        //    animator.SetInteger("KnockdownPhase", 0);
        //}
        if (health != null && health.currentCCState == CrowdControlState.Knockdown)
        {
            if (currentKnockdownPhase == 0)
            {
                externalVelocityOverride = true;
                currentKnockdownPhase = 1;
                knockdownPhaseTimer = 0.1f;
                animator.SetInteger("KnockdownPhase", 1);
                Debug.Log("Knockdown Started - Phase 1: Launch");
                wasGroundedLastFrame = false;
            }
            else
            {
                knockdownPhaseTimer -= Time.deltaTime;

                switch (currentKnockdownPhase)
                {
                    case 1: 
                        if (knockdownPhaseTimer <= 0 && velocity.y < 0.1f)
                        {
                            currentKnockdownPhase = 2;
                            animator.SetInteger("KnockdownPhase", 2);
                            Debug.Log("Knockdown Phase 2: Falling");
                            wasGroundedLastFrame = false;
                        }
                        break;

                    case 2: 
                           
                        if (IsGrounded && currentKnockdownPhase < 3)
                        {
                            AudioManager.PlaySFX(SFXTYPE.PLAYER_LAND);
                            currentKnockdownPhase = 3;
                            animator.SetInteger("KnockdownPhase", 3);
                            Debug.Log("Knockdown Phase 3: Landing");
                            health.isInArcKnockdown = false;
                        }
                        break;

                    case 3:
                        break;
                }
            }

            wasGroundedLastFrame = IsGrounded;
        }
        else
        {
            // Reset when not in knockdown
            animator.SetInteger("KnockdownPhase", 0);
            currentKnockdownPhase = 0;
            wasGroundedLastFrame = false;
            knockdownPhaseTimer = 0f;
        }

        // Stop player movement and input while CC active
        if (health != null && health.currentCCState != CrowdControlState.None)
        {
            velocity.x = 0f;

            // Allow knockdown animation updates to still play
            if (health.currentCCState != CrowdControlState.Knockdown)
                return;
        }

        animator.SetBool("IsUsingSkill", skills != null && skills.IsUsingSkill);

        // === ONLY UPDATE THESE ANIMATIONS IF NOT USING SKILL ===
        if (skills == null || !skills.IsUsingSkill)
        {
            if (IsGrounded)
                animator.SetFloat("Speed", Mathf.Abs(moveInput.x));
            else
                animator.SetFloat("Speed", 0f);

            animator.SetBool("IsFalling", !IsGrounded && velocity.y < -0.1f);
        }

        lastPosition = transform.position;

        // Dash timers
        if (isDashing)
        {
            dashTimer -= Time.deltaTime;
            if (dashTimer <= 0f)
            {
                isDashing = false;
                animator.SetBool("isDashing", false);
                dashTrail.emitting = false;
                if (Mathf.Abs(velocity.y) > 0.1f)
                    velocity.y *= 0.3f;
            }
        }

        if (dashCooldownTimer > 0f)
        {
            dashCooldownTimer -= Time.deltaTime;
            if (dashCooldownTimer <= 0f)
            {
                dashesRemaining = dashCount;
            }
        }

        isFloating = false;

        if (IsGrounded)
        {
            jumpLocked = false;
            hasAirDashed = false;
            HasAirSwordDashed = false;
            HasAirUppercut = false;
            hasWallJumped = false;
            lastWallJumpDirection = 0f;

            if (velocity.y < 0) velocity.y = -1f;

            if (jumpBufferCounter > 0f && !jumpLocked && !platformDropping && !combat.isAttacking)
            {
                Debug.Log("Jump1");
                animator.SetBool("IsAttacking", false);
                velocity.y = jumpForce;
                jumpLocked = true;
                jumpBufferCounter = 0f;
                externalVelocityOverride = false;
                AudioManager.PlaySFX(SFXTYPE.PLAYER_JUMP);
            }
        }
        else if (velocity.y < 0f)
        {
            if (canFloat && Input.GetKey(KeyCode.Space))
                isFloating = true;
        }

        if (platformDropping)
        {
            if (!externalVelocityOverride) velocity.y = -platformDropSpeed;
            platformDropTimer -= Time.deltaTime;
            if (platformDropTimer < 0f)
            {
                platformDropTimer = 0f;
                platformDropping = false;
            }
        }

        // Movement logic (only runs if NOT in CC state)
        if (!externalVelocityOverride)
        {
            if (skills != null && skills.IsChargeLocked)
                velocity.x = 0f;
            else if (!isDashing)
                velocity.x = moveInput.x * moveSpeed;
            else
                velocity = dashDirection * dashSpeed;
        }

        //Flipping of sprites
        bool isKnockedDown = health != null && health.currentCCState == CrowdControlState.Knockdown;

        if (!isKnockedDown && !skills.IsUsingSkill && !combat.isAttacking)
        {
            if (moveInput.x > 0 && !facingRight)
                Flip();
            else if (moveInput.x < 0 && facingRight)
                Flip();
        }

        // Handle wall coyote timer
        if (IsTouchingWall() && !IsGrounded)
        {
            wallCoyoteCounter = wallCoyoteTime;
        }
        else
        {
            if (wallCoyoteCounter > 0)
                wallCoyoteCounter -= Time.deltaTime;
        }
    }
    private void FixedUpdate()
    {
        if (isInHitstop) return;

        var health = GetComponent<Health>();
        bool inArcKnockdown = health != null && health.isInArcKnockdown;
        if (isInKnockback)
        {
            velocity = knockbackVelocity;
            knockbackTimer -= Time.deltaTime;
            knockbackVelocity -= knockbackVelocity * Time.fixedDeltaTime;
            if(knockbackTimer < 0 && currentKnockdownPhase <= 0)
            {
                isInKnockback = false;
                externalVelocityOverride = false;
                knockbackVelocity = Vector3.zero;
            }
        }
        // Apply gravity
        if (!isDashing)
            velocity.y += Time.fixedDeltaTime * (isFloating ? floatGravity : gravity);

        // Move the character
        Move(velocity * Time.fixedDeltaTime);

        // Tick down jump buffer
        if (jumpBufferCounter > 0)
            jumpBufferCounter -= Time.fixedDeltaTime;
    }


    public void Move(Vector2 moveAmount)
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
                return;
            }
            if (dir.y < 0 && !platformDropping) // check platform collision seperately
            {
                hit = Physics2D.BoxCast(groundCheck.position, new Vector2(colliderSize.x, 0.01f), 0f, Vector2.down, 0f, platformLayer);
                if (hit.collider != null)
                {
                    float dist = hit.distance - 0.01f;
                    transform.Translate(dir * dist);
                    velocity.y = 0;
                    return;
                }
            }
            transform.Translate(Vector2.up * moveAmount.y);
        }
    }

    public void OnMove(InputValue value)
    {
        moveInput = value.Get<Vector2>();
    }

    public void OnJump()
    {
        if (skills != null && skills.IsChargeLocked) return;

        if (platformDropping) return;

        jumpBufferCounter = jumpBufferTime;

        if (IsGrounded)
        {
            if (IsOnPlatform && moveInput.y < 0)
            {
                platformDropping = true;
                platformDropTimer = platformDropDuration;
                velocity.y = -platformDropSpeed;
            }
            else if (!jumpLocked && !combat.isAttacking && !externalVelocityOverride)
            {
                Debug.Log("Jump1");
                velocity.y = jumpForce;
                jumpLocked = true;
                airJumpsDone = 0;
                animator.SetBool("IsAttacking", false);
                animator.SetTrigger("Jump");
                AudioManager.PlaySFX(SFXTYPE.PLAYER_JUMP);
            }
        }
        else if (!IsGrounded)
        {
            if (IsTouchingWall())
            {
                bool wallOnRight = IsWallOnRight();
                bool wallOnLeft = IsWallOnLeft();

                float jumpDirection = 0f;
                bool canWallJump = false;

                // determine jump direction based on wall
                if (wallOnRight && !wallOnLeft)
                {
                    jumpDirection = -1f; // jump left from right wall
                }
                else if (wallOnLeft && !wallOnRight)
                {
                    jumpDirection = 1f; // jump right from left wall
                }

                if (jumpDirection != 0f && (!hasWallJumped || jumpDirection != lastWallJumpDirection))
                {
                    canWallJump = true;
                }

                if (canWallJump)
                {
                    hasWallJumped = true;
                    lastWallJumpDirection = jumpDirection;

                    velocity = new Vector2(
                        jumpDirection * wallJumpDirection.x * wallJumpForce,
                        wallJumpDirection.y * wallJumpForce
                    );

                    StartCoroutine(WallJumpBuffer());
                }
            }
            else if (airJumpsDone < airJumpCount)
            {
                velocity.y = jumpForce;
                jumpLocked = true;
                airJumpsDone++;
            }
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
        if (skills != null && skills.IsUsingSkill) return;
        if (isInCutscene) return;
        if (dashCooldownTimer > 0f) return;
        if (dashesRemaining <= 0) return;

        if (moveInput.sqrMagnitude > 0.1f)
            dashDirection = moveInput.normalized;
        else
            dashDirection = new Vector2(facingRight ? 1f : -1f, 0f);

        isDashing = true;
        dashTimer = dashDuration;
        velocity = dashDirection * dashSpeed;
        animator.SetBool("isDashing", true);
        dashTrail.emitting = true;
        dashesRemaining--;
        if (combat != null && combat.isAttacking){
            combat.SetCanTransition(1);
            animator.SetBool("IsAttacking", false);
            combat.HideVFX();
        }
        AudioManager.PlaySFX(SFXTYPE.PLAYER_DASH);


        // Start cooldown only when all dashes are used
        if (dashesRemaining <= 0)
        {
            dashCooldownTimer = dashCooldown;
        }
    }

    public void MarkAirSwordDash() => HasAirSwordDashed = true;
    public void MarkAirUppercut() => HasAirUppercut = true;

    public void Flip()
    {
        facingRight = !facingRight;
        Vector3 spriteScale = spriteTransform.localScale;
        spriteScale.x *= -1f;
        spriteTransform.localScale = spriteScale;
        flipped?.Invoke();
        //float yRotation = facingRight ? 0f : 180f;
        //spriteTransform.rotation = Quaternion.Euler(0f, yRotation, 0f);
    }

    private bool IsTouchingWall()
    {
        return IsWallOnRight() || IsWallOnLeft();
    }

    private bool IsWallOnRight()
    {
        float offsetX = colliderSize.x / 2f + 0.05f;
        Vector2 originRight = (Vector2)transform.position + Vector2.right * offsetX;
        return Physics2D.Raycast(originRight, Vector2.right, wallCheckDistance, groundLayer);
    }

    private bool IsWallOnLeft()
    {
        float offsetX = colliderSize.x / 2f + 0.05f;
        Vector2 originLeft = (Vector2)transform.position + Vector2.left * offsetX;
        return Physics2D.Raycast(originLeft, Vector2.left, wallCheckDistance, groundLayer);
    }

    public void SetVelocity(Vector2 newVel)
    {
        velocity = newVel;
    }
    public void SetKnockback(Vector2 knockback, float knockbackDuration)
    {
        isInKnockback = true;
        knockbackTimer = knockbackDuration;
        knockbackVelocity = knockback;
        externalVelocityOverride = true;
    }

    //HIT STOP
    public void SetHitstop(bool state)
    {
        isInHitstop = state;
        if (state)
        {
            velocity = Vector2.zero;
            Time.timeScale = 0f;
        }
        else Time.timeScale = 1f;
    }

    public void ResetState()
    {
        combat.isAttacking = false;
        animator.SetBool("IsAttacking", false);
        
    }
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireCube(transform.position, colliderSize);
    }
}