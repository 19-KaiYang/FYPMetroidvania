using Unity.Cinemachine;
using UnityEngine;

public class CameraTransition : MonoBehaviour
{
    [SerializeField] CinemachineCamera Camera1;
    [SerializeField] CinemachineCamera Camera2;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!collision.gameObject.CompareTag("Player")) return;
        if(Camera1.gameObject.activeSelf)
        {
            Camera1.gameObject.SetActive(false);
            Camera2.gameObject.SetActive(true);
        }
        else
        {
            Camera1.gameObject.SetActive(true);
            Camera2.gameObject.SetActive(false);
        }
    }
}
