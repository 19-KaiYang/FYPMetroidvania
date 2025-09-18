using UnityEngine;

public class MovableObject : MonoBehaviour
{
    private Rigidbody2D rb;
    public float force = 10f;
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            rb.AddForceY(force, ForceMode2D.Impulse);
        }
        if(Input.GetKeyDown(KeyCode.RightArrow))
        {
            rb.AddForceX(force, ForceMode2D.Impulse);
        }
    }
}
