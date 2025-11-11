using NUnit.Framework;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;
using UnityEngine.UI;

[CreateAssetMenu(fileName = "DialogueTextSO", menuName = "Scriptable Objects/DialogueTextSO")]
public class DialogueTextSO : ScriptableObject
{
    public SPEAKER_TYPE SpeakerType;
    public Sprite defaultPlayerImage;
    public Sprite defaultNPCImage;
    public bool hasCutscene;
    public List<DialogueStep> Steps;
}

public enum SPEAKER_TYPE
{
    PLAYER = 0,
    NPC,
    NARRATOR
}

[Serializable]
public class DialogueStep
{
    public SPEAKER_TYPE SpeakerType;
    public string Name;
    public Sprite SpeakerImage;
    public string Text;
    public SFXTYPE sfx = SFXTYPE.DIALOGUE_1;
    [UnityEngine.Range(0.01f, 3f)]
    public float pitch = 1f;
    public bool autoskip;
    public bool screenshake;
}
