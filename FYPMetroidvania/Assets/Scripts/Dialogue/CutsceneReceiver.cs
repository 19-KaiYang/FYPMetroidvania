using UnityEngine;

public class CutsceneReceiver : MonoBehaviour
{
    public Animator animator;
    private void Update()
    {
        if(Input.GetKeyDown(KeyCode.Escape))
        {
            RestoreAnimator();
        }
    }
    public void RestoreAnimator()
    {
        // Wait until the next frame to ensure Timeline fully releases control
        StartCoroutine(DelayedRestore());
    }

    private System.Collections.IEnumerator DelayedRestore()
    {
        yield return new WaitForSeconds(1f); // wait one frame
        animator.Rebind();
        animator.Update(0f);
        Debug.Log("Animator restored after cutscene");
    }
}
