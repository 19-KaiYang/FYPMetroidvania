using UnityEngine;

public class TrainingDummy : Enemy
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    protected override void Start()
    {
        base.Start();
    }

    // Update is called once per frame
    protected override void Update()
    {
        base.Update();
        if (health.currentHealth <= 0) health.currentHealth = health.maxHealth;
    }
}
