using UnityEngine;
using UnityEngine.Rendering;

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
    [SerializeField] private bool playerDetected;
    [SerializeField] private bool inDetectArea;
    [SerializeField] private bool inAttackArea;

    [Header("Attack")]
    [SerializeField] private float thrustCooldown;
    [SerializeField] private float thrustTimer;
    [SerializeField] private bool isThrustFinished;

    [SerializeField] private float throwCooldown;
    [SerializeField] private float throwTimer;
    [SerializeField] private bool isThrowFinished;

    [SerializeField] private GameObject spearPrefab;
    [SerializeField] private Transform throwPoint;

    

    protected override void Awake()
    {
        base.Awake();
        stateMachine = new StateMachine();
        
        stateMachine.stateChanged += OnStateChanged;
    }

    void Start()
    {
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

    private void ThrowSpear()
    {
        Instantiate(spearPrefab,throwPoint.position, throwPoint.rotation);
        //Spear spear = ProjectileManager.instance.SpawnSpear(throwPoint.position, Quaternion.identity);
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

        public SpearmanChaseState(Spearman _enemy)
        {
            enemy = _enemy;
        }
        public void OnEnter()
        {
            enemy.thrustCooldown = Random.Range(1f, 2f);
            enemy.throwCooldown = Random.Range(1f, 3f);
        }
        public void OnUpdate()
        {
            if (enemy.health.currentCCState == CrowdControlState.Stunned)
            {
                enemy.stateMachine.ChangeState(new SpearmanCCState(enemy));
            }

            if (enemy.playerDetected)
            {
                enemy.FaceToPlayer();

                if (Mathf.Abs(enemy.distanceToPlayer.x) >= Mathf.Abs(enemy.attackAreaOffset.x))
                {
                    
                    enemy.rb.linearVelocity = new Vector2(enemy.moveSpeed * enemy.transform.localScale.x, 0);
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
            enemy.animator.SetTrigger("Thrust");
            enemy.isThrustFinished = false;
            enemy.thrustTimer = 0;
        }
        public void OnUpdate()
        {
            if (enemy.isThrustFinished)
            {
                enemy.stateMachine.ChangeState(new SpearmanChaseState(enemy));
            }
        }
        public void OnExit()
        {

        }
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
            enemy.animator.SetTrigger("Throw");
            enemy.isThrowFinished = false;
            enemy.throwTimer = 0;
        }
        public void OnUpdate()
        {
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
        public SpearmanCCState(Spearman _enemy) 
        { 
            enemy = _enemy; 
        }

        public void OnEnter()
        {
            //enemy.rb.linearVelocity = Vector2.zero;
            //enemy.animator.SetTrigger("Stunned");
        }

        public void OnUpdate()
        {
            if (enemy.health.currentCCState == CrowdControlState.None)
            {
                enemy.stateMachine.ChangeState(new SpearmanChaseState(enemy));
            }
        }

        public void OnExit() { }
    }

}
