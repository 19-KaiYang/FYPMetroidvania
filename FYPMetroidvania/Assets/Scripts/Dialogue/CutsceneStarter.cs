using System;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.SceneManagement;
using UnityEngine.Timeline;

public class CutsceneStarter : MonoBehaviour
{
    private PlayableDirector director;
    [SerializeField] PlayableAsset endingCutscene;
    private TruckBoss boss;
    public PlayerController controller;

    private void Awake()
    {
        director = GetComponent<PlayableDirector>();
        //SceneManager.sceneLoaded += OnSceneLoaded;
    }
    private void Start()
    {
        StartCutscene();
    }
    public void StartCutscene()
    {
        boss = FindFirstObjectByType<TruckBoss>();
        boss.health.enemyDeath += EndingCutscene;

        controller = PlayerController.instance;
        if (controller == null) return;
        var animator = controller.GetComponent<Animator>();
        animator.enabled = true;
        var signal = controller.GetComponentInChildren<SignalReceiver>();

        foreach (var output in director.playableAsset.outputs)
        {
            if (output.streamName == "Player")
            {
                director.SetGenericBinding(output.sourceObject, animator);
            }
            else if (output.streamName == "Player Animation")
            {
                director.SetGenericBinding(output.sourceObject, controller.animator);
            }
            else if(output.streamName == "Player Signal")
            {
                director.SetGenericBinding(output.sourceObject, signal);
            }
        }
        controller.isInCutscene = true;
        director.Play();
    }

    void EndingCutscene(GameObject boss)
    {
        director.playableAsset = endingCutscene;
        boss.GetComponent<Animator>().enabled = true;
        var animator = controller.GetComponent<Animator>();
        animator.enabled = true;
        var signal = controller.GetComponentInChildren<SignalReceiver>();
        foreach (var output in director.playableAsset.outputs)
        {
            if (output.streamName == "Player")
            {
                director.SetGenericBinding(output.sourceObject, animator);
            }
            else if (output.streamName == "Player Animation")
            {
                director.SetGenericBinding(output.sourceObject, controller.animator);
            }
            else if (output.streamName == "Player Signal")
            {
                director.SetGenericBinding(output.sourceObject, signal);
            }
        }
        controller.isInCutscene = true;
        director.Play();
    }
}
