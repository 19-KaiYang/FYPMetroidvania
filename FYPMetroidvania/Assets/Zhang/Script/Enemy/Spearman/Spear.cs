using System.Collections;
using Unity.VisualScripting;
using UnityEngine;

public class Spear : ProjectileBase
{
    public CrowdControlState currentCCState = CrowdControlState.None;
    private PlayerController player;
    [SerializeField] private float forceY;
    [SerializeField] private float destroyTime;
    private float offset;
    [SerializeField] private Vector2 offSet;
    [SerializeField] private Material matetial;
    private Spearman owner;
    [SerializeField] private float ownerAttackDamage;

    [SerializeField] private float attackMultiplier;
    [SerializeField] private float finalDamage;

    public void Init(float _attackDamage, Spearman _enemy)
    {
        ownerAttackDamage = _attackDamage;
        owner = _enemy;
    }
    public void SetOwner(Spearman enemy)
    {
        owner = enemy;
    }
    protected override void Awake()
    {
        base.Awake();
        player = FindFirstObjectByType<PlayerController>();
    }
    private void Start()
    {
        matetial = GetComponent<Renderer>().material;
        offset = Random.Range((float)offSet.x, (float)offSet.y);
        finalDamage = attackMultiplier * owner.attackDamage;
        Move();
    }
    protected override void Update()
    {
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
    protected override void Move()
    {
        ThrowToPlayer(rb, transform, player.transform, forceY, offset);
    }
    private void ThrowToPlayer(Rigidbody2D _rb, Transform _enemy, Transform _player, float _jumpForceY, float offsetX)
    {
        float g = Mathf.Abs(Physics2D.gravity.y * _rb.gravityScale);

        Vector2 enemyPos = _enemy.position;
        Vector2 playerPos = _player.position;

        float velocityY = _jumpForceY;

        float time = (2 * velocityY) / g;

        float direction = Mathf.Sign(playerPos.x - enemyPos.x);
        float targetX = playerPos.x + direction * offsetX;

        float velocitX = (targetX - enemyPos.x) / time;

        _rb.linearVelocity = new Vector2(velocitX, velocityY);
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.layer == LayerMask.NameToLayer("Ground"))
        {
            rb.linearVelocity = Vector2.zero;
            rb.simulated = false;
            GetComponent<Collider2D>().enabled = false;

            StartCoroutine(Destroy());
        }

        if (collision.CompareTag("Player"))
        {
            Health p = collision.GetComponent<Health>();
            Vector2 dir;
            dir = (collision.transform.position - this.transform.position).normalized;

            p.TakeDamage(finalDamage, dir, true, CrowdControlState.Knockdown, 0f);

            if (currentCCState == CrowdControlState.Stunned) p.ApplyStun(1, dir);
            else if (currentCCState == CrowdControlState.Knockdown) p.ApplyKnockdown(1, false, dir);

        }
    }
    private IEnumerator Destroy()
    {
        Color c = matetial.color;
        float startAlpha = c.a;
        float t = 0;

        yield return new WaitForSeconds(3);

        while (t < destroyTime)
        {
            t += Time.deltaTime;
            float alpha = Mathf.Lerp(startAlpha, 0, t / destroyTime);
            matetial.color = new Color(c.r, c.g, c.b, alpha);
            yield return null;
        }
        Despawn();
    }
}
