using UnityEngine;

public class EnergySystem : MonoBehaviour
{
    [Header("Energy Settings")]
    public float maxEnergy = 100f;
    public float regenPerSecond = 5f;

    private float currentEnergy;
    public bool HasEnough(float amount) => currentEnergy >= amount;


    private void Awake()
    {
        currentEnergy = maxEnergy;
    }

    private void Update()
    {
        Regenerate();
    }

    public bool TrySpend(float amount)
    {
        if (currentEnergy >= amount)
        {
            currentEnergy -= amount;
            return true; 
        }
        return false; 
    }

    private void Regenerate()
    {
        if (currentEnergy < maxEnergy)
        {
            currentEnergy += regenPerSecond * Time.deltaTime;
            currentEnergy = Mathf.Min(currentEnergy, maxEnergy);
        }
    }

    public void ResetEnergy()
    {
        currentEnergy = maxEnergy;
    }

    public float GetCurrentEnergy() => currentEnergy;
    public float GetMaxEnergy() => maxEnergy;
    public float GetEnergyPercent() => currentEnergy / maxEnergy;
}
