using UnityEngine;
using UnityEngine.Rendering;
using static MeleeEnemy;

public class Spearman : Enemy
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
    [SerializeField] private float thrustCooldown;
    [SerializeField] private float thrustTimer;
    [SerializeField] private ParticleSystem thrustParticleSystem;
    [SerializeField] public bool isThrustFinished;
    [SerializeField] public bool isThrowFinished;

    [SerializeField] private float throwCooldown;
    [SerializeField] private float throwTimer;
    

    [SerializeField] private GameObject spearPrefab;
    [SerializeField] private Transform throwPoint;

    [Header("Collider")]
    [SerializeField] private GameObject thrustCollider;
    [SerializeField] private GameObject thrustCollider2;
    
    protected override void Awake()
    {
        base.Awake();
        stateMachine = new StateMachine();
        
        stateMachine.stateChanged += OnStateChanged;
    }

    protected override void Start()
    {
        base.Start();
        stateMachine.Initialize(new SpearmanIdleState(this));
    }

    protected override void Update()
    {
        base.Update();
        stateMachine.Update();
        DetectPlayer();

        if (Input.GetKeyDown(KeyCode.B))
        {
            ThrowSpear();
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
        RaycastHit2D ray = Physics2D.Raycast(transform.position, dir, distance, obstacleLayer);

        if (pDetected != null)
        {
            inDetectArea = true;
            playerDetected = true;
        }
        else if (pEscaped == null)
        {
            playerDetected = false;
            inDetectArea = false;
        }
        else if (playerDetected)
        {
            playerDetected = false;
        }

        if (attack != null) inAttackArea = true;
        else if (attack == null) inAttackArea = false;
    }

    

    public void ThrowSpear()
    {
        //Spear spear = ProjectileManager.instance.SpawnSpear(throwPoint.position, Quaternion.identity);

        GameObject spearObj = Instantiate(spearPrefab, throwPoint.position, Quaternion.identity);
        Spear spear = spearObj.GetComponentInChildren<Spear>();
        spear.SetOwner(this);
        spear.Init(attackDamage, this);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireCube((Vector2)transform.position + detectOffset, detectSize);

        Gizmos.color = Color.green;
        Gizmos.DrawWireCube((Vector2)transform.position + playerEscapeOffset, playerEscapeSize);

        Gizmos.color = Color.purple;
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
    public void EnableThrustCollider() => thrustCollider.SetActive(true);
    public void DisableThrustCollider() => thrustCollider.SetActive(false);

    public class SpearmanIdleState : IState
    {
        private Spearman enemy;

        public SpearmanIdleState(Spearman _enemy)
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
                enemy.stateMachine.ChangeState(new SpearmanChaseState(enemy));
            }
        }
        public void OnExit()
        {

        }
    }

    public class SpearmanChaseState : IState
    {
        private Spearman enemy;
        private Vector2 prevposition;
        private float stuckCheck;

        public SpearmanChaseState(Spearman _enemy)
        {
            enemy = _enemy;
        }
        public void OnEnter()
        {
            enemy.thrustCooldown = Random.Range(0.5f, 1.5f);
            enemy.throwCooldown = Random.Range(1.5f, 2.5f);
        }
        public void OnUpdate()
        {
            if (enemy.health.currentCCState != CrowdControlState.None)
            {
                enemy.stateMachine.ChangeState(new SpearmanCCState(enemy));
            }

            if (enemy.playerDetected)
            {
                enemy.FaceToPlayer();

                if (Mathf.Abs(enemy.distanceToPlayer.x) >= Mathf.Abs(enemy.attackAreaOffset.x) && enemy.health.currentCCState == CrowdControlState.None)
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


                if (Mathf.Abs(enemy.distanceToPlayer.x) < 99 && Mathf.Abs(enemy.distanceToPlayer.x) > Mathf.Abs(enemy.attackAreaOffset.x * 2))
                {
                    //SpearmanThrowtState
                    enemy.throwTimer += Time.deltaTime;
                    if (enemy.throwTimer >= enemy.throwCooldown)
                    {
                        enemy.stateMachine.ChangeState(new SpearmanThrowtState(enemy));
                    }
                }
                else
                {
                    enemy.thrustTimer += Time.deltaTime;

                    if (enemy.thrustTimer >= enemy.thrustCooldown)
                    {
                        enemy.stateMachine.ChangeState(new SpearmanThrustState(enemy));
                    }
                }
            }
            else enemy.animator.SetBool("isWalk", false);
            prevposition = enemy.rb.position;
        }
        public void OnExit()
        {

        }
    }

    public class SpearmanThrustState : IState
    {
        private Spearman enemy;

        public SpearmanThrustState(Spearman _enemy)
        {
            enemy = _enemy;
        }
        public void OnEnter()
        {
            enemy.animator.SetTrigger("thrust");
            enemy.isThrustFinished = false;
            enemy.thrustTimer = 0;
            enemy.health.spriteRenderer.color = Color.orange;
            enemy.health.knockdownImmune = true;
            enemy.health.stunImmune = true;
            //AudioManager.PlaySFX(SFXTYPE.SPEARMAN_CHARGE, 0.2f);
        }
        public void OnUpdate()
        {
            if (enemy.health.currentCCState != CrowdControlState.None)
            {
                enemy.stateMachine.ChangeState(new SpearmanCCState(enemy));
            }

            if (enemy.isThrustFinished)
            {
                enemy.stateMachine.ChangeState(new SpearmanChaseState(enemy));
            }
        }
        public void OnExit()
        {
            enemy.health.knockdownImmune = false;
            enemy.health.stunImmune = false;
        }
    }
    public void ThrustVFX()
    {
        thrustParticleSystem.Play();
    }
    public class SpearmanThrowtState : IState
    {
        private Spearman enemy;

        public SpearmanThrowtState(Spearman _enemy)
        {
            enemy = _enemy;
        }
        public void OnEnter()
        {
            enemy.animator.SetTrigger("throw");
            enemy.isThrowFinished = false;
            enemy.throwTimer = 0;
            AudioManager.PlaySFX(SFXTYPE.SPEARMAN_CHARGE, 0.2f, pitch: 1.4f);
        }
        public void OnUpdate()
        {
            if (enemy.health.currentCCState != CrowdControlState.None)
            {
                enemy.stateMachine.ChangeState(new SpearmanCCState(enemy));
            }
            if (enemy.isThrowFinished)
            {
                enemy.stateMachine.ChangeState(new SpearmanChaseState(enemy));
            }
        }
        public void OnExit()
        {

        }
    }

    public class SpearmanCCState : IState
    {
        private Spearman enemy;
        private bool knockdown;
        private float elapsed;
        public SpearmanCCState(Spearman _enemy) 
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
            if (elapsed < 0.2f)
            {
                elapsed += Time.deltaTime;
                return;
            }
            if (!knockdown)
            {
                if (enemy.health.currentCCState == CrowdControlState.Knockdown) enemy.stateMachine.ChangeState(new SpearmanCCState(enemy));
            }
            if (enemy.health.currentCCState == CrowdControlState.Knockdown)
            {
                if (enemy.isGround)
                {
                    enemy.animator.SetTrigger("land");
                    enemy.animator.ResetTrigger("knockdown");
                    enemy.health.invincible = true;
                }
                else
                {
                    enemy.animator.SetTrigger("knockdown");
                    enemy.animator.ResetTrigger("land");
                }
            }
            if (enemy.health.currentCCState == CrowdControlState.None)
            {
                if (knockdown)
                {
                    enemy.animator.ResetTrigger("land");
                    enemy.animator.SetTrigger("getup");
                    if (enemy.getUp) enemy.stateMachine.ChangeState(new SpearmanChaseState(enemy));
                    return;
                }
                enemy.stateMachine.ChangeState(new SpearmanChaseState(enemy));
            }
        }

        public void OnExit()
        {
            enemy.animator.SetBool("isStun", false);
            enemy.animator.ResetTrigger("land");
            enemy.animator.ResetTrigger("getup");
            enemy.health.invincible = false;
        }
    }
}
