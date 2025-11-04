using System.Collections;
using Unity.VisualScripting;
using UnityEngine;
using static MeleeEnemy;
using static UnityEngine.CullingGroup;

public class DaggerCultist : Enemy
{
    [SerializeField] private string currentState;

    [Header("Detect")]
    [SerializeField] private Vector2 detectSize;
    [SerializeField] private Vector2 detectOffset;
    [Space]
    [SerializeField] private Vector2 playerEscapeSize;
    [SerializeField] private Vector2 playerEscapeOffset;
    [Space]
    [SerializeField] private float groundCheckSize;
    [SerializeField] private Vector2 groundCheckOffset;
    [Space]
    [SerializeField] private Vector2 playerPosition;
    [SerializeField] private bool playerDetected;
    [SerializeField] private bool inDetectArea;
    [SerializeField] private bool isGround;

    [Header("Attack")]
    [SerializeField] private LineRenderer line;
    [SerializeField] private Transform startPoint;
    [SerializeField] private Transform targetPoint;
    [SerializeField] private bool isAiming;
    private float dCooldown;
    [SerializeField] private Vector2 daggerCooldown;
    [SerializeField] private float aimingTime;
    [SerializeField] private float throwingTime;
    [SerializeField] private GameObject daggerPrefab;
    [SerializeField] private Transform throwPoint;

    protected override void Awake()
    {
        base.Awake();
        stateMachine = new StateMachine();
        stateMachine.stateChanged += OnStateChanged;

        line = GetComponentInChildren<LineRenderer>();
        
    }

    protected override void Start()
    {
        line.positionCount = 2;
        stateMachine.Initialize(new DaggerCultistIdleState(this));
    }

    protected override void Update()
    {
        base.Update();
        stateMachine.Update();
        DetectPlayer(); 
    }

    private void DetectPlayer()
    {
        Collider2D ground = Physics2D.OverlapCircle((Vector2)transform.position + groundCheckOffset, groundCheckSize, groundleLayer);
        if (ground != null) isGround = true;
        else isGround = false;
        if (isOnPlatform) isGround = true;

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
    private IEnumerator ThrowDagger()
    {
        while (true)
        {
            if (player.transform.position.x < transform.position.x && isFacingRight)
            {
                Flip();
            }
            else if (player.transform.position.x > transform.position.x && !isFacingRight)
            {
                Flip();
            }
            animator.SetTrigger("aim");
            dCooldown = Random.Range((float)daggerCooldown.x, (float)daggerCooldown.y);
            isAiming = true;
            line.startColor = Color.yellow;
            line.endColor = Color.yellow;
            line.enabled = true;
            yield return new WaitForSeconds(aimingTime);
            line.startColor = Color.orange;
            line.endColor = Color.orange;
            isAiming = false;
            Vector2 dir = (line.GetPosition(1) - transform.position).normalized;
            line.SetPosition(1, (Vector2)transform.position + (dir * 30f));
            yield return new WaitForSeconds(throwingTime);
            //throw dagger
            animator.SetTrigger("attack");
            GameObject daggerObj = Instantiate(daggerPrefab, throwPoint.position, Quaternion.identity);
            Dagger spear = daggerObj.GetComponentInChildren<Dagger>();
            spear.SetOwner(this);

            spear.Init(attackDamage, this, dir);
            line.enabled = false;
            isAiming = false;
            yield return new WaitForSeconds(dCooldown);
            animator.ResetTrigger("attack");

        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireCube((Vector2)transform.position + detectOffset, detectSize);

        Gizmos.color = Color.green;
        Gizmos.DrawWireCube((Vector2)transform.position + playerEscapeOffset, playerEscapeSize);

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
    public override void Die()
    {
        base.Die();
    }

    public class DaggerCultistIdleState : IState
    {
        private DaggerCultist enemy;
        public DaggerCultistIdleState(DaggerCultist _enemy)
        {
            enemy = _enemy;
        }

        public void OnEnter()
        {
            enemy.animator.ResetTrigger("attack");
        }
        public void OnUpdate()
        {
            if (enemy.playerDetected)
            {
                enemy.stateMachine.ChangeState(new DaggerCultistAttackState(enemy));
            }
        }
        public void OnExit()
        {

        }
    }
    public class DaggerCultistAttackState : IState
    {
        private DaggerCultist enemy;
        private Coroutine attackRoutine;
        public DaggerCultistAttackState(DaggerCultist _enemy) 
        {
            enemy = _enemy;
        }

        public void OnEnter()
        {
            attackRoutine = enemy.StartCoroutine(enemy.ThrowDagger());
            enemy.line.enabled = true;
        }
        public void OnUpdate()
        {
            if(enemy.health.currentCCState != CrowdControlState.None)
            {
                enemy.stateMachine.ChangeState(new DaggerCultistCCState(enemy));
            }
            if(enemy.isAiming) AimingLine();

            if (!enemy.playerDetected)
            {
                enemy.stateMachine.ChangeState(new DaggerCultistIdleState(enemy));
            }
        }
        public void OnExit()
        {
            if (attackRoutine != null)
            {
                enemy.StopCoroutine(attackRoutine);
                attackRoutine = null;
            }
            enemy.isAiming = false;
            enemy.animator.ResetTrigger("attack");
            enemy.animator.ResetTrigger("aim");
            enemy.line.enabled = false;
        }
        private void AimingLine()
        {
            enemy.line.SetPosition(0, enemy.transform.position);
            enemy.line.SetPosition(1, enemy.player.transform.position);
        }
        //private IEnumerator ThrowDagger()
        //{
        //    while (true)
        //    {
        //        enemy.dCooldown = Random.Range((float)enemy.daggerCooldown.x, (float)enemy.daggerCooldown.y);
        //        enemy.isAiming = true;
        //        yield return new WaitForSeconds(enemy.aimingTime);
        //        //throw dagger
        //        Instantiate(enemy.daggerPrefab, enemy.throwPoint.position, enemy.throwPoint.rotation);
        //        enemy.isAiming = false;
        //        yield return new WaitForSeconds(enemy.dCooldown);
        //    }
        //}
    }

    public class DaggerCultistCCState : IState
    {
        private DaggerCultist enemy;
        private bool knockdown = false;
        private float elapsed;
        public DaggerCultistCCState(DaggerCultist _enemy)
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
                if (enemy.health.currentCCState == CrowdControlState.Knockdown) enemy.stateMachine.ChangeState(new DaggerCultistCCState(enemy));
            }
            if (enemy.health.currentCCState == CrowdControlState.Knockdown)
            {
                if (enemy.isGround)
                {
                    enemy.animator.SetTrigger("land");
                    enemy.animator.ResetTrigger("knockdown");
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
                    if (enemy.getUp) enemy.stateMachine.ChangeState(new DaggerCultistIdleState(enemy));
                    return;
                }
                enemy.stateMachine.ChangeState(new DaggerCultistIdleState(enemy));
            }
        }

        public void OnExit()
        {
            enemy.animator.SetBool("isStun", false);
            enemy.animator.ResetTrigger("land");
            enemy.animator.ResetTrigger("getup");
        }
    }
}
