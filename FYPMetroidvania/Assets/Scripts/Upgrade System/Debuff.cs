using UnityEngine;

public abstract class Debuff : ScriptableObject
{
    public enum debuffTriggerType
    {
        WhenApplied,
        WhenRemoved,
        OverTime,
        WhenHit,
    }

    public string debuffName;
    public debuffTriggerType triggerType;
    public int maxStacks = 1;
    public float tickRate;
    public GameObject VFX;
    public void ApplyDebuff(Health owner, int stacks, float duration)
    {
        DebuffInstance match = owner.debuffs.Find(s => s.debuff.debuffName == debuffName);
        if (match != null)
        {
            match.stacks = Mathf.Clamp(match.stacks + stacks, 0, maxStacks);
            match.duration = duration;
        }
        else
        {
            DebuffInstance instance = new DebuffInstance(this, stacks, duration, Time.time);
            owner.debuffs.Add(instance);
            if (VFX != null)
            {
                GameObject debuffVFX = Instantiate(VFX, owner.transform);
                owner.debuffVFXs.Add(debuffVFX);
            }

            if (triggerType == debuffTriggerType.WhenApplied)
                TriggerDebuff(owner, instance);
            else if (triggerType == debuffTriggerType.WhenHit)
                owner.damageTaken += instance.TriggerDebuffEvent;
        }
    }
    public abstract void TriggerDebuff(Health owner, DebuffInstance instance);

}
