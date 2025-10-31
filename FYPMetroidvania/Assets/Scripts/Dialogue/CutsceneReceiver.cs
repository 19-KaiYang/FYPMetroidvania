using UnityEngine;

public class CutsceneReceiver : MonoBehaviour
{
    public Animator animator;
    private void Update()
    {
    }
    public void RestoreAnimator()
    {
        PlayerController.instance.isInCutscene = false;
        // Wait until the next frame to ensure Timeline fully releases control
        StartCoroutine(DelayedRestore());
    }

    private System.Collections.IEnumerator DelayedRestore()
    {
        yield return new WaitForSeconds(0.1f); // wait one frame
        animator.Rebind();
        animator.Update(0f);
        Debug.Log("Animator restored after cutscene");
    }
}
