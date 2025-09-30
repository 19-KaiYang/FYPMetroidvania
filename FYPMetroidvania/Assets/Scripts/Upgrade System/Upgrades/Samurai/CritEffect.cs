using UnityEngine;

[CreateAssetMenu(fileName = "Critical", menuName = "Effects/Critical")]
public class CritEffect : UpgradeEffect
{
    public float critChance = 0.2f;
    public float critDmgMultiplier = 1.5f;
    public override void DoEffect(ActionContext context)
    {
        float rng = Random.Range(0f, 1f);
        if(rng <= critChance)
        {
            if (context.hitbox != null && context.skillSystem == null) {
                context.hitbox.damage *= critDmgMultiplier;
                Debug.Log("Crit effect for " + context.hitbox.name);
            }
        }
    }

}
