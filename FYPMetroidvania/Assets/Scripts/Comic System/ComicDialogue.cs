using System.Collections;
using TMPro;
using UnityEngine;

public class ComicDialogue : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI _textBox;
    public SFXTYPE sfx;
    public int TextSpeed = 30;
    public bool isDone = false;
    public bool skip = false;
    public SFXTYPE dialougeSound;
    public float pitch = 1f;

    private void Awake()
    {
        _textBox = GetComponentInChildren<TextMeshProUGUI>();
    }
    public void PlayDialogueBox()
    {
        isDone = false;
        skip = false;
        if(sfx != SFXTYPE.NONE)
        {
            AudioManager.PlaySFX(sfx, 0.5f);
        }
        StartCoroutine(DialogueDisplay());
    }

    IEnumerator DialogueDisplay()
    {
        char[] line = _textBox.text.ToCharArray();
        int textLength = line.Length;
        float speed = 1f / (TextSpeed);
        _textBox.maxVisibleCharacters = 0;

        for (int i = 0; i < textLength; i++)
        {
            _textBox.maxVisibleCharacters = i + 1;
            if(skip)
            {
                skip = false;
                _textBox.maxVisibleCharacters = textLength;
                break;
            }
            if (dialougeSound != SFXTYPE.NONE) AudioManager.PlaySFX(SFXTYPE.DIALOGUE_1, pitch: pitch);
            if (line[i] == '.' || line[i] == '!' || line[i] == '?') yield return new WaitForSeconds(0.15f);
            else if (line[i] == ',') yield return new WaitForSeconds(0.05f);
            yield return new WaitForSeconds(speed);
        }
        isDone = true;
    }
}
