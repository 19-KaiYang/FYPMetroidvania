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

    protected override void Awake()
    {
        base.Awake();
        stateMachine = new StateMachine();

        stateMachine.stateChanged += OnStateChanged;
    }
    void Start()
    {
        stateMachine.Initialize(new MeleeEnemyIdleState(this));

        Debug.Log(Random.value);
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

    public class MeleeEnemyIdleState : IState
    {
        private MeleeEnemy enemy;
        public MeleeEnemyIdleState(MeleeEnemy _enemy)
        {
            enemy = _enemy;

        }

        public void OnEnter()
        {
            Debug.Log("Enter Melee Idle state");
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

        private float jumpAttackProbability = 0.3f;
        private float attackCheckCooldown = 1f;
        private float lastAttackCheckTime;

        public MeleeEnemyChaseState(MeleeEnemy _enemy)
        {
            enemy = _enemy;
        }
        public void OnEnter()
        {

        }
        public void OnUpdate()
        {
            if (enemy.playerDetected)
            {
                if(Mathf.Abs(enemy.distanceToPlayer.x)<99 && Mathf.Abs(enemy.distanceToPlayer.x) > 6)
                {
                    //if (Time.time >= lastAttackCheckTime + attackCheckCooldown)
                    //{
                    //    lastAttackCheckTime = Time.time;

                    //    if (Random.value < jumpAttackProbability)
                    //    {
                    //        enemy.stateMachine.ChangeState(new MeleeEnemyAttackState(enemy));
                    //    }
                    //}

                    if (Input.GetKey(KeyCode.C))
                    {
                        enemy.stateMachine.ChangeState(new MeleeEnemyAttackState(enemy));
                    }
                    
                }
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

        }
        public void OnUpdate()
        {

        }
        public void OnExit()
        {

        }
    }
}
