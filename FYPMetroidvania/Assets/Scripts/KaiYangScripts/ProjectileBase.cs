using UnityEngine;

public abstract class ProjectileBase : MonoBehaviour
{
    public float damage;
    public float speed;
    public Rigidbody2D rb;


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



}
