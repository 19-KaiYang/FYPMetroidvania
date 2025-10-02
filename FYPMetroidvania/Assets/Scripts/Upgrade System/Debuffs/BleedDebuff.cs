using UnityEngine;

[CreateAssetMenu(menuName = "Debuffs/Bleed")]
public class BleedDebuff : Debuff
{
    public float damage = 3f;
    public override void TriggerDebuff(Health owner, DebuffInstance instance)
    {
        if (owner.CurrentHealth <= 0) return;
        float finalDamage = damage;
        DebuffInstance match = owner.debuffs.Find(s => s.debuff.debuffName == debuffName);
        if (match != null)
        {
            finalDamage *= 2f;
        }
        owner.TakeDamage(finalDamage, triggerEffects: false);
        Debug.Log("Bleed ticked at: " + Time.time + " on target: " + owner.name + " for damage: " + finalDamage);
    }
}
