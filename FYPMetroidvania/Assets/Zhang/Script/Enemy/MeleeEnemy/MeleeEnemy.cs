using System.Collections;
using Unity.VisualScripting;
using UnityEngine;

public class MeleeEnemy : Enemy
{
    [SerializeField] private string currentState;

    [Header("Detaect")]
    [SerializeField] private Vector2 detectSize;
    [SerializeField] private Vector2 playerEscapeSize;
    [SerializeField] private Vector2 detectOffset;
    [SerializeField] private Vector2 playerEscapeOffset;
    [SerializeField] private Vector2 attackArea;
    [SerializeField] private Vector2 attackAreaOffset;
    [SerializeField] private float groundCheckSize;
    [SerializeField] private Vector2 groundCheckOffset;

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

    [SerializeField] private float meleeAttackCooldown;
    [SerializeField] private float meleeAttackTimer;
    [SerializeField] private bool isAttackFinished;

    [SerializeField] private float jumpCooldown;
    [SerializeField] private float jumpTimer;

    protected override void Awake()
    {
        base.Awake();
        stateMachine = new StateMachine();

        stateMachine.stateChanged += OnStateChanged;
    }
    void Start()
    {
        stateMachine.Initialize(new MeleeEnemyIdleState(this));
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
        if (Input.GetKey(KeyCode.M))
        {
            if (isGround)
            {
                rb.linearVelocity = new Vector2(jumpForceX * transform.localScale.x, jumpForceY);   
            }
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

        Collider2D pDetected = Physics2D.OverlapBox((Vector2)transform.position + detectOffset, detectSize, 0f, playerLayer);
        Collider2D pEscaped = Physics2D.OverlapBox((Vector2)transform.position + playerEscapeOffset, playerEscapeSize, 0f, playerLayer);
        Collider2D attack = Physics2D.OverlapBox((Vector2)transform.position + attackAreaOffset * transform.localScale.x, attackArea, 0f, playerLayer);

        Vector2 dir = (player.transform.position - transform.position).normalized;
        float distance = Vector2.Distance(transform.position, player.transform.position);
        RaycastHit2D ray = Physics2D.Raycast(transform.position, dir, distance, obstacleLayer);

        if (pDetected != null && ray.collider == null)
        {
            inDetectArea = true;
            playerDetected = true;
        }
        else if (pEscaped == null)
        {
            playerDetected = false;
            inDetectArea = false;
        }
        else if (playerDetected && ray.collider != null)
        {
            playerDetected = false;
        }

        if (attack != null) inAttackArea = true;
        else if (attack == null) inAttackArea = false;
    }
    private void JumpToPlayer(Rigidbody2D _rb, Transform _enemy, Transform _player, float _jumpForceY, float offsetX)
    {
        float g = Mathf.Abs(Physics2D.gravity.y * _rb.gravityScale);

        Vector2 enemyPos = _enemy.position;
        Vector2 playerPos = _player.position;

        float VelocitY = _jumpForceY;

        float time = (2 * VelocitY) / g;

        float direction = Mathf.Sign(playerPos.x - enemyPos.x);
        float targetX = playerPos.x + direction * offsetX;

        float velocitX = (targetX - enemyPos.x) / time;

        _rb.linearVelocity = new Vector2(velocitX, VelocitY);
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

    public override void TakeDamage(float _damage, Vector2 _dir)
    {
        base.TakeDamage(_damage, _dir);


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

        private float attack_2_Cooldown;

        public MeleeEnemyChaseState(MeleeEnemy _enemy)
        {
            enemy = _enemy;
        }
        public void OnEnter()
        {
            enemy.meleeAttackCooldown = Random.Range(1f, 2f);
            enemy.jumpCooldown = Random.Range(1f, 3f);
        }
        public void OnUpdate()
        {
            if (enemy.playerDetected)
            {
                if (Mathf.Abs(enemy.distanceToPlayer.x) >= Mathf.Abs(enemy.attackAreaOffset.x))
                {
                    enemy.FaceToPlayer();
                    enemy.rb.linearVelocity = new Vector2(enemy.moveSpeed * enemy.transform.localScale.x, 0);
                }

                

                if (Mathf.Abs(enemy.distanceToPlayer.x) < 99 && Mathf.Abs(enemy.distanceToPlayer.x) > Mathf.Abs(enemy.attackAreaOffset.x * 2))
                {
                    enemy.jumpTimer += Time.deltaTime;
                    if(enemy.jumpTimer>= enemy.jumpCooldown)
                    {
                        enemy.stateMachine.ChangeState(new MeleeEnemyAttackState(enemy));
                    }
                }
                else
                {
                    enemy.meleeAttackTimer += Time.deltaTime;

                    if (enemy.meleeAttackTimer >= enemy.meleeAttackCooldown)
                    {
                        enemy.stateMachine.ChangeState(new MeleeEnemyAttack2State(enemy));
                    }
                }

                //if (Mathf.Abs(enemy.distanceToPlayer.x) < 99 && Mathf.Abs(enemy.distanceToPlayer.x) > 5.5f)
                //{
                //    if (Time.time >= lastAttackCheckTime + attackCheckCooldown)
                //    {
                //        lastAttackCheckTime = Time.time;

                    //        if (Random.value < jumpAttackProbability)
                    //        {
                    //            enemy.stateMachine.ChangeState(new MeleeEnemyAttackState(enemy));
                    //        }
                    //    }

                    //    if (Input.GetKey(KeyCode.C))
                    //    {
                    //        enemy.stateMachine.ChangeState(new MeleeEnemyAttackState(enemy));
                    //    }
                    //}
            }
        }
        public void OnExit()
        {
        }
    }

    public class MeleeEnemyAttackState : IState
    {
        private MeleeEnemy enemy;
        private int state;
        private bool hasJumped;
        public MeleeEnemyAttackState(MeleeEnemy _enemy)
        {
            enemy = _enemy;
        }

        public void OnEnter()
        {
            hasJumped = false;
            enemy.jumpTimer = 0;
            state = Random.Range(1, 10);
            Debug.Log("randon"+state);

            if (enemy.rb.linearVelocity.y < 0)
            {
                enemy.rb.gravityScale = enemy.gravityB;
            }
            else
            {
                enemy.rb.gravityScale = enemy.gravityA;
            }

            if (enemy.isGround)
            {
                //if (enemy.player.transform.position.x < enemy.transform.position.x)
                //{
                //    if (enemy.player.moveInput.x < 0) enemy.distanceOffset = 5;
                //    else if (enemy.player.moveInput.x > 0) enemy.distanceOffset = -3;
                //}
                //else if (enemy.player.transform.position.x > enemy.transform.position.x)
                //{
                //    if (enemy.player.moveInput.x < 0) enemy.distanceOffset = -3;
                //    else if (enemy.player.moveInput.x > 0) enemy.distanceOffset = 5;
                //}

                //enemy.rb.linearVelocity = new Vector2(enemy.jumpForceX, enemy.jumpForceY);

                enemy.JumpToPlayer(enemy.rb, enemy.transform, enemy.player.transform, enemy.jumpForceY, enemy.distanceOffset);

                enemy.StartCoroutine(Wait(0.1f));
            }

            if (enemy.player.transform.position.x < enemy.transform.position.x && enemy.isFacingRight)
            {
                enemy.Flip();
            }
            else if (enemy.player.transform.position.x > enemy.transform.position.x && !enemy.isFacingRight)
            {
                enemy.Flip();
            }
        }
        public void OnUpdate()
        {
            if (hasJumped && enemy.isGround)
            {
                //switch (state)
                //{
                //    case < 2:
                //        enemy.stateMachine.ChangeState(new MeleeEnemyIdleState(enemy));
                //        break;
                //    case >= 2:
                //        enemy.stateMachine.ChangeState(new MeleeEnemyIdleState(enemy));
                //        break;
                //}
                enemy.stateMachine.ChangeState(new MeleeEnemyIdleState(enemy));
            }   
        }
        public void OnExit()
        {
            enemy.distanceOffset = 0;
        }

        private IEnumerator Wait(float _time)
        {
            yield return new WaitForSeconds(_time);
            hasJumped = true;
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
            enemy.animator.SetTrigger("Attack");
            enemy.isAttackFinished = false;
            enemy.meleeAttackTimer = 0;
        }
        public void OnUpdate()
        {
            if(enemy.isAttackFinished)
            {
                enemy.stateMachine.ChangeState(new MeleeEnemyChaseState(enemy));
            }
        }
        public void OnExit()
        {

        }
    }
}
