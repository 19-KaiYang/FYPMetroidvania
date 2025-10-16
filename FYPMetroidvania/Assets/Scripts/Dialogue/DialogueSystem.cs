using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class Dialogue : MonoBehaviour, IPointerClickHandler
{
    [SerializeField] private Canvas DialogueCanvas;
    [SerializeField] private Image characterImage;
    [SerializeField] private Image npcImage;
    [SerializeField] private Image textPanel;
    [SerializeField] private Image namePanel;
    [SerializeField] private Image nextIndicator;
    [SerializeField] private TextMeshProUGUI _speakerNameBox;
    [SerializeField] private TextMeshProUGUI _textBox;
    public List<Button> optionButtons;
    [SerializeField] private RectTransform characterOffscreenPos;
    public Vector3 characterOnscreenPos;
    public float activeXposition, inactiveXposition;

    public DialogueTextSO DialogueData;
    public float TextSpeed = 1.0f;
    public DialogueStep currentDialogueStep = null;
    public int currentStepIndex;
    private bool textDone;
    public bool skipSpace = true;

    public bool DebugClick = false;

    public bool dialogueActive;
    public bool nextInputbuffer = false;

    void Awake()
    {
        if (_textBox == null)
            _textBox = GetComponent<TextMeshProUGUI>();

        Init();
    }
    public void Init()
    {
        _textBox.text = string.Empty;
        //characterOnscreenPos = characterImage.rectTransform.anchoredPosition;
        characterImage.enabled = false;
        characterImage.rectTransform.DOAnchorPosX(-inactiveXposition - inactiveXposition, 0.1f);
        npcImage.enabled = false;
        npcImage.rectTransform.DOAnchorPosX(inactiveXposition + inactiveXposition, 0.1f);
        namePanel.enabled = false;
        textPanel.gameObject.SetActive(false);
        nextIndicator.enabled = false;
        textDone = false;
        //foreach (var button in optionButtons) button.gameObject.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
    }
    public void StartDialogue(DialogueTextSO dialogueData, int startstep = 0)
    {
        currentDialogueStep = null;
        DialogueCanvas.enabled = true;
        dialogueActive = true;
        StartCoroutine(DialogueCoroutine(dialogueData, startstep));
    }
    IEnumerator DialogueCoroutine(DialogueTextSO dialogueData, int startstep = 0)
    {
        //foreach (var button in optionButtons) button.gameObject.SetActive(false);
        dialogueActive = true;
        characterImage.enabled = true;
        npcImage.enabled = true;
        namePanel.enabled = true;
        textPanel.gameObject.SetActive(true);
        characterImage.sprite = dialogueData.defaultPlayerImage;
        npcImage.sprite = dialogueData.defaultNPCImage;
        //characterImage.rectTransform.anchoredPosition = characterOffscreenPos.anchoredPosition;
        string currSpeaker = null;
        bool animate = true;
        currentStepIndex = startstep;
        DialogueStep dialogueStep;

        for (int i = startstep; i < dialogueData.Steps.Count; i++)
        {
            dialogueStep = dialogueData.Steps[i];
            animate = dialogueStep.Name != currSpeaker;
            SpeakerTransition(dialogueStep);
            Debug.Log("next dialogue step");
            nextIndicator.enabled = true;
            while (!textDone) yield return null;
            if (!dialogueStep.autoskip)
            {
                while (!nextInputbuffer) yield return null; // Wait for player input
            }
            nextInputbuffer = false;
            nextIndicator.enabled = false;
            currSpeaker = dialogueStep.Name;
            currentStepIndex++;
        }
        DialogueCanvas.enabled = false;
        dialogueActive = false;
    }
    IEnumerator DialogueTextCoroutine(DialogueStep dialoguestep)
    {
        _textBox.text = dialoguestep.Text;
        _speakerNameBox.text = dialoguestep.Name;
        currentDialogueStep = dialoguestep;
        //StartCoroutine(AnimateCharacterImage(moveIn: true, 1f));

        int textSize = _textBox.text.Length;
        char[] line = dialoguestep.Text.ToCharArray();
        int textLength = line.Length;
        float speed = 1f / TextSpeed;
        for (int i = 0; i < textLength; i++)
        {
            if (nextInputbuffer)
            {
                nextInputbuffer = false;
                _textBox.maxVisibleCharacters = textLength;
                break;
            }
            if (line[i] == ' ')
            {
                i++;
            }

            _textBox.maxVisibleCharacters = i + 1;

            yield return new WaitForSeconds(speed);
        }
        textDone = true;
    }
    void SpeakerTransition(DialogueStep nextStep, float duration = 0.5f)
    {
        DOTween.CompleteAll();
        textDone = false;
        if(currentDialogueStep.Name == "")
        {
            Debug.Log("start");
            StartCoroutine(DialogueTextCoroutine(nextStep));
            AnimateSpeakerChange(nextStep.SpeakerType, nextStep.SpeakerImage);
            return;
        }
        if(nextStep.SpeakerType != currentDialogueStep.SpeakerType)
        {
            StartCoroutine(DialogueTextCoroutine(nextStep));
            AnimateSpeakerChange(nextStep.SpeakerType, nextStep.SpeakerImage);
            return;
        }
        if (nextStep.SpeakerType == currentDialogueStep.SpeakerType)
        {
            if (nextStep.SpeakerImage == currentDialogueStep.SpeakerImage)
            {
                StartCoroutine(DialogueTextCoroutine(nextStep));
                return;
            }

            DG.Tweening.Sequence animationSeq = DOTween.Sequence();
            Image portraitToAnimate = nextStep.SpeakerType == SPEAKER_TYPE.PLAYER ? characterImage : npcImage;
            float offscreenFactor = nextStep.SpeakerType == SPEAKER_TYPE.PLAYER ? -1 : 1;
            TweenCallback tweenCallback = new TweenCallback(() => { portraitToAnimate.sprite = nextStep.SpeakerImage; StartCoroutine(DialogueTextCoroutine(nextStep)); });
            animationSeq.Append(portraitToAnimate.rectTransform.DOAnchorPosX((offscreenFactor * inactiveXposition) * 2f, duration / 2));
            animationSeq.AppendCallback(tweenCallback);
            animationSeq.Append(portraitToAnimate.rectTransform.DOAnchorPosX(offscreenFactor * activeXposition, duration / 2));
        }

    }
    void AnimateSpeakerChange(SPEAKER_TYPE speakertype, Sprite nextSprite, float duration = 0.3f)
    {
        switch (speakertype)
        {
            case SPEAKER_TYPE.PLAYER:
                characterImage.sprite = nextSprite;
                characterImage.rectTransform.DOAnchorPosX(-activeXposition, duration).SetEase(Ease.OutCubic);
                characterImage.rectTransform.DOScale(1f, duration * 0.5f);
                characterImage.color = Color.white;
                npcImage.rectTransform.DOAnchorPosX(inactiveXposition , duration).SetEase(Ease.OutCubic);
                npcImage.rectTransform.DOScale(0.8f, duration * 0.5f);
                npcImage.color = Color.gray;
                break;
            case SPEAKER_TYPE.NPC:
                npcImage.sprite = nextSprite;
                npcImage.rectTransform.DOAnchorPosX(activeXposition, duration).SetEase(Ease.OutCubic);
                npcImage.rectTransform.DOScale(1f, duration * 0.5f);
                npcImage.color = Color.white;
                characterImage.rectTransform.DOAnchorPosX(-inactiveXposition, duration).SetEase(Ease.OutCubic);
                characterImage.rectTransform.DOScale(0.8f, duration * 0.5f);
                characterImage.color = Color.gray;
                break;
            case SPEAKER_TYPE.NARRATOR:
                npcImage.rectTransform.DOAnchorPosX(inactiveXposition, duration).SetEase(Ease.OutCubic);
                npcImage.rectTransform.DOScale(0.8f, duration * 0.5f);
                npcImage.color = Color.gray;
                characterImage.rectTransform.DOAnchorPosX(-inactiveXposition, duration * 0.75f).SetEase(Ease.OutCubic);
                characterImage.rectTransform.DOScale(0.8f, duration * 0.5f);
                characterImage.color = Color.gray;
                break;
        }

    }
    IEnumerator AnimateCharacterImage(bool moveIn, float duration = 0.75f)
    {
        float elapsed = 0f;
        if (characterImage.sprite == null) characterImage.enabled = false;
        else characterImage.enabled = true;
        Vector3 position = characterImage.rectTransform.anchoredPosition;
        Vector3 finalPosition = moveIn ? new Vector3(425f, 0, 0) : characterOffscreenPos.anchoredPosition;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            position += (finalPosition - position);
            characterImage.rectTransform.anchoredPosition = position;
            //Debug.Log("Animate " + characterImage.rectTransform.anchoredPosition);
            yield return null;
        }
        characterImage.rectTransform.anchoredPosition = finalPosition;

    }
    public void OnPointerClick(PointerEventData eventData)
    {
        if (!dialogueActive)
        {
            dialogueActive = true;
            StartCoroutine(DialogueCoroutine(DialogueData));
            return;
        }
        else nextInputbuffer = true;
    }
}
