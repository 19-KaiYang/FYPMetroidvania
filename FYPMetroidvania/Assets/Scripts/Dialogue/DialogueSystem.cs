using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Playables;
using UnityEngine.UI;

public class DialogueSystem : MonoBehaviour, IPointerClickHandler
{
    public GameObject postCutsceneObject;
    [SerializeField] PlayableDirector cutscenePlayer;
    [SerializeField] CinemachineImpulseSource impulseSource;
    [SerializeField] private Canvas DialogueCanvas;
    [SerializeField] private Canvas KeybindIndicator;
    [SerializeField] private Image characterImage;
    [SerializeField] private Image npcImage;
    [SerializeField] private Image textPanel;
    [SerializeField] private Image namePanel;
    [SerializeField] private Image nextIndicator;
    private Vector3 nextArrowPosition;
    [SerializeField] private TextMeshProUGUI _speakerNameBox;
    [SerializeField] private TextMeshProUGUI _textBox;
    public List<Button> optionButtons;
    [SerializeField] private RectTransform characterOffscreenPos;
    public Vector3 characterOnscreenPos;
    public float activeXposition, inactiveXposition;
    public string playerName = "Eris";

    public DialogueTextSO DialogueData;
    public float TextSpeed = 1.0f;
    public DialogueStep currentDialogueStep = null;
    public int currentStepIndex;
    private bool textDone;
    public bool autoplaying = false;
    public float autoContinueTime = 1f;
    public Image autoplayHighlight;

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
        nextArrowPosition = nextIndicator.rectTransform.anchoredPosition;
        KeybindIndicator.gameObject.SetActive(false);
        //foreach (var button in optionButtons) button.gameObject.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        if (dialogueActive)
        {
            // Tracking inputs
            if(Input.GetKeyDown(KeyCode.Backspace))
            {
                StopAllCoroutines();
                DialogueCanvas.enabled = false;
                dialogueActive = false;
                PlayerController.instance.isInCutscene = false;
                if (postCutsceneObject != null) postCutsceneObject.SetActive(true);
                KeybindIndicator.gameObject.SetActive(false);
            }
            else if(Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.Space))
            {
                nextInputbuffer = true;  
            }
            else if(Input.GetKeyDown(KeyCode.Tab))
            {
                autoplaying = !autoplaying;
                if (autoplaying)
                {
                    autoplayHighlight.gameObject.SetActive(true);
                    autoplayHighlight.fillAmount = 0f;
                    autoplayHighlight.DOFillAmount(1f, 3f).SetEase(Ease.Linear).SetLoops(-1, LoopType.Restart);
                }
                else
                {
                    autoplayHighlight.gameObject.SetActive(false);
                    autoplayHighlight.DOKill();
                }
            }
        } 
    }
    public void StartDialogue(DialogueTextSO dialogueData)
    {
        StopAllCoroutines();
        //currentDialogueStep = null;
        DialogueData = dialogueData;
        DialogueCanvas.enabled = true;
        dialogueActive = true;
        PlayerController.instance.isInCutscene = true;
        KeybindIndicator.gameObject.SetActive(true);
        autoplaying = false;
        autoplayHighlight.gameObject.SetActive(false);
        StartCoroutine(DialogueCoroutine(dialogueData));
    }
    IEnumerator DialogueCoroutine(DialogueTextSO dialogueData, int startstep = 0)
    {
        //foreach (var button in optionButtons) button.gameObject.SetActive(false);
        dialogueActive = true;
        namePanel.enabled = true;
        textPanel.gameObject.SetActive(true);
        characterImage.sprite = dialogueData.defaultPlayerImage;
        npcImage.sprite = dialogueData.defaultNPCImage;
        if (characterImage.sprite != null) characterImage.enabled = true;
        if (npcImage.sprite != null) npcImage.enabled = true;
        //characterImage.rectTransform.anchoredPosition = characterOffscreenPos.anchoredPosition;
        string currSpeaker = null;
        bool animate = true;
        currentStepIndex = startstep;
        DialogueStep dialogueStep;
        float autoTimer = 0f;

        for (int i = startstep; i < dialogueData.Steps.Count; i++)
        {
            dialogueStep = dialogueData.Steps[i];
            animate = dialogueStep.Name != currSpeaker;
            SpeakerTransition(dialogueStep);
            Debug.Log("next dialogue step");
            while (!textDone) yield return null;
            if (!dialogueStep.autoskip)
            {
                //autoTimer += Time.deltaTime;
                while (!(autoplaying && autoTimer > autoContinueTime) && !nextInputbuffer)
                {
                    autoTimer += Time.deltaTime;
                    yield return null;
                }
                //while (!nextInputbuffer) yield return null; // Wait for player input
            }
            autoTimer = 0f;
            nextInputbuffer = false;
            currSpeaker = dialogueStep.Name;
            currentStepIndex++;
        }
        DialogueCanvas.enabled = false;
        dialogueActive = false;
        if (dialogueData.hasCutscene)
        {
            cutscenePlayer.Resume();
        }
        else PlayerController.instance.isInCutscene = false;
        if (postCutsceneObject != null) postCutsceneObject.SetActive(true);
        KeybindIndicator.gameObject.SetActive(false);
    }
    IEnumerator DialogueTextCoroutine(DialogueStep dialoguestep)
    {
        nextIndicator.enabled = false;
        nextIndicator.rectTransform.DOKill();
        nextIndicator.rectTransform.anchoredPosition = nextArrowPosition;
        _textBox.text = dialoguestep.Text;
        if (dialoguestep.SpeakerType == SPEAKER_TYPE.PLAYER && dialoguestep.Name == "Player") _speakerNameBox.text = playerName;
        else _speakerNameBox.text = dialoguestep.Name;
        currentDialogueStep = dialoguestep;
        int textSize = _textBox.text.Length;
        char[] line = dialoguestep.Text.ToCharArray();
        int textLength = line.Length;
        float speed = 1f / (TextSpeed * SettingData.instance.textSpeedMult);

        if (dialoguestep.screenshake && SettingData.instance.screenshake) impulseSource.GenerateImpulse(0.2f);
        for (int i = 0; i < textLength; i++)
        {
            if (nextInputbuffer)
            {
                nextInputbuffer = false;
                _textBox.maxVisibleCharacters = textLength;
                break;
            }
            if (line[i] == ' ' && i < textLength - 1)
            {
                i++;
            }
            _textBox.maxVisibleCharacters = i + 1;
            AudioManager.PlaySFX(dialoguestep.sfx, 0.5f, pitch: dialoguestep.pitch);
            if (line[i] == '.' || line[i] == '!' || line[i] == '?') yield return new WaitForSeconds(0.3f);
            else if (line[i] == ',') yield return new WaitForSeconds(0.15f);
            yield return new WaitForSeconds(speed);
        }
        textDone = true;
        if (!autoplaying)
        {
            nextIndicator.enabled = true;
            nextIndicator.rectTransform.DOAnchorPosY(nextIndicator.rectTransform.anchoredPosition.y - 10f, 1f).SetEase(Ease.InQuad).SetLoops(-1, LoopType.Yoyo);
        }
    }
    void SpeakerTransition(DialogueStep nextStep, float duration = 0.5f)
    {
        DOTween.CompleteAll();
        textDone = false;
        if (currentDialogueStep == null || currentDialogueStep.Name == "") 
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
            else if (nextStep.Name == currentDialogueStep.Name)
            {
                Image imageChange = nextStep.SpeakerType == SPEAKER_TYPE.PLAYER ? characterImage : npcImage;
                imageChange.sprite = nextStep.SpeakerImage;
                StartCoroutine(DialogueTextCoroutine(nextStep));
                return;
            }
            DG.Tweening.Sequence animationSeq = DOTween.Sequence();
            Image portraitToAnimate = nextStep.SpeakerType == SPEAKER_TYPE.PLAYER ? characterImage : npcImage;
            float offscreenFactor = nextStep.SpeakerType == SPEAKER_TYPE.PLAYER ? -1 : 1;
            TweenCallback tweenCallback = new TweenCallback(() => {
                if (nextStep.SpeakerImage != null)
                {
                    portraitToAnimate.enabled = true;
                    portraitToAnimate.sprite = nextStep.SpeakerImage;
                }
                else portraitToAnimate.enabled = false;
                StartCoroutine(DialogueTextCoroutine(nextStep)); 
            });
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
                if (nextSprite != null)
                {
                    characterImage.enabled = true;
                    characterImage.sprite = nextSprite;
                }
                else characterImage.enabled = false;
                characterImage.rectTransform.DOAnchorPosX(-activeXposition, duration).SetEase(Ease.OutCubic);
                characterImage.rectTransform.DOScale(1f, duration * 0.5f);
                characterImage.color = Color.white;
                npcImage.rectTransform.DOAnchorPosX(inactiveXposition , duration).SetEase(Ease.OutCubic);
                npcImage.rectTransform.DOScale(0.8f, duration * 0.5f);
                npcImage.color = Color.gray;
                break;
            case SPEAKER_TYPE.NPC:
                if (nextSprite != null)
                {
                    npcImage.enabled = true;
                    npcImage.sprite = nextSprite;
                }
                else npcImage.enabled = false;
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

    public void StartDialogueInteraction(DialogueTextSO dialogueData, GameObject postCutsceneObj = null)
    {
        dialogueActive = true;
        DialogueData = dialogueData;
        if (postCutsceneObj != null) postCutsceneObject = postCutsceneObj;
        else postCutsceneObject = null;
        StartDialogue(DialogueData);
    }

}
