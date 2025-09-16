using System.Collections;
using System.Xml.Serialization;
using Unity.VisualScripting;
using UnityEngine;

public class MeleeEnemy : Enemy
{
    [Header("Detaect")]
    [SerializeField] private Vector2 detectSize;
    [SerializeField] private Vector2 playerEscapeSize;
    [SerializeField] private Vector2 detectOffset;
    [SerializeField] private Vector2 playerEscapeOffset;
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

    

    protected override void Awake()
    {
        base.Awake();
        stateMachine = new StateMachine();

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

        if (Input.GetKeyDown(KeyCode.M))
        {
            rb.linearVelocity = new Vector2(2*transform.localScale.x, 10);
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

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.orange;
        Gizmos.DrawWireSphere((Vector2)transform.position + groundCheckOffset, groundCheckSize);

        Gizmos.color = Color.red;
        Gizmos.DrawWireCube((Vector2)transform.position + detectOffset, detectSize);

        Gizmos.color = Color.green;
        Gizmos.DrawWireCube((Vector2)transform.position + detectOffset, playerEscapeSize);

        if (player != null && inDetectArea)
        {
            if (!playerDetected)
                Gizmos.color = Color.green;
            else
                Gizmos.color = Color.red;

            Gizmos.DrawLine(transform.position, player.transform.position);
        }
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
                enemy.stateMachine.ChangeState(new MeleeEnemyAttackState(enemy));
            }
        }
        public void OnExit()
        {

        }
    }

    public class MeleeEnemyChaseState : IState
    {
        private MeleeEnemy enemy;
        public MeleeEnemyChaseState(MeleeEnemy _enemy)
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

    public class MeleeEnemyAttackState : IState
    {
        private MeleeEnemy enemy;
        public MeleeEnemyAttackState(MeleeEnemy _enemy)
        {
            enemy = _enemy;
        }

        public void OnEnter()
        {

        }
        public void OnUpdate()
        {
            if(enemy.rb.linearVelocity.y < 0)
            {
                enemy.rb.gravityScale = enemy.gravityB;
            }
            else
            {
                enemy.rb.gravityScale = enemy.gravityA;
            }

            if (enemy.isGround)
            {
                enemy.rb.linearVelocity = new Vector2(enemy.jumpForceX * enemy.transform.localScale.x, enemy.jumpForceY);
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
        public void OnExit()
        {

        }

       
    }

}
