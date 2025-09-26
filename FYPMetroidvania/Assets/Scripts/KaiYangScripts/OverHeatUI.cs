using UnityEngine;
using UnityEngine.UI;

public class OverheatBarUI : MonoBehaviour
{
    public OverheatSystem overheatSystem;  
    public RectTransform fillTransform;    

    private float fullWidth;

    private void Start()
    {
        fullWidth = fillTransform.sizeDelta.x;
    }

    private void Update()
    {
        if (overheatSystem == null) return;

        float percent = overheatSystem.GetHeatPercentage();


        fillTransform.sizeDelta = new Vector2(fullWidth * percent, fillTransform.sizeDelta.y);


        if (overheatSystem.IsOverheated)
            fillTransform.GetComponent<Image>().color = Color.orange;
        else
            fillTransform.GetComponent<Image>().color = Color.yellow;
    }
}
