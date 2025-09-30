using System.Collections;
using UnityEngine;
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
    [SerializeField] private bool playerDetected;
    [SerializeField] private bool inDetectArea;

    [Header("Attack")]
    [SerializeField] private LineRenderer line;
    [SerializeField] private Transform startPoint;
    [SerializeField] private Transform targetPoint;
    [SerializeField] private bool isAiming;
    private float dCooldown;
    [SerializeField] private Vector2 daggerCooldown;
    [SerializeField] private float aimingTime;
    [SerializeField] private GameObject daggerPrefab;
    [SerializeField] private Transform throwPoint;

    protected override void Awake()
    {
        base.Awake();
        stateMachine = new StateMachine();
        stateMachine.stateChanged += OnStateChanged;

        line = GetComponentInChildren<LineRenderer>();
        
    }

    private void Start()
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

    public override void TakeDamage(float _damage, Vector2 _dir)
    {
        base.TakeDamage(_damage, _dir);
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
            attackRoutine = enemy.StartCoroutine(ThrowDagger());
        }
        public void OnUpdate()
        {
            AimingLine();

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
        }
        private void AimingLine()
        {
            enemy.line.SetPosition(0, enemy.transform.position);
            enemy.line.SetPosition(1, enemy.player.transform.position);

            if (enemy.isAiming && enemy.playerDetected) enemy.line.enabled = true;
            else enemy.line.enabled = false;
        }
        private IEnumerator ThrowDagger()
        {
            while (true)
            {
                enemy.dCooldown = Random.Range((float)enemy.daggerCooldown.x, (float)enemy.daggerCooldown.y);
                enemy.isAiming = true;
                yield return new WaitForSeconds(enemy.aimingTime);
                //throw dagger
                Instantiate(enemy.daggerPrefab, enemy.throwPoint.position, enemy.throwPoint.rotation);
                enemy.isAiming = false;
                yield return new WaitForSeconds(enemy.dCooldown);
            }
        }
    }
}
