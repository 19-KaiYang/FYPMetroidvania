using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [Header("Target")]
    public Transform target;  

    [Header("Camera Settings")]
    public float smoothSpeed = 5f;   
    public Vector3 offset = new Vector3(0, 0, -10f); 

    private void LateUpdate()
    {
        if (target == null) return;

      
        Vector3 desiredPosition = target.position + offset;
        Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed * Time.deltaTime);

        transform.position = smoothedPosition;
    }
}
