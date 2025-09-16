using UnityEngine;
using UnityEngine.SceneManagement;

public class CheckPoint : MonoBehaviour
{
    [SerializeField] private bool isTriggerWithPlayer;

    void Start()
    {
        
    }
    void Update()
    {
        RegistCheckPoint();
    }

    void RegistCheckPoint()
    {
        if (isTriggerWithPlayer && Input.GetKey(KeyCode.UpArrow))
        {
            RespawnManager.instance.SetCheckpoint(
                SceneManager.GetActiveScene().name,
                transform.position);

            //PlayerController.instance.Health = int.MaxValue;
        }
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            isTriggerWithPlayer = true;
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            isTriggerWithPlayer = false;
        }
    }
}
