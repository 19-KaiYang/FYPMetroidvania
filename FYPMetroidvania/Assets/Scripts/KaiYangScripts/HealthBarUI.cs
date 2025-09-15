using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HealthBarUI : MonoBehaviour
{
    public Health playerHealth;
    public RectTransform fillTransform;  

    private float fullWidth;

    private void Start()
    {
        fullWidth = fillTransform.sizeDelta.x;
    }

    private void Update()
    {

        if (Input.GetKeyDown(KeyCode.P))
        {
            if (playerHealth != null)
            {
                playerHealth.TakeDamage(10f);
                Debug.Log("Player took 10 damage (test key E).");
            }
        }


        if (playerHealth != null)
        {
            float percent = playerHealth.GetHealthPercentage();

            // shrink based on pivot = left
            fillTransform.sizeDelta = new Vector2(fullWidth * percent, fillTransform.sizeDelta.y);
        }
    }
}
