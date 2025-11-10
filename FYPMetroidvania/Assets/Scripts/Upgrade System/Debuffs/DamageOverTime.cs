using UnityEngine;

[CreateAssetMenu(menuName = "Debuffs/DoT")]
public class DamageOverTime : Debuff
{
    public int damagePerStack;
    public Color damageColor;
    public override void TriggerDebuff(Health owner, DebuffInstance instance)
    {
        if (owner.CurrentHealth <= 0) return;
        float damage = damagePerStack * instance.stacks;
        owner.TakeDamage(damage, triggerEffects: false, isDebuff: true, damageNumberColor: damageColor);
        Debug.Log("DoT ticked at: " + Time.time + " on target: " + owner.name);
    }
}
