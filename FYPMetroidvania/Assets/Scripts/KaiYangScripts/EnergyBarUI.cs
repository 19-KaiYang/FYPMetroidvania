using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnergyBarUI : MonoBehaviour
{
    public EnergySystem playerEnergy;
    public RectTransform fillTransform;

    private float fullWidth;

    private void Start()
    {
        playerEnergy = PlayerController.instance.GetComponent<EnergySystem>();
        if (fillTransform != null)
            fullWidth = fillTransform.sizeDelta.x;
    }

    private void Update()
    {
       

        if (playerEnergy != null)
        {
            float percent = playerEnergy.GetEnergyPercent();

            // shrink based on pivot = left
            fillTransform.sizeDelta = new Vector2(fullWidth * percent, fillTransform.sizeDelta.y);
        }
    }
}
