using DG.Tweening;
using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

public class ComicPanel : MonoBehaviour
{
    public RectTransform rectTransform;
    public List<ComicDialogue> SpeechBubbles;
    public int currentDialogue = 0;

    [Header("Animation Settings")]
    public Vector2 startPosition;
    public Vector2 endPosition;

    void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
    }
    public void InitialiseDialogues()
    {
        currentDialogue = -1;
        for (int i = 0;i < SpeechBubbles.Count;i++)
        {
            var bubble = SpeechBubbles[i];
            bubble.transform.localScale = Vector2.zero;
        }
    }

    public bool TryNextDialogue()
    {
        if (currentDialogue >= 0)
        {
            if (!SpeechBubbles[currentDialogue].isDone)
            {
                SpeechBubbles[currentDialogue].skip = true;
                return true;
            }
        }
        currentDialogue++;
        if (currentDialogue < SpeechBubbles.Count)
        {
            SpeechBubbles[currentDialogue].transform.DOScale(Vector3.one, 0.25f);
            SpeechBubbles[currentDialogue].PlayDialogueBox();
            return true;
        }

        return false; // Dialogues done, end of panel
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
