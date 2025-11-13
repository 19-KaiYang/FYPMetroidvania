using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

public class CutsceneTrigger : MonoBehaviour
{
    private PlayableDirector director;

    private void Start()
    {
        director = GetComponent<PlayableDirector>();
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!collision.gameObject.CompareTag("Player")) return;
        var animator = collision.gameObject.GetComponent<Animator>();
        animator.enabled = true;
        var controller = collision.gameObject.GetComponent<PlayerController>();

        foreach (var output in director.playableAsset.outputs)
        {
            if (output.streamName == "Player")
            {
                director.SetGenericBinding(output.sourceObject, animator);
            }
            else if(output.streamName == "Player Animation")
            {
                director.SetGenericBinding(output.sourceObject, controller.animator);
            }
        }
        controller.animator.Rebind();
        controller.animator.Update(0f);
        controller.isInCutscene = true;
        director.Play();
    }
}
