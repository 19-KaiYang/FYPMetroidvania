using UnityEngine;

public class EnemyHitBox : MonoBehaviour
{
    public CrowdControlState currentCCState = CrowdControlState.None;
    private Enemy enemy;
    [SerializeField] private float attackMultiplier;
    [SerializeField] private float finalDamage;
    private Spearman owner;
    [SerializeField] private float attackDamage;

    public void Init(float _attackDamage, Spearman _enemy)
    {
        attackDamage = _attackDamage;
        owner = _enemy;
    }
    public void SetOwner(Spearman enemy)
    {
        owner = enemy;
    }

    private void Awake()
    {
        enemy = GetComponentInParent<Enemy>();
    }
    private void OnEnable()
    {
        if (owner != null)
        {
            finalDamage = attackMultiplier * enemy.attackDamage;
        }
        else
        {
            finalDamage = attackMultiplier * attackDamage;
        }
    }
    public void Update()
    {
    }
    public void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            Health p = collision.GetComponent<Health>();
            Vector2 dir = (collision.transform.position - enemy.transform.position).normalized;
            p.TakeDamage(finalDamage, dir, true, CrowdControlState.Knockdown, 0f);

            if (currentCCState == CrowdControlState.Stunned) p.ApplyStun(1, dir);
            else if (currentCCState == CrowdControlState.Knockdown) p.ApplyKnockdown(1, false, dir);

            Debug.Log($"Player take {finalDamage} damage");
        }
    }
    private void OnDrawGizmos()
    {
        if (!this.gameObject.activeInHierarchy) return;

        PolygonCollider2D polygon = GetComponent<PolygonCollider2D>();

        Gizmos.color = Color.red;

        if (polygon != null)
        {
            for (int p = 0; p < polygon.pathCount; p++)
            {
                Vector2[] points = polygon.GetPath(p);

                for (int i = 0; i < points.Length; i++)
                {
                    Vector2 start = polygon.transform.TransformPoint(points[i]);
                    Vector2 end = polygon.transform.TransformPoint(points[(i + 1) % points.Length]);
                    Gizmos.DrawLine(start, end);
                }
            }
        }

        BoxCollider2D box = GetComponent<BoxCollider2D>();
        if (box != null)
        {
            Vector3 worldCenter = box.transform.TransformPoint(box.offset);
            Vector3 worldSize = Vector3.Scale(box.size, box.transform.lossyScale);
            Gizmos.DrawWireCube(worldCenter, worldSize);
        }
    }
}
