using TMPro;
using UnityEngine;

public class DamageNumber : MonoBehaviour
{
    public TextMeshProUGUI numberText;

    private void Awake()
    {
        if(!numberText) numberText = GetComponentInChildren<TextMeshProUGUI>();
        Canvas canvas = GetComponent<Canvas>();
        canvas.worldCamera = Camera.main;
    }
    public void Initialize(int damage, Color color, bool critical = false)
    {
        numberText.color = color;
        numberText.alpha = 0.1f;
        numberText.text = damage.ToString();
        if (critical) numberText.text += "!";
    }
}
