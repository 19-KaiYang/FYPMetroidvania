using UnityEngine;

public class OverheatSystem : MonoBehaviour
{
    [Header("Overheat Settings")]
    public float maxHeat = 100f;
    public float heatPerSkill = 20f;

    [Header("Chunk Decay")]
    public float chunkSize = 10f;        
    public float chunkInterval = 0.5f;   

    private float currentHeat;
    private bool overheated;
    private float chunkTimer;

    public bool IsOverheated => overheated;
    public float CurrentHeat => currentHeat;

    private void Update()
    {
        if (overheated)
        {
            chunkTimer -= Time.deltaTime;

            if (chunkTimer <= 0f)
            {
                currentHeat -= chunkSize;
                chunkTimer = chunkInterval;

                if (currentHeat <= 0f)
                {
                    currentHeat = 0f;
                    overheated = false;   
                }
            }
        }
    }

    public void AddHeat(float amount)
    {
        if (overheated) return; 

        currentHeat = Mathf.Min(maxHeat, currentHeat + amount);

        if (currentHeat >= maxHeat)
        {
            currentHeat = maxHeat;
            overheated = true;   
            chunkTimer = chunkInterval; 
        }
    }

    public float GetHeatPercentage()
    {
        return currentHeat / maxHeat;
    }

    public void ResetHeat()
    {
        currentHeat = 0f;
        overheated = false;
        chunkTimer = 0f;
    }
}
