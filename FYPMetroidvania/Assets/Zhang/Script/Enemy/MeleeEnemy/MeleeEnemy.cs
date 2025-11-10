using System.Collections;
using System.Runtime.CompilerServices;
using UnityEngine;
using static Spearman;

public class MeleeEnemy : Enemy
{
    [SerializeField] private string currentState;

    [Header("Detect")]
    [SerializeField] private Vector2 detectSize;
    [SerializeField] private Vector2 detectOffset;
    [Space]
    [SerializeField] private Vector2 playerEscapeSize;
    [SerializeField] private Vector2 playerEscapeOffset;
    [Space]
    [SerializeField] private Vector2 attackArea;
    [SerializeField] private Vector2 attackAreaOffset;
    [Space]
    [SerializeField] private float groundCheckSize;
    [SerializeField] private Vector2 groundCheckOffset;
    [Space]
    [SerializeField] private bool playerDetected;
    [SerializeField] private bool inDetectArea;
    [SerializeField] private bool inAttackArea;
    [SerializeField] private bool isGround;

    [Header("Attack")]
    [SerializeField] private float jumpForceY;
    [SerializeField] private float jumpForceX;
    [SerializeField] private float gravityA;
    [SerializeField] private float gravityB;
    [SerializeField] private float distanceOffset;
    [SerializeField] private float maxJumpRange;
    [SerializeField] private GameObject jumpHitbox;
    [SerializeField] private GameObject jumpTrail;

    [SerializeField] private float meleeAttackCooldown;
    [SerializeField] private float meleeAttackTimer;
    [SerializeField] public bool isAttackFinished {  get;  set; }

    [SerializeField] private float jumpCooldown;
    [SerializeField] private float jumpTimer;

    protected override void Awake()
    {
        base.Awake();
        stateMachine = new StateMachine();

        stateMachine.stateChanged += OnStateChanged;
    }
    protected override void Start()
    {
        stateMachine.Initialize(new MeleeEnemyIdleState(this));
        jumpHitbox.SetActive(false);
        jumpTrail.SetActive(false);
    }
    protected override void Update()
    {
        base.Update();
        stateMachine.Update();
        DetectPlayer();

        if (rb.linearVelocity.y < 0)
        {
            rb.gravityScale = gravityB;
        }
        else
        {
            rb.gravityScale = gravityA;
        }
        if (Input.GetKeyDown(KeyCode.N))
        {
            animator.SetTrigger("Attack");
        }
    }
    private void DetectPlayer()
    {
        Collider2D ground = Physics2D.OverlapCircle((Vector2)transform.position + groundCheckOffset, groundCheckSize, groundleLayer);
        if (ground != null) isGround = true;
        else isGround = false;
        if (isOnPlatform) isGround = true;

        Collider2D pDetected = Physics2D.OverlapBox((Vector2)transform.position + detectOffset, detectSize, 0f, playerLayer);
        Collider2D pEscaped = Physics2D.OverlapBox((Vector2)transform.position + playerEscapeOffset, playerEscapeSize, 0f, playerLayer);
        Collider2D attack = Physics2D.OverlapBox((Vector2)transform.position + attackAreaOffset * transform.localScale.x, attackArea, 0f, playerLayer);

        Vector2 dir = (player.transform.position - transform.position).normalized;
        float distance = Vector2.Distance(transform.position, player.transform.position);
        RaycastHit2D ray = Physics2D.Raycast(transform.position, dir, distance * 2, obstacleLayer);

        if (pDetected != null)
        {
            inDetectArea = true;
            playerDetected = true;
            if (ray.collider == null) inAttackArea = true;
            else inAttackArea = false;
        }
        else if (pEscaped == null)
        {
            playerDetected = false;
            inDetectArea = false;
        }
        else if (playerDetected)
        {
            playerDetected = false;
            //inAttackArea = false;
        }

        //if (attack != null) inAttackArea = true;
        //else if (attack == null) inAttackArea = false;
    }
    private void JumpToPlayer(Rigidbody2D _rb, Transform _enemy, Transform _player, float _jumpForceY, float offsetX)
    {
        Debug.Log("Jump to player");
        _rb.MovePosition(_rb.position + new Vector2(0f, 0.1f));
        float g = Mathf.Abs(Physics2D.gravity.y * _rb.gravityScale);

        Vector2 enemyPos = _enemy.position;
        Vector2 playerPos = player.transform.position;

        float VelocitY = _jumpForceY;

        float time = (2 * VelocitY) / g;

        float direction = Mathf.Sign(playerPos.x - enemyPos.x);
        float targetX = playerPos.x + direction * offsetX;
        targetX = Mathf.Clamp(targetX - enemyPos.x, -maxJumpRange, maxJumpRange);
        float velocitX = targetX / time;


        _rb.linearVelocity = new Vector2(velocitX, VelocitY);
    }

    public void PlaySFX()
    {
        AudioManager.PlaySFX(SFXTYPE.BRAWLER_ATTACK, 0.5f);
    }
    private void MoveBack()
    {
        if (isFacingRight)
        {
            rb.linearVelocity = new Vector2(-moveSpeed, 0);
        }
        else if(!isFacingRight) 
        {
            rb.linearVelocity = new Vector2(moveSpeed, 0);
        }
    }
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.orange;
        Gizmos.DrawWireSphere((Vector2)transform.position + groundCheckOffset, groundCheckSize);

        Gizmos.color = Color.red;
        Gizmos.DrawWireCube((Vector2)transform.position + detectOffset, detectSize);

        Gizmos.color = Color.green;
        Gizmos.DrawWireCube((Vector2)transform.position + playerEscapeOffset, playerEscapeSize);

        Gizmos.color = Color.purple ;
        Gizmos.DrawWireCube((Vector2)transform.position + attackAreaOffset * transform.localScale.x, attackArea);

        if (player != null && inDetectArea)
        {
            if (!playerDetected)
                Gizmos.color = Color.green;
            else
                Gizmos.color = Color.red;

            Gizmos.DrawLine(transform.position, player.transform.position);
        }
    }
    void OnStateChanged(IState _state)
    {
        currentState = _state.GetType().Name;
    }
    

    public class MeleeEnemyIdleState : IState
    {
        private MeleeEnemy enemy;
        public MeleeEnemyIdleState(MeleeEnemy _enemy)
        {
            enemy = _enemy;
        }

        public void OnEnter()
        {
            //Debug.Log("Enter Melee Idle state");
        }
        public void OnUpdate()
        {

            if (enemy.playerDetected)
            {
                enemy.stateMachine.ChangeState(new MeleeEnemyChaseState(enemy));
            }
        }
        public void OnExit()
        {

        }
    }

    public class MeleeEnemyChaseState : IState
    {
        private MeleeEnemy enemy;
        private Vector2 prevposition;
        private float stuckCheck;
        private float attack_2_Cooldown;

        public MeleeEnemyChaseState(MeleeEnemy _enemy)
        {
            enemy = _enemy;
        }
        public void OnEnter()
        {
            enemy.meleeAttackCooldown = Random.Range(0.5f, 1.5f);
            enemy.jumpCooldown = Random.Range(1f, 2f);
        }
        public void OnUpdate()
        {
            if (enemy.health.currentCCState != CrowdControlState.None)
            {
                enemy.stateMachine.ChangeState(new MeleeEnemyCCState(enemy));
            }
            if (enemy.playerDetected)
            {

                if (Mathf.Abs(enemy.distanceToPlayer.x) >= Mathf.Abs(enemy.attackAreaOffset.x) && enemy.health.currentCCState == CrowdControlState.None && enemy.distanceToPlayer.x < 8f)
                {
                    enemy.animator.SetBool("isWalk", true);
                    enemy.FaceToPlayer();
                    enemy.rb.linearVelocity = new Vector2(enemy.moveSpeed * enemy.transform.localScale.x, enemy.rb.linearVelocityY);

                    // Unstuck from floor
                    if (enemy.rb.position == prevposition)
                        stuckCheck += Time.deltaTime;
                    else stuckCheck = 0f;
                    if (stuckCheck > 0.1f) enemy.rb.MovePosition(enemy.rb.position + new Vector2(0f, 0.02f));
                }
                else
                {
                    enemy.animator.SetBool("isWalk", false);
                }
                if (enemy.inAttackArea && enemy.distanceToPlayer.y < 2f)
                {
                    if (Mathf.Abs(enemy.distanceToPlayer.x) < 99 && Mathf.Abs(enemy.distanceToPlayer.x) > Mathf.Abs(enemy.attackAreaOffset.x * 2))
                    {
                        enemy.jumpTimer += Time.deltaTime;
                        if (enemy.jumpTimer >= enemy.jumpCooldown)
                        {
                            enemy.stateMachine.ChangeState(new MeleeEnemyAttackState(enemy));
                        }
                    }
                    else if (Mathf.Abs(enemy.distanceToPlayer.x) <= enemy.attackArea.x)
                    {
                        enemy.meleeAttackTimer += Time.deltaTime;

                        if (enemy.meleeAttackTimer >= enemy.meleeAttackCooldown)
                        {
                            enemy.stateMachine.ChangeState(new MeleeEnemyAttack2State(enemy));
                        }
                    }
                }
            }
            else enemy.animator.SetBool("isWalk", false);
            prevposition = enemy.rb.position;
        }
        public void OnExit()
        {
            enemy.animator.SetBool("isWalk", false);
        }
    }

    public class MeleeEnemyAttackState : IState
    {
        private MeleeEnemy enemy;
        private int state;
        private bool hasJumped;
        private bool exit;
        private float chargeTime;
        public MeleeEnemyAttackState(MeleeEnemy _enemy)
        {
            enemy = _enemy;
        }

        public void OnEnter()
        {
            hasJumped = false;
            enemy.jumpTimer = 0;
            chargeTime = 0f;
            state = Random.Range(1, 10);
            Debug.Log("randon"+state);

            if (enemy.player.transform.position.x < enemy.transform.position.x && enemy.isFacingRight)
            {
                enemy.Flip();
            }
            else if (enemy.player.transform.position.x > enemy.transform.position.x && !enemy.isFacingRight)
            {
                enemy.Flip();
            }
            enemy.animator.SetTrigger("jump");
            AudioManager.PlaySFX(SFXTYPE.BRALWER_CHARGE, 0.2f, pitch: 0.85f);
        }
        public void OnUpdate()
        {
            if (enemy.health.currentCCState != CrowdControlState.None)
            {
                enemy.stateMachine.ChangeState(new MeleeEnemyCCState(enemy));
            }

            if (chargeTime < 1f && !exit)
            {
                chargeTime += Time.deltaTime;
            }
            else if (!exit)
            {
                // Jump
                //if (enemy.rb.linearVelocity.y < 0)
                //{
                //    enemy.rb.gravityScale = enemy.gravityB;
                //}
                //else
                //{
                //    enemy.rb.gravityScale = enemy.gravityA;
                //}
                enemy.JumpToPlayer(enemy.rb, enemy.transform, enemy.player.transform, enemy.jumpForceY, enemy.distanceOffset);
                enemy.jumpHitbox.SetActive(true);
                enemy.jumpTrail.SetActive(true);
                exit = true;
                enemy.animator.SetTrigger("jump");

                enemy.StartCoroutine(Wait(0.3f));
            }
            if (hasJumped && exit && enemy.isGround)
            {
                enemy.stateMachine.ChangeState(new MeleeEnemyIdleState(enemy));
            }   
        }
        public void OnExit()
        {
            Debug.Log("Exit jump state");
            //enemy.distanceOffset = 0;
            enemy.rb.linearVelocity = Vector2.zero;
            enemy.jumpHitbox.SetActive(false);
            enemy.StartCoroutine(DisableTrail(0.2f));
        }

        private IEnumerator Wait(float _time)
        {
            yield return new WaitForSeconds(_time);
            hasJumped = true;
        }

        private IEnumerator DisableTrail(float _time)
        {
            yield return new WaitForSeconds(_time);
            enemy.jumpTrail.SetActive(false);
        }
    }

    public class MeleeEnemyAttack2State : IState
    {
        private MeleeEnemy enemy;

        public MeleeEnemyAttack2State   (MeleeEnemy _enemy)
        {
            enemy = _enemy;
        }

        public void OnEnter()
        {
            enemy.animator.SetTrigger("attack");
            enemy.isAttackFinished = false;
            enemy.meleeAttackTimer = 0;
            enemy.rb.linearVelocity = Vector3.zero;
            enemy.health.spriteRenderer.color = Color.orange;
            enemy.health.knockdownImmune = true;
            enemy.health.stunImmune = true;
            //AudioManager.PlaySFX(SFXTYPE.BRALWER_CHARGE, 0.2f);
        }
        public void OnUpdate()
        {
            if (enemy.health.currentCCState != CrowdControlState.None)
            {
                enemy.stateMachine.ChangeState(new MeleeEnemyCCState(enemy));
            }

            if (enemy.isAttackFinished)
            {
                enemy.stateMachine.ChangeState(new MeleeEnemyChaseState(enemy));
            }
        }
        public void OnExit()
        {
            enemy.health.knockdownImmune = false;
            enemy.health.stunImmune = false;
        }
    }
    public class MeleeEnemyCCState : IState
    {
        private MeleeEnemy enemy;
        private bool knockdown = false;
        private float elapsed;
        public MeleeEnemyCCState(MeleeEnemy _enemy)
        {
            enemy = _enemy;
        }

        public void OnEnter()
        {
            elapsed = 0f;
            //enemy.animator.SetTrigger("Stunned");
            if (enemy.health.currentCCState == CrowdControlState.Stunned)
            {
                enemy.animator.SetBool("isStun", true);
                knockdown = false;
            }
            else if (enemy.health.currentCCState == CrowdControlState.Knockdown)
            {
                enemy.animator.SetTrigger("knockdown");
                knockdown = true;
                enemy.getUp = false;
            }
        }

        public void OnUpdate()
        {
            if(elapsed < 0.2f)
            {
                elapsed += Time.deltaTime;
                return;
            }
            if (!knockdown)
            {
                if(enemy.health.currentCCState == CrowdControlState.Knockdown) enemy.stateMachine.ChangeState(new MeleeEnemyCCState(enemy));
            }
            if(enemy.health.currentCCState == CrowdControlState.Knockdown)
            {
                if (enemy.isGround)
                {
                    enemy.animator.SetTrigger("land");
                    enemy.animator.ResetTrigger("knockdown");
                    enemy.health.juggleTime = 0f;
                    enemy.health.stunImmune = true;
                }
                else
                {
                    enemy.animator.SetTrigger("knockdown");
                    enemy.animator.ResetTrigger("land");
                    enemy.health.juggleTime += Time.deltaTime;
                }
            }
            if (enemy.health.currentCCState == CrowdControlState.None)
            {
                if (knockdown)
                {
                    enemy.animator.ResetTrigger("land");
                    enemy.animator.SetTrigger("getup");
                    if(enemy.getUp) enemy.stateMachine.ChangeState(new MeleeEnemyChaseState(enemy));
                    return;
                }
                enemy.stateMachine.ChangeState(new MeleeEnemyChaseState(enemy));
            }
        }

        public void OnExit()
        {
            enemy.animator.SetBool("isStun", false);
            enemy.animator.ResetTrigger("land");
            enemy.animator.ResetTrigger("getup");
            enemy.health.isInArcKnockdown = false;
            enemy.health.juggleTime = 0f;
            enemy.health.stunImmune = false;
        }
    }
}
