using UnityEngine;

public abstract class ProjectileBase : MonoBehaviour
{
    public float damage;
    public float speed;
    public float knockback;
    public Rigidbody2D rb;
    public Vector2 direction;

    protected virtual void Awake()
    {
        if (!rb) rb = GetComponent<Rigidbody2D>();
    }

    protected abstract void Move();

    protected virtual void Update()
    {
        Move();
    }

    public virtual void Despawn()
    {
        if (ProjectileManager.instance != null)
            ProjectileManager.instance.ReturnToPool(gameObject);
        else
            gameObject.SetActive(false);                   
    }

    protected void ApplyKnockback(Health target, Vector2 dir)
    {
        if (target == null || target.isPlayer) return;

        Rigidbody2D trb = target.GetComponent<Rigidbody2D>();
        if (trb != null && knockback > 0f)
        {
            trb.AddForce(dir.normalized * knockback, ForceMode2D.Impulse);
        }
    }

    //CREATE YOUR OWN MOVEMENT IN YOUR PROJECTILES

}
