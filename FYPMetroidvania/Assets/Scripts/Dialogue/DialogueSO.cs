using NUnit.Framework;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[CreateAssetMenu(fileName = "DialogueTextSO", menuName = "Scriptable Objects/DialogueTextSO")]
public class DialogueTextSO : ScriptableObject
{
    public SPEAKER_TYPE SpeakerType;
    public string Name;
    public string Text;
    public Sprite defaultPlayerImage;
    public Sprite defaultNPCImage;
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
    public bool autoskip;
}
