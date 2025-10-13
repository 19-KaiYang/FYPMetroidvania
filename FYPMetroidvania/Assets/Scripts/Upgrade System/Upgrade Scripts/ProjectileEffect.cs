using UnityEngine;

[CreateAssetMenu(fileName = "Projectile", menuName = "Effects/Projectile")]
public class ProjectileEffect : UpgradeEffect
{
    public GameObject projectile;
    public float shootChance = 1.0f;
    public override void DoEffect(ActionContext context)
    {

        if (context.upgradeManager != null)
        {
            Debug.Log($"[ProjectileEffect] {name} triggered, chance={shootChance}");
            float random = Random.value;
            if (random <= shootChance)
            {
                context.upgradeManager.SpawnEffectProjectile(projectile);
            }
        }
    }
}
