using UnityEngine;
using UnityEngine.Playables;

public class CutsceneHandler : MonoBehaviour
{
    public bool inCutscene;
    [SerializeField] PlayableDirector cutscene;
    [SerializeField] Canvas skipIndicator;
    public bool canSkip;
    private void Start()
    {
        //inCutscene = true;
        skipIndicator.enabled = false;
        canSkip = false;
    }
    // Update is called once per frame
    void Update()
    {
        if (cutscene.state == PlayState.Paused) inCutscene = true;
        else
        {
            inCutscene = false;
            skipIndicator.enabled = false;
            canSkip = false;
        }
        if (!inCutscene) return;
        if (Input.GetKeyDown(KeyCode.Backspace) && canSkip)
        {
            //cutscene.Resume();
            inCutscene = false;
            cutscene.time = cutscene.duration * 0.99f;
            cutscene.Resume();
            cutscene.Evaluate();
            if (!AudioManager.instance.isPlayingBGM) AudioManager.instance.PlayBGM(BGMType.OPENING_CUTSCENE);
        }
    }
    public void CanSkip()
    {
        skipIndicator.enabled = true;
        canSkip = true;
    }
}
