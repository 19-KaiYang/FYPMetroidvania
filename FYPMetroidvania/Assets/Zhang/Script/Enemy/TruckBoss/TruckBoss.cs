using DG.Tweening;
using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using static UnityEngine.CullingGroup;

public class TruckBoss : Enemy
{
    public enum BossPhase { Phase1, Phase2}
    [SerializeField]private BossPhase bossPhase = BossPhase.Phase1;
    private bool phase2 = false;
    public bool changePhase = false;
    public float currentHP;

    [Space(20)]
    [Header("TruckBoss")]
    [SerializeField] private string currentState;
    private Vector2 rampTangent = Vector2.zero;
    private float zAngle = 0f;
    public float rotationSpeed;
    public float RampAttackSpeed = 10;
    public Vector2 rampAttackJumpForce;
    public float moveDistance;
    public bool reverse;
    public bool moving = true;
    public bool isGround = true;

    [Header("Drive")]//¡ö¡ö¡ö¡ö¡ö¡ö¡ö¡ö¡ö¡ö
    [SerializeField] private bool isDriving = false;
    [SerializeField] private float driveDirection;
    [SerializeField] private float driveDistance;
    [SerializeField] private float currentMoveSpeed;
    [SerializeField] private float reverseSpeed;
    private Vector2 startPos;
    public bool canMove = false;

    [Header("DriveAttack")]//¡ö¡ö¡ö¡ö¡ö¡ö¡ö¡ö¡ö¡ö
    [SerializeField] private float waitTime;
    private float driveSpeed = 1;
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

    [Header("RampAttack")]//¡ö¡ö¡ö¡ö¡ö¡ö¡ö¡ö¡ö¡ö
    [SerializeField] private float chargeTime;
    [SerializeField] private bool summonFinished = false;
    [SerializeField] private bool triggerRamp = false;
    [SerializeField] private bool canJump = false;
    private float debugTimer;

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


    [Header("Burst")]//¡ö¡ö¡ö¡ö¡ö¡ö¡ö¡ö¡ö¡ö
    [SerializeField] public bool bFinished = false;

    [Header("Rampage")]//¡ö¡ö¡ö¡ö¡ö¡ö¡ö¡ö¡ö¡ö

    [SerializeField] private Vector2 rayPos;
    [SerializeField] private float rayLength = 5f;
    [SerializeField] private float delayTime;
    
    [SerializeField] private bool isOnRamp;
    [SerializeField] private bool rDetect = false;
    [SerializeField] private GameObject hitBox;
    [SerializeField] private GameObject ray;
    
    private float raywidth;

    private float delayTimer;
    private float posY; 
    private float gScale; 

    private enum RampageStep
    {
        NONE,
        START,
        MOVE,
        DELAY,
        DROP,
        END
    }
    [SerializeField] private RampageStep rampageStep = RampageStep.NONE;

    [Header("Slash Combo")]//¡ö¡ö¡ö¡ö¡ö¡ö¡ö¡ö¡ö¡ö
    [SerializeField] private float slashCharge1Time = 1;
    [SerializeField] private float slashCharge2Time = 1;
    private float slashChargeTimer;
    [SerializeField] public bool slashFinished;
    [SerializeField] public bool startSlash = false;
    [SerializeField] private bool hasFlipped = true;

    private enum SlashComboStep
    {
        NONE,
        START,
        CHARGE1,
        SLASH1,
        SLASH1_1,
        CHARGE2,
        SLASH2,
        END
    }
    [SerializeField] private SlashComboStep slashComboStep = SlashComboStep.NONE;

    [Header("Refuel")]//¡ö¡ö¡ö¡ö¡ö¡ö¡ö¡ö¡ö¡ö
    public List<GameObject> enemyPrefabs = new List<GameObject>();
    public List<Transform> spawnPoints = new List<Transform>();
    private List<GameObject> aliveEnemies = new List<GameObject>();
    public int totalEnemiesToSpawn = 2;
    [SerializeField] private float refuelTime = 10;
    private float refuelTimer;
    [SerializeField] private float dizzyTime = 5;
    private float dizzyTimer;
    [SerializeField] private bool enemyDead = false;
    [SerializeField] private bool SuccessRefuel = false;
    [SerializeField] private Transform refuelPos;
    private bool hasRefuel = false;
    [SerializeField] private GameObject particle;

    private enum RefuelStep
    {
        NONE,
        START,
        REFUEL,
        DIZZY,
        END
    }
    [SerializeField] private RefuelStep refuelStep = RefuelStep.NONE;

    [Header("Revving Rampage")]//¡ö¡ö¡ö¡ö¡ö¡ö¡ö¡ö¡ö¡ö
    [SerializeField] private float revChargeTime = 1.5f;
    private float revChargeTimer;
    [SerializeField] private float revDashSpeed = 25f;
    [SerializeField] private float revDashDistance = 20f;
    private int revDashCount;
    [SerializeField] private int minDashes = 2;
    [SerializeField] private int maxDashes = 3;
    private Vector2 revStartPos;
    private Vector2 revTargetPos;
    private bool revDashDone;
    public bool startRevving;

    private enum RevvingRampageStep
    {
        NONE,
        START,
        CHARGE,
        DASH,
        RETURN,
        END
    }
    [SerializeField] private RevvingRampageStep revvingRampageStep = RevvingRampageStep.NONE;

    [Header("Detect")]//¡ö¡ö¡ö¡ö¡ö¡ö¡ö¡ö¡ö¡ö
    [SerializeField] private Vector2 detectSize;
    [SerializeField] private Vector2 detectOffset;
    [SerializeField] private bool playerDetected = false;
    [Space]
    [SerializeField] private Vector2 attackArea;
    [SerializeField] private Vector2 attackAreaOffset;
    [SerializeField] private bool inAttackArea;
    [Space]
    [SerializeField] private float groundCheckSize;
    [SerializeField] private Vector2 groundCheckOffset;
    [SerializeField] private Vector2 rampCheckSize;
    [SerializeField] private Vector2 rampCheckOffset;

    Coroutine myRoutine;

    protected override void Awake()
    {
        base.Awake();
        stateMachine = new StateMachine();
        stateMachine.stateChanged += OnStateChanged;
    }
    protected override void Start()
    {
        Time.timeScale = 1.2f;
        stateMachine.Initialize(new TruckBossStayStillState(this));

        rightRamp = GameObject.Find("RightRamp");
        rightRampPos = rightRamp.transform.position;
        leftRamp = GameObject.Find("LeftRamp");
        leftRampPos = leftRamp.transform.position;

        gScale = rb.gravityScale;
        raywidth = ray.transform.localScale.x;
    }
    protected override void Update()
    {
        base.Update();
        Detect();
        distanceToLeftRampPos = (Vector2)transform.position - leftRampPos;
        distanceToRightRampPos = (Vector2)transform.position - rightRampPos;

        currentHP = health.currentHealth;

        if (bossPhase == BossPhase.Phase1 && health.currentHealth <= health.maxHealth * 0.5)
        {
            if (phase2) return;
            
            //stateMachine.ChangeState(null);
            if(driveAttackStep == DriveAttackStep.NONE &&
                rampAttackStep == RampAttackStep.NONE &&
                slashComboStep == SlashComboStep.NONE)
            {
                phase2 = true;
                bossPhase = BossPhase.Phase2;
                rb.linearVelocity = Vector2.zero;
                animator.SetTrigger("changePhase");

                stateMachine.ChangeState(new TruckBossIdleState(this));
            }
        }

        float dir = transform.localScale.x;
        float vel = rb.linearVelocityX;
        bool moving = Mathf.Abs(vel) >= 4f;
        animator.SetBool("isForward", moving && dir * vel > 0);
        animator.SetBool("isReverse", moving && dir * vel < 0);


        if (Input.GetKey(KeyCode.L))
        {
            stateMachine.ChangeState(new TruckBossRevvingRampageState(this));
        }

        if (Input.GetKeyDown(KeyCode.M))
        {
            health.currentHealth -= 10;
            //SummonEnemies();
        }
        //
    }
    private void FixedUpdate()
    {
        Drive();
        stateMachine.Update();
    }

    private void RotateGameObject()
    {
        var targetZAngle = Vector2.SignedAngle(Vector2.left, rampTangent);

        zAngle = Mathf.Lerp(zAngle, targetZAngle, Time.deltaTime * 99);
        transform.rotation = Quaternion.Euler(new Vector3(0, 0, zAngle));

        Vector2 groundRaycastDirection = isOnRamp ? Vector2.Perpendicular(rampTangent) : Vector2.down;
        Debug.DrawRay((Vector2)transform.position + rayPos, groundRaycastDirection * rayLength, Color.blue);

        RaycastHit2D hitResult = Physics2D.Raycast(
            origin: (Vector2)transform.position + rayPos,
            direction: groundRaycastDirection,
            distance: rayLength,
            layerMask: LayerMask.GetMask("Ramp"));
        isOnRamp = hitResult.collider != null;

        rampTangent = isOnRamp ? Vector2.Perpendicular(hitResult.normal) : Vector2.zero;
        Debug.DrawRay(hitResult.point, rampTangent, Color.yellow);
    }
    public void StartDrive(float _dir, float _distance, float _speed)
    {
        currentMoveSpeed = _speed;
        if (isDriving) return;
        isDriving = true;
        driveDirection = _dir;
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
                float value = new float[] { 0.8f, 1.6f, 2.3f }[Random.Range(0, 3)];
                driveSpeed = value;
                FaceToPlayer();
                if (distanceToPlayer.x >= 0)
                {
                    StartDrive(-Mathf.Sign(transform.localScale.x), Mathf.Abs(distanceToRightRampPos.x), moveSpeed * reverseSpeed);
                    isRight = true;
                }
                else if (distanceToPlayer.x <= 0)
                {
                    StartDrive(-Mathf.Sign(transform.localScale.x), Mathf.Abs(distanceToLeftRampPos.x), moveSpeed * reverseSpeed);
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
                    if (!canMove)
                    {
                        animator.SetTrigger("forwardStart");
                    }
                    if (canMove)
                    {
                        animator.ResetTrigger("forwardStart");
                        if (isRight) StartDrive(Mathf.Sign(transform.localScale.x), Mathf.Abs(distanceToLeftRampPos.x) - 3, moveSpeed * driveSpeed);
                        else if (!isRight) StartDrive(Mathf.Sign(transform.localScale.x), Mathf.Abs(distanceToRightRampPos.x) - 3, moveSpeed * driveSpeed);
                        driveAttackStep = DriveAttackStep.FORWARD;
                    }
                    
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
        Ramp rampGO = ramp.GetComponentInChildren<Ramp>();
        rampGO.SetOwner(this);
        rampGO.Init(attackDamage, this);
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
                rampAttackStep = RampAttackStep.SUMMONRAMP;
                break;

            case RampAttackStep.SUMMONRAMP:
                
                if (summonFinished)
                {
                    SummonRamp();
                    if (transform.localScale.x >= 0 && Mathf.Abs(distanceToLeftRampPos.x) >= 6.5f ||
                        transform.localScale.x <= 0 && Mathf.Abs(distanceToRightRampPos.x) >= 6.5f)
                    {
                        StartDrive(-transform.localScale.x, 5, moveSpeed * reverseSpeed);
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
                    //rb.AddForce(transform.localScale.x * transform.right * moveSpeed * 1.5f, ForceMode2D.Impulse);
                    if (!canMove)
                    {
                        animator.SetTrigger("forwardStart");
                    }
                    if (canMove)
                    {
                        animator.ResetTrigger("forwardStart");
                        canJump = true;
                        //StartDrive(Mathf.Sign(transform.localScale.x), Mathf.Abs(distanceToLeftRampPos.x), moveSpeed * reverseSpeed);
                        rb.AddForce(transform.localScale.x * transform.right * moveSpeed * 1.5f, ForceMode2D.Impulse);
                        debugTimer = 3f;
                        rampAttackStep = RampAttackStep.FORWARD;
                    }
                }
                break;

            case RampAttackStep.FORWARD:
                debugTimer-=Time.deltaTime;
                if (triggerRamp && rb.linearVelocityY == 0 || triggerRamp && isGround)
                {
                    triggerRamp = false;
                    rampAttackStep = RampAttackStep.END;
                }
                else if (debugTimer <= 0)
                {
                    triggerRamp = false;
                    rampAttackStep = RampAttackStep.END;
                }
                    break;

            case RampAttackStep.END:
                canJump = false;
                rampAttackStep = RampAttackStep.NONE;
                Debug.Log("RampAttackStep end");
                break;
        }
    }
    private void StartRampage()
    {
        rampageStep = RampageStep.START;
    }
    private void Rampage()
    {
        switch (rampageStep)
        {
            case RampageStep.NONE:
                return;

            case RampageStep.START:
                delayTime = Random.Range(1.5f, 4f);
                if (Mathf.Abs(distanceToRightRampPos.x) < Mathf.Abs(distanceToLeftRampPos.x))
                {
                    if (transform.localScale.x < 0) Flip();
                }
                else if(Mathf.Abs(distanceToLeftRampPos.x) < Mathf.Abs(distanceToRightRampPos.x))
                {
                    if (transform.localScale.x > 0) Flip();
                }
                rampageStep = RampageStep.MOVE;
                break;

            case RampageStep.MOVE:

                if (isOnRamp) rb.linearVelocity = Mathf.Sign(transform.localScale.x) * moveSpeed * transform.right;
                else rb.linearVelocity = new Vector2(Mathf.Sign(transform.localScale.x) * moveSpeed, rb.linearVelocityY);
                if (transform.eulerAngles.z >= 70) rb.AddForce(-transform.up * 70);

                if (rDetect == true)
                {
                    rb.bodyType = RigidbodyType2D.Kinematic;
                    rb.linearVelocity = Vector2.zero;
                    delayTimer = delayTime;
                    rampageStep = RampageStep.DELAY;
                }
                break;

            case RampageStep.DELAY:
                delayTimer -= Time.deltaTime;
                if (delayTimer <= delayTime - 0.8f)
                {
                    ray.SetActive(true);
                    float newX = Mathf.Lerp(0.5f, 5.0f, delayTimer / delayTime);
                    ray.transform.localScale = new Vector3(newX, 25, transform.localScale.z);
                }

                transform.position = new Vector3(
                    player.transform.position.x, 
                    posY + 5, 
                    transform.position.z);

                if(delayTimer <= 0 && rampageStep == RampageStep.DELAY)
                {
                    rb.bodyType = RigidbodyType2D.Dynamic;
                    rb.gravityScale = 10;
                    ray.SetActive(false);
                    ray.transform.localScale = new Vector3(raywidth, 25, transform.localScale.z);
                    hitBox.SetActive(true);
                    rampageStep = RampageStep.DROP;
                }
                break;

            case RampageStep.DROP:
                if (isGround || Mathf.Abs(rb.linearVelocityY) <= 0.08)
                {
                    rDetect = false;
                    hitBox.SetActive(false);
                    rb.gravityScale = gScale;
                    rampageStep = RampageStep.END;
                }
                break;

            case RampageStep.END:
                rampageStep = RampageStep.NONE;
                break;
        }
    }
    private void StartSlashCombo()
    {
        slashComboStep = SlashComboStep.START;
    }
    private void SlashCombo()   
    {
        switch (slashComboStep)
        {
            case SlashComboStep.NONE:
                return;

            case SlashComboStep.START:
                float value1 = new float[] { 0.1f, 1.0f}[Random.Range(0, 2)];
                float value2 = new float[] { 0.1f, 1.0f}[Random.Range(0, 2)];
                slashCharge1Time = value1;
                slashCharge2Time = value2;
                slashChargeTimer = slashCharge1Time;
                animator.SetTrigger("startSlash");
                FaceToPlayer();
                slashComboStep = SlashComboStep.CHARGE1;
                break;

            case SlashComboStep.CHARGE1:
                if (startSlash)
                {
                    slashChargeTimer -= Time.deltaTime;
                    animator.SetBool("isSlashCharging", true);
                }

                if (slashChargeTimer <= 0)
                {
                    //animator.SetBool("isSlashCharging", false);
                    //animator.SetTrigger("slash1");
                    //slashComboStep = SlashComboStep.SLASH1;

                    float r = Random.value;
                    if (r < 0.8f)
                    {
                        FaceToPlayer();
                        animator.SetBool("isSlashCharging", false);
                        animator.SetTrigger("slash1");
                        slashComboStep = SlashComboStep.SLASH1;

                        //float rr = Random.value;
                        //if (rr < 0.0f)
                        //{
                        //    FaceToPlayer();
                        //    animator.SetBool("isSlashCharging", false);
                        //    animator.SetTrigger("slash1");
                        //    slashComboStep = SlashComboStep.SLASH1;
                        //}
                        //else if (rr < 1.0f)
                        //{
                        //    FaceToPlayer();
                        //    StartDrive(Mathf.Sign(transform.localScale.x), Mathf.Abs(distanceToPlayer.x) - 5, moveSpeed * 3);
                        //    slashComboStep = SlashComboStep.SLASH1_1;
                        //}
                    }
                    else if (r < 1.0f)
                    {
                        slashComboStep = SlashComboStep.END;
                    }
                }
                break;

            //case SlashComboStep.SLASH1_1:
            //    if (!isDriving)
            //    {
            //        animator.SetBool("isSlashCharging", false);
            //        animator.SetTrigger("slash1");
            //        slashComboStep = SlashComboStep.SLASH1;
            //    }
            //    break;

            case SlashComboStep.SLASH1:
                startSlash = false;
                slashChargeTimer = slashCharge2Time;
                if (slashFinished)
                {
                    animator.SetTrigger("startSlash");
                    FaceToPlayer();
                    slashComboStep = SlashComboStep.CHARGE2;
                }
                break;

            case SlashComboStep.CHARGE2:
                slashFinished = false;
                
                if (startSlash)
                {
                    animator.SetBool("isSlashCharging", true);
                    slashChargeTimer -= Time.deltaTime;
                }
                if (slashChargeTimer <= 0)
                {
                    animator.SetBool("isSlashCharging", false);
                    FaceToPlayer();
                    if (distanceToPlayer.y <= 1)
                    {
                        float r = Random.value;
                        if (r < 0.3f) animator.SetTrigger("slash2");//down
                        else if (r < 1.0f) animator.SetTrigger("slash3");//up
                    }
                    else
                    {
                        float r = Random.value;
                        if (r < 0.3f) animator.SetTrigger("slash3");
                        else if (r < 1.0f) animator.SetTrigger("slash2");
                    }
                    slashComboStep = SlashComboStep.SLASH2;
                }
                break;

            case SlashComboStep.SLASH2:
                startSlash = false;
                if (slashFinished) slashComboStep = SlashComboStep.END;
                break;

            case SlashComboStep.END:
                animator.SetBool("isSlashCharging", false);
                startSlash = false;
                slashFinished = false;
                slashComboStep = SlashComboStep.NONE;
                break;
        }
    }
    private void SummonEnemies()
    {
        aliveEnemies.Clear();

        for (int i = 0; i < totalEnemiesToSpawn; i++)
        {
            if (enemyPrefabs.Count == 0) return;

            GameObject prefab = enemyPrefabs[Random.Range(0, enemyPrefabs.Count)];
            Transform spawnPoint = spawnPoints.Count > 0
                ? spawnPoints[i % spawnPoints.Count]
                : transform;

            GameObject enemy = Instantiate(prefab, spawnPoint.position, spawnPoint.rotation);
            aliveEnemies.Add(enemy);

            Health hp = enemy.GetComponent<Health>();
            if (hp != null && hp.gameObject != this.gameObject)
            {
                hp.enemyDeath += OnEnemyDeath;
            }
        }
    }
    private void OnEnemyDeath(GameObject deadEnemy)
    {
        aliveEnemies.Remove(deadEnemy);

        if (aliveEnemies.Count == 0 && !SuccessRefuel)
        {
            enemyDead = true;
            Debug.Log($"All summoned eneny{deadEnemy.name} dead");
        }
    }
    private void StartRefuel()
    {
        refuelStep = RefuelStep.START;
    }
    private void Refuel()
    {
        switch (refuelStep)
        {
            case RefuelStep.NONE:
                return;

            case RefuelStep.START:
                SummonEnemies();
                refuelTimer = refuelTime;
                
                refuelStep =RefuelStep.REFUEL;
                break;

            case RefuelStep.REFUEL:
                refuelTimer -= Time.deltaTime;
                //rb.MovePosition((Vector2)refuelPos.position);
                rb.MovePosition(Vector2.MoveTowards(transform.position, refuelPos.position, moveSpeed * 2 * Time.deltaTime));
                if (Vector3.Distance(transform.position, refuelPos.position) < 0.1f)
                {
                    particle.SetActive(true);
                    animator.SetBool("isRefuel", true);
                }
                
                if (enemyDead)
                {
                    animator.SetBool("isRefuel", false);
                    particle.SetActive(false);
                    refuelStep = RefuelStep.DIZZY;
                    dizzyTimer = dizzyTime;
                }
                else if (refuelTimer <= 0)
                {
                    SuccessRefuel = true;
                    animator.SetBool("isRefuel", false);
                    health.currentHealth += health.maxHealth * 0.3f;
                    if(health.currentHealth > health.maxHealth) health.currentHealth = health.maxHealth;

                    var enemiesToKill = new List<GameObject>(aliveEnemies);
                    foreach (GameObject enemy in enemiesToKill)
                    {
                        if (enemy != null)
                        {
                            Health hp = enemy.GetComponent<Health>();
                            hp.TakeDamage(hp.maxHealth);
                        }
                    }
                    aliveEnemies.Clear();
                    particle.SetActive(false);
                    refuelStep = RefuelStep.END;
                }
                break;

            case RefuelStep.DIZZY:
                dizzyTimer -= Time.deltaTime;
                animator.SetBool("isDizzy", true);
                if (dizzyTimer <= 0)
                {
                    animator.SetBool("isDizzy", false);
                    refuelStep = RefuelStep.END;
                }
                break;

            case RefuelStep.END:
                SuccessRefuel = false;
                enemyDead = false;
                refuelStep = RefuelStep.NONE;
                break;
        }
    }
    private void StartRevvingRampage()
    {
        revDashCount = Random.Range(minDashes, maxDashes + 1);
        revvingRampageStep = RevvingRampageStep.START;
    }
    private void RevvingRampage()
    {
        Debug.Log("ASDFGHJK" + revDashCount);
        switch (revvingRampageStep)
        {
            case RevvingRampageStep.NONE:
                return;
            case RevvingRampageStep.START:
                revChargeTimer = revChargeTime;
                animator.SetTrigger("startRevving");
                FaceToPlayer();
                revvingRampageStep = RevvingRampageStep.CHARGE;
                break;

            case RevvingRampageStep.CHARGE:
                
                if (!isDriving)
                {
                    
                    animator.SetBool("isDash", false);
                    if (startRevving)
                    {
                        animator.SetBool("isRevvingCharging", true);
                        revChargeTimer -= Time.deltaTime;
                    }
                    if (!hasFlipped)
                    {
                        Flip();
                        hasFlipped = true;
                    }
                }
                if (revChargeTimer <= 0)
                {
                    hasFlipped = false;
                    revvingRampageStep = RevvingRampageStep.DASH;
                }
                break;

            case RevvingRampageStep.DASH:
                revDashCount -= 1;
                if (revDashCount >= 0)
                {
                    animator.SetBool("isDash", true);
                    animator.SetBool("isRevvingCharging", false);
                    if (isFacingRight) StartDrive(Mathf.Sign(transform.localScale.x), Mathf.Abs(distanceToRightRampPos.x) - 3, moveSpeed * 2);
                    else StartDrive(Mathf.Sign(transform.localScale.x), Mathf.Abs(distanceToLeftRampPos.x) - 3, moveSpeed * 2);

                    animator.SetTrigger("startRevving");
                    revvingRampageStep = RevvingRampageStep.CHARGE;
                    revChargeTimer = revChargeTime;
                }
                else revvingRampageStep = RevvingRampageStep.RETURN;

                break;

            case RevvingRampageStep.RETURN:
                animator.SetBool("isRevvingCharging", false);
                Vector2 midPos = new Vector2((leftRampPos.x + rightRampPos.x) / 2, transform.position.y);
                StartDrive(Mathf.Sign(transform.localScale.x), Mathf.Abs(transform.position.x - midPos.x), moveSpeed * 2);
                revvingRampageStep = RevvingRampageStep.END;
                break;

            case RevvingRampageStep.END:
                hasFlipped = true;
                if(!isDriving) revvingRampageStep = RevvingRampageStep.NONE;
                break;
        }
    }
    private void Detect()
    {
        //groundCheck
        Collider2D ground = Physics2D.OverlapCircle((Vector2)transform.position + groundCheckOffset, groundCheckSize, groundleLayer);
        if (ground != null) isGround = true;
        else isGround = false;

        Vector2 offset = new Vector2(rampCheckOffset.x * transform.localScale.x, rampCheckOffset.y);
        Collider2D ramp = Physics2D.OverlapBox((Vector2)transform.position + offset, rampCheckSize, 0f, rampLayer);

        if (ramp != null && ramp.CompareTag("SideRamp")) rb.freezeRotation = false;
        else
        {
            rb.freezeRotation = true;
            Vector3 currentRotation = transform.eulerAngles;
            float newZ = Mathf.LerpAngle(currentRotation.z, 0, Time.deltaTime * rotationSpeed);
            if (Mathf.Abs(newZ) <= 0.5f) newZ = 0;
            transform.rotation = Quaternion.Euler(new Vector3(currentRotation.x, currentRotation.y, newZ));
        }
        if (ramp != null && ramp.CompareTag("Ramp") && canJump)
        {
            triggerRamp = true;
            RampAttackJump();
            ramp.gameObject.tag = "Untagged";
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
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere((Vector2)transform.position + groundCheckOffset, groundCheckSize);

        Gizmos.color = Color.brown;
        Vector2 offset = new Vector2(rampCheckOffset.x * transform.localScale.x, rampCheckOffset.y);
        Gizmos.DrawWireCube((Vector2)transform.position + offset, rampCheckSize);

        Gizmos.color = Color.red;
        Gizmos.DrawWireCube((Vector2)transform.position + detectOffset, detectSize);

        Gizmos.color = Color.purple;
        Gizmos.DrawWireCube((Vector2)transform.position + attackAreaOffset * transform.localScale.x, attackArea);
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.name == "AttackDetection" && rDetect == false)
        {
            posY = transform.position.y;
            rDetect = true;
        }
    }
    void OnStateChanged(IState _state)
    {
        currentState = _state.GetType().Name;
    }


    //
    public class TruckBossStayStillState : IState
    {
        private TruckBoss enemy;
        public TruckBossStayStillState(TruckBoss _enemy)
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
                enemy.stateMachine.ChangeState(new TruckBossIdleState(enemy));
            }
        }
        public void OnExit()
        {
        }
    }
    public class TruckBossIdleState : IState
    {
        private TruckBoss enemy;
        private float randomIdleTime;
        public TruckBossIdleState(TruckBoss _enemy)
        {
            enemy = _enemy;
        }
        public void OnEnter()
        {
            randomIdleTime = Random.Range(1f, 3.0f);
            //enemy.StartCoroutine(IdleTIme(randomIdleTime));
        }
        public void OnUpdate()
        {
            if(enemy.bossPhase == BossPhase.Phase1 || enemy.bossPhase == BossPhase.Phase2 && enemy.changePhase) randomIdleTime -= Time.deltaTime;

            if (randomIdleTime < 0f)
            {
                if (enemy.bossPhase == BossPhase.Phase1)
                {
                    if (enemy.inAttackArea)
                    {
                        float r = Random.value;
                        if (r < 0.4f)
                        {
                            enemy.stateMachine.ChangeState(new TruckBossBurstState(enemy));
                        }
                        else if(r < 0.6f)
                        {
                            enemy.stateMachine.ChangeState(new TruckBossDriveAttackState(enemy));
                        }
                        else if (r < 1.0f)
                        {
                            enemy.stateMachine.ChangeState(new TruckBossRampAttackState(enemy));
                        }
                    }
                    else if (Mathf.Abs(enemy.player.transform.position.x - enemy.rightRampPos.x) >= 7f
                        && Mathf.Abs(enemy.player.transform.position.x - enemy.leftRampPos.x) >= 7f)
                    {
                        float r = Random.value;
                        if (r < 0.4f)
                        {
                            enemy.stateMachine.ChangeState(new TruckBossRampAttackState(enemy));
                        }
                        else if (r < 0.8f)
                        {
                            enemy.stateMachine.ChangeState(new TruckBossDriveAttackState(enemy));
                        }
                        else if (r < 1.0f)
                        {
                            enemy.stateMachine.ChangeState(new TruckBossBurstState(enemy));
                        }
                    }
                    else
                    {
                        float r = Random.value;
                        if (r < 0.7f)
                        {
                            enemy.stateMachine.ChangeState(new TruckBossDriveAttackState(enemy));
                        }
                        else if (r < 1.0f)
                        {
                            enemy.stateMachine.ChangeState(new TruckBossBurstState(enemy));
                        }
                    }
                }
                else if (enemy.changePhase)
                {
                    //enemy.stateMachine.ChangeState(new TruckBossRampageState(enemy));
                    //enemy.stateMachine.ChangeState(new TruckBossSlashState(enemy));
                    if (enemy.health.currentHealth <= enemy.health.maxHealth * 0.3f && !enemy.hasRefuel)
                    {
                        enemy.stateMachine.ChangeState(new TruckBossRefuelState(enemy));
                        enemy.hasRefuel = true;
                    }
                    else
                    {
                        if (enemy.inAttackArea)
                        {
                            float r = Random.value;
                            if (r < 0.5f)
                            {
                                enemy.stateMachine.ChangeState(new TruckBossSlashState(enemy));
                            }
                            else if (r < 0.7f)
                            {
                                enemy.stateMachine.ChangeState(new TruckBossRampageState(enemy));
                            }
                            else if (r < 0.9f)
                            {
                                enemy.stateMachine.ChangeState(new TruckBossRevvingRampageState(enemy));
                            }
                            else if (r < 1.0f && enemy.health.currentHealth <= enemy.health.maxHealth * 0.3f && enemy.hasRefuel)
                            {
                                enemy.stateMachine.ChangeState(new TruckBossRefuelState(enemy));
                            }
                        }
                        else
                        {
                            float r = Random.value;
                            if (r < 0.2f)
                            {
                                enemy.stateMachine.ChangeState(new TruckBossSlashState(enemy));
                            }
                            else if (r < 0.5f)
                            {
                                enemy.stateMachine.ChangeState(new TruckBossRampageState(enemy));
                            }
                            else if (r < 0.9f)
                            {
                                enemy.stateMachine.ChangeState(new TruckBossRevvingRampageState(enemy));
                            }
                            else if (r < 1.0f && enemy.health.currentHealth <= enemy.health.maxHealth * 0.3f && enemy.hasRefuel)
                            {
                                enemy.stateMachine.ChangeState(new TruckBossRefuelState(enemy));
                            }
                        }
                    }
                }
            }
        }
        public void OnExit()
        {
        }
        private IEnumerator IdleTIme(float time)
        {
            yield return new WaitForSeconds(time);

            if (enemy.changePhase)
            {
                enemy.stateMachine.ChangeState(new TruckBossRampageState(enemy));
                yield break;
            }

            if (enemy.bossPhase == BossPhase.Phase1)
            {
                if (enemy.inAttackArea)
                {
                    float r = Random.value;
                    if (r < 0.5f)
                    {
                        enemy.stateMachine.ChangeState(new TruckBossBurstState(enemy));
                    }
                    else
                    {
                        enemy.stateMachine.ChangeState(new TruckBossDriveAttackState(enemy));
                    }
                }
                else if (Mathf.Abs(enemy.player.transform.position.x - enemy.rightRampPos.x) >= 7f
                    && Mathf.Abs(enemy.player.transform.position.x - enemy.leftRampPos.x) >= 7f)
                {
                    float r = Random.value;
                    if (r < 0.5f)
                    {
                        enemy.stateMachine.ChangeState(new TruckBossRampAttackState(enemy));
                    }
                    else if (r < 0.8f)
                    {
                        enemy.stateMachine.ChangeState(new TruckBossDriveAttackState(enemy));
                    }
                    else if (r < 1.0f)
                    {
                        enemy.stateMachine.ChangeState(new TruckBossBurstState(enemy));
                    }
                }
                else
                {
                    float r = Random.value;
                    if (r < 0.7f)
                    {
                        enemy.stateMachine.ChangeState(new TruckBossDriveAttackState(enemy));
                    }
                    else if (r < 1.0f)
                    {
                        enemy.stateMachine.ChangeState(new TruckBossBurstState(enemy));
                    }
                }
            }
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
        }
        public void OnUpdate()
        {

        }
        public void OnExit()
        {

        }
    }//phase1

    public class TruckBossDriveAttackState : IState
    {
        private TruckBoss enemy;
        public TruckBossDriveAttackState(TruckBoss _enemy)
        {
            enemy = _enemy;
        }
        public void OnEnter()
        {
            enemy.StartDriveAttack();
        }
        public void OnUpdate()
        {
            enemy.DriveAttack();

            if (enemy.driveAttackStep == DriveAttackStep.NONE)
            {
                float r = Random.value;
                if (r < 0.2f)
                {
                    enemy.stateMachine.ChangeState(new TruckBossDriveAttackState(enemy));
                }
                else if (r < 1.0f)
                {
                    enemy.stateMachine.ChangeState(new TruckBossIdleState(enemy));
                }
            }
        }
        public void OnExit()
        {
            //enemy.driveAttackStep = DriveAttackStep.NONE;
        }
    }//phase1

    public class TruckBossRampAttackState : IState
    {
        private TruckBoss enemy;

        public TruckBossRampAttackState(TruckBoss _enemy)
        {
            enemy = _enemy;
        }
        public void OnEnter()
        {

            enemy.StartRampAttack();
        }
        public void OnUpdate()
        {
            enemy.RampAttack();

            if (enemy.rampAttackStep == RampAttackStep.NONE)
            {
                enemy.stateMachine.ChangeState(new TruckBossIdleState(enemy));
            }
        }
        public void OnExit()
        {
            //enemy.rampAttackStep = RampAttackStep.NONE;
        }
    }//phase1

    public class TruckBossBurstState : IState
    {
        private TruckBoss enemy;
        public TruckBossBurstState(TruckBoss _enemy)
        {
            enemy = _enemy;
        }
        public void OnEnter()
        {
            enemy.bFinished = false;
            enemy.animator.SetTrigger("Burst");
        }
        public void OnUpdate()
        {
            if (enemy.bFinished)
            {
                float r = Random.value;
                if (r < 0.3f)
                {
                    enemy.stateMachine.ChangeState(new TruckBossDriveAttackState(enemy));
                }
                else if (r < 1.0f)
                {
                    enemy.stateMachine.ChangeState(new TruckBossIdleState(enemy));
                }
            }
        }
        public void OnExit()
        {

        }
    }//phase1


    public class TruckBossSlashState : IState
    {
        private TruckBoss enemy;
        public TruckBossSlashState(TruckBoss _enemy)
        {
            enemy = _enemy;
        }
        public void OnEnter()
        {
            enemy.StartSlashCombo();
        }
        public void OnUpdate()
        {   
            enemy.SlashCombo();
            if (enemy.slashComboStep == SlashComboStep.NONE)
            {
                enemy.stateMachine.ChangeState(new TruckBossIdleState(enemy));
            }
        }
        public void OnExit()
        {
        }
    }//phase2

    public class TruckBossRevvingRampageState : IState
    {
        private TruckBoss enemy;
        public TruckBossRevvingRampageState(TruckBoss _enemy)
        {
            enemy = _enemy;
        }
        public void OnEnter()
        {
            enemy.StartRevvingRampage();
        }
        public void OnUpdate()
        {
            enemy.RevvingRampage();
            if (enemy.revvingRampageStep == RevvingRampageStep.NONE)
            {
                enemy.stateMachine.ChangeState(new TruckBossIdleState(enemy));
            }
        }
        public void OnExit()
        {
        }
    }//phase2

    public class TruckBossRefuelState : IState
    {
        private TruckBoss enemy;
        public TruckBossRefuelState(TruckBoss _enemy)
        {
            enemy = _enemy;
        }
        public void OnEnter()
        {
            enemy.StartRefuel();
        }
        public void OnUpdate()
        {
            enemy.Refuel();

            if (enemy.refuelStep == RefuelStep.NONE)
            {
                enemy.stateMachine.ChangeState(new TruckBossIdleState(enemy));
            }
        }
        public void OnExit()
        {
        }
    }//phase2

    public class TruckBossRampageState : IState
    {
        private TruckBoss enemy;
        public TruckBossRampageState(TruckBoss _enemy)
        {
            enemy = _enemy;
        }
        public void OnEnter()
        {
            enemy.StartRampage();
        }
        public void OnUpdate()
        {
            enemy.RotateGameObject();
            enemy.Rampage();

            if (enemy.rampageStep == RampageStep.NONE)
            {
                enemy.stateMachine.ChangeState(new TruckBossIdleState(enemy));
            }
        }
        public void OnExit()
        {
            //enemy.rampageStep = RampageStep.NONE;
        }
    }//phase2
}
