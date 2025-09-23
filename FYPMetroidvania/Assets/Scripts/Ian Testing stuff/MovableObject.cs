using UnityEngine;

public class MovableObject : MonoBehaviour
{
    private Rigidbody2D rb;
    public float force = 10f;
    bool checkPlayer = false;

    public PlayerController player;
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            rb.linearVelocityY += force;
            if (player != null)
            {
                player.SetVelocity(rb.linearVelocity);
            }
        }
    }
    private void FixedUpdate()
    {
        bool playerDetected = false;
        RaycastHit2D[] hits = Physics2D.RaycastAll(transform.position + new Vector3(0f,0.5f,0f), Vector2.up, 0.5f);
        foreach(RaycastHit2D hit in hits )
        {
            if(hit.collider.gameObject.tag == "Player"){
                if(player == null) player = hit.collider.gameObject.GetComponent<PlayerController>();
                playerDetected = true; break;
            }
        }
        if (!playerDetected) player = null;
    }
}
