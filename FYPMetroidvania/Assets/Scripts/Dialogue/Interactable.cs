using UnityEngine;

public class Interactable : MonoBehaviour
{
    [SerializeField] DialogueSystem _dialogueSystem;
    [SerializeField] Canvas interactbutton;
    [SerializeField] DialogueTextSO dialogueSO;
    [SerializeField] GameObject postCutsceneObj;
    public bool canInteract;
    public bool interacted;
    public bool oneTimeOnly;


    private void Awake()
    {
        canInteract = false;
        interacted = false;
        interactbutton.enabled = false;
    }

    private void Update()
    {
        if (canInteract)
        {
            if (Input.GetKeyDown(KeyCode.Return))
            {
                Debug.Log("Start dialogue");
                _dialogueSystem.gameObject.SetActive(true);
                _dialogueSystem.StartDialogueInteraction(dialogueSO, postCutsceneObj);
                interacted = true;
                if (oneTimeOnly)
                {
                    interactbutton.enabled = false;
                    canInteract = false;
                }
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!collision.gameObject.CompareTag("Player")) return;
        if(interacted && oneTimeOnly)
        {
            canInteract = false;
            interactbutton.enabled = false;
            return;
        }
        else
        {
            canInteract = true;
            interactbutton.enabled = true;
        }
    }
    private void OnTriggerExit2D(Collider2D collision)
    {
        if (!collision.gameObject.CompareTag("Player")) return;
        canInteract = false;
        interactbutton.enabled = false;
    }
}
