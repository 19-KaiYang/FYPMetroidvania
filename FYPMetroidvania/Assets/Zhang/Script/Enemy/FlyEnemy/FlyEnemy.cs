using System.Collections;
using Unity.VisualScripting;
using UnityEngine;

public class FlyEnemy : Enemy
{
    [Header("Position")]
    [SerializeField] private Vector2 originalPosiiton;

    [Header("Move")]
    [SerializeField] private float chaseSpeed = 3f;
    [SerializeField] private float waveAmplitude = 0.3f;
    [SerializeField] private float waveFrequency = 3f;
    [SerializeField] private bool canFlip = true;

    [Header("Attack")]
    [SerializeField, Range(0f, 1f)] private float attackProbability = 0.3f;
    [SerializeField] private float attackCheckCooldown = 1f;
    private float lastAttackCheckTime;

    [SerializeField] private float attackMoveSpeed = 8f;
    private Vector2 dashDir;

    private bool stopAttack = false;

    [Header("Detaect")]
    [SerializeField] private float playerDetectDistance = 5f;
    [SerializeField] private float attackDistance = 3f;
    [SerializeField] private float playerEscapeDistance = 10f;

    [SerializeField] private bool playerDetected;
    [SerializeField] private bool inDetectArea;
    [SerializeField] private bool inAttackArea;

    [SerializeField] private string currentState;

    protected override void Awake()
    {
        base.Awake();
        stateMachine = new StateMachine();

        stateMachine.stateChanged += OnStateChanged;
    }

    void Start()
    {
        originalPosiiton = transform.position;
        stateMachine.Initialize(new FlyEnemyIdleState(this));
    }

    protected override void Update()
    {
        base.Update();

        DetectPlayer();
        stateMachine.Update();

        if (player.transform.position.x < transform.position.x && isFacingRight && canFlip)
        {
            Flip();
        }
        else if (player.transform.position.x > transform.position.x && !isFacingRight && canFlip)
        {
            Flip();
        }
    }

    private void DetectPlayer()
    {
        Collider2D pDetected = Physics2D.OverlapCircle(transform.position, playerDetectDistance, playerLayer);
        Collider2D pEscaped = Physics2D.OverlapCircle(transform.position, playerEscapeDistance, playerLayer);

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

        Collider2D attack = Physics2D.OverlapCircle(transform.position, attackDistance, playerLayer);
        if (attack != null && ray.collider == null)
        {
            inAttackArea = true;
        }
        else if (attack == null || ray.collider != null)
        {
            inAttackArea = false;
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        stopAttack = true;
    }
    private void OnCollisionExit2D(Collision2D collision)
    {
        stopAttack = false;
    }

    private void OnStateChanged(IState _state)
    {
        currentState = _state.GetType().Name;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, playerDetectDistance);
        Gizmos.color = Color.purple;
        Gizmos.DrawWireSphere(transform.position, attackDistance);

        if (player != null && inDetectArea)
        {
            if (!playerDetected)
                Gizmos.color = Color.green;
            else
                Gizmos.color = Color.red;

            Gizmos.DrawLine(transform.position, player.transform.position);
        }

        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, playerEscapeDistance);
    }

    public class FlyEnemyIdleState : IState
    {
        private FlyEnemy enemy;

        public FlyEnemyIdleState(FlyEnemy _enemy)
        {
            enemy = _enemy;
        }

        public void OnEnter()
        {
            Debug.Log("Enter FlyEnemy Idle State");
        }
        public void OnUpdate()
        {
            enemy.rb.linearVelocity = Vector2.zero;

            if (enemy.playerDetected)
            {
                enemy.stateMachine.ChangeState(new FlyEnemyChaseState(enemy));
            }
        }
        public void OnExit()
        {

        }
    }
    public class FlyEnemyChaseState : IState
    {
        private FlyEnemy enemy;

        public FlyEnemyChaseState(FlyEnemy _enemy)
        {
            enemy = _enemy;
        }

        public void OnEnter()
        {
            Debug.Log("Enter FlyEnemy Chase State");
        }
        public void OnUpdate()
        {
            float playerY = enemy.player.transform.position.y;
            float playerX = enemy.player.transform.position.x;
            Vector2 targetPos = new Vector2();

            if (playerX > enemy.transform.position.x)
            {
                targetPos = new Vector2(playerX + Mathf.Sin(Time.time) * 2 - 5, playerY + Mathf.Sin(Time.time) * 2 + 4);
            }
            else if (playerX < enemy.transform.position.x)
            {
                targetPos = new Vector2(playerX + Mathf.Sin(Time.time) * 2 + 5, playerY + Mathf.Sin(Time.time) * 2 + 3);
            }
            Vector2 dir = (targetPos - (Vector2)enemy.transform.position).normalized;

            Vector2 w = new Vector2(-dir.y, dir.x);
            Vector2 waveOffset = w * (Mathf.Sin(Time.time * enemy.waveFrequency) * enemy.waveAmplitude);
            Vector2 velocity = dir * enemy.chaseSpeed + waveOffset;
            //Vector2 velocity = dir * enemy.chaseSpeed;

            enemy.rb.linearVelocity = velocity;

            if (!enemy.playerDetected)
            {
                Vector2 dir2 = (enemy.originalPosiiton - (Vector2)enemy.transform.position).normalized;
                enemy.rb.linearVelocity = dir2 * enemy.chaseSpeed;

                if (Mathf.Abs(enemy.transform.position.x - enemy.originalPosiiton.x) <= 0.5 &&
                    Mathf.Abs(enemy.transform.position.y - enemy.originalPosiiton.y) <= 0.5)
                {
                    enemy.rb.linearVelocity = Vector2.zero;
                    enemy.stateMachine.ChangeState(new FlyEnemyIdleState(enemy));
                }
            }

            if (enemy.inAttackArea)
            {
                if (Time.time >= enemy.lastAttackCheckTime + enemy.attackCheckCooldown)
                {
                    enemy.lastAttackCheckTime = Time.time;

                    if (Random.value < enemy.attackProbability)
                    {
                        enemy.stateMachine.ChangeState(new FlyEnemyAttackState(enemy));
                    }
                }
            }
        }

        public void OnExit()
        {
            enemy.rb.linearVelocity = Vector2.zero;
        }
    }

    public class FlyEnemyAttackState : IState
    {
        private FlyEnemy enemy;
        public FlyEnemyAttackState(FlyEnemy _enemy)
        {
            enemy = _enemy;
        }

        public void OnEnter()
        {
            enemy.canFlip = false;

            enemy.StartCoroutine(WaitToAttack(0.4f));

            enemy.dashDir = (enemy.player.transform.position - enemy.transform.position).normalized;

            Vector2 direction = enemy.player.transform.position - enemy.transform.position;
            float angle = Mathf.Atan2(enemy.dashDir.y, enemy.dashDir.x) * Mathf.Rad2Deg;

            if (enemy.player.transform.position.x > enemy.transform.position.x)
                enemy.rb.rotation = angle;
            else if (enemy.player.transform.position.x < enemy.transform.position.x)
                enemy.rb.rotation = angle - 180;
        }
        public void OnUpdate()
        {
            if (enemy.stopAttack)
            {
                enemy.stateMachine.ChangeState(new FlyEnemyChaseState(enemy));
            }
        }
        public void OnExit()
        {
            enemy.canFlip = true;

            enemy.rb.linearVelocity = Vector2.zero;
            enemy.rb.rotation = 0f;
        }
        public IEnumerator WaitToAttack(float _time)
        {
            yield return new WaitForSeconds(_time);

            enemy.rb.linearVelocity = enemy.dashDir * enemy.attackMoveSpeed;
        }
    }
}
