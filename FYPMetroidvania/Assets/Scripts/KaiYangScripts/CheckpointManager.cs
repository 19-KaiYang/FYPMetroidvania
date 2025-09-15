
using UnityEngine;

public class CheckpointManager : MonoBehaviour
{
    public static CheckpointManager Instance;

    private Checkpoint currentCheckpoint;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void SetCheckpoint(Checkpoint checkpoint)
    {
        currentCheckpoint = checkpoint;
        Debug.Log($"Checkpoint set at {checkpoint.transform.position}");
    }

    public Vector3 GetSpawnPoint()
    {
        return currentCheckpoint != null
            ? currentCheckpoint.GetSpawnPoint()
            : Vector3.zero;
    }
}
