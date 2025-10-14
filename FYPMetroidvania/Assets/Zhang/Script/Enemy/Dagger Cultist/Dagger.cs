using UnityEngine;

public class Dagger : ProjectileBase
{
    public CrowdControlState currentCCState = CrowdControlState.None;
    private PlayerController player;
    private Vector3 playerPosition;

    private DaggerCultist owner;
    [SerializeField] private float ownerAttackDamage;

    [SerializeField] private float attackMultiplier;
    [SerializeField] private float finalDamage;

    public void Init(float _attackDamage, DaggerCultist _enemy)
    {
        ownerAttackDamage = _attackDamage;
        owner = _enemy;
    }
    public void SetOwner(DaggerCultist enemy)
    {
        owner = enemy;
    }

    protected override void Awake()
    {
        base.Awake();
        player = FindFirstObjectByType<PlayerController>();
        playerPosition = player.transform.position;
        Vector2 dir = (playerPosition - transform.position).normalized;
        rb.linearVelocity = dir * speed;

        Vector3 f = transform.localScale;
        if (playerPosition.x < transform.position.x) f.y = -1;
        else f.y = 1;
        transform.localScale = f;
    }
    private void Start()
    {
        finalDamage = attackMultiplier * owner.attackDamage;
    }

    protected override void Move()
    {
          
    }

    protected override void Update()
    {
        base.Update();

        if (rb != null)
        {
            Vector2 v = rb.linearVelocity;
            if (v.sqrMagnitude > 0.01f)
            {
                float angle = Mathf.Atan2(v.y, v.x) * Mathf.Rad2Deg;
                transform.rotation = Quaternion.Euler(0, 0, angle);
            }
        }
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        //Health player = collision.GetComponent<Health>();
        //if (player != null && player.isPlayer)
        //{
        //    player.TakeDamage(damage);

        //    //Rigidbody2D rbEnemy = player.GetComponent<Rigidbody2D>();
        //    //if (rbEnemy != null)
        //    //{
        //    //    Vector2 knockDir = (player.transform.position - transform.position).normalized;
        //    //    ApplyKnockback(player, knockDir);
        //    //}

        //    Despawn();
        //}

        if (collision.CompareTag("Player"))
        {
            Health p = collision.GetComponent<Health>();
            Vector2 dir;
            dir = (collision.transform.position - this.transform.position).normalized;

            p.TakeDamage(finalDamage, dir, true, CrowdControlState.Knockdown, 0f);

            if (currentCCState == CrowdControlState.Stunned) p.ApplyStun(1, dir);
            else if (currentCCState == CrowdControlState.Knockdown) p.ApplyKnockdown(1, false, dir);


            Despawn();
        }

        if (!collision.CompareTag("Enemy") && !collision.CompareTag("Hurtbox"))
        {
            Despawn();
        }
    }
}
