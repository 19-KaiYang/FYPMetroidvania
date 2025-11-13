using Unity.Cinemachine;
using UnityEngine;

public class CameraTransition : MonoBehaviour
{
    [SerializeField] CinemachineCamera Camera1;
    [SerializeField] CinemachineCamera Camera2;
    [SerializeField] bool disableonactivate;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!collision.gameObject.CompareTag("Player")) return;
        if(Camera1.gameObject.activeSelf)
        {
            Camera1.gameObject.SetActive(false);
            Camera2.gameObject.SetActive(true);
        }
        if(disableonactivate) gameObject.SetActive(false);
    }
}
