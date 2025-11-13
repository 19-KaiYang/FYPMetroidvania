using UnityEngine;

public class SpiritGauge : MonoBehaviour
{
    [Header("Spirit Settings")]
    public float maxSpirit = 100f;
    public float drainPerSecond = 20f;

    private float currentSpirit;
    private bool draining = false;

    public bool IsEmpty => currentSpirit <= 0f;
    public bool IsFull => currentSpirit >= maxSpirit;

    private void Start()
    {
        currentSpirit = 0f;
    }

    private void Update()
    {
        if (draining && currentSpirit > 0f)
        {
            currentSpirit -= drainPerSecond * Time.deltaTime;
            if (currentSpirit < 0f) currentSpirit = 0f;
        }
        else if(draining && currentSpirit <= 0f)
        {
            StopDrain();
        }
    }

    public void StartDrain()
    {
        draining = true;
    }

    public void StopDrain()
    {
        draining = false;
    }

    public void Refill(float amount)
    {
        currentSpirit = Mathf.Min(currentSpirit + amount, maxSpirit);
    }

    public void ResetGauge()
    {
        currentSpirit = maxSpirit;
    }

    public void SetCurrentSpirit(float value)
    {
        currentSpirit = Mathf.Clamp(value, 0f, maxSpirit);
    }

    public float GetCurrentSpirit() => currentSpirit;
    public float GetMaxSpirit() => maxSpirit;
    public float GetSpiritPercent() => currentSpirit / maxSpirit;
}