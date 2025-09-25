using UnityEngine;

public class SpiritBarUI : MonoBehaviour
{
    public SpiritGauge spiritGauge;     
    public RectTransform fillTransform;  

    private float fullWidth;

    private void Start()
    {
        if (fillTransform != null)
            fullWidth = fillTransform.sizeDelta.x;
    }

    private void Update()
    {
        if (spiritGauge != null)
        {
            float percent = spiritGauge.GetSpiritPercent();

            fillTransform.sizeDelta = new Vector2(fullWidth * percent, fillTransform.sizeDelta.y);
        }
    }
}
