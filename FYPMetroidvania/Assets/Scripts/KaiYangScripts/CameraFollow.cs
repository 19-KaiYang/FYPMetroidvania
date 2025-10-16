using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.SceneManagement;

public class CameraFollow : MonoBehaviour
{
    public CinemachineCamera FollowCamera;
    [Header("Target")]
    public Transform target;  

    [Header("Camera Settings")]
    public float smoothSpeed = 5f;   
    public Vector3 offset = new Vector3(0, 0, -10f);

    private void Awake()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void LateUpdate()
    {
        //if (target == null) return;

      
        //Vector3 desiredPosition = target.position + offset;
        //Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed * Time.deltaTime);

        //transform.position = smoothedPosition;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player == null) return;
        //target = player.transform;
        //FollowCamera.Target.TrackingTarget = player.transform;
    }
}
