using DG.Tweening;
using System;
using UnityEngine;
using UnityEngine.SceneManagement;

public class CameraFollowTarget : MonoBehaviour
{
    public float rotationDuration = 0.75f;
    private PlayerController playerController;
    private void Awake()
    {
        SceneManager.sceneLoaded += OnSceneLoad;
    }
    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoad;
        playerController.flipped -= FlipCamera;
    }
    private void OnSceneLoad(Scene arg0, LoadSceneMode arg1)
    {
        playerController = PlayerController.instance;
        playerController.flipped += FlipCamera;
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        transform.position = playerController.transform.position;
    }
    void FlipCamera()
    {
        float yRotation = playerController.facingRight ? 0f : 180f;
        transform.DORotate(new Vector3(0f, yRotation, 0f), rotationDuration).SetEase(Ease.OutCubic);
    }
}
