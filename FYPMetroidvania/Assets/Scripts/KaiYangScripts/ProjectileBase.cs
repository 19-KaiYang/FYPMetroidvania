using UnityEngine;

public abstract class ProjectileBase : MonoBehaviour
{
    public float damage;
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


}
