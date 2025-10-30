using System.Collections;
using UnityEngine;
using static UnityEngine.CullingGroup;

public class TruckBoss : Enemy
{
    public enum BossPhase { Phase1, Phase2}
    private BossPhase bossPhase = BossPhase.Phase1;

    [Space(20)]
    [Header("TruckBoss")]
    [SerializeField] private string currentState;
    public BossPhase currentPhase = BossPhase.Phase1;
    private Vector2 rampTangent = Vector2.zero;
    private float zAngle = 0f;
    public float rotationSpeed;
    private float originalRotationSpeed;
    public float RampAttackSpeed = 10;
    public Vector2 rampAttackJumpForce;
    public float moveDistance;
    public bool reverse;
    public bool moving = true;

    [Header("Drive")]
    [SerializeField] private bool isDriving = false;
    [SerializeField] private float driveDirection;
    [SerializeField] private float driveDistance;
    [SerializeField] private float currentMoveSpeed;
    private Vector2 startPos;


    [Header("DriveAttack")]
    [SerializeField] private float waitTime;
    [SerializeField] private float driveSpeed = 1;
    private bool isRight;
    private enum DriveAttackStep
    {
        NONE,
        START,
        FORWARD,
        WAIT,
        BACKWARD,
        END
    }
    [SerializeField] private DriveAttackStep driveAttackStep = DriveAttackStep.NONE;


    [Header("RampAttack")]
    [SerializeField] private float chargeTime;
    [SerializeField] private bool summonFinished = false;
    [SerializeField] private bool triggerRamp = false;

    public LayerMask rampLayer;
    public GameObject rampSpawnPos;
    public GameObject rampPrefab;
    public float rampAttackProbability;
    
    private GameObject rightRamp;
    private Vector2 rightRampPos;
    private Vector2 distanceToRightRampPos;
    private GameObject leftRamp;
    private Vector2 leftRampPos;
    private Vector2 distanceToLeftRampPos;

    private enum RampAttackStep
    {
        NONE,
        START,
        SUMMONRAMP,
        FORWARD,
        WAIT,
        BACKWARD,
        END
    }
    [SerializeField] private RampAttackStep rampAttackStep = RampAttackStep.NONE;


    [Header("Detect")]
    [SerializeField] private Vector2 detectSize;
    [SerializeField] private Vector2 detectOffset;
    [SerializeField] private bool playerDetected = false;
    [Space]
    [SerializeField] private Vector2 attackArea;
    [SerializeField] private Vector2 attackAreaOffset;
    [SerializeField] private bool inAttackArea;
    [Space]
    [SerializeField] private Vector2 groundCheckSize;
    [SerializeField] private Vector2 groundCheckOffset;
    [SerializeField] private bool isGround;
    Coroutine myRoutine;
    protected override void Awake()
    {
        base.Awake();
        stateMachine = new StateMachine();
        stateMachine.stateChanged += OnStateChanged;
    }
    protected override void Start()
    {
        Debug.Log(transform.right);
        //Time.timeScale = 0.1f;
        originalRotationSpeed = rotationSpeed;
        stateMachine.Initialize(new TruckBossIdleState(this));

        rightRamp = GameObject.Find("RightRamp");
        rightRampPos=rightRamp.transform.position;
        leftRamp = GameObject.Find("LeftRamp");
        leftRampPos=leftRamp.transform.position;

    }
    protected override void Update()
    {
        base.Update();
        stateMachine.Update();
        Detect();

        distanceToLeftRampPos = (Vector2)transform.position - leftRampPos;
        distanceToRightRampPos = (Vector2)transform.position - rightRampPos;

        if (health.currentHealth <= health.maxHealth * 0.5)
        {
            bossPhase = BossPhase.Phase2;
        }

        float dir = transform.localScale.x;
        float vel = rb.linearVelocityX;
        bool moving = Mathf.Abs(vel) >= 6f;

        animator.SetBool("isForward", moving && dir * vel > 0);
        animator.SetBool("isReverse", moving && dir * vel < 0);


        if (Input.GetKeyDown(KeyCode.L))
        {
            //stateMachine.ChangeState(new TruckBossRampAttackState(this));
            StartDriveAttack();
            //StartRampAttack();
        }

        if (Input.GetKeyDown(KeyCode.M))
        {
            rb.linearVelocity = new Vector2(driveDirection * currentMoveSpeed, rb.linearVelocity.y);

        }
    }
    private void FixedUpdate()
    {
        Drive();
        DriveAttack();
        RampAttack();
    }
    public void StartDrive(float _dir, float _distance, float _speed)
    {
        currentMoveSpeed = _speed;
        if (isDriving) return;
        isDriving = true;
        driveDirection = _dir;
        currentMoveSpeed = _speed;
        driveDistance = _distance;
        startPos = transform.position;
    }
    private void Drive()
    {
        if (isDriving)
        {
            //rb.MovePosition(rb.position + (Vector2)transform.right * driveDirection * currentMoveSpeed * Time.fixedDeltaTime);
            rb.linearVelocity = new Vector2(driveDirection * currentMoveSpeed, rb.linearVelocity.y);

            if (Mathf.Abs(transform.position.x - startPos.x) >= driveDistance)
            {
                isDriving = false;
                rb.linearVelocity = Vector2.zero;
                //OnDriveEnd();
            }
        }
    }
    private void StartDriveAttack()
    {
        driveAttackStep = DriveAttackStep.START;
    }
    private void DriveAttack()
    {
        switch (driveAttackStep)
        {
            case DriveAttackStep.NONE:
                return;

            case DriveAttackStep.START:
                if (distanceToPlayer.x >= 0)
                {
                    FaceToPlayer();
                    StartDrive(1, Mathf.Abs(distanceToRightRampPos.x), moveSpeed * 0.3f);
                    isRight = true;
                }
                else if (distanceToPlayer.x <= 0)
                {
                    FaceToPlayer();
                    StartDrive(-1, Mathf.Abs(distanceToLeftRampPos.x), moveSpeed * 0.3f);
                    isRight = false;
                }
                driveAttackStep = DriveAttackStep.BACKWARD;
                break;

            case DriveAttackStep.BACKWARD:
                waitTime = 1;
                if (!isDriving) driveAttackStep = DriveAttackStep.WAIT;
                break;

            case DriveAttackStep.WAIT:
                waitTime-= Time.deltaTime;
                if (waitTime <= 0)
                {
                    if (isRight) StartDrive(-1, Mathf.Abs(distanceToLeftRampPos.x), moveSpeed * driveSpeed);
                    else if (!isRight) StartDrive(1, Mathf.Abs(distanceToRightRampPos.x), moveSpeed * driveSpeed);
                    driveAttackStep = DriveAttackStep.FORWARD;
                }
                break;

            case DriveAttackStep.FORWARD:
                if (!isDriving) driveAttackStep = DriveAttackStep.END;
                break;

            case DriveAttackStep.END:
                driveAttackStep = DriveAttackStep.NONE;
                Debug.Log("DriveAttack end");
                break;
        }
    }
    private void SummonRamp()
    {
        Vector2 rPos = new Vector2(player.transform.position.x, rampSpawnPos.transform.position.y);
        GameObject ramp = Instantiate(rampPrefab, rPos, rampPrefab.transform.rotation);
        Destroy(ramp, 5);
        Vector2 s = ramp.transform.localScale;
        if (distanceToPlayer.x >= 0)
        {
            s.x = -1;
            ramp.transform.localScale = s;
        }
    }
    private void RampAttackJump()
    {
        float gravity = Mathf.Abs(Physics2D.gravity.y);
        float vY = Mathf.Sqrt(2 * gravity * rampAttackJumpForce.y);
        float t = 2 * vY / gravity;
        float vX = rampAttackJumpForce.x / t;
        Debug.Log($"vX={vX}, vY={vY}, totalTime={t}");
        rb.linearVelocity = Vector2.zero;
        rb.linearVelocity = new Vector2(vX * transform.localScale.x, vY);
    }
    private void StartRampAttack()
    {
        rampAttackStep = RampAttackStep.START;
    }
    private void RampAttack()
    {
        switch (rampAttackStep)
        {
            case RampAttackStep.NONE:
                return;

            case RampAttackStep.START:
                FaceToPlayer();
                Debug.Log("MMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMM");
                rampAttackStep = RampAttackStep.SUMMONRAMP;
                break;

            case RampAttackStep.SUMMONRAMP:
                
                if (summonFinished)
                {
                    SummonRamp();
                    if (transform.localScale.x >= 0 && Mathf.Abs(distanceToLeftRampPos.x) >= 6.5f ||
                        transform.localScale.x <= 0 && Mathf.Abs(distanceToRightRampPos.x) >= 6.5f)
                    {
                        StartDrive(-transform.localScale.x, 5, moveSpeed * 0.3f);
                    }
                    rampAttackStep = RampAttackStep.BACKWARD;
                }
                break;

            case RampAttackStep.BACKWARD:
                chargeTime = 1;
                if (!isDriving) rampAttackStep = RampAttackStep.WAIT;
                break;

            case RampAttackStep.WAIT:
                chargeTime -= Time.deltaTime;
                if (chargeTime <= 0)
                {
                    //StartDrive(transform.localScale.x, Mathf.Abs(distanceToLeftRampPos.x), moveSpeed * driveSpeed);
                    //rb.linearVelocity = new Vector2(transform.localScale.x * currentMoveSpeed, rb.linearVelocity.y);
                    rb.AddForce(transform.localScale.x * transform.right * moveSpeed, ForceMode2D.Impulse);
                    rampAttackStep = RampAttackStep.FORWARD;
                }
                break;

            case RampAttackStep.FORWARD:
                if (triggerRamp && rb.linearVelocityY == 0)
                {
                    triggerRamp = false;
                    rampAttackStep = RampAttackStep.END;
                }
                break;

            case RampAttackStep.END:
                rampAttackStep = RampAttackStep.NONE;
                Debug.Log("RampAttackStep end");
                break;
        }
    }
    private void Detect()
    {
        Vector2 offset = new Vector2(groundCheckOffset.x * transform.localScale.x, groundCheckOffset.y);
        Collider2D ground = Physics2D.OverlapBox((Vector2)transform.position + offset, groundCheckSize, 0f, rampLayer);

        if (ground != null && ground.CompareTag("SideRamp")) rb.freezeRotation = false;
        else
        {
            rb.freezeRotation = true;
            Vector3 currentRotation = transform.eulerAngles;
            float newZ = Mathf.LerpAngle(currentRotation.z, 0, Time.deltaTime * rotationSpeed);
            if (Mathf.Abs(newZ) <= 0.5f) newZ = 0;
            transform.rotation = Quaternion.Euler(new Vector3(currentRotation.x, currentRotation.y, newZ));
        }
        if (ground != null && ground.CompareTag("Ramp"))
        {
            triggerRamp = true;
            RampAttackJump();
            ground.gameObject.tag = "Untagged";
        }

        Collider2D pDetected = Physics2D.OverlapBox((Vector2)transform.position + detectOffset, detectSize, 0f, playerLayer);
        Collider2D attack = Physics2D.OverlapBox((Vector2)transform.position + attackAreaOffset * transform.localScale.x, attackArea, 0f, playerLayer);

        Vector2 dir = (player.transform.position - transform.position).normalized;
        float distance = Vector2.Distance(transform.position, player.transform.position);
        RaycastHit2D ray = Physics2D.Raycast(transform.position, dir, distance, obstacleLayer);

        if (pDetected != null) playerDetected = true;
        //else if (pDetected == null) playerDetected = false;

        if (attack != null) inAttackArea = true;
        else if (attack == null) inAttackArea = false;
    }
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.orange;
        //Gizmos.DrawWireSphere((Vector2)transform.localPosition + groundCheckOffset, groundCheckSize);
        Vector2 offset = new Vector2(groundCheckOffset.x * transform.localScale.x, groundCheckOffset.y);
        Gizmos.DrawWireCube((Vector2)transform.position + offset, groundCheckSize);

        Gizmos.color = Color.red;
        Gizmos.DrawWireCube((Vector2)transform.position + detectOffset, detectSize);

        Gizmos.color = Color.purple;
        Gizmos.DrawWireCube((Vector2)transform.position + attackAreaOffset * transform.localScale.x, attackArea);
    }
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Ramp"))
        {

        }
    }
    void OnStateChanged(IState _state)
    {
        currentState = _state.GetType().Name;
    }


    //
    public class TruckBossIdleState : IState
    {
        private TruckBoss enemy;
        public TruckBossIdleState(TruckBoss _enemy)
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

    public class TruckBossPatrolState : IState
    {
        private TruckBoss enemy;
        public TruckBossPatrolState(TruckBoss _enemy)
        {
            enemy = _enemy;
        }
        public void OnEnter()
        {
            enemy.StartDrive(1, enemy.driveDistance, enemy.moveSpeed);
        }
        public void OnUpdate()
        {

        }
        public void OnExit()
        {

        }
    }

    public class TruckBossDriveAttackState : IState//phase1
    {
        private TruckBoss enemy;
        public TruckBossDriveAttackState(TruckBoss _enemy)
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

    public class TruckBossRampAttackState : IState//phase1
    {
        private TruckBoss enemy;
        private float randomFloat;

        public TruckBossRampAttackState(TruckBoss _enemy)
        {
            enemy = _enemy;
        }
        public void OnEnter()
        {
            //enemy.FaceToPlayer();
            //enemy.SpawnRamp();
            randomFloat=Random.value;
            Debug.Log("FAKSLJDAJKLSFHGAJLDKSGKJGFASKLJAGFSJ"+randomFloat);

            if (randomFloat < enemy.rampAttackProbability)
            {
                enemy.StartRampAttack();
            }
        }
        public void OnUpdate()
        {
            
            if (Input.GetKeyDown(KeyCode.P))
            {
                enemy.stateMachine.ChangeState(new TruckBossIdleState(enemy));
            } 
        }
        public void OnExit()
        {

        }
    }

    public class TruckBossDriftState : IState//phase1
    {
        private TruckBoss enemy;
        public TruckBossDriftState(TruckBoss _enemy)
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
