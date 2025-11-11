using UnityEngine;
using UnityEngine.Playables;

public class CutsceneHandler : MonoBehaviour
{
    public bool inCutscene;
    [SerializeField] PlayableDirector cutscene;
    private void Start()
    {
        inCutscene = true;
    }
    // Update is called once per frame
    void Update()
    {
        if (!inCutscene) return;
        if (Input.GetKeyDown(KeyCode.Backspace))
        {
            //cutscene.Resume();
            inCutscene = false;
            cutscene.time = cutscene.duration * 0.99f;
            cutscene.Resume();
            cutscene.Evaluate();
            if (!AudioManager.instance.isPlayingBGM) AudioManager.instance.PlayBGM(BGMType.OPENING_CUTSCENE);
        }
    }
}
