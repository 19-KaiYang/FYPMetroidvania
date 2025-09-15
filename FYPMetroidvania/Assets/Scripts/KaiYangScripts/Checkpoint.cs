
using UnityEngine;

public class Checkpoint : MonoBehaviour
{
    private bool alreadyActivated = false;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player") && !alreadyActivated)
        {
            alreadyActivated = true;
            CheckpointManager.Instance.SetCheckpoint(this);
        }
    }

    public Vector3 GetSpawnPoint()
    {
        return transform.position;
    }
}
