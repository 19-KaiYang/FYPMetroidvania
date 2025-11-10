using UnityEngine;

public class Ramp : MonoBehaviour
{
    public CrowdControlState currentCCState = CrowdControlState.None;
    private PlayerController player;
    private TruckBoss owner;
    [SerializeField] private float ownerAttackDamage;
    [SerializeField] private float attackMultiplier;
    [SerializeField] private float finalDamage;

    public void Init(float _attackDamage, TruckBoss _boss)
    {
        ownerAttackDamage = _attackDamage;
        owner = _boss;
    }
    public void SetOwner(TruckBoss _boss)
    {
        owner = _boss;
    }
    void Awake()
    {
        player = FindFirstObjectByType<PlayerController>();
    }

    void Start()
    {
        finalDamage = attackMultiplier * owner.attackDamage;
    }

    
    void Update()
    {
        
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
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
}
